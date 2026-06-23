# 03_MODULE_CORE

## 1. Module Overview
The Core module is the foundational tenant-management layer of the ERP system. It handles multi-tenancy configurations by managing `Company` profiles, defining available system `Modules` (e.g., Sales, HR), toggling `CompanyModule` subscriptions, and managing internal company user accounts and roles.

---

## 2. Domain Entities

### `Company` (Base: `AuditableEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key |
| `Name` | `string` | Yes | Company display name |
| `CommercialName` | `string?` | No | Legal/commercial name |
| `TaxNumber` | `string?` | No | Tax Identification Number |
| `Phone` | `string?` | No | Contact number |
| `Address` | `string?` | No | Physical address |
| `IsActive` | `bool` | Yes | Default: `true` |
| `CompanyModules` | `ICollection<CompanyModule>`| - | Navigation |

### `Module` (Base: `BaseEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key |
| `Key` | `string` | Yes | Unique identifier (e.g., "SALES") |
| `Name` | `string` | Yes | Display name |
| `Description`| `string?` | No | Detail of module features |
| `IsActive` | `bool` | Yes | Default: `true` |
| `CompanyModules`| `ICollection<CompanyModule>`| - | Navigation |

### `CompanyModule` (Base: `AuditableEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key |
| `CompanyId` | `int` | Yes | FK to `Company` |
| `ModuleId` | `int` | Yes | FK to `Module` |
| `IsEnabled` | `bool` | Yes | Default: `true` |
| `EnabledAt` | `DateTime` | Yes | Timestamp of activation |
| `ExpiresAt` | `DateTime?`| No | Subscription expiration |

---

## 3. Repository Interfaces

### `ICompanyRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetByIdAsync` | `int id, CancellationToken ct` | `Task<Company?>` |
| `GetByIdTrackingAsync` | `int id, CancellationToken ct` | `Task<Company?>` |
| `GetAllAsync` | `CancellationToken ct` | `Task<List<Company>>` |
| `CodeExistsAsync` | `string code, int? excludeId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `Company company, CancellationToken ct` | `Task` |
| `Update` | `Company company` | `void` |
| `SoftDeleteAsync` | `int id, Guid deletedByUserId, CancellationToken ct`| `Task` |
| `ExistsAsync` | `int id, CancellationToken ct` | `Task<bool>` |
| `SaveChangesAsync` | `CancellationToken ct` | `Task` |

### `IModuleRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetByIdAsync` | `int id, CancellationToken ct` | `Task<Module?>` |
| `GetByKeyAsync` | `string key, CancellationToken ct` | `Task<Module?>` |
| `GetAllAsync` | `CancellationToken ct` | `Task<List<Module>>` |
| `KeyExistsAsync` | `string key, int? excludeId, CancellationToken ct` | `Task<bool>` |
| `AddAsync` | `Module module, CancellationToken ct` | `Task` |
| `Update` | `Module module` | `void` |
| `SoftDeleteAsync`| `int id, CancellationToken ct` | `Task` |
| `SaveChangesAsync`| `CancellationToken ct` | `Task` |

### `ICompanyModuleRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetAsync` | `int companyId, int moduleId, CancellationToken ct`| `Task<CompanyModule?>` |
| `GetByCompanyAsync`| `int companyId, CancellationToken ct` | `Task<List<CompanyModule>>`|
| `IsModuleEnabledAsync`|`int companyId, string moduleCode, CancellationToken ct`|`Task<bool>` |
| `EnableAsync` | `int companyId, int moduleId, Guid actorUserId, CancellationToken ct`| `Task` |
| `DisableAsync` | `int companyId, int moduleId, Guid actorUserId, CancellationToken ct`| `Task` |
| `SaveChangesAsync`| `CancellationToken ct` | `Task` |

---

## 4. Service Interfaces

### `ICompanyProfileService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetMyCompanyAsync` | `CancellationToken ct` | `Task<CompanyMeDto?>` | Uses `ICurrentUserService` for context |
| `UpdateMyCompanyAsync` | `UpdateCompanyMeDto dto, CancellationToken ct`| `Task<CompanyMeDto>` | Tracks `UpdatedByUserId` |

### `IModuleService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetAllAsync` | `CancellationToken ct` | `Task<IReadOnlyList<ModuleDto>>`| - |
| `CreateAsync` | `CreateModuleDto dto, CancellationToken ct` | `Task<ModuleDto>` | Validates unique Key |

### `ICompanyModuleService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetMyCompanyModulesAsync`| `CancellationToken ct` | `Task<IReadOnlyList<CompanyModuleDto>>` | Maps against ALL existing modules |
| `ToggleModuleAsync` | `int moduleId, bool isEnabled, CancellationToken ct` | `Task<CompanyModuleDto>` | Upserts `CompanyModule` record |

### `IModuleAccessService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `IsModuleEnabledAsync` | `int companyId, string moduleCode, CancellationToken ct` | `Task<bool>` | Base check |
| `Is[Module]EnabledAsync` | `CancellationToken ct` | `Task<bool>` | Helpers for: Sales, Inventory, Contact, Expenses, Crm, Hr |
| `Ensure[Module]EnabledAsync`| `CancellationToken ct` | `Task` | Throws `BusinessException` if false |

### `ICompanyUserService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetAllAsync` | `CancellationToken ct` | `Task<IReadOnlyList<CompanyUserDto>>`| Uses scoped company |
| `CreateAsync` | `CreateCompanyUserDto dto, CancellationToken ct` | `Task<CompanyUserDto>` | Checks Email uniqueness |
| `UpdateStatusAsync`| `Guid userId, UpdateUserStatusDto dto, CancellationToken ct` | `Task<CompanyUserDto>` | Toggles Identity Lockout |
| `AssignRoleAsync` | `Guid userId, string roleDisplayName, CancellationToken ct` | `Task<CompanyUserDto>` | Scopes role via `RoleKey` utility |
| `RemoveRoleAsync` | `Guid userId, string roleDisplayName, CancellationToken ct` | `Task<CompanyUserDto>` | Removes scoped role |
| `UpdateProfileAsync`| `Guid userId, AdminUpdateUserProfileDto dto, CancellationToken ct`| `Task<CompanyUserDto>`| Admin profile override |
| `DeleteAsync` | `Guid userId, CancellationToken ct` | `Task` | Soft delete + Identity Lockout |

---

## 5. DTOs & View Models

*Inferred fields from interface usage and service implementation mapping logic.*

### Request DTOs
| DTO | Fields |
| :--- | :--- |
| `UpdateCompanyMeDto` | `Name` (string), `CommercialName` (string?), `TaxNumber` (string), `Phone` (string?), `Address` (string?) |
| `CreateModuleDto` | `Key` (string), `Name` (string), `Description` (string?), `IsActive` (bool) |
| `ToggleCompanyModuleDto`| `IsEnabled` (bool) |
| `CreateCompanyUserDto` | `Email` (string), `FullName` (string), `Password` (string), `PhoneNumber` (string?), `Roles` (IEnumerable<string>?) |
| `UpdateUserStatusDto` | `IsActive` (bool) |
| `UserRoleAssignmentRequest`| `RoleName` (string) |
| `UserRoleRemovalRequest` | `RoleName` (string) |
| `AdminUpdateUserProfileDto`| `FullName` (string?), `PhoneNumber` (string?), `Email` (string?) |

### Response DTOs
| DTO | Fields |
| :--- | :--- |
| `CompanyMeDto` | `Id` (int), `Name` (string), `CommercialName` (string?), `TaxNumber` (string?), `Phone` (string?), `Address` (string?), `IsActive` (bool), `CreatedAt` (DateTime) |
| `ModuleDto` | `Id` (int), `Key` (string), `Name` (string), `Description` (string?), `IsActive` (bool) |
| `CompanyModuleDto` | `ModuleId` (int), `ModuleKey` (string), `ModuleName` (string), `IsEnabled` (bool), `EnabledAt` (DateTime?), `ExpiresAt` (DateTime?) |
| `CompanyUserDto` | `Id` (Guid), `Email` (string), `FullName` (string), `PhoneNumber` (string?), `IsActive` (bool), `IsLockedOut` (bool), `Roles` (List<string>) |

---

## 6. API Endpoints

### `CompaniesController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/companies/me` | Policy: `core.companies.read` | - | `CompanyMeDto` | Gets current user's company profile |
| PUT | `/api/companies/me` | Policy: `core.companies.update` | `UpdateCompanyMeDto` | `CompanyMeDto` | Updates current user's company profile |

### `CompanyModulesController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/company-modules` | Policy: `core.modules.read` | - | `List<CompanyModuleDto>` | Lists all modules with current company enable state |
| PUT | `/api/company-modules/{moduleId}` | Policy: `core.modules.manage` | `ToggleCompanyModuleDto`| `CompanyModuleDto` | Toggles module enable/disable for company |

### `ModulesController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/modules` | JWT Token | - | `List<ModuleDto>` | Lists all global modules available in system |

### `CompanyUsersController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/company-users` | Policy: `core.users.read` | - | `List<CompanyUserDto>`| Lists active users in company |
| POST | `/api/company-users` | Policy: `core.users.create` | `CreateCompanyUserDto` | `CompanyUserDto` | Creates user, assigns scoped roles |
| PUT | `/api/company-users/{userId:guid}/status` | Policy: `core.users.update` | `UpdateUserStatusDto` | `CompanyUserDto` | Locks/Unlocks user |
| POST | `/api/company-users/{userId:guid}/roles/assign`| Policy: `core.users.update`| `UserRoleAssignmentRequest`| `CompanyUserDto` | Adds scoped role to user |
| POST | `/api/company-users/{userId:guid}/roles/remove`| Policy: `core.users.update`| `UserRoleRemovalRequest`| `CompanyUserDto` | Removes scoped role from user |
| PUT | `/api/company-users/{userId:guid}/profile`| Policy: `core.users.update`| `AdminUpdateUserProfileDto`| `CompanyUserDto`| Updates user profile via admin |
| DELETE | `/api/company-users/{userId:guid}` | Policy: `core.users.delete`| - | `204 NoContent` | Soft deletes and locks user |

---

## 7. Business Rules & Validation

1. **Company Access Isolation:** Almost all services rely heavily on `ICurrentUserService.CompanyId` to automatically filter results (`GetByCompanyAsync`, `GetAllAsync` for Users).
2. **Self-Lock/Delete Protection:** A user cannot lock (`IsActive = false`) or Soft Delete their own account (`userId == _currentUser.UserId`).
3. **Email Uniqueness:** User emails must be unique globally, checked explicitly against `!u.IsDeleted` records before creation/update.
4. **Role Scoping (`RoleKey`)**:
   * Users are assigned roles using the format `c:{companyId}:{roleName}`.
   * `CompanyUserService` automatically prefixes and strips this identifier when reading/writing roles.
   * Attempting to assign a role requires a check against `RoleManager` to ensure the specific `c:{id}:{name}` role exists.
5. **Module Activation Constraints:** You cannot enable a `Module` for a company if the global `Module.IsActive` flag is `false`.
6. **Soft Delete Behavior:**
   * Deleting a user sets `IsDeleted = true`, records `DeletedAt`, and immediately triggers `UserManager.SetLockoutEnabledAsync` to permanently block login.

---

## 8. Cross-Module Dependencies

* **Infrastructure Layer (Identity):** `CompanyUserService` utilizes standard ASP.NET Core `UserManager<ApplicationUser>` and `RoleManager<IdentityRole<Guid>>`.
* **Security Extraction:** Relies heavily on `ICurrentUserService` for identifying the executing user context (`UserId`, `CompanyId`).
* **RoleKey:** Relies on `ERPSystem.Application.Authorization.RoleKey` for multi-tenant role formatting.