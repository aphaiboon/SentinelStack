using System.Linq.Expressions;
using Hangfire;
using SentinelStack.Application.Common.Interfaces;

namespace SentinelStack.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire implementation of the background job service.
/// </summary>
public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(Expression<Action<T>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        return BackgroundJob.Schedule(methodCall, delay);
    }

    public string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt)
    {
        return BackgroundJob.Schedule(methodCall, enqueueAt);
    }

    public void RecurringJob<T>(string jobId, Expression<Action<T>> methodCall, string cronExpression)
    {
        Hangfire.RecurringJob.AddOrUpdate(jobId, methodCall, cronExpression);
    }

    public void RemoveRecurringJob(string jobId)
    {
        Hangfire.RecurringJob.RemoveIfExists(jobId);
    }

    public bool Delete(string jobId)
    {
        return BackgroundJob.Delete(jobId);
    }
}
