# 11_API_ENDPOINTS_MAP

> **Purpose:** Comprehensive index of all API endpoints across the ERP system. Used as an explicit routing and policy layout map for architectural validation and AI agent reference.

---

## 1. API Endpoints Summary Dashboard

| Module | Sub-Domain / System Namespace | Total Endpoints |
| :--- | :--- | :--- |
| **Core** | Multi-Tenancy, Companies, Modules & Users Management | 16 |
| **HR** | Attendance, Departments, Employees, Job Positions, Leave, Payroll | 43 |
| **Products** | Categories, Product Catalog, Units of Measure | 15 |
| **Sales** | Customers, Deliveries, Invoices, Receipts, Returns | 30 |
| **Contacts** | Directory / Address Book Management | 5 |
| **Expenses** | Item Tracking, Categories, Financial Statistics | 14 |
| **Inventory** | Stock Movements, Warehouse Isolation, Reports & Valuation | 16 |
| **CRM** | Sales Pipeline, Lead Capture & Conversion Management | 12 |
| **Security / Access** | System Permissions Verification & Role Assignments Scoping | 8 |
| **TOTAL** | **Active System Routes** | **159** |

---

## 2. API Endpoints Breakdown By Module

### 2.1 Core Module
* Foundations of client-onboarding, licensing validation, profile configuration, and core identity allocation.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | POST | `/api/Auth/register-owner` | `RegisterOwner` | `AllowAnonymous` | `RegisterOwnerRequest` | `AuthResponse` |
| 2 | POST | `/api/Auth/login` | `Login` | `AllowAnonymous` | `LoginRequest` | `AuthResponse` |
| 3 | GET | `/api/companies/me` | `GetMyCompany` | `Permissions.Core.Companie.Read` | None | `CompanyMeDto` |
| 4 | PUT | `/api/companies/me` | `UpdateMyCompany` | `Permissions.Core.Companie.Update` | `UpdateCompanyMeDto` | `CompanyMeDto` |
| 5 | GET | `/api/company-modules` | `GetAll` | `Permissions.Core.Modules.Read` | None | `IEnumerable<CompanyModuleDto>` |
| 6 | PUT | `/api/company-modules/{moduleId}` | `Toggle` | `Permissions.Core.Modules.Manage` | `ToggleCompanyModuleDto` | `CompanyModuleDto` |
| 7 | GET | `/api/company-users` | `GetAll` | `Permissions.Core.Users.Read` | None | `IEnumerable<CompanyUserDto>` |
| 8 | POST | `/api/company-users` | `Create` | `Permissions.Core.Users.Create` | `CreateCompanyUserDto` | `CompanyUserDto` |
| 9 | PUT | `/api/company-users/{userId:guid}/status` | `UpdateStatus` | `Permissions.Core.Users.Update` | `UpdateUserStatusDto` | `CompanyUserDto` |
| 10 | POST | `/api/company-users/{userId:guid}/roles/assign` | `AssignRole` | `Permissions.Core.Users.Update` | `UserRoleAssignmentRequest` | `CompanyUserDto` |
| 11 | POST | `/api/company-users/{userId:guid}/roles/remove` | `RemoveRole` | `Permissions.Core.Users.Update` | `UserRoleRemovalRequest` | `CompanyUserDto` |
| 12 | PUT | `/api/company-users/{userId:guid}/profile` | `UpdateProfile` | `Permissions.Core.Users.Update` | `AdminUpdateUserProfileDto` | `CompanyUserDto` |
| 13 | DELETE | `/api/company-users/{userId:guid}` | `Delete` | `Permissions.Core.Users.Delete` | None | `NoContent` |
| 14 | GET | `/api/modules` | `GetAll` | None | None | `IActionResult` |
| 15 | GET | `/api/my-account` | `GetProfile` | `[Authorize]` (Default) | None | `MyAccountDto` |
| 16 | PUT | `/api/my-account/password` | `ChangePassword` | `[Authorize]` (Default) | `ChangePasswordRequest` | `NoContent` |

### 2.2 HR Module
* Complete management of corporate human resources, organizational departments, job allocations, tracking, and payroll runs.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | POST | `/api/Attendance/checkin` | `CheckIn` | `[Authorize]` (Default) | `CheckInDto` | `AttendanceDto` |
| 2 | POST | `/api/Attendance/checkout` | `CheckOut` | `[Authorize]` (Default) | `CheckOutDto` | `AttendanceDto` |
| 3 | POST | `/api/Attendance/manual` | `CreateManual` | `[Authorize]` (Default) | `ManualAttendanceDto` | `AttendanceDto` |
| 4 | PUT | `/api/Attendance/{id}` | `Update` | `[Authorize]` (Default) | `UpdateAttendanceDto` | `AttendanceDto` |
| 5 | GET | `/api/Attendance/summary/{employeeId}/{month}/{year}` | `GetSummary` | `[Authorize]` (Default) | None | `AttendanceSummaryDto` |
| 6 | GET | `/api/Department` | `GetAll` | `[Authorize]` (Default) | None | `IEnumerable<DepartmentDto>` |
| 7 | GET | `/api/Department/{id:guid}` | `GetById` | `[Authorize]` (Default) | None | `DepartmentDetailDto` |
| 8 | POST | `/api/Department` | `Create` | `[Authorize]` (Default) | `CreateDepartmentDto` | `DepartmentDto` |
| 9 | PUT | `/api/Department/{id:guid}` | `Update` | `[Authorize]` (Default) | `UpdateDepartmentDto` | `DepartmentDto` |
| 10 | DELETE | `/api/Department/{id:guid}` | `Delete` | `[Authorize]` (Default) | None | `NoContent` |
| 11 | GET | `/api/Employee` | `GetAll` | `[Authorize]` (Default) | None | `IEnumerable<EmployeeListDto>` |
| 12 | GET | `/api/Employee/{id}` | `GetById` | `[Authorize]` (Default) | None | `EmployeeDetailDto` |
| 13 | GET | `/api/Employee/department/{departmentId}` | `GetByDepartment` | `[Authorize]` (Default) | None | `IEnumerable<EmployeeListDto>` |
| 14 | GET | `/api/Employee/status/{status}` | `GetByStatus` | `[Authorize]` (Default) | None | `IEnumerable<EmployeeListDto>` |
| 15 | POST | `/api/Employee` | `Create` | `[Authorize]` (Default) | `CreateEmployeeDto` | `EmployeeDetailDto` |
| 16 | PUT | `/api/Employee/{id}` | `Update` | `[Authorize]` (Default) | `UpdateEmployeeDto` | `EmployeeDetailDto` |
| 17 | PUT | `/api/Employee/{id}/status` | `UpdateStatus` | `[Authorize]` (Default) | `UpdateEmployeeDto` | `NoContent` |
| 18 | DELETE | `/api/Employee/{id}` | `Delete` | `[Authorize]` (Default) | None | `NoContent` |
| 19 | GET | `/api/JobPosition` | `GetAll` | `[Authorize]` (Default) | None | `IEnumerable<PositionDto>` |
| 20 | GET | `/api/JobPosition/{id}` | `GetById` | `[Authorize]` (Default) | None | `PositionDto` |
| 21 | POST | `/api/JobPosition` | `Create` | `[Authorize]` (Default) | `CreatePositionDto` | `PositionDto` |
| 22 | PUT | `/api/JobPosition/{id}` | `Update` | `[Authorize]` (Default) | `UpdatePositionDto` | `PositionDto` |
| 23 | DELETE | `/api/JobPosition/{id}` | `Delete` | `[Authorize]` (Default) | None | `NoContent` |
| 24 | POST | `/api/LeaveRequest` | `Create` | `[Authorize]` (Default) | `CreateLeaveRequestDto` | `LeaveRequestDto` |
| 25 | GET | `/api/LeaveRequest/{id}` | `GetById` | `[Authorize]` (Default) | None | `LeaveRequestDetailDto` |
| 26 | GET | `/api/LeaveRequest/employee/{employeeId}` | `GetByEmployee` | `[Authorize]` (Default) | None | `IEnumerable<LeaveRequestDto>` |
| 27 | GET | `/api/LeaveRequest/pending` | `GetPending` | `[Authorize]` (Default) | None | `IEnumerable<LeaveRequestDto>` |
| 28 | POST | `/api/LeaveRequest/{id}/approve` | `Approve` | `[Authorize]` (Default) | `ApproveLeaveDto` | `NoContent` |
| 29 | POST | `/api/LeaveRequest/{id}/reject` | `Reject` | `[Authorize]` (Default) | `RejectLeaveDto` | `NoContent` |
| 30 | POST | `/api/LeaveRequest/{id}/cancel` | `Cancel` | `[Authorize]` (Default) | `CancelLeaveDto` | `NoContent` |
| 31 | GET | `/api/LeaveRequest/balance/{employeeId}/{year}` | `GetBalance` | `[Authorize]` (Default) | None | `LeaveBalanceDto` |
| 32 | DELETE | `/api/LeaveRequest/{id}` | `Delete` | `[Authorize]` (Default) | None | `NoContent` |
| 33 | POST | `/api/Payroll/generate` | `Generate` | `[Authorize]` (Default) | `GeneratePayrollDto` | `PayrollBatchDto` |
| 34 | GET | `/api/Payroll/{id}` | `GetById` | `[Authorize]` (Default) | None | `PayrollDetailDto` |
| 35 | GET | `/api/Payroll/employee/{employeeId}` | `GetByEmployee` | `[Authorize]` (Default) | None | `IEnumerable<PayrollDto>` |
| 36 | GET | `/api/Payroll/period/{month}/{year}` | `GetByPeriod` | `[Authorize]` (Default) | None | `IEnumerable<PayrollDto>` |
| 37 | PUT | `/api/Payroll/{id}` | `Update` | `[Authorize]` (Default) | `UpdatePayrollDto` | `PayrollDetailDto` |
| 38 | POST | `/api/Payroll/{id}/process` | `Process` | `[Authorize]` (Default) | None | `NoContent` |
| 39 | POST | `/api/Payroll/{id}/mark-paid` | `MarkPaid` | `[Authorize]` (Default) | `MarkPaidDto` | `NoContent` |
| 40 | POST | `/api/Payroll/{id}/revert` | `RevertToDraft` | `[Authorize]` (Default) | None | `NoContent` |
| 41 | DELETE | `/api/Payroll/{id}` | `Delete` | `[Authorize]` (Default) | None | `NoContent` |
| 42 | GET | `/api/Payroll/summary/{month}/{year}` | `GetSummary` | `[Authorize]` (Default) | None | `PayrollSummaryDto` |
| 43 | POST | `/api/Payroll/{id}/recalculate` | `Recalculate` | `[Authorize]` (Default) | None | `PayrollDetailDto` |

### 2.3 Products Module
* Catalog control, category nesting hierarchies, and global Unit of Measure (UoM) dictionaries.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/Category` | `GetAll` | `Permissions.Products.Categories.Read` | None | `IReadOnlyList<CategoryDto>` |
| 2 | GET | `/api/Category/{id}` | `GetById` | `Permissions.Products.Categories.Read` | None | `CategoryDto` |
| 3 | POST | `/api/Category` | `Create` | `Permissions.Products.Categories.Manage` | `CreateCategoryRequest` | `CategoryDto` |
| 4 | PUT | `/api/Category/{id}` | `Update` | `Permissions.Products.Categories.Manage` | `UpdateCategoryRequest` | `CategoryDto` |
| 5 | DELETE | `/api/Category/{id}` | `Delete` | `Permissions.Products.Categories.Manage` | None | `NoContent` |
| 6 | GET | `/api/Products` | `GetAll` | `Permissions.Products.Product.Read` | None | `IActionResult` |
| 7 | GET | `/api/Products/{id}` | `GetById` | `Permissions.Products.Product.Read` | None | `IActionResult` |
| 8 | POST | `/api/Products` | `Create` | `Permissions.Products.Product.Manage` | `CreateProductDto` | `IActionResult` |
| 9 | PUT | `/api/Products/{id}` | `Update` | `Permissions.Products.Product.Manage` | `UpdateProductDto` | `IActionResult` |
| 10 | DELETE | `/api/Products/{id}` | `Delete` | `Permissions.Products.Product.Manage` | None | `IActionResult` |
| 11 | GET | `/api/UnitOfMeasure` | `GetAll` | `Permissions.Products.UnitOfMeasures.Read` | None | `IActionResult` |
| 12 | GET | `/api/UnitOfMeasure/{id}` | `GetById` | `Permissions.Products.UnitOfMeasures.Read` | None | `IActionResult` |
| 13 | POST | `/api/UnitOfMeasure` | `Create` | `Permissions.Products.UnitOfMeasures.Manage` | `CreateUnitOfMeasureRequest` | `IActionResult` |
| 14 | PUT | `/api/UnitOfMeasure/{id}` | `Update` | `Permissions.Products.UnitOfMeasures.Manage` | `UpdateUnitOfMeasureRequest` | `IActionResult` |
| 15 | DELETE | `/api/UnitOfMeasure/{id}` | `Delete` | `Permissions.Products.UnitOfMeasures.Manage` | None | `IActionResult` |

### 2.4 Sales Module
* Full transaction pipelines tracking clients, item ordering, invoice allocation, receipt capture, and processing sales returns.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/sales/Customers` | `GetAll` | `Permissions.Sales.Customers.Read` | `[FromQuery] isActive` | `IEnumerable<CustomerDto>` |
| 2 | GET | `/api/sales/Customers/{id}` | `GetById` | `Permissions.Sales.Customers.Read` | None | `CustomerDto` |
| 3 | POST | `/api/sales/Customers` | `Create` | `Permissions.Sales.Customers.Manage` | `CreateCustomerRequest` | `CustomerDto` |
| 4 | PUT | `/api/sales/Customers/{id}` | `Update` | `Permissions.Sales.Customers.Manage` | `UpdateCustomerRequest` | `CustomerDto` |
| 5 | DELETE | `/api/sales/Customers/{id}` | `Delete` | `Permissions.Sales.Customers.Manage` | None | `NoContent` |
| 6 | GET | `/api/sales/deliveries` | `GetAll` | `Permissions.Sales.Deliveries.Read` | `[FromQuery] invId, status...` | `IActionResult` |
| 7 | GET | `/api/sales/deliveries/{id}` | `GetById` | `Permissions.Sales.Deliveries.Read` | None | `IActionResult` |
| 8 | POST | `/api/sales/deliveries` | `Create` | `Permissions.Sales.Deliveries.Access` | `CreateSalesDeliveryRequest` | `IActionResult` |
| 9 | POST | `/api/sales/deliveries/{id}/post` | `Post` | `Permissions.Sales.Deliveries.Manage` | None | `IActionResult` |
| 10 | POST | `/api/sales/deliveries/{id}/cancel` | `Cancel` | `Permissions.Sales.Deliveries.Access` | None | `IActionResult` |
| 11 | DELETE | `/api/sales/deliveries/{id}` | `Delete` | `Permissions.Sales.Deliveries.Manage` | None | `IActionResult` |
| 12 | GET | `/api/sales/invoices` | `GetAll` | `Permissions.Sales.Invoices.Read` | `[FromQuery] custId, status...` | `IActionResult` |
| 13 | GET | `/api/sales/invoices/{id}` | `GetById` | `Permissions.Sales.Invoices.Read` | None | `IActionResult` |
| 14 | POST | `/api/sales/invoices` | `Create` | `Permissions.Sales.Invoices.Access` | `CreateSalesInvoiceRequest` | `IActionResult` |
| 15 | PUT | `/api/sales/invoices/{id}` | `Update` | `Permissions.Sales.Invoices.Access` | `UpdateSalesInvoiceRequest` | `IActionResult` |
| 16 | POST | `/api/sales/invoices/{id}/post` | `Post` | `Permissions.Sales.Invoices.Manage` | None | `IActionResult` |
| 17 | POST | `/api/sales/invoices/{id}/cancel` | `Cancel` | `Permissions.Sales.Invoices.Manage` | None | `IActionResult` |
| 18 | DELETE | `/api/sales/invoices/{id}` | `Delete` | `Permissions.Sales.Invoices.Manage` | None | `IActionResult` |
| 19 | GET | `/api/sales/receipts` | `GetAll` | `Permissions.Sales.Receipts.Read` | `[FromQuery] custId, status...` | `IActionResult` |
| 20 | GET | `/api/sales/receipts/{id}` | `GetById` | `Permissions.Sales.Receipts.Read` | None | `IActionResult` |
| 21 | POST | `/api/sales/receipts` | `Create` | `Permissions.Sales.Receipts.Access` | `CreateSalesReceiptRequest` | `IActionResult` |
| 22 | POST | `/api/sales/receipts/{id}/post` | `Post` | `Permissions.Sales.Receipts.Manage` | None | `IActionResult` |
| 23 | POST | `/api/sales/receipts/{id}/cancel` | `Cancel` | `Permissions.Sales.Receipts.Manage` | None | `IActionResult` |
| 24 | DELETE | `/api/sales/receipts/{id}` | `Delete` | `Permissions.Sales.Receipts.Manage` | None | `IActionResult` |
| 25 | GET | `/api/sales/returns` | `GetAll` | `Permissions.Sales.Returns.Read` | `[FromQuery] custId, status...` | `IActionResult` |
| 26 | GET | `/api/sales/returns/{id}` | `GetById` | `Permissions.Sales.Returns.Read` | None | `IActionResult` |
| 27 | POST | `/api/sales/returns` | `Create` | `Permissions.Sales.Returns.Access` | `CreateSalesReturnRequest` | `IActionResult` |
| 28 | POST | `/api/sales/returns/{id}/post` | `Post` | `Permissions.Sales.Returns.Manage` | None | `IActionResult` |
| 29 | POST | `/api/sales/returns/{id}/cancel` | `Cancel` | `Permissions.Sales.Returns.Access` | None | `IActionResult` |
| 30 | DELETE | `/api/sales/returns/{id}` | `Delete` | `Permissions.Sales.Returns.Manage` | None | `IActionResult` |

### 2.5 Contacts Module
* Lightweight centralized directory / corporate contact listings.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/Contact` | `GetAll` | `Permissions.Contacts.Contact.Read` | None | `IEnumerable<ContactDto>` |
| 2 | GET | `/api/Contact/{id}` | `GetContact` | `Permissions.Contacts.Contact.Read` | None | `ContactDto` |
| 3 | POST | `/api/Contact` | `CreateAsync` | `Permissions.Contacts.Contact.Manage` | `CreateContactRequest` | `ContactDto` |
| 4 | PUT | `/api/Contact` | `UpdateAsync` | `Permissions.Contacts.Contact.Manage` | `UpdateContactDto` | `ContactDto` |
| 5 | DELETE | `/api/Contact/{id}` | `DeleteAAsync` | `Permissions.Contacts.Contact.Manage` | None | `NoContent` |

### 2.6 Expenses Module
* Operational cash expenditure tracking, category classifications, and operational analysis dashboards.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/expenses` | `GetAll` | `Permissions.Expenses.Items.Read` | `[FromQuery] ExpenseQuery` | `IActionResult` |
| 2 | GET | `/api/expenses/{id}` | `GetById` | `Permissions.Expenses.Items.Read` | None | `IActionResult` |
| 3 | POST | `/api/expenses` | `Create` | `Permissions.Expenses.Items.Manage` | `CreateExpenseDto` | `IActionResult` |
| 4 | PUT | `/api/expenses/{id}` | `Update` | `Permissions.Expenses.Items.Manage` | `UpdateExpenseDto` | `IActionResult` |
| 5 | PATCH | `/api/expenses/{id}/status` | `UpdateStatus` | `Permissions.Expenses.Items.Manage` | `UpdateExpenseStatusDto` | `IActionResult` |
| 6 | DELETE | `/api/expenses/{id}` | `Delete` | `Permissions.Expenses.Items.Manage` | None | `IActionResult` |
| 7 | GET | `/api/expenses/stats/summary` | `GetSummary` | `Permissions.Expenses.Stats.Read` | `[FromQuery] StatsQuery` | `IActionResult` |
| 8 | GET | `/api/expenses/stats/over-time` | `GetOverTime` | `Permissions.Expenses.Stats.Read` | `[FromQuery] StatsQuery` | `IActionResult` |
| 9 | GET | `/api/expenses/stats/by-category` | `GetByCategory` | `Permissions.Expenses.Stats.Read` | `[FromQuery] StatsQuery` | `IActionResult` |
| 10 | GET | `/api/expense-categories` | `GetAll` | `Permissions.Expenses.Items.Read` | None | `IActionResult` |
| 11 | GET | `/api/expense-categories/{id}` | `GetById` | `Permissions.Expenses.Items.Read` | None | `IActionResult` |
| 12 | POST | `/api/expense-categories` | `Create` | `Permissions.Expenses.Items.Manage` | `CreateExpenseCategoryDto` | `IActionResult` |
| 13 | PUT | `/api/expense-categories/{id}` | `Update` | `Permissions.Expenses.Items.Manage` | `UpdateExpenseCategoryDto` | `IActionResult` |
| 14 | DELETE | `/api/expense-categories/{id}` | `Delete` | `Permissions.Expenses.Items.Manage` | None | `IActionResult` |

### 2.7 Inventory Module
* Warehouse allocations, inventory stock adjustments, internal transfers, and financial valuation metrics.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | POST | `/api/Inventory/inStockIn` | `StockIn` | `Permissions.Inventory.Stock.Manage` | `StockInRequest` | `IActionResult` |
| 2 | POST | `/api/Inventory/outStockOut` | `StockOut` | `Permissions.Inventory.Stock.Manage` | `StockOutRequest` | `IActionResult` |
| 3 | POST | `/api/Inventory/transfer` | `Transfer` | `Permissions.Inventory.Stock.Manage` | `StockTransferRequest` | `IActionResult` |
| 4 | POST | `/api/Inventory/opening-balance` | `OpeningBalance` | `Permissions.Inventory.Stock.Manage` | `OpeningBalanceRequest` | `IActionResult` |
| 5 | POST | `/api/Inventory/adjustment` | `Adjustment` | `Permissions.Inventory.Stock.Manage` | `StockAdjustmentRequest` | `IActionResult` |
| 6 | GET | `/api/InventoryReports/stock-balance` | `GetStockBalance` | `Permissions.Inventory.Reports.Read` | `[FromQuery] prodId, whId...` | `IActionResult` |
| 7 | GET | `/api/InventoryReports/warehouse/{warehouseId}/stock` | `GetWarehouseStock` | `Permissions.Inventory.Reports.Read` | None | `IActionResult` |
| 8 | GET | `/api/InventoryReports/product/{productId}/stock` | `GetProductStock` | `Permissions.Inventory.Reports.Read` | None | `IActionResult` |
| 9 | GET | `/api/InventoryReports/movements` | `GetMovements` | `Permissions.Inventory.Reports.Read` | `[FromQuery] prodId, whId...` | `IActionResult` |
| 10 | GET | `/api/InventoryReports/low-stock` | `GetLowStock` | `Permissions.Inventory.Reports.Read` | `[FromQuery] warehouseId` | `IActionResult` |
| 11 | GET | `/api/InventoryReports/valuation` | `GetValuation` | `Permissions.Inventory.Reports.Read` | `[FromQuery] warehouseId` | `IActionResult` |
| 12 | GET | `/api/Warehouses` | `GetAll` | `Permissions.Inventory.Warehouses.Read` | `[FromQuery] branchId` | `IActionResult` |
| 13 | GET | `/api/Warehouses/{id:int}` | `GetById` | `Permissions.Inventory.Warehouses.Read` | None | `IActionResult` |
| 14 | POST | `/api/Warehouses` | `Create` | `Permissions.Inventory.Warehouses.Manage` | `CreateWarehouseDto` | `IActionResult` |
| 15 | PUT | `/api/Warehouses/{id:int}` | `Update` | `Permissions.Inventory.Warehouses.Manage` | `UpdateWarehouseDto` | `IActionResult` |
| 16 | DELETE | `/api/Warehouses/{id:int}` | `Delete` | `Permissions.Inventory.Warehouses.Manage` | None | `IActionResult` |

### 2.8 CRM Module
* Lead capture pipelines, interaction logging stages, tracking deal states, and converting prospects to accounts.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/Lead` | `GetAll` | `[Authorize]` (Default) | None | `List<LeadDto>` |
| 2 | GET | `/api/Lead/{id}` | `GetById` | `[Authorize]` (Default) | None | `LeadDto` |
| 3 | POST | `/api/Lead` | `Create` | `[Authorize]` (Default) | `CreateLeadDto` | `IActionResult` |
| 4 | PUT | `/api/Lead/{id}` | `Update` | `[Authorize]` (Default) | `UpdateLeadDto` | `IActionResult` |
| 5 | DELETE | `/api/Lead/{id}` | `Delete` | `[Authorize]` (Default) | None | `IActionResult` |
| 6 | POST | `/api/Lead/{id}/convert` | `Convert` | `[Authorize]` (Default) | `ConvertLeadDto` | `IActionResult` |
| 7 | GET | `/api/Pipeline` | `GetAll` | `[Authorize]` (Default) | None | `List<PipelineDto>` |
| 8 | GET | `/api/Pipeline/{id}` | `GetById` | `[Authorize]` (Default) | None | `PipelineDto` |
| 9 | POST | `/api/Pipeline` | `Create` | `[Authorize]` (Default) | `CreatePipelineDto` | `object` |
| 10 | PUT | `/api/Pipeline/{id}` | `Update` | `[Authorize]` (Default) | `UpdatePipelineDto` | `IActionResult` |
| 11 | DELETE | `/api/Pipeline/{id}` | `Delete` | `[Authorize]` (Default) | None | `IActionResult` |
| 12 | PATCH | `/api/Pipeline/{id}/move-stage` | `MoveStage` | `[Authorize]` (Default) | `MovePiplineStageDto` | `IActionResult` |

### 2.9 Security / Access Module
* Granular policy controls, system permissions listing reflection, custom role bindings, and user privilege adjustments.

| # | HTTP Verb | Full Route | Controller Action | Auth Policy / Permission | Request Type (DTO) | Response Type |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| 1 | GET | `/api/access/permissions` | `GetAllPermissions` | `[Authorize]` (Default) | None | `IEnumerable<PermissionGroupDto>` |
| 2 | GET | `/api/access/roles` | `GetRoles` | `Permissions.Security.Roles.Manage` | None | `IEnumerable<RoleDto>` |
| 3 | POST | `/api/access/roles` | `CreateRole` | `Permissions.Security.Roles.Manage` | `CreateRoleRequest` | `RoleDto` |
| 4 | PUT | `/api/access/roles/{roleName}` | `UpdateRolePermissions` | `Permissions.Security.Roles.Manage` | `UpdateRolePermissionsRequest` | `RoleDto` |
| 5 | DELETE | `/api/access/roles/{roleName}` | `DeleteRolePermissions` | `Permissions.Security.Roles.Manage` | None | `IActionResult` |
| 6 | POST | `/api/access/user-roles/assign` | `AssignRole` | `Permissions.Security.Roles.Manage` | `AssignRoleRequest` | `IActionResult` |
| 7 | POST | `/api/access/user-roles/remove` | `RemoveRole` | `Permissions.Security.Roles.Manage` | `AssignRoleRequest` | `IActionResult` |
| 8 | GET | `/api/access/user-roles/{userId:guid}` | `GetUserRoles` | `Permissions.Security.Roles.Manage` | None | `UserRolesDto` |

---
**Document Version:** 1.0  
**Status:** Unified Endpoint Index Mapped Successfully