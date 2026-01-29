using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static BusinessException ContactModuleNotEnabled() =>
           new("Contact_MODULE_DISABLED", "Contact module is not enabled for this company.", 403);

        // Not Found
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

        // Authorization
        public static BusinessException Unauthorized(string message = "You do not have access to this resource.") =>
            new("UNAUTHORIZED", message, 403);
    }
}