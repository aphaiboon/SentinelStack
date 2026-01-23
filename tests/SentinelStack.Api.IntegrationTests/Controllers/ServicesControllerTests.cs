using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Services.DTOs;
using SentinelStack.Application.Services.Queries;

namespace SentinelStack.Api.IntegrationTests.Controllers;

public class ServicesControllerTests : BaseIntegrationTest
{
    public ServicesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateService_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateServiceRequest
        {
            Name = "Test Service",
            ServiceKey = $"test-service-{Guid.NewGuid():N}".Substring(0, 40),
            Description = "A test service",
            OwnerTeam = "Platform Team",
            ContactEmail = "platform@sentinelstack.io"
        };

        // Act
        var response = await PostAsync("/api/v1/services", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var service = await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);
        service.Should().NotBeNull();
        service!.Name.Should().Be(request.Name);
        service.ServiceKey.Should().Be(request.ServiceKey);
        service.Description.Should().Be(request.Description);
    }

    [Fact]
    public async Task GetService_WithValidId_ReturnsService()
    {
        // Arrange - Create a service first
        var createRequest = new CreateServiceRequest
        {
            Name = "Service to Get",
            ServiceKey = $"get-service-{Guid.NewGuid():N}".Substring(0, 40)
        };
        var createResponse = await PostAsync("/api/v1/services", createRequest);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/v1/services/{createdService!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var service = await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);
        service.Should().NotBeNull();
        service!.Id.Should().Be(createdService.Id);
        service.Name.Should().Be(createRequest.Name);
    }

    [Fact]
    public async Task GetService_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/services/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListServices_ReturnsPagedResults()
    {
        // Arrange - Create a service
        await PostAsync("/api/v1/services", new CreateServiceRequest
        {
            Name = "List Service",
            ServiceKey = $"list-service-{Guid.NewGuid():N}".Substring(0, 40)
        });

        // Act
        var response = await Client.GetAsync("/api/v1/services?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetServicesResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateService_WithValidData_ReturnsUpdated()
    {
        // Arrange - Create a service first
        var createRequest = new CreateServiceRequest
        {
            Name = "Original Service Name",
            ServiceKey = $"update-service-{Guid.NewGuid():N}".Substring(0, 40)
        };
        var createResponse = await PostAsync("/api/v1/services", createRequest);
        var createdService = await createResponse.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);

        var updateRequest = new UpdateServiceRequest
        {
            Name = "Updated Service Name",
            Description = "Updated description",
            OwnerTeam = "New Team"
        };

        // Act
        var response = await PutAsync($"/api/v1/services/{createdService!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var service = await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);
        service.Should().NotBeNull();
        service!.Name.Should().Be(updateRequest.Name);
        service.Description.Should().Be(updateRequest.Description);
        service.OwnerTeam.Should().Be(updateRequest.OwnerTeam);
    }

    [Fact]
    public async Task GetServiceByKey_WithValidKey_ReturnsService()
    {
        // Arrange - Create a service first
        var serviceKey = $"key-service-{Guid.NewGuid():N}".Substring(0, 40);
        await PostAsync("/api/v1/services", new CreateServiceRequest
        {
            Name = "Service By Key",
            ServiceKey = serviceKey
        });

        // Act
        var response = await Client.GetAsync($"/api/v1/services/by-key/{serviceKey}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var service = await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions);
        service.Should().NotBeNull();
        service!.ServiceKey.Should().Be(serviceKey);
    }
}
