using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Logging;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Secrets provider implementation that retrieves secrets from AWS Secrets Manager.
/// Used in production environments.
/// </summary>
public class AwsSecretsManagerProvider : ISecretsProvider
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly ILogger<AwsSecretsManagerProvider> _logger;

    public AwsSecretsManagerProvider(
        IAmazonSecretsManager secretsManager,
        ILogger<AwsSecretsManagerProvider> logger)
    {
        _secretsManager = secretsManager ?? throw new ArgumentNullException(nameof(secretsManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));
        }

        try
        {
            _logger.LogInformation("Retrieving secret '{SecretName}' from AWS Secrets Manager", secretName);

            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            var response = await _secretsManager.GetSecretValueAsync(request, cancellationToken);

            _logger.LogInformation("Successfully retrieved secret '{SecretName}'", secretName);

            return response.SecretString;
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError(ex, "Secret '{SecretName}' not found in AWS Secrets Manager", secretName);
            throw new InvalidOperationException(
                $"Required secret '{secretName}' not found in AWS Secrets Manager. " +
                "Ensure the secret exists and the application has permission to access it.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret '{SecretName}' from AWS Secrets Manager", secretName);
            throw new InvalidOperationException(
                $"Failed to retrieve secret '{secretName}' from AWS Secrets Manager. " +
                "Check AWS credentials and permissions.", ex);
        }
    }

    public async Task<Dictionary<string, string>> GetSecretsAsync(
        IEnumerable<string> secretNames,
        CancellationToken cancellationToken = default)
    {
        if (secretNames == null)
        {
            throw new ArgumentNullException(nameof(secretNames));
        }

        var namesList = secretNames.ToList();
        var tasks = namesList.Select(name => GetSecretAsync(name, cancellationToken));
        var values = await Task.WhenAll(tasks);

        var secrets = new Dictionary<string, string>();
        for (var i = 0; i < namesList.Count; i++)
        {
            secrets[namesList[i]] = values[i];
        }

        return secrets;
    }
}
