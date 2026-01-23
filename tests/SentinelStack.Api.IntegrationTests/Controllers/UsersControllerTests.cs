using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Users.DTOs;
using SentinelStack.Application.Users.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.IntegrationTests.Controllers;

public class UsersControllerTests : BaseIntegrationTest
{
    public UsersControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            DisplayName = "New Test User",
            Email = $"newuser_{Guid.NewGuid():N}@sentinelstack.io",
            Password = "SecurePassword@123!",
            Role = UserRole.Responder
        };

        // Act
        var response = await PostAsync("/api/v1/users", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();
        user!.DisplayName.Should().Be(request.DisplayName);
        user.Email.Should().Be(request.Email);
        user.Role.Should().Be(request.Role);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid():N}@sentinelstack.io";
        var request = new CreateUserRequest
        {
            DisplayName = "First User",
            Email = email,
            Password = "SecurePassword@123!",
            Role = UserRole.Viewer
        };

        // Create first user
        await PostAsync("/api/v1/users", request);

        // Try to create second user with same email
        var duplicateRequest = new CreateUserRequest
        {
            DisplayName = "Second User",
            Email = email,
            Password = "SecurePassword@123!",
            Role = UserRole.Viewer
        };

        // Act
        var response = await PostAsync("/api/v1/users", duplicateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange - Create a user first
        var createRequest = new CreateUserRequest
        {
            DisplayName = "User to Get",
            Email = $"getuser_{Guid.NewGuid():N}@sentinelstack.io",
            Password = "SecurePassword@123!",
            Role = UserRole.Viewer
        };
        var createResponse = await PostAsync("/api/v1/users", createRequest);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/v1/users/{createdUser!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();
        user!.Id.Should().Be(createdUser.Id);
        user.DisplayName.Should().Be(createRequest.DisplayName);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListUsers_ReturnsPagedResults()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUsersResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ListUsers_WithRoleFilter_ReturnsFilteredResults()
    {
        // Arrange - Create a user with specific role
        await PostAsync("/api/v1/users", new CreateUserRequest
        {
            DisplayName = "Admin User",
            Email = $"admin_{Guid.NewGuid():N}@sentinelstack.io",
            Password = "SecurePassword@123!",
            Role = UserRole.Admin
        });

        // Act
        var response = await Client.GetAsync("/api/v1/users?role=Admin");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetUsersResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(u => u.Role == UserRole.Admin);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ReturnsUpdated()
    {
        // Arrange - Create a user first
        var createRequest = new CreateUserRequest
        {
            DisplayName = "Original Name",
            Email = $"update_{Guid.NewGuid():N}@sentinelstack.io",
            Password = "SecurePassword@123!",
            Role = UserRole.Viewer
        };
        var createResponse = await PostAsync("/api/v1/users", createRequest);
        var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>(JsonOptions);

        var updateRequest = new UpdateUserRequest
        {
            DisplayName = "Updated Name",
            Role = UserRole.Admin
        };

        // Act
        var response = await PutAsync($"/api/v1/users/{createdUser!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();
        user!.DisplayName.Should().Be(updateRequest.DisplayName);
        user.Role.Should().Be(updateRequest.Role);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ReturnsCurrentUser()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/users/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>(JsonOptions);
        user.Should().NotBeNull();
        user!.Email.Should().Be(TestUserEmail);
    }
}
