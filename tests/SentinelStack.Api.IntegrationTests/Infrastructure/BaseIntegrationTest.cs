using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SentinelStack.Application.Auth.DTOs;
using SentinelStack.Application.Auth.Interfaces;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using SentinelStack.Infrastructure.Data;

namespace SentinelStack.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests providing common functionality.
/// </summary>
[Collection("Integration")]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected HttpClient Client { get; private set; } = null!;
    protected readonly JsonSerializerOptions JsonOptions;

    protected Guid TestTenantId { get; private set; }
    protected Guid TestUserId { get; private set; }
    protected string TestUserEmail { get; private set; } = null!;

    protected BaseIntegrationTest(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        // Don't create client here - wait until InitializeAsync
        // when the PostgreSQL container is ready
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public virtual async Task InitializeAsync()
    {
        // Create the client now that the container is ready
        // (ICollectionFixture's InitializeAsync runs before test class InitializeAsync)
        Client = Factory.CreateClient();

        // Create a unique test user for this test run
        TestTenantId = Guid.NewGuid();
        TestUserEmail = $"test_{Guid.NewGuid():N}@sentinelstack.io";
        var testPassword = "Test@123!";

        // Create test user directly in database
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = new User(
            TestTenantId,
            TestUserEmail,
            "Test User",
            passwordHasher.HashPassword(testPassword),
            UserRole.Admin);

        // Set the ID using reflection
        typeof(User).GetProperty("Id")!.SetValue(user, Guid.NewGuid());
        TestUserId = user.Id;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // Authenticate and set bearer token
        await AuthenticateAsync(testPassword);
    }

    public virtual async Task DisposeAsync()
    {
        // Cleanup test data
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Remove all test data for this tenant
        var incidents = context.Incidents.Where(i => i.TenantId == TestTenantId);
        context.Incidents.RemoveRange(incidents);

        var services = context.Services.Where(s => s.TenantId == TestTenantId);
        context.Services.RemoveRange(services);

        var webhooks = context.WebhookEndpoints.Where(w => w.TenantId == TestTenantId);
        context.WebhookEndpoints.RemoveRange(webhooks);

        var escalations = context.EscalationPolicies.Where(e => e.TenantId == TestTenantId);
        context.EscalationPolicies.RemoveRange(escalations);

        var users = context.Users.Where(u => u.TenantId == TestTenantId);
        context.Users.RemoveRange(users);

        await context.SaveChangesAsync();
    }

    protected async Task AuthenticateAsync(string password = "Test@123!")
    {
        var loginRequest = new LoginRequest
        {
            Email = TestUserEmail,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions);
            if (result?.AccessToken != null)
            {
                Client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", result.AccessToken);

                // Add X-Tenant-Id header required by TenantContextMiddleware
                Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
                Client.DefaultRequestHeaders.Add("X-Tenant-Id", TestTenantId.ToString());
            }
        }
    }

    protected async Task<T?> GetAsync<T>(string url) where T : class
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T data)
    {
        return await Client.PostAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> PutAsync<T>(string url, T data)
    {
        return await Client.PutAsJsonAsync(url, data);
    }

    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    protected async Task SeedTenant(Tenant tenant)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
    }
}
