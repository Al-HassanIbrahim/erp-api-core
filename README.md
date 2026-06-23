## High-Level Architecture
The solution follows a variant of **Clean Architecture** (or Onion Architecture / N-Tier Architecture), separated into four distinct layers. The core is the Domain layer, with Application and Infrastructure layers depending on it. The API layer acts as the presentation boundary and composition root, aggregating the Application and Infrastructure logic.

## Solution Structure & Main Projects
The workspace consists of the following project components:

1. **`ERPSyatem.API`** (Presentation / Host)
   * The startup project and main entry point. *(Note: There appears to be a typo in the directory name "ERPSyatem" as opposed to "ERPSystem")*
   * Responsible for configuring the hosting environment, dependency injection (DI) container, middleware pipeline, and web endpoints.
2. **`ERPSystem.Infrastructure`** (Data / External Concerns)
   * Houses database access logic (Entity Framework Core), repositories, identity management, and external service adaptations.
   * Depends on `ERPSystem.Application` and `ERPSystem.Domain`.
3. **`ERPSystem.Application`** (Business Use Cases)
   * Contains Application Services, Interfaces, and DTO/Mapping abstractions.
   * Depends purely on `ERPSystem.Domain`.
4. **`ERPSystem.Domain`** (Enterprise Business Rules)
   * The core of the application containing domain models, value objects, and abstractions.
   * Has no dependencies on other projects.

## Startup Flow (`Program.cs`)
The startup process utilizes the modern .NET 8 Top-Level Statements/Minimal Hosting model:
1. **Host Setup:** `WebApplication.CreateBuilder(args)` initializes configuration.
2. **Database:** `AppDbContext` is registered with SQL Server provider using the `DefaultConnection` string.
3. **Dependency Injection:** Services and Repositories are registered broadly with the `Scoped` lifetime across distinct boundaries (Core, Product, Inventory, HR, Sales, Contact, Expenses, CRM).
4. **Swagger / OpenAPI:** Adds security definitions enforcing Bearer (JWT) token utilization in the UI.
5. **Security & Identity:** Registers `ApplicationUser` mapped to `AppDbContext`, followed by standard JWT bearer token validation mappings. 
6. **Authorization Policies:** Custom policy extensions are injected via `AddPermissionPolicies()`.
7. **CORS:** Configuration of a permissive policy ("MyPolicy") allowing any origin, method, and header.
8. **Pipeline Build:** `app.Build()` finalizes the service collection.
9. **Data Seeding:** Runs `DbSeeder.SeedModulesAsync(context)` against the `AppDbContext` immediately upon successful build prior to accepting requests.
10. **Middleware Pipeline:** Environment checks for Development (enabling Swagger UI), exception handling (`UseExceptionHandling()`), CORS, Authentication, Authorization, tracking to Controller mapping, and finally `app.Run()`.