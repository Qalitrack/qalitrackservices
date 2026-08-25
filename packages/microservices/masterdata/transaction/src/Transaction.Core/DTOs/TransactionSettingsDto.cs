namespace Transaction.Core.DTOs;

public class TransactionSettingsResponseDto
{
    public string ReceiptPrefix { get; set; } = "NCCU";
}

public class UpdateTransactionSettingsDto
{
    public string ReceiptPrefix { get; set; } = "NCCU";
}
