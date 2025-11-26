using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class snapshot_tbl_overpayment_modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Total_AvailableOverPayment",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_OverpaymentCarriedToNext");

            migrationBuilder.AddColumn<decimal>(
                name: "Total_OverpaymentCarriedFromPrevious",
                table: "BNPL_PlanSettlementSummaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Total_OverpaymentCarriedFromPrevious",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.RenameColumn(
                name: "Total_OverpaymentCarriedToNext",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_AvailableOverPayment");
        }
    }
}
