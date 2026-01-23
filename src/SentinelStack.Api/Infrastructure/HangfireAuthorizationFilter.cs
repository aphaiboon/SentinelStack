using Hangfire.Dashboard;

namespace SentinelStack.Api.Infrastructure;

/// <summary>
/// Authorization filter for Hangfire dashboard access.
/// Restricts access to authenticated users with Admin role.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In development, allow access without authentication
        if (httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
        {
            return true;
        }

        // Require authentication
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Require Admin role
        return httpContext.User.IsInRole("Admin");
    }
}
