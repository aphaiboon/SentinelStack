namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Provides access to application secrets from various storage backends.
/// </summary>
public interface ISecretsProvider
{
    /// <summary>
    /// Retrieves a single secret by name.
    /// </summary>
    /// <param name="secretName">The name or key of the secret to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the secret is not found.</exception>
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple secrets by their names.
    /// </summary>
    /// <param name="secretNames">Collection of secret names to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping secret names to their values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when any secret is not found.</exception>
    Task<Dictionary<string, string>> GetSecretsAsync(
        IEnumerable<string> secretNames,
        CancellationToken cancellationToken = default);
}
