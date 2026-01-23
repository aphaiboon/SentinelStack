namespace SentinelStack.Application.Common.Settings;

/// <summary>
/// Application-wide settings.
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Base URL for the API (used for generating webhook URLs).
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.sentinelstack.io";
}
