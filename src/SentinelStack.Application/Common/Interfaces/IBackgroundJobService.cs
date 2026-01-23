namespace SentinelStack.Application.Common.Interfaces;

/// <summary>
/// Service interface for scheduling background jobs.
/// </summary>
public interface IBackgroundJobService
{
    /// <summary>
    /// Enqueues a job to run immediately in the background.
    /// </summary>
    string Enqueue<T>(System.Linq.Expressions.Expression<Action<T>> methodCall);

    /// <summary>
    /// Schedules a job to run at a specific time.
    /// </summary>
    string Schedule<T>(System.Linq.Expressions.Expression<Action<T>> methodCall, TimeSpan delay);

    /// <summary>
    /// Schedules a job to run at a specific date/time.
    /// </summary>
    string Schedule<T>(System.Linq.Expressions.Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);

    /// <summary>
    /// Creates or updates a recurring job.
    /// </summary>
    void RecurringJob<T>(string jobId, System.Linq.Expressions.Expression<Action<T>> methodCall, string cronExpression);

    /// <summary>
    /// Removes a recurring job.
    /// </summary>
    void RemoveRecurringJob(string jobId);

    /// <summary>
    /// Deletes a scheduled or enqueued job.
    /// </summary>
    bool Delete(string jobId);
}
