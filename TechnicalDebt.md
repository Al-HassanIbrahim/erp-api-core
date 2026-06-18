# Technical Debt Log

## Overview
Tracks known suboptimal implementations, workarounds, and architectural gaps in the ERP API Core.

---
## Responsibilities
Highlights areas of the codebase that require refactoring, optimization, or re-architecture.

---
## Workflow
N/A

---
## Dependencies
N/A

---
## Public APIs
N/A

---
## Limitations
N/A

---
## Technical Debt

### 1. Lack of Domain Events (Inferred High)
- **Description:** Cross-module interactions (e.g., Sales to Inventory) are likely handled by procedural service calls.
- **Impact:** High coupling between application services.
- **Fix:** Introduce MediatR and Domain Events to decouple module side-effects.

### 2. Missing Global Query Filters for Multi-Tenancy (Verified)
- **Description:** While `CompanyId` exists on entities, repositories manually filter by it (or `IsDeleted`). For example, `CompanyRepository` explicitly checks `!c.IsDeleted`.
- **Impact:** High risk of developer error resulting in cross-tenant data leakage or retrieving deleted records.
- **Fix:** Implement EF Core Global Query Filters in `AppDbContext.OnModelCreating` for `CompanyId` and `IsDeleted`.

### 3. Namespace Typos (Verified)
- **Description:** The API project folder is named `ERPSyatem.API` (typo: 'a' instead of 's').
- **Impact:** Confusing developer experience.
- **Fix:** Rename folder and namespaces to `ERPSystem.API`.

### 4. Direct Dependency on EF Core in Tests (Verified)
- **Description:** `ERPSystem.Tests.Unit` uses `Microsoft.EntityFrameworkCore.InMemory`.
- **Impact:** In-memory provider lacks relational features (transactions, raw SQL, default constraints) making tests less reliable compared to the actual SQL Server.
- **Fix:** Migrate to Testcontainers or SQLite in-memory mode.

---
## AI Quick Context
### Depends On
N/A
### Uses
N/A
### Used By
Future AI during refactoring tasks.
### Related Modules
All
### Shared Components
AppDbContext
### Entities
All
### Business Rules
N/A
### Endpoints
N/A
### Future Refactoring
Global Query Filters implementation is the highest priority.
### Modification Risk
Low
### Confidence
High (Directly verified from code inspections)
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Repositories/Core/CompanyRepository.cs
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Data/AppDbContext.cs
