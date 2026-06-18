# Module Index

## Overview
Comprehensive map of business modules within the ERP System.

---
## Responsibilities
Acts as the central directory for locating functionality, services, repositories, and entities for any given domain concept.

---
## Workflow
Modules are logical groupings applied across the Clean Architecture layers.

---
## Dependencies
- Core is depended upon by almost all other modules.

---
## Public APIs
N/A

---
## Limitations
- Modules are not currently physically isolated into separate projects.

---
## Technical Debt
N/A

---
## Modules

### 1. Core
- **Responsibility:** Tenant management, User identity, Role and Permission assignment, System-wide Modules.
- **Entities:** `Company`, `ApplicationUser`, `Module`, `CompanyModule`.
- **Services:** `AuthService`, `CompanyProfileService`, `ModuleAccessService`.
- **Endpoints:** `/api/auth/*`, `/api/companies/*`, `/api/permissions/*`.

### 2. Products
- **Responsibility:** Product catalog, item configurations, unit measurements.
- **Entities:** `Product`, `Category`, `UnitOfMeasure`.
- **Services:** `ProductService`, `CategoryService`.

### 3. Inventory
- **Responsibility:** Stock tracking, warehouse management, inventory movements.
- **Entities:** `Warehouse`, `StockItem`, `InventoryDocument`.
- **Services:** `WarehouseService`, `InventoryService`, `InventoryReportsService`.

### 4. HR (Human Resources)
- **Responsibility:** Employee records, organizational structure, attendance, leaves, payroll.
- **Entities:** `Employee`, `Department`, `JobPosition`, `Attendance`, `LeaveRequest`, `Payroll`.
- **Services:** `EmployeeService`, `AttendanceService`, `PayrollService`.

### 5. Sales
- **Responsibility:** Customer management, invoicing, deliveries, payments, returns.
- **Entities:** `Customer`, `SalesInvoice`, `SalesDelivery`, `SalesReceipt`, `SalesReturn`.
- **Services:** `CustomerService`, `SalesInvoiceService`, `SalesDeliveryService`.

### 6. Contacts
- **Responsibility:** General contact ledger.
- **Entities:** `Contact`.
- **Services:** `ContactService`.

### 7. Expenses
- **Responsibility:** Tracking outgoing costs, categorizing spending.
- **Entities:** `Expense`, `ExpenseCategory`.
- **Services:** `ExpenseService`, `ExpenseCategoryService`, `ExpenseStatsService`.

### 8. CRM (Customer Relationship Management)
- **Responsibility:** Sales pipeline, lead tracking, conversion.
- **Entities:** `Lead`, `Pipeline`.
- **Services:** `LeadService`, `PipelineService`.

---
## AI Quick Context
### Depends On
Clean Architecture structure
### Uses
Entities, Services, Controllers
### Used By
Future AI agents navigating the codebase
### Related Modules
All
### Shared Components
N/A
### Entities
Listed above.
### Business Rules
N/A
### Endpoints
N/A
### Future Refactoring
Vertical Slice physical separation.
### Modification Risk
Low
### Confidence
High (Verified from folder structures and DbContext)
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Data/AppDbContext.cs
- Directory structures under Application/Services and API/Controllers.
