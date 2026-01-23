using SentinelStack.Domain.Entities;

namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Abstraction for Slack integration.
/// Note: Slack integration is deferred to Beta phase. This interface is defined for architecture consistency.
/// </summary>
public interface ISlackService
{
    /// <summary>
    /// Creates a dedicated incident channel in Slack.
    /// </summary>
    /// <param name="incident">The incident to create a channel for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Slack channel ID.</returns>
    Task<string> CreateIncidentChannelAsync(Incident incident, CancellationToken cancellationToken = default);

    /// <summary>
    /// Posts a message to a Slack channel.
    /// </summary>
    /// <param name="channelId">The Slack channel ID.</param>
    /// <param name="message">The message to post.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PostMessageAsync(string channelId, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the topic of a Slack channel.
    /// </summary>
    /// <param name="channelId">The Slack channel ID.</param>
    /// <param name="topic">The new channel topic.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateChannelTopicAsync(string channelId, string topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a Slack channel after incident resolution.
    /// </summary>
    /// <param name="channelId">The Slack channel ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ArchiveChannelAsync(string channelId, CancellationToken cancellationToken = default);
}
