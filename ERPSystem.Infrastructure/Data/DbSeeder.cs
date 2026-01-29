using ERPSystem.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

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

    }
}