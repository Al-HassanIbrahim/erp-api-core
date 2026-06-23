# 00_PROJECT_OVERVIEW.md — ERP System API: System-Wide Architecture Reference

> **Purpose:** This file is the permanent AI-context foundation for every session. It describes the full system skeleton — contracts, conventions, and architecture — so any AI can write code, fix bugs, or add features without opening raw source files. Business-module logic (HR, Sales, etc.) is covered in their own separate context files.

---

## 1. Tech Stack

| Technology | Version | Purpose |
|---|---|---|
| .NET | 8.0 (`net8.0`) | Runtime and SDK target |
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.0.0 | ORM for database access |
| EF Core SQL Server Provider | 8.0.0 | SQL Server database driver |
| EF Core Design/Tools | 8.0.0 | Migration scaffolding (build-time only) |
| ASP.NET Core Identity | 8.0.19–22 | User, role, and claims management |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.23 | JWT token validation middleware |
| Microsoft.AspNetCore.Authorization | 8.0.18 | Policy-based authorization engine |
| Microsoft.AspNetCore.Http.Abstractions | 2.3.9 | HttpContext access in Infrastructure |
| Swashbuckle.AspNetCore | 6.6.2 | Swagger / OpenAPI UI and spec generation |
| SQL Server | (server-side) | Primary relational database (LocalDB/full) |

> All four projects target `net8.0` with `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`.

---

## 2. Solution Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    ERPSyatem.API  (⚠️ typo)                      │
│   Controllers · Middleware · Extensions · Program.cs            │
│   Namespace: ERPSyatem.API                                      │
└───────────────────────┬────────────────────┬────────────────────┘
                        │ depends on         │ depends on
          ┌─────────────▼──────────┐  ┌──────▼──────────────────┐
          │  ERPSystem.Application  │  │ ERPSystem.Infrastructure │
          │  Services · Interfaces  │  │ Repositories · Identity  │
          │  DTOs · Auth · Except.  │  │ AppDbContext · Seeders   │
          └─────────────┬──────────┘  └──────┬────────────────────┘
                        │ depends on         │ depends on
                        └──────────┬─────────┘
                    ┌──────────────▼───────────────┐
                    │      ERPSystem.Domain          │
                    │  Entities · Abstractions       │
                    │  (Interfaces) · Enums          │
                    └──────────────────────────────┘

Dependency Rule:  Domain ← Application ← Infrastructure ← API
                  (arrows point toward what a layer KNOWS ABOUT)
```

| Layer | Responsibility |
|---|---|
| **ERPSystem.Domain** | Pure domain — entities, repository interfaces (`I*Repository`), base classes, enums. No framework dependencies. |
| **ERPSystem.Application** | Business logic — service interfaces (`I*Service`), service implementations, DTOs, exceptions (`BusinessException`), authorization constants (`Permissions`). |
| **ERPSystem.Infrastructure** | Data access — `AppDbContext`, concrete repository implementations, EF migrations, ASP.NET Identity (`ApplicationUser`), JWT utilities, seeders. |
| **ERPSyatem.API** | HTTP surface — ASP.NET Controllers, middleware, extension methods, `Program.cs` wiring. |

> ⚠️ **Typo alert:** The presentation project and its namespace are spelled `ERPSyatem` (missing a 't') instead of `ERPSystem`. All four correct projects use `ERPSystem.*`; only the API project uses the typo. Do **not** "fix" this in new code — the namespace must match the existing project or it will break compilation.

---

## 3. Project & File Naming Conventions

| Artifact | Naming Pattern | Example | Location (Layer) |
|---|---|---|---|
| Domain Entity | `{Name}` (PascalCase) | `SalesInvoice`, `Employee` | `ERPSystem.Domain.Entities.{Module}` |
| Base Entity | `BaseEntity`, `AuditableEntity` | — | `ERPSystem.Domain.Abstractions` |
| Company-scoped marker | `ICompanyEntity` (interface) | — | `ERPSystem.Domain.Abstractions` |
| Repository Interface | `I{Name}Repository` | `ISalesInvoiceRepository` | `ERPSystem.Domain.Abstractions` |
| Repository Implementation | `{Name}Repository` | `SalesInvoiceRepository` | `ERPSystem.Infrastructure.Repositories.{Module}` |
| Service Interface | `I{Name}Service` | `ISalesInvoiceService` | `ERPSystem.Application.Interfaces` |
| Service Implementation | `{Name}Service` | `SalesInvoiceService` | `ERPSystem.Application.Services.{Module}` |
| Controller | `{Name}Controller` | `SalesInvoiceController` | `ERPSyatem.API.Controllers` |
| DTO (request) | `{Action}{Name}Dto` or `Create{Name}Request` | `CreateInvoiceRequest` | `ERPSystem.Application.DTOs.{Module}` (⚠️ DTOs folder exists; specific pattern unconfirmed from provided files) |
| Middleware | `{Name}Middleware` | `ExceptionHandlingMiddleware` | `ERPSyatem.API.Middleware` |
| Extension class | `{Name}Extensions` | `PermissionPolicyExtensions` | `ERPSyatem.API.Extensions` |
| Enum | `{DescriptiveName}` | `SalesInvoiceStatus`, `LeaveType` | `ERPSystem.Domain.Enums` |
| Authorization constants | `Permissions` (nested static classes) | `Permissions.Sales.Invoices.Read` | `ERPSystem.Application.Authorization` |
| Custom exception | `{Domain}Exception` | `BusinessException` | `ERPSystem.Application.Exceptions` |
| DB Context | `AppDbContext` | — | `ERPSystem.Infrastructure.Data` |
| DB Seeder | `DbSeeder` (static) | — | `ERPSystem.Infrastructure.Data` |
| Role/Company seeder | `CompanyRoleSeeder` (static) | — | `ERPSystem.Infrastructure.Identity` |
| Identity user | `ApplicationUser` | — | `ERPSystem.Infrastructure.Identity` |
| Migrations | EF Core default naming | `20240101_InitialCreate` | `ERPSystem.Infrastructure.Migrations` (⚠️ directory inferred; migrations not provided) |

---

## 4. Dependency Injection Registration Summary

**Lifetime convention:** All application services and repositories are registered as **`AddScoped`** (per HTTP request).

### Module Registration Counts (`Program.cs`)

| Module | Repositories (AddScoped) | Services (AddScoped) |
|---|---|---|
| Core | 3 (`Company`, `Module`, `CompanyModule`) | 5 (`CompanyProfile`, `Module`, `CompanyModule`, `CompanyUser`, `MyAccount`) + 1 (`ModuleAccess`) |
| Products | 3 (`Product`, `UnitOfMeasure`, `Category`) | 3 (`Product`, `UnitOfMeasure`, `Category`) |
| Inventory | 3 (`Warehouse`, `Inventory`, `InventoryReports`) | 3 (`Warehouse`, `Inventory`, `InventoryReports`) |
| HR | 7 (`Employee`, `Department`, `Position`, `Attendance`, `LeaveRequest`, `LeaveBalance`, `Payroll`) | 4 (`Employee`, `Attendance`, `LeaveRequest`, `Payroll`) |
| Sales | 5 (`Customer`, `SalesInvoice`, `SalesDelivery`, `SalesReceipt`, `SalesReturn`) | 5 (`Customer`, `SalesInvoice`, `SalesDelivery`, `SalesReceipt`, `SalesReturn`) |
| Contact | 1 (`Contact`) | 1 (`Contact`) |
| Expenses | 2 (`Expense`, `ExpenseCategory`) | 3 (`Expense`, `ExpenseCategory`, `ExpenseStats`) |
| CRM | 2 (`Lead`, `Pipeline`) | 2 (`Lead`, `Pipeline`) |

### Special Registrations

| Registration | Lifetime | Notes |
|---|---|---|
| `AppDbContext` | Scoped (EF default) | Via `AddDbContext<AppDbContext>` |
| `AddIdentity<ApplicationUser, IdentityRole<Guid>>()` | Framework-managed | Adds UserManager, RoleManager, SignInManager, etc. as Scoped |
| `AddEntityFrameworkStores<AppDbContext>()` | — | Wires Identity to EF Core |
| `AddDefaultTokenProviders()` | — | Enables email/phone/2FA token generation |
| `IJwtTokenService` / `JwtTokenService` | Scoped | Custom JWT creation |
| `IAuthService` / `AuthService` | Scoped | Login/register logic |
| `IHttpContextAccessor` | Singleton (framework behavior) | Via `AddHttpContextAccessor()` |
| `ICurrentUserService` / `CurrentUserService` | Scoped | Reads current user from `HttpContext` |
| `IAuthorizationHandler` / `PermissionHandler` | **Singleton** | Registered inside `AddPermissionPolicies()` |
| Swagger / OpenAPI | Singleton (framework) | Via `AddSwaggerGen(...)` with Bearer definition |
| CORS policy `"MyPolicy"` | Singleton (framework) | Via `AddCors(...)` |

---

## 5. Startup & Initialization Sequence

```
Program.Main(args)
│
├── 1.  WebApplication.CreateBuilder(args)
│
├── 2.  AddDbContext<AppDbContext>  ← SQL Server, "DefaultConnection"
│
├── 3.  AddScoped — Core repos (Company, Module, CompanyModule)
├── 4.  AddScoped — Core services (CompanyProfile, Module, CompanyModule, CompanyUser, MyAccount, ModuleAccess)
│
├── 5.  AddScoped — Products repos (Product, UnitOfMeasure, Category)
├── 6.  AddScoped — Products services (Product, UnitOfMeasure, Category)
│
├── 7.  AddScoped — Inventory repos (Warehouse, Inventory, InventoryReports)
├── 8.  AddScoped — Inventory services (Warehouse, Inventory, InventoryReports)
│
├── 9.  AddScoped — HR repos (Employee, Department, Position, Attendance, LeaveRequest, LeaveBalance, Payroll)
├── 10. AddScoped — HR services (Employee, Attendance, LeaveRequest, Payroll)
│
├── 11. AddControllers()
├── 12. AddEndpointsApiExplorer()
│
├── 13. AddScoped — Sales repos (Customer, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn)
├── 14. AddScoped — Sales services (Customer, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn)
│
├── 15. AddScoped — Contact repo + service
│
├── 16. AddScoped — Expenses repos (Expense, ExpenseCategory)
├── 17. AddScoped — Expenses services (Expense, ExpenseCategory, ExpenseStats)
│
├── 18. AddScoped — CRM repos (Lead, Pipeline)
├── 19. AddScoped — CRM services (Lead, Pipeline)
│
├── 20. AddSwaggerGen(...)  ← with Bearer security definition
│
├── 21. AddIdentity<ApplicationUser, IdentityRole<Guid>>()
│         .AddEntityFrameworkStores<AppDbContext>()
│         .AddDefaultTokenProviders()
│
├── 22. AddScoped<IJwtTokenService, JwtTokenService>()
├── 23. AddScoped<IAuthService, AuthService>()
│
├── 24. AddHttpContextAccessor()
├── 25. AddScoped<ICurrentUserService, CurrentUserService>()
│
├── 26. AddAuthentication(JwtBearer) + AddJwtBearer(...)
│
├── 27. AddPermissionPolicies()  ← reflects Permissions class, registers all policies
│
├── 28. AddCors("MyPolicy")
│
├── 29. var app = builder.Build()        ◄─ DI container locked
│
├── 30. ⚠️ SEEDING (runs synchronously before any middleware):
│         using scope → DbSeeder.SeedModulesAsync(context)
│         (seeds 6 modules: SALES, INVENTORY, CONTACT, EXPENSES, HR, CRM)
│
├── 31. if (Development) → UseSwagger() + UseSwaggerUI()
│
├── 32. app.UseExceptionHandling()       ◄─ MUST be first to catch all errors
│
├── 33. app.UseCors("MyPolicy")
│
├── 34. app.UseAuthentication()
│
├── 35. app.UseAuthorization()
│
├── 36. app.MapControllers()
│
└── 37. app.Run()
```

> ⚠️ **Timing-sensitive:** Seeding (step 30) runs **after** `app.Build()` but **before** all middleware. If seeding throws, the application will crash at startup before serving any requests. The seeder is idempotent (checks existing keys before inserting).

---

## 6. Middleware Pipeline (Execution Order)

| # | Middleware | What It Does | Notes |
|---|---|---|---|
| 1 | **Swagger / SwaggerUI** | Serves OpenAPI spec at `/swagger/v1/swagger.json` and UI at `/swagger` | Development environment only |
| 2 | **ExceptionHandlingMiddleware** | Catches all unhandled exceptions; maps to structured JSON error response | Must remain first real middleware so it wraps all subsequent errors |
| 3 | **CORS (`"MyPolicy"`)** | Adds `Access-Control-*` headers; allows any origin/method/header | ⚠️ Wildcard — not production-safe |
| 4 | **Authentication** | Validates JWT Bearer token; populates `HttpContext.User` | Identity claims (including `permission` claims) available after this |
| 5 | **Authorization** | Enforces `[Authorize]` and permission policies via `PermissionHandler` | Must come after Authentication |
| 6 | **MapControllers** | Routes requests to controller action methods | Terminal middleware |

> ⚠️ **Order sensitivity:** CORS must precede Authentication so that preflight `OPTIONS` requests receive CORS headers even when unauthenticated. ExceptionHandlingMiddleware must be the outermost handler (registered first) to catch exceptions from all subsequent middleware.

---

## 7. Exception Handling System

**Middleware class:** `ExceptionHandlingMiddleware`  
**Location:** `ERPSyatem.API.Middleware`  
**Registration:** `app.UseExceptionHandling()` (extension method on `IApplicationBuilder`)

### Exception → HTTP Status Mapping

| Exception Type | HTTP Status | Code Field | What Gets Logged |
|---|---|---|---|
| `BusinessException` | `ex.HttpStatusCode` (default 400) | `ex.Code` (e.g., `"CUSTOMER_NOT_FOUND"`) | `LogWarning` with code + message |
| `UnauthorizedAccessException` | 403 | `"FORBIDDEN"` | `LogWarning` with message |
| `InvalidOperationException` | 400 | `"INVALID_OPERATION"` | `LogWarning` with message |
| `Exception` (all others) | 500 | `"INTERNAL_ERROR"` | `LogError` with full exception; generic message to client |

### Error Response JSON Shape

```json
{
  "error": {
    "code": "CUSTOMER_NOT_FOUND",
    "message": "Customer not found."
  }
}
```

- `Content-Type: application/json`
- Property names use **camelCase** (`JsonNamingPolicy.CamelCase`)
- For 500 errors, `message` is always `"An unexpected error occurred."` (the real message is **never** returned to the client)

### `BusinessException` Constructor

```csharp
public BusinessException(string code, string message, int httpStatusCode = 400)
```

Pre-built factory methods live in `BusinessErrors` static class (`ERPSystem.Application.Exceptions.BusinessException`).

---

## 8. Response & Result Patterns

> ⚠️ No generic `ApiResponse<T>` or `Result<T>` wrapper class was found in the provided files. Controllers appear to return domain objects or DTOs directly with appropriate HTTP status codes.

### Successful Responses

- **List queries:** Controllers return `List<T>` or `IReadOnlyList<T>` directly — no envelope.
- **Single item:** Returns the entity/DTO directly with `200 OK`.
- **Creation:** Expected convention is `201 Created` (⚠️ exact usage not confirmed without controller files).
- **Paged results (Expenses module):** The repository returns `(IReadOnlyList<T> Items, int TotalCount)` — the service/controller builds the paged response shape.

### Validation Errors

- ASP.NET Core's built-in model validation returns `400 Bad Request` with the standard `ProblemDetails` shape (unless overridden).
- Business-rule violations throw `BusinessException` which is caught by `ExceptionHandlingMiddleware` and returned as the `{ "error": { "code", "message" } }` shape.

### HTTP Status Code Conventions

| Code | When Used |
|---|---|
| `200 OK` | Successful GET, successful PUT/PATCH with body |
| `201 Created` | Successful POST that creates a resource |
| `400 Bad Request` | Validation failure, `InvalidOperationException`, `BusinessException` with httpStatusCode 400 |
| `401 Unauthorized` | JWT missing or invalid (handled by ASP.NET Core auth layer, not the middleware) |
| `403 Forbidden` | `UnauthorizedAccessException`, `BusinessException` with httpStatusCode 403 |
| `404 Not Found` | `BusinessException` with httpStatusCode 404 |
| `409 Conflict` | `BusinessException` with httpStatusCode 409 (e.g., `RoleAlreadyExists`) |
| `500 Internal Server Error` | Any unhandled `Exception` |

---

## 9. Base Classes & Abstractions (`ERPSystem.Domain.Abstractions`)

### `BaseEntity` (abstract class)

All standard domain entities inherit from this.

| Property | Type | Default | Description |
|---|---|---|---|
| `Id` | `int` | — | Primary key (auto-increment) |
| `CreatedAt` | `DateTime` | `DateTime.UtcNow` | UTC creation timestamp |
| `UpdatedAt` | `DateTime?` | `null` | UTC last-update timestamp |
| `IsDeleted` | `bool` | `false` | Soft-delete flag |

### `AuditableEntity : BaseEntity` (abstract class)

Extends `BaseEntity` with user-audit fields. Used by entities where knowing *who* made a change matters.

| Property | Type | Description |
|---|---|---|
| `CreatedByUserId` | `Guid` | Identity user ID of the creator |
| `UpdatedByUserId` | `Guid?` | Identity user ID of the last updater |
| `DeletedByUserId` | `Guid?` | Identity user ID who soft-deleted |

### `ICompanyEntity` (interface)

Marker interface applied to all entities that are scoped to a specific company (multi-tenant isolation).

| Member | Type | Description |
|---|---|---|
| `CompanyId` | `int` { get; set; } | Foreign key to `Company` |

### Repository Interfaces in `ERPSystem.Domain.Abstractions`

All repository interfaces live in this namespace. They define the data-access contract between Application and Infrastructure layers.

| Interface | Entity | Module |
|---|---|---|
| `ICompanyRepository` | `Company` | Core |
| `IModuleRepository` | `Module` | Core |
| `ICompanyModuleRepository` | `CompanyModule` | Core |
| `IProductRepository` | `Product` | Products (⚠️ interface not in provided files) |
| `ICategoryRepository` | `Category` | Products |
| `IUnitOfMeasureRepository` | `UnitOfMeasure` | Products |
| `IWarehouseRepository` | `Warehouse` | Inventory |
| `IInventoryRepository` | `InventoryDocument`, `StockItem` | Inventory |
| `IInventoryReportsRepository` | `StockItem`, `InventoryDocumentLine` | Inventory |
| `IEmployeeRepository` | `Employee` | HR (⚠️ interface not in provided files) |
| `IDepartmentRepository` | `Department` | HR (⚠️ interface not in provided files) |
| `IPositionRepository` | `JobPosition` | HR (⚠️ interface not in provided files) |
| `IAttendanceRepository` | `Attendance` | HR (⚠️ interface not in provided files) |
| `ILeaveRequestRepository` | `LeaveRequest` | HR (⚠️ interface not in provided files) |
| `ILeaveBalanceRepository` | `LeaveBalance` | HR (⚠️ interface not in provided files) |
| `IPayrollRepository` | `Payroll` | HR (⚠️ interface not in provided files) |
| `ICustomerRepository` | `Customer` | Sales |
| `ISalesInvoiceRepository` | `SalesInvoice` | Sales |
| `ISalesDeliveryRepository` | `SalesDelivery` | Sales |
| `ISalesReceiptRepository` | `SalesReceipt` | Sales |
| `ISalesReturnRepository` | `SalesReturn` | Sales |
| `IContactRepository` | `Contact` | Contacts |
| `IExpenseRepository` | `Expense` | Expenses |
| `IExpenseCategoryRepository` | `ExpenseCategory` | Expenses |
| `ILeadRepository` | `Lead` | CRM |
| `IPipelineRepository` | `Pipeline` | CRM (⚠️ interface not in provided files) |

**Common repository method patterns** (not a strict base interface — each declares its own methods):

```
GetByIdAsync(int id, [int companyId], [CancellationToken ct])
GetAllByCompanyAsync(int companyId, [filters], [CancellationToken ct])
AddAsync(TEntity entity, [CancellationToken ct])
Update(TEntity entity)          ← synchronous, marks as Modified
Delete(TEntity entity)          ← synchronous (soft or hard delete depending on module)
SaveChangesAsync([CancellationToken ct])
```

---

## 10. Pagination System

The system does **not** use a single universal pagination abstraction. Pagination is implemented per-module based on need.

### Observed Pagination Pattern (Expenses module — most complete)

**Request parameters** (passed individually to service/repository, not via a wrapper class):

| Parameter | Type | Description |
|---|---|---|
| `page` | `int` | 1-based page number |
| `pageSize` | `int` | Number of items per page |
| `search` | `string?` | Free-text search |
| `sortBy` | `string?` | Column name to sort by |
| `sortDir` | `string?` | `"asc"` or `"desc"` |
| Various filter params | nullable | e.g., `categoryId?`, `status?`, `fromDate?`, `toDate?`, `minAmount?`, `maxAmount?` |

**Repository return value:**

```csharp
Task<(IReadOnlyList<Expense> Items, int TotalCount)> GetPagedAsync(...)
```

**Where pagination logic lives:** Repository layer (`IExpenseRepository.GetPagedAsync`). The Application service receives the tuple and constructs the paged DTO response.

> ⚠️ Other modules (Sales, HR, etc.) return unfiltered `List<T>` from their repositories and do not implement server-side pagination in the provided files. A universal `PaginatedRequest<T>` / `PagedResult<T>` class does not exist in the codebase from what was provided.

---

## 11. Authentication & Security Overview

### JWT Configuration Keys (`appsettings.json`)

| Key Path | Purpose |
|---|---|
| `JWT:Key` | HMAC-SHA signing secret (symmetric) |
| `JWT:IssuerIP` | Token issuer URL validated on every request |
| `JWT:AudienceIP` | Token audience string validated on every request |
| `JWT:ExpireMinutes` | Token lifetime in minutes (`120` = 2 hours) |

### Token Validation Rules

| Rule | Setting |
|---|---|
| Validate Issuer | ✅ `true` — must match `JWT:IssuerIP` |
| Validate Audience | ✅ `true` — must match `JWT:AudienceIP` |
| Validate Lifetime | ✅ `true` |
| Clock Skew | `TimeSpan.Zero` — **no tolerance**; tokens expire exactly at `exp` claim |
| Validate Signing Key | ✅ `true` — SymmetricSecurityKey (UTF-8 bytes of `JWT:Key`) |
| Save Token | `true` — token accessible via `HttpContext` |
| Default Scheme | `JwtBearerDefaults.AuthenticationScheme` for authenticate, challenge, and default |

> ⚠️ `options.RequireHttpsMetadata = false;` is **commented out**. This means the line is not active, so `RequireHttpsMetadata` retains its framework default. In development the default may be `false`; in production it should be verified. The commented line suggests this was intentionally relaxed during development — it must be explicitly set to `true` in any production deployment.

### Identity Setup

- **User class:** `ApplicationUser : IdentityUser` (exact extra properties ⚠️ not in provided files)
- **Role class:** `IdentityRole<Guid>` (standard Identity role with Guid PK)
- **Primary key type:** `Guid` for both users and roles
- **Store:** `AppDbContext` via `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`

### `ICurrentUserService`

Registered as `AddScoped<ICurrentUserService, CurrentUserService>()`. Implementation in `ERPSystem.Infrastructure.Identity.CurrentUserService`. Uses `IHttpContextAccessor` to read the authenticated user's claims.

> ⚠️ The `ICurrentUserService` interface definition is not in the provided files. Expected properties/methods (inferred from usage pattern): `Guid UserId`, `int CompanyId`, `string Email`, `bool IsAuthenticated`. Verify against actual interface source before implementing dependents.

---

## 12. Authorization & Permission System

### How `AddPermissionPolicies()` Works

The extension method in `ERPSyatem.API.Extensions.PermissionPolicyExtensions`:

1. Registers `PermissionHandler` as a **singleton** `IAuthorizationHandler`
2. Uses reflection (`BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy`) to iterate all `public const string` fields in `Permissions` class (in `ERPSystem.Application.Authorization`) and all its **nested types recursively**
3. For each discovered permission string value, registers an authorization policy whose **name equals the permission string value**
4. Each policy has a single `PermissionRequirement(permissionValue)` requirement
5. `PermissionHandler` checks that the authenticated user has a claim of type `"permission"` with a value matching the requirement

### Permission Claim Storage

- **Claim type:** `"permission"` (constant in `CompanyRoleSeeder`)
- **Claim value:** The permission string constant (e.g., `"Sales.Customers.Manage"` — ⚠️ exact string values depend on constants defined in `Permissions` class, not provided)
- Claims are stored on **roles** via `RoleManager.AddClaimAsync(role, new Claim("permission", value))`

### Role Scoping (Multi-Tenancy)

Roles are **company-scoped** using `RoleKey.ForCompany(companyId, displayName)`:
- A role name in Identity is `"{companyId}_{roleName}"` (exact format from `RoleKey.ForCompany` — ⚠️ implementation not in provided files)
- This prevents role bleeding between companies
- Only the **Owner** role is actively seeded by default (Admin, Manager, User roles are commented out in `CompanyRoleSeeder.DefaultRolePermissions`)

### Permission Naming Structure

The `Permissions` class uses nested static classes with pattern: **`Permissions.{Module}.{Resource}.{Action}`**

| Module | Resources | Observed Actions |
|---|---|---|
| `Core` | `Users`, `Companie` (⚠️ typo vs Company), `Modules` | `Read`, `Create`, `Update`, `Delete`, `Manage` |
| `Security` | `Roles` | `Manage` |
| `Sales` | `Customers`, `Invoices`, `Deliveries`, `Receipts`, `Returns` | `Read`, `Manage`, `Access`, `Create`, `Update`, `Delete`, `Post`, `Cancel` |
| `Inventory` | `Stock`, `Warehouses`, `Reports` | `Read`, `Manage`, `StockIn`, `StockOut`, `Adjust`, `Transfer`, `Opening` |
| `Products` | `Product`, `Categories`, `UnitOfMeasures` | `Read`, `Manage`, `Create`, `Update`, `Delete` |
| `Expenses` | `Items`, `Categories` | `Read`, `Manage`, `Create`, `Update`, `Delete` |
| `Contacts` | `Contact` | `Read`, `Manage` |
| `Hr` | `Employees`, `Departments`, `Positions`, `PayRolls`, `LeaveRequests` | `Read`, `Manage`, `Access` |
| `CRM` | `Leads`, `Customers` | `Read`, `Manage`, `Access` |

### Applying Authorization in Controllers

Decorated with `[Authorize(Policy = "PermissionStringValue")]` — applied at the **action level** or **controller level** (⚠️ exact placement pattern not confirmed without controller files).

---

## 13. CORS Configuration

| Setting | Value |
|---|---|
| Policy Name | `"MyPolicy"` |
| Allowed Origins | `AllowAnyOrigin()` — all origins permitted |
| Allowed Methods | `AllowAnyMethod()` — GET, POST, PUT, DELETE, OPTIONS, etc. |
| Allowed Headers | `AllowAnyHeader()` — all headers including `Authorization` |

> ⚠️ **Production readiness:** This configuration is a wildcard and is **not safe for production**. It bypasses all origin restrictions. Before deploying, replace with explicit `WithOrigins("https://yourapp.com")` and restrict methods/headers as needed. Note also that `AllowAnyOrigin()` is incompatible with `AllowCredentials()` — if cookies or credentials are ever needed, the policy must be rewritten.

---

## 14. Database & Seeding Overview

### AppDbContext

- **Class:** `AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- **Namespace:** `ERPSystem.Infrastructure.Data`
- **Connection string key:** `ConnectionStrings:DefaultConnection`
- **Connection string format:** SQL Server with Integrated Security + `TrustServerCertificate=True`

### DbSet Count by Module

| Module | DbSets | Entity Names |
|---|---|---|
| Core | 3 | `Companies`, `Modules`, `CompanyModules` |
| Products | 3 | `Products`, `Categories`, `UnitsOfMeasure` |
| Inventory | 4 | `Warehouses`, `StockItems`, `InventoryDocuments`, `InventoryDocumentLines` |
| HR | 10 | `Employees`, `EmployeeDocuments`, `Departments`, `JobPositions`, `Attendances`, `LeaveRequests`, `LeaveAttachments`, `LeaveBalances`, `Payrolls`, `PayrollLineItems` |
| Sales | 9 | `Customers`, `SalesInvoices`, `SalesInvoiceLines`, `SalesDeliveries`, `SalesDeliveryLines`, `SalesReceipts`, `SalesReceiptAllocations`, `SalesReturns`, `SalesReturnLines` |
| Contacts | 1 | `Contacts` |
| Expenses | 2 | `ExpenseCategories`, `Expenses` |
| CRM | 2 | `Leads`, `Pipelines` |
| **Total** | **34** | Plus 7 standard ASP.NET Identity tables |

> Full EF configuration details (indexes, precision, delete behaviors) are documented in `02_DATABASE_CONTEXT.md`.

### Seeding

| Seeder | Method | What It Seeds | When Called |
|---|---|---|---|
| `DbSeeder` | `SeedModulesAsync(context)` | 6 system modules: `SALES`, `INVENTORY`, `CONTACT`, `EXPENSES`, `HR`, `CRM` | On every app startup (idempotent, checks existing keys) |
| `DbSeeder` | `SeedDefaultExpenseCategoriesAsync(context, companyId)` | 8 expense categories: Rent, Software, Marketing, Supplies, Meals, Utilities, Travel, Other | Called when a company enables the Expenses module (not at startup) |
| `CompanyRoleSeeder` | `SeedCompanyRolesAsync(roleManager, companyId)` | `Owner` role with all permissions as claims | Called when a new company is created (not at startup) |
| `AppDbContext.SeedData()` | (via `OnModelCreating`) | 2 Departments (HR, IT) and 3 JobPositions (HR Manager, Dev Manager, Senior Developer) with fixed GUIDs | Applied via EF migration (compile-time seed data) |

> **Timing:** Runtime seeding runs **after `app.Build()`** but **before middleware** is activated (step 30 in startup sequence). The seeder uses a manually created DI scope.

---

## 15. Shared Enums & Constants

All enums are in `ERPSystem.Domain.Enums`. All are used across the Infrastructure and Application layers.

| Enum Name | Values | Used In Modules |
|---|---|---|
| `ContactPersonType` | `Client=1`, `Vendor=2`, `Partner=4`, `leader=5` (⚠️ lowercase typo) | Contacts, CRM |
| `SalesInvoiceStatus` | `Draft=0`, `Posted=1`, `PartiallyDelivered=2`, `FullyDelivered=3`, `Cancelled=4` | Sales |
| `PaymentStatus` | `Unpaid=0`, `PartiallyPaid=1`, `Paid=2` | Sales |
| `SalesDeliveryStatus` | `Draft=0`, `Posted=1`, `Cancelled=2` | Sales |
| `SalesReturnStatus` | `Draft=0`, `Posted=1`, `Cancelled=2` | Sales |
| `SalesReceiptStatus` | `Draft=0`, `Posted=1`, `Cancelled=2` | Sales |
| `ExpenseStatus` | `Pending=0`, `Paid=1` | Expenses |
| `PaymentMethod` | `Cash=0`, `CreditCard=1`, `DebitCard=2`, `BankTransfer=3`, `Check=4`, `Other=5` | Expenses |
| `InventoryDocType` | `In=1`, `Out=2`, `Transfer=3`, `Adjustment=4`, `Opening=5` | Inventory |
| `InventoryDocumentStatus` | `Draft=1`, `Posted=2`, `Canceled=3` | Inventory |
| `InventoryLineType` | `In=1`, `Out=2` | Inventory |
| `EmployeeStatus` | `Active=1`, `Inactive=2`, `OnLeave=3`, `Terminated=4` | HR |
| `Gender` | `Male=1`, `Female=2`, `Other=3` | HR |
| `MaritalStatus` | `Single=1`, `Married=2`, `Divorced=3`, `Widowed=4` | HR |
| `PositionLevel` | `Junior=1`, `Mid=2`, `Senior=3`, `Lead=4`, `Manager=5`, `Director=6` | HR |
| `AttendanceStatus` | `Present=1`, `Absent=2`, `Late=3`, `OnLeave=4`, `Holiday=5`, `Weekend=6` | HR |
| `LeaveType` | `Annual=1`, `Sick=2`, `Unpaid=3`, `Emergency=4`, `Maternity=5`, `Paternity=6`, `Study=7` | HR |
| `LeaveRequestStatus` | `Pending=1`, `Approved=2`, `Rejected=3`, `Cancelled=4` | HR |
| `PayrollStatus` | `Draft=1`, `Processed=2`, `Paid=3` | HR |
| `PayrollLineItemType` | `Allowance=1`, `Deduction=2` | HR |
| `LeadStatus` | `New=1`, `Contacted=2`, `Qualified=3`, `Proposal=4`, `Negotiation=5`, `Won=6`, `Lost=7` | CRM |
| `LeadSource` | `Website=1`, `Referral=2`, `ColdCall=3`, `SocialMedia=4`, `Email=5`, `Event=6`, `LinkedIn=7` | CRM |
| `DealStatus` | `New=1`, `Qualified=2`, `Proposal=3`, `Negotiation=4` | CRM |

> ⚠️ Two known casing inconsistencies in existing code: `ContactPersonType.leader` (lowercase `l`) and `Core.Companie` (missing `s`). Do **not** rename these — it will break EF data and authorization policies.

---

## 16. Key Architectural Rules (Do's and Don'ts)

### Layer Responsibilities

- **Controllers MUST NOT contain business logic.** All computation, validation, and domain rules must be delegated to the injected `I*Service`.
- **Services MUST NOT access `DbContext` directly.** All database operations go through `I*Repository`. Services depend on repository interfaces, not on `AppDbContext`.
- **Repositories MUST NOT contain business logic.** They only perform data retrieval and persistence. Business rule validation belongs in services.
- **Domain entities MUST NOT reference Application or Infrastructure namespaces.** The dependency arrow only goes outward from Domain.

### Registration

- **All new services and repositories MUST be registered with `AddScoped` in `Program.cs`.** Forgetting to register breaks runtime without a compile error.
- **`IAuthorizationHandler` implementations MUST be registered as `AddSingleton`.** `PermissionHandler` already follows this; maintain it for any new handlers.

### Soft Deletes

- `BaseEntity.IsDeleted` is the soft-delete flag. Queries MUST filter on `IsDeleted == false` unless explicitly retrieving deleted records. EF Core does not apply this filter automatically — repositories are responsible.

### Multi-Tenancy (Company Isolation)

- Every entity that implements `ICompanyEntity` MUST include `CompanyId` in all queries. Never return data across companies.
- Roles are company-scoped via `RoleKey.ForCompany(companyId, roleName)`. Never assign or query roles without the company prefix.

### Module Access Guard

- Before performing any module-specific business operation, services MUST check that the company has the relevant module enabled via `IModuleAccessService` or equivalent. Bypassing this check will allow unauthorized module access. The `BusinessErrors` static class provides pre-built exceptions for each module's disabled state.

### Exception Handling

- **Business rule violations MUST throw `BusinessException`** (with appropriate `code` and `httpStatusCode`) and MUST NOT return `null` or empty responses for error states.
- **Never throw raw `Exception` for expected business scenarios** — use the factories in `BusinessErrors` or create new named factory methods there.
- **Never log sensitive data** (PII, tokens, passwords) — the middleware logs only exception type, code, and message.

### Decimal Precision

- All monetary amounts use `HasPrecision(18, 2)`.
- All quantities use `HasPrecision(18, 4)`.
- All percentage fields use `HasPrecision(5, 2)`.
- New decimal properties MUST be configured in `AppDbContext.OnModelCreating` — EF will default to `decimal(18,2)` otherwise, which may silently truncate quantities.

### CancellationToken

- All async repository and service methods SHOULD accept and propagate `CancellationToken`. Follow the existing signatures (`ct = default`) for new methods.

### New Modules

When adding a new business module:
1. Add entity classes inheriting from `BaseEntity` or `AuditableEntity` and implementing `ICompanyEntity` in `ERPSystem.Domain.Entities.{NewModule}`.
2. Define `I{Name}Repository` interfaces in `ERPSystem.Domain.Abstractions`.
3. Add `DbSet<{Entity}>` properties to `AppDbContext` and configure precision/indexes in `OnModelCreating`.
4. Implement repositories in `ERPSystem.Infrastructure.Repositories.{NewModule}`.
5. Define `I{Name}Service` in `ERPSystem.Application.Interfaces` and implement in `ERPSystem.Application.Services.{NewModule}`.
6. Add permissions to the `Permissions` class in `ERPSystem.Application.Authorization` as nested static classes.
7. Register repos and services in `Program.cs` with `AddScoped`.
8. Seed the module key in `DbSeeder.SeedModulesAsync`.
9. Add the module-disabled `BusinessException` factory to `BusinessErrors`.
10. Create and run EF Core migration from the `ERPSystem.Infrastructure` project.
