using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IOrganizationUserService
{
    Task<OrganizationUserDto?> GetOrganizationUserAsync(string userId, string organizationId);
    Task<IEnumerable<OrganizationUserDto>> GetUserOrganizationsAsync(string userId);
    Task<PaginatedResponseDto<OrganizationUserDto>> GetOrganizationUsersAsync(string organizationId, PaginationRequestDto request);
    Task<OrganizationUserDto> AddUserToOrganizationAsync(CreateOrganizationUserDto request);
    Task<OrganizationUserDto> UpdateOrganizationUserAsync(string userId, string organizationId, UpdateOrganizationUserDto request);
    Task<bool> RemoveUserFromOrganizationAsync(string userId, string organizationId);
    Task<bool> IsUserInOrganizationAsync(string userId, string organizationId);
    Task<bool> IsUserOwnerAsync(string userId, string organizationId);
    Task<bool> IsUserAdminAsync(string userId, string organizationId);
    Task<UserInvitationDto> InviteUserAsync(CreateUserInvitationDto request);
    Task<bool> AcceptInvitationAsync(AcceptInvitationDto request);
    Task<bool> CancelInvitationAsync(string invitationId);
    Task<IEnumerable<UserInvitationDto>> GetOrganizationInvitationsAsync(string organizationId);
    Task<IEnumerable<UserInvitationDto>> GetPendingInvitationsAsync(string organizationId);
    Task<bool> ResendInvitationAsync(string invitationId);
}