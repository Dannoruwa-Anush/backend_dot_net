using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class installment_tbl_overpayment_col_modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OverpaymentCarriedToNextMonth",
                table: "BNPL_Installments",
                newName: "OverpaymentCarriedToNext");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OverpaymentCarriedToNext",
                table: "BNPL_Installments",
                newName: "OverpaymentCarriedToNextMonth");
        }
    }
}
