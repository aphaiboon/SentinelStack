using System.Text;
using Amazon;
using Amazon.SecretsManager;
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
using SentinelStack.Application.AuditLogs.Queries;
using SentinelStack.Application.Escalations.Commands;
using SentinelStack.Application.Escalations.Queries;
using SentinelStack.Application.Webhooks.Commands;
using SentinelStack.Application.Webhooks.Queries;
using SentinelStack.Application.Common.Settings;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using SentinelStack.Api.Infrastructure;
using SentinelStack.Infrastructure.Auth;
using SentinelStack.Infrastructure.BackgroundJobs;
using SentinelStack.Infrastructure.Data;
using SentinelStack.Infrastructure.Repositories;
using SentinelStack.Infrastructure.Services;

// Configure Serilog early for startup logging
// Use CreateLogger instead of CreateBootstrapLogger to avoid "logger already frozen" error with WebApplicationFactory
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting SentinelStack API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Secrets Provider (environment-based)
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
    {
        // Development/Testing: Use user-secrets and appsettings
        builder.Services.AddScoped<ISecretsProvider, UserSecretsProvider>();
        Log.Information("Using UserSecretsProvider for development/testing environment");
    }
    else
    {
        // Production: Use AWS Secrets Manager
        builder.Services.AddSingleton<IAmazonSecretsManager>(sp =>
            new AmazonSecretsManagerClient(RegionEndpoint.USEast1));
        builder.Services.AddScoped<ISecretsProvider, AwsSecretsManagerProvider>();
        Log.Information("Using AWS Secrets Manager for production environment");
    }

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

    // Allow test environment to use default secret
    var secret = jwtSettings["Secret"];
    if (string.IsNullOrEmpty(secret))
    {
        if (builder.Environment.IsEnvironment("Testing"))
        {
            secret = "TestSecretKeyThatIsAtLeast32CharactersLongForHMACSHA256";
        }
        else
        {
            throw new InvalidOperationException("JWT Secret is required");
        }
    }
    var key = Encoding.UTF8.GetBytes(secret);

    var jwtIssuer = jwtSettings["Issuer"] ?? (builder.Environment.IsEnvironment("Testing") ? "SentinelStack.Tests" : "SentinelStack");
    var jwtAudience = jwtSettings["Audience"] ?? (builder.Environment.IsEnvironment("Testing") ? "SentinelStack.Tests" : "SentinelStack");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing");
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();

    // Application Settings
    builder.Services.Configure<ApplicationSettings>(options =>
    {
        options.BaseUrl = builder.Configuration["Application:BaseUrl"] ?? "https://api.sentinelstack.io";
    });

    // Database Context
    var connectionString = builder.Configuration.GetValue<string>("DatabaseSettings:ConnectionString");
    if (string.IsNullOrEmpty(connectionString))
    {
        if (builder.Environment.IsEnvironment("Testing"))
        {
            // Tests will override this with their own DbContext registration
            connectionString = "Host=localhost;Database=sentinelstack_test;Username=test;Password=test";
        }
        else
        {
            throw new InvalidOperationException("Database connection string is required");
        }
    }
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
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IEscalationPolicyRepository, EscalationPolicyRepository>();
    builder.Services.AddScoped<IWebhookEndpointRepository, WebhookEndpointRepository>();
    builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

    // Audit Service
    builder.Services.AddScoped<IAuditService, AuditService>();

    // Email Settings & Services
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();

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

    // Audit Log Queries (read-only - audit logs are immutable)
    builder.Services.AddScoped<GetAuditLogQuery>();
    builder.Services.AddScoped<GetAuditLogsQuery>();

    // Escalation Policy Commands & Queries
    builder.Services.AddScoped<CreateEscalationPolicyCommand>();
    builder.Services.AddScoped<UpdateEscalationPolicyCommand>();
    builder.Services.AddScoped<GetEscalationPolicyQuery>();
    builder.Services.AddScoped<GetEscalationPoliciesQuery>();

    // Webhook Commands & Queries
    builder.Services.AddScoped<CreateWebhookEndpointCommand>();
    builder.Services.AddScoped<UpdateWebhookEndpointCommand>();
    builder.Services.AddScoped<ProcessWebhookCommand>();
    builder.Services.AddScoped<GetWebhookEndpointQuery>();
    builder.Services.AddScoped<GetWebhookEndpointsQuery>();

    // Health Checks
    var healthChecksBuilder = builder.Services.AddHealthChecks();
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        healthChecksBuilder.AddNpgSql(
            connectionString,
            name: "postgresql",
            tags: new[] { "db", "sql", "postgresql" });
    }

    // Hangfire Background Jobs - skip in Testing (tests will mock/override if needed)
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options
                .UseNpgsqlConnection(connectionString)));

        builder.Services.AddHangfireServer(options =>
        {
            options.Queues = new[] { "escalations", "notifications", "default" };
            options.WorkerCount = Environment.ProcessorCount * 2;
        });
    }

    // Background Job Services
    builder.Services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();
    builder.Services.AddScoped<EscalationJobService>();

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

    // Fail-fast validation for required secrets (production only)
    if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
    {
        Log.Information("Validating required secrets from AWS Secrets Manager...");
        using var scope = app.Services.CreateScope();
        var secretsProvider = scope.ServiceProvider.GetRequiredService<ISecretsProvider>();

        var requiredSecrets = new[]
        {
            "sentinelstack-db-connection",
            "sentinelstack-jwt-secret",
            "sentinelstack-hangfire-password"
        };

        foreach (var secretName in requiredSecrets)
        {
            try
            {
                _ = await secretsProvider.GetSecretAsync(secretName);
                Log.Information("✓ Secret '{SecretName}' validated successfully", secretName);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "✗ Required secret '{SecretName}' is missing or inaccessible", secretName);
                throw new InvalidOperationException(
                    $"Required secret '{secretName}' not found in AWS Secrets Manager. " +
                    "Ensure all secrets are created and IAM permissions are configured.", ex);
            }
        }

        Log.Information("All required secrets validated successfully");
    }

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

    // Hangfire Dashboard and recurring jobs - skip in Testing
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() },
            DashboardTitle = "SentinelStack Background Jobs"
        });

        // Setup recurring jobs
        RecurringJob.AddOrUpdate<EscalationJobService>(
            "process-escalations",
            service => service.ProcessPendingEscalationsAsync(),
            "*/5 * * * *", // Every 5 minutes
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }

    Log.Information("SentinelStack API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw; // Re-throw so WebApplicationFactory can see what went wrong
}
finally
{
    Log.CloseAndFlush();
}
