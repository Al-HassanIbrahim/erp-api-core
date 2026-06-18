# Module Dependency Graph

## Overview
This document maps the architectural and modular dependencies of the ERP API Core.

---
## Responsibilities
Defines layer interaction constraints and cross-module couplings to ensure boundaries are respected.

---
## Workflow
The application is built on Clean Architecture:
1. **API Layer** references Application and Infrastructure to compose the final runtime.
2. **Infrastructure Layer** references Application (for Interfaces) and Domain (for Entities).
3. **Application Layer** references Domain.
4. **Domain Layer** is at the center and references nothing.

---
## Dependencies
### Layer Dependencies
- `ERPSyatem.API.csproj` -> `ERPSystem.Application.csproj`, `ERPSystem.Infrastructure.csproj`
- `ERPSystem.Infrastructure.csproj` -> `ERPSystem.Application.csproj`, `ERPSystem.Domain.csproj`
- `ERPSystem.Application.csproj` -> `ERPSystem.Domain.csproj`

### Module Dependencies (Inferred from common ERP practices and EF core relationships)
- **Inventory** -> Depends on **Products** (Stock items map to Products)
- **Sales** -> Depends on **Inventory** (Deliveries decrease stock), **Products** (Invoice lines reference items)
- **HR** -> Highly independent, depends on **Core** for users.
- **CRM** -> Depends on **Core** and **Sales** (Lead conversion to Customer).
- **Expenses** -> Highly independent.

---
## Public APIs
N/A

---
## Limitations
- Module separation is currently done via namespaces/folders inside the same monolithic assemblies rather than strictly isolated projects/assemblies per module.

---
## Technical Debt
- Lack of strict modular boundaries means a developer could easily reference a domain entity from another module directly, leading to tight coupling over time.

---
## AI Quick Context
### Depends On
Clean Architecture
### Uses
.NET Project References
### Used By
System Architects, Future AI
### Related Modules
All
### Shared Components
N/A
### Entities
N/A
### Business Rules
Domain must remain pure.
### Endpoints
N/A
### Future Refactoring
Break modules into individual assemblies if the monolith becomes too large.
### Modification Risk
Medium (Architectural rules are strict)
### Confidence
High (Verified from .csproj files and folder structure)
### Source Files
- /home/runner/work/erp-api-core/erp-api-core/ERPSyatem.API/ERPSyatem.API.csproj
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Application/ERPSystem.Application.csproj
- /home/runner/work/erp-api-core/erp-api-core/ERPSystem.Infrastructure/ERPSystem.Infrastructure.csproj
