# 🏢 ERP System API

A comprehensive, multi-tenant **Enterprise Resource Planning (ERP)** backend built with **ASP.NET Core**, following **Clean Architecture** principles. This system provides a powerful REST API for managing the full lifecycle of a modern business — from HR and payroll to sales, purchasing, inventory, CRM, and accounting.

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Modules](#-modules)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [API Endpoints](#-api-endpoints)
- [Authentication & Authorization](#-authentication--authorization)
- [Multi-Tenancy](#-multi-tenancy)
- [Contributing](#-contributing)

---

## 🌐 Overview

The ERP System API is a scalable, multi-tenant SaaS backend designed to serve companies of all sizes. Each company is isolated within the platform with its own data, roles, permissions, and activated modules. The system is built around **domain-driven design**, with a clean separation of concerns across API, Application, Domain, and Infrastructure layers.

Key capabilities at a glance:

- **Multi-tenant**: Companies are fully isolated; each operates in its own data context.
- **Modular**: Modules (HR, CRM, Sales, etc.) can be enabled or disabled per company.
- **Role-based access control (RBAC)**: Fine-grained permission management at the company level.
- **JWT Authentication**: Secure, stateless auth using JWT tokens.
- **PDF Generation**: Export invoices, payrolls, and reports as PDFs.
- **Bulk Import**: Upload and process data via structured import files.

---

## 🏛️ Architecture

The solution follows a **Clean Architecture** (Onion Architecture) pattern, organized into four distinct layers:

```
┌──────────────────────────────────────────────────────────┐
│                    ERPSystem.API                         │
│           (Controllers, Middleware, DI Setup)            │
├──────────────────────────────────────────────────────────┤
│                ERPSystem.Application                     │
│     (Services, DTOs, Interfaces, Business Logic)         │
├──────────────────────────────────────────────────────────┤
│                  ERPSystem.Domain                        │
│        (Entities, Enums, Abstractions/Interfaces)        │
├──────────────────────────────────────────────────────────┤
│               ERPSystem.Infrastructure                   │
│    (EF Core DbContext, Repositories, Identity, PDF)      │
└──────────────────────────────────────────────────────────┘
```

Dependencies flow inward — the Domain has no external dependencies, and Infrastructure implements the interfaces defined in the Domain and Application layers.

---

## 🧩 Modules

| Module | Description |
|---|---|
| **Core** | Company management, module activation, user assignment |
| **Authentication** | JWT login, registration, role seeding |
| **HR** | Employees, departments, job positions, attendance, leave requests |
| **Payroll** | Payroll generation, line items, batch processing, payment marking |
| **CRM** | Leads management, sales pipelines, stage tracking |
| **Sales** | Customers, sales invoices, sales receipts, deliveries, returns |
| **Purchase** | Suppliers, purchase invoices, purchase returns, supplier payments |
| **Inventory** | Warehouses, stock items, inventory documents, inventory reports |
| **Products** | Product catalog, categories, units of measure |
| **Expenses** | Expense tracking, expense categories, expense statistics |
| **Contacts** | Shared contact book across modules |
| **Import** | Bulk data import with validation, parsing, and job tracking |
| **Permissions** | RBAC — roles, permissions, user-role assignments |

---

## 🛠️ Tech Stack

| Technology | Purpose |
|---|---|
| **ASP.NET Core** | Web API framework |
| **Entity Framework Core** | ORM / Data access |
| **ASP.NET Core Identity** | User management & authentication |
| **JWT Bearer Tokens** | Stateless API authentication |
| **Clean Architecture** | Structural pattern |
| **Repository + Unit of Work** | Data access abstraction |
| **QuestPDF / PDF Libraries** | PDF document generation |
| **Custom Middleware** | Global exception handling |

---

## 📁 Project Structure

```
ERPSystem-API/
│
├── ERPSystem.API/                        # Presentation Layer
│   ├── Controllers/                      # All API controllers (40+)
│   │   ├── AuthController.cs
│   │   ├── EmployeeController.cs
│   │   ├── SalesInvoicesController.cs
│   │   └── ...
│   ├── Extensions/
│   │   └── PermissionPolicyExtensions.cs
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── ERPSystem.Application/                # Application Layer
│   ├── Authorization/                    # Permission policies & role keys
│   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Authorization/
│   │   ├── Core/
│   │   ├── CRM/
│   │   ├── Expenses/
│   │   ├── HR/
│   │   │   ├── Attendance/
│   │   │   ├── Department/
│   │   │   ├── Employee/
│   │   │   ├── JobPosition/
│   │   │   ├── Leave/
│   │   │   └── Payroll/
│   │   ├── Import/
│   │   ├── Inventory/
│   │   ├── Products/
│   │   ├── Purchase/
│   │   └── Sales/
│   ├── Exceptions/                       # Custom business exceptions
│   ├── Interfaces/                       # Service contracts (30+)
│   └── Services/                        # Business logic implementations
│       ├── CRM/
│       ├── Expenses/
│       ├── Hr/
│       ├── Import/
│       ├── Inventory/
│       ├── Products/
│       ├── Purchase/
│       └── Sales/
│
├── ERPSystem.Domain/                     # Domain Layer
│   ├── Abstractions/                    # Repository interfaces & base classes
│   ├── Entities/                        # Domain entities
│   │   ├── Contacts/
│   │   ├── Core/
│   │   ├── CRM/
│   │   ├── Expenses/
│   │   ├── HR/
│   │   ├── Import/
│   │   ├── Inventory/
│   │   ├── Products/
│   │   ├── Purchase/
│   │   └── Sales/
│   └── Enums/                           # Domain enumerations
│
└── ERPSystem.Infrastructure/            # Infrastructure Layer
    ├── Data/
    │   ├── AppDbContext.cs              # EF Core DbContext
    │   ├── DbSeeder.cs                  # Seed data
    │   └── UnitOfWork.cs
    ├── Identity/                        # ASP.NET Identity integration
    │   ├── ApplicationUser.cs
    │   ├── AuthService.cs
    │   ├── JwtTokenService.cs
    │   └── ...
    ├── Repositories/                    # Repository implementations (30+)
    └── Shared/
        ├── DocumentSequenceService.cs
        └── PdfGeneration/              # PDF export components
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or configure another EF Core-compatible provider)
- Git

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/your-username/ERPSystem-API.git
cd ERPSystem-API
```

**2. Configure the database connection**

Update `appsettings.json` in `ERPSystem.API` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ERPSystemDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "ERPSystem",
    "Audience": "ERPSystemClients",
    "ExpiryMinutes": 60
  }
}
```

**3. Apply database migrations**

```bash
cd ERPSystem.API
dotnet ef database update
```

**4. Run the application**

```bash
dotnet run
```

The API will be available at `https://localhost:7000` (or as configured in `launchSettings.json`).

**5. Explore the API**

Swagger UI is available at:
```
https://localhost:7000/swagger
```

---

## 📡 API Endpoints

Below is a summary of the main endpoint groups. All routes are prefixed with `/api`.

### Authentication
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Login and receive JWT token |
| `POST` | `/api/auth/register` | Register a new user |

### Core / Company
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/companies` | List all companies |
| `POST` | `/api/companies` | Create a company |
| `GET/PUT` | `/api/companies/{id}` | Get / update a company |
| `GET/POST` | `/api/company-modules` | Manage module activations per company |
| `GET/POST` | `/api/company-users` | Manage users within a company |

### HR
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/employees` | List / create employees |
| `GET/PUT/DELETE` | `/api/employees/{id}` | Get / update / delete employee |
| `GET/POST` | `/api/departments` | Departments management |
| `GET/POST` | `/api/job-positions` | Job position management |
| `GET/POST` | `/api/attendance` | Log and view attendance |
| `GET/POST` | `/api/leave-requests` | Create and manage leave requests |

### Payroll
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/payroll/generate` | Generate payroll for a period |
| `GET` | `/api/payroll` | List payroll batches |
| `POST` | `/api/payroll/{id}/mark-paid` | Mark a payroll batch as paid |

### CRM
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/leads` | Manage sales leads |
| `POST` | `/api/leads/{id}/convert` | Convert a lead to a customer |
| `GET/POST` | `/api/pipeline` | Manage pipeline stages |
| `POST` | `/api/pipeline/{id}/move` | Move a lead to another stage |

### Sales
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/customers` | Customer management |
| `GET/POST` | `/api/sales-invoices` | Sales invoice management |
| `GET/POST` | `/api/sales-receipts` | Record payments against invoices |
| `GET/POST` | `/api/sales-deliveries` | Delivery management |
| `GET/POST` | `/api/sales-returns` | Handle sales returns |

### Purchase
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/suppliers` | Supplier management |
| `GET/POST` | `/api/purchase-invoices` | Purchase invoice management |
| `GET/POST` | `/api/purchase-returns` | Handle purchase returns |
| `GET/POST` | `/api/supplier-payments` | Record payments to suppliers |

### Inventory
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/warehouses` | Warehouse management |
| `GET/POST` | `/api/inventory` | Manage stock and movements |
| `GET` | `/api/inventory-reports` | Stock reports and valuation |

### Products
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/products` | Product catalog |
| `GET/POST` | `/api/categories` | Product categories |
| `GET/POST` | `/api/unit-of-measure` | Units of measure |

### Expenses
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/expenses` | Expense records |
| `GET/POST` | `/api/expense-categories` | Expense categories |
| `GET` | `/api/expense-stats` | Expense statistics and summaries |

### Access Control
| Method | Endpoint | Description |
|---|---|---|
| `GET/POST` | `/api/roles` | Role management |
| `GET/POST` | `/api/permissions` | Permission management |
| `GET/POST` | `/api/user-roles` | Assign roles to users |

### Utilities
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/import` | Bulk data import |
| `GET/PUT` | `/api/my-account` | Current user profile management |
| `GET/POST` | `/api/contacts` | Shared contacts |

---

## 🔐 Authentication & Authorization

The API uses **JWT Bearer Token** authentication.

**Obtaining a token:**

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "your-password"
}
```

**Using the token:**

Include the token in the `Authorization` header for all protected endpoints:

```http
Authorization: Bearer <your-jwt-token>
```

### Permissions System

Authorization is enforced through a **custom RBAC permission system**:

- Each **Company** has its own set of **Roles**.
- Each Role is assigned granular **Permissions** (e.g., `Employees.View`, `Invoices.Create`).
- Users are assigned Roles within a Company.
- The API validates permissions on every protected endpoint via custom **permission policies**.

---

## 🏗️ Multi-Tenancy

The system is designed as a **multi-tenant** platform:

- Each **Company** is a tenant, fully isolated from others.
- A `TenantGuard` pattern is applied in repositories to ensure queries are always scoped to the authenticated user's active company.
- Module access is controlled per company — administrators can enable or disable modules (e.g., CRM, Payroll) for their company from the `company-modules` endpoints.

---

## 📄 PDF Export

The system includes a built-in PDF generation service for:

- Sales Invoices
- Purchase Invoices
- Payroll slips
- Custom reports

PDFs are rendered server-side using a component-based generation pipeline (`DocumentMetaBlock`, `ItemsTableComponent`, `TotalsBlockComponent`, `FooterComponent`).

---

## 📥 Bulk Import

The Import module supports structured bulk data uploads:

- Upload a file via `/api/import`
- The system parses, validates each row, and processes records
- An `ImportJob` entity tracks the status and results of each import
- Supports products, employees, and other entity types

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/your-feature-name`)
3. Commit your changes (`git commit -m 'Add some feature'`)
4. Push to the branch (`git push origin feature/your-feature-name`)
5. Open a Pull Request

Please ensure your code follows the existing architecture patterns and that new services are registered through the appropriate DI configuration.

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

> Built with ❤️ using ASP.NET Core and Clean Architecture.
