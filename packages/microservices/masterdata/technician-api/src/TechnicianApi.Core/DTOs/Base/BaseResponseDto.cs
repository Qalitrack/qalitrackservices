using TechnicianApi.Core.DTOs.Attachment;

namespace TechnicianApi.Core.DTOs.Base;

public class BaseResponseDto
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public ICollection<AttachmentDto> Attachments { get; set; } = new List<AttachmentDto>();
}
