namespace SentinelStack.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Collection definition for sharing the WebApplicationFactory across tests.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
