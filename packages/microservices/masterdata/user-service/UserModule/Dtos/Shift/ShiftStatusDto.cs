namespace UserModule.Dtos.Shift;

public class ShiftStatusDto
{
    public Guid ShiftId { get; set; }
    public string Name { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsActiveNow { get; set; }
    public int UserCount { get; set; }
    public string Mode { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}