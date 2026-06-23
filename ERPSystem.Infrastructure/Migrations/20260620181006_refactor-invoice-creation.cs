using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class refactorinvoicecreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence<int>(
                name: "InventoryDocumentSeq");

            migrationBuilder.CreateSequence<int>(
                name: "SalesDeliverySeq");

            migrationBuilder.CreateSequence<int>(
                name: "SalesInvoiceSeq");

            migrationBuilder.CreateSequence<int>(
                name: "SalesReceiptSeq");

            migrationBuilder.CreateSequence<int>(
                name: "SalesReturnSeq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropSequence(
                name: "InventoryDocumentSeq");

            migrationBuilder.DropSequence(
                name: "SalesDeliverySeq");

            migrationBuilder.DropSequence(
                name: "SalesInvoiceSeq");

            migrationBuilder.DropSequence(
                name: "SalesReceiptSeq");

            migrationBuilder.DropSequence(
                name: "SalesReturnSeq");
        }
    }
}
