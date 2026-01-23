using System.Net;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;

namespace SentinelStack.Api.IntegrationTests.Controllers;

[Collection("Integration")]
public class HealthControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public HealthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync()
    {
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Health_ReturnsHealthy_WhenDatabaseIsAvailable()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [Fact]
    public async Task HealthLive_ReturnsOk_WhenAppIsRunning()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ReturnsHealthy_WhenDatabaseIsReady()
    {
        // Act
        var response = await _client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
