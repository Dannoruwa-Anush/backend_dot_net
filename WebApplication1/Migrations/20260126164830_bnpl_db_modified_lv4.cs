using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class bnpl_db_modified_lv4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceID_ReceiptFileUrl",
                table: "Invoices",
                columns: new[] { "InvoiceID", "ReceiptFileUrl" },
                unique: true,
                filter: "[ReceiptFileUrl] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Invoices_InvoiceID_ReceiptFileUrl",
                table: "Invoices");
        }
    }
}
