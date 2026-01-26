using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Application.Incidents.Queries;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.IntegrationTests.Controllers;

public class IncidentsControllerTests : BaseIntegrationTest
{
    public IncidentsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateIncident_WithValidData_ReturnsCreated()
    {
        // Arrange
        var request = new CreateIncidentRequest
        {
            Title = "Test Incident",
            Description = "This is a test incident",
            Severity = IncidentSeverity.High
        };

        // Act
        var response = await PostAsync("/api/v1/incidents", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var incident = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        incident.Should().NotBeNull();
        incident!.Title.Should().Be(request.Title);
        incident.Description.Should().Be(request.Description);
        incident.Severity.Should().Be(request.Severity);
        incident.Status.Should().Be(IncidentStatus.Open);
    }

    [Fact]
    public async Task CreateIncident_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange - Remove auth header and tenant header
        Client.DefaultRequestHeaders.Authorization = null;
        Client.DefaultRequestHeaders.Remove("X-Tenant-Id");
        var request = new CreateIncidentRequest
        {
            Title = "Test Incident",
            Severity = IncidentSeverity.High
        };

        // Act
        var response = await PostAsync("/api/v1/incidents", request);

        // Assert
        // Without authentication, middleware cannot validate tenant claims
        // Returns either 401 (Unauthorized), 403 (Forbidden), or 400 (BadRequest if missing X-Tenant-Id)
        (response.StatusCode == HttpStatusCode.Unauthorized ||
         response.StatusCode == HttpStatusCode.Forbidden ||
         response.StatusCode == HttpStatusCode.BadRequest).Should().BeTrue($"but got {response.StatusCode}");
    }

    [Fact]
    public async Task GetIncident_WithValidId_ReturnsIncident()
    {
        // Arrange - Create an incident first
        var createRequest = new CreateIncidentRequest
        {
            Title = "Test Incident for Get",
            Severity = IncidentSeverity.Medium
        };
        var createResponse = await PostAsync("/api/v1/incidents", createRequest);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);

        // Act
        var response = await Client.GetAsync($"/api/v1/incidents/{createdIncident!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var incident = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        incident.Should().NotBeNull();
        incident!.Id.Should().Be(createdIncident.Id);
        incident.Title.Should().Be(createRequest.Title);
    }

    [Fact]
    public async Task GetIncident_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/incidents/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListIncidents_ReturnsPagedResults()
    {
        // Arrange - Create multiple incidents
        for (int i = 0; i < 3; i++)
        {
            await PostAsync("/api/v1/incidents", new CreateIncidentRequest
            {
                Title = $"Test Incident {i}",
                Severity = IncidentSeverity.Low
            });
        }

        // Act
        var response = await Client.GetAsync("/api/v1/incidents?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetIncidentsResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ListIncidents_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange - Create an incident
        var createResponse = await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Open Incident",
            Severity = IncidentSeverity.High
        });
        createResponse.EnsureSuccessStatusCode();

        // Act
        var response = await Client.GetAsync("/api/v1/incidents?status=Open");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetIncidentsResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(i => i.Status == IncidentStatus.Open);
    }

    [Fact]
    public async Task UpdateIncident_WithValidData_ReturnsUpdated()
    {
        // Arrange - Create an incident first
        var createRequest = new CreateIncidentRequest
        {
            Title = "Original Title",
            Severity = IncidentSeverity.Low
        };
        var createResponse = await PostAsync("/api/v1/incidents", createRequest);
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);

        var updateRequest = new UpdateIncidentRequest
        {
            Title = "Updated Title",
            Description = "Updated description",
            Severity = IncidentSeverity.Critical
        };

        // Act
        var response = await PutAsync($"/api/v1/incidents/{createdIncident!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var incident = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        incident.Should().NotBeNull();
        incident!.Title.Should().Be(updateRequest.Title);
        incident.Description.Should().Be(updateRequest.Description);
        incident.Severity.Should().Be(updateRequest.Severity);
    }

    [Fact]
    public async Task AcknowledgeIncident_WithOpenIncident_ReturnsAcknowledged()
    {
        // Arrange - Create an incident first
        var createResponse = await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Incident to Acknowledge",
            Severity = IncidentSeverity.High
        });
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);

        // Act
        var response = await PostAsync($"/api/v1/incidents/{createdIncident!.Id}/acknowledge", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var incident = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        incident.Should().NotBeNull();
        incident!.Status.Should().Be(IncidentStatus.Acknowledged);
        incident.AcknowledgedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task ResolveIncident_WithAcknowledgedIncident_ReturnsResolved()
    {
        // Arrange - Create and acknowledge an incident
        var createResponse = await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Incident to Resolve",
            Severity = IncidentSeverity.High
        });
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        await PostAsync($"/api/v1/incidents/{createdIncident!.Id}/acknowledge", new { });

        // Act
        var response = await PostAsync($"/api/v1/incidents/{createdIncident.Id}/resolve", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var incident = await response.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);
        incident.Should().NotBeNull();
        incident!.Status.Should().Be(IncidentStatus.Resolved);
        incident.ResolvedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteIncident_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create an incident first
        var createResponse = await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Incident to Delete",
            Severity = IncidentSeverity.Low
        });
        var createdIncident = await createResponse.Content.ReadFromJsonAsync<IncidentDto>(JsonOptions);

        // Act
        var response = await DeleteAsync($"/api/v1/incidents/{createdIncident!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
