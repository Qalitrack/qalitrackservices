using AutoMapper;
using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;
using CustomerService.Core.Interfaces;

namespace CustomerService.Core.Services;

public class ContactService : IContactService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IRepository<ContactCommunication> _communicationRepository;
    private readonly IMapper _mapper;

    public ContactService(
        IRepository<Contact> contactRepository,
        ICustomerRepository customerRepository,
        IRepository<ContactCommunication> communicationRepository,
        IMapper mapper)
    {
        _contactRepository = contactRepository;
        _customerRepository = customerRepository;
        _communicationRepository = communicationRepository;
        _mapper = mapper;
    }

    public async Task<ContactReadDto> CreateContactAsync(CreateContactDto createContactDto)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetByIdAsync(createContactDto.CustomerId);
        if (customer == null)
        {
            throw new ArgumentException($"Customer with ID {createContactDto.CustomerId} not found");
        }

        // If this is set as primary, ensure no other primary contact exists
        if (createContactDto.IsPrimary)
        {
            var existingPrimary = await _contactRepository.FirstOrDefaultAsync(c => 
                c.CustomerId == createContactDto.CustomerId && c.IsPrimary);
            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
                await _contactRepository.UpdateAsync(existingPrimary);
            }
        }

        var contact = _mapper.Map<Contact>(createContactDto);
        contact.Id = Guid.NewGuid().ToString();
        contact.CreatedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        var createdContact = await _contactRepository.CreateAsync(contact);
        return _mapper.Map<ContactReadDto>(createdContact);
    }

    public async Task<ContactReadDto?> GetContactAsync(string id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        return contact != null ? _mapper.Map<ContactReadDto>(contact) : null;
    }

    public async Task<IEnumerable<ContactReadDto>> GetCustomerContactsAsync(string customerId)
    {
        var contacts = await _contactRepository.FindAsync(c => c.CustomerId == customerId && c.IsActive);
        return _mapper.Map<IEnumerable<ContactReadDto>>(contacts.OrderByDescending(c => c.IsPrimary));
    }

    public async Task<ContactReadDto?> UpdateContactAsync(string id, UpdateContactDto updateContactDto)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        if (contact == null)
        {
            return null;
        }

        // If this is being set as primary, ensure no other primary contact exists
        if (updateContactDto.IsPrimary && !contact.IsPrimary)
        {
            var existingPrimary = await _contactRepository.FirstOrDefaultAsync(c => 
                c.CustomerId == contact.CustomerId && c.IsPrimary && c.Id != id);
            if (existingPrimary != null)
            {
                existingPrimary.IsPrimary = false;
                await _contactRepository.UpdateAsync(existingPrimary);
            }
        }

        _mapper.Map(updateContactDto, contact);
        contact.UpdatedAt = DateTime.UtcNow;

        var updatedContact = await _contactRepository.UpdateAsync(contact);
        return updatedContact != null ? _mapper.Map<ContactReadDto>(updatedContact) : null;
    }

    public async Task<bool> DeleteContactAsync(string id)
    {
        var contact = await _contactRepository.GetByIdAsync(id);
        if (contact == null) return false;

        // Soft delete by setting IsActive to false
        contact.IsActive = false;
        contact.UpdatedAt = DateTime.UtcNow;
        await _contactRepository.UpdateAsync(contact);
        return true;
    }

    public async Task<ContactReadDto?> GetPrimaryContactAsync(string customerId)
    {
        var primaryContact = await _contactRepository.FirstOrDefaultAsync(c => 
            c.CustomerId == customerId && c.IsPrimary && c.IsActive);
        return primaryContact != null ? _mapper.Map<ContactReadDto>(primaryContact) : null;
    }

    public async Task<IEnumerable<ContactReadDto>> GetContactsByTypeAsync(string customerId, ContactType contactType)
    {
        var contacts = await _contactRepository.FindAsync(c => 
            c.CustomerId == customerId && c.ContactType == contactType && c.IsActive);
        return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
    }

    public async Task<IEnumerable<ContactReadDto>> GetContactsByRoleAsync(string customerId, ContactRole role)
    {
        var contacts = await _contactRepository.FindAsync(c => 
            c.CustomerId == customerId && c.Role == role && c.IsActive);
        return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
    }

    // Communication logging methods
    public async Task<ContactCommunicationReadDto> LogCommunicationAsync(LogCommunicationDto logDto)
    {
        var contact = await _contactRepository.GetByIdAsync(logDto.ContactId);
        if (contact == null)
        {
            throw new ArgumentException($"Contact with ID {logDto.ContactId} not found");
        }

        var communication = _mapper.Map<ContactCommunication>(logDto);
        communication.Id = Guid.NewGuid().ToString();
        communication.CreatedAt = DateTime.UtcNow;
        communication.UpdatedAt = DateTime.UtcNow;

        var createdCommunication = await _communicationRepository.CreateAsync(communication);

        // Update contact's last communication info
        contact.LastContactDate = communication.CommunicationDate;
        contact.LastContactMethod = communication.Type.ToString();
        contact.LastContactNotes = communication.Subject;
        contact.ContactFrequency += 1;
        contact.UpdatedAt = DateTime.UtcNow;
        await _contactRepository.UpdateAsync(contact);

        return _mapper.Map<ContactCommunicationReadDto>(createdCommunication);
    }

    public async Task<IEnumerable<ContactCommunicationReadDto>> GetContactCommunicationsAsync(string contactId)
    {
        var communications = await _communicationRepository.FindAsync(c => c.ContactId == contactId);
        return _mapper.Map<IEnumerable<ContactCommunicationReadDto>>(communications.OrderByDescending(c => c.CommunicationDate));
    }

    public async Task<IEnumerable<ContactCommunicationReadDto>> GetCustomerCommunicationsAsync(string customerId)
    {
        var customerContacts = await _contactRepository.FindAsync(c => c.CustomerId == customerId);
        var contactIds = customerContacts.Select(c => c.Id).ToList();
        
        var communications = await _communicationRepository.FindAsync(c => contactIds.Contains(c.ContactId));
        return _mapper.Map<IEnumerable<ContactCommunicationReadDto>>(communications.OrderByDescending(c => c.CommunicationDate));
    }

    public async Task<bool> MarkCommunicationResolvedAsync(string communicationId, string? resolution = null)
    {
        var communication = await _communicationRepository.GetByIdAsync(communicationId);
        if (communication == null) return false;

        communication.IsResolved = true;
        communication.Resolution = resolution;
        communication.UpdatedAt = DateTime.UtcNow;
        await _communicationRepository.UpdateAsync(communication);
        return true;
    }

    public async Task<IEnumerable<ContactCommunicationReadDto>> GetPendingCommunicationsAsync(string? customerId = null)
    {
        IEnumerable<ContactCommunication> communications;

        if (!string.IsNullOrEmpty(customerId))
        {
            var customerContacts = await _contactRepository.FindAsync(c => c.CustomerId == customerId);
            var contactIds = customerContacts.Select(c => c.Id).ToList();
            communications = await _communicationRepository.FindAsync(c => 
                contactIds.Contains(c.ContactId) && 
                !c.IsResolved && 
                c.ResponseDue.HasValue && 
                c.ResponseDue.Value <= DateTime.UtcNow);
        }
        else
        {
            communications = await _communicationRepository.FindAsync(c => 
                !c.IsResolved && 
                c.ResponseDue.HasValue && 
                c.ResponseDue.Value <= DateTime.UtcNow);
        }

        return _mapper.Map<IEnumerable<ContactCommunicationReadDto>>(communications.OrderBy(c => c.ResponseDue));
    }

    public async Task<bool> UpdateContactPreferencesAsync(string contactId, UpdateContactPreferencesDto preferencesDto)
    {
        var contact = await _contactRepository.GetByIdAsync(contactId);
        if (contact == null) return false;

        _mapper.Map(preferencesDto, contact);
        contact.UpdatedAt = DateTime.UtcNow;
        await _contactRepository.UpdateAsync(contact);
        return true;
    }

    public async Task<IEnumerable<ContactReadDto>> GetContactsForNotificationAsync(string customerId, string notificationType)
    {
        var contacts = await _contactRepository.FindAsync(c => c.CustomerId == customerId && c.IsActive);
        
        // Filter based on notification preferences
        var filteredContacts = contacts.Where(c => notificationType switch
        {
            "OrderUpdates" => c.NotifyOnOrderUpdates,
            "ContractRenewals" => c.NotifyOnContractRenewals,
            "PaymentDue" => c.NotifyOnPaymentDue,
            "DeliveryUpdates" => c.NotifyOnDeliveryUpdates,
            "ComplianceIssues" => c.NotifyOnComplianceIssues,
            _ => true
        });

        return _mapper.Map<IEnumerable<ContactReadDto>>(filteredContacts);
    }
}