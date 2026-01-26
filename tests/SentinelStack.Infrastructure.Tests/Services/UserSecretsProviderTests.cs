using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SentinelStack.Infrastructure.Services;

namespace SentinelStack.Infrastructure.Tests.Services;

public class UserSecretsProviderTests
{
    [Fact]
    public async Task GetSecretAsync_WithValidKey_ReturnsValue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestSecret"] = "TestValue123"
            })
            .Build();

        var provider = new UserSecretsProvider(configuration);

        // Act
        var result = await provider.GetSecretAsync("TestSecret");

        // Assert
        result.Should().Be("TestValue123");
    }

    [Fact]
    public async Task GetSecretAsync_WithMissingKey_ThrowsException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var provider = new UserSecretsProvider(configuration);

        // Act
        var act = () => provider.GetSecretAsync("NonExistentSecret");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*NonExistentSecret*not found*");
    }

    [Fact]
    public async Task GetSecretsAsync_WithValidKeys_ReturnsAllValues()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Secret1"] = "Value1",
                ["Secret2"] = "Value2",
                ["Secret3"] = "Value3"
            })
            .Build();

        var provider = new UserSecretsProvider(configuration);

        // Act
        var result = await provider.GetSecretsAsync(new[] { "Secret1", "Secret2", "Secret3" });

        // Assert
        result.Should().HaveCount(3);
        result["Secret1"].Should().Be("Value1");
        result["Secret2"].Should().Be("Value2");
        result["Secret3"].Should().Be("Value3");
    }

    [Fact]
    public async Task GetSecretsAsync_WithSomeMissingKeys_ThrowsException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Secret1"] = "Value1"
            })
            .Build();

        var provider = new UserSecretsProvider(configuration);

        // Act
        var act = () => provider.GetSecretsAsync(new[] { "Secret1", "MissingSecret" });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*MissingSecret*not found*");
    }
}
