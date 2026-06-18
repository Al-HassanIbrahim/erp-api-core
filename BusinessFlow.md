# Business Flow Map

## Overview
Documents the core business processes and cross-module interactions within the ERP.

---
## Responsibilities
Explains how data flows from one state to another across different modules (e.g., CRM to Sales, Sales to Inventory).

---
## Workflow

### 1. Tenant Provisioning
1. User calls `/api/auth/register-owner`.
2. `AuthService.RegisterOwnerAsync` creates a `Company`.
3. Default roles are seeded (`CompanyRoleSeeder`).
4. An `ApplicationUser` is created and assigned the Owner role.
5. Modules are seeded (`DbSeeder.SeedModulesAsync`).

### 2. CRM to Sales Flow (Inferred)
1. A `Lead` is created in CRM.
2. As the pipeline progresses, the `Lead` is converted to a `Customer`.
3. `Lead.ConvertedCustomerId` is populated, linking CRM to Sales.

### 3. Sales to Inventory Flow (Inferred)
1. `SalesInvoice` is created for a `Customer`.
2. `SalesDelivery` handles the physical movement of goods.
3. `StockItem.QuantityOnHand` is updated accordingly.

### 4. HR Payroll Flow (Inferred)
1. `Employee` attendance and `LeaveRequest` data is recorded.
2. `Payroll` is generated monthly.
3. `PayrollLineItem` records calculate gross and net pay.

---
## Dependencies
- EF Core Transactions to ensure consistency across flows.

---
## Public APIs
N/A

---
## Limitations
- Specific integration points (like Sales decreasing Inventory) might be tightly coupled rather than event-driven.

---
## Technical Debt
- Lack of Domain Events for cross-module communication (e.g., using MediatR to decoupled Sales and Inventory).

---
## AI Quick Context
### Depends On
AppDbContext
### Uses
AuthService, Lead, Pipeline, SalesInvoice, StockItem
### Used By
Future AI to understand business context.
### Related Modules
CRM, Sales, Inventory, HR, Core
### Shared Components
AuthService
### Entities
Lead, Customer, SalesInvoice, Product, InventoryDocument
### Business Rules
- Tenants are completely isolated.
- Leads convert to Customers.
### Endpoints
`/api/auth/register-owner`
### Future Refactoring
Implement Domain Events (MediatR) for cross-module triggers.
### Modification Risk
Low
### Confidence
High (Tenant Provisioning), Inferred (Medium for exact data flows)
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Identity/AuthService.cs
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Data/AppDbContext.cs
