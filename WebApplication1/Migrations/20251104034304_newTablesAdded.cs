using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class newTablesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Customers_Email",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "BrandId",
                table: "Brands",
                newName: "BrandID");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Customers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Customers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Categories",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Categories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Brands",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Brands",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BNPL_PlanTypes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "InterestRate",
                table: "BNPL_PlanTypes",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LatePayInterestRate",
                table: "BNPL_PlanTypes",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BNPL_PlanTypes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerOrders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ShippedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OrderStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PaymentCompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OrderPaymentStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrders", x => x.OrderID);
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ElectronicItems",
                columns: table => new
                {
                    ElectronicItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ElectronicItemName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QOH = table.Column<int>(type: "int", nullable: false),
                    ElectronicItemImage = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BrandID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectronicItems", x => x.ElectronicItemID);
                    table.ForeignKey(
                        name: "FK_ElectronicItems_Brands_BrandID",
                        column: x => x.BrandID,
                        principalTable: "Brands",
                        principalColumn: "BrandID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ElectronicItems_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Role = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BNPL_PLANs",
                columns: table => new
                {
                    Bnpl_PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Bnpl_AmountPerInstallment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bnpl_TotalInstallmentCount = table.Column<int>(type: "int", nullable: false),
                    Bnpl_RemainingInstallmentCount = table.Column<int>(type: "int", nullable: false),
                    Bnpl_StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Bnpl_NextDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bnpl_Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Bnpl_PlanTypeID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BNPL_PLANs", x => x.Bnpl_PlanID);
                    table.ForeignKey(
                        name: "FK_BNPL_PLANs_BNPL_PlanTypes_Bnpl_PlanTypeID",
                        column: x => x.Bnpl_PlanTypeID,
                        principalTable: "BNPL_PlanTypes",
                        principalColumn: "Bnpl_PlanTypeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BNPL_PLANs_CustomerOrders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "CustomerOrders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cashflows",
                columns: table => new
                {
                    CashflowID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CashflowRef = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CashflowDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RefundDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CashflowStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cashflows", x => x.CashflowID);
                    table.ForeignKey(
                        name: "FK_Cashflows_CustomerOrders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "CustomerOrders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerOrderElectronicItems",
                columns: table => new
                {
                    E_ItemID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    OrderItemID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrderElectronicItems", x => new { x.OrderID, x.E_ItemID });
                    table.ForeignKey(
                        name: "FK_CustomerOrderElectronicItems_CustomerOrders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "CustomerOrders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerOrderElectronicItems_ElectronicItems_E_ItemID",
                        column: x => x.E_ItemID,
                        principalTable: "ElectronicItems",
                        principalColumn: "ElectronicItemID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Position = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BNPL_Installments",
                columns: table => new
                {
                    InstallmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InstallmentNo = table.Column<int>(type: "int", nullable: false),
                    Installment_BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Installment_DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OverPaymentCarried = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ArrearsCarried = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDueAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bnpl_Installment_Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    Bnpl_PlanID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BNPL_Installments", x => x.InstallmentID);
                    table.ForeignKey(
                        name: "FK_BNPL_Installments_BNPL_PLANs_Bnpl_PlanID",
                        column: x => x.Bnpl_PlanID,
                        principalTable: "BNPL_PLANs",
                        principalColumn: "Bnpl_PlanID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNo",
                table: "Customers",
                column: "PhoneNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserID",
                table: "Customers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_Bnpl_PlanID",
                table: "BNPL_Installments",
                column: "Bnpl_PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_Bnpl_PlanTypeID",
                table: "BNPL_PLANs",
                column: "Bnpl_PlanTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_OrderID",
                table: "BNPL_PLANs",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_OrderID",
                table: "Cashflows",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_E_ItemID",
                table: "CustomerOrderElectronicItems",
                column: "E_ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerID",
                table: "CustomerOrders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_BrandID",
                table: "ElectronicItems",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_CategoryID",
                table: "ElectronicItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_ElectronicItemName",
                table: "ElectronicItems",
                column: "ElectronicItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserID",
                table: "Employees",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Users_UserID",
                table: "Customers",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Users_UserID",
                table: "Customers");

            migrationBuilder.DropTable(
                name: "BNPL_Installments");

            migrationBuilder.DropTable(
                name: "Cashflows");

            migrationBuilder.DropTable(
                name: "CustomerOrderElectronicItems");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "BNPL_PLANs");

            migrationBuilder.DropTable(
                name: "ElectronicItems");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "CustomerOrders");

            migrationBuilder.DropIndex(
                name: "IX_Customers_PhoneNo",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_UserID",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropColumn(
                name: "InterestRate",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropColumn(
                name: "LatePayInterestRate",
                table: "BNPL_PlanTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BNPL_PlanTypes");

            migrationBuilder.RenameColumn(
                name: "BrandID",
                table: "Brands",
                newName: "BrandId");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Customers",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);
        }
    }
}
