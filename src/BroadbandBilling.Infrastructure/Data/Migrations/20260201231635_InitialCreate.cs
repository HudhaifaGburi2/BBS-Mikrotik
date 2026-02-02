using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BroadbandBilling.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MikroTikDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MikroTikDevices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    SpeedMbps = table.Column<int>(type: "int", nullable: false),
                    DataLimitGB = table.Column<int>(type: "int", nullable: false),
                    BillingCycleDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MikroTikProfileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscribers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NationalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscribers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PppoeAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MikroTikDeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StaticIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ProfileName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastConnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastDisconnectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PppoeAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PppoeAccounts_MikroTikDevices_MikroTikDeviceId",
                        column: x => x.MikroTikDeviceId,
                        principalTable: "MikroTikDevices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PppoeAccounts_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PppoeAccounts_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PppoeAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadBytes = table.Column<long>(type: "bigint", nullable: false),
                    DownloadBytes = table.Column<long>(type: "bigint", nullable: false),
                    TotalBytes = table.Column<long>(type: "bigint", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    CallingStationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageLogs_PppoeAccounts_PppoeAccountId",
                        column: x => x.PppoeAccountId,
                        principalTable: "PppoeAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsageLogs_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsageLogs_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "MikroTikDevices",
                columns: new[] { "Id", "CreatedAt", "IsActive", "LastConnectedAt", "Location", "Name", "Password", "Port", "UpdatedAt", "Username", "IpAddress" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, null, "المقر الرئيسي", "الموجه الرئيسي", "password", 8728, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin", "192.168.88.1" });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "BillingCycleDays", "CreatedAt", "DataLimitGB", "Description", "IsActive", "MikroTikProfileName", "Name", "SpeedMbps", "UpdatedAt", "Price", "Currency" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "باقة إنترنت أساسية بسرعة 10 ميجابت في الثانية", true, "BASIC_10M", "الباقة الأساسية 10 ميجا", 10, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 56.25m, "SAR" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "باقة إنترنت قياسية بسرعة 20 ميجابت في الثانية", true, "STANDARD_20M", "الباقة القياسية 20 ميجا", 20, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 93.75m, "SAR" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "باقة إنترنت مميزة بسرعة 50 ميجابت في الثانية", true, "PREMIUM_50M", "الباقة المميزة 50 ميجا", 50, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 187.50m, "SAR" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), 30, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, "باقة إنترنت للأعمال بسرعة 100 ميجابت في الثانية ببيانات غير محدودة", true, "BUSINESS_100M", "باقة الأعمال 100 ميجا", 100, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 375.00m, "SAR" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DueDate",
                table: "Invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubscriberId",
                table: "Invoices",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubscriptionId",
                table: "Invoices",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MikroTikDevices_IsActive",
                table: "MikroTikDevices",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MikroTikDevices_Name",
                table: "MikroTikDevices",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentReference",
                table: "Payments",
                column: "PaymentReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriberId",
                table: "Payments",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_IsActive",
                table: "Plans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_IsEnabled",
                table: "PppoeAccounts",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_MikroTikDeviceId",
                table: "PppoeAccounts",
                column: "MikroTikDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_SubscriberId",
                table: "PppoeAccounts",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_SubscriptionId",
                table: "PppoeAccounts",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PppoeAccounts_Username",
                table: "PppoeAccounts",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_Email",
                table: "Subscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_IsActive",
                table: "Subscribers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_PhoneNumber",
                table: "Subscribers",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriberId",
                table: "Subscriptions",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriberId_Status",
                table: "Subscriptions",
                columns: new[] { "SubscriberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_PppoeAccountId",
                table: "UsageLogs",
                column: "PppoeAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_SessionStart",
                table: "UsageLogs",
                column: "SessionStart");

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_SubscriberId",
                table: "UsageLogs",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_SubscriberId_SessionStart",
                table: "UsageLogs",
                columns: new[] { "SubscriberId", "SessionStart" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageLogs_SubscriptionId",
                table: "UsageLogs",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "UsageLogs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PppoeAccounts");

            migrationBuilder.DropTable(
                name: "MikroTikDevices");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Subscribers");
        }
    }
}
