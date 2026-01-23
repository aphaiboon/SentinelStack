using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SentinelStack.Api.IntegrationTests.Infrastructure;
using SentinelStack.Application.AuditLogs.DTOs;
using SentinelStack.Application.AuditLogs.Queries;
using SentinelStack.Application.Incidents.DTOs;
using SentinelStack.Domain.Enums;

namespace SentinelStack.Api.IntegrationTests.Controllers;

public class AuditLogsControllerTests : BaseIntegrationTest
{
    public AuditLogsControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ListAuditLogs_ReturnsPagedResults()
    {
        // Arrange - Create an incident to generate audit logs
        await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Audit Log Test Incident",
            Severity = IncidentSeverity.Low
        });

        // Act
        var response = await Client.GetAsync("/api/v1/auditlogs?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetAuditLogsResponse>(JsonOptions);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ListAuditLogs_WithEntityTypeFilter_ReturnsFilteredResults()
    {
        // Arrange - Create an incident to generate audit logs
        await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Filter Test Incident",
            Severity = IncidentSeverity.Low
        });

        // Act
        var response = await Client.GetAsync("/api/v1/auditlogs?entityType=Incident");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetAuditLogsResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(l => l.EntityType == "Incident");
    }

    [Fact]
    public async Task GetAuditLog_WithValidId_ReturnsAuditLog()
    {
        // Arrange - Create an incident to generate audit logs, then get the audit log
        await PostAsync("/api/v1/incidents", new CreateIncidentRequest
        {
            Title = "Get Audit Log Test",
            Severity = IncidentSeverity.Low
        });

        var listResponse = await Client.GetAsync("/api/v1/auditlogs?page=1&pageSize=1");
        var listResult = await listResponse.Content.ReadFromJsonAsync<GetAuditLogsResponse>(JsonOptions);

        if (listResult?.Items.Any() == true)
        {
            var auditLogId = listResult.Items.First().Id;

            // Act
            var response = await Client.GetAsync($"/api/v1/auditlogs/{auditLogId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var auditLog = await response.Content.ReadFromJsonAsync<AuditLogDto>(JsonOptions);
            auditLog.Should().NotBeNull();
            auditLog!.Id.Should().Be(auditLogId);
        }
    }

    [Fact]
    public async Task GetAuditLog_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync($"/api/v1/auditlogs/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
