namespace ERPSystem.Domain.Enums
{
    public enum ExpenseStatus
    {
        Pending = 0,
        Paid = 1
    }

    public enum PaymentMethod
    {
        Cash = 0,
        CreditCard = 1,
        DebitCard = 2,
        BankTransfer = 3,
        Check = 4,
        Other = 5
    }
}