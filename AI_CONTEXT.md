# Project Name
ERP System API Core (erp-api-core)

# Project Type
Multi-tenant Enterprise Resource Planning (ERP) Backend API

# Architecture
Clean Architecture (Onion Architecture) with Layered Separation.
Modules are vertically sliced within the standard layers.

# Technology Stack
- Framework: .NET 8.0 (C#)
- Database ORM: Entity Framework Core 8.0.0
- Database Engine: SQL Server
- Testing: xUnit, Moq, FluentAssertions, EF InMemory, Coverlet

# Database
- Single SQL Server Database with multi-tenant filtering (typically via `CompanyId`).
- Decimal precision configured via FluentAPI in `AppDbContext` (18, 2 or 18, 4 depending on context).

# Authentication
- JWT ****** (`IJwtTokenService`, `AuthService`).
- Token issued with `CompanyId` and display roles.

# Authorization
- Claims-based / Policy-based authorization.
- Custom `PermissionPolicyExtensions` dynamically loads policies from static `Permissions` constants.

# Shared Components
- Core Module (Company, Module Access, Users)
- Exception Handling Middleware (`ExceptionHandlingMiddleware`)
- JWT Token Service
- Current User Context (`ICurrentUserService`)

# Business Modules
- **Core**: Multi-tenancy setup, Company profiles, User management, Module access control.
- **Products**: Items, Categories, Unit of Measure.
- **Inventory**: Warehouses, Stock Items, Inventory Documents.
- **HR**: Employees, Departments, Job Positions, Attendance, Leaves, Payroll.
- **Sales**: Customers, Invoices, Deliveries, Receipts, Returns.
- **Contacts**: General contact management.
- **Expenses**: Expense Categories, Expenses tracking.
- **CRM**: Leads, Pipelines.

# Dependency Rules
- `ERPSystem.Domain`: Core Entities and Abstractions (No external dependencies).
- `ERPSystem.Application`: Use Cases, DTOs, Services (Depends on Domain).
- `ERPSystem.Infrastructure`: EF Core, Identity, Data Access (Depends on Application & Domain).
- `ERPSyatem.API`: Controllers, Configuration, Entry Point (Depends on Application & Infrastructure).

# Coding Standards
- Asynchronous patterns (`async/await` with `CancellationToken`).
- Dependency Injection for Repositories and Services.
- Result encapsulation via exceptions (`BusinessException`) mapped to HTTP Status Codes.
- Soft Deletes implemented on most entities (`IsDeleted`, `DeletedByUserId`).

# Naming Conventions
- Services: `I[Entity]Service` / `[Entity]Service`
- Repositories: `I[Entity]Repository` / `[Entity]Repository`
- DTOs: `[Action][Entity]Dto` (e.g., `UpdateCompanyMeDto`)

# Important Architectural Decisions
1. Use `BusinessException` for domain logic failures.
2. Implement Multi-tenancy via `CompanyId` on domain entities.
3. Use Identity for `ApplicationUser` but extend `CompanyId` inside tokens.

# Non-Negotiable Rules
1. Never bypass Repositories to access `AppDbContext` in Services or Controllers.
2. Cross-tenant data leakage MUST be prevented (Always filter by `CompanyId`).
3. Domain entities must remain pure and oblivious of EF Core configurations (Configured in `AppDbContext.OnModelCreating`).

# Known Limitations
1. In-memory caching isn't widely used; DB queries are heavy.
2. EF InMemory is used for unit tests instead of SQLite or Testcontainers, which might miss SQL Server specific issues.

# AI Instructions
- Always verify multi-tenancy constraints when adding/editing queries.
- Check `AppDbContext` for existing precision/relationship configurations before modifying domain entities.
- Add `CancellationToken` to all async data access operations.

---
## AI Quick Context
### Depends On
None
### Uses
N/A
### Used By
Future AI sessions
### Related Modules
All
### Shared Components
N/A
### Entities
N/A
### Business Rules
N/A
### Endpoints
N/A
### Future Refactoring
N/A
### Modification Risk
Low
### Confidence
High
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSyatem.API/Program.cs
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Data/AppDbContext.cs
