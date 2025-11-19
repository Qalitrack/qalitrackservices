namespace TechnicianApi.Core.DTOs.PerDiemReturn;

public class CreatePerDiemReturnFormDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string TechnicianId { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal FaresOrCarExpense { get; set; }
    public decimal Mileage { get; set; }
    public decimal Meals { get; set; }
    public decimal Medical { get; set; }
    public decimal Incidentals { get; set; }
}
