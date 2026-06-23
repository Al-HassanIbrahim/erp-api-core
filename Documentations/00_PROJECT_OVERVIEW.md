# 00_PROJECT_OVERVIEW

---

## 1. Tech Stack

| Component | Technology | Version |
|---|---|---|
| **Framework** | .NET | 8.0 |
| **Web Framework** | ASP.NET Core | 8.0 |
| **ORM** | Entity Framework Core | 8.0.0 |
| **Database** | SQL Server | Latest |
| **Authentication** | ASP.NET Core Identity | 8.0 |
| **API Security** | JWT Bearer Tokens | - |
| **API Documentation** | Swashbuckle.AspNetCore (Swagger) | 6.6.2 |
| **SDK Type** | `Microsoft.NET.Sdk.Web` | - |

---

## 2. Solution Architecture

The application follows **Clean Architecture / N-Tier** principles with strict dependency isolation.

### Architecture Diagram

```
┌─────────────────────────────────────────┐
│    [ERPSyatem.API]                      │
│    (Presentation / Host / Controllers)  │
└─────────────────┬───────────────────────┘
                  │
        ┌─────────┴──────────┬──────────────┐
        │                    │              │
    ┌───▼──────────┐  ┌──────▼───┐  ┌──────▼────┐
    │ Extensions   │  │ Middleware│  │Controllers│
    └──────────────┘  └───────────┘  └───────────┘
        │
        └─────────────────┬───────────────────────┐
                          │                       │
                ┌─────────▼──────────┐  ┌────────▼─────────┐
                │ Application Layer  │  │ Infrastructure   │
                │ - Services         │  │ - Repositories   │
                │ - DTOs             │  │ - DbContext      │
                │ - Interfaces (I*)  │  │ - Identity       │
                │ - Business Logic   │  │ - External APIs  │
                └─────────┬──────────┘  └────────┬─────────┘
                          │                      │
                          └──────────┬───────────┘
                                     │
                          ┌──────────▼─────────┐
                          │  Domain Layer      │
                          │  - Entities        │
                          │  - Value Objects   │
                          │  - Core Enums      │
                          │  - Aggregates      │
                          │  (Zero deps)       │
                          └────────────────────┘
```

### Layer Responsibilities

| Layer | Project | Dependencies | Responsibility |
|---|---|---|---|
| **Domain** | `ERPSystem.Domain` | None | Core business entities, enums, value objects. No external dependencies. |
| **Application** | `ERPSystem.Application` | Domain only | Business rules, service interfaces, DTOs, use cases. Maps domain logic to APIs. |
| **Infrastructure** | `ERPSystem.Infrastructure` | Domain + Application | DbContext, repositories, EF configurations, identity, external service integrations. |
| **Presentation (API)** | `ERPSyatem.API` | All layers | Composition root, controllers, middleware, DI setup, HTTP concerns. |

---

## 3. Project Name Convention

### **⚠️ CRITICAL TYPO NOTICE**

| Project | Correct Spelling | Actual Name | Notes |
|---|---|---|---|
| Presentation | `ERPSystem.API` | **`ERPSyatem.API`** | **TYPO: 'a' instead of 's'** |
| Application | `ERPSystem.Application` | `ERPSystem.Application` | ✅ Correct |
| Domain | `ERPSystem.Domain` | `ERPSystem.Domain` | ✅ Correct |
| Infrastructure | `ERPSystem.Infrastructure` | `ERPSystem.Infrastructure` | ✅ Correct |

**Action:** Rename `ERPSyatem.API` → `ERPSystem.API` (future refactor priority). Until then, all code references map `ERPSyatem.*` to the presentation layer context.

---

## 4. Registered Modules

The system is **heavily modularized** using dependency injection extensions. Each module registers its own repositories and services via `AddXxxModule()` in `Program.cs`.

### Module Inventory

| Module | Domain Entities | Core Repositories | Core Services | Business Domain |
|---|---|---|---|---|
| **Core** | Company, Module, CompanyModule, CompanyProfile, CompanyUser | Company, Module, CompanyModule, User | CompanyProfile, ModuleAccess, MyAccount | Configuration, multi-tenancy setup |
| **Product** | Product, UnitOfMeasure, Category | Product, UnitOfMeasure, Category | Product, UnitOfMeasure, Category | Product catalog management |
| **Inventory** | Warehouse, Inventory, InventoryReports | Warehouse, Inventory, InventoryReports | Warehouse, Inventory, InventoryReports | Stock & warehouse management |
| **HR** | Employee, Department, Position, Attendance, LeaveRequest, LeaveBalance, Payroll | Employee, Department, Position, Attendance, LeaveRequest, LeaveBalance, Payroll | Employee, Attendance, LeaveRequest, Payroll | Human resources, payroll, leave |
| **Sales** | Customer, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn | Customer, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn | Customer, SalesInvoice, SalesDelivery, SalesReceipt, SalesReturn | Sales cycle, invoicing, delivery |
| **Contact** | Contact | Contact | Contact | Address book, contact management |
| **Expenses** | Expense, ExpenseCategory, ExpenseStats | Expense, ExpenseCategory | Expense, ExpenseCategory, ExpenseStats | Expense tracking & reporting |
| **CRM** | Lead, Pipeline | Lead, Pipeline | Lead, Pipeline | Sales pipeline, lead management |

### Module Registration Pattern

```csharp
// In Program.cs:
builder.Services
    .AddCoreModule()
    .AddProductModule()
    .AddInventoryModule()
    .AddHRModule()
    .AddSalesModule()
    .AddContactModule()
    .AddExpensesModule()
    .AddCRMModule();
```

---

## 5. Naming Conventions

| Category | Pattern | Example | Location |
|---|---|---|---|
| **Interface (Service)** | `I[Entity]Service` | `IProductService` | Application layer |
| **Interface (Repository)** | `I[Entity]Repository` | `IProductRepository` | Application layer |
| **Service Implementation** | `[Entity]Service` | `ProductService` | Application layer |
| **Repository Implementation** | `[Entity]Repository` | `ProductRepository` | Infrastructure layer |
| **Controller** | `[Entity]Controller` | `ProductController` | API layer |
| **DTO (Request)** | `Create[Entity]Dto` or `Update[Entity]Dto` | `CreateProductDto` | Application.DTOs |
| **DTO (Response)** | `[Entity]Dto` or `[Entity]ViewDto` | `ProductDto` | Application.DTOs |
| **Entity** | `[Entity]` | `Product` | Domain.Entities |
| **Enum** | `[Concept][Type]` | `ProductStatus`, `InvoiceType` | Domain.Enums |
| **Middleware** | `[Purpose]Middleware` | `ExceptionHandlingMiddleware` | API.Middleware |
| **Extension** | `[Module]ServiceCollectionExtensions` | `ProductServiceCollectionExtensions` | Application.Extensions |

### Dependency Injection Lifetime

* **Default Lifetime:** `AddScoped` (creates new instance per HTTP request)
* **All services and repositories** use `Scoped` lifetime unless explicitly documented otherwise.
* **DbContext:** Scoped (managed by EF Core's DI integration)

---

## 6. Startup Sequence

The initialization pipeline in `Program.cs` follows a strict, ordered sequence to avoid configuration race conditions:

### Numbered Startup Steps

1. **Create WebApplication Builder**
   ```csharp
   var builder = WebApplication.CreateBuilder(args);
   ```
   - Initialize ASP.NET Core host builder.

2. **Register DbContext**
   ```csharp
   builder.Services.AddDbContext<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```
   - SQL Server connection via EF Core.
   - Connection string: `DefaultConnection` in `appsettings.json`.

3. **Register Domain Layer (Modules)**
   ```csharp
   builder.Services
       .AddCoreModule()
       .AddProductModule()
       .AddInventoryModule()
       ... (all modules)
   ```
   - Each module registers its repositories and services.
   - All via `AddScoped`.

4. **Register Controllers & API**
   ```csharp
   builder.Services.AddControllers();
   builder.Services.AddEndpointsApiExplorer();
   ```
   - Enable MVC controller routing.
   - Enable Swagger/OpenAPI endpoint discovery.

5. **Configure Swagger/OpenAPI**
   ```csharp
   builder.Services.AddSwaggerGen(options =>
       options.AddSecurityDefinition("Bearer", /* JWT schema */)
   );
   ```
   - Swagger documentation with JWT Bearer security scheme.

6. **Register Identity & Authentication**
   ```csharp
   builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(...)
       .AddEntityFrameworkStores<AppDbContext>();
   
   builder.Services
       .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddJwtBearer(options => { /* token validation params */ });
   ```
   - ASP.NET Core Identity with Guid-based roles.
   - JWT Bearer token validation.

7. **Register Authorization & Custom Policies**
   ```csharp
   builder.Services.AddPermissionPolicies();
   builder.Services.AddAuthorization();
   ```
   - Add custom permission-based policies (e.g., `CanViewReports`, `CanManageEmployees`).

8. **Register CORS**
   ```csharp
   builder.Services.AddCors(options =>
       options.AddPolicy("MyPolicy", policy =>
           policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader())
   );
   ```
   - CORS policy named `MyPolicy`.

9. **Build the Application**
   ```csharp
   var app = builder.Build();
   ```
   - Construct the middleware pipeline.

10. **Data Seeding (Initial DB Population)**
    ```csharp
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbSeeder.SeedModulesAsync(context);
    }
    ```
    - Populate database with initial/reference data (Modules, Roles, etc.).
    - Runs **after** migrations but **before** `app.Run()`.

11. **Configure Middleware Pipeline**
    - See Section 7 for pipeline order.

12. **Run Application**
    ```csharp
    app.Run();
    ```
    - Start listening for HTTP requests.

---

## 7. Middleware Pipeline Order

The request pipeline is strictly ordered to prevent auth/CORS race conditions. **Order matters.**

### Pipeline Sequence

| Order | Middleware | Purpose | Notes |
|---|---|---|---|
| 1 | `UseSwagger()` + `UseSwaggerUI()` | OpenAPI documentation | Dev-only (conditional) |
| 2 | `UseExceptionHandling()` | Custom exception handling | Catches all unhandled exceptions |
| 3 | `UseCors("MyPolicy")` | CORS policy application | Must run before auth |
| 4 | `UseAuthentication()` | Extract JWT token, set `HttpContext.User` | Validates token signature/expiry |
| 5 | `UseAuthorization()` | Apply policy-based authorization | Checks `[Authorize]` attributes & policies |
| 6 | `MapControllers()` | Route requests to controller actions | HTTP verb + route matching |
| 7 | `Run()` | Start request pipeline | Infinite loop, listens for connections |

### Critical Rules

* **CORS before Auth:** CORS must execute before authentication to avoid preflight request failures.
* **Auth before Authorization:** Authentication extracts identity; authorization checks permissions.
* **Exception handler first:** Catches all exceptions from lower middleware.

---

## 8. CORS Policy

| Property | Value | Status | Notes |
|---|---|---|---|
| **Policy Name** | `MyPolicy` | Active | Referenced in middleware setup |
| **AllowAnyOrigin** | `true` | ⚠️ Permissive | Allows requests from any domain |
| **AllowAnyMethod** | `true` | ⚠️ Permissive | Allows GET, POST, PUT, DELETE, PATCH, etc. |
| **AllowAnyHeader** | `true` | ⚠️ Permissive | Allows all custom headers |

### Production Concern

**Current Configuration:** Maximum permissiveness (likely for Development/Testing).

**Recommendation for Production:**
```csharp
policy
    .WithOrigins("https://yourdomain.com", "https://app.yourdomain.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
```

---

## 9. Key Patterns

### 9.1 Repository Pattern

* **Purpose:** Isolate data access logic from business logic.
* **Implementation:** Every domain entity has a corresponding `I[Entity]Repository` interface.
* **Scope:** All database queries (LINQ, EF Core calls) occur **exclusively** in repository implementations.
* **Principle:** Services **never** directly reference `DbContext`; they depend on `IRepository` abstractions.

**Example Flow:**
```
Controller → ProductService (logic) → IProductRepository (data) → DbContext → SQL Server
```

### 9.2 Service Layer Pattern

* **Purpose:** Encapsulate business rules, validations, and workflows.
* **Implementation:** Thin controllers delegate all logic to `I[Entity]Service`.
* **Responsibility:**
  - Validate inputs.
  - Call repositories for data.
  - Orchestrate workflows.
  - Throw domain-specific exceptions.
  - Return DTOs (not entities).

**Controller Principle:** Controllers should be **thin** — typically 5–15 lines per action.

### 9.3 Current User Context

* **Service:** `ICurrentUserService`
* **Dependencies:** Internally uses `IHttpContextAccessor` to extract current user from `HttpContext.User`.
* **Usage:** Injected into services to determine ownership, permissions, company affiliation.
* **Returns:** `ApplicationUser` id, email, company id, assigned roles.

### 9.4 Security Extraction

* **Token Generation:** `IJwtTokenService` → generates JWT tokens after successful login.
* **Token Validation:** `IAuthService` → validates credentials, returns `LoginResultDto` with token.
* **Claims Extraction:** `ICurrentUserService` → reads claims from validated token and returns user context.

### 9.5 Configuration Binding

* **JWT Settings:** Bound from `appsettings.json` → `JwtSettings` class.
  - `Jwt:Key` (secret key for signing)
  - `Jwt:IssuerIP` (API's IP/domain)
  - `Jwt:AudienceIP` (expected client IP/domain)
  - `Jwt:ExpireMinutes` (token TTL)
* **Database Connection:** Bound from `appsettings.json` → `DefaultConnection`.
* **Logging Levels:** Configured per namespace in `appsettings.json`.

---

## 10. Dependency Injection Summary

| Service | Lifetime | Registered In | Purpose |
|---|---|---|---|
| `AppDbContext` | Scoped | Infrastructure | EF Core context, database access |
| `I[Entity]Repository` | Scoped | Each module | Data persistence abstraction |
| `I[Entity]Service` | Scoped | Each module | Business logic abstraction |
| `IJwtTokenService` | Scoped | Infrastructure | Token generation & validation |
| `IAuthService` | Scoped | Infrastructure | Authentication workflows |
| `ICurrentUserService` | Scoped | Infrastructure | Current user context extraction |
| `IHttpContextAccessor` | Singleton | Core ASP.NET | Access to `HttpContext` |
| Identity (`UserManager`, `RoleManager`, `SignInManager`) | Scoped | Identity service | User management, role management, sign-in |

---

## 11. Configuration Files

| File | Location | Purpose | Key Sections |
|---|---|---|---|
| `appsettings.json` | `ERPSyatem.API` root | Production settings | `Jwt`, `ConnectionStrings`, `Logging` |
| `appsettings.Development.json` | `ERPSyatem.API` root | Dev overrides | Swagger enabled, Logging: Debug |

---

## 12. Key Assumptions & Constraints

1. **Single Database:** One `AppDbContext` serves all modules; no sharding.
2. **Scoped Lifetime:** All services and repositories are scoped; no singletons for stateful services.
3. **JWT-Only Auth:** No session-based authentication; purely token-based.
4. **Guid-Based Identity:** Roles are `IdentityRole<Guid>`, not int.
5. **SQL Server:** Hard-coded to SQL Server; no DB abstraction layer.
6. **Eager Validation:** Client input validation happens in DTOs and services; no validation at repository level.
7. **No Global Exception Filter:** Exceptions are caught by custom middleware.

---

## 13. Quick Reference

### When Working On...

| Task | Load These Files | Expected Tokens |
|---|---|---|
| Any feature | `00_PROJECT_OVERVIEW.md` | 2,000 |
| One specific module | `00_PROJECT_OVERVIEW.md` + `MODULE_*.md` | 3,500 |
| Auth / JWT issues | `00_PROJECT_OVERVIEW.md` + `01_AUTH_SECURITY.md` | 2,500 |
| Database / migrations | `00_PROJECT_OVERVIEW.md` + `02_DATABASE_CONTEXT.md` | 3,000 |
| New API endpoint | `00_PROJECT_OVERVIEW.md` + `11_API_ENDPOINTS_MAP.md` | 2,200 |
| Cross-module feature | `00_PROJECT_OVERVIEW.md` + relevant `MODULE_*.md` files | 4,000+ |

---

## 14. Typo Map (For Reference)

| Misspelled | Correct | Context |
|---|---|---|
| `ERPSyatem` | `ERPSystem` | Project name (presentation layer) |
| All others | ✅ | Correct spelling used elsewhere |

---

**Document Version:** 1.0  
**Last Updated:** 2024  
**Status:** Ready for AI Consumption
