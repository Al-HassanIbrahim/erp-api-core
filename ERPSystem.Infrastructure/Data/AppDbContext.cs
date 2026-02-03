using ERPSystem.Domain.Entities.Contacts;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Entities.Expenses;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

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

        // Hr
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<JobPosition> JobPositions => Set<JobPosition>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
        public DbSet<LeaveAttachment> LeaveAttachments => Set<LeaveAttachment>();
        public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<PayrollLineItem> PayrollLineItems => Set<PayrollLineItem>();

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

        // Contacts
        public DbSet<Contact> Contacts => Set<Contact>();

        // Expenses
        public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
        public DbSet<Expense> Expenses => Set<Expense>();

        // CRM
        public DbSet<Lead> Leads => Set<Lead>();
        public DbSet<Pipeline> Pipelines => Set<Pipeline>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee Configration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.NationalId).IsUnique();

                entity.HasOne(e => e.Manager)
                    .WithMany(e => e.DirectReports)
                    .HasForeignKey(e => e.ReportsToId)
                    .OnDelete(DeleteBehavior.Restrict);
                // Department relationship
                entity.HasOne(e => e.Department)
                    .WithMany(d => d.Employees)
                    .HasForeignKey(e => e.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
                // JobPosition relationship
                entity.HasOne(e => e.Position)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(e => e.PositionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Department Configration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.HasIndex(d => d.Code).IsUnique();
                entity.HasIndex(d => d.Name).IsUnique();
                // Manager relationship
                entity.HasOne(d => d.Manager)
                    .WithMany()
                    .HasForeignKey(d => d.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // JobPosition Configration
            modelBuilder.Entity<JobPosition>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Code).IsUnique();

                entity.HasOne(p => p.Department)
                    .WithMany(d => d.Positions)
                    .HasForeignKey(p => p.DepartmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Attendance Configration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();

                entity.HasOne(a => a.Employee)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(a => a.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Leave Request Configration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasKey(lr => lr.Id);

                entity.HasOne(lr => lr.Employee)
                    .WithMany(e => e.LeaveRequests)
                    .HasForeignKey(lr => lr.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Leave Balance Configration
            modelBuilder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(lb => lb.Id);
                entity.HasIndex(lb => new { lb.EmployeeId, lb.Year, lb.LeaveType }).IsUnique();

                entity.HasOne(lb => lb.Employee)
                    .WithMany()
                    .HasForeignKey(lb => lb.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payroll Configration
            modelBuilder.Entity<Payroll>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.EmployeeId, p.Month, p.Year }).IsUnique();

                entity.HasOne(p => p.Employee)
                    .WithMany(e => e.Payrolls)
                    .HasForeignKey(p => p.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payroll Item Configration
            modelBuilder.Entity<PayrollLineItem>(entity =>
            {
                entity.HasKey(pli => pli.Id);

                entity.HasOne(pli => pli.Payroll)
                    .WithMany(p => p.LineItems)
                    .HasForeignKey(pli => pli.PayrollId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Employee Document Configration
            modelBuilder.Entity<EmployeeDocument>(entity =>
            {
                entity.HasKey(ed => ed.Id);

                entity.HasOne(ed => ed.Employee)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(ed => ed.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Leave Attachment Configration
            modelBuilder.Entity<LeaveAttachment>(entity =>
            {
                entity.HasKey(la => la.Id);

                entity.HasOne(la => la.LeaveRequest)
                    .WithMany(lr => lr.Attachments)
                    .HasForeignKey(la => la.LeaveRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Lead Configuration
            modelBuilder.Entity<Lead>(entity =>
            {
                entity.HasKey(l => l.Id);
                entity.HasIndex(l => new { l.CompanyId, l.Stage });
                entity.HasIndex(l => l.Email);
                entity.HasIndex(l => l.AssignedToId);
                entity.HasOne(l => l.ConvertedCustomer)
                    .WithMany() 
                    .HasForeignKey(l => l.ConvertedCustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(l => l.AssignedTo)
                    .WithMany()
                    .HasForeignKey(l => l.AssignedToId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
                entity.Property(l => l.CompanyName).IsRequired().HasMaxLength(200);
                entity.Property(l => l.Email).HasMaxLength(100);
                entity.Property(l => l.PhoneNumber).HasMaxLength(20);
                entity.Property(l => l.DealValue).HasPrecision(18, 2);
            });

            // Pipeline Configuration
            modelBuilder.Entity<Pipeline>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => new { p.CompanyId, p.Stage });
                entity.HasIndex(p => p.CustomerId);
                entity.HasIndex(p => p.LeadId);
                // Sales
                entity.HasOne(p => p.Customer)
                    .WithMany() 
                    .HasForeignKey(p => p.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.SourceLead)
                    .WithMany() 
                    .HasForeignKey(p => p.LeadId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(p => p.Owner)
                    .WithMany()
                    .HasForeignKey(p => p.OwnerId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.Property(p => p.DealName).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Amount).HasPrecision(18, 2);
            });

            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.NoAction;

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveRequest>()
                .HasOne(lr => lr.Employee)
                .WithMany(e => e.LeaveRequests)
                .HasForeignKey(lr => lr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveBalance>()
                .HasOne(lb => lb.Employee)
                .WithMany()
                .HasForeignKey(lb => lb.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Payroll>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Payrolls)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PayrollLineItem>()
                .HasOne(pli => pli.Payroll)
                .WithMany(p => p.LineItems)
                .HasForeignKey(pli => pli.PayrollId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeDocument>()
                .HasOne(ed => ed.Employee)
                .WithMany(e => e.Documents)
                .HasForeignKey(ed => ed.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveAttachment>()
                .HasOne(la => la.LeaveRequest)
                .WithMany(lr => lr.Attachments)
                .HasForeignKey(la => la.LeaveRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ReportsToId)
                .OnDelete(DeleteBehavior.Restrict);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var hrDeptId = new Guid("10000000-0000-0000-0000-000000000001");
            var itDeptId = new Guid("10000000-0000-0000-0000-000000000002");
            var seedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = hrDeptId,
                    Code = "HR",
                    Name = "Human Resources",
                    Description = "Human Resources Department",
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Department
                {
                    Id = itDeptId,
                    Code = "IT",
                    Name = "Information Technology",
                    Description = "IT Department",
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );

            var hrManagerId = new Guid("20000000-0000-0000-0000-000000000001");
            var devManagerId = new Guid("20000000-0000-0000-0000-000000000002");
            var seniorDevId = new Guid("20000000-0000-0000-0000-000000000003");

            modelBuilder.Entity<JobPosition>().HasData(
                new JobPosition
                {
                    Id = hrManagerId,
                    Code = "HRM",
                    Title = "HR Manager",
                    Description = "Manages HR operations",
                    Level = PositionLevel.Manager,
                    DepartmentId = hrDeptId,
                    MinSalary = 8000,
                    MaxSalary = 12000,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new JobPosition
                {
                    Id = devManagerId,
                    Code = "DEVM",
                    Title = "Development Manager",
                    Description = "Manages development team",
                    Level = PositionLevel.Manager,
                    DepartmentId = itDeptId,
                    MinSalary = 10000,
                    MaxSalary = 15000,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new JobPosition
                {
                    Id = seniorDevId,
                    Code = "SDEV",
                    Title = "Senior Developer",
                    Description = "Senior software developer",
                    Level = PositionLevel.Senior,
                    DepartmentId = itDeptId,
                    MinSalary = 6000,
                    MaxSalary = 9000,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );

            modelBuilder.Entity<Product>()
                .HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();

            modelBuilder.Entity<Warehouse>()
                .HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();

            modelBuilder.Entity<InventoryDocument>()
                .HasIndex(e => new { e.CompanyId, e.DocNumber }).IsUnique();

            modelBuilder.Entity<StockItem>()
                .HasIndex(e => new { e.WarehouseId, e.ProductId }).IsUnique();

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

            modelBuilder.Entity<ExpenseCategory>()
                .HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();

            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.CompanyId, e.ExpenseDate });
            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.CompanyId, e.ExpenseCategoryId });
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount).HasPrecision(18, 2);
        }
    }
}
