namespace SentinelStack.Domain.Enums;

/// <summary>
/// Database isolation strategy for multi-tenancy.
/// Implements AR27: Dual Multi-Tenancy Model.
/// </summary>
public enum IsolationModel
{
    /// <summary>
    /// Shared schema with tenant_id column for row-level isolation.
    /// Used for Free, Starter, Growth, Enterprise tiers.
    /// </summary>
    SharedSchema = 0,

    /// <summary>
    /// Separate PostgreSQL schema per tenant for strict isolation.
    /// Used for Healthcare tier to meet HIPAA compliance requirements.
    /// </summary>
    SchemaPerTenant = 1
}
