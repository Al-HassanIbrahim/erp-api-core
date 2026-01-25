using ERPSystem.Domain.Entities.Core;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Core
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<CompanyModule> CompanyModules => Set<CompanyModule>();

        // Products
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

        // Inventory
        public DbSet<Warehouse> Warehouses => Set<Warehouse>();
        public DbSet<StockItem> StockItems => Set<StockItem>();
        public DbSet<InventoryDocument> InventoryDocuments => Set<InventoryDocument>();
        public DbSet<InventoryDocumentLine> InventoryDocumentLines => Set<InventoryDocumentLine>();

        // Sales
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
        public DbSet<SalesInvoiceLine> SalesInvoiceLines => Set<SalesInvoiceLine>();
        public DbSet<SalesDelivery> SalesDeliveries => Set<SalesDelivery>();
        public DbSet<SalesDeliveryLine> SalesDeliveryLines => Set<SalesDeliveryLine>();
        public DbSet<SalesReceipt> SalesReceipts => Set<SalesReceipt>();
        public DbSet<SalesReceiptAllocation> SalesReceiptAllocations => Set<SalesReceiptAllocation>();
        public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
        public DbSet<SalesReturnLine> SalesReturnLines => Set<SalesReturnLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== DISABLE CASCADE DELETE GLOBALLY ==========
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // ========== UNIQUE INDEXES ==========

            // Products
            modelBuilder.Entity<Product>()
                .HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();

            // Inventory
            modelBuilder.Entity<Warehouse>()
                .HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            modelBuilder.Entity<InventoryDocument>()
                .HasIndex(e => new { e.CompanyId, e.DocNumber }).IsUnique();

            modelBuilder.Entity<StockItem>()
                .HasIndex(e => new { e.WarehouseId, e.ProductId }).IsUnique();

            // Sales
            modelBuilder.Entity<Customer>()
                .HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            modelBuilder.Entity<SalesInvoice>()
                .HasIndex(e => new { e.CompanyId, e.InvoiceNumber }).IsUnique();

            modelBuilder.Entity<SalesDelivery>()
                .HasIndex(e => new { e.CompanyId, e.DeliveryNumber }).IsUnique();

            modelBuilder.Entity<SalesReceipt>()
                .HasIndex(e => new { e.CompanyId, e.ReceiptNumber }).IsUnique();

            modelBuilder.Entity<SalesReturn>()
                .HasIndex(e => new { e.CompanyId, e.ReturnNumber }).IsUnique();
        }
    }
}