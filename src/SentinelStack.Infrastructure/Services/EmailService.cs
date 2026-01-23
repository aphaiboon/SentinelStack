using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.Services;

/// <summary>
/// Email settings for SMTP configuration.
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@sentinelstack.io";
    public string FromName { get; set; } = "SentinelStack";
}

/// <summary>
/// Email service implementation using SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        await SendEmailAsync(new[] { to }, subject, htmlBody, plainTextBody, cancellationToken);
    }

    public async Task SendEmailAsync(
        IEnumerable<string> to,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            foreach (var recipient in to)
            {
                message.To.Add(recipient);
            }

            // Add plain text alternative if provided
            if (!string.IsNullOrEmpty(plainTextBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(plainTextBody, null, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
                message.Body = plainTextBody;
                message.IsBodyHtml = false;
            }

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl
            };

            if (!string.IsNullOrEmpty(_settings.SmtpUsername))
            {
                client.Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword);
            }

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Recipients}. Subject: {Subject}",
                string.Join(", ", to),
                subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}. Subject: {Subject}",
                string.Join(", ", to),
                subject);
            throw;
        }
    }

    public async Task SendTemplatedEmailAsync(
        string to,
        string templateName,
        object templateData,
        CancellationToken cancellationToken = default)
    {
        var (subject, htmlBody, plainTextBody) = RenderTemplate(templateName, templateData);
        await SendEmailAsync(to, subject, htmlBody, plainTextBody, cancellationToken);
    }

    private static (string Subject, string HtmlBody, string PlainTextBody) RenderTemplate(string templateName, object data)
    {
        // Simple template rendering - in production, use a proper templating engine
        return templateName switch
        {
            "IncidentCreated" => RenderIncidentCreatedTemplate(data),
            "IncidentAcknowledged" => RenderIncidentAcknowledgedTemplate(data),
            "IncidentResolved" => RenderIncidentResolvedTemplate(data),
            "IncidentEscalation" => RenderIncidentEscalationTemplate(data),
            _ => throw new ArgumentException($"Unknown template: {templateName}")
        };
    }

    private static (string Subject, string HtmlBody, string PlainTextBody) RenderIncidentCreatedTemplate(object data)
    {
        dynamic d = data;
        var subject = $"[{d.Severity}] New Incident: {d.Title}";
        var htmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #dc3545;'>New Incident Created</h2>
    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Title}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Severity:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Severity}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Service:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.ServiceName ?? "N/A"}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Description:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Description}</td></tr>
    </table>
    <p style='margin-top: 20px;'>
        <a href='{d.IncidentUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>View Incident</a>
    </p>
</body>
</html>";

        var plainTextBody = $@"
New Incident Created

Title: {d.Title}
Severity: {d.Severity}
Service: {d.ServiceName ?? "N/A"}
Description: {d.Description}

View Incident: {d.IncidentUrl}
";

        return (subject, htmlBody, plainTextBody);
    }

    private static (string Subject, string HtmlBody, string PlainTextBody) RenderIncidentAcknowledgedTemplate(object data)
    {
        dynamic d = data;
        var subject = $"Incident Acknowledged: {d.Title}";
        var htmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #ffc107;'>Incident Acknowledged</h2>
    <p>The following incident has been acknowledged by {d.AcknowledgedBy}:</p>
    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Title}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Severity:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Severity}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Acknowledged At:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.AcknowledgedAt}</td></tr>
    </table>
</body>
</html>";

        var plainTextBody = $@"
Incident Acknowledged

Title: {d.Title}
Severity: {d.Severity}
Acknowledged By: {d.AcknowledgedBy}
Acknowledged At: {d.AcknowledgedAt}
";

        return (subject, htmlBody, plainTextBody);
    }

    private static (string Subject, string HtmlBody, string PlainTextBody) RenderIncidentResolvedTemplate(object data)
    {
        dynamic d = data;
        var subject = $"Incident Resolved: {d.Title}";
        var htmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #28a745;'>Incident Resolved</h2>
    <p>The following incident has been resolved:</p>
    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Title}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Resolved By:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.ResolvedBy}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Resolved At:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.ResolvedAt}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Duration:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Duration}</td></tr>
    </table>
</body>
</html>";

        var plainTextBody = $@"
Incident Resolved

Title: {d.Title}
Resolved By: {d.ResolvedBy}
Resolved At: {d.ResolvedAt}
Duration: {d.Duration}
";

        return (subject, htmlBody, plainTextBody);
    }

    private static (string Subject, string HtmlBody, string PlainTextBody) RenderIncidentEscalationTemplate(object data)
    {
        dynamic d = data;
        var subject = $"[ESCALATION] {d.Title} - Action Required";
        var htmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6;'>
    <h2 style='color: #dc3545;'>Incident Escalation</h2>
    <p style='color: #dc3545; font-weight: bold;'>This incident requires your immediate attention!</p>
    <table style='border-collapse: collapse; width: 100%; max-width: 600px;'>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Title:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Title}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Severity:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Severity}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Status:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.Status}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Escalation Step:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.EscalationStep}</td></tr>
        <tr><td style='padding: 8px; border-bottom: 1px solid #ddd;'><strong>Open Since:</strong></td><td style='padding: 8px; border-bottom: 1px solid #ddd;'>{d.OpenSince}</td></tr>
    </table>
    {(d.CustomMessage != null ? $"<p style='margin-top: 20px; padding: 10px; background-color: #f8f9fa; border-left: 4px solid #007bff;'>{d.CustomMessage}</p>" : "")}
    <p style='margin-top: 20px;'>
        <a href='{d.IncidentUrl}' style='background-color: #dc3545; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>Acknowledge Incident</a>
    </p>
</body>
</html>";

        var plainTextBody = $@"
INCIDENT ESCALATION - ACTION REQUIRED

Title: {d.Title}
Severity: {d.Severity}
Status: {d.Status}
Escalation Step: {d.EscalationStep}
Open Since: {d.OpenSince}

{(d.CustomMessage != null ? $"Message: {d.CustomMessage}" : "")}

View Incident: {d.IncidentUrl}
";

        return (subject, htmlBody, plainTextBody);
    }
}
