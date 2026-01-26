namespace SentinelStack.Domain.Enums;

/// <summary>
/// Tenant subscription tier with associated features and pricing.
/// </summary>
public enum TenantTier
{
    /// <summary>
    /// Free tier with basic features
    /// </summary>
    Free = 0,

    /// <summary>
    /// Starter tier for small teams
    /// </summary>
    Starter = 1,

    /// <summary>
    /// Growth tier for mid-market customers
    /// </summary>
    Growth = 2,

    /// <summary>
    /// Enterprise tier with advanced features (SSO, SAML)
    /// </summary>
    Enterprise = 3,

    /// <summary>
    /// Healthcare tier with HIPAA compliance and strict data isolation (SchemaPerTenant)
    /// </summary>
    Healthcare = 10
}
