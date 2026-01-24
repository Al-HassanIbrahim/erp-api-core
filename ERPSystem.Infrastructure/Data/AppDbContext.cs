using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
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
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Module> Modules => Set<Module>();
        public DbSet<CompanyModule> CompanyModules => Set<CompanyModule>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
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
        public DbSet<LeaveBalance> leaveBalances => Set<LeaveBalance>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<PayrollLineItem> PayrollLineItems => Set<PayrollLineItem>();



    }
}
