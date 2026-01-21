using Microsoft.AspNetCore.Http;

namespace Transaction.Core.DTOs;


public class UpdateTransactionImagesDto
{
    public int TicketID { get; set; }
    public string? ChangeDescription { get; set; }
}
