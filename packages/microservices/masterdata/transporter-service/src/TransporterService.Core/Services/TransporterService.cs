using AutoMapper;
using TransporterService.Core.DTOs;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;

namespace TransporterService.Core.Services;

public class TransporterService : ITransporterService
{
    private readonly ITransporterRepository _transporterRepository;
    private readonly ITransporterContactRepository _contactRepository;
    private readonly ITransporterFleetRepository _fleetRepository;
    private readonly ITransporterDriverRepository _driverRepository;
    private readonly ITransporterLicenseRepository _licenseRepository;
    private readonly ITransporterInsuranceRepository _insuranceRepository;
    private readonly ITransporterContractRepository _contractRepository;
    private readonly ITransporterPerformanceRepository _performanceRepository;
    private readonly IMapper _mapper;

    public TransporterService(
        ITransporterRepository transporterRepository,
        ITransporterContactRepository contactRepository,
        ITransporterFleetRepository fleetRepository,
        ITransporterDriverRepository driverRepository,
        ITransporterLicenseRepository licenseRepository,
        ITransporterInsuranceRepository insuranceRepository,
        ITransporterContractRepository contractRepository,
        ITransporterPerformanceRepository performanceRepository,
        IMapper mapper)
    {
        _transporterRepository = transporterRepository;
        _contactRepository = contactRepository;
        _fleetRepository = fleetRepository;
        _driverRepository = driverRepository;
        _licenseRepository = licenseRepository;
        _insuranceRepository = insuranceRepository;
        _contractRepository = contractRepository;
        _performanceRepository = performanceRepository;
        _mapper = mapper;
    }

    // Transporter Management
    public async Task<TransporterDto> RegisterTransporterAsync(RegisterTransporterRequest request)
    {
        // Validate unique registration number
        if (!await _transporterRepository.IsRegistrationNumberUniqueAsync(request.RegistrationNumber))
        {
            throw new InvalidOperationException($"Registration number {request.RegistrationNumber} already exists.");
        }

        var transporter = new Transporter
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            TaxNumber = request.TaxNumber,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            TransporterType = request.TransporterType,
            FleetSize = request.FleetSize,
            OperatingLicense = request.OperatingLicense,
            LicenseExpiryDate = request.LicenseExpiryDate,
            Website = request.Website,
            Description = request.Description,
            Status = TransporterStatus.Active
        };

        var result = await _transporterRepository.AddAsync(transporter);
        return _mapper.Map<TransporterDto>(result);
    }

    public async Task<TransporterDto> GetTransporterByIdAsync(string id)
    {
        var transporter = await _transporterRepository.GetByIdAsync(id);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {id} not found.");

        return _mapper.Map<TransporterDto>(transporter);
    }

    public async Task<IEnumerable<TransporterDto>> GetAllTransportersAsync()
    {
        var transporters = await _transporterRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters.Where(t => !t.IsDeleted));
    }

    public async Task<IEnumerable<TransporterDto>> GetActiveTransportersAsync()
    {
        var transporters = await _transporterRepository.GetActiveTransportersAsync();
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }

    public async Task<IEnumerable<TransporterDto>> GetTransportersByTypeAsync(TransporterType type)
    {
        var transporters = await _transporterRepository.GetTransportersByTypeAsync(type);
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }

    public async Task<IEnumerable<TransporterDto>> GetTransportersByStatusAsync(TransporterStatus status)
    {
        var transporters = await _transporterRepository.GetTransportersByStatusAsync(status);
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }

    public async Task<TransporterDto> UpdateTransporterAsync(string id, UpdateTransporterRequest request)
    {
        var transporter = await _transporterRepository.GetByIdAsync(id);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {id} not found.");

        // Update properties
        transporter.Name = request.Name;
        transporter.TaxNumber = request.TaxNumber;
        transporter.ContactEmail = request.ContactEmail;
        transporter.ContactPhone = request.ContactPhone;
        transporter.Address = request.Address;
        transporter.City = request.City;
        transporter.State = request.State;
        transporter.Country = request.Country;
        transporter.PostalCode = request.PostalCode;
        transporter.FleetSize = request.FleetSize;
        transporter.OperatingLicense = request.OperatingLicense;
        transporter.LicenseExpiryDate = request.LicenseExpiryDate;
        transporter.Status = request.Status;
        transporter.Website = request.Website;
        transporter.Description = request.Description;

        var result = await _transporterRepository.UpdateAsync(transporter);
        return _mapper.Map<TransporterDto>(result);
    }

    public async Task<bool> DeleteTransporterAsync(string id)
    {
        var transporter = await _transporterRepository.GetByIdAsync(id);
        if (transporter == null)
            return false;

        transporter.IsDeleted = true;
        await _transporterRepository.UpdateAsync(transporter);
        return true;
    }

    public async Task<IEnumerable<TransporterDto>> SearchTransportersAsync(string searchTerm)
    {
        var transporters = await _transporterRepository.SearchTransportersAsync(searchTerm);
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }

    public async Task<IEnumerable<TransporterDto>> GetAvailableTransportersAsync(DateTime date, string? routeId = null)
    {
        var transporters = await _transporterRepository.GetAvailableTransportersAsync(date, routeId);
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }

    // Contact Management
    public async Task<TransporterContactDto> AddContactAsync(CreateTransporterContactRequest request)
    {
        // Verify transporter exists
        var transporter = await _transporterRepository.GetByIdAsync(request.TransporterId);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {request.TransporterId} not found.");

        var contact = _mapper.Map<TransporterContact>(request);
        var result = await _contactRepository.AddAsync(contact);
        return _mapper.Map<TransporterContactDto>(result);
    }

    public async Task<IEnumerable<TransporterContactDto>> GetContactsByTransporterIdAsync(string transporterId)
    {
        var contacts = await _contactRepository.GetContactsByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterContactDto>>(contacts);
    }

    public async Task<TransporterContactDto?> GetPrimaryContactAsync(string transporterId)
    {
        var contact = await _contactRepository.GetPrimaryContactAsync(transporterId);
        return contact != null ? _mapper.Map<TransporterContactDto>(contact) : null;
    }

    public async Task<bool> DeleteContactAsync(string contactId)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
            return false;

        contact.IsDeleted = true;
        await _contactRepository.UpdateAsync(contact);
        return true;
    }

    // Fleet Management
    public async Task<TransporterFleetDto> AddFleetVehicleAsync(AddFleetVehicleRequest request)
    {
        // Verify transporter exists
        var transporter = await _transporterRepository.GetByIdAsync(request.TransporterId);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {request.TransporterId} not found.");

        // Validate unique registration number
        if (!await _fleetRepository.IsRegistrationNumberUniqueAsync(request.RegistrationNumber))
        {
            throw new InvalidOperationException($"Vehicle registration number {request.RegistrationNumber} already exists.");
        }

        var vehicle = _mapper.Map<TransporterFleet>(request);
        var result = await _fleetRepository.AddAsync(vehicle);
        return _mapper.Map<TransporterFleetDto>(result);
    }

    public async Task<IEnumerable<TransporterFleetDto>> GetFleetByTransporterIdAsync(string transporterId)
    {
        var fleet = await _fleetRepository.GetFleetByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterFleetDto>>(fleet);
    }

    public async Task<IEnumerable<TransporterFleetDto>> GetAvailableVehiclesAsync(string transporterId)
    {
        var vehicles = await _fleetRepository.GetAvailableVehiclesAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterFleetDto>>(vehicles);
    }

    public async Task<bool> DeleteFleetVehicleAsync(string vehicleId)
    {
        var vehicle = await _fleetRepository.GetByIdAsync(vehicleId);
        if (vehicle == null)
            return false;

        vehicle.IsDeleted = true;
        await _fleetRepository.UpdateAsync(vehicle);
        return true;
    }

    // Driver Management
    public async Task<TransporterDriverDto> AssignDriverAsync(AssignDriverRequest request)
    {
        // Verify transporter exists
        var transporter = await _transporterRepository.GetByIdAsync(request.TransporterId);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {request.TransporterId} not found.");

        var driver = _mapper.Map<TransporterDriver>(request);
        var result = await _driverRepository.AddAsync(driver);
        return _mapper.Map<TransporterDriverDto>(result);
    }

    public async Task<IEnumerable<TransporterDriverDto>> GetDriversByTransporterIdAsync(string transporterId)
    {
        var drivers = await _driverRepository.GetDriversByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterDriverDto>>(drivers);
    }

    public async Task<IEnumerable<TransporterDriverDto>> GetActiveDriversAsync(string transporterId)
    {
        var drivers = await _driverRepository.GetActiveDriversAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterDriverDto>>(drivers);
    }

    public async Task<bool> UnassignDriverAsync(string driverId)
    {
        var driver = await _driverRepository.GetByIdAsync(driverId);
        if (driver == null)
            return false;

        driver.Status = DriverStatus.Terminated;
        driver.TerminationDate = DateTime.UtcNow;
        await _driverRepository.UpdateAsync(driver);
        return true;
    }

    // License Management
    public async Task<IEnumerable<TransporterLicenseDto>> GetLicensesByTransporterIdAsync(string transporterId)
    {
        var licenses = await _licenseRepository.GetLicensesByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterLicenseDto>>(licenses);
    }

    public async Task<IEnumerable<TransporterLicenseDto>> GetExpiringLicensesAsync(string transporterId, int daysAhead = 30)
    {
        var licenses = await _licenseRepository.GetExpiringLicensesAsync(transporterId, daysAhead);
        return _mapper.Map<IEnumerable<TransporterLicenseDto>>(licenses);
    }

    // Insurance Management
    public async Task<IEnumerable<TransporterInsuranceDto>> GetInsuranceByTransporterIdAsync(string transporterId)
    {
        var insurance = await _insuranceRepository.GetInsuranceByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterInsuranceDto>>(insurance);
    }

    public async Task<IEnumerable<TransporterInsuranceDto>> GetExpiringInsuranceAsync(string transporterId, int daysAhead = 30)
    {
        var insurance = await _insuranceRepository.GetExpiringInsuranceAsync(transporterId, daysAhead);
        return _mapper.Map<IEnumerable<TransporterInsuranceDto>>(insurance);
    }

    // Contract Management
    public async Task<IEnumerable<TransporterContractDto>> GetContractsByTransporterIdAsync(string transporterId)
    {
        var contracts = await _contractRepository.GetContractsByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterContractDto>>(contracts);
    }

    public async Task<IEnumerable<TransporterContractDto>> GetActiveContractsAsync(string transporterId)
    {
        var contracts = await _contractRepository.GetActiveContractsAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterContractDto>>(contracts);
    }

    // Performance Management
    public async Task<IEnumerable<TransporterPerformanceDto>> GetPerformanceByTransporterIdAsync(string transporterId)
    {
        var performance = await _performanceRepository.GetPerformanceByTransporterIdAsync(transporterId);
        return _mapper.Map<IEnumerable<TransporterPerformanceDto>>(performance);
    }

    public async Task<TransporterPerformanceSummaryDto> GetPerformanceSummaryAsync(string transporterId)
    {
        var transporter = await _transporterRepository.GetByIdAsync(transporterId);
        if (transporter == null)
            throw new KeyNotFoundException($"Transporter with ID {transporterId} not found.");

        var performances = await _performanceRepository.GetPerformanceByTransporterIdAsync(transporterId);
        var performanceList = performances.ToList();

        var summary = new TransporterPerformanceSummaryDto
        {
            TransporterId = transporterId,
            TransporterName = transporter.Name,
            OverallRating = transporter.Rating ?? 0,
            LastUpdated = DateTime.UtcNow
        };

        if (performanceList.Any())
        {
            var onTimeDelivery = performanceList.Where(p => p.MetricType == PerformanceMetricType.OnTimeDelivery).OrderByDescending(p => p.RecordDate).FirstOrDefault();
            var safety = performanceList.Where(p => p.MetricType == PerformanceMetricType.SafetyRecord).OrderByDescending(p => p.RecordDate).FirstOrDefault();
            var customerSat = performanceList.Where(p => p.MetricType == PerformanceMetricType.CustomerSatisfaction).OrderByDescending(p => p.RecordDate).FirstOrDefault();

            summary.OnTimeDeliveryRate = onTimeDelivery?.Value ?? 0;
            summary.SafetyScore = safety?.Value ?? 0;
            summary.CustomerSatisfactionScore = customerSat?.Value ?? 0;
            summary.TotalTripsCompleted = performanceList.Sum(p => p.TotalTrips);
            summary.TotalDistanceCovered = performanceList.Sum(p => p.TotalDistance);
            summary.TotalAccidents = performanceList.Sum(p => p.AccidentCount);
            summary.TotalViolations = performanceList.Sum(p => p.ViolationCount);
        }

        return summary;
    }

    // Dual-Role Management
    public async Task<TransporterDto> EnableCustomerRoleAsync(string transporterId, EnableCustomerRoleDto enableCustomerRoleDto)
    {
        var transporter = await _transporterRepository.GetByIdAsync(transporterId);
        if (transporter == null)
        {
            throw new KeyNotFoundException($"Transporter with ID {transporterId} not found");
        }

        transporter.IsCustomer = true;
        transporter.CustomerServiceReference = enableCustomerRoleDto.CustomerServiceReference;
        transporter.CustomerRegistrationDate = DateTime.UtcNow;
        transporter.CustomerNotes = enableCustomerRoleDto.CustomerNotes;
        transporter.UpdatedAt = DateTime.UtcNow;

        await _transporterRepository.UpdateAsync(transporter);
        return _mapper.Map<TransporterDto>(transporter);
    }

    public async Task<TransporterDto> DisableCustomerRoleAsync(string transporterId)
    {
        var transporter = await _transporterRepository.GetByIdAsync(transporterId);
        if (transporter == null)
        {
            throw new KeyNotFoundException($"Transporter with ID {transporterId} not found");
        }

        transporter.IsCustomer = false;
        transporter.CustomerServiceReference = null;
        transporter.CustomerRegistrationDate = null;
        transporter.CustomerNotes = null;
        transporter.UpdatedAt = DateTime.UtcNow;

        await _transporterRepository.UpdateAsync(transporter);
        return _mapper.Map<TransporterDto>(transporter);
    }

    public async Task<IEnumerable<TransporterDto>> GetDualRoleTransportersAsync()
    {
        var transporters = await _transporterRepository.GetDualRoleTransportersAsync();
        return _mapper.Map<IEnumerable<TransporterDto>>(transporters);
    }
}