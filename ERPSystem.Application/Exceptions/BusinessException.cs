using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ERPSystem.Application.Authorization.Permissions.Sales;
namespace ERPSystem.Application.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }
        public int HttpStatusCode { get; }

        public BusinessException(string code, string message, int httpStatusCode = 400)
            : base(message)
        {
            Code = code;
            HttpStatusCode = httpStatusCode;
        }
    }

    public static class BusinessErrors
    {
        // Module Access
        public static BusinessException SalesModuleNotEnabled() =>
            new("SALES_MODULE_DISABLED", "Sales module is not enabled for this company.", 403);

        public static BusinessException InventoryModuleNotEnabled() =>
            new("INVENTORY_MODULE_DISABLED", "Inventory module is not enabled for this company. Delivery/Return posting is not available.", 403);
        // Hr Module
        public static BusinessException HrModuleNotEnabled()
        => new BusinessException("HR_MODULE_NOT_ENABLED", "HR module is not enabled for this company.", 403);

        public static BusinessException ContactModuleNotEnabled() =>
           new("Contact_MODULE_DISABLED", "Contact module is not enabled for this company.", 403);
        public static BusinessException ExpensesModuleNotEnabled() =>
           new("EXPENSES_MODULE_NOT_ENABLED", "Expenses module is not enabled for this company.", 403);
        public static BusinessException CrmModuleNotEnabled() => 
           new("CRM_MODULE_NOT_ENABLED", "CRM module is not enabled for this company.", 403);

        // Not Foundz
        public static BusinessException CustomerNotFound() =>
            new("CUSTOMER_NOT_FOUND", "Customer not found.", 404);

        public static BusinessException InvoiceNotFound() =>
            new("INVOICE_NOT_FOUND", "Sales invoice not found.", 404);

        public static BusinessException DeliveryNotFound() =>
            new("DELIVERY_NOT_FOUND", "Sales delivery not found.", 404);

        public static BusinessException ReceiptNotFound() =>
            new("RECEIPT_NOT_FOUND", "Sales receipt not found.", 404);

        public static BusinessException ReturnNotFound() =>
            new("RETURN_NOT_FOUND", "Sales return not found.", 404);

        public static BusinessException ProductNotFound() =>
            new("PRODUCT_NOT_FOUND", "Product not found.", 404);

        public static BusinessException WarehouseNotFound() =>
            new("WAREHOUSE_NOT_FOUND", "Warehouse not found.", 404);
        public static BusinessException ContactNotFound() =>
            new("Contact_NOT_FOUND", "Contact not found.", 404);
        public static BusinessException SalesInvoiceNotFound(int invoiceId) =>
            new("SALES_INVOICE_NOT_FOUND", $"Invoice {invoiceId} not found.", 404);
        public static BusinessException CompanyNotFound(int companyId) =>
            new("COMPANYID_NOT_FOUND", $"CompanyId {companyId} not found.", 404);
        public static BusinessException SalesReturnNotFound(int returnid) =>
            new("RETURNID_NOT_FOUND", $"ReturnId {returnid} not found.", 404);
        public static BusinessException SalesReceiptNotFound(int receiptid) =>
            new("RECEIPTID_NOT_FOUND", $"ReceiptId {receiptid} not found.", 404);
        public static BusinessException SalesDeliveryNotFound(int deliveryid) =>
            new("DELIVERYID_NOT_FOUND", $"DeliveryId {deliveryid} not found.", 404);

        // Validation
        public static BusinessException InvalidStatus(string message) =>
            new("INVALID_STATUS", message, 400);

        public static BusinessException InsufficientStock(string productName) =>
            new("INSUFFICIENT_STOCK", $"Insufficient stock for product: {productName}", 400);

        public static BusinessException CannotModifyPostedDocument() =>
            new("CANNOT_MODIFY_POSTED", "Cannot modify a posted document.", 400);

        public static BusinessException ExceedsRemainingQuantity() =>
            new("EXCEEDS_REMAINING_QTY", "Delivery quantity exceeds remaining quantity on invoice.", 400);

        public static BusinessException AllocationExceedsBalance() =>
            new("ALLOCATION_EXCEEDS_BALANCE", "Allocation amount exceeds invoice balance.", 400);
        public static BusinessException ExpenseNotFound() =>
           new("EXPENSE_NOT_FOUND", "Expense not found.", 404);

        public static BusinessException ExpenseCategoryNotFound() =>
            new("EXPENSE_CATEGORY_NOT_FOUND", "Expense category not found.", 404);
        public static BusinessException ExpenseCategoryInUse() =>
           new("EXPENSE_CATEGORY_IN_USE", "Cannot delete category because it has associated expenses.", 400);

        public static BusinessException DuplicateExpenseCategoryName() =>
            new("DUPLICATE_EXPENSE_CATEGORY_NAME", "A category with this name already exists.", 400);

        public static BusinessException InvalidExpenseStatus() =>
            new("INVALID_EXPENSE_STATUS", "Invalid expense status. Use 'Paid' or 'Pending'.", 400);

        public static BusinessException InvalidPaymentMethod() =>
            new("INVALID_PAYMENT_METHOD", "Invalid payment method.", 400);

        public static BusinessException InvalidTimeGrouping() =>
            new("INVALID_TIME_GROUPING", "Invalid time grouping. Use 'Day', 'Week', or 'Month'.", 400);
    

        // Authorization
        public static BusinessException Unauthorized(string message = "You do not have access to this resource.") =>
            new("UNAUTHORIZED", message, 403);
        //Roles
        public static BusinessException RoleNameRequired() =>
            new("ROLE_NAME_REQUIRED", "Role display name is required.", 400);

        public static BusinessException RoleAlreadyExists(string roleName) =>
            new("ROLE_ALREADY_EXISTS", $"Role '{roleName}' already exists for this company.", 409);

        public static BusinessException RoleNotFound(string roleName) =>
            new("ROLE_NOT_FOUND", $"Role '{roleName}' not found in this company.", 404);

        public static BusinessException UnknownPermission(string permissionKey) =>
            new("UNKNOWN_PERMISSION", $"Unknown permission: '{permissionKey}'.", 400);

        public static BusinessException EmptyPermissionKey() =>
            new("EMPTY_PERMISSION_KEY", "Permission key cannot be empty.", 400);

        public static BusinessException RoleOperationFailed(string operation, string details) =>
            new("ROLE_OPERATION_FAILED", $"Failed to {operation} role: {details}", 400);

        public static BusinessException UserNotFound() =>
            new("USER_NOT_FOUND", "User not found.", 404);

        public static BusinessException CannotAssignRoleAcrossCompanies() =>
            new("CROSS_COMPANY_ROLE_ASSIGNMENT", "Cannot assign or remove roles across companies.", 403);

        // ── Purchasing Module ─────────────────────────────────────

        public static BusinessException PurchasingModuleNotEnabled() =>
            new("PURCHASING_MODULE_DISABLED",
                "Purchasing module is not enabled for this company.", 403);

        public static BusinessException SupplierNotFound(int supplierId) =>
            new("SUPPLIER_NOT_FOUND", $"Supplier {supplierId} not found.", 404);

        public static BusinessException DuplicateSupplierCode(string code) =>
            new("DUPLICATE_SUPPLIER_CODE",
                $"Supplier code '{code}' is already in use.", 409);

        public static BusinessException PurchaseInvoiceNotFound(int invoiceId) =>
            new("PURCHASE_INVOICE_NOT_FOUND",
                $"Purchase invoice {invoiceId} not found.", 404);

        public static BusinessException PurchaseReturnNotFound(int returnId) =>
            new("PURCHASE_RETURN_NOT_FOUND",
                $"Purchase return {returnId} not found.", 404);

        public static BusinessException SupplierPaymentNotFound(int paymentId) =>
            new("SUPPLIER_PAYMENT_NOT_FOUND",
                $"Supplier payment {paymentId} not found.", 404);
        //  // Fix 10 — Optimistic concurrency conflict
        public static Exception ConcurrencyConflict() =>
            new InvalidOperationException(
                "The record was modified by another user. " +
                "Please reload the record and try again.");

        //  // Fix 9 — Supplier has active purchasing history
        public static Exception SupplierHasActiveDocuments() =>
            new InvalidOperationException(
                "This supplier cannot be deleted because they have active " +
                "purchase invoices, returns, or payments on record.");

        //  // Fix 3 — Over-return
        public static Exception ReturnQuantityExceeded(string productName, decimal available) =>
            new InvalidOperationException(
                $"Return quantity for '{productName}' exceeds the available " +
                $"returnable quantity of {available:N4} units.");

        //  // Fix 4 — Invoice belongs to a different supplier
        public static Exception InvoiceSupplierMismatch(string invoiceNumber) =>
            new InvalidOperationException(
                $"Invoice '{invoiceNumber}' belongs to a different supplier " +
                "than the one selected for this payment.");

        //  // Fix 5 — Invalid line field value
        public static Exception InvalidLineValue(string fieldName) =>
            new InvalidOperationException(
                $"Invalid line value: {fieldName}.");
    }
}