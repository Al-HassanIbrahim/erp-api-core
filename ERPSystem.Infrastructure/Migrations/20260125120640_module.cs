using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class module : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxNumber",
                table: "Companies",
                newName: "CommercialName");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Modules");

            migrationBuilder.RenameColumn(
                name: "CommercialName",
                table: "Companies",
                newName: "TaxNumber");
        }
    }
}
