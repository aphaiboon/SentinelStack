using FluentAssertions;
using SentinelStack.Domain.Entities;
using SentinelStack.Domain.Enums;
using Xunit;

namespace SentinelStack.Domain.UnitTests.Entities;

public class IncidentTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Constructor_SetsInitialState_ToOpen()
    {
        // Arrange & Act
        var incident = new Incident(
            _tenantId,
            "Test Incident",
            "Test Description",
            IncidentSeverity.Medium);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Open);
        incident.TenantId.Should().Be(_tenantId);
        incident.Title.Should().Be("Test Incident");
        incident.Description.Should().Be("Test Description");
        incident.Severity.Should().Be(IncidentSeverity.Medium);
    }

    [Fact]
    public void Acknowledge_WhenOpen_TransitionsToAcknowledged()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.Acknowledge(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Acknowledged);
        incident.AcknowledgedBy.Should().Be(_userId);
        incident.AcknowledgedAtUtc.Should().NotBeNull();
        incident.AcknowledgedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Acknowledge_WhenNotOpen_DoesNotChangeState()
    {
        // Arrange
        var incident = CreateOpenIncident();
        incident.Acknowledge(_userId);
        var originalAcknowledgedBy = incident.AcknowledgedBy;
        var originalAcknowledgedAt = incident.AcknowledgedAtUtc;

        // Act - try to acknowledge again
        var differentUser = Guid.NewGuid();
        incident.Acknowledge(differentUser);

        // Assert - should remain unchanged
        incident.Status.Should().Be(IncidentStatus.Acknowledged);
        incident.AcknowledgedBy.Should().Be(originalAcknowledgedBy);
        incident.AcknowledgedAtUtc.Should().Be(originalAcknowledgedAt);
    }

    [Fact]
    public void StartInvestigation_WhenAcknowledged_TransitionsToInvestigating()
    {
        // Arrange
        var incident = CreateAcknowledgedIncident();

        // Act
        incident.StartInvestigation(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Investigating);
    }

    [Fact]
    public void StartInvestigation_WhenOpen_DoesNotChangeState()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.StartInvestigation(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Open);
    }

    [Fact]
    public void Resolve_WhenInvestigating_TransitionsToResolved()
    {
        // Arrange
        var incident = CreateInvestigatingIncident();

        // Act
        incident.Resolve(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Resolved);
        incident.ResolvedBy.Should().Be(_userId);
        incident.ResolvedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_WhenOpen_TransitionsToResolved()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.Resolve(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Resolved);
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_DoesNotChangeState()
    {
        // Arrange
        var incident = CreateResolvedIncident();
        var originalResolvedBy = incident.ResolvedBy;

        // Act
        var differentUser = Guid.NewGuid();
        incident.Resolve(differentUser);

        // Assert
        incident.ResolvedBy.Should().Be(originalResolvedBy);
    }

    [Fact]
    public void Close_WhenResolved_TransitionsToClosed()
    {
        // Arrange
        var incident = CreateResolvedIncident();

        // Act
        incident.Close(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Closed);
    }

    [Fact]
    public void Close_WhenNotResolved_DoesNotChangeState()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.Close(_userId);

        // Assert
        incident.Status.Should().Be(IncidentStatus.Open);
    }

    [Fact]
    public void UpdateSeverity_ChangesTheSeverity()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.UpdateSeverity(IncidentSeverity.Critical, _userId);

        // Assert
        incident.Severity.Should().Be(IncidentSeverity.Critical);
    }

    [Fact]
    public void UpdateDescription_ChangesTheDescription()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.UpdateDescription("New Description", _userId);

        // Assert
        incident.Description.Should().Be("New Description");
    }

    [Fact]
    public void Archive_SetsArchivedTimestamp()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.Archive(_userId);

        // Assert
        incident.ArchivedAtUtc.Should().NotBeNull();
        incident.ArchivedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_SetsSoftDeleteTimestamp()
    {
        // Arrange
        var incident = CreateOpenIncident();

        // Act
        incident.Delete(_userId);

        // Assert
        incident.DeletedAtUtc.Should().NotBeNull();
        incident.DeletedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    // Helper methods for creating incidents in specific states
    private Incident CreateOpenIncident()
    {
        return new Incident(
            _tenantId,
            "Test Incident",
            "Test Description",
            IncidentSeverity.Medium);
    }

    private Incident CreateAcknowledgedIncident()
    {
        var incident = CreateOpenIncident();
        incident.Acknowledge(_userId);
        return incident;
    }

    private Incident CreateInvestigatingIncident()
    {
        var incident = CreateAcknowledgedIncident();
        incident.StartInvestigation(_userId);
        return incident;
    }

    private Incident CreateResolvedIncident()
    {
        var incident = CreateInvestigatingIncident();
        incident.Resolve(_userId);
        return incident;
    }
}
