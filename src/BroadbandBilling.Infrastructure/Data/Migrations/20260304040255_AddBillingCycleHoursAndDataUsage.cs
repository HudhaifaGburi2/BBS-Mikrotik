using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BroadbandBilling.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingCycleHoursAndDataUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DataLimitExceeded",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataLimitExceededAt",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DataUsedBytes",
                table: "Subscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BillingCycleHours",
                table: "Plans",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "BillingCycleHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "BillingCycleHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "BillingCycleHours",
                value: null);

            migrationBuilder.UpdateData(
                table: "Plans",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "BillingCycleHours",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataLimitExceeded",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DataLimitExceededAt",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DataUsedBytes",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BillingCycleHours",
                table: "Plans");
        }
    }
}
