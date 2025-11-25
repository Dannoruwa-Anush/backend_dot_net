using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class ConcurrencyCheck_for_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ElectronicItems",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomerOrders",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CustomerOrderElectronicItems",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Cashflows",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BNPL_PlanSettlementSummaries",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BNPL_PLANs",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "BNPL_Installments",
                type: "BINARY(8)",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ElectronicItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Cashflows");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BNPL_PLANs");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "BNPL_Installments");
        }
    }
}
