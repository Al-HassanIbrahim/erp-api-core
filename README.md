<div align="center">

<!-- Logo Placeholder -->
<img src="https://via.placeholder.com/150x150.png?text=ERP+LOGO" alt="ERP System Logo" width="150" height="150" style="border-radius: 16px;" />

# 🏢 ERP API Core

### *A modular, production-ready Enterprise Resource Planning system built on Clean Architecture*

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=for-the-badge&logo=github-actions)](https://github.com/Al-HassanIbrahim/erp-api-core/actions)
[![Version](https://img.shields.io/badge/version-1.0.0-blue?style=for-the-badge)](https://github.com/Al-HassanIbrahim/erp-api-core/releases)
[![License](https://img.shields.io/badge/license-MIT-green?style=for-the-badge)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen?style=for-the-badge)](CONTRIBUTING.md)
[![Status](https://img.shields.io/badge/status-active-success?style=for-the-badge)]()
[![Stars](https://img.shields.io/github/stars/Al-HassanIbrahim/erp-api-core?style=for-the-badge)](https://github.com/Al-HassanIbrahim/erp-api-core/stargazers)

</div>

---

## 📋 Table of Contents

- [🌐 Overview](#-overview)
- [✨ Features](#-features)
- [🛠️ Tech Stack](#-tech-stack)
- [🏛️ System Architecture](#-system-architecture)
- [🚀 Getting Started](#-getting-started)
- [📁 Project Structure](#-project-structure)
- [📸 Screenshots & Demo](#-screenshots--demo)
- [📖 API Documentation](#-api-documentation)
- [🗺️ Roadmap](#-roadmap)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [📬 Contact & Support](#-contact--support)

---

## 🌐 Overview

**ERP API Core** is a fully modular, multi-tenant Enterprise Resource Planning backend system built with **ASP.NET Core 8** and **Clean Architecture**. It provides a robust, scalable foundation for managing core business operations — from sales and procurement to HR, inventory, and financial reporting — all through a secure, JWT-authenticated REST API.

Designed from the ground up for **Egyptian and regional businesses**, the system supports bilingual output (Arabic/English), multi-tenancy with strict company-level data isolation, and a flexible role-based permission model that maps naturally to real-world enterprise access control.

### 🧩 Business Modules

| Module | Description |
|--------|-------------|
| 🛒 **Sales** | Invoices, deliveries, receipts, returns, customer management |
| 📦 **Inventory** | Products, units, warehouses, stock movements, adjustments |
| 🛍️ **Purchasing** | Purchase orders, vendor invoices, GRN, returns |
| 👥 **HR & Payroll** | Employees, departments, attendance, leave, payroll runs |
| 🤝 **CRM** | Customers, contacts, activities, pipeline |
| 📊 **Reporting** | PDF/Excel exports, financial statements, module dashboards |
| 🔐 **Identity & Access** | Multi-tenant auth, JWT, role-based permission policies |

---

## ✨ Features

### 💰 Finance & Accounting
- ✅ Full double-entry general ledger with automatic journal posting
- ✅ Configurable chart of accounts (asset, liability, equity, income, expense)
- ✅ Financial period management with open/close controls
- ✅ Balance sheet, income statement, and trial balance generation

### 🛒 Sales Management
- ✅ Complete sales cycle: Quote → Invoice → Delivery → Receipt → Return
- ✅ Multi-line invoices with per-line discount, tax, and unit pricing
- ✅ Payment tracking with partial payment and allocation support
- ✅ Bilingual PDF export (Arabic RTL / English LTR) using QuestPDF

### 📦 Inventory & Warehousing
- ✅ Multi-warehouse stock management with real-time availability
- ✅ Product catalog with unit-of-measure conversion
- ✅ Stock movements linked to sales, purchase, and manual adjustments
- ✅ Low-stock alerts and reorder point tracking

### 🛍️ Purchasing
- ✅ Vendor management with payment terms and tax profiles
- ✅ Purchase orders with approval workflow and GRN matching
- ✅ Vendor invoice 3-way matching (PO → GRN → Invoice)
- ✅ Purchase return processing with credit note generation

### 👥 HR & Payroll
- ✅ Employee master with departments, positions, and contract types
- ✅ Attendance tracking with overtime and shift rules
- ✅ Leave management with accrual policies and approval flow
- ✅ Automated payroll engine with deductions, taxes, and payslip generation

### 🔐 Identity & Security
- ✅ JWT Bearer authentication with refresh token rotation
- ✅ Policy-based authorization (`Permissions.Module.Entity.Action`)
- ✅ Full multi-tenancy: every entity is `CompanyId`-scoped
- ✅ Global audit log capturing all create/update/delete events
- ✅ Soft-delete on all entities — nothing is permanently lost

### 📊 Reporting & Exports
- ✅ PDF export for all sales documents (QuestPDF, embedded fonts)
- ✅ Excel export for reports and data grids (ClosedXML)
- ✅ Swagger UI with full JWT integration for live API testing
- ✅ Structured logging via Serilog with per-tenant correlation

---

## 🛠️ Tech Stack

### Backend
| Technology | Version | Purpose |
|-----------|---------|---------|
| ![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-purple?logo=dotnet) | 8.0 | Web API framework |
| ![EF Core](https://img.shields.io/badge/EF_Core-8.0-blue?logo=dotnet) | 8.0 | ORM & migrations |
| ![Identity](https://img.shields.io/badge/ASP.NET_Identity-8.0-blueviolet) | 8.0 | User management |
| ![JWT](https://img.shields.io/badge/JWT-Bearer-orange?logo=jsonwebtokens) | - | Authentication |
| ![MediatR](https://img.shields.io/badge/MediatR-CQRS-lightblue) | Latest | CQRS / Mediator |
| ![Hangfire](https://img.shields.io/badge/Hangfire-Background_Jobs-red) | Latest | Background processing |
| ![Serilog](https://img.shields.io/badge/Serilog-Structured_Logging-green) | Latest | Logging |

### Database
| Technology | Purpose |
|-----------|---------|
| ![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver) | Primary relational database |
| ![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis) | Distributed caching & rate limiting |

### Reporting & Export
| Technology | Purpose |
|-----------|---------|
| ![QuestPDF](https://img.shields.io/badge/QuestPDF-PDF_Generation-blue) | Bilingual PDF document generation |
| ![ClosedXML](https://img.shields.io/badge/ClosedXML-Excel_Export-green) | Excel file generation (MIT) |

### DevOps
| Technology | Purpose |
|-----------|---------|
| ![Docker](https://img.shields.io/badge/Docker-Containerization-2496ED?logo=docker) | Containerized deployment |
| ![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-2088FF?logo=githubactions) | CI/CD pipeline |
| ![Swagger](https://img.shields.io/badge/Swagger-API_Docs-85EA2D?logo=swagger) | API documentation |

---

## 🏛️ System Architecture

The system follows **Clean Architecture** with a strict unidirectional dependency rule:

```
Domain ← Application ← Infrastructure ← API
```

Each layer has a clear responsibility:

| Layer | Project | Responsibility |
|-------|---------|---------------|
| **Domain** | `ERPSystem.Domain` | Entities, interfaces, enums, value objects — zero framework dependencies |
| **Application** | `ERPSystem.Application` | Business logic, service interfaces, DTOs, permission constants, validators |
| **Infrastructure** | `ERPSystem.Infrastructure` | EF Core, repositories, Identity, PDF/Excel generation, external services |
| **API** | `ERPSyatem.API` | Controllers, middleware, DI composition root, Swagger config |

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         HTTP Clients / Swagger                       │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│                          ERPSyatem.API                               │
│     Controllers │ Middleware │ Extensions │ Program.cs               │
│     JWT Auth │ Global Exception Handler │ Rate Limiting              │
└───────────────────────────────┬─────────────────────────────────────┘
                                │
┌───────────────────────────────▼─────────────────────────────────────┐
│                       ERPSystem.Application                          │
│     Service Interfaces │ Service Implementations │ DTOs              │
│     Permissions │ Validators │ Business Exceptions                   │
└─────────────────┬─────────────────────────────┬────────────────────-┘
                  │                             │
┌─────────────────▼──────────┐  ┌──────────────▼────────────────────-┐
│     ERPSystem.Domain       │  │     ERPSystem.Infrastructure        │
│  Entities │ Interfaces     │  │  DbContext │ Repositories           │
│  Enums │ Base Classes      │  │  Identity │ PDF/Excel Generation    │
│  Value Objects             │  │  Migrations │ External Services     │
└────────────────────────────┘  └────────────────────────────────────-┘
                                              │
                                ┌─────────────▼──────────────────────┐
                                │     SQL Server + Redis             │
                                └────────────────────────────────────┘
```

### Key Design Decisions

- **Services never touch `DbContext` directly** — all data access flows through repository interfaces defined in the Domain layer
- **Controllers never contain business logic** — they delegate exclusively to injected Application services
- **Multi-tenancy** is enforced at query level via `ICurrentUserService.CompanyId` read from JWT claims — no cross-tenant data leakage is possible
- **Soft deletes** are applied on all entities via `BaseEntity.IsDeleted` — queries always filter `IsDeleted == false` unless explicitly fetching deleted records

---

## 🚀 Getting Started

### Prerequisites

Ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [SQL Server 2019+](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server in Docker](https://hub.docker.com/_/microsoft-mssql-server)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) (optional but recommended)
- [Git](https://git-scm.com/)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# Dev Kit

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/Al-HassanIbrahim/erp-api-core.git
cd erp-api-core
```

**2. Configure environment variables**

```bash
cp .env.example .env
# Edit .env with your actual values
```

**3. Apply database migrations**

```bash
cd src/ERPSystem.Infrastructure
dotnet ef database update --startup-project ../ERPSyatem.API
```

**4. Run the application**

```bash
cd src/ERPSyatem.API
dotnet run
```

The API will be available at `https://localhost:5001` and Swagger UI at `https://localhost:5001/swagger`.

---

### 🐳 Running with Docker

**Start all services (API + SQL Server + Redis)**

```bash
docker-compose up --build
```

**Run in detached mode**

```bash
docker-compose up -d
```

**Stop all services**

```bash
docker-compose down
```

---

### ⚙️ Environment Variables

Create a `.env` file in the root directory based on the example below:

```env
# ─── Database ────────────────────────────────────────────────
CONNECTION_STRING=Server=localhost,1433;Database=ERPCoreDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True

# ─── JWT Authentication ──────────────────────────────────────
JWT_SECRET=your-very-secret-key-minimum-32-characters-long
JWT_ISSUER=erp-api-core
JWT_AUDIENCE=erp-clients
JWT_EXPIRE_MINUTES=60
JWT_REFRESH_EXPIRE_DAYS=30

# ─── Redis Cache ─────────────────────────────────────────────
REDIS_CONNECTION=localhost:6379

# ─── App Settings ────────────────────────────────────────────
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://+:5001;http://+:5000
SEED_ADMIN_EMAIL=admin@erp.local
SEED_ADMIN_PASSWORD=Admin@123456

# ─── QuestPDF ────────────────────────────────────────────────
QUESTPDF_LICENSE=Community
```

---

### 🔧 Development vs Production

| Mode | Command | Notes |
|------|---------|-------|
| **Development** | `dotnet run --environment Development` | Swagger enabled, detailed errors, seeded demo data |
| **Production** | `dotnet run --environment Production` | Swagger disabled, HSTS enforced, structured logging to file |
| **Docker Dev** | `docker-compose -f docker-compose.dev.yml up` | Hot reload via volume mounts |
| **Docker Prod** | `docker-compose -f docker-compose.prod.yml up` | Optimized image, no dev tools |

---

## 📁 Project Structure

<details>
<summary>📂 Click to expand full folder tree</summary>

```
erp-api-core/
│
├── 📁 src/
│   │
│   ├── 📁 ERPSystem.Domain/                    # Pure domain layer
│   │   ├── 📁 Entities/
│   │   │   ├── 📁 Sales/                       # SalesInvoice, SalesDelivery, ...
│   │   │   ├── 📁 Purchasing/                  # PurchaseOrder, VendorInvoice, ...
│   │   │   ├── 📁 Inventory/                   # Product, Warehouse, StockMovement, ...
│   │   │   ├── 📁 HR/                          # Employee, Department, Payroll, ...
│   │   │   ├── 📁 Finance/                     # Account, JournalEntry, ...
│   │   │   └── 📁 Identity/                    # Company, ApplicationUser, ...
│   │   ├── 📁 Abstractions/
│   │   │   └── 📁 Repositories/                # All IRepository interfaces
│   │   ├── 📁 Enums/                           # SalesInvoiceStatus, PaymentStatus, ...
│   │   ├── 📁 Common/                          # BaseEntity, ICompanyEntity
│   │   └── 📁 ValueObjects/
│   │
│   ├── 📁 ERPSystem.Application/               # Business logic layer
│   │   ├── 📁 Interfaces/                      # ICurrentUserService, IPdfExportService, ...
│   │   ├── 📁 Services/                        # Service implementations
│   │   │   ├── 📁 Sales/
│   │   │   ├── 📁 Purchasing/
│   │   │   ├── 📁 Inventory/
│   │   │   ├── 📁 HR/
│   │   │   └── 📁 Finance/
│   │   ├── 📁 DTOs/                            # Request/Response DTOs per module
│   │   ├── 📁 Authorization/
│   │   │   └── Permissions.cs                  # All policy constants
│   │   ├── 📁 Exceptions/                      # BusinessException, BusinessErrors
│   │   ├── 📁 PdfGeneration/
│   │   │   └── PdfLabels.cs                    # Bilingual label dictionary
│   │   └── 📁 Validators/                      # FluentValidation validators
│   │
│   ├── 📁 ERPSystem.Infrastructure/            # Data & infrastructure layer
│   │   ├── 📁 Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── 📁 Configurations/              # EF fluent configurations
│   │   │   └── 📁 Migrations/
│   │   ├── 📁 Repositories/                    # Repository implementations
│   │   │   ├── 📁 Sales/
│   │   │   ├── 📁 Purchasing/
│   │   │   ├── 📁 Inventory/
│   │   │   ├── 📁 HR/
│   │   │   └── 📁 Finance/
│   │   ├── 📁 Identity/                        # UserService, JWT, RoleSeeder
│   │   ├── 📁 PdfGeneration/                   # QuestPDF service + components
│   │   │   ├── PdfExportService.cs
│   │   │   └── 📁 Components/
│   │   │       ├── CompanyHeaderComponent.cs
│   │   │       ├── DocumentMetaBlock.cs
│   │   │       ├── ItemsTableComponent.cs
│   │   │       ├── TotalsBlockComponent.cs
│   │   │       └── FooterComponent.cs
│   │   └── 📁 Services/                        # ICurrentUserService, IModuleAccessService
│   │
│   └── 📁 ERPSyatem.API/                       # HTTP surface layer
│       ├── 📁 Controllers/
│       │   ├── 📁 Sales/                       # SalesInvoicesController, ...
│       │   ├── 📁 Purchasing/
│       │   ├── 📁 Inventory/
│       │   ├── 📁 HR/
│       │   ├── 📁 Finance/
│       │   └── 📁 Auth/
│       ├── 📁 Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── TenantResolutionMiddleware.cs
│       ├── 📁 Extensions/                      # ServiceCollection extensions
│       └── Program.cs
│
├── 📁 tests/
│   ├── 📁 ERPSystem.UnitTests/
│   ├── 📁 ERPSystem.IntegrationTests/
│   └── 📁 ERPSystem.ArchitectureTests/
│
├── 📁 docs/
│   ├── architecture.md
│   ├── api-reference.md
│   └── deployment.md
│
├── 📄 docker-compose.yml
├── 📄 docker-compose.dev.yml
├── 📄 docker-compose.prod.yml
├── 📄 .env.example
├── 📄 .gitignore
├── 📄 .editorconfig
└── 📄 README.md
```

</details>

---

## 📸 Screenshots & Demo

> 📌 *Live demo and screenshots will be added upon first production deployment.*

<details>
<summary>📷 Preview Module Screenshots</summary>

### 🔐 Login & Authentication
![Auth Screenshot](https://via.placeholder.com/800x450.png?text=JWT+Auth+%2F+Login+Screen)
*JWT-based authentication with role and permission assignment per company*

---

### 🛒 Sales Invoice
![Sales Invoice Screenshot](https://via.placeholder.com/800x450.png?text=Sales+Invoice+Module)
*Multi-line sales invoice with tax, discount, payment tracking, and PDF export*

---

### 📦 Inventory Dashboard
![Inventory Screenshot](https://via.placeholder.com/800x450.png?text=Inventory+%2F+Stock+Management)
*Real-time stock levels across warehouses with movement history*

---

### 📊 Financial Reports
![Reports Screenshot](https://via.placeholder.com/800x450.png?text=Financial+Reports+%2F+PDF+Export)
*Balance sheet, income statement, and trial balance with bilingual PDF export*

---

### 👥 HR & Payroll
![HR Screenshot](https://via.placeholder.com/800x450.png?text=HR+%2F+Payroll+Module)
*Employee management, leave tracking, and automated payroll runs*

</details>

---

## 📖 API Documentation

The API is fully documented via **Swagger UI** (available in Development mode).

### Accessing Swagger

```
https://localhost:5001/swagger
```

> Authenticate by clicking **Authorize** → enter `Bearer {your_jwt_token}` to test protected endpoints.

### Postman Collection

[![Postman](https://img.shields.io/badge/Postman-Collection-FF6C37?style=for-the-badge&logo=postman)](https://www.postman.com/)

> 📌 *Postman collection will be published to the `/docs` folder. Import the file and set `{{base_url}}` and `{{token}}` variables.*

### Endpoint Overview

| Module | Base Path | Auth Required |
|--------|-----------|:-------------:|
| Auth | `/api/auth` | ❌ (login/register) |
| Companies | `/api/companies` | ✅ SuperAdmin |
| Sales Invoices | `/api/sales/invoices` | ✅ |
| Sales Deliveries | `/api/sales/deliveries` | ✅ |
| Sales Receipts | `/api/sales/receipts` | ✅ |
| Sales Returns | `/api/sales/returns` | ✅ |
| Purchase Orders | `/api/purchasing/orders` | ✅ |
| Products | `/api/inventory/products` | ✅ |
| Warehouses | `/api/inventory/warehouses` | ✅ |
| Employees | `/api/hr/employees` | ✅ |
| Payroll | `/api/hr/payroll` | ✅ |
| Accounts | `/api/finance/accounts` | ✅ |
| Journal Entries | `/api/finance/journals` | ✅ |

---

## 🗺️ Roadmap

### ✅ Completed

- [x] Clean Architecture foundation (Domain / Application / Infrastructure / API)
- [x] Multi-tenant JWT authentication with policy-based authorization
- [x] Sales module (Invoice, Delivery, Receipt, Return) with full CRUD
- [x] Inventory module (Products, Warehouses, Stock Movements)
- [x] Purchasing module (Purchase Orders, Vendor Invoices, GRN)
- [x] HR module (Employees, Departments, Leave Management)
- [x] Finance module (Chart of Accounts, Journal Entries, Periods)
- [x] Bilingual PDF export (Arabic RTL + English LTR) via QuestPDF
- [x] Excel export via ClosedXML
- [x] Global exception handling middleware
- [x] Audit trail (create/update/delete logging)
- [x] Soft delete across all entities

### 🔄 In Progress

- [ ] Payroll engine — automated deductions, tax calculation, payslip generation
- [ ] Background jobs via Hangfire (payroll runs, report generation, notifications)
- [ ] Rate limiting and API throttling per tenant
- [ ] Output caching for read-heavy endpoints (Redis)
- [ ] Architecture tests (dependency direction enforcement via NetArchTest)

### 📅 Upcoming

- [ ] CRM module (Customer pipeline, activities, tasks)
- [ ] Multi-currency support with exchange rate management
- [ ] E-invoicing integration (Egyptian Tax Authority ETA compliance)
- [ ] GraphQL endpoint (HotChocolate) for flexible client querying
- [ ] Webhook system for real-time event notifications
- [ ] Mobile-friendly API response optimization
- [ ] Two-factor authentication (2FA)
- [ ] Full Kubernetes deployment manifests
- [ ] SDK / client library generation via NSwag

---

## 🤝 Contributing

Contributions are welcome and greatly appreciated! Here's how to get involved:

### Fork & Branch

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/<your-username>/erp-api-core.git
git checkout -b feature/your-feature-name
# or
git checkout -b fix/issue-description
```

### Branch Naming Convention

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feature/module-feature` | `feature/hr-payroll-engine` |
| Bug Fix | `fix/issue-description` | `fix/invoice-tax-calculation` |
| Refactor | `refactor/description` | `refactor/repository-pattern` |
| Docs | `docs/description` | `docs/api-reference-update` |

### Pull Request Guidelines

1. **Ensure tests pass** — run `dotnet test` before submitting
2. **Follow the architecture** — all new code must respect the Clean Architecture layer boundaries
3. **No logic in controllers** — delegate everything to Application services
4. **Multi-tenancy first** — every new entity must implement `ICompanyEntity` and be `CompanyId`-scoped
5. **Soft delete required** — all entities inherit from `BaseEntity` and support `IsDeleted`
6. **Descriptive PR title** — e.g., `feat(sales): add bulk invoice export endpoint`
7. **Link related issues** — use `Closes #123` in the PR description

### Code Style

- Follow standard C# naming conventions (PascalCase for types/members, camelCase for locals)
- Use `var` for implicitly typed locals where the type is obvious
- All async methods must have `Async` suffix and accept `CancellationToken ct = default`
- Decimal currency values use `.ToString("N2")` — never raw `.ToString()`
- All new service/repository types must be registered with `AddScoped` in `Program.cs`
- Code comments must be in **English only**

<details>
<summary>💡 Areas where contributions are most needed</summary>

- [ ] Unit and integration test coverage (currently minimal)
- [ ] Architecture tests using NetArchTest
- [ ] Payroll engine implementation
- [ ] ETA e-invoicing integration
- [ ] Docker/Kubernetes production configuration
- [ ] CRM module implementation
- [ ] Performance benchmarks for large Excel exports

</details>

---

## 📄 License

[![MIT License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)](LICENSE)

```
MIT License

Copyright (c) 2026 Al-Hassan Ibrahim

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

> ⚠️ **QuestPDF License Note:** The PDF generation module uses QuestPDF under its Community License, which is free for individuals and companies with annual revenue under $1M. Enterprise deployments must acquire a [QuestPDF Professional or Enterprise license](https://www.questpdf.com/license/).

---

## 📬 Contact & Support

<div align="center">

**Al-Hassan Ibrahim**
*Backend Engineer · ASP.NET Core · Clean Architecture · AI/ML*

[![GitHub](https://img.shields.io/badge/GitHub-Al--HassanIbrahim-181717?style=for-the-badge&logo=github)](https://github.com/Al-HassanIbrahim)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0A66C2?style=for-the-badge&logo=linkedin)](https://linkedin.com/in/your-profile)
[![Email](https://img.shields.io/badge/Email-Contact-EA4335?style=for-the-badge&logo=gmail)](mailto:your.email@example.com)

</div>

---

### 🐛 Found a Bug?

Please [open an issue](https://github.com/Al-HassanIbrahim/erp-api-core/issues/new/choose) with:
- Clear description of the bug
- Steps to reproduce
- Expected vs actual behavior
- Environment details (.NET version, OS, DB version)

### 💡 Have a Feature Idea?

[Start a discussion](https://github.com/Al-HassanIbrahim/erp-api-core/discussions/new) or open a Feature Request issue. Ideas aligned with the roadmap are prioritized.

---

<div align="center">

**⭐ If this project helped you, please consider giving it a star — it means a lot!**

[![Star History](https://img.shields.io/github/stars/Al-HassanIbrahim/erp-api-core?style=social)](https://github.com/Al-HassanIbrahim/erp-api-core/stargazers)

*Built with ❤️ in Cairo, Egypt 🇪🇬*

</div>
