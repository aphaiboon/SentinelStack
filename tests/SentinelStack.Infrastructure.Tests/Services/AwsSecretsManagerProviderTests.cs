using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SentinelStack.Infrastructure.Services;

namespace SentinelStack.Infrastructure.Tests.Services;

public class AwsSecretsManagerProviderTests
{
    private readonly Mock<IAmazonSecretsManager> _mockSecretsManager;
    private readonly Mock<ILogger<AwsSecretsManagerProvider>> _mockLogger;

    public AwsSecretsManagerProviderTests()
    {
        _mockSecretsManager = new Mock<IAmazonSecretsManager>();
        _mockLogger = new Mock<ILogger<AwsSecretsManagerProvider>>();
    }

    [Fact]
    public async Task GetSecretAsync_WithValidSecretName_ReturnsSecretValue()
    {
        // Arrange
        var secretName = "test-secret";
        var secretValue = "super-secret-value";

        _mockSecretsManager
            .Setup(x => x.GetSecretValueAsync(
                It.Is<GetSecretValueRequest>(r => r.SecretId == secretName),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSecretValueResponse
            {
                SecretString = secretValue
            });

        var provider = new AwsSecretsManagerProvider(_mockSecretsManager.Object, _mockLogger.Object);

        // Act
        var result = await provider.GetSecretAsync(secretName);

        // Assert
        result.Should().Be(secretValue);
    }

    [Fact]
    public async Task GetSecretAsync_WhenSecretNotFound_ThrowsException()
    {
        // Arrange
        var secretName = "non-existent-secret";

        _mockSecretsManager
            .Setup(x => x.GetSecretValueAsync(
                It.IsAny<GetSecretValueRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Secret not found"));

        var provider = new AwsSecretsManagerProvider(_mockSecretsManager.Object, _mockLogger.Object);

        // Act
        var act = () => provider.GetSecretAsync(secretName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task GetSecretsAsync_WithMultipleSecrets_ReturnsAllValues()
    {
        // Arrange
        var secrets = new Dictionary<string, string>
        {
            ["secret1"] = "value1",
            ["secret2"] = "value2",
            ["secret3"] = "value3"
        };

        foreach (var kvp in secrets)
        {
            _mockSecretsManager
                .Setup(x => x.GetSecretValueAsync(
                    It.Is<GetSecretValueRequest>(r => r.SecretId == kvp.Key),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetSecretValueResponse
                {
                    SecretString = kvp.Value
                });
        }

        var provider = new AwsSecretsManagerProvider(_mockSecretsManager.Object, _mockLogger.Object);

        // Act
        var result = await provider.GetSecretsAsync(secrets.Keys);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(secrets);
    }

    [Fact]
    public async Task GetSecretAsync_LogsSecretRetrieval_WithoutLoggingValue()
    {
        // Arrange
        var secretName = "test-secret";

        _mockSecretsManager
            .Setup(x => x.GetSecretValueAsync(
                It.IsAny<GetSecretValueRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetSecretValueResponse
            {
                SecretString = "secret-value"
            });

        var provider = new AwsSecretsManagerProvider(_mockSecretsManager.Object, _mockLogger.Object);

        // Act
        await provider.GetSecretAsync(secretName);

        // Assert - Verify that logging occurs (2 times: start and success) and never contains the secret value
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(secretName) && !v.ToString()!.Contains("secret-value")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2)); // Logs both "Retrieving..." and "Successfully retrieved..."
    }
}
