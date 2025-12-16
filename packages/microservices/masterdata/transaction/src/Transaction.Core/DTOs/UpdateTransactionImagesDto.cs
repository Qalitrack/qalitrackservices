using Microsoft.AspNetCore.Http;

namespace Transaction.Core.DTOs;

public class UpdateTransactionImagesDto
{
    public string TransactionId { get; set; } = string.Empty;
    public IFormFile? NprImage { get; set; }
    public IFormFile? TransactionImage { get; set; }
    public string? ChangeDescription { get; set; }
}
