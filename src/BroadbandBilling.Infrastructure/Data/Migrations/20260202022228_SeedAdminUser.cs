using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadbandBilling.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccessFailedCount", "CreatedAt", "Email", "EmailConfirmed", "IsActive", "LastLoginDate", "LastLoginIpAddress", "LockoutEnd", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "RefreshToken", "RefreshTokenExpiryTime", "SubscriberId", "TwoFactorEnabled", "UpdatedAt", "UserType", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), 0, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@broadband.com", true, true, null, null, null, "$2a$11$LQv3c1yqBWLFJGa5Lw4OYeYKvU5hFr5JJ5J0HvG8QwF5J0HvG8Qw.", null, false, null, null, null, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Admin", "admin" });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "Id", "CreatedAt", "FullName", "IsActive", "Permissions", "Role", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "مدير النظام", true, null, "SuperAdmin", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000001") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
