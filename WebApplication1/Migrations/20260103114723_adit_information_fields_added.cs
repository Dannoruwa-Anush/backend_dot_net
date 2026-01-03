using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class adit_information_fields_added : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "ElectronicItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "ElectronicItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "CustomerOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "CustomerOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "CustomerOrderElectronicItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "CustomerOrderElectronicItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Cashflows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Cashflows",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Brands",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Brands",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "BNPL_PlanTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "BNPL_PlanTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "BNPL_PLANs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "BNPL_PLANs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "BNPL_Installments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "BNPL_Installments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedByUserID",
                table: "Users",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedByUserID",
                table: "Users",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedByUserID",
                table: "Employees",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UpdatedByUserID",
                table: "Employees",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_CreatedByUserID",
                table: "ElectronicItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_UpdatedByUserID",
                table: "ElectronicItems",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserID",
                table: "Customers",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UpdatedByUserID",
                table: "Customers",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CreatedByUserID",
                table: "CustomerOrders",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_UpdatedByUserID",
                table: "CustomerOrders",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_CreatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_UpdatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedByUserID",
                table: "Categories",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedByUserID",
                table: "Categories",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_CreatedByUserID",
                table: "Cashflows",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_UpdatedByUserID",
                table: "Cashflows",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_CreatedByUserID",
                table: "Brands",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_UpdatedByUserID",
                table: "Brands",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_CreatedByUserID",
                table: "BNPL_PlanTypes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_UpdatedByUserID",
                table: "BNPL_PlanTypes",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_CreatedByUserID",
                table: "BNPL_PLANs",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_UpdatedByUserID",
                table: "BNPL_PLANs",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_CreatedByUserID",
                table: "BNPL_Installments",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_UpdatedByUserID",
                table: "BNPL_Installments",
                column: "UpdatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_Installments_Users_CreatedByUserID",
                table: "BNPL_Installments",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_Installments_Users_UpdatedByUserID",
                table: "BNPL_Installments",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PLANs_Users_CreatedByUserID",
                table: "BNPL_PLANs",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PLANs_Users_UpdatedByUserID",
                table: "BNPL_PLANs",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PlanSettlementSummaries_Users_CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PlanSettlementSummaries_Users_UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PlanTypes_Users_CreatedByUserID",
                table: "BNPL_PlanTypes",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_BNPL_PlanTypes_Users_UpdatedByUserID",
                table: "BNPL_PlanTypes",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_Users_CreatedByUserID",
                table: "Brands",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Brands_Users_UpdatedByUserID",
                table: "Brands",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cashflows_Users_CreatedByUserID",
                table: "Cashflows",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Cashflows_Users_UpdatedByUserID",
                table: "Cashflows",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_CreatedByUserID",
                table: "Categories",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UpdatedByUserID",
                table: "Categories",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrderElectronicItems_Users_CreatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrderElectronicItems_Users_UpdatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrders_Users_CreatedByUserID",
                table: "CustomerOrders",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrders_Users_UpdatedByUserID",
                table: "CustomerOrders",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_CreatedByUserID",
                table: "Customers",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UpdatedByUserID",
                table: "Customers",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicItems_Users_CreatedByUserID",
                table: "ElectronicItems",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicItems_Users_UpdatedByUserID",
                table: "ElectronicItems",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_CreatedByUserID",
                table: "Employees",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_UpdatedByUserID",
                table: "Employees",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_CreatedByUserID",
                table: "Users",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_UpdatedByUserID",
                table: "Users",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_Installments_Users_CreatedByUserID",
                table: "BNPL_Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_Installments_Users_UpdatedByUserID",
                table: "BNPL_Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PLANs_Users_CreatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PLANs_Users_UpdatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PlanSettlementSummaries_Users_CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PlanSettlementSummaries_Users_UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PlanTypes_Users_CreatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_BNPL_PlanTypes_Users_UpdatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Brands_Users_CreatedByUserID",
                table: "Brands");

            migrationBuilder.DropForeignKey(
                name: "FK_Brands_Users_UpdatedByUserID",
                table: "Brands");

            migrationBuilder.DropForeignKey(
                name: "FK_Cashflows_Users_CreatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropForeignKey(
                name: "FK_Cashflows_Users_UpdatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_CreatedByUserID",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UpdatedByUserID",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrderElectronicItems_Users_CreatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrderElectronicItems_Users_UpdatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrders_Users_CreatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrders_Users_UpdatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_CreatedByUserID",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UpdatedByUserID",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicItems_Users_CreatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicItems_Users_UpdatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_CreatedByUserID",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_UpdatedByUserID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedByUserID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UpdatedByUserID",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_ElectronicItems_CreatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropIndex(
                name: "IX_ElectronicItems_UpdatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CreatedByUserID",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UpdatedByUserID",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrders_CreatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrders_UpdatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrderElectronicItems_CreatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrderElectronicItems_UpdatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropIndex(
                name: "IX_Categories_CreatedByUserID",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UpdatedByUserID",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Cashflows_CreatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropIndex(
                name: "IX_Cashflows_UpdatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropIndex(
                name: "IX_Brands_CreatedByUserID",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Brands_UpdatedByUserID",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanTypes_CreatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanTypes_UpdatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanSettlementSummaries_CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PlanSettlementSummaries_UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PLANs_CreatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_PLANs_UpdatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_Installments_CreatedByUserID",
                table: "BNPL_Installments");

            migrationBuilder.DropIndex(
                name: "IX_BNPL_Installments_UpdatedByUserID",
                table: "BNPL_Installments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "ElectronicItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "CustomerOrderElectronicItems");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Cashflows");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "BNPL_PLANs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "BNPL_Installments");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "BNPL_Installments");
        }
    }
}
