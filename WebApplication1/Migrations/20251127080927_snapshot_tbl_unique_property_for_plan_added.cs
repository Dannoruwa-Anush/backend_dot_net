using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class snapshot_tbl_unique_property_for_plan_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActiveLatestKey",
                table: "BNPL_PlanSettlementSummaries",
                type: "int",
                nullable: true,
                computedColumnSql: "IF(`Bnpl_PlanSettlementSummary_Status` = 1 AND `IsLatest` = 1, `Bnpl_PlanID`, NULL)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_ActiveLatestKey",
                table: "BNPL_PlanSettlementSummaries",
                column: "ActiveLatestKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanSettlementSummaries_ActiveLatestKey",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "ActiveLatestKey",
                table: "BNPL_PlanSettlementSummaries");
        }
    }
}
