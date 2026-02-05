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
            Permissions.Core.Companies.Read,
            Permissions.Core.Companies.Update,
            Permissions.Core.Modules.Read,
            Permissions.Core.Modules.Manage,

            // HR - Full access
            Permissions.Hr.Employees.Read,
            Permissions.Hr.Employees.Create,
            Permissions.Hr.Employees.Update,
            Permissions.Hr.Employees.Delete,
            Permissions.Hr.Departments.Read,
            Permissions.Hr.Departments.Create,
            Permissions.Hr.Departments.Update,
            Permissions.Hr.Departments.Delete,
            Permissions.Hr.Positions.Read,
            Permissions.Hr.Positions.Create,
            Permissions.Hr.Positions.Update,
            Permissions.Hr.Positions.Delete,

            // Sales - Full access
            Permissions.Sales.Customers.Read,
            Permissions.Sales.Customers.Create,
            Permissions.Sales.Customers.Update,
            Permissions.Sales.Customers.Delete,
            Permissions.Sales.Invoices.Read,
            Permissions.Sales.Invoices.Create,
            Permissions.Sales.Invoices.Update,
            Permissions.Sales.Invoices.Delete,
            Permissions.Sales.Invoices.Post,
            Permissions.Sales.Invoices.Void,
            Permissions.Sales.Deliveries.Read,
            Permissions.Sales.Deliveries.Create,
            Permissions.Sales.Deliveries.Update,
            Permissions.Sales.Deliveries.Delete,
            Permissions.Sales.Receipts.Read,
            Permissions.Sales.Receipts.Create,
            Permissions.Sales.Receipts.Update,
            Permissions.Sales.Receipts.Delete,
            Permissions.Sales.Returns.Read,
            Permissions.Sales.Returns.Create,
            Permissions.Sales.Returns.Update,
            Permissions.Sales.Returns.Delete,

            // Inventory - Full access
            Permissions.Inventory.Stock.Read,
            Permissions.Inventory.Stock.Adjust,
            Permissions.Inventory.Stock.Transfer,
            Permissions.Inventory.Stock.Opening,
            Permissions.Inventory.Warehouses.Read,
            Permissions.Inventory.Warehouses.Create,
            Permissions.Inventory.Warehouses.Update,
            Permissions.Inventory.Warehouses.Delete,
            Permissions.Inventory.Reports.Read,

            // Products - Full access
            Permissions.Products.Items.Read,
            Permissions.Products.Items.Create,
            Permissions.Products.Items.Update,
            Permissions.Products.Items.Delete,
            Permissions.Products.Categories.Read,
            Permissions.Products.Categories.Create,
            Permissions.Products.Categories.Update,
            Permissions.Products.Categories.Delete,

            // Contacts - Full access
            Permissions.Contacts.Items.Read,
            Permissions.Contacts.Items.Create,
            Permissions.Contacts.Items.Update,
            Permissions.Contacts.Items.Delete,

            // Expenses - Full access
            Permissions.Expenses.Items.Read,
            Permissions.Expenses.Items.Create,
            Permissions.Expenses.Items.Update,
            Permissions.Expenses.Items.Delete,
            Permissions.Expenses.Items.Approve,
            Permissions.Expenses.Items.Reject,
            Permissions.Expenses.Categories.Read,
            Permissions.Expenses.Categories.Create,
            Permissions.Expenses.Categories.Update,
            Permissions.Expenses.Categories.Delete,
        },

        ["Admin"] = new[]
        {
            // Core - Limited (can manage users but not delete, cannot manage modules)
            Permissions.Core.Users.Read,
            Permissions.Core.Users.Create,
            Permissions.Core.Users.Update,
            Permissions.Core.Companies.Read,
            Permissions.Core.Modules.Read,

            // HR - Full access
            Permissions.Hr.Employees.Read,
            Permissions.Hr.Employees.Create,
            Permissions.Hr.Employees.Update,
            Permissions.Hr.Employees.Delete,
            Permissions.Hr.Departments.Read,
            Permissions.Hr.Departments.Create,
            Permissions.Hr.Departments.Update,
            Permissions.Hr.Departments.Delete,
            Permissions.Hr.Positions.Read,
            Permissions.Hr.Positions.Create,
            Permissions.Hr.Positions.Update,
            Permissions.Hr.Positions.Delete,

            // Sales - Full access
            Permissions.Sales.Customers.Read,
            Permissions.Sales.Customers.Create,
            Permissions.Sales.Customers.Update,
            Permissions.Sales.Customers.Delete,
            Permissions.Sales.Invoices.Read,
            Permissions.Sales.Invoices.Create,
            Permissions.Sales.Invoices.Update,
            Permissions.Sales.Invoices.Delete,
            Permissions.Sales.Invoices.Post,
            Permissions.Sales.Invoices.Void,
            Permissions.Sales.Deliveries.Read,
            Permissions.Sales.Deliveries.Create,
            Permissions.Sales.Deliveries.Update,
            Permissions.Sales.Deliveries.Delete,
            Permissions.Sales.Receipts.Read,
            Permissions.Sales.Receipts.Create,
            Permissions.Sales.Receipts.Update,
            Permissions.Sales.Receipts.Delete,
            Permissions.Sales.Returns.Read,
            Permissions.Sales.Returns.Create,
            Permissions.Sales.Returns.Update,
            Permissions.Sales.Returns.Delete,

            // Inventory - Full access
            Permissions.Inventory.Stock.Read,
            Permissions.Inventory.Stock.Adjust,
            Permissions.Inventory.Stock.Transfer,
            Permissions.Inventory.Stock.Opening,
            Permissions.Inventory.Warehouses.Read,
            Permissions.Inventory.Warehouses.Create,
            Permissions.Inventory.Warehouses.Update,
            Permissions.Inventory.Warehouses.Delete,
            Permissions.Inventory.Reports.Read,

            // Products - Full access
            Permissions.Products.Items.Read,
            Permissions.Products.Items.Create,
            Permissions.Products.Items.Update,
            Permissions.Products.Items.Delete,
            Permissions.Products.Categories.Read,
            Permissions.Products.Categories.Create,
            Permissions.Products.Categories.Update,
            Permissions.Products.Categories.Delete,

            // Contacts - Full access
            Permissions.Contacts.Items.Read,
            Permissions.Contacts.Items.Create,
            Permissions.Contacts.Items.Update,
            Permissions.Contacts.Items.Delete,

            // Expenses - Full access (including approve/reject)
            Permissions.Expenses.Items.Read,
            Permissions.Expenses.Items.Create,
            Permissions.Expenses.Items.Update,
            Permissions.Expenses.Items.Delete,
            Permissions.Expenses.Items.Approve,
            Permissions.Expenses.Items.Reject,
            Permissions.Expenses.Categories.Read,
            Permissions.Expenses.Categories.Create,
            Permissions.Expenses.Categories.Update,
            Permissions.Expenses.Categories.Delete,
        },

        ["Manager"] = new[]
        {
            // Core - Read only for users
            Permissions.Core.Users.Read,
            Permissions.Core.Companies.Read,

            // HR - Read and limited write
            Permissions.Hr.Employees.Read,
            Permissions.Hr.Employees.Create,
            Permissions.Hr.Employees.Update,
            Permissions.Hr.Departments.Read,
            Permissions.Hr.Positions.Read,

            // Sales - Full operational access
            Permissions.Sales.Customers.Read,
            Permissions.Sales.Customers.Create,
            Permissions.Sales.Customers.Update,
            Permissions.Sales.Invoices.Read,
            Permissions.Sales.Invoices.Create,
            Permissions.Sales.Invoices.Update,
            Permissions.Sales.Invoices.Post,
            Permissions.Sales.Deliveries.Read,
            Permissions.Sales.Deliveries.Create,
            Permissions.Sales.Deliveries.Update,
            Permissions.Sales.Receipts.Read,
            Permissions.Sales.Receipts.Create,
            Permissions.Sales.Receipts.Update,
            Permissions.Sales.Returns.Read,
            Permissions.Sales.Returns.Create,
            Permissions.Sales.Returns.Update,

            // Inventory - Operational access
            Permissions.Inventory.Stock.Read,
            Permissions.Inventory.Stock.Adjust,
            Permissions.Inventory.Stock.Transfer,
            Permissions.Inventory.Warehouses.Read,
            Permissions.Inventory.Reports.Read,

            // Products - Read and update
            Permissions.Products.Items.Read,
            Permissions.Products.Items.Create,
            Permissions.Products.Items.Update,
            Permissions.Products.Categories.Read,

            // Contacts - Full access
            Permissions.Contacts.Items.Read,
            Permissions.Contacts.Items.Create,
            Permissions.Contacts.Items.Update,

            // Expenses - Can create and view, manager can approve
            Permissions.Expenses.Items.Read,
            Permissions.Expenses.Items.Create,
            Permissions.Expenses.Items.Update,
            Permissions.Expenses.Items.Approve,
            Permissions.Expenses.Categories.Read,
        },

        ["User"] = new[]
        {
            // Core - Minimal
            Permissions.Core.Companies.Read,

            // HR - Read only
            Permissions.Hr.Employees.Read,
            Permissions.Hr.Departments.Read,
            Permissions.Hr.Positions.Read,

            // Sales - Operational (create/view)
            Permissions.Sales.Customers.Read,
            Permissions.Sales.Invoices.Read,
            Permissions.Sales.Invoices.Create,
            Permissions.Sales.Deliveries.Read,
            Permissions.Sales.Deliveries.Create,
            Permissions.Sales.Receipts.Read,
            Permissions.Sales.Returns.Read,

            // Inventory - Read only
            Permissions.Inventory.Stock.Read,
            Permissions.Inventory.Warehouses.Read,
            Permissions.Inventory.Reports.Read,

            // Products - Read only
            Permissions.Products.Items.Read,
            Permissions.Products.Categories.Read,

            // Contacts - Read and create
            Permissions.Contacts.Items.Read,
            Permissions.Contacts.Items.Create,

            // Expenses - Create own expenses
            Permissions.Expenses.Items.Read,
            Permissions.Expenses.Items.Create,
            Permissions.Expenses.Categories.Read,
        }
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