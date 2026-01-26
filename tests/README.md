# SentinelStack Test Suite

This directory contains all automated tests for the SentinelStack application.

## Test Projects

- **SentinelStack.Domain.UnitTests** - Unit tests for domain entities and value objects
- **SentinelStack.Api.IntegrationTests** - Integration tests for API endpoints using Testcontainers
- **SentinelStack.Infrastructure.Tests** - Unit tests for infrastructure services and repositories

## Test Data Builders

Test data builders are available in the `Builders/` directory of each test project. These builders use the [Bogus](https://github.com/bchavez/Bogus) library to generate realistic fake data for testing.

### Available Builders

- **UserBuilder** - Creates `User` entities with configurable properties
- **IncidentBuilder** - Creates `Incident` entities with configurable properties
- **ServiceBuilder** - Creates `Service` entities with configurable properties
- **EscalationPolicyBuilder** - Creates `EscalationPolicy` entities with configurable steps
- **EscalationStepBuilder** - Creates `EscalationStep` entities for escalation policies

### UserBuilder Examples

```csharp
using SentinelStack.Domain.UnitTests.Builders;
using SentinelStack.Domain.Enums;

// Create a user with all defaults (random email, name, etc.)
var user = UserBuilder.Default().Build();

// Create a user with specific properties
var user = UserBuilder.Default()
    .WithEmail("john.doe@example.com")
    .WithName("John", "Doe")
    .WithRole(UserRole.Admin)
    .Build();

// Create an inactive user
var inactiveUser = UserBuilder.Default()
    .AsInactive()
    .Build();

// Create a user with MFA enabled
var mfaUser = UserBuilder.Default()
    .WithMfaEnabled()
    .Build();

// Create a user for a specific tenant
var tenantId = Guid.NewGuid();
var user = UserBuilder.Default()
    .WithTenant(tenantId)
    .WithEmail("tenant-user@example.com")
    .WithRole(UserRole.Manager)
    .Build();

// Create a user with a custom password (will be hashed with BCrypt)
var user = UserBuilder.Default()
    .WithPassword("CustomPassword123!")
    .Build();
```

### IncidentBuilder Examples

```csharp
using SentinelStack.Domain.UnitTests.Builders;
using SentinelStack.Domain.Enums;

// Create an incident with defaults
var incident = IncidentBuilder.Default().Build();

// Create a high-severity incident
var incident = IncidentBuilder.Default()
    .WithTitle("Database outage")
    .WithDescription("PostgreSQL primary node is down")
    .WithSeverity(IncidentSeverity.Critical)
    .Build();

// Create an incident assigned to a specific user and service
var serviceId = Guid.NewGuid();
var userId = Guid.NewGuid();
var incident = IncidentBuilder.Default()
    .WithService(serviceId)
    .WithAssignee(userId)
    .Build();

// Create an incident that has been acknowledged
var incident = IncidentBuilder.Default()
    .AsAcknowledged(userId)
    .Build();

// Create a resolved incident
var incident = IncidentBuilder.Default()
    .AsResolved(userId)
    .Build();

// Create an incident with specific status
var incident = IncidentBuilder.Default()
    .WithStatus(IncidentStatus.Investigating)
    .Build();

// Create an incident for a specific tenant
var tenantId = Guid.NewGuid();
var incident = IncidentBuilder.Default()
    .WithTenant(tenantId)
    .WithSeverity(IncidentSeverity.High)
    .Build();
```

### ServiceBuilder Examples

```csharp
using SentinelStack.Domain.UnitTests.Builders;

// Create a service with defaults
var service = ServiceBuilder.Default().Build();

// Create a service with specific properties
var service = ServiceBuilder.Default()
    .WithName("API Gateway")
    .WithServiceKey("api-gateway")
    .WithDescription("Main API gateway for all client requests")
    .Build();

// Create a service with owner information
var service = ServiceBuilder.Default()
    .WithName("Payment Service")
    .WithOwner("Platform Team", "platform@example.com")
    .Build();

// Create a service with Slack integration
var service = ServiceBuilder.Default()
    .WithName("Auth Service")
    .WithSlackChannel("#auth-alerts")
    .Build();

// Create an inactive service
var service = ServiceBuilder.Default()
    .WithName("Legacy API")
    .AsInactive()
    .Build();

// Create a service for a specific tenant
var tenantId = Guid.NewGuid();
var service = ServiceBuilder.Default()
    .WithTenant(tenantId)
    .WithName("Customer Portal")
    .Build();
```

### EscalationPolicyBuilder Examples

```csharp
using SentinelStack.Domain.UnitTests.Builders;
using SentinelStack.Domain.Enums;

// Create an escalation policy with defaults
var policy = EscalationPolicyBuilder.Default().Build();

// Create a default escalation policy
var policy = EscalationPolicyBuilder.Default()
    .WithName("Default On-Call Policy")
    .WithDescription("Standard escalation for all incidents")
    .AsDefault()
    .Build();

// Create an escalation policy with steps
var policyId = Guid.NewGuid();
var userId1 = Guid.NewGuid();
var userId2 = Guid.NewGuid();

// Create escalation steps
var step1 = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(1)
    .WithDelay(0) // Immediate
    .NotifyUser(userId1)
    .WithNotificationType(NotificationType.Email)
    .Build();

var step2 = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(2)
    .WithDelay(15) // After 15 minutes
    .NotifyUser(userId2)
    .WithNotificationType(NotificationType.Slack)
    .WithRepeat(5, maxRepeats: 3) // Repeat every 5 minutes, max 3 times
    .Build();

var policy = EscalationPolicyBuilder.Default()
    .WithName("Critical Incident Policy")
    .WithSteps(step1, step2)
    .Build();

// Create a service-specific escalation policy
var serviceId = Guid.NewGuid();
var policy = EscalationPolicyBuilder.Default()
    .WithName("Database Service Policy")
    .ForService(serviceId)
    .Build();

// Create an inactive escalation policy
var policy = EscalationPolicyBuilder.Default()
    .WithName("Deprecated Policy")
    .AsInactive()
    .Build();
```

### EscalationStepBuilder Examples

```csharp
using SentinelStack.Domain.UnitTests.Builders;
using SentinelStack.Domain.Enums;

// Create a simple email notification step
var policyId = Guid.NewGuid();
var step = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(1)
    .WithDelay(0)
    .NotifyEmails("oncall@example.com")
    .WithNotificationType(NotificationType.Email)
    .Build();

// Create a step that notifies a team
var step = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(2)
    .WithDelay(10)
    .NotifyTeam("Platform Team")
    .WithNotificationType(NotificationType.Slack)
    .Build();

// Create a step with repeat configuration
var step = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(3)
    .WithDelay(30)
    .NotifyUser(Guid.NewGuid())
    .WithRepeat(intervalMinutes: 10, maxRepeats: 5)
    .WithNotificationType(NotificationType.Sms)
    .Build();

// Create a step with custom message template
var step = EscalationStepBuilder.Default()
    .ForPolicy(policyId)
    .WithOrder(1)
    .WithMessageTemplate("URGENT: {IncidentTitle} - Severity: {Severity}")
    .NotifyEmails("manager@example.com")
    .WithNotificationType(NotificationType.Email)
    .Build();
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test project
```bash
dotnet test tests/SentinelStack.Domain.UnitTests/
dotnet test tests/SentinelStack.Api.IntegrationTests/
dotnet test tests/SentinelStack.Infrastructure.Tests/
```

### Run with code coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run tests in watch mode
```bash
dotnet watch test --project tests/SentinelStack.Domain.UnitTests/
```

## Test Naming Convention

All tests follow the pattern: `MethodName_Scenario_ExpectedResult`

Examples:
- `CreateUser_WithValidData_ReturnsUser`
- `CreateIncident_WithoutTitle_ThrowsArgumentException`
- `Acknowledge_WhenAlreadyAcknowledged_DoesNothing`

## AAA Pattern

All tests follow the Arrange-Act-Assert pattern with explicit comments:

```csharp
[Fact]
public void CreateUser_WithValidData_ReturnsUser()
{
    // Arrange
    var user = UserBuilder.Default()
        .WithEmail("test@example.com")
        .Build();

    // Act
    var result = user.Email;

    // Assert
    result.Should().Be("test@example.com");
}
```

## Dependencies

- **xUnit** - Test framework
- **FluentAssertions** - Assertion library for more readable tests
- **Bogus** - Fake data generation library
- **BCrypt.Net-Next** - Password hashing for test users
- **Testcontainers.PostgreSql** - Docker-based PostgreSQL for integration tests
- **Microsoft.AspNetCore.Mvc.Testing** - Web application factory for API integration tests

## Best Practices

1. **Use builders for entity creation** - Always use test data builders instead of constructors
2. **Keep tests isolated** - Each test should be independent and not rely on shared state
3. **Use meaningful test names** - Test names should clearly describe what is being tested
4. **Follow AAA pattern** - Always use Arrange-Act-Assert with comments
5. **Test one thing** - Each test should verify one specific behavior
6. **Use FluentAssertions** - Prefer `.Should().Be()` over `Assert.Equal()`
7. **Clean up resources** - Ensure proper disposal of resources in integration tests

## Integration Test Notes

Integration tests use Testcontainers to spin up a real PostgreSQL database in Docker. This ensures tests run against a real database instance and catch SQL/EF Core issues that unit tests might miss.

The `CustomWebApplicationFactory` handles:
- Starting a PostgreSQL container
- Running EF Core migrations
- Configuring the test application
- Providing a test HTTP client

Example integration test:

```csharp
public class AuthControllerTests : BaseIntegrationTest
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            DisplayName = "New User"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
```
