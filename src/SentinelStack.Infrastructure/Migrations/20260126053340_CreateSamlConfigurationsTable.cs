using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelStack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSamlConfigurationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "saml_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    idp_entity_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sso_service_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    signing_certificate = table.Column<string>(type: "text", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    idp_metadata_xml = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saml_configurations", x => x.id);
                    table.ForeignKey(
                        name: "fk_saml_configurations_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_saml_configurations_tenant_id",
                table: "saml_configurations",
                column: "tenant_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "saml_configurations");
        }
    }
}
