using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Bulk_Import : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "UnitsOfMeasure",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "UnitsOfMeasure",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "UnitsOfMeasure",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "UnitsOfMeasure",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Categories",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "Categories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityTypeNormalised = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, computedColumnSql: "UPPER([EntityType])", stored: true),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    FileStorageKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    SuccessCount = table.Column<int>(type: "int", nullable: false),
                    FailureCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportJobId = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobResults_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalTable: "ImportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobResults_ImportJobId_RowNumber",
                table: "ImportJobResults",
                columns: new[] { "ImportJobId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_CompanyId_EntityTypeNormalised_IsDeleted",
                table: "ImportJobs",
                columns: new[] { "CompanyId", "EntityTypeNormalised", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_CompanyId_IsDeleted",
                table: "ImportJobs",
                columns: new[] { "CompanyId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "UIX_ImportJobs_CompanyId_IdempotencyKey",
                table: "ImportJobs",
                columns: new[] { "CompanyId", "IdempotencyKey" },
                unique: true,
                filter: "[IdempotencyKey] IS NOT NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobResults");

            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "UnitsOfMeasure");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "UnitsOfMeasure");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UnitsOfMeasure");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "UnitsOfMeasure");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Categories");
        }
    }
}
