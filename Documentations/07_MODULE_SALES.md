# 07 — MODULE: SALES

---

## 1. Module Overview

The Sales module manages the full customer-facing sales cycle within the ERP system, from invoice creation through delivery and payment receipt. It handles customers, sales invoices, physical deliveries of goods, customer payments (receipts with invoice allocation), and sales returns. The module integrates with the Inventory module (for stock movements on delivery and return posting) and enforces company-level data isolation throughout.

---

## 2. Domain Entities

### 2.1 `Customer`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK (inherited from `AuditableEntity`) |
| `CompanyId` | `int` | ✅ | FK → Company |
| `Code` | `string` | ✅ | Unique per company |
| `Name` | `string` | ✅ | Display name |
| `Email` | `string?` | ❌ | |
| `Phone` | `string?` | ❌ | |
| `Address` | `string?` | ❌ | |
| `TaxNumber` | `string?` | ❌ | |
| `CreditLimit` | `decimal` | ✅ | |
| `IsActive` | `bool` | ✅ | Default: `true` |
| `Invoices` | `ICollection<SalesInvoice>` | — | Navigation |
| `Receipts` | `ICollection<SalesReceipt>` | — | Navigation |

---

### 2.2 `SalesInvoice`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch |
| `InvoiceNumber` | `string` | ✅ | Auto-generated (e.g. `INV-202506-00001`) |
| `InvoiceDate` | `DateTime` | ✅ | |
| `DueDate` | `DateTime?` | ❌ | |
| `CustomerId` | `int` | ✅ | FK → `Customer` |
| `Customer` | `Customer` | — | Navigation |
| `Status` | `SalesInvoiceStatus` | ✅ | Enum; default: `Draft` |
| `PaymentStatus` | `PaymentStatus` | ✅ | Enum; default: `Unpaid` |
| `SubTotal` | `decimal` | ✅ | Sum of line quantities × unit prices |
| `DiscountAmount` | `decimal` | ✅ | Sum of line discounts |
| `TaxAmount` | `decimal` | ✅ | Sum of line taxes |
| `GrandTotal` | `decimal` | ✅ | Net amount after discounts + tax |
| `PaidAmount` | `decimal` | ✅ | Accumulated from posted receipts |
| `BalanceDue` | `decimal` | — | Computed: `GrandTotal - PaidAmount` |
| `Notes` | `string?` | ❌ | |
| `PostedByUserId` | `Guid?` | ❌ | |
| `PostedAt` | `DateTime?` | ❌ | |
| `Lines` | `ICollection<SalesInvoiceLine>` | — | Navigation |
| `Deliveries` | `ICollection<SalesDelivery>` | — | Navigation |
| `ReceiptAllocations` | `ICollection<SalesReceiptAllocation>` | — | Navigation |

---

### 2.3 `SalesInvoiceLine`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `SalesInvoiceId` | `int` | ✅ | FK → `SalesInvoice` |
| `ProductId` | `int` | ✅ | FK → `Product` |
| `UnitId` | `int` | ✅ | FK → `UnitOfMeasure` |
| `Quantity` | `decimal` | ✅ | Ordered quantity |
| `UnitPrice` | `decimal` | ✅ | |
| `DiscountPercent` | `decimal` | ✅ | |
| `DiscountAmount` | `decimal` | ✅ | Computed from percent |
| `TaxPercent` | `decimal` | ✅ | |
| `TaxAmount` | `decimal` | ✅ | Computed from percent |
| `LineTotal` | `decimal` | ✅ | Net line amount |
| `DeliveredQuantity` | `decimal` | ✅ | Updated on delivery posting |
| `RemainingQuantity` | `decimal` | — | Computed: `Quantity - DeliveredQuantity` |
| `Notes` | `string?` | ❌ | |

---

### 2.4 `SalesDelivery`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch |
| `DeliveryNumber` | `string` | ✅ | Auto-generated (e.g. `DEL-202506-00001`) |
| `DeliveryDate` | `DateTime` | ✅ | |
| `SalesInvoiceId` | `int` | ✅ | FK → `SalesInvoice` |
| `SalesInvoice` | `SalesInvoice` | — | Navigation |
| `CustomerId` | `int` | ✅ | FK → `Customer` (copied from invoice) |
| `Customer` | `Customer` | — | Navigation |
| `WarehouseId` | `int` | ✅ | FK → `Warehouse` |
| `Warehouse` | `Warehouse` | — | Navigation |
| `Status` | `SalesDeliveryStatus` | ✅ | Enum; default: `Draft` |
| `Notes` | `string?` | ❌ | |
| `PostedByUserId` | `Guid?` | ❌ | |
| `PostedAt` | `DateTime?` | ❌ | |
| `InventoryDocumentId` | `int?` | ❌ | FK → Inventory document created on posting |
| `Lines` | `ICollection<SalesDeliveryLine>` | — | Navigation |

---

### 2.5 `SalesDeliveryLine`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `SalesDeliveryId` | `int` | ✅ | FK → `SalesDelivery` |
| `SalesInvoiceLineId` | `int` | ✅ | FK → `SalesInvoiceLine` |
| `SalesInvoiceLine` | `SalesInvoiceLine` | — | Navigation |
| `ProductId` | `int` | ✅ | FK → `Product` |
| `UnitId` | `int` | ✅ | FK → `UnitOfMeasure` |
| `Quantity` | `decimal` | ✅ | Must not exceed `SalesInvoiceLine.RemainingQuantity` |
| `Notes` | `string?` | ❌ | |

---

### 2.6 `SalesReceipt`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch |
| `ReceiptNumber` | `string` | ✅ | Auto-generated (e.g. `RCP-202506-00001`) |
| `ReceiptDate` | `DateTime` | ✅ | |
| `CustomerId` | `int` | ✅ | FK → `Customer` |
| `Customer` | `Customer` | — | Navigation |
| `Amount` | `decimal` | ✅ | Total payment received |
| `PaymentMethod` | `string?` | ❌ | e.g. Cash, Transfer |
| `ReferenceNumber` | `string?` | ❌ | e.g. cheque/bank reference |
| `Status` | `SalesReceiptStatus` | ✅ | Enum; default: `Draft` |
| `Notes` | `string?` | ❌ | |
| `PostedByUserId` | `Guid?` | ❌ | |
| `PostedAt` | `DateTime?` | ❌ | |
| `Allocations` | `ICollection<SalesReceiptAllocation>` | — | Navigation |

---

### 2.7 `SalesReceiptAllocation`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `SalesReceiptId` | `int` | ✅ | FK → `SalesReceipt` |
| `SalesInvoiceId` | `int` | ✅ | FK → `SalesInvoice` |
| `AllocatedAmount` | `decimal` | ✅ | Must not exceed invoice `BalanceDue` |

---

### 2.8 `SalesReturn`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch |
| `ReturnNumber` | `string` | ✅ | Auto-generated (e.g. `RET-202506-00001`) |
| `ReturnDate` | `DateTime` | ✅ | |
| `SalesInvoiceId` | `int?` | ❌ | FK → `SalesInvoice` (optional reference) |
| `CustomerId` | `int` | ✅ | FK → `Customer` |
| `WarehouseId` | `int` | ✅ | FK → `Warehouse` (stock returned here) |
| `Status` | `SalesReturnStatus` | ✅ | Enum; default: `Draft` |
| `SubTotal` | `decimal` | ✅ | |
| `TaxAmount` | `decimal` | ✅ | |
| `GrandTotal` | `decimal` | ✅ | |
| `Reason` | `string?` | ❌ | |
| `Notes` | `string?` | ❌ | |
| `PostedByUserId` | `Guid?` | ❌ | |
| `PostedAt` | `DateTime?` | ❌ | |
| `InventoryDocumentId` | `int?` | ❌ | FK → Inventory document created on posting |
| `Lines` | `ICollection<SalesReturnLine>` | — | Navigation |

---

### 2.9 `SalesReturnLine`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK |
| `SalesReturnId` | `int` | ✅ | FK → `SalesReturn` |
| `ProductId` | `int` | ✅ | FK → `Product` |
| `UnitId` | `int` | ✅ | FK → `UnitOfMeasure` |
| `Quantity` | `decimal` | ✅ | |
| `UnitPrice` | `decimal` | ✅ | |
| `TaxPercent` | `decimal` | ✅ | |
| `TaxAmount` | `decimal` | ✅ | Computed |
| `LineTotal` | `decimal` | ✅ | Computed |
| `Notes` | `string?` | ❌ | |

---

## 3. Repository Interfaces

### 3.1 `ICustomerRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `int id, CancellationToken` | `Task<Customer?>` |
| `GetAllByCompanyAsync` | `int companyId, bool? isActive, CancellationToken` | `Task<List<Customer>>` |
| `GetByCodeAsync` | `int companyId, string code, CancellationToken` | `Task<Customer?>` |
| `ExistsAsync` | `int companyId, string code, int? excludeId, CancellationToken` | `Task<bool>` |
| `AddAsync` | `Customer customer, CancellationToken` | `Task` |
| `Update` | `Customer customer` | `void` |
| `Delete` | `Customer customer` | `void` (soft-delete: sets `IsDeleted = true`) |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

---

### 3.2 `ISalesInvoiceRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesInvoice?>` |
| `GetByIdWithLinesAsync` | `int id, CancellationToken` | `Task<SalesInvoice?>` |
| `GetAllByCompanyAsync` | `int companyId, int? customerId, SalesInvoiceStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<List<SalesInvoice>>` |
| `GenerateInvoiceNumberAsync` | `int companyId, CancellationToken` | `Task<string>` |
| `AddAsync` | `SalesInvoice invoice, CancellationToken` | `Task` |
| `Update` | `SalesInvoice invoice` | `void` |
| `Delete` | `SalesInvoice invoice` | `void` (soft-delete) |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

> `GetByIdWithLinesAsync` eagerly loads: `Customer`, `Lines` → `Product`, `Lines` → `Unit`

---

### 3.3 `ISalesDeliveryRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesDelivery?>` |
| `GetByIdWithLinesAsync` | `int id, CancellationToken` | `Task<SalesDelivery?>` |
| `GetAllByCompanyAsync` | `int companyId, int? invoiceId, SalesDeliveryStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<List<SalesDelivery>>` |
| `GenerateDeliveryNumberAsync` | `int companyId, CancellationToken` | `Task<string>` |
| `AddAsync` | `SalesDelivery delivery, CancellationToken` | `Task` |
| `Update` | `SalesDelivery delivery` | `void` |
| `Delete` | `SalesDelivery delivery` | `void` (soft-delete) |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

> `GetByIdWithLinesAsync` eagerly loads: `Customer`, `SalesInvoice` → `Lines`, `Warehouse`, `Lines` → `Product`, `Lines` → `Unit`, `Lines` → `SalesInvoiceLine`

---

### 3.4 `ISalesReceiptRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesReceipt?>` |
| `GetByIdWithAllocationsAsync` | `int id, CancellationToken` | `Task<SalesReceipt?>` |
| `GetAllByCompanyAsync` | `int companyId, int? customerId, SalesReceiptStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<List<SalesReceipt>>` |
| `GenerateReceiptNumberAsync` | `int companyId, CancellationToken` | `Task<string>` |
| `AddAsync` | `SalesReceipt receipt, CancellationToken` | `Task` |
| `Update` | `SalesReceipt receipt` | `void` |
| `Delete` | `SalesReceipt receipt` | `void` (soft-delete) |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

> `GetByIdWithAllocationsAsync` eagerly loads: `Customer`, `Allocations` → `SalesInvoice`

---

### 3.5 `ISalesReturnRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesReturn?>` |
| `GetByIdWithLinesAsync` | `int id, CancellationToken` | `Task<SalesReturn?>` |
| `GetAllByCompanyAsync` | `int companyId, int? customerId, SalesReturnStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<List<SalesReturn>>` |
| `GenerateReturnNumberAsync` | `int companyId, CancellationToken` | `Task<string>` |
| `AddAsync` | `SalesReturn salesReturn, CancellationToken` | `Task` |
| `Update` | `SalesReturn salesReturn` | `void` |
| `Delete` | `SalesReturn salesReturn` | `void` (soft-delete) |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

> `GetByIdWithLinesAsync` eagerly loads: `Customer`, `Warehouse`, `SalesInvoice`, `Lines` → `Product`, `Lines` → `Unit`

---

## 4. Service Interfaces

### 4.1 `ICustomerService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `bool? isActive, CancellationToken` | `Task<IReadOnlyList<CustomerDto>>` | Scoped to current company |
| `GetByIdAsync` | `int id, CancellationToken` | `Task<CustomerDto?>` | Returns `null` if not found or wrong company |
| `CreateAsync` | `CreateCustomerRequest, CancellationToken` | `Task<CustomerDto>` | Validates unique `Code` per company |
| `UpdateAsync` | `int id, UpdateCustomerRequest, CancellationToken` | `Task<CustomerDto>` | Cannot change `Code` |
| `DeleteAsync` | `int id, CancellationToken` | `Task` | Soft-delete only |

---

### 4.2 `ISalesInvoiceService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `int? customerId, SalesInvoiceStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<IReadOnlyList<SalesInvoiceListDto>>` | |
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesInvoiceDto?>` | Includes lines |
| `CreateAsync` | `CreateSalesInvoiceRequest, CancellationToken` | `Task<SalesInvoiceDto>` | Calculates line and header totals |
| `UpdateAsync` | `int id, UpdateSalesInvoiceRequest, CancellationToken` | `Task<SalesInvoiceDto>` | Draft only; replaces all lines |
| `PostAsync` | `int id, CancellationToken` | `Task<SalesInvoiceDto>` | Draft → Posted |
| `CancelAsync` | `int id, CancellationToken` | `Task<SalesInvoiceDto>` | Blocked if deliveries exist |
| `DeleteAsync` | `int id, CancellationToken` | `Task` | Draft only; soft-delete |

---

### 4.3 `ISalesDeliveryService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `int? invoiceId, SalesDeliveryStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<IReadOnlyList<SalesDeliveryListDto>>` | |
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesDeliveryDto?>` | Includes lines |
| `CreateAsync` | `CreateSalesDeliveryRequest, CancellationToken` | `Task<SalesDeliveryDto>` | Validates invoice status and remaining quantities |
| `PostAsync` | `int id, CancellationToken` | `Task<PostDeliveryResponse>` | Requires Inventory module; creates StockOut; updates invoice delivery status |
| `CancelAsync` | `int id, CancellationToken` | `Task<SalesDeliveryDto>` | Draft only |
| `DeleteAsync` | `int id, CancellationToken` | `Task` | Draft only; soft-delete |

---

### 4.4 `ISalesReceiptService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `int? customerId, SalesReceiptStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<IReadOnlyList<SalesReceiptListDto>>` | |
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesReceiptDto?>` | Includes allocations |
| `CreateAsync` | `CreateSalesReceiptRequest, CancellationToken` | `Task<SalesReceiptDto>` | Validates total allocation ≤ receipt amount |
| `PostAsync` | `int id, CancellationToken` | `Task<SalesReceiptDto>` | Applies allocations; updates invoice `PaidAmount` and `PaymentStatus` |
| `CancelAsync` | `int id, CancellationToken` | `Task<SalesReceiptDto>` | If posted: reverses all allocations on invoices |
| `DeleteAsync` | `int id, CancellationToken` | `Task` | Draft only; soft-delete |

---

### 4.5 `ISalesReturnService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `int? customerId, SalesReturnStatus? status, DateTime? fromDate, DateTime? toDate, CancellationToken` | `Task<IReadOnlyList<SalesReturnListDto>>` | |
| `GetByIdAsync` | `int id, CancellationToken` | `Task<SalesReturnDto?>` | Includes lines |
| `CreateAsync` | `CreateSalesReturnRequest, CancellationToken` | `Task<SalesReturnDto>` | Calculates line and header totals |
| `PostAsync` | `int id, CancellationToken` | `Task<PostReturnResponse>` | Requires Inventory module; creates StockIn; `UnitPrice` used as cost |
| `CancelAsync` | `int id, CancellationToken` | `Task<SalesReturnDto>` | Draft only; posted returns cannot be cancelled |
| `DeleteAsync` | `int id, CancellationToken` | `Task` | Draft only; soft-delete |

---

## 5. DTOs & View Models

### 5.1 Request DTOs

#### `CreateCustomerRequest`

| Field | Type |
|---|---|
| `Code` | `string` |
| `Name` | `string` |
| `Email` | `string?` |
| `Phone` | `string?` |
| `Address` | `string?` |
| `TaxNumber` | `string?` |
| `CreditLimit` | `decimal` |

#### `UpdateCustomerRequest`

| Field | Type |
|---|---|
| `Name` | `string` |
| `Email` | `string?` |
| `Phone` | `string?` |
| `Address` | `string?` |
| `TaxNumber` | `string?` |
| `CreditLimit` | `decimal` |
| `IsActive` | `bool` |

#### `CreateSalesInvoiceRequest`

| Field | Type |
|---|---|
| `BranchId` | `int?` |
| `InvoiceDate` | `DateTime` |
| `DueDate` | `DateTime?` |
| `CustomerId` | `int` |
| `Notes` | `string?` |
| `Lines` | `List<CreateSalesInvoiceLineRequest>` |

#### `CreateSalesInvoiceLineRequest`

| Field | Type |
|---|---|
| `ProductId` | `int` |
| `UnitId` | `int` |
| `Quantity` | `decimal` |
| `UnitPrice` | `decimal` |
| `DiscountPercent` | `decimal` |
| `TaxPercent` | `decimal` |
| `Notes` | `string?` |

#### `UpdateSalesInvoiceRequest`

| Field | Type |
|---|---|
| `InvoiceDate` | `DateTime` |
| `DueDate` | `DateTime?` |
| `CustomerId` | `int` |
| `Notes` | `string?` |
| `Lines` | `List<UpdateSalesInvoiceLineRequest>` |

#### `UpdateSalesInvoiceLineRequest`

| Field | Type |
|---|---|
| `ProductId` | `int` |
| `UnitId` | `int` |
| `Quantity` | `decimal` |
| `UnitPrice` | `decimal` |
| `DiscountPercent` | `decimal` |
| `TaxPercent` | `decimal` |
| `Notes` | `string?` |

#### `CreateSalesDeliveryRequest`

| Field | Type |
|---|---|
| `BranchId` | `int?` |
| `SalesInvoiceId` | `int` |
| `WarehouseId` | `int` |
| `DeliveryDate` | `DateTime` |
| `Notes` | `string?` |
| `Lines` | `List<CreateSalesDeliveryLineRequest>` |

#### `CreateSalesDeliveryLineRequest`

| Field | Type |
|---|---|
| `SalesInvoiceLineId` | `int` |
| `Quantity` | `decimal` |
| `Notes` | `string?` |

#### `CreateSalesReceiptRequest`

| Field | Type |
|---|---|
| `BranchId` | `int?` |
| `ReceiptDate` | `DateTime` |
| `CustomerId` | `int` |
| `Amount` | `decimal` |
| `PaymentMethod` | `string?` |
| `ReferenceNumber` | `string?` |
| `Notes` | `string?` |
| `Allocations` | `List<CreateSalesReceiptAllocationRequest>?` |

#### `CreateSalesReceiptAllocationRequest`

| Field | Type |
|---|---|
| `SalesInvoiceId` | `int` |
| `AllocatedAmount` | `decimal` |

#### `CreateSalesReturnRequest`

| Field | Type |
|---|---|
| `BranchId` | `int?` |
| `ReturnDate` | `DateTime` |
| `CustomerId` | `int` |
| `SalesInvoiceId` | `int?` |
| `WarehouseId` | `int` |
| `Reason` | `string?` |
| `Notes` | `string?` |
| `Lines` | `List<CreateSalesReturnLineRequest>` |

#### `CreateSalesReturnLineRequest`

| Field | Type |
|---|---|
| `ProductId` | `int` |
| `UnitId` | `int` |
| `Quantity` | `decimal` |
| `UnitPrice` | `decimal` |
| `TaxPercent` | `decimal` |
| `Notes` | `string?` |

---

### 5.2 Response DTOs

#### `CustomerDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `Code` | `string` |
| `Name` | `string` |
| `Email` | `string?` |
| `Phone` | `string?` |
| `Address` | `string?` |
| `TaxNumber` | `string?` |
| `CreditLimit` | `decimal` |
| `IsActive` | `bool` |

#### `SalesInvoiceDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `InvoiceNumber` | `string` |
| `InvoiceDate` | `DateTime` |
| `DueDate` | `DateTime?` |
| `CustomerId` | `int` |
| `CustomerName` | `string` |
| `Status` | `string` |
| `PaymentStatus` | `string` |
| `SubTotal` | `decimal` |
| `DiscountAmount` | `decimal` |
| `TaxAmount` | `decimal` |
| `GrandTotal` | `decimal` |
| `PaidAmount` | `decimal` |
| `BalanceDue` | `decimal` |
| `Notes` | `string?` |
| `Lines` | `List<SalesInvoiceLineDto>` |

#### `SalesInvoiceLineDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ProductId` | `int` |
| `ProductName` | `string` |
| `ProductCode` | `string` |
| `UnitId` | `int` |
| `UnitName` | `string` |
| `Quantity` | `decimal` |
| `UnitPrice` | `decimal` |
| `DiscountPercent` | `decimal` |
| `DiscountAmount` | `decimal` |
| `TaxPercent` | `decimal` |
| `TaxAmount` | `decimal` |
| `LineTotal` | `decimal` |
| `DeliveredQuantity` | `decimal` |
| `RemainingQuantity` | `decimal` |

#### `SalesDeliveryDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `DeliveryNumber` | `string` |
| `DeliveryDate` | `DateTime` |
| `SalesInvoiceId` | `int` |
| `InvoiceNumber` | `string` |
| `CustomerId` | `int` |
| `CustomerName` | `string` |
| `WarehouseId` | `int` |
| `WarehouseName` | `string` |
| `Status` | `string` |
| `Notes` | `string?` |
| `Lines` | `List<SalesDeliveryLineDto>` |

#### `SalesDeliveryLineDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `SalesInvoiceLineId` | `int` |
| `ProductId` | `int` |
| `ProductName` | `string` |
| `ProductCode` | `string` |
| `UnitId` | `int` |
| `UnitName` | `string` |
| `Quantity` | `decimal` |

#### `PostDeliveryResponse`

| Field | Type |
|---|---|
| `DeliveryId` | `int` |
| `DeliveryNumber` | `string` |
| `InventoryDocumentId` | `int` |
| `InventoryDocNumber` | `string` |

#### `SalesReceiptDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ReceiptNumber` | `string` |
| `ReceiptDate` | `DateTime` |
| `CustomerId` | `int` |
| `CustomerName` | `string` |
| `Amount` | `decimal` |
| `PaymentMethod` | `string?` |
| `ReferenceNumber` | `string?` |
| `Status` | `string` |
| `Notes` | `string?` |
| `Allocations` | `List<SalesReceiptAllocationDto>` |

#### `SalesReceiptAllocationDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `SalesInvoiceId` | `int` |
| `InvoiceNumber` | `string` |
| `AllocatedAmount` | `decimal` |

#### `SalesReturnDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ReturnNumber` | `string` |
| `ReturnDate` | `DateTime` |
| `SalesInvoiceId` | `int?` |
| `InvoiceNumber` | `string?` |
| `CustomerId` | `int` |
| `CustomerName` | `string` |
| `WarehouseId` | `int` |
| `WarehouseName` | `string` |
| `Status` | `string` |
| `SubTotal` | `decimal` |
| `TaxAmount` | `decimal` |
| `GrandTotal` | `decimal` |
| `Reason` | `string?` |
| `Notes` | `string?` |
| `Lines` | `List<SalesReturnLineDto>` |

#### `SalesReturnLineDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ProductId` | `int` |
| `ProductName` | `string` |
| `ProductCode` | `string` |
| `UnitId` | `int` |
| `UnitName` | `string` |
| `Quantity` | `decimal` |
| `UnitPrice` | `decimal` |
| `TaxPercent` | `decimal` |
| `TaxAmount` | `decimal` |
| `LineTotal` | `decimal` |

#### `PostReturnResponse`

| Field | Type |
|---|---|
| `ReturnId` | `int` |
| `ReturnNumber` | `string` |
| `InventoryDocumentId` | `int` |
| `InventoryDocNumber` | `string` |

---

### 5.3 List / Summary DTOs

#### `SalesInvoiceListDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `InvoiceNumber` | `string` |
| `InvoiceDate` | `DateTime` |
| `CustomerName` | `string` |
| `Status` | `string` |
| `PaymentStatus` | `string` |
| `GrandTotal` | `decimal` |
| `BalanceDue` | `decimal` |

#### `SalesDeliveryListDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `DeliveryNumber` | `string` |
| `DeliveryDate` | `DateTime` |
| `InvoiceNumber` | `string` |
| `CustomerName` | `string` |
| `WarehouseName` | `string` |
| `Status` | `string` |

#### `SalesReceiptListDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ReceiptNumber` | `string` |
| `ReceiptDate` | `DateTime` |
| `CustomerName` | `string` |
| `Amount` | `decimal` |
| `Status` | `string` |

#### `SalesReturnListDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `ReturnNumber` | `string` |
| `ReturnDate` | `DateTime` |
| `CustomerName` | `string` |
| `WarehouseName` | `string` |
| `Status` | `string` |
| `GrandTotal` | `decimal` |

---

## 6. API Endpoints

### 6.1 Customers

There is no dedicated `CustomersController` visible in the provided files. Customer management is exposed via the service layer and may be part of a shared or separate controller. The endpoints below are inferred from the service interface.

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `api/sales/customers` | Sales.Customers.Read | `?isActive=bool` | `CustomerDto[]` | List all customers for company |
| `GET` | `api/sales/customers/{id}` | Sales.Customers.Read | — | `CustomerDto` | Get customer by ID |
| `POST` | `api/sales/customers` | Sales.Customers.Access | `CreateCustomerRequest` | `CustomerDto` | Create customer |
| `PUT` | `api/sales/customers/{id}` | Sales.Customers.Access | `UpdateCustomerRequest` | `CustomerDto` | Update customer |
| `DELETE` | `api/sales/customers/{id}` | Sales.Customers.Manage | — | `204` | Soft-delete customer |

---

### 6.2 Sales Invoices — `api/sales/invoices`

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/` | `Sales.Invoices.Read` | `?customerId, ?status, ?fromDate, ?toDate` | `SalesInvoiceListDto[]` | List invoices with optional filters |
| `GET` | `/{id}` | `Sales.Invoices.Read` | — | `SalesInvoiceDto` | Get invoice with lines |
| `POST` | `/` | `Sales.Invoices.Access` | `CreateSalesInvoiceRequest` | `SalesInvoiceDto` (201) | Create draft invoice |
| `PUT` | `/{id}` | `Sales.Invoices.Access` | `UpdateSalesInvoiceRequest` | `SalesInvoiceDto` | Update draft invoice (replaces lines) |
| `POST` | `/{id}/post` | `Sales.Invoices.Manage` | — | `SalesInvoiceDto` | Post invoice (Draft → Posted) |
| `POST` | `/{id}/cancel` | `Sales.Invoices.Manage` | — | `SalesInvoiceDto` | Cancel invoice |
| `DELETE` | `/{id}` | `Sales.Invoices.Manage` | — | `204` | Soft-delete draft invoice |

---

### 6.3 Sales Deliveries — `api/sales/deliveries`

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/` | `Sales.Deliveries.Read` | `?invoiceId, ?status, ?fromDate, ?toDate` | `SalesDeliveryListDto[]` | List deliveries with optional filters |
| `GET` | `/{id}` | `Sales.Deliveries.Read` | — | `SalesDeliveryDto` | Get delivery with lines |
| `POST` | `/` | `Sales.Deliveries.Access` | `CreateSalesDeliveryRequest` | `SalesDeliveryDto` (201) | Create draft delivery |
| `POST` | `/{id}/post` | `Sales.Deliveries.Manage` | — | `PostDeliveryResponse` | Post delivery; triggers StockOut if Inventory enabled |
| `POST` | `/{id}/cancel` | `Sales.Deliveries.Access` | — | `SalesDeliveryDto` | Cancel draft delivery |
| `DELETE` | `/{id}` | `Sales.Deliveries.Manage` | — | `204` | Soft-delete draft delivery |

---

### 6.4 Sales Receipts — `api/sales/receipts`

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/` | `Sales.Receipts.Read` | `?customerId, ?status, ?fromDate, ?toDate` | `SalesReceiptListDto[]` | List receipts with optional filters |
| `GET` | `/{id}` | `Sales.Receipts.Read` | — | `SalesReceiptDto` | Get receipt with allocations |
| `POST` | `/` | `Sales.Receipts.Access` | `CreateSalesReceiptRequest` | `SalesReceiptDto` (201) | Create draft receipt with optional allocations |
| `POST` | `/{id}/post` | `Sales.Receipts.Manage` | — | `SalesReceiptDto` | Post receipt; applies allocations to invoices |
| `POST` | `/{id}/cancel` | `Sales.Receipts.Manage` | — | `SalesReceiptDto` | Cancel receipt; reverses allocations if was posted |
| `DELETE` | `/{id}` | `Sales.Receipts.Manage` | — | `204` | Soft-delete draft receipt |

---

### 6.5 Sales Returns — `api/sales/returns`

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `/` | `Sales.Returns.Read` | `?customerId, ?status, ?fromDate, ?toDate` | `SalesReturnListDto[]` | List returns with optional filters |
| `GET` | `/{id}` | `Sales.Returns.Read` | — | `SalesReturnDto` | Get return with lines |
| `POST` | `/` | `Sales.Returns.Access` | `CreateSalesReturnRequest` | `SalesReturnDto` (201) | Create draft return |
| `POST` | `/{id}/post` | `Sales.Returns.Manage` | — | `PostReturnResponse` | Post return; triggers StockIn if Inventory enabled |
| `POST` | `/{id}/cancel` | `Sales.Returns.Access` | — | `SalesReturnDto` | Cancel draft return |
| `DELETE` | `/{id}` | `Sales.Returns.Manage` | — | `204` | Soft-delete draft return |

---

## 7. Business Rules & Validation

### 7.1 Customer Rules

| Rule | Detail |
|---|---|
| Unique code per company | `Customer.Code` must be unique within a `CompanyId`; enforced on create |
| Company isolation | All reads and writes verify `customer.CompanyId == currentUser.CompanyId` |
| Soft-delete only | `IsDeleted = true`; deleted records excluded from all queries |
| `Code` is immutable after creation | `UpdateAsync` does not accept a new code |

---

### 7.2 Sales Invoice Rules

| Rule | Detail |
|---|---|
| At least one line required | `NO_LINES` error thrown if `Lines` is null or empty |
| Customer must belong to company | Cross-company customer assignment blocked |
| Only Draft invoices can be updated | `CannotModifyPostedDocument` exception if not Draft |
| Only Draft invoices can be deleted | `InvalidStatus` exception otherwise |
| Cannot cancel if deliveries exist | Blocked when status is `PartiallyDelivered` or `FullyDelivered` |
| Line total computation | `LineTotal = (Qty × UnitPrice - Discount) + Tax` |
| Header totals | `SubTotal = Σ(Qty × UnitPrice)`, `DiscountAmount = Σ(line discounts)`, `TaxAmount = Σ(line taxes)`, `GrandTotal = Σ(LineTotals)` |

---

### 7.3 Sales Delivery Rules

| Rule | Detail |
|---|---|
| Invoice must be Posted | Cannot deliver against Draft or Cancelled invoices |
| Quantity must not exceed remaining | `lineRequest.Quantity ≤ invoiceLine.RemainingQuantity` |
| Warehouse must belong to company | Cross-company warehouse assignment blocked |
| At least one line required | `NO_LINES` exception if empty |
| Inventory module required to post | `EnsureInventoryEnabledAsync` called before posting |
| Posted delivery triggers StockOut | Via `IInventoryService.StockOutAsync`; `InventoryDocumentId` stored on delivery |
| Posting updates invoice line quantities | `DeliveredQuantity += deliveryLine.Quantity` for each matched `SalesInvoiceLine` |
| Posting updates invoice delivery status | `FullyDelivered` if all lines fully delivered; `PartiallyDelivered` if any line has delivery |
| Posted deliveries cannot be cancelled | Must create a `SalesReturn` instead |
| Only Draft deliveries can be deleted | `InvalidStatus` exception otherwise |

---

### 7.4 Sales Receipt Rules

| Rule | Detail |
|---|---|
| Total allocations ≤ receipt amount | `OVER_ALLOCATION` error if `Σ(allocations) > receipt.Amount` |
| Allocation must not exceed invoice balance | `AllocationExceedsBalance` error if `allocatedAmount > invoice.BalanceDue` |
| Invoice must belong to same customer | `WRONG_CUSTOMER` error if customer mismatch |
| Posting applies allocations | `invoice.PaidAmount += allocation.AllocatedAmount` for each allocation |
| Cancelling a posted receipt reverses allocations | `invoice.PaidAmount -= allocation.AllocatedAmount`; payment status recalculated |
| Payment status recalculation | `Paid` if `PaidAmount ≥ GrandTotal`; `PartiallyPaid` if `PaidAmount > 0`; `Unpaid` if `PaidAmount ≤ 0` |
| Only Draft receipts can be deleted | `InvalidStatus` exception otherwise |

---

### 7.5 Sales Return Rules

| Rule | Detail |
|---|---|
| Customer and warehouse must belong to company | Cross-company assignment blocked |
| At least one line required | `NO_LINES` exception if empty |
| Inventory module required to post | `EnsureInventoryEnabledAsync` called before posting |
| Posting triggers StockIn | Via `IInventoryService.StockInAsync`; `UnitPrice` used as `UnitCost`; `InventoryDocumentId` stored |
| Posted returns cannot be cancelled | `InvalidStatus` exception; a corrective document must be created instead |
| Only Draft returns can be deleted | `InvalidStatus` exception otherwise |
| Line total computation | `LineTotal = (Qty × UnitPrice) + TaxAmount` |
| Header totals | `SubTotal = Σ(Qty × UnitPrice)`, `TaxAmount = Σ(line taxes)`, `GrandTotal = Σ(LineTotals)` |

---

### 7.6 Status Enums & Transitions

#### `SalesInvoiceStatus`

```
Draft → Posted
Draft → Cancelled
Posted → PartiallyDelivered   (auto, set by delivery service)
Posted → FullyDelivered       (auto, set by delivery service)
* → Cancelled                 (blocked if PartiallyDelivered or FullyDelivered)
```

#### `PaymentStatus` (on `SalesInvoice`)

```
Unpaid → PartiallyPaid   (when PaidAmount > 0 and < GrandTotal)
PartiallyPaid → Paid     (when PaidAmount >= GrandTotal)
Paid → PartiallyPaid     (on receipt cancellation)
PartiallyPaid → Unpaid   (on receipt cancellation if PaidAmount falls to 0)
```

#### `SalesDeliveryStatus`

```
Draft → Posted     (via PostAsync; requires Inventory module)
Draft → Cancelled  (via CancelAsync)
Posted → ✗         (immutable; reverse via SalesReturn)
```

#### `SalesReceiptStatus`

```
Draft → Posted     (via PostAsync)
Draft → Cancelled  (via CancelAsync)
Posted → Cancelled (via CancelAsync; reverses invoice allocations)
```

#### `SalesReturnStatus`

```
Draft → Posted     (via PostAsync; requires Inventory module)
Draft → Cancelled  (via CancelAsync)
Posted → ✗         (immutable; create corrective document)
```

---

### 7.7 Document Number Formats

| Entity | Format | Example |
|---|---|---|
| `SalesInvoice` | `INV-{yyyyMM}-{count+1:D5}` | `INV-202506-00003` |
| `SalesDelivery` | `DEL-{yyyyMM}-{count+1:D5}` | `DEL-202506-00001` |
| `SalesReceipt` | `RCP-{yyyyMM}-{count+1:D5}` | `RCP-202506-00002` |
| `SalesReturn` | `RET-{yyyyMM}-{count+1:D5}` | `RET-202506-00001` |

> ⚠️ Number generation is based on a total count of documents per company, not a sequence. Under concurrent high load, duplicates are theoretically possible. Consider using a dedicated sequence if this becomes an issue.

---

## 8. Cross-Module Dependencies

| Interface / Entity | Source Module | Used By (Sales) | Purpose |
|---|---|---|---|
| `ICurrentUserService` | Application / Auth | All services | Provides `CompanyId`, `UserId` for isolation and audit |
| `IModuleAccessService` | Application / Config | All services | `EnsureSalesEnabledAsync()` — gates all operations; `EnsureInventoryEnabledAsync()` — gates posting of deliveries and returns |
| `IInventoryService` | Inventory Module | `SalesDeliveryService`, `SalesReturnService` | `StockOutAsync(StockOutRequest)` on delivery post; `StockInAsync(StockInRequest)` on return post |
| `IWarehouseRepository` | Inventory / Warehouse Module | `SalesDeliveryService`, `SalesReturnService` | Validates warehouse existence and company ownership |
| `IProductRepository` | Products Module | `SalesInvoiceService`, `SalesReturnService` | Validates product existence and company ownership |
| `IUnitOfMeasureRepository` | Products Module | `SalesInvoiceService` | Validates unit of measure on invoice lines |
| `Warehouse` (entity) | Inventory Module | `SalesDelivery`, `SalesReturn` | Navigation property for warehouse name in responses |
| `Product` (entity) | Products Module | `SalesInvoiceLine`, `SalesDeliveryLine`, `SalesReturnLine` | Navigation for product name/code in DTOs |
| `UnitOfMeasure` (entity) | Products Module | `SalesInvoiceLine`, `SalesDeliveryLine`, `SalesReturnLine` | Navigation for unit name in DTOs |
| `StockOutRequest` / `StockOutLineRequest` | Inventory Module DTOs | `SalesDeliveryService.PostAsync` | Payload sent to inventory on delivery posting |
| `StockInRequest` / `StockInLineRequest` | Inventory Module DTOs | `SalesReturnService.PostAsync` | Payload sent to inventory on return posting |
| `AuditableEntity` | Domain Abstractions | All entities | Provides `Id`, `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `IsDeleted`, `DeletedByUserId` |
| `ICompanyEntity` | Domain Abstractions | All aggregate roots | Enforces `CompanyId` on entity |
| `BusinessErrors` | Application / Exceptions | All services | Centralised error factory (e.g. `CustomerNotFound`, `Unauthorized`, `InvalidStatus`) |
| `BusinessException` | Application / Exceptions | All services | Base exception class for domain rule violations |
