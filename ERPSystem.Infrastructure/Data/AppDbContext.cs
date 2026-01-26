using ERPSystem.Domain.Entities.Core;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Domain.Entities.Inventory;
using ERPSystem.Domain.Entities.Products;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                // Self-referencing relationship for Manager
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

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Departments
            var hrDeptId = Guid.NewGuid();
            var itDeptId = Guid.NewGuid();

            modelBuilder.Entity<Department>().HasData(
                new Department
                {
                    Id = hrDeptId,
                    Code = "HR",
                    Name = "Human Resources",
                    Description = "Human Resources Department",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Department
                {
                    Id = itDeptId,
                    Code = "IT",
                    Name = "Information Technology",
                    Description = "IT Department",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            // Seed Positions
            var hrManagerId = Guid.NewGuid();
            var devManagerId = Guid.NewGuid();
            var seniorDevId = Guid.NewGuid();

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
                    CreatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow
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
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
