namespace Transaction.Core.Entities;

public class TransactionSettings : BaseEntity
{
    // Fixed, well-known id for the single settings row — a random default
    // id would let two concurrent "first ever save" requests each insert
    // their own row, with no defined "current" one.
    public const string SingletonId = "00000000-0000-0000-0000-000000000e03";

    // Singleton pattern - only one instance should exist
    public string ReceiptPrefix { get; set; } = "NCCU";
}
