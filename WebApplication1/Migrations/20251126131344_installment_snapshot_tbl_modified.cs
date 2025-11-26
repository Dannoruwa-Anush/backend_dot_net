using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class installment_snapshot_tbl_modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid_AgainstArrears",
                table: "BNPL_Installments");

            migrationBuilder.RenameColumn(
                name: "TotalPayableSettlement",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_PayableSettlement");

            migrationBuilder.RenameColumn(
                name: "TotalCurrentOverPayment",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_LateInterest");

            migrationBuilder.RenameColumn(
                name: "TotalCurrentLateInterest",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_InstallmentBaseArrears");

            migrationBuilder.RenameColumn(
                name: "TotalCurrentArrears",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Total_AvailableOverPayment");

            migrationBuilder.RenameColumn(
                name: "InstallmentBaseAmount",
                table: "BNPL_PlanSettlementSummaries",
                newName: "Paid_AgainstTotalLateInterest");

            migrationBuilder.RenameColumn(
                name: "OverPaymentCarried",
                table: "BNPL_Installments",
                newName: "OverPaymentCarriedFromPreviousInstallment");

            migrationBuilder.AddColumn<decimal>(
                name: "NotYetDueCurrentInstallmentBaseAmount",
                table: "BNPL_PlanSettlementSummaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Paid_AgainstNotYetDueCurrentInstallmentBaseAmount",
                table: "BNPL_PlanSettlementSummaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Paid_AgainstTotalArrears",
                table: "BNPL_PlanSettlementSummaries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotYetDueCurrentInstallmentBaseAmount",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "Paid_AgainstNotYetDueCurrentInstallmentBaseAmount",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "Paid_AgainstTotalArrears",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.RenameColumn(
                name: "Total_PayableSettlement",
                table: "BNPL_PlanSettlementSummaries",
                newName: "TotalPayableSettlement");

            migrationBuilder.RenameColumn(
                name: "Total_LateInterest",
                table: "BNPL_PlanSettlementSummaries",
                newName: "TotalCurrentOverPayment");

            migrationBuilder.RenameColumn(
                name: "Total_InstallmentBaseArrears",
                table: "BNPL_PlanSettlementSummaries",
                newName: "TotalCurrentLateInterest");

            migrationBuilder.RenameColumn(
                name: "Total_AvailableOverPayment",
                table: "BNPL_PlanSettlementSummaries",
                newName: "TotalCurrentArrears");

            migrationBuilder.RenameColumn(
                name: "Paid_AgainstTotalLateInterest",
                table: "BNPL_PlanSettlementSummaries",
                newName: "InstallmentBaseAmount");

            migrationBuilder.RenameColumn(
                name: "OverPaymentCarriedFromPreviousInstallment",
                table: "BNPL_Installments",
                newName: "OverPaymentCarried");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid_AgainstArrears",
                table: "BNPL_Installments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
