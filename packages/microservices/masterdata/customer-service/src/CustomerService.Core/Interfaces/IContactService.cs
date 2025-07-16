using CustomerService.Core.DTOs;
using CustomerService.Core.Entities;

namespace CustomerService.Core.Interfaces;

public interface IContactService
{
    Task<ContactReadDto> CreateContactAsync(CreateContactDto createContactDto);
    Task<ContactReadDto?> GetContactAsync(string id);
    Task<IEnumerable<ContactReadDto>> GetCustomerContactsAsync(string customerId);
    Task<ContactReadDto?> UpdateContactAsync(string id, UpdateContactDto updateContactDto);
    Task<bool> DeleteContactAsync(string id);
    Task<ContactReadDto?> GetPrimaryContactAsync(string customerId);
    Task<IEnumerable<ContactReadDto>> GetContactsByTypeAsync(string customerId, ContactType contactType);
    Task<IEnumerable<ContactReadDto>> GetContactsByRoleAsync(string customerId, ContactRole role);
    
    // Communication logging methods
    Task<ContactCommunicationReadDto> LogCommunicationAsync(LogCommunicationDto logDto);
    Task<IEnumerable<ContactCommunicationReadDto>> GetContactCommunicationsAsync(string contactId);
    Task<IEnumerable<ContactCommunicationReadDto>> GetCustomerCommunicationsAsync(string customerId);
    Task<bool> MarkCommunicationResolvedAsync(string communicationId, string? resolution = null);
    Task<IEnumerable<ContactCommunicationReadDto>> GetPendingCommunicationsAsync(string? customerId = null);
    Task<bool> UpdateContactPreferencesAsync(string contactId, UpdateContactPreferencesDto preferencesDto);
    Task<IEnumerable<ContactReadDto>> GetContactsForNotificationAsync(string customerId, string notificationType);
}