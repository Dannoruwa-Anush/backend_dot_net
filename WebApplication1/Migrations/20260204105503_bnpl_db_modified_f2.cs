using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class bnpl_db_modified_f2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoicePaymentChannel",
                table: "Invoices",
                type: "nvarchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PhysicalShopSessionId",
                table: "Cashflows",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_PhysicalShopSessionId",
                table: "Cashflows",
                column: "PhysicalShopSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cashflows_PhysicalShopSessions_PhysicalShopSessionId",
                table: "Cashflows",
                column: "PhysicalShopSessionId",
                principalTable: "PhysicalShopSessions",
                principalColumn: "PhysicalShopSessionID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cashflows_PhysicalShopSessions_PhysicalShopSessionId",
                table: "Cashflows");

            migrationBuilder.DropIndex(
                name: "IX_Cashflows_PhysicalShopSessionId",
                table: "Cashflows");

            migrationBuilder.DropColumn(
                name: "InvoicePaymentChannel",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PhysicalShopSessionId",
                table: "Cashflows");
        }
    }
}
