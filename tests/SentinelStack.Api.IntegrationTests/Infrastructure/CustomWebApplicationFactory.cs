using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SentinelStack.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace SentinelStack.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory that uses PostgreSQL Testcontainers for realistic database testing.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("sentinelstack_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private bool _containerStarted;

    public string ConnectionString => _postgresContainer.GetConnectionString();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure the container is started and connection string is set
        // BEFORE the base.CreateHost runs Program.cs
        EnsureContainerStarted();

        return base.CreateHost(builder);
    }

    private void EnsureContainerStarted()
    {
        if (!_containerStarted)
        {
            _postgresContainer.StartAsync().GetAwaiter().GetResult();
            _containerStarted = true;

            // Set the connection string environment variable
            // This MUST be set before Program.cs runs
            Environment.SetEnvironmentVariable(
                "DatabaseSettings__ConnectionString",
                _postgresContainer.GetConnectionString());
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Add DbContext with Testcontainers PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_postgresContainer.GetConnectionString());
            });

            // Build service provider and ensure database is created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync()
    {
        // Container will be started in CreateHost if not already started
        if (!_containerStarted)
        {
            await _postgresContainer.StartAsync();
            _containerStarted = true;

            Environment.SetEnvironmentVariable(
                "DatabaseSettings__ConnectionString",
                _postgresContainer.GetConnectionString());
        }
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}
