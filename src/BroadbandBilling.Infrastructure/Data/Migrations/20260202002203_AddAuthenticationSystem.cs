using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadbandBilling.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Subscribers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLoginBrowser",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLoginDevice",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLoginOS",
                table: "Subscribers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Subscribers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Subscribers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsSyncedWithMikroTik",
                table: "PppoeAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncDate",
                table: "PppoeAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastValidationDate",
                table: "PppoeAccounts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MacAddress",
                table: "PppoeAccounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriberId1",
                table: "PppoeAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSessions",
                table: "PppoeAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValidationStatus",
                table: "PppoeAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentActiveUsers",
                table: "MikroTikDevices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MikroTikDevices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "MikroTikDevices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPingDate",
                table: "MikroTikDevices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "MikroTikDevices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginIpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Permissions = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoginHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoginStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoginDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "MikroTikDevices",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                columns: new[] { "CurrentActiveUsers", "Description", "IsOnline", "LastPingDate", "MaxUsers" },
                values: new object[] { 0, null, false, null, 1000 });

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_NationalId",
                table: "Subscribers",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_UserId",
                table: "Subscribers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_SubscriberId1",
                table: "PppoeAccounts",
                column: "SubscriberId1");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Role",
                table: "Admins",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistory_LoginDate",
                table: "LoginHistory",
                column: "LoginDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistory_LoginStatus",
                table: "LoginHistory",
                column: "LoginStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LoginHistory_UserId",
                table: "LoginHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_SubscriberId",
                table: "Users",
                column: "SubscriberId",
                unique: true,
                filter: "[SubscriberId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserType",
                table: "Users",
                column: "UserType");

            migrationBuilder.AddForeignKey(
                name: "FK_PppoeAccounts_Subscribers_SubscriberId1",
                table: "PppoeAccounts",
                column: "SubscriberId1",
                principalTable: "Subscribers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PppoeAccounts_Subscribers_SubscriberId1",
                table: "PppoeAccounts");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "LoginHistory");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_NationalId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_Subscribers_UserId",
                table: "Subscribers");

            migrationBuilder.DropIndex(
                name: "IX_PppoeAccounts_SubscriberId1",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "LastLoginBrowser",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "LastLoginDevice",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "LastLoginOS",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "IsSyncedWithMikroTik",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "LastSyncDate",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "LastValidationDate",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "MacAddress",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "SubscriberId1",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "TotalSessions",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                table: "PppoeAccounts");

            migrationBuilder.DropColumn(
                name: "CurrentActiveUsers",
                table: "MikroTikDevices");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MikroTikDevices");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "MikroTikDevices");

            migrationBuilder.DropColumn(
                name: "LastPingDate",
                table: "MikroTikDevices");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "MikroTikDevices");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers",
                column: "Email",
                unique: true);
        }
    }
}
