using AutoMapper;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Transporters;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Entities;

namespace Masterdata.Core.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Driver mappings
        CreateMap<CreateDriverDto, Driver>();
        CreateMap<UpdateDriverDto, Driver>();
        CreateMap<Driver, DriverReadDto>();

        // Transporter mappings
        CreateMap<CreateTransporterDto, Transporter>();
        CreateMap<UpdateTransporterDto, Transporter>();
        CreateMap<Transporter, TransporterReadDto>();

        // Vehicle mappings
        CreateMap<CreateVehicleDto, Vehicle>();
        CreateMap<UpdateVehicleDto, Vehicle>();
        CreateMap<Vehicle, VehicleReadDto>();
        
        // Add other mappings as needed
    }
}
