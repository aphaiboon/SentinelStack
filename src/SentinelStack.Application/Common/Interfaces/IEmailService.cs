namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service interface for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to a single recipient.
    /// </summary>
    Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email to multiple recipients.
    /// </summary>
    Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a templated email.
    /// </summary>
    Task SendTemplatedEmailAsync(
        string to,
        string templateName,
        object templateData,
        CancellationToken cancellationToken = default);
}
