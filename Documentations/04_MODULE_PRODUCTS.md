# 04_MODULE_PRODUCTS

## 1. Module Overview
The Products module is the core catalog-management layer of the ERP system[cite: 1, 10]. It handles multi-tenancy inventory definitions by managing `Product` records, establishing hierarchical `Category` structures, and standardizing physical metrics through `UnitOfMeasure` configurations[cite: 1].

---

## 2. Domain Entities

### `Product` (Base: `BaseEntity`, `ICompanyEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key[cite: 1, 10] |
| `Code` | `string` | Yes | Internal product code (SKU)[cite: 1] |
| `Name` | `string` | Yes | Display name[cite: 1] |
| `CompanyId` | `int` | Yes | FK to Tenant Company[cite: 1] |
| `Description` | `string?` | No | Detailed description[cite: 1] |
| `CategoryId` | `int?` | No | FK to `Category`[cite: 1] |
| `Category` | `Category?` | No | Navigation[cite: 1] |
| `UnitOfMeasureId`| `int` | Yes | FK to `UnitOfMeasure`[cite: 1] |
| `UnitOfMeasure` | `UnitOfMeasure`| Yes | Navigation[cite: 1] |
| `DefaultPrice` | `decimal`| Yes | Baseline selling price[cite: 1] |
| `MinQuantity` | `decimal?`| No | Low stock detection limit[cite: 1] |
| `Barcode` | `string?`| No | Barcode identifier[cite: 1] |
| `IsActive` | `bool` | Yes | Default: `true`[cite: 1] |

### `Category` (Base: `BaseEntity`, `ICompanyEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key[cite: 1, 10] |
| `Name` | `string` | Yes | Display name[cite: 1] |
| `CompanyId` | `int` | Yes | FK to Tenant Company[cite: 1] |
| `Description`| `string?` | No | Detailed description[cite: 1] |
| `ParentCategoryId` | `int?` | No | FK to parent `Category`[cite: 1] |
| `ParentCategory`| `Category?` | No | Navigation[cite: 1] |
| `Children` | `ICollection<Category>` | - | Navigation[cite: 1] |
| `Products` | `ICollection<Product>` | - | Navigation[cite: 1] |

### `UnitOfMeasure` (Base: `BaseEntity`, `ICompanyEntity`)
| Property | Type | Required | Description / FK |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | Yes | Primary Key[cite: 1, 10] |
| `Name` | `string` | Yes | Display name[cite: 1] |
| `CompanyId` | `int` | Yes | FK to Tenant Company[cite: 1] |
| `Symbol` | `string` | Yes | Short symbol (e.g., "kg")[cite: 1] |
| `Products` | `ICollection<Product>` | - | Navigation[cite: 1] |

---

## 3. Repository Interfaces

### `IProductRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetByIdAsync` | `int id` | `Task<Product?>`[cite: 2] |
| `GetByIdAsync` | `int id, int companyId` | `Task<Product?>`[cite: 2] |
| `GetAllAsync` | - | `Task<IEnumerable<Product>>`[cite: 2] |
| `GetAllByCompanyAsync` | `int companyId` | `Task<IEnumerable<Product>>`[cite: 2] |
| `ExistsAsync` | `int id, int companyId` | `Task<bool>`[cite: 2] |
| `CodeExistsAsync`| `string code, int companyId, int? excludeId = null` | `Task<bool>`[cite: 2] |
| `AddAsync` | `Product product` | `Task`[cite: 2] |
| `UpdateAsync` | `Product product` | `Task`[cite: 2] |
| `DeleteAsync` | `int id` | `Task`[cite: 2] |
| `SaveChangesAsync` | - | `Task`[cite: 2] |

### `ICategoryRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetByIdAsync` | `int id, CancellationToken ct = default` | `Task<Category?>`[cite: 2] |
| `GetAllByCompanyAsync` | `int companyId, CancellationToken ct = default` | `Task<List<Category>>`[cite: 2] |
| `AddAsync` | `Category category, CancellationToken ct = default` | `Task`[cite: 2] |
| `Update` | `Category category` | `void`[cite: 2] |
| `Delete` | `Category category` | `void`[cite: 2] |
| `SaveChangesAsync` | `CancellationToken ct = default` | `Task`[cite: 2] |

### `IUnitOfMeasureRepository`
| Method | Parameters | Return Type |
| :--- | :--- | :--- |
| `GetByIdAsync` | `int id, CancellationToken ct = default` | `Task<UnitOfMeasure?>`[cite: 2] |
| `GetAllByCompanyAsync` | `int companyId, CancellationToken ct = default` | `Task<List<UnitOfMeasure>>`[cite: 2] |
| `AddAsync` | `UnitOfMeasure unitOfMeasure, CancellationToken ct = default` | `Task`[cite: 2] |
| `Update` | `UnitOfMeasure unitOfMeasure` | `void`[cite: 2] |
| `Delete` | `UnitOfMeasure unitOfMeasure` | `void`[cite: 2] |
| `SaveChangesAsync` | `CancellationToken ct = default` | `Task`[cite: 2] |

---

## 4. Service Interfaces

### `IProductService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetByIdAsync` | `int id` | `Task<ProductDto?>` | Scopes to current company[cite: 4] |
| `GetAllAsync` | - | `Task<IEnumerable<ProductDto>>` | Fetches active company products[cite: 4] |
| `CreateAsync` | `CreateProductDto dto` | `Task<int>` | Enforces code uniqueness[cite: 4] |
| `UpdateAsync` | `UpdateProductDto dto` | `Task` | Prevents code duplication[cite: 4] |
| `DeleteAsync` | `int id` | `Task` | Marks product as `IsDeleted`[cite: 4] |

### `ICategoryService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetAllAsync` | `CancellationToken ct = default` | `Task<IReadOnlyList<CategoryDto>>` | Returns flat scoped categories[cite: 3] |
| `GetByIdAsync` | `int id, CancellationToken ct = default`| `Task<CategoryDto?>` | Verifies company ownership[cite: 3] |
| `CreateAsync` | `CreateCategoryRequest request, CancellationToken ct = default`| `Task<CategoryDto>` | Validates parent existence/ownership[cite: 3] |
| `UpdateAsync` | `int id, UpdateCategoryRequest request, CancellationToken ct = default`| `Task<CategoryDto>` | Prevents self-parenting[cite: 3] |
| `DeleteAsync` | `int id, CancellationToken ct = default`| `Task` | Marks `IsDeleted` on entity[cite: 3] |

### `IUnitOfMeasureService`
| Method | Parameters | Return Type | Notes |
| :--- | :--- | :--- | :--- |
| `GetAllAsync` | `CancellationToken ct = default` | `Task<IReadOnlyList<UnitOfMeasureDto>>` | Scoped to current company[cite: 5] |
| `GetByIdAsync` | `int id, CancellationToken ct = default`| `Task<UnitOfMeasureDto?>` | Verifies company ownership[cite: 5] |
| `CreateAsync` | `CreateUnitOfMeasureRequest request, CancellationToken ct = default`| `Task<UnitOfMeasureDto>` | Standardizes strings (Trim)[cite: 5] |
| `UpdateAsync` | `int id, UpdateUnitOfMeasureRequest request, CancellationToken ct = default`| `Task<UnitOfMeasureDto>` | Standardizes strings (Trim)[cite: 5] |
| `DeleteAsync` | `int id, CancellationToken ct = default`| `Task` | Marks `IsDeleted` on entity[cite: 5] |

---

## 5. DTOs & View Models

*Inferred fields from interface usage and service implementation mapping logic.*

### Request DTOs
| DTO | Fields |
| :--- | :--- |
| `CreateProductDto` | `Code` (string), `Name` (string), `Description` (string?), `CategoryId` (int?), `UnitOfMeasureId` (int), `DefaultPrice` (decimal), `MinQuantity` (decimal?), `Barcode` (string?)[cite: 6] |
| `UpdateProductDto` | `Id` (int), `Code` (string), `Name` (string), `Description` (string?), `CategoryId` (int?), `UnitOfMeasureId` (int), `DefaultPrice` (decimal), `MinQuantity` (decimal?), `Barcode` (string?), `IsActive` (bool)[cite: 6] |
| `CreateCategoryRequest` | `Name` (string), `Description` (string?), `ParentCategoryId` (int?)[cite: 6] |
| `UpdateCategoryRequest` | `Name` (string), `Description` (string?), `ParentCategoryId` (int?)[cite: 6] |
| `CreateUnitOfMeasureRequest` | `Name` (string), `Symbol` (string)[cite: 6] |
| `UpdateUnitOfMeasureRequest` | `Name` (string), `Symbol` (string)[cite: 6] |

### Response DTOs
| DTO | Fields |
| :--- | :--- |
| `ProductDto` | `Id` (int), `Code` (string), `Name` (string), `Description` (string?), `DefaultPrice` (decimal), `MinQuantity` (decimal?), `CategoryName` (string?), `UnitOfMeasureName` (string), `IsActive` (bool)[cite: 6] |
| `CategoryDto` | `Id` (int), `Name` (string), `Description` (string?), `ParentCategoryId` (int?), `ParentCategoryName` (string?)[cite: 6] |
| `UnitOfMeasureDto` | `Id` (int), `Name` (string), `Symbol` (string)[cite: 6] |

---

## 6. API Endpoints

### `ProductsController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/Products` | Policy: `Permissions.Products.Product.Read` | - | `IEnumerable<ProductDto>` | Lists products in company[cite: 8] |
| GET | `/api/Products/{id}` | Policy: `Permissions.Products.Product.Read` | - | `ProductDto` | Gets single product[cite: 8] |
| POST | `/api/Products` | Policy: `Permissions.Products.Product.Manage` | `CreateProductDto` | `201 CreatedAtAction` | Creates product[cite: 8] |
| PUT | `/api/Products/{id}` | Policy: `Permissions.Products.Product.Manage` | `UpdateProductDto` | `204 NoContent` | Updates product[cite: 8] |
| DELETE | `/api/Products/{id}` | Policy: `Permissions.Products.Product.Manage` | - | `204 NoContent` | Soft deletes product[cite: 8] |

### `CategoryController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/Category` | Policy: `Permissions.Products.Categories.Read` | - | `IReadOnlyList<CategoryDto>`| Lists scoped categories[cite: 7] |
| GET | `/api/Category/{id}` | Policy: `Permissions.Products.Categories.Read` | - | `CategoryDto` | Gets single category[cite: 7] |
| POST | `/api/Category` | Policy: `Permissions.Products.Categories.Manage` | `CreateCategoryRequest` | `201 CreatedAtAction` | Creates category[cite: 7] |
| PUT | `/api/Category/{id}` | Policy: `Permissions.Products.Categories.Manage` | `UpdateCategoryRequest` | `CategoryDto` | Updates category[cite: 7] |
| DELETE | `/api/Category/{id}` | Policy: `Permissions.Products.Categories.Manage` | - | `204 NoContent` | Soft deletes category[cite: 7] |

### `UnitOfMeasureController`
| HTTP Verb | Route | Auth Required | Request Body | Response Type | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/UnitOfMeasure` | Policy: `Permissions.Products.UnitOfMeasures.Read` | - | `IReadOnlyList<UnitOfMeasureDto>` | Lists scoped measures[cite: 9] |
| GET | `/api/UnitOfMeasure/{id}` | Policy: `Permissions.Products.UnitOfMeasures.Read` | - | `UnitOfMeasureDto` | Gets single measure[cite: 9] |
| POST | `/api/UnitOfMeasure` | Policy: `Permissions.Products.UnitOfMeasures.Manage` | `CreateUnitOfMeasureRequest` | `201 CreatedAtAction` | Creates measure[cite: 9] |
| PUT | `/api/UnitOfMeasure/{id}` | Policy: `Permissions.Products.UnitOfMeasures.Manage`| `UpdateUnitOfMeasureRequest` | `UnitOfMeasureDto` | Updates measure[cite: 9] |
| DELETE | `/api/UnitOfMeasure/{id}`| Policy: `Permissions.Products.UnitOfMeasures.Manage`| - | `204 NoContent` | Soft deletes measure[cite: 9] |

---

## 7. Business Rules & Validation

1. **Company Access Isolation:** Almost all services rely heavily on `ICurrentUserService.CompanyId` to automatically filter results (`GetAllByCompanyAsync` for Products, Categories, and Units of Measure) and validate ownership during updates/deletes[cite: 3, 4, 5].
2. **Product Code Uniqueness:** A product's `Code` must be strictly unique within the tenant's context, enforced by `CodeExistsAsync` during both creation and updates[cite: 4].
3. **Hierarchy Circular Reference Protection:** A category cannot be set as its own parent (`request.ParentCategoryId.Value == id`)[cite: 3].
4. **Hierarchy Isolation Check:** When assigning a parent category, the system verifies that the specified parent actually exists and belongs to the current user's `CompanyId`[cite: 3].
5. **Soft Delete Behavior:** Deleting entities never drops rows physically; it sets `IsDeleted = true` and updates the `UpdatedAt` timestamp[cite: 2, 3, 4, 5].

---

## 8. Cross-Module Dependencies

* **Infrastructure Layer (Data):** Relies on `AppDbContext` for standard Entity Framework Core persistence[cite: 2].
* **Security Context Extraction:** Heavily depends on `ICurrentUserService` (`ERPSystem.Application.Interfaces`) for safely isolating the executing user's `CompanyId`[cite: 3, 4, 5].
* **Authorization Policies:** Depends on explicitly defined policy constants (e.g., `Permissions.Products.Product.Manage`) to lock down HTTP routes[cite: 7, 8, 9].
* **Domain Abstractions:** Implements foundational `BaseEntity` and `ICompanyEntity` structures to inherit primary keys, auditable properties, and multi-tenant keys[cite: 1].