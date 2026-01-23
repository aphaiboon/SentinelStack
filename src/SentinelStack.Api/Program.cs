using System.Text;
using Asp.Versioning;
using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Common.Interfaces;
using SentinelStack.Application.Incidents.Commands;
using SentinelStack.Application.Incidents.Queries;
using SentinelStack.Application.Users.Commands;
using SentinelStack.Application.Users.Queries;
using SentinelStack.Application.Services.Commands;
using SentinelStack.Application.Services.Queries;
using SentinelStack.Infrastructure.Auth;
using SentinelStack.Infrastructure.Data;
using SentinelStack.Infrastructure.Repositories;
using SentinelStack.Infrastructure.Services;

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SentinelStack API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"));

    // API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    });

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSettings);

    var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is required");
    var key = Encoding.UTF8.GetBytes(secret);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Database Context
    var connectionString = builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString")
        ?? throw new InvalidOperationException("Database connection string is required");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    // Register Services
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();
    builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

    // Repositories
    builder.Services.AddScoped<IIncidentRepository, IncidentRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
    builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

    // Incident Commands & Queries
    builder.Services.AddScoped<CreateIncidentCommand>();
    builder.Services.AddScoped<UpdateIncidentCommand>();
    builder.Services.AddScoped<AcknowledgeIncidentCommand>();
    builder.Services.AddScoped<ResolveIncidentCommand>();
    builder.Services.AddScoped<GetIncidentQuery>();
    builder.Services.AddScoped<GetIncidentsQuery>();

    // User Commands & Queries
    builder.Services.AddScoped<CreateUserCommand>();
    builder.Services.AddScoped<UpdateUserCommand>();
    builder.Services.AddScoped<GetUserQuery>();
    builder.Services.AddScoped<GetUsersQuery>();

    // Service Commands & Queries
    builder.Services.AddScoped<CreateServiceCommand>();
    builder.Services.AddScoped<UpdateServiceCommand>();
    builder.Services.AddScoped<GetServiceQuery>();
    builder.Services.AddScoped<GetServicesQuery>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString,
            name: "postgresql",
            tags: new[] { "db", "sql", "postgresql" });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "SentinelStack API",
            Version = "v1",
            Description = "HIPAA-compliant Incident Management Platform API"
        });
    });

    var app = builder.Build();

    // Request logging middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SentinelStack API V1");
        c.RoutePrefix = "swagger";
    });

    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoints
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds
                }),
                totalDuration = report.TotalDuration.TotalMilliseconds
            });
            await context.Response.WriteAsync(result);
        }
    });

    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false // Liveness: just check if app is running
    });

    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("db") // Readiness: check dependencies
    });

    app.MapControllers();

    Log.Information("SentinelStack API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
