using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Core.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<CustomerContact> _contactRepository;
    private readonly IRepository<CustomerContract> _contractRepository;
    private readonly IRepository<CustomerBilling> _billingRepository;
    private readonly IRepository<CustomerCredit> _creditRepository;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository customerRepository,
        IRepository<CustomerContact> contactRepository,
        IRepository<CustomerContract> contractRepository,
        IRepository<CustomerBilling> billingRepository,
        IRepository<CustomerCredit> creditRepository,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _contactRepository = contactRepository;
        _contractRepository = contractRepository;
        _billingRepository = billingRepository;
        _creditRepository = creditRepository;
        _mapper = mapper;
    }

    public async Task<CustomerDto> RegisterCustomerAsync(RegisterCustomerRequest request)
    {
        var existingCustomer = await _customerRepository.GetByEmailAsync(request.ContactEmail);
        if (existingCustomer != null)
        {
            throw new InvalidOperationException($"Customer with email {request.ContactEmail} already exists");
        }

        if (!string.IsNullOrEmpty(request.TaxNumber))
        {
            var existingByTax = await _customerRepository.GetByTaxNumberAsync(request.TaxNumber);
            if (existingByTax != null)
            {
                throw new InvalidOperationException($"Customer with tax number {request.TaxNumber} already exists");
            }
        }

        if (!string.IsNullOrEmpty(request.RegistrationNumber))
        {
            var existingByReg = await _customerRepository.GetByRegistrationNumberAsync(request.RegistrationNumber);
            if (existingByReg != null)
            {
                throw new InvalidOperationException($"Customer with registration number {request.RegistrationNumber} already exists");
            }
        }

        var customer = _mapper.Map<Customer>(request);
        var savedCustomer = await _customerRepository.AddAsync(customer);

        var customerCredit = new CustomerCredit
        {
            CustomerId = savedCustomer.Id,
            CreditLimit = request.CreditLimit,
            AvailableCredit = request.CreditLimit,
            UsedCredit = 0,
            CreditStatus = CreditStatus.Good,
            CreditScore = 700,
            RequiresApproval = request.CreditLimit > 10000,
            PaymentHistory = new PaymentHistory()
        };

        await _creditRepository.AddAsync(customerCredit);

        return _mapper.Map<CustomerDto>(savedCustomer);
    }

    public async Task<CustomerDto?> GetCustomerAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        var customer = await _customerRepository.GetByEmailAsync(email);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto?> GetCustomerByTaxNumberAsync(string taxNumber)
    {
        var customer = await _customerRepository.GetByTaxNumberAsync(taxNumber);
        return customer == null ? null : _mapper.Map<CustomerDto>(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersPagedAsync(int pageNumber, int pageSize)
    {
        var customers = await _customerRepository.GetPagedAsync(pageNumber, pageSize);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
    {
        var customers = await _customerRepository.SearchAsync(searchTerm);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(string id, UpdateCustomerRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {id} not found");
        }

        if (request.ContactEmail != customer.ContactEmail)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.ContactEmail);
            if (existingCustomer != null && existingCustomer.Id != id)
            {
                throw new InvalidOperationException($"Customer with email {request.ContactEmail} already exists");
            }
        }

        _mapper.Map(request, customer);
        var updatedCustomer = await _customerRepository.UpdateAsync(customer);
        return _mapper.Map<CustomerDto>(updatedCustomer);
    }

    public async Task<bool> DeleteCustomerAsync(string id)
    {
        return await _customerRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> ActivateCustomerAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return false;

        customer.Status = CustomerStatus.Active;
        await _customerRepository.UpdateAsync(customer);
        return true;
    }

    public async Task<bool> DeactivateCustomerAsync(string id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null) return false;

        customer.Status = CustomerStatus.Inactive;
        await _customerRepository.UpdateAsync(customer);
        return true;
    }

    public async Task<CustomerDetailDto?> GetCustomerDetailsAsync(string id)
    {
        var customer = await _customerRepository.GetWithAllDetailsAsync(id);
        return customer == null ? null : _mapper.Map<CustomerDetailDto>(customer);
    }

    public async Task<CustomerContactDto> AddContactAsync(string customerId, CreateCustomerContactRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerId} not found");
        }

        var contact = _mapper.Map<CustomerContact>(request);
        contact.CustomerId = customerId;

        var savedContact = await _contactRepository.AddAsync(contact);
        return _mapper.Map<CustomerContactDto>(savedContact);
    }

    public async Task<IEnumerable<CustomerContactDto>> GetCustomerContactsAsync(string customerId)
    {
        var contacts = await _contactRepository.FindAsync(c => c.CustomerId == customerId);
        return _mapper.Map<IEnumerable<CustomerContactDto>>(contacts);
    }

    public async Task<CustomerContactDto> UpdateContactAsync(string contactId, UpdateCustomerContactRequest request)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null)
        {
            throw new ArgumentException($"Contact with ID {contactId} not found");
        }

        _mapper.Map(request, contact);
        var updatedContact = await _contactRepository.UpdateAsync(contact);
        return _mapper.Map<CustomerContactDto>(updatedContact);
    }

    public async Task<bool> DeleteContactAsync(string contactId)
    {
        return await _contactRepository.DeleteByIdAsync(contactId);
    }

    public async Task<CustomerContractDto> CreateContractAsync(string customerId, CreateCustomerContractRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerId} not found");
        }

        var existingContract = await _contractRepository.FirstOrDefaultAsync(c => c.ContractNumber == request.ContractNumber);
        if (existingContract != null)
        {
            throw new InvalidOperationException($"Contract with number {request.ContractNumber} already exists");
        }

        var contract = _mapper.Map<CustomerContract>(request);
        contract.CustomerId = customerId;

        var savedContract = await _contractRepository.AddAsync(contract);
        return _mapper.Map<CustomerContractDto>(savedContract);
    }

    public async Task<IEnumerable<CustomerContractDto>> GetCustomerContractsAsync(string customerId)
    {
        var contracts = await _contractRepository.FindAsync(c => c.CustomerId == customerId);
        return _mapper.Map<IEnumerable<CustomerContractDto>>(contracts);
    }

    public async Task<CustomerContractDto> UpdateContractAsync(string contractId, UpdateCustomerContractRequest request)
    {
        var contract = await _contractRepository.GetByIdAsync(contractId);
        if (contract == null)
        {
            throw new ArgumentException($"Contract with ID {contractId} not found");
        }

        _mapper.Map(request, contract);
        var updatedContract = await _contractRepository.UpdateAsync(contract);
        return _mapper.Map<CustomerContractDto>(updatedContract);
    }

    public async Task<bool> DeleteContractAsync(string contractId)
    {
        return await _contractRepository.DeleteByIdAsync(contractId);
    }

    public async Task<CustomerBillingDto> UpdateBillingAsync(string customerId, UpdateCustomerBillingRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerId} not found");
        }

        var billing = await _billingRepository.FirstOrDefaultAsync(b => b.CustomerId == customerId);
        if (billing == null)
        {
            billing = new CustomerBilling { CustomerId = customerId };
            _mapper.Map(request, billing);
            billing = await _billingRepository.AddAsync(billing);
        }
        else
        {
            _mapper.Map(request, billing);
            billing = await _billingRepository.UpdateAsync(billing);
        }

        return _mapper.Map<CustomerBillingDto>(billing);
    }

    public async Task<CustomerBillingDto?> GetCustomerBillingAsync(string customerId)
    {
        var billing = await _billingRepository.FirstOrDefaultAsync(b => b.CustomerId == customerId);
        return billing == null ? null : _mapper.Map<CustomerBillingDto>(billing);
    }

    public async Task<CustomerCreditDto> UpdateCreditAsync(string customerId, UpdateCustomerCreditRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {customerId} not found");
        }

        var credit = await _creditRepository.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        if (credit == null)
        {
            throw new ArgumentException($"Credit record for customer {customerId} not found");
        }

        var oldLimit = credit.CreditLimit;
        _mapper.Map(request, credit);
        
        credit.AvailableCredit = credit.CreditLimit - credit.UsedCredit;
        credit.LastCreditCheck = DateTime.UtcNow;

        var updatedCredit = await _creditRepository.UpdateAsync(credit);
        return _mapper.Map<CustomerCreditDto>(updatedCredit);
    }

    public async Task<CustomerCreditDto?> GetCustomerCreditAsync(string customerId)
    {
        var credit = await _creditRepository.FirstOrDefaultAsync(c => c.CustomerId == customerId);
        return credit == null ? null : _mapper.Map<CustomerCreditDto>(credit);
    }
}