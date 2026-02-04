using ERPSystem.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ERPSystem.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedModulesAsync(AppDbContext context)
        {
            var modules = new List<Module>
            {
                new() { Key = "SALES",     Name = "Sales",     Description = "Sales management module",     IsActive = true },
                new() { Key = "INVENTORY", Name = "Inventory", Description = "Inventory management module", IsActive = true },
                new() { Key = "CONTACT",  Name = "CONTACT",  Description = "Contacts management module",  IsActive = true },
                new() { Key = "EXPENSES",  Name = "Expenses",  Description = "Expenses management module",  IsActive = true },
                new() { Key = "HR",        Name = "HR",        Description = "Human Resources module",      IsActive = true },
                new() { Key = "CRM",       Name = "CRM",       Description = "Customer Relationship Management module", IsActive = true }
            };

            var existingKeys = await context.Modules
                .Select(m => m.Key)
                .ToListAsync();

            var newModules = modules
                .Where(m => !existingKeys.Contains(m.Key))
                .ToList();

            if (newModules.Any())
            {
                await context.Modules.AddRangeAsync(newModules);
                await context.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Seeds default expense categories for a company when the Expenses module is enabled.
        /// Call this when a company first enables the Expenses module.
        /// </summary>
        public static async Task SeedDefaultExpenseCategoriesAsync(AppDbContext context, int companyId)
        {
            var defaultCategories = new[]
            {
                "Rent", "Software", "Marketing", "Supplies",
                "Meals", "Utilities", "Travel", "Other"
            };

            var existingNames = await context.ExpenseCategories
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .Select(c => c.Name)
                .ToListAsync();

            var newCategories = defaultCategories
                .Where(name => !existingNames.Contains(name))
                .Select(name => new ERPSystem.Domain.Entities.Expenses.ExpenseCategory
                {
                    CompanyId = companyId,
                    Name = name,
                    IsDeleted = false
                })
                .ToList();

            if (newCategories.Any())
            {
                await context.ExpenseCategories.AddRangeAsync(newCategories);
                await context.SaveChangesAsync();
            }
        }
    }
}