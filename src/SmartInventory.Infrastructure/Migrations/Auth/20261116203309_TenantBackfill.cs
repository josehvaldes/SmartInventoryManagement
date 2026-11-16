using Microsoft.EntityFrameworkCore.Migrations;
using SmartInventory.Domain.Entities;

#nullable disable

namespace SmartInventory.Infrastructure.Migrations.Auth
{
    /// <inheritdoc />
    public partial class TenantBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var LegacyTenantId = SystemTenants.LegacyTenantId;
            migrationBuilder.InsertData(
            schema: "Auth",
            table: "Tenants",
            columns: new[]
            {
                "Id",
                "Name",
                "Slug",
                "Status",
                "CreatedAt"
            },
            values: new object[]
            {
                LegacyTenantId,
                "Legacy Tenant",
                "legacy-tenant",
                1, // Active
                DateTime.UtcNow
            });

            migrationBuilder.InsertData(
                schema: "Auth",
                table: "Users",
                columns: new[]
                {
                    "Id",
                    "Username",
                    "Email",
                    "Name",
                    "PasswordHash",
                    "IsActive"
                },
                values: new object[]
                {
                    SystemTenants.LegacyUserId,
                    "legacyuser",
                    "legacyuser@example.com",
                    "Legacy User",
                    "AQAAAAEAACcQAAAAELpo10170luQf1mei1TxbRKmy4Mdz5U82Fp/qdhZAAGyguv7S1lHNVHdN2lT0vO7PQ==",
                    true
                });

            migrationBuilder.InsertData(
                schema: "Auth",
                table: "TenantMemberships",
                columns: new[]
                {
                    "Id",
                    "TenantId",
                    "UserId",
                    "Role",
                    "JoinedAt"
                },
                values: new object[]
                {
                    SystemTenants.LegacyTenantMembershipId,
                    LegacyTenantId,
                    SystemTenants.LegacyUserId,
                    1, // Assuming 1 represents a specific role, e.g., Admin
                    DateTime.UtcNow
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "TenantMemberships",
                keyColumn: "Id",
                keyValue: SystemTenants.LegacyTenantMembershipId);

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Users",
                keyColumn: "Id",
                keyValue: SystemTenants.LegacyUserId);

            migrationBuilder.DeleteData(
                schema: "Auth",
                table: "Tenants",
                keyColumn: "Id",
                keyValue: SystemTenants.LegacyTenantId);
        }
    }
}
