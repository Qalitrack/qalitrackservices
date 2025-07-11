using AutoMapper;
using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Mappings;

public class ReportProfile : Profile
{
    public ReportProfile()
    {
        CreateMap<Report, ReportReadDto>();
        CreateMap<CreateReportDto, Report>();
        CreateMap<UpdateReportDto, Report>();
    }
}