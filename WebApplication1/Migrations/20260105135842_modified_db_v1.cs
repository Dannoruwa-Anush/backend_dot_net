using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class modified_db_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Users_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BNPL_PlanTypes",
                columns: table => new
                {
                    Bnpl_PlanTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Bnpl_PlanTypeName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bnpl_DurationDays = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LatePayInterestRatePerDay = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Bnpl_Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BNPL_PlanTypes", x => x.Bnpl_PlanTypeID);
                    table.ForeignKey(
                        name: "FK_BNPL_PlanTypes_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_BNPL_PlanTypes_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    BrandID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BrandName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.BrandID);
                    table.ForeignKey(
                        name: "FK_Brands_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Brands_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CategoryName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Categories_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNo = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                    table.ForeignKey(
                        name: "FK_Customers_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Customers_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                    table.ForeignKey(
                        name: "FK_Employees_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Employees_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Employees_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhysicalShopSessions",
                columns: table => new
                {
                    PhysicalShopSessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalShopSessions", x => x.PhysicalShopSessionID);
                    table.ForeignKey(
                        name: "FK_PhysicalShopSessions_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_PhysicalShopSessions_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
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
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    BrandID = table.Column<int>(type: "int", nullable: false),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_ElectronicItems_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_ElectronicItems_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerOrders",
                columns: table => new
                {
                    OrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OrderSource = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ShippedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationRequestDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancellationReason = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CancellationApproved = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CancellationRejectionReason = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrderStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PendingPaymentOrderAutoCancelledDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PaymentCompletedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OrderPaymentStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    IsBnplPlanExist = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    PhysicalShopSessionId = table.Column<int>(type: "int", nullable: true),
                    CustomerID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_CustomerOrders_PhysicalShopSessions_PhysicalShopSessionId",
                        column: x => x.PhysicalShopSessionId,
                        principalTable: "PhysicalShopSessions",
                        principalColumn: "PhysicalShopSessionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BNPL_PLANs",
                columns: table => new
                {
                    Bnpl_PlanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Bnpl_InitialPayment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bnpl_AmountPerInstallment = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bnpl_TotalInstallmentCount = table.Column<int>(type: "int", nullable: false),
                    Bnpl_RemainingInstallmentCount = table.Column<int>(type: "int", nullable: false),
                    Bnpl_StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Bnpl_NextDueDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bnpl_Status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    Bnpl_PlanTypeID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_BNPL_PLANs_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_BNPL_PLANs_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerOrderElectronicItems",
                columns: table => new
                {
                    E_ItemID = table.Column<int>(type: "int", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_CustomerOrderElectronicItems_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_CustomerOrderElectronicItems_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    InvoiceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentNo = table.Column<int>(type: "int", nullable: true),
                    InvoiceType = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    InvoiceStatus = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    OrderID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.InvoiceID);
                    table.ForeignKey(
                        name: "FK_Invoices_CustomerOrders_OrderID",
                        column: x => x.OrderID,
                        principalTable: "CustomerOrders",
                        principalColumn: "OrderID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Invoices_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
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
                    OverpaymentCarriedToNext = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LateInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDueAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid_AgainstBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid_AgainstLateInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastLateInterestAppliedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Bnpl_Installment_Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    Bnpl_PlanID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
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
                    table.ForeignKey(
                        name: "FK_BNPL_Installments_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_BNPL_Installments_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BNPL_PlanSettlementSummaries",
                columns: table => new
                {
                    SettlementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CurrentInstallmentNo = table.Column<int>(type: "int", nullable: false),
                    NotYetDueCurrentInstallmentBaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_InstallmentBaseArrears = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_LateInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_PayableSettlement = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Paid_AgainstNotYetDueCurrentInstallmentBaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Paid_AgainstTotalArrears = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Paid_AgainstTotalLateInterest = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total_OverpaymentCarriedToNext = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bnpl_PlanSettlementSummaryRef = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bnpl_PlanSettlementSummary_Status = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    IsLatest = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Bnpl_PlanSettlementSummary_PaymentStatus = table.Column<string>(type: "nvarchar(30)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    Bnpl_PlanID = table.Column<int>(type: "int", nullable: false),
                    ActiveLatestKey = table.Column<int>(type: "int", nullable: true, computedColumnSql: "IF(`Bnpl_PlanSettlementSummary_Status` = 1 AND `IsLatest` = 1, `Bnpl_PlanID`, NULL)", stored: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BNPL_PlanSettlementSummaries", x => x.SettlementID);
                    table.ForeignKey(
                        name: "FK_BNPL_PlanSettlementSummaries_BNPL_PLANs_Bnpl_PlanID",
                        column: x => x.Bnpl_PlanID,
                        principalTable: "BNPL_PLANs",
                        principalColumn: "Bnpl_PlanID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BNPL_PlanSettlementSummaries_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_BNPL_PlanSettlementSummaries_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
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
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", nullable: false),
                    InvoiceID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    UpdatedByUserID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cashflows", x => x.CashflowID);
                    table.ForeignKey(
                        name: "FK_Cashflows_Invoices_InvoiceID",
                        column: x => x.InvoiceID,
                        principalTable: "Invoices",
                        principalColumn: "InvoiceID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cashflows_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Cashflows_Users_UpdatedByUserID",
                        column: x => x.UpdatedByUserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_Bnpl_PlanID_InstallmentNo",
                table: "BNPL_Installments",
                columns: new[] { "Bnpl_PlanID", "InstallmentNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_CreatedByUserID",
                table: "BNPL_Installments",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_Installments_UpdatedByUserID",
                table: "BNPL_Installments",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_Bnpl_PlanTypeID",
                table: "BNPL_PLANs",
                column: "Bnpl_PlanTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_CreatedByUserID",
                table: "BNPL_PLANs",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_OrderID",
                table: "BNPL_PLANs",
                column: "OrderID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PLANs_UpdatedByUserID",
                table: "BNPL_PLANs",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_ActiveLatestKey",
                table: "BNPL_PlanSettlementSummaries",
                column: "ActiveLatestKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_Bnpl_PlanID",
                table: "BNPL_PlanSettlementSummaries",
                column: "Bnpl_PlanID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_CreatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanSettlementSummaries_UpdatedByUserID",
                table: "BNPL_PlanSettlementSummaries",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_Bnpl_PlanTypeName",
                table: "BNPL_PlanTypes",
                column: "Bnpl_PlanTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_CreatedByUserID",
                table: "BNPL_PlanTypes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_BNPL_PlanTypes_UpdatedByUserID",
                table: "BNPL_PlanTypes",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_BrandName",
                table: "Brands",
                column: "BrandName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brands_CreatedByUserID",
                table: "Brands",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_UpdatedByUserID",
                table: "Brands",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_CashflowRef",
                table: "Cashflows",
                column: "CashflowRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_CreatedByUserID",
                table: "Cashflows",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_InvoiceID",
                table: "Cashflows",
                column: "InvoiceID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cashflows_UpdatedByUserID",
                table: "Cashflows",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CategoryName",
                table: "Categories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_CreatedByUserID",
                table: "Categories",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UpdatedByUserID",
                table: "Categories",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_CreatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_E_ItemID",
                table: "CustomerOrderElectronicItems",
                column: "E_ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderElectronicItems_UpdatedByUserID",
                table: "CustomerOrderElectronicItems",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CreatedByUserID",
                table: "CustomerOrders",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerID",
                table: "CustomerOrders",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_PhysicalShopSessionId",
                table: "CustomerOrders",
                column: "PhysicalShopSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_UpdatedByUserID",
                table: "CustomerOrders",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CreatedByUserID",
                table: "Customers",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_PhoneNo",
                table: "Customers",
                column: "PhoneNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UpdatedByUserID",
                table: "Customers",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserID",
                table: "Customers",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_BrandID",
                table: "ElectronicItems",
                column: "BrandID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_CategoryID",
                table: "ElectronicItems",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_CreatedByUserID",
                table: "ElectronicItems",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_ElectronicItemName",
                table: "ElectronicItems",
                column: "ElectronicItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ElectronicItems_UpdatedByUserID",
                table: "ElectronicItems",
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
                name: "IX_Employees_UserID",
                table: "Employees",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedByUserID",
                table: "Invoices",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderID",
                table: "Invoices",
                column: "OrderID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_UpdatedByUserID",
                table: "Invoices",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShopSessions_CreatedByUserID",
                table: "PhysicalShopSessions",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShopSessions_IsActive",
                table: "PhysicalShopSessions",
                column: "IsActive",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalShopSessions_UpdatedByUserID",
                table: "PhysicalShopSessions",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedByUserID",
                table: "Users",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedByUserID",
                table: "Users",
                column: "UpdatedByUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BNPL_Installments");

            migrationBuilder.DropTable(
                name: "BNPL_PlanSettlementSummaries");

            migrationBuilder.DropTable(
                name: "Cashflows");

            migrationBuilder.DropTable(
                name: "CustomerOrderElectronicItems");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "BNPL_PLANs");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ElectronicItems");

            migrationBuilder.DropTable(
                name: "BNPL_PlanTypes");

            migrationBuilder.DropTable(
                name: "CustomerOrders");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "PhysicalShopSessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
