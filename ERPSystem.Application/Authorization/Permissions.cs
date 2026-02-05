using Microsoft.AspNetCore.Authorization;

namespace ERPSystem.Application.Authorization;

/// <summary>
/// Centralized permission constants for the ERP system.
/// Naming convention: module.resource.action
/// </summary>
public static class Permissions
{
    public static class Hr
    {
        public static class Positions
        {
            public const string Read = "hr.positions.read";
            public const string Create = "hr.positions.create";
            public const string Update = "hr.positions.update";
            public const string Delete = "hr.positions.delete";
        }

        public static class Departments
        {
            public const string Read = "hr.departments.read";
            public const string Create = "hr.departments.create";
            public const string Update = "hr.departments.update";
            public const string Delete = "hr.departments.delete";
        }

        public static class Employees
        {
            public const string Read = "hr.employees.read";
            public const string Create = "hr.employees.create";
            public const string Update = "hr.employees.update";
            public const string Delete = "hr.employees.delete";
        }
    }

    public static class Sales
    {
        public static class Invoices
        {
            public const string Read = "sales.invoices.read";
            public const string Create = "sales.invoices.create";
            public const string Update = "sales.invoices.update";
            public const string Delete = "sales.invoices.delete";
            public const string Post = "sales.invoices.post";
            public const string cancel = "sales.invoices.cancel";
        }

        public static class Deliveries
        {
            public const string Read = "sales.deliveries.read";
            public const string Create = "sales.deliveries.create";
            public const string Post = "sales.deliveries.post";
            public const string Cancel = "sales.deliveries.cancel";
            public const string Delete = "sales.deliveries.delete";
        }

        public static class Receipts
        {
            public const string Read = "sales.receipts.read";
            public const string Create = "sales.receipts.create";
            public const string Post = "sales.receipts.post";
            public const string Cancel = "sales.receipts.cancel";
            public const string Delete = "sales.receipts.delete";
        }

        public static class Returns
        {
            public const string Read = "sales.returns.read";
            public const string Create = "sales.returns.create";
            public const string Update = "sales.returns.update";
            public const string Delete = "sales.returns.delete";
            public const string Post = "sales.returns.post";
            public const string Cancel = "sales.returns.cancel";
        }

        public static class Customers
        {
            public const string Read = "sales.customers.read";
            public const string Create = "sales.customers.create";
            public const string Update = "sales.customers.update";
            public const string Delete = "sales.customers.delete";
        }
    }

    public static class Inventory
    {
        public static class Stock
        {
            public const string Read = "inventory.stock.read";
            public const string StockIn = "inventory.stock.stockIn";
            public const string StockOut = "inventory.stock.stockOut";
            public const string Adjust = "inventory.stock.adjust";
            public const string Transfer = "inventory.stock.transfer";
            public const string Opening = "inventory.stock.opening";
        }

        public static class Warehouses
        {
            public const string Read = "inventory.warehouses.read";
            public const string Create = "inventory.warehouses.create";
            public const string Update = "inventory.warehouses.update";
            public const string Delete = "inventory.warehouses.delete";
        }

        public static class Reports
        {
            public const string Read = "inventory.reports.read";
        }
    }

    public static class Products
    {
        public const string Read = "products.read";
        public const string Create = "products.create";
        public const string Update = "products.update";
        public const string Delete = "products.delete";
        public static class Categories
        {
            public const string Read = "products.categories.read";
            public const string Create = "products.categories.create";
            public const string Update = "products.categories.update";
            public const string Delete = "products.categories.delete";
        }
        public static class UnitOfMeasures
        {
            public const string access = "Products.UnitOfMeasures.access";
        }
        
    }

    public static class Expenses
    {
        public static class Items
        {
            public const string Read = "expenses.items.read";
            public const string Create = "expenses.items.create";
            public const string Update = "expenses.items.update";
            public const string Delete = "expenses.items.delete";
            public const string Approve = "expenses.items.approve";
            public const string Reject = "expenses.items.reject";
        }

        public static class Categories
        {
            public const string Read = "expenses.categories.read";
            public const string Create = "expenses.categories.create";
            public const string Update = "expenses.categories.update";
            public const string Delete = "expenses.categories.delete";
        }
    }

    public static class Core
    {
        public static class Companies
        {
            public const string Read = "core.companies.read";
            public const string Update = "core.companies.update";
        }

        public static class Users
        {
            public const string Read = "core.users.read";
            public const string Create = "core.users.create";
            public const string Update = "core.users.update";
            public const string Delete = "core.users.delete";
        }

        public static class Modules
        {
            public const string Read = "core.modules.read";
            public const string Manage = "core.modules.manage";
        }
    }
}