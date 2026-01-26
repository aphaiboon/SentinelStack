using Microsoft.Extensions.Configuration;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Secrets provider implementation that reads from .NET configuration system (user-secrets, appsettings).
/// Used in development environments.
/// </summary>
public class UserSecretsProvider : ISecretsProvider
{
    private readonly IConfiguration _configuration;

    public UserSecretsProvider(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be null or empty.", nameof(secretName));
        }

        var value = _configuration[secretName];

        if (string.IsNullOrEmpty(value))
        {
            throw new InvalidOperationException(
                $"Required secret '{secretName}' not found in configuration. " +
                "Ensure it is set in user-secrets or appsettings.");
        }

        return Task.FromResult(value);
    }

    public async Task<Dictionary<string, string>> GetSecretsAsync(
        IEnumerable<string> secretNames,
        CancellationToken cancellationToken = default)
    {
        if (secretNames == null)
        {
            throw new ArgumentNullException(nameof(secretNames));
        }

        var secrets = new Dictionary<string, string>();

        foreach (var secretName in secretNames)
        {
            var value = await GetSecretAsync(secretName, cancellationToken);
            secrets[secretName] = value;
        }

        return secrets;
    }
}
