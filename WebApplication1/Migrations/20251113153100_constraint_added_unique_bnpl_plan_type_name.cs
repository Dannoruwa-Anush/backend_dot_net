using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class constraint_added_unique_bnpl_plan_type_name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_Bnpl_PlanTypeName",
                table: "BNPL_PlanTypes",
                column: "Bnpl_PlanTypeName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanTypes_Bnpl_PlanTypeName",
                table: "BNPL_PlanTypes");
        }
    }
}
