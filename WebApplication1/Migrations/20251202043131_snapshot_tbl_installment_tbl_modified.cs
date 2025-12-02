using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class snapshot_tbl_installment_tbl_modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Total_OverpaymentCarriedFromPrevious",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.RenameColumn(
                name: "Bnpl_PlanSettlementSummary_Type",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Bnpl_PlanSettlementSummary_PaymentStatus");

            migrationBuilder.RenameColumn(
                name: "OverPaymentCarriedFromPreviousInstallment",
                table: "BNPL_Installments",
                newName: "OverpaymentCarriedToNextMonth");

            migrationBuilder.AddColumn<string>(
                name: "Bnpl_PlanSettlementSummaryRef",
                table: "BNPL_PlanSettlementSummaries",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bnpl_PlanSettlementSummaryRef",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.RenameColumn(
                name: "Bnpl_PlanSettlementSummary_PaymentStatus",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Bnpl_PlanSettlementSummary_Type");

            migrationBuilder.RenameColumn(
                name: "OverpaymentCarriedToNextMonth",
                table: "BNPL_Installments",
                newName: "OverPaymentCarriedFromPreviousInstallment");

            migrationBuilder.AddColumn<decimal>(
                name: "Total_OverpaymentCarriedFromPrevious",
                table: "BNPL_PlanSettlementSummaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
