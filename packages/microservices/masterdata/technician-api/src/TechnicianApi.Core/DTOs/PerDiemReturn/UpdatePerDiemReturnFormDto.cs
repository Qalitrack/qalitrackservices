namespace TechnicianApi.Core.DTOs.PerDiemReturn;

public class UpdatePerDiemReturnFormDto
{
    public string? ProjectName { get; set; }
    public decimal? FaresOrCarExpense { get; set; }
    public decimal? Mileage { get; set; }
    public decimal? Meals { get; set; }
    public decimal? Medical { get; set; }
    public decimal? Incidentals { get; set; }
}
