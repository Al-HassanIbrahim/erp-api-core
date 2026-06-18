# Architectural Decision Log

## Overview
Records critical architectural, design, and technical decisions made in the ERP API Core project.

---
## Responsibilities
Provides context and reasoning behind technical choices to prevent regressions or circular discussions.

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
Some early decisions might incur debt as the system scales (e.g., synchronous repository calls across modules).

---
## Decisions

### ADR-001: Clean Architecture Implementation
- **Decision:** Adopt Clean Architecture (Domain, Application, Infrastructure, API).
- **Reason:** To isolate domain logic from external concerns (like EF Core or HTTP) ensuring high testability.
- **Status:** Active.

### ADR-002: Multi-Tenancy Strategy
- **Decision:** Row-level multi-tenancy using `CompanyId`.
- **Reason:** Simplifies database management and migrations compared to database-per-tenant.
- **Status:** Active.

### ADR-003: Authorization Model
- **Decision:** Custom Policy-based permissions driven by reflection (`PermissionPolicyExtensions.cs`).
- **Reason:** Allows dynamic scaling of permissions without rewriting policy registration logic; ties permissions to static string constants natively.
- **Status:** Active.

### ADR-004: Soft Delete
- **Decision:** Use `IsDeleted` flag and `DeletedByUserId` rather than hard deletes.
- **Reason:** Preserves historical audit trails and prevents accidental catastrophic data loss in a multi-tenant environment.
- **Status:** Active.

---
## AI Quick Context
### Depends On
N/A
### Uses
N/A
### Used By
Future AI for architectural context.
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
Revisit row-level multi-tenancy if database size exceeds single instance capacities.
### Modification Risk
Low
### Confidence
High
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSyatem.API/Extensions/PermissionPolicyExtensions.cs
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/Data/AppDbContext.cs
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Domain/Abstractions/AuditableEntity.cs
