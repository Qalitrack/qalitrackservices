using AutoMapper;
using TransporterService.Core.DTOs;
using TransporterService.Core.Entities;

namespace TransporterService.Core.Mappings;

public class TransporterProfile : Profile
{
    public TransporterProfile()
    {
        CreateMap<Transporter, TransporterDto>();
        CreateMap<RegisterTransporterRequest, Transporter>();
        CreateMap<UpdateTransporterRequest, Transporter>();

        CreateMap<TransporterContact, TransporterContactDto>();
        CreateMap<CreateTransporterContactRequest, TransporterContact>();

        CreateMap<TransporterFleet, TransporterFleetDto>();
        CreateMap<AddFleetVehicleRequest, TransporterFleet>();

        CreateMap<TransporterDriver, TransporterDriverDto>();
        CreateMap<AssignDriverRequest, TransporterDriver>();

        CreateMap<TransporterLicense, TransporterLicenseDto>();

        CreateMap<TransporterInsurance, TransporterInsuranceDto>();

        CreateMap<TransporterContract, TransporterContractDto>();

        CreateMap<TransporterPerformance, TransporterPerformanceDto>();
    }
}