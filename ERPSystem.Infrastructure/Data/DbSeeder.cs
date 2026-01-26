using ERPSystem.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Data
{
    public static class DbSeeder
    {
        public static async Task SeedModulesAsync(AppDbContext context)
        {
            if (await context.Modules.AnyAsync())
                return;

            var modules = new List<Module>
            {
                new() { Key = "SALES", Name = "Sales", Description = "Sales management module", IsActive = true },
                new() { Key = "INVENTORY", Name = "Inventory", Description = "Inventory management module", IsActive = true },
            };

            await context.Modules.AddRangeAsync(modules);
            await context.SaveChangesAsync();
        }
    }
}