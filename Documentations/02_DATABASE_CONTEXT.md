# 02_DATABASE_CONTEXT

## 1. DbContext Overview

* **Class Name:** `AppDbContext`
* **Base Class:** `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
* **Constructor:** Injects `DbContextOptions<AppDbContext>`
* **Connection String Key:** `DefaultConnection` (Loaded from `appsettings.json`)
* **Primary Key Strategy:** Guid for Identity (Users/Roles), Identity Integer (`int` auto-increment) for most domain entities, `Guid` for HR entities.

---

## 2. DbSet Table

| Module | DbSet Name | Entity Class | DB Table Name |
| :--- | :--- | :--- | :--- |
| **Core** | `Companies` | `Company` | Companies |
| **Core** | `Modules` | `Module` | Modules |
| **Core** | `CompanyModules` | `CompanyModule` | CompanyModules |
| **Products** | `Products` | `Product` | Products |
| **Products** | `Categories` | `Category` | Categories |
| **Products** | `UnitsOfMeasure` | `UnitOfMeasure` | UnitsOfMeasure |
| **Inventory** | `Warehouses` | `Warehouse` | Warehouses |
| **Inventory** | `StockItems` | `StockItem` | StockItems |
| **Inventory** | `InventoryDocuments` | `InventoryDocument` | InventoryDocuments |
| **Inventory** | `InventoryDocumentLines` | `InventoryDocumentLine` | InventoryDocumentLines |
| **HR** | `Employees` | `Employee` | Employees |
| **HR** | `EmployeeDocuments` | `EmployeeDocument` | EmployeeDocuments |
| **HR** | `Departments` | `Department` | Departments |
| **HR** | `JobPositions` | `JobPosition` | JobPositions |
| **HR** | `Attendances` | `Attendance` | Attendances |
| **HR** | `LeaveRequests` | `LeaveRequest` | LeaveRequests |
| **HR** | `LeaveAttachments` | `LeaveAttachment` | LeaveAttachments |
| **HR** | `LeaveBalances` | `LeaveBalance` | LeaveBalances |
| **HR** | `Payrolls` | `Payroll` | Payrolls |
| **HR** | `PayrollLineItems` | `PayrollLineItem` | PayrollLineItems |
| **Sales** | `Customers` | `Customer` | Customers |
| **Sales** | `SalesInvoices` | `SalesInvoice` | SalesInvoices |
| **Sales** | `SalesInvoiceLines` | `SalesInvoiceLine` | SalesInvoiceLines |
| **Sales** | `SalesDeliveries` | `SalesDelivery` | SalesDeliveries |
| **Sales** | `SalesDeliveryLines` | `SalesDeliveryLine` | SalesDeliveryLines |
| **Sales** | `SalesReceipts` | `SalesReceipt` | SalesReceipts |
| **Sales** | `SalesReceiptAllocations`| `SalesReceiptAllocation`| SalesReceiptAllocations |
| **Sales** | `SalesReturns` | `SalesReturn` | SalesReturns |
| **Sales** | `SalesReturnLines` | `SalesReturnLine` | SalesReturnLines |
| **Contacts** | `Contacts` | `Contact` | Contacts |
| **Expenses** | `ExpenseCategories` | `ExpenseCategory` | ExpenseCategories |
| **Expenses** | `Expenses` | `Expense` | Expenses |
| **CRM** | `Leads` | `Lead` | Leads |
| **CRM** | `Pipelines` | `Pipeline` | Pipelines |

---

## 3. Entity Relationships Map

### Core & Base
| Entity | PK | FKs | Navigations | Relationship Type |
| :--- | :--- | :--- | :--- | :--- |
| `Company` | `Id` (int) | - | `CompanyModules`, `Users` | 1-to-Many |
| `Module` | `Id` (int) | - | `CompanyModules` | 1-to-Many |
| `CompanyModule`| `Id` (int) | `CompanyId`, `ModuleId` | `Company`, `Module` | Many-to-Many join |
| `ApplicationUser`| `Id` (Guid)| `CompanyId` | `Company` | Many-to-1 |

### HR Module
| Entity | PK | FKs | Navigations | Relationship Type |
| :--- | :--- | :--- | :--- | :--- |
| `Employee` | `Id` (Guid)| `DepartmentId`, `PositionId`, `ReportsToId` (Self) | `Department`, `Position`, `Manager`, `DirectReports`, `Attendances`, `LeaveRequests`, `Payrolls` | 1-to-Many |
| `Department` | `Id` (Guid)| `ManagerId` (Employee) | `Manager`, `Employees`, `Positions` | 1-to-Many |
| `JobPosition` | `Id` (Guid)| `DepartmentId` | `Department`, `Employees` | 1-to-Many |
| `Attendance` | `Id` (Guid)| `EmployeeId` | `Employee` | Many-to-1 |
| `LeaveRequest` | `Id` (Guid)| `EmployeeId` | `Employee`, `Attachments` | Many-to-1 / 1-to-Many |
| `LeaveBalance` | `Id` (Guid)| `EmployeeId` | `Employee` | Many-to-1 |
| `Payroll` | `Id` (Guid)| `EmployeeId` | `Employee`, `LineItems` | Many-to-1 / 1-to-Many |

### Products & Inventory
| Entity | PK | FKs | Navigations | Relationship Type |
| :--- | :--- | :--- | :--- | :--- |
| `Category` | `Id` (int) | `ParentCategoryId` (Self) | `ParentCategory`, `Children`, `Products` | 1-to-Many / Hierarchy |
| `UnitOfMeasure`| `Id` (int) | - | `Products` | 1-to-Many |
| `Product` | `Id` (int) | `CategoryId`, `UnitOfMeasureId` | `Category`, `UnitOfMeasure` | Many-to-1 |
| `Warehouse` | `Id` (int) | - | `StockItems`, `InventoryDocuments` | 1-to-Many |
| `StockItem` | `Id` (int) | `WarehouseId`, `ProductId` | `Warehouse`, `Product` | Many-to-1 |
| `InventoryDocument` | `Id` (int) | `DefaultWarehouseId` | `DefaultWarehouse`, `Lines` | Many-to-1 / 1-to-Many |
| `InventoryDocumentLine`| `Id` (int) | `InventoryDocumentId`, `ProductId`, `WarehouseId` | `Document`, `Product`, `Warehouse` | Many-to-1 |

### Sales & CRM
| Entity | PK | FKs | Navigations | Relationship Type |
| :--- | :--- | :--- | :--- | :--- |
| `Customer` | `Id` (int) | - | `Invoices`, `Receipts` | 1-to-Many |
| `SalesInvoice` | `Id` (int) | `CustomerId` | `Customer`, `Lines`, `Deliveries`, `ReceiptAllocations` | Many-to-1 / 1-to-Many |
| `SalesInvoiceLine` | `Id` (int) | `SalesInvoiceId`, `ProductId`, `UnitId` | `SalesInvoice`, `Product`, `Unit` | Many-to-1 |
| `SalesReceipt` | `Id` (int) | `CustomerId` | `Customer`, `Allocations` | Many-to-1 / 1-to-Many |
| `Lead` | `Id` (int) | `AssignedToId` (Employee), `ConvertedCustomerId` | `AssignedTo`, `ConvertedCustomer` | Many-to-1 |
| `Pipeline` | `Id` (int) | `CustomerId`, `LeadId`, `OwnerId` (Employee) | `Customer`, `SourceLead`, `Owner` | Many-to-1 |

---

## 4. EF Configurations

### Global Conventions
* **Default Delete Behavior:** ALL Foreign Keys are globally overridden to `DeleteBehavior.NoAction` using reflection in `OnModelCreating`.
* **Explicit Cascades:** Specific child entities override the global `NoAction` constraint back to `DeleteBehavior.Cascade` to prevent orphaned records:
  * Employee ➜ Attendances, LeaveRequests, LeaveBalances, Payrolls, EmployeeDocuments
  * Payroll ➜ PayrollLineItems
  * LeaveRequest ➜ LeaveAttachments
* **Explicit Restricts/SetNull:**
  * Employee ➜ Manager (`Restrict`)
  * Department ➜ Manager (`Restrict`)
  * Employee ➜ Position (`Restrict`)
  * JobPosition ➜ Department (`SetNull`)
  * Lead / Pipeline ➜ Owners/Converted Customers (`SetNull` / `Restrict`)

### Multi-Tenancy Unique Constraints
To isolate tenant data, unique indexes are heavily utilized as composite keys combining `CompanyId` with the entity's distinct identifier:
* `(CompanyId, Code)`: Product, Warehouse, Customer
* `(CompanyId, Name)`: Category, ExpenseCategory
* `(CompanyId, DocNumber)`: InventoryDocument, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn

### Other Unique Constraints
* `(EmployeeId, Date)` on `Attendance`
* `(EmployeeId, Year, LeaveType)` on `LeaveBalance`
* `(EmployeeId, Month, Year)` on `Payroll`
* `(WarehouseId, ProductId)` on `StockItem`
* Unique indexes on Employee: `EmployeeCode`, `Email`, `NationalId`

### Decimal Precision Rules
* **Monetary/Pricing (18, 2):** Credit limits, Subtotals, Discounts, Taxes, GrandTotals, UnitPrices, LineTotals, Expense Amounts, Payroll Amounts, Deal Values.
* **Quantities/Costs (18, 4):** Inventory quantities, Stock Item quantities (Min/Max/OnHand), Unit Costs, Delivered Quantities.
* **Percentages (5, 2):** DiscountPercent, TaxPercent, Leave entitlement/balances, Overtime/Worked Hours.

---

## 5. Identity Tables

The Context directly manages the ASP.NET Core Identity schema. Due to the `<Guid>` generic parameters, all standard Identity IDs are formatted as `uniqueidentifier` in SQL Server.
* `AspNetUsers` (Extended with custom `ApplicationUser` properties)
* `AspNetRoles`
* `AspNetUserClaims`
* `AspNetUserLogins`
* `AspNetUserRoles`
* `AspNetUserTokens`
* `AspNetRoleClaims`

---

## 6. DbSeeder

Seeding logic is split into static pre-configuration and runtime data injection.

### Runtime Seeder (`DbSeeder.cs`)
Invoked in `Program.cs` immediately after build:
* **Modules (`SeedModulesAsync`):** Ensures all base modules (SALES, INVENTORY, CONTACT, EXPENSES, HR, CRM) exist in the `Modules` table. Performs a diff check against existing keys before inserting.
* **Expense Categories (`SeedDefaultExpenseCategoriesAsync`):** Call-on-demand method. Populates default categories (Rent, Software, Marketing, Supplies, Meals, Utilities, Travel, Other) for a specific `CompanyId` when they activate the Expenses module.

### EF Core Static Seeding (`OnModelCreating`)
* Uses `HasData` to hardcode structural HR reference data with fixed Guids (`1000...` and `2000...`).
* **Departments seeded:** Human Resources (HR), Information Technology (IT).
* **JobPositions seeded:** HR Manager (HRM), Development Manager (DEVM), Senior Developer (SDEV).

---

## 7. Migration Notes

* **`20260205204052_initt.cs`**: The initial baseline migration. Established the full complex schema, relationships, identity tables, and `NoAction` default rules.
* **`20260206123215_company-new-details.cs`**: Added `Phone` (nvarchar) and `TaxNumber` (nvarchar) to the `Companies` table.
* **Workflow Rule:** Any new module/entity must implement multi-tenant checks (`CompanyId`), specify decimal precision in `OnModelCreating`, explicitly manage `DeleteBehavior`, and establish a `CompanyId`-scoped unique index where applicable before generating a migration.