using System.Security.Claims;
using ERPSystem.Application.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ERPSystem.Infrastructure.Identity;

/// <summary>
/// Seeds default roles and permissions for a company.
/// Called when a new company is created (during owner registration).
/// </summary>
public static class CompanyRoleSeeder
{
    private const string PermissionClaimType = "permission";

    /// <summary>
    /// Default roles with their permissions.
    /// These are seeded for every new company.
    /// </summary>
    public static readonly Dictionary<string, string[]> DefaultRolePermissions = new()
    {
        ["Owner"] = new[]
        {
            // Core - Full access
            Permissions.Core.Users.Read,
            Permissions.Core.Users.Create,
            Permissions.Core.Users.Update,
            Permissions.Core.Users.Delete,
            Permissions.Core.Companie.Read,
            Permissions.Core.Companie.Update,
            Permissions.Core.Modules.Read,
            Permissions.Core.Modules.Manage,

            //securityFull access
            Permissions.Security.Roles.Manage,

            // Sales - Full access (updated keys)
            Permissions.Sales.Customers.Manage,
            Permissions.Sales.Customers.Access,
            Permissions.Sales.Customers.Read,
            //Permissions.Sales.Customers.Create,
            //Permissions.Sales.Customers.Update,
            //Permissions.Sales.Customers.Delete,

             Permissions.Sales.Invoices.Manage,
            Permissions.Sales.Invoices.Access,
            Permissions.Sales.Invoices.Read,
            //Permissions.Sales.Invoices.Create,
            //Permissions.Sales.Invoices.Update,
            //Permissions.Sales.Invoices.Delete,
            //Permissions.Sales.Invoices.Post,
            //Permissions.Sales.Invoices.cancel,

            Permissions.Sales.Deliveries.Manage,
            Permissions.Sales.Deliveries.Access,
            Permissions.Sales.Deliveries.Read,
            //Permissions.Sales.Deliveries.Create,
            //Permissions.Sales.Deliveries.Post,
            //Permissions.Sales.Deliveries.Cancel,
            //Permissions.Sales.Deliveries.Delete,

             Permissions.Sales.Receipts.Manage,
            Permissions.Sales.Receipts.Access,
            Permissions.Sales.Receipts.Read,
            //Permissions.Sales.Receipts.Create,
            //Permissions.Sales.Receipts.Post,
            //Permissions.Sales.Receipts.Cancel,
            //Permissions.Sales.Receipts.Delete,

             Permissions.Sales.Returns.Manage,
            Permissions.Sales.Returns.Access,
            Permissions.Sales.Returns.Read,
            //Permissions.Sales.Returns.Create,
            //Permissions.Sales.Returns.Update,
            //Permissions.Sales.Returns.Delete,
            //Permissions.Sales.Returns.Post,
            //Permissions.Sales.Returns.Cancel,

            // Inventory - Full access (updated keys)
            Permissions.Inventory.Stock.Manage,
            //Permissions.Inventory.Stock.Read,
            //Permissions.Inventory.Stock.StockIn,
            //Permissions.Inventory.Stock.StockOut,
            //Permissions.Inventory.Stock.Adjust,
            //Permissions.Inventory.Stock.Transfer,
            //Permissions.Inventory.Stock.Opening,
            Permissions.Inventory.Warehouses.Manage,
            Permissions.Inventory.Warehouses.Read,
            //Permissions.Inventory.Warehouses.Create,
            //Permissions.Inventory.Warehouses.Update,
            //Permissions.Inventory.Warehouses.Delete,
            Permissions.Inventory.Reports.Read,

            // Products - Full access (updated keys: Products.* is now top-level, not Products.Items.*)
            Permissions.Products.Product.Manage,
            Permissions.Products.Product.Read,
            //Permissions.Products.Create,
            //Permissions.Products.Update,
            //Permissions.Products.Delete,
            Permissions.Products.Categories.Manage,
            Permissions.Products.Categories.Read,
            //Permissions.Products.Categories.Create,
            //Permissions.Products.Categories.Update,
            //Permissions.Products.Categories.Delete,
            Permissions.Products.UnitOfMeasures.Manage,
            Permissions.Products.UnitOfMeasures.Read,

            // Expenses - Full access (Approve/Reject removed in Permissions class)
            Permissions.Expenses.Items.Manage,
            Permissions.Expenses.Items.Read,
            //Permissions.Expenses.Items.Create,
            //Permissions.Expenses.Items.Update,
            //Permissions.Expenses.Items.Delete,
            Permissions.Expenses.Categories.Manage,
            Permissions.Expenses.Categories.Read,
            //Permissions.Expenses.Categories.Create,
            //Permissions.Expenses.Categories.Update,
            //Permissions.Expenses.Categories.Delete,

            // Contacts - Full access
            Permissions.Contacts.Contact.Manage,
            Permissions.Contacts.Contact.Read,

            // HR - Full access
            Permissions.Hr.Employees.Manage,
            Permissions.Hr.Employees.Access,
            Permissions.Hr.Employees.Read,
            //Permissions.Hr.Employees.Create,
            //Permissions.Hr.Employees.Update,
            //Permissions.Hr.Employees.Delete,
            Permissions.Hr.Departments.Manage,
            Permissions.Hr.Departments.Access,
            Permissions.Hr.Departments.Read,
            //Permissions.Hr.Departments.Create,
            //Permissions.Hr.Departments.Update,
            //Permissions.Hr.Departments.Delete,
            Permissions.Hr.Positions.Manage,
            Permissions.Hr.Positions.Access,
            Permissions.Hr.Positions.Read,
            //Permissions.Hr.Positions.Create,
            //Permissions.Hr.Positions.Update,
            //Permissions.Hr.Positions.Delete,
            Permissions.Hr.PayRolls.Manage,
            Permissions.Hr.PayRolls.Access,
            Permissions.Hr.PayRolls.Read,
            Permissions.Hr.Positions.Manage,
            Permissions.Hr.Positions.Access,
            Permissions.Hr.Positions.Read,
            Permissions.Hr.LeaveRequests.Manage,
            Permissions.Hr.LeaveRequests.Access,
            Permissions.Hr.LeaveRequests.Read,

            // CRM - Full access
            Permissions.CRM.Leads.Manage,
            Permissions.CRM.Leads.Access,
            Permissions.CRM.Leads.Read,
            Permissions.CRM.Customers.Manage,
            Permissions.CRM.Customers.Access,
            Permissions.CRM.Customers.Read,
        },

        //["Admin"] = new[]
        //{
        //    // Core - Limited
        //    Permissions.Core.Users.Read,
        //    Permissions.Core.Users.Create,
        //    Permissions.Core.Users.Update,
        //    Permissions.Core.Companie.Read,
        //    Permissions.Core.Modules.Read,

        //    // HR - Full access
        //    Permissions.Hr.Employees.Read,
        //    Permissions.Hr.Employees.Create,
        //    Permissions.Hr.Employees.Update,
        //    Permissions.Hr.Employees.Delete,
        //    Permissions.Hr.Departments.Read,
        //    Permissions.Hr.Departments.Create,
        //    Permissions.Hr.Departments.Update,
        //    Permissions.Hr.Departments.Delete,
        //    Permissions.Hr.Positions.Read,
        //    Permissions.Hr.Positions.Create,
        //    Permissions.Hr.Positions.Update,
        //    Permissions.Hr.Positions.Delete,

        //    // Sales - Operational full (updated keys)
        //    Permissions.Sales.Customers.Read,
        //    Permissions.Sales.Customers.Create,
        //    Permissions.Sales.Customers.Update,
        //    Permissions.Sales.Customers.Delete,

        //    Permissions.Sales.Invoices.Read,
        //    Permissions.Sales.Invoices.Create,
        //    Permissions.Sales.Invoices.Update,
        //    Permissions.Sales.Invoices.Delete,
        //    Permissions.Sales.Invoices.Post,
        //    Permissions.Sales.Invoices.cancel,

        //    Permissions.Sales.Deliveries.Read,
        //    Permissions.Sales.Deliveries.Create,
        //    Permissions.Sales.Deliveries.Post,
        //    Permissions.Sales.Deliveries.Cancel,
        //    Permissions.Sales.Deliveries.Delete,

        //    Permissions.Sales.Receipts.Read,
        //    Permissions.Sales.Receipts.Create,
        //    Permissions.Sales.Receipts.Post,
        //    Permissions.Sales.Receipts.Cancel,
        //    Permissions.Sales.Receipts.Delete,

        //    Permissions.Sales.Returns.Read,
        //    Permissions.Sales.Returns.Create,
        //    Permissions.Sales.Returns.Update,
        //    Permissions.Sales.Returns.Delete,
        //    Permissions.Sales.Returns.Post,
        //    Permissions.Sales.Returns.Cancel,

        //    // Inventory - Full access (updated keys)
        //    Permissions.Inventory.Stock.Read,
        //    Permissions.Inventory.Stock.StockIn,
        //    Permissions.Inventory.Stock.StockOut,
        //    Permissions.Inventory.Stock.Adjust,
        //    Permissions.Inventory.Stock.Transfer,
        //    Permissions.Inventory.Stock.Opening,
        //    Permissions.Inventory.Warehouses.Read,
        //    Permissions.Inventory.Warehouses.Create,
        //    Permissions.Inventory.Warehouses.Update,
        //    Permissions.Inventory.Warehouses.Delete,
        //    Permissions.Inventory.Reports.Read,

        //    // Products - Full access (updated keys)
        //    Permissions.Products.Read,
        //    Permissions.Products.Create,
        //    Permissions.Products.Update,
        //    Permissions.Products.Delete,
        //    Permissions.Products.Categories.Read,
        //    Permissions.Products.Categories.Create,
        //    Permissions.Products.Categories.Update,
        //    Permissions.Products.Categories.Delete,
        //    Permissions.Products.UnitOfMeasures.access,

        //    // Expenses - Full access (Approve/Reject removed)
        //    Permissions.Expenses.Items.Read,
        //    Permissions.Expenses.Items.Create,
        //    Permissions.Expenses.Items.Update,
        //    Permissions.Expenses.Items.Delete,
        //    Permissions.Expenses.Categories.Read,
        //    Permissions.Expenses.Categories.Create,
        //    Permissions.Expenses.Categories.Update,
        //    Permissions.Expenses.Categories.Delete,
        //},

        //["Manager"] = new[]
        //{
        //    // Core - Read only
        //    Permissions.Core.Users.Read,
        //    Permissions.Core.Companie.Read,
        //    Permissions.Core.Modules.Read,

        //    // HR - Read and limited write
        //    Permissions.Hr.Employees.Read,
        //    Permissions.Hr.Employees.Create,
        //    Permissions.Hr.Employees.Update,
        //    Permissions.Hr.Departments.Read,
        //    Permissions.Hr.Positions.Read,

        //    // Sales - Operational (updated keys)
        //    Permissions.Sales.Customers.Read,
        //    Permissions.Sales.Customers.Create,
        //    Permissions.Sales.Customers.Update,

        //    Permissions.Sales.Invoices.Read,
        //    Permissions.Sales.Invoices.Create,
        //    Permissions.Sales.Invoices.Update,
        //    Permissions.Sales.Invoices.Post,
        //    Permissions.Sales.Invoices.cancel,

        //    Permissions.Sales.Deliveries.Read,
        //    Permissions.Sales.Deliveries.Create,
        //    Permissions.Sales.Deliveries.Post,
        //    Permissions.Sales.Deliveries.Cancel,

        //    Permissions.Sales.Receipts.Read,
        //    Permissions.Sales.Receipts.Create,
        //    Permissions.Sales.Receipts.Post,
        //    Permissions.Sales.Receipts.Cancel,

        //    Permissions.Sales.Returns.Read,
        //    Permissions.Sales.Returns.Create,
        //    Permissions.Sales.Returns.Update,
        //    Permissions.Sales.Returns.Post,
        //    Permissions.Sales.Returns.Cancel,

        //    // Inventory - Operational (updated keys)
        //    Permissions.Inventory.Stock.Read,
        //    Permissions.Inventory.Stock.StockIn,
        //    Permissions.Inventory.Stock.StockOut,
        //    Permissions.Inventory.Stock.Adjust,
        //    Permissions.Inventory.Stock.Transfer,
        //    Permissions.Inventory.Warehouses.Read,
        //    Permissions.Inventory.Reports.Read,

        //    // Products - Read and update (updated keys)
        //    Permissions.Products.Read,
        //    Permissions.Products.Create,
        //    Permissions.Products.Update,
        //    Permissions.Products.Categories.Read,
        //    Permissions.Products.UnitOfMeasures.access,

        //    // Expenses - Create and view (Approve removed)
        //    Permissions.Expenses.Items.Read,
        //    Permissions.Expenses.Items.Create,
        //    Permissions.Expenses.Items.Update,
        //    Permissions.Expenses.Categories.Read,
        //},

        //["User"] = new[]
        //{
        //    // Core - Minimal
        //    Permissions.Core.Companie.Read,

        //    // HR - Read only
        //    Permissions.Hr.Employees.Read,
        //    Permissions.Hr.Departments.Read,
        //    Permissions.Hr.Positions.Read,

        //    // Sales - Create/view (updated keys)
        //    Permissions.Sales.Customers.Read,
        //    Permissions.Sales.Invoices.Read,
        //    Permissions.Sales.Invoices.Create,
        //    Permissions.Sales.Deliveries.Read,
        //    Permissions.Sales.Deliveries.Create,
        //    Permissions.Sales.Receipts.Read,
        //    Permissions.Sales.Receipts.Create,
        //    Permissions.Sales.Returns.Read,
        //    Permissions.Sales.Returns.Create,

        //    // Inventory - Read only (updated keys)
        //    Permissions.Inventory.Stock.Read,
        //    Permissions.Inventory.Warehouses.Read,
        //    Permissions.Inventory.Reports.Read,

        //    // Products - Read only (updated keys)
        //    Permissions.Products.Read,
        //    Permissions.Products.Categories.Read,
        //    Permissions.Products.UnitOfMeasures.access,

        //    // Expenses - Create own expenses
        //    Permissions.Expenses.Items.Read,
        //    Permissions.Expenses.Items.Create,
        //    Permissions.Expenses.Categories.Read,
        //}
    };

    /// <summary>
    /// Seeds default roles for a specific company.
    /// Should be called when a new company is created.
    /// </summary>
    public static async Task SeedCompanyRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        int companyId,
        CancellationToken ct = default)
    {
        foreach (var (displayName, permissions) in DefaultRolePermissions)
        {
            var scopedRoleName = RoleKey.ForCompany(companyId, displayName);

            // 1) Ensure role exists
            var role = await roleManager.FindByNameAsync(scopedRoleName);
            if (role == null)
            {
                var createResult = await roleManager.CreateAsync(new IdentityRole<Guid>(scopedRoleName));
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{displayName}' for company {companyId}: " +
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                }

                role = await roleManager.FindByNameAsync(scopedRoleName);
                if (role == null)
                {
                    throw new InvalidOperationException(
                        $"Role '{displayName}' not found after creation for company {companyId}.");
                }
            }

            // 2) Ensure all permissions are present as claims
            var existingClaims = await roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims
                .Where(c => c.Type == PermissionClaimType)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (existingPermissions.Contains(permission))
                    continue;

                var addResult = await roleManager.AddClaimAsync(role, new Claim(PermissionClaimType, permission));
                if (!addResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to add permission '{permission}' to role '{displayName}' for company {companyId}: " +
                        string.Join(", ", addResult.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    /// <summary>
    /// Gets the scoped role name for the Owner role of a company.
    /// </summary>
    public static string GetOwnerRoleName(int companyId) => RoleKey.ForCompany(companyId, "Owner");

    /// <summary>
    /// Gets the scoped role name for the Admin role of a company.
    /// </summary>
    public static string GetAdminRoleName(int companyId) => RoleKey.ForCompany(companyId, "Admin");

    /// <summary>
    /// Gets the scoped role name for the Manager role of a company.
    /// </summary>
    public static string GetManagerRoleName(int companyId) => RoleKey.ForCompany(companyId, "Manager");

    /// <summary>
    /// Gets the scoped role name for the User role of a company.
    /// </summary>
    public static string GetUserRoleName(int companyId) => RoleKey.ForCompany(companyId, "User");
}