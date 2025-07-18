using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Core.Services;

public class ContractService : IContractService
{
    private readonly IRepository<Contract> _contractRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<ContractRenewal> _renewalRepository;
    private readonly IMapper _mapper;

    public ContractService(
        IRepository<Contract> contractRepository,
        ICustomerRepository customerRepository,
        IRepository<ContractRenewal> renewalRepository,
        IMapper mapper)
    {
        _contractRepository = contractRepository;
        _customerRepository = customerRepository;
        _renewalRepository = renewalRepository;
        _mapper = mapper;
    }

    public async Task<ContractReadDto> CreateContractAsync(CreateContractDto createContractDto)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(createContractDto.CustomerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {createContractDto.CustomerId} not found");
        }

        // Validate unique contract number
        var existingContract = await _contractRepository.FirstOrDefaultAsync(c => c.ContractNumber == createContractDto.ContractNumber);
        if (existingContract != null)
        {
            throw new InvalidOperationException($"Contract with number {createContractDto.ContractNumber} already exists");
        }

        var contract = _mapper.Map<Contract>(createContractDto);
        contract.Id = Guid.NewGuid().ToString();
        contract.CreatedAt = DateTime.UtcNow;
        contract.UpdatedAt = DateTime.UtcNow;

        // Calculate next renewal date if auto-renew is enabled
        if (contract.AutoRenew && contract.RenewalPeriodMonths.HasValue)
        {
            contract.NextRenewalDate = contract.EndDate.AddMonths(-1); // Notify 1 month before expiry
        }

        var createdContract = await _contractRepository.CreateAsync(contract);
        return _mapper.Map<ContractReadDto>(createdContract);
    }

    public async Task<ContractReadDto?> GetContractAsync(string id)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        return contract != null ? _mapper.Map<ContractReadDto>(contract) : null;
    }

    public async Task<IEnumerable<ContractReadDto>> GetCustomerContractsAsync(string customerId)
    {
        var contracts = await _contractRepository.FindAsync(c => c.CustomerId == customerId);
        return _mapper.Map<IEnumerable<ContractReadDto>>(contracts);
    }

    public async Task<ContractReadDto?> UpdateContractAsync(string id, UpdateContractDto updateContractDto)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null)
        {
            return null;
        }

        // Validate unique contract number if changed
        if (!string.IsNullOrEmpty(updateContractDto.ContractNumber) && 
            updateContractDto.ContractNumber != contract.ContractNumber)
        {
            var existingContract = await _contractRepository.FirstOrDefaultAsync(c => c.ContractNumber == updateContractDto.ContractNumber);
            if (existingContract != null)
            {
                throw new InvalidOperationException($"Contract with number {updateContractDto.ContractNumber} already exists");
            }
        }

        _mapper.Map(updateContractDto, contract);
        contract.UpdatedAt = DateTime.UtcNow;

        // Recalculate next renewal date if auto-renew settings changed
        if (contract.AutoRenew && contract.RenewalPeriodMonths.HasValue)
        {
            contract.NextRenewalDate = contract.EndDate.AddMonths(-1);
        }
        else
        {
            contract.NextRenewalDate = null;
        }

        var updatedContract = await _contractRepository.UpdateAsync(contract);
        return updatedContract != null ? _mapper.Map<ContractReadDto>(updatedContract) : null;
    }

    public async Task<bool> DeleteContractAsync(string id)
    {
        return await _contractRepository.DeleteAsync(id);
    }

    public async Task<bool> ActivateContractAsync(string id)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null) return false;

        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = DateTime.UtcNow;
        await _contractRepository.UpdateAsync(contract);
        return true;
    }

    public async Task<bool> SuspendContractAsync(string id, string? reason = null)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null) return false;

        contract.Status = ContractStatus.Suspended;
        contract.UpdatedAt = DateTime.UtcNow;
        await _contractRepository.UpdateAsync(contract);
        return true;
    }

    public async Task<bool> TerminateContractAsync(string id, string? reason = null)
    {
        var contract = await _contractRepository.GetByIdAsync(id);
        if (contract == null) return false;

        contract.Status = ContractStatus.Terminated;
        contract.UpdatedAt = DateTime.UtcNow;
        await _contractRepository.UpdateAsync(contract);
        return true;
    }

    public async Task<ContractRenewalReadDto> RenewContractAsync(string contractId, RenewContractDto renewalDto)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new ArgumentException($"Contract with ID {contractId} not found");
        }

        // Create renewal record
        var renewal = new ContractRenewal
        {
            Id = Guid.NewGuid().ToString(),
            ContractId = contractId,
            RenewalDate = DateTime.UtcNow,
            NewEndDate = renewalDto.NewEndDate,
            NewContractValue = renewalDto.NewContractValue,
            RenewalTerms = renewalDto.RenewalTerms,
            RenewedBy = renewalDto.RenewedBy,
            Notes = renewalDto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdRenewal = await _renewalRepository.CreateAsync(renewal);

        // Update contract with new end date and value
        contract.EndDate = renewalDto.NewEndDate;
        if (renewalDto.NewContractValue.HasValue)
        {
            contract.ContractValue = renewalDto.NewContractValue.Value;
        }
        contract.Status = ContractStatus.Active;
        contract.UpdatedAt = DateTime.UtcNow;

        // Calculate next renewal date if auto-renew is enabled
        if (contract.AutoRenew && contract.RenewalPeriodMonths.HasValue)
        {
            contract.NextRenewalDate = contract.EndDate.AddMonths(-1);
        }

        await _contractRepository.UpdateAsync(contract);

        return _mapper.Map<ContractRenewalReadDto>(createdRenewal);
    }

    public async Task<IEnumerable<ContractReadDto>> GetExpiringContractsAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        var expiringContracts = await _contractRepository.FindAsync(c => 
            c.Status == ContractStatus.Active && 
            c.EndDate <= cutoffDate);
        
        return _mapper.Map<IEnumerable<ContractReadDto>>(expiringContracts);
    }

    public async Task<IEnumerable<ContractReadDto>> GetContractsForRenewalNotificationAsync()
    {
        var today = DateTime.UtcNow.Date;
        var contractsForNotification = await _contractRepository.FindAsync(c => 
            c.AutoRenew && 
            c.NextRenewalDate.HasValue && 
            c.NextRenewalDate.Value.Date <= today &&
            c.Status == ContractStatus.Active);
        
        return _mapper.Map<IEnumerable<ContractReadDto>>(contractsForNotification);
    }

    public async Task<IEnumerable<ContractRenewalReadDto>> GetContractRenewalsAsync(string contractId)
    {
        var renewals = await _renewalRepository.FindAsync(r => r.ContractId == contractId);
        return _mapper.Map<IEnumerable<ContractRenewalReadDto>>(renewals.OrderByDescending(r => r.RenewalDate));
    }

    public async Task<IEnumerable<ContractReadDto>> GetActiveContractsAsync()
    {
        var activeContracts = await _contractRepository.FindAsync(c => c.Status == ContractStatus.Active);
        return _mapper.Map<IEnumerable<ContractReadDto>>(activeContracts);
    }

    public async Task<IEnumerable<ContractReadDto>> SearchContractsAsync(string searchTerm)
    {
        var contracts = await _contractRepository.FindAsync(c => 
            c.ContractNumber.Contains(searchTerm) ||
            c.Title.Contains(searchTerm) ||
            (c.Description != null && c.Description.Contains(searchTerm)));
        
        return _mapper.Map<IEnumerable<ContractReadDto>>(contracts);
    }
}