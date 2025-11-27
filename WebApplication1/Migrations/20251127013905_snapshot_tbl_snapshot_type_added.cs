using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class snapshot_tbl_snapshot_type_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bnpl_PlanSettlementSummary_Type",
                table: "BNPL_PlanSettlementSummaries",
                type: "nvarchar(30)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bnpl_PlanSettlementSummary_Type",
                table: "BNPL_PlanSettlementSummaries");
        }
    }
}
