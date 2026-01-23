using System.Runtime.CompilerServices;

namespace SentinelStack.Api.IntegrationTests;

/// <summary>
/// Module initializer to set up test environment before any tests run.
/// </summary>
public static class TestInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Set environment to Testing so appsettings.Testing.json is loaded
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");

        // Set required configuration values via environment variables
        // These use double underscore (__) as the section separator
        Environment.SetEnvironmentVariable("JwtSettings__Secret", "TestSecretKeyThatIsAtLeast32CharactersLongForHMACSHA256");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "SentinelStack.Tests");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "SentinelStack.Tests");
        Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
        Environment.SetEnvironmentVariable("Application__BaseUrl", "https://test.sentinelstack.io");
        Environment.SetEnvironmentVariable("EmailSettings__FromAddress", "test@sentinelstack.io");
        Environment.SetEnvironmentVariable("EmailSettings__FromName", "SentinelStack Test");

        // Note: DatabaseSettings__ConnectionString will be set by CustomWebApplicationFactory
        // when the PostgreSQL container is ready
    }
}
