using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelStack.Infrastructure.Migrations;

/// <inheritdoc />
public partial class CreateTenantsTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "audit_logs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_id = table.Column<Guid>(type: "uuid", nullable: true),
                user_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ip_address = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                old_values = table.Column<string>(type: "jsonb", nullable: true),
                new_values = table.Column<string>(type: "jsonb", nullable: true),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                correlation_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_audit_logs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "EscalationPolicies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                ServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EscalationPolicies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "incidents",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                service_id = table.Column<Guid>(type: "uuid", nullable: true),
                AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                acknowledged_by = table.Column<Guid>(type: "uuid", nullable: true),
                acknowledged_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                resolved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                archived_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_incidents", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "services",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                service_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                owner_team = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                contact_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                slack_channel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_services", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "tenants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                subdomain = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                tier = table.Column<int>(type: "integer", nullable: false),
                isolation_model = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                schema_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                connection_string = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                suspended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tenants", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                password_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                mfa_enabled = table.Column<bool>(type: "boolean", nullable: false),
                last_login_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                locked_until_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                created_by = table.Column<Guid>(type: "uuid", nullable: true),
                updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                updated_by = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "WebhookEndpoints",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                EndpointKey = table.Column<string>(type: "text", nullable: false),
                SecretKey = table.Column<string>(type: "text", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                SourceType = table.Column<int>(type: "integer", nullable: false),
                DefaultServiceId = table.Column<Guid>(type: "uuid", nullable: true),
                DefaultSeverity = table.Column<int>(type: "integer", nullable: false),
                TitleMapping = table.Column<string>(type: "text", nullable: true),
                DescriptionMapping = table.Column<string>(type: "text", nullable: true),
                SeverityMapping = table.Column<string>(type: "text", nullable: true),
                LastReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                TotalReceived = table.Column<int>(type: "integer", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_WebhookEndpoints", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "EscalationSteps",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EscalationPolicyId = table.Column<Guid>(type: "uuid", nullable: false),
                StepOrder = table.Column<int>(type: "integer", nullable: false),
                DelayMinutes = table.Column<int>(type: "integer", nullable: false),
                NotificationType = table.Column<int>(type: "integer", nullable: false),
                NotifyUserId = table.Column<Guid>(type: "uuid", nullable: true),
                NotifyTeam = table.Column<string>(type: "text", nullable: true),
                NotifyEmails = table.Column<string>(type: "text", nullable: true),
                MessageTemplate = table.Column<string>(type: "text", nullable: true),
                RepeatIfUnacknowledged = table.Column<bool>(type: "boolean", nullable: false),
                RepeatIntervalMinutes = table.Column<int>(type: "integer", nullable: true),
                MaxRepeats = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_EscalationSteps", x => x.Id);
                table.ForeignKey(
                    name: "FK_EscalationSteps_EscalationPolicies_EscalationPolicyId",
                    column: x => x.EscalationPolicyId,
                    principalTable: "EscalationPolicies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_correlation",
            table: "audit_logs",
            column: "correlation_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_occurred_at",
            table: "audit_logs",
            column: "occurred_at_utc");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_entity",
            table: "audit_logs",
            columns: new[] { "tenant_id", "entity_type", "entity_id" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_id",
            table: "audit_logs",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_occurred",
            table: "audit_logs",
            columns: new[] { "tenant_id", "occurred_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_audit_logs_tenant_user",
            table: "audit_logs",
            columns: new[] { "tenant_id", "user_id" });

        migrationBuilder.CreateIndex(
            name: "IX_EscalationSteps_EscalationPolicyId",
            table: "EscalationSteps",
            column: "EscalationPolicyId");

        migrationBuilder.CreateIndex(
            name: "ix_incidents_severity",
            table: "incidents",
            column: "severity");

        migrationBuilder.CreateIndex(
            name: "ix_incidents_status",
            table: "incidents",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_incidents_tenant_created",
            table: "incidents",
            columns: new[] { "tenant_id", "created_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_incidents_tenant_id",
            table: "incidents",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "ix_incidents_tenant_status",
            table: "incidents",
            columns: new[] { "tenant_id", "status" });

        migrationBuilder.CreateIndex(
            name: "ix_services_tenant_active",
            table: "services",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "ix_services_tenant_id",
            table: "services",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "uq_services_tenant_key",
            table: "services",
            columns: new[] { "tenant_id", "service_key" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_tenants_status",
            table: "tenants",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_tenants_subdomain",
            table: "tenants",
            column: "subdomain",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_active",
            table: "users",
            columns: new[] { "tenant_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "ix_users_tenant_id",
            table: "users",
            column: "tenant_id");

        migrationBuilder.CreateIndex(
            name: "uq_users_tenant_email",
            table: "users",
            columns: new[] { "tenant_id", "email" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "audit_logs");

        migrationBuilder.DropTable(
            name: "EscalationSteps");

        migrationBuilder.DropTable(
            name: "incidents");

        migrationBuilder.DropTable(
            name: "services");

        migrationBuilder.DropTable(
            name: "tenants");

        migrationBuilder.DropTable(
            name: "users");

        migrationBuilder.DropTable(
            name: "WebhookEndpoints");

        migrationBuilder.DropTable(
            name: "EscalationPolicies");
    }
}
