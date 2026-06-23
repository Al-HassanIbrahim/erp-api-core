# 01_AUTH_SECURITY

## 1. Identity Setup

The system uses ASP.NET Core Identity customized for multi-tenancy and soft-deletion.

* **Primary Key Type:** `Guid`
* **Base Classes:** `IdentityUser<Guid>`, `IdentityRole<Guid>`

### `ApplicationUser` Custom Properties
Extends `IdentityUser<Guid>` with the following fields:

| Property | Type | Description |
| :--- | :--- | :--- |
| `CompanyId` | `int` | Foreign key to the `Company` entity (Multi-tenancy isolation). |
| `Company` | `Company` | Navigation property. |
| `FullName` | `string` | User's full display name. |
| `ProfileImageUrl` | `string?` | Optional avatar URL. |
| `IsDeleted` | `bool` | Soft-delete flag (default: `false`). |
| `DeletedAt` | `DateTime?` | Timestamp of soft-delete. |
| `DeletedByUserId` | `Guid?` | Track who deleted the user. |

---

## 2. JWT Configuration

### Config Keys (`appsettings.json`)
```json
"JWT": {
    "Key": "...",
    "IssuerIP": "...",
    "AudienceIP": "...",
    "ExpireMinutes": 120
}

```

### TokenValidationParameters

Configured in `Program.cs` via `AddJwtBearer`:

| Parameter | Value | Purpose |
| --- | --- | --- |
| `ValidateIssuer` | `true` | Validates against `JWT:IssuerIP`. |
| `ValidateAudience` | `true` | Validates against `JWT:AudienceIP`. |
| `ValidateLifetime` | `true` | Ensures token is not expired. |
| `ClockSkew` | `TimeSpan.Zero` | Strict expiration (no 5-minute default grace period). |
| `ValidateIssuerSigningKey` | `true` | Verifies signature using `JWT:Key`. |
| `SaveToken` | `true` | Allows retrieval of the raw token later in the pipeline. |

---

## 3. `IJwtTokenService`

Responsible for minting the JWT and injecting the custom payload.

| Method | Parameters | Returns |
| --- | --- | --- |
| `CreateToken` | `Guid userId`<br>

<br>`int companyId`<br>

<br>`string email`<br>

<br>`string[] roles`<br>

<br>`string[] permissions` | `AuthResponse` |

**Injected Claims:** `sub` (UserId), `email`, `companyId` (Custom multi-tenant claim), `role` (Array), `permission` (Array).

---

## 4. `IAuthService`

Handles authentication workflows and role/permission resolution.

| Method | Parameters | Returns |
| --- | --- | --- |
| `RegisterOwnerAsync` | `RegisterOwnerRequest request`, `CancellationToken ct` | `Task<AuthResponse>` |
| `LoginAsync` | `LoginRequest request`, `CancellationToken ct` | `Task<AuthResponse>` |

---

## 5. `ICurrentUserService`

Extracts the current user context directly from the HTTP Context claims. Used by Application services for data isolation.

| Property | Type | Source Claim | Default if Missing |
| --- | --- | --- | --- |
| `UserId` | `Guid` | `sub` | `Guid.Empty` |
| `CompanyId` | `int` | `companyId` | `0` |

---

## 6. Permission System

The system uses **Policy-Based Authorization** driven by granular permission strings.

### Mechanics

1. **Definition:** All permissions are defined as string constants in the static `Permissions` class (e.g., `Permissions.Sales.Invoices.Read = "sales.invoices.read"`).
2. **Registration:** `AddPermissionPolicies()` uses reflection to scan the `Permissions` class on startup. It dynamically generates an ASP.NET Authorization Policy for *every* constant field.
3. **Handler:** `PermissionHandler` evaluates `PermissionRequirement`. It succeeds if the current user possesses a claim of type `"permission"` that exactly matches the required policy string.

### Controller Application

Policies are enforced at the Controller/Action level using standard `[Authorize]` attributes referencing the policy string.

```csharp
// Example usage (Do not implement, context only)
[Authorize(Policy = Permissions.Sales.Invoices.Read)]
[HttpGet]
public async Task<IActionResult> GetInvoices() { ... }

```

---

## 7. Roles (Multi-Tenant Scoping)

Roles are **company-scoped**. A standard system might just use "Admin", but to prevent Cross-Tenant privilege escalation, the `RoleKey` utility formats roles in the database.

* **Format in Database:** `c:{companyId}:{roleDisplayName}` (e.g., `c:12:Owner`, `c:12:HR_Manager`)
* **RoleKey Utility Methods:**
* `ForCompany(int companyId, string roleDisplayName)` → Creates the DB string.
* `TryParse(...)` → Extracts ID and Name.
* `GetDisplayName(...)` → Strips the prefix for the UI/Token.


* **Token Output:** When `AuthService` generates the token, it strips the `c:{id}:` prefix so the client only sees `"Owner"`.
* **Permissions:** Roles do not grant access directly in controllers. Roles merely act as collections of `permission` claims inside the database (`AspNetRoleClaims`).

---

## 8. Auth Flow Sequence

1. **Login Request:** Client posts credentials to `/api/auth/login`.
2. **Validation:** `AuthService.LoginAsync` uses `UserManager.CheckPasswordAsync`. Checks lockout status and handles failed attempt counts.
3. **Claim Gathering:** `AuthService` fetches user's roles (`UserManager.GetRolesAsync`) and all associated permission claims (`RoleManager.GetClaimsAsync`).
4. **Token Generation:** `JwtTokenService` creates the JWT, embedding `companyId`, `role` strings (prefix stripped), and flat array of `permission` strings.
5. **Token Validation:** On subsequent requests, `JwtBearer` middleware validates the signature and lifetime.
6. **Authorization Check:** `[Authorize(Policy = "...")]` triggers the `PermissionHandler` to ensure the specific `permission` claim exists in the token.
7. **Service Execution:** Controllers pass requests to Services, which use `ICurrentUserService.CompanyId` to automatically filter all DB queries (Tenant Isolation).

---

## 9. DTOs

*Inferred from `AuthService` implementation logic:*

### `LoginRequest`

* `string Email`
* `string Password`

### `RegisterOwnerRequest`

* `string Email`
* `string Password`
* `string FullName`
* `CompanyDto Company` (Contains `Name`, `Address`, `TaxNumber`, `CommercialName`, `PhoneNumber`)

### `AuthResponse`

* `string AccessToken`
* `DateTime ExpiresAtUtc`
* `Guid UserId`
* `int CompanyId`
* `string Email`
* `string[] Roles`
* `string[] Permissions`

---

## 10. Security Concerns & Notes

1. **Hardcoded Secrets:** `appsettings.json` contains a hardcoded JWT Key. This must be injected via environment variables or Azure KeyVault in Production.
2. **HTTPS Metadata:** Configuration options for `RequireHttpsMetadata = false` exist (commented out in Program.cs). Must ensure this defaults to `true` in Production environments to prevent token interception.
3. **CORS:** Currently configured to `AllowAnyOrigin()`. Highly vulnerable to cross-origin attacks; must restrict to specific SPA/Client domains in production.
4. **Database Role Tampering:** Because roles rely on the strict `c:{id}:{name}` naming convention, direct manual database edits to `AspNetRoles.Name` that bypass `RoleKey.cs` will break tenant isolation or authorization checks.
5. **Strict Expiration:** `ClockSkew = TimeSpan.Zero` means tokens expire *exactly* at the specified minute, without the ASP.NET default 5-minute grace period. Refresh token flow (if implemented later) must account for this strict cutoff.
