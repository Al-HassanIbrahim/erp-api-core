# 05 — MODULE: INVENTORY

---

## 1. Module Overview

The Inventory module manages physical stock across one or more warehouses, tracking quantities and valuations in real time using a **moving average cost** method. It provides five core stock operations (Stock In, Stock Out, Transfer, Opening Balance, Adjustment), each of which immediately posts and generates an `InventoryDocument` as a permanent audit trail. The module also exposes read-only reporting endpoints for stock balances, movement history, low-stock alerts, and inventory valuation, and is consumed as a dependency by the Sales module when posting deliveries (StockOut) and returns (StockIn).

---

## 2. Domain Entities

### 2.1 `Warehouse`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK (inherited from `BaseEntity`) |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch (optional) |
| `Code` | `string` | ✅ | Unique per company |
| `Name` | `string` | ✅ | Display name |
| `Address` | `string?` | ❌ | |
| `IsActive` | `bool` | ✅ | Default: `true`; set to `false` instead of deleting when inventory activity exists |
| `StockItems` | `ICollection<StockItem>` | — | Navigation |
| `InventoryDocuments` | `ICollection<InventoryDocument>` | — | Navigation |
| `InventoryDocumentLines` | `ICollection<InventoryDocumentLine>` | — | Navigation |

---

### 2.2 `InventoryDocument`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK (inherited from `AuditableEntity`) |
| `CompanyId` | `int` | ✅ | FK → Company |
| `BranchId` | `int?` | ❌ | FK → Branch |
| `DocNumber` | `string` | ✅ | Auto-generated; format varies by doc type (see §7) |
| `DocDate` | `DateTime` | ✅ | Document/transaction date |
| `DocType` | `InventoryDocType` | ✅ | Enum: `In`, `Out`, `Transfer`, `Opening`, `Adjustment` |
| `Status` | `InventoryDocumentStatus` | ✅ | Enum; always `Posted` immediately on creation |
| `DefaultWarehouseId` | `int?` | ❌ | FK → `Warehouse` (header-level default; lines may specify their own) |
| `Notes` | `string?` | ❌ | |
| `PostedByUserId` | `Guid?` | ❌ | User who created/posted the document |
| `PostedAt` | `DateTime?` | ❌ | Timestamp of posting |
| `SourceType` | `string?` | ❌ | Originating module/operation (e.g. `"SalesDelivery"`, `"SalesReturn"`, `"Adjustment"`, `"Opening"`) |
| `SourceId` | `int?` | ❌ | ID of the originating document in the source module |
| `DefaultWarehouse` | `Warehouse?` | — | Navigation |
| `Lines` | `ICollection<InventoryDocumentLine>` | — | Navigation |

---

### 2.3 `InventoryDocumentLine`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK (inherited from `BaseEntity`) |
| `InventoryDocumentId` | `int` | ✅ | FK → `InventoryDocument` |
| `ProductId` | `int` | ✅ | FK → `Product` |
| `WarehouseId` | `int` | ✅ | FK → `Warehouse` (line-level; overrides document default) |
| `Quantity` | `decimal` | ✅ | Always positive; direction determined by `LineType` |
| `UnitId` | `int` | ✅ | FK → `UnitOfMeasure` |
| `LineType` | `InventoryLineType` | ✅ | Enum: `In` or `Out` |
| `UnitCost` | `decimal?` | ❌ | Cost per unit at time of transaction; used in moving average calculation |
| `Notes` | `string?` | ❌ | |
| `Document` | `InventoryDocument` | — | Navigation |
| `Product` | `Product` | — | Navigation |
| `Warehouse` | `Warehouse` | — | Navigation |

---

### 2.4 `StockItem`

| Property | Type | Required | Description / FK |
|---|---|---|---|
| `Id` | `int` | ✅ | PK (inherited from `BaseEntity`) |
| `CompanyId` | `int` | ✅ | FK → Company |
| `WarehouseId` | `int` | ✅ | FK → `Warehouse` — composite key with `ProductId` for uniqueness |
| `ProductId` | `int` | ✅ | FK → `Product` — composite key with `WarehouseId` |
| `QuantityOnHand` | `decimal` | ✅ | Current stock quantity; updated on every transaction |
| `MinQuantity` | `decimal?` | ❌ | Low-stock alert threshold; set on Opening Balance |
| `MaxQuantity` | `decimal?` | ❌ | Upper stocking limit (informational) |
| `AverageUnitCost` | `decimal` | ✅ | Moving average cost per unit; recalculated on every In transaction |
| `LastUpdatedAt` | `DateTime` | ✅ | Timestamp of last stock movement |
| `Warehouse` | `Warehouse` | — | Navigation |
| `Product` | `Product` | — | Navigation |

> One `StockItem` record exists per (Product × Warehouse) combination. Records are created automatically on the first In, Transfer-to, Opening Balance, or positive Adjustment for a given pair.

---

## 3. Repository Interfaces

### 3.1 `IInventoryRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `AddDocumentAsync` | `InventoryDocument document, CancellationToken` | `Task` |
| `GetStockItemAsync` | `int productId, int warehouseId, CancellationToken` | `Task<StockItem?>` |
| `AddStockItemAsync` | `StockItem stockItem, CancellationToken` | `Task` |
| `SaveChangesAsync` | `CancellationToken` | `Task` |

> **Note:** There is no `Update` method; EF Core change tracking handles updates to `StockItem` and `InventoryDocument` in-memory before `SaveChangesAsync`.

---

### 3.2 `IWarehouseRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetAllAsync` | `int companyId, int? branchId` | `Task<List<Warehouse>>` |
| `GetByIdAsync` | `int id` | `Task<Warehouse?>` |
| `GetByIdAsync` *(overload)* | `int id, int companyId` | `Task<Warehouse?>` |
| `ExistsAsync` | `int id, int companyId` | `Task<bool>` |
| `CodeExistsAsync` | `string code, int companyId, int? excludeId` | `Task<bool>` |
| `AddAsync` | `Warehouse warehouse` | `Task` |
| `HasInventoryActivityAsync` | `int warehouseId` | `Task<bool>` |
| `SaveChangesAsync` | — | `Task` |

> `HasInventoryActivityAsync` checks for related `InventoryDocuments`, `InventoryDocumentLines`, or `StockItems`. Used to decide soft-delete vs. deactivation on delete.

---

### 3.3 `IInventoryReportsRepository`

| Method | Parameters | Return Type |
|---|---|---|
| `GetStockItemsAsync` | `int companyId, int? productId, int? warehouseId, CancellationToken` | `Task<List<StockItem>>` |
| `GetLowStockItemsAsync` | `int companyId, int? warehouseId, CancellationToken` | `Task<List<StockItem>>` |
| `GetMovementLinesAsync` | `int companyId, int productId, int? warehouseId, DateTime fromDate, DateTime toDate, CancellationToken` | `Task<List<InventoryDocumentLine>>` |

> `GetStockItemsAsync` eagerly loads: `Product` → `Category`, `Product` → `UnitOfMeasure`, `Warehouse`.
> `GetMovementLinesAsync` filters to `Status == Posted` documents only and eagerly loads: `Document`, `Product`, `Warehouse`.

---

## 4. Service Interfaces

### 4.1 `IInventoryService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `StockInAsync` | `StockInRequest request, CancellationToken` | `Task<InventoryDocumentResponse>` | Increases stock; creates StockItem if first movement; updates moving average cost; doc type: `In`; prefix: `IN-` |
| `StockOutAsync` | `StockOutRequest request, CancellationToken` | `Task<InventoryDocumentResponse>` | Decreases stock; validates sufficient quantity; doc type: `Out`; prefix: `OUT-`; StockItem must exist |
| `TransferAsync` | `StockTransferRequest request, CancellationToken` | `Task<InventoryDocumentResponse>` | Moves stock between warehouses; creates Out line on source + In line on destination within one document; doc type: `Transfer`; prefix: `TRF-` |
| `OpeningBalanceAsync` | `OpeningBalanceRequest request, CancellationToken` | `Task<InventoryDocumentResponse>` | Sets initial stock levels; skips zero-quantity lines silently; supports `MinQuantity` per line; doc type: `Opening`; prefix: `OPEN-` |
| `AdjustmentAsync` | `StockAdjustmentRequest request, CancellationToken` | `Task<InventoryDocumentResponse>` | Reconciles actual vs. system quantity; positive diff → In line; negative diff → Out line; zero diff → skipped; doc type: `Adjustment`; prefix: `ADJ-` |

---

### 4.2 `IWarehouseService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetAllAsync` | `int? branchId` | `Task<List<WarehouseDto>>` | Scoped to current company; optionally filtered by branch |
| `GetByIdAsync` | `int id` | `Task<WarehouseDto?>` | Returns `null` if not found or wrong company |
| `CreateAsync` | `CreateWarehouseDto dto` | `Task<int>` | Returns new warehouse `Id`; validates unique code per company |
| `UpdateAsync` | `int id, UpdateWarehouseDto dto` | `Task<bool>` | Returns `false` if not found; validates unique code excluding self |
| `DeleteAsync` | `int id` | `Task<bool>` | Returns `false` if not found; deactivates (`IsActive = false`) if inventory activity exists; physically soft-deletes (`IsDeleted = true`) otherwise |

---

### 4.3 `IInventoryReportsService`

| Method | Parameters | Return Type | Notes |
|---|---|---|---|
| `GetStockBalanceAsync` | `int? productId, int? warehouseId, CancellationToken` | `Task<IReadOnlyList<StockBalanceDto>>` | All params optional; returns all stock if both null |
| `GetWarehouseStockAsync` | `int warehouseId, CancellationToken` | `Task<IReadOnlyList<StockBalanceDto>>` | All products in one warehouse; delegates to `GetStockBalanceAsync` |
| `GetProductStockAsync` | `int productId, CancellationToken` | `Task<IReadOnlyList<StockBalanceDto>>` | One product across all warehouses; delegates to `GetStockBalanceAsync` |
| `GetMovementsAsync` | `int productId, int? warehouseId, DateTime fromDate, DateTime toDate, CancellationToken` | `Task<IReadOnlyList<InventoryMovementDto>>` | Movement history; only Posted documents included |
| `GetLowStockAsync` | `int? warehouseId, CancellationToken` | `Task<IReadOnlyList<LowStockItemDto>>` | Items where `QuantityOnHand ≤ MinQuantity`; only items with `MinQuantity` set |
| `GetInventoryValuationAsync` | `int? warehouseId, CancellationToken` | `Task<IReadOnlyList<StockBalanceDto>>` | Returns same shape as `GetStockBalanceAsync`; valuation = `QuantityOnHand × AverageUnitCost` (computed client-side) |

---

## 5. DTOs & View Models

### 5.1 Request DTOs

#### `StockInRequest`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `DocDate` | `DateTime` | |
| `SourceType` | `string?` | e.g. `"SalesReturn"`, `"Purchase"` |
| `SourceId` | `int?` | ID in source module |
| `Notes` | `string?` | |
| `Lines` | `List<StockInLineRequest>` | At least one required |

#### `StockInLineRequest`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `WarehouseId` | `int` | |
| `Quantity` | `decimal` | Must be > 0 |
| `UnitId` | `int` | |
| `UnitCost` | `decimal` | Must be ≥ 0; used in moving average calculation |
| `MinQuantity` | `decimal?` | Sets `StockItem.MinQuantity` on creation |
| `Notes` | `string?` | |

#### `StockOutRequest`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `DocDate` | `DateTime` | |
| `SourceType` | `string?` | e.g. `"SalesDelivery"` |
| `SourceId` | `int?` | |
| `Notes` | `string?` | |
| `Lines` | `List<StockOutLineRequest>` | At least one required |

#### `StockOutLineRequest`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `WarehouseId` | `int` | |
| `Quantity` | `decimal` | Must be > 0 |
| `UnitId` | `int` | |
| `Notes` | `string?` | |

> `UnitCost` is NOT on the request for StockOut — the service reads `StockItem.AverageUnitCost` automatically.

#### `StockTransferRequest`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `DocDate` | `DateTime` | |
| `SourceType` | `string?` | |
| `Notes` | `string?` | |
| `Lines` | `List<StockTransferLineRequest>` | At least one required |

#### `StockTransferLineRequest`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `FromWarehouseId` | `int` | Must differ from `ToWarehouseId` |
| `ToWarehouseId` | `int` | Must differ from `FromWarehouseId` |
| `Quantity` | `decimal` | Must be > 0 |
| `UnitId` | `int` | |
| `Notes` | `string?` | |

#### `OpeningBalanceRequest`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `DocDate` | `DateTime` | |
| `Notes` | `string?` | |
| `Lines` | `List<OpeningBalanceLineRequest>` | At least one required; zero-qty lines are silently skipped |

#### `OpeningBalanceLineRequest`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `WarehouseId` | `int` | |
| `Quantity` | `decimal` | Must be ≥ 0; zero lines are skipped |
| `UnitId` | `int` | |
| `UnitCost` | `decimal` | Must be ≥ 0 |
| `MinQuantity` | `decimal?` | Sets `StockItem.MinQuantity` |
| `Notes` | `string?` | |

#### `StockAdjustmentRequest`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `DocDate` | `DateTime` | |
| `Notes` | `string?` | |
| `Lines` | `List<StockAdjustmentLineRequest>` | At least one required |

#### `StockAdjustmentLineRequest`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `WarehouseId` | `int` | |
| `ActualQuantity` | `decimal` | Physical count; must be ≥ 0; diff vs. system quantity determines In/Out line |
| `UnitId` | `int` | |
| `UnitCost` | `decimal?` | Used when diff is positive (In); falls back to `StockItem.AverageUnitCost` if null |
| `Notes` | `string?` | |

#### `CreateWarehouseDto`

| Field | Type | Notes |
|---|---|---|
| `BranchId` | `int?` | |
| `Code` | `string` | Must be unique per company |
| `Name` | `string` | |
| `Address` | `string?` | |

#### `UpdateWarehouseDto`

| Field | Type | Notes |
|---|---|---|
| `Code` | `string` | Must be unique per company (excluding self) |
| `Name` | `string` | |
| `Address` | `string?` | |
| `IsActive` | `bool` | |

---

### 5.2 Response DTOs

#### `InventoryDocumentResponse`

| Field | Type | Notes |
|---|---|---|
| `DocumentId` | `int` | ID of the created `InventoryDocument` |
| `DocNumber` | `string` | Generated document number (e.g. `IN-20250620143022-A3F1`) |

#### `WarehouseDto`

| Field | Type |
|---|---|
| `Id` | `int` |
| `Code` | `string` |
| `Name` | `string` |
| `Address` | `string?` |
| `IsActive` | `bool` |

#### `StockBalanceDto`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `ProductCode` | `string` | |
| `ProductName` | `string` | |
| `CategoryName` | `string?` | From `Product.Category.Name` |
| `WarehouseId` | `int` | |
| `WarehouseName` | `string` | |
| `UnitName` | `string` | From `Product.UnitOfMeasure.Name` |
| `UnitSymbol` | `string` | From `Product.UnitOfMeasure.Symbol` |
| `QuantityOnHand` | `decimal` | |
| `AverageUnitCost` | `decimal` | Valuation = `QuantityOnHand × AverageUnitCost` |

#### `InventoryMovementDto`

| Field | Type | Notes |
|---|---|---|
| `DocumentId` | `int` | FK → `InventoryDocument` |
| `DocNumber` | `string` | |
| `DocDate` | `DateTime` | |
| `DocType` | `string` | Stringified `InventoryDocType` enum |
| `ProductId` | `int` | |
| `ProductCode` | `string` | |
| `ProductName` | `string` | |
| `WarehouseId` | `int` | |
| `WarehouseName` | `string` | |
| `LineType` | `string` | `"In"` or `"Out"` |
| `Quantity` | `decimal` | Always positive; direction from `LineType` |
| `UnitCost` | `decimal?` | Cost at time of movement |

#### `LowStockItemDto`

| Field | Type | Notes |
|---|---|---|
| `ProductId` | `int` | |
| `ProductCode` | `string` | |
| `ProductName` | `string` | |
| `WarehouseId` | `int` | |
| `WarehouseName` | `string` | |
| `QuantityOnHand` | `decimal` | |
| `MinQuantity` | `decimal?` | Alert threshold; only items with `MinQuantity` set appear here |

---

### 5.3 List / Summary DTOs

No dedicated list DTOs beyond the response DTOs above. `StockBalanceDto` serves as both the detail and list view for stock balance and valuation reports.

---

## 6. API Endpoints

### 6.1 Inventory Operations — `api/inventory`

Controller route: `api/[controller]` → `api/inventory`. All actions require `Inventory.Stock.Manage` policy.

| HTTP Verb | Route | Auth Policy | Request Body | Response | Description |
|---|---|---|---|---|---|
| `POST` | `api/inventory/in` | `Inventory.Stock.Manage` | `StockInRequest` | `InventoryDocumentResponse` | Receive stock into a warehouse; creates Posted document + updates StockItem |
| `POST` | `api/inventory/out` | `Inventory.Stock.Manage` | `StockOutRequest` | `InventoryDocumentResponse` | Issue stock from a warehouse; validates sufficient quantity |
| `POST` | `api/inventory/transfer` | `Inventory.Stock.Manage` | `StockTransferRequest` | `InventoryDocumentResponse` | Move stock between two warehouses in a single document |
| `POST` | `api/inventory/opening-balance` | `Inventory.Stock.Manage` | `OpeningBalanceRequest` | `InventoryDocumentResponse` | Set initial stock quantities and costs; skips zero-qty lines |
| `POST` | `api/inventory/adjustment` | `Inventory.Stock.Manage` | `StockAdjustmentRequest` | `InventoryDocumentResponse` | Reconcile physical count against system; auto-determines In/Out direction |

---

### 6.2 Inventory Reports — `api/inventoryreports`

Controller route: `api/[controller]` → `api/inventoryreports`. All actions share class-level policy `Inventory.Reports.Read`.

| HTTP Verb | Route | Auth Policy | Query Params | Response | Description |
|---|---|---|---|---|---|
| `GET` | `api/inventoryreports/stock-balance` | `Inventory.Reports.Read` | `?productId, ?warehouseId` | `StockBalanceDto[]` | Flexible stock balance; all params optional; returns all stock if none provided |
| `GET` | `api/inventoryreports/warehouse/{warehouseId}/stock` | `Inventory.Reports.Read` | — | `StockBalanceDto[]` | All products in one warehouse |
| `GET` | `api/inventoryreports/product/{productId}/stock` | `Inventory.Reports.Read` | — | `StockBalanceDto[]` | One product across all warehouses |
| `GET` | `api/inventoryreports/movements` | `Inventory.Reports.Read` | `?productId (required), ?warehouseId, ?from (required), ?to (required)` | `InventoryMovementDto[]` | Movement history (In/Out/Transfer/Adjustment) for a product in a date range |
| `GET` | `api/inventoryreports/low-stock` | `Inventory.Reports.Read` | `?warehouseId` | `LowStockItemDto[]` | Items below `MinQuantity`; only items with `MinQuantity` set are included |
| `GET` | `api/inventoryreports/valuation` | `Inventory.Reports.Read` | `?warehouseId` | `StockBalanceDto[]` | Stock valuation (quantity × average cost); same shape as stock balance |

---

### 6.3 Warehouses — `api/warehouses`

| HTTP Verb | Route | Auth Policy | Request | Response | Description |
|---|---|---|---|---|---|
| `GET` | `api/warehouses` | `Inventory.Warehouses.Read` | `?branchId` | `WarehouseDto[]` | All active warehouses for company; optional branch filter |
| `GET` | `api/warehouses/{id}` | `Inventory.Warehouses.Read` | — | `WarehouseDto` | Single warehouse by ID |
| `POST` | `api/warehouses` | `Inventory.Warehouses.Manage` | `CreateWarehouseDto` | `{ id }` (201) | Create warehouse; returns new ID |
| `PUT` | `api/warehouses/{id}` | `Inventory.Warehouses.Manage` | `UpdateWarehouseDto` | `204` | Update warehouse; `404` if not found |
| `DELETE` | `api/warehouses/{id}` | `Inventory.Warehouses.Manage` | — | `204` | Smart delete: deactivates if activity exists, soft-deletes if not; `404` if not found |

---

## 7. Business Rules & Validation

### 7.1 General Rules (All Operations)

| Rule | Detail |
|---|---|
| At least one line required | All five operations throw `InvalidOperationException` if `Lines` is null or empty |
| Product must be active and belong to company | `GetValidProductAsync` validates both; throws if product missing, deleted, or inactive |
| Warehouse must be active and belong to company | `GetValidWarehouseAsync` validates both; throws if warehouse missing, deleted, or inactive |
| Branch consistency | If request has `BranchId` and warehouse has a different `BranchId`, operation is blocked |
| All documents are immediately Posted | No Draft → Post flow; every operation creates a document with `Status = Posted` on the same call |
| Documents are immutable | There is no update or cancel operation on inventory documents; corrections are made via new Adjustment documents |

---

### 7.2 Stock In Rules

| Rule | Detail |
|---|---|
| Quantity must be > 0 | Per line; throws if ≤ 0 |
| UnitCost must be ≥ 0 | Per line; throws if negative |
| StockItem auto-created | If no StockItem exists for the (Product × Warehouse) pair, one is created with `QuantityOnHand = 0` |
| Moving average cost updated | `NewAvg = (CurrentQty × CurrentAvg + InQty × InCost) / (CurrentQty + InQty)` |
| If current qty ≤ 0 | `AverageUnitCost` is set directly to `InCost` (no weighted average needed) |

---

### 7.3 Stock Out Rules

| Rule | Detail |
|---|---|
| Quantity must be > 0 | Per line; throws if ≤ 0 |
| StockItem must exist | Throws `"Stock item does not exist"` if no record found; does NOT auto-create |
| Sufficient quantity required | Throws if `StockItem.QuantityOnHand < requested quantity` with available vs. requested amounts in message |
| UnitCost on line | Automatically taken from `StockItem.AverageUnitCost`; caller cannot override |

---

### 7.4 Transfer Rules

| Rule | Detail |
|---|---|
| Quantity must be > 0 | Per line |
| Source and destination must differ | `FromWarehouseId != ToWarehouseId`; throws if same |
| Source StockItem must exist | Does NOT auto-create; throws if missing |
| Sufficient quantity in source | Validates against source `QuantityOnHand` |
| Cost preserved | `transferUnitCost = fromStock.AverageUnitCost` used for both the Out line and the In moving average update on the destination |
| Destination StockItem auto-created | Creates if not found, then applies incoming cost |
| Single document, two lines | One document with `DocType = Transfer` containing one `Out` line (source) and one `In` line (destination) |

---

### 7.5 Opening Balance Rules

| Rule | Detail |
|---|---|
| Quantity must be ≥ 0 | Negative quantities throw; zero quantities are silently skipped |
| UnitCost must be ≥ 0 | Throws if negative |
| StockItem auto-created | Always creates if missing; applies `MinQuantity` from request line |
| Moving average updated | Same formula as Stock In |
| Intended for initial setup | No guard against multiple Opening Balance documents; it is the caller's responsibility to run this only once per product/warehouse |

---

### 7.6 Adjustment Rules

| Rule | Detail |
|---|---|
| ActualQuantity must be ≥ 0 | Throws if negative |
| Diff = 0 | Line is skipped silently |
| Diff > 0 (In) | StockItem auto-created if missing; `UnitCost` from request or falls back to `StockItem.AverageUnitCost`; moving average updated |
| Diff < 0 (Out) | StockItem must exist; throws if attempting negative adjustment with no stock record; `QuantityOnHand` reduced by `Math.Abs(diff)` |
| UnitCost for negative adjustment | Always uses `StockItem.AverageUnitCost`; cannot be overridden |

---

### 7.7 Warehouse Rules

| Rule | Detail |
|---|---|
| Unique code per company | `Code` must be unique within `CompanyId`; enforced on create and update |
| Smart delete | If `HasInventoryActivityAsync` returns `true` → set `IsActive = false` (preserve history); otherwise set `IsDeleted = true` (physical soft-delete) |
| Company isolation | All warehouse reads and writes scoped to `currentUser.CompanyId` |

---

### 7.8 Document Number Formats

| Operation | Prefix | Format | Example |
|---|---|---|---|
| Stock In | `IN` | `IN-{yyyyMMddHHmmss}-{4 random hex chars}` | `IN-20250620143022-A3F1` |
| Stock Out | `OUT` | `OUT-{yyyyMMddHHmmss}-{4 random hex chars}` | `OUT-20250620143022-B9C2` |
| Transfer | `TRF` | `TRF-{yyyyMMddHHmmss}-{4 random hex chars}` | `TRF-20250620143022-D7E4` |
| Opening Balance | `OPEN` | `OPEN-{yyyyMMddHHmmss}-{4 random hex chars}` | `OPEN-20250620143022-F2A8` |
| Adjustment | `ADJ` | `ADJ-{yyyyMMddHHmmss}-{4 random hex chars}` | `ADJ-20250620143022-3C11` |

> Numbers include a timestamp + 4-char random hex suffix from a GUID. Uniqueness is probabilistic, not guaranteed under extreme concurrency. The suffix is uppercase.

---

### 7.9 Status Enums

#### `InventoryDocumentStatus`

```
(No transitions) — All documents are created directly as Posted.
Draft state does not exist in this module.
```

| Value | Meaning |
|---|---|
| `Posted` | Document is finalized and has affected StockItem quantities |

#### `InventoryDocType`

| Value | Triggered By | Lines |
|---|---|---|
| `In` | `StockInAsync` | All lines are `In` |
| `Out` | `StockOutAsync` | All lines are `Out` |
| `Transfer` | `TransferAsync` | Mixed: `Out` (source) + `In` (destination) per item |
| `Opening` | `OpeningBalanceAsync` | All lines are `In` |
| `Adjustment` | `AdjustmentAsync` | Mixed: `In` (positive diff) or `Out` (negative diff) per item |

#### `InventoryLineType`

| Value | Effect on Stock |
|---|---|
| `In` | Increases `StockItem.QuantityOnHand`; updates moving average cost |
| `Out` | Decreases `StockItem.QuantityOnHand`; cost taken from current average |

---

## 8. Cross-Module Dependencies

| Interface / Entity | Source Module | Used By (Inventory) | Purpose |
|---|---|---|---|
| `ICurrentUserService` | Application / Auth | `InventoryService`, `InventoryReportsService`, `WarehouseService` | Provides `CompanyId` and `UserId` for company isolation and audit trail |
| `IProductRepository` | Products Module | `InventoryService` | `GetByIdAsync(productId, companyId)` — validates product existence, active status, and company ownership before any stock operation |
| `Product` (entity) | Products Module | `InventoryDocumentLine`, `StockItem` | Navigation property; used to populate `ProductCode`, `ProductName`, `CategoryName`, `UnitName`, `UnitSymbol` in report DTOs |
| `AuditableEntity` | Domain Abstractions | `InventoryDocument` | Provides `Id`, `CreatedAt`, `CreatedByUserId`, `UpdatedAt`, `UpdatedByUserId`, `IsDeleted`, `DeletedByUserId` |
| `BaseEntity` | Domain Abstractions | `InventoryDocumentLine`, `StockItem`, `Warehouse` | Provides `Id` and `IsDeleted` |
| `ICompanyEntity` | Domain Abstractions | `InventoryDocument`, `StockItem`, `Warehouse` | Enforces `CompanyId` property on entity |

### Consumed by Other Modules

| Interface | Consumed By | How |
|---|---|---|
| `IInventoryService` | Sales Module (`SalesDeliveryService`, `SalesReturnService`) | `StockOutAsync` called when posting a `SalesDelivery`; `StockInAsync` called when posting a `SalesReturn` |
| `IWarehouseRepository` | Sales Module (`SalesDeliveryService`, `SalesReturnService`) | `GetByIdAsync(id)` to validate warehouse existence and company ownership before creating a delivery or return |
| `Warehouse` (entity) | Sales Module | Navigation property on `SalesDelivery` and `SalesReturn` for warehouse name in response DTOs |
