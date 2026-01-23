namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Abstraction for sending notifications across multiple channels.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (HTML supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email notification to multiple recipients.
    /// </summary>
    /// <param name="recipients">List of recipient email addresses.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (HTML supported).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendEmailAsync(IEnumerable<string> recipients, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an SMS notification.
    /// Note: SMS is deferred to Growth phase (ADR-002). This method will throw NotImplementedException until then.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number in E.164 format.</param>
    /// <param name="message">SMS message content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}
