
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Masterdata.Core.Utils
{
    /// <summary>
    /// Utility service for extracting user information from JWT tokens.
    /// This service provides convenient methods to access claims from the authenticated user's token.
    /// </summary>
    public interface ITokenExtractionService
    {
        /// <summary>
        /// Extracts the user ID from the token claims.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <returns>User ID as Guid, or null if not found or invalid</returns>
        Guid? GetUserIdFromToken(ClaimsPrincipal user);

        /// <summary>
        /// Extracts the user email from the token claims.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <returns>User email, or null if not found</returns>
        string? GetUserEmailFromToken(ClaimsPrincipal user);

        /// <summary>
        /// Extracts all roles assigned to the user from the token claims.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <returns>List of role names</returns>
        List<string> GetUserRolesFromToken(ClaimsPrincipal user);

        /// <summary>
        /// Extracts all permissions assigned to the user from the token claims.
        /// Note: Permissions must be included in the token as "Permission" claims.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <returns>List of permission names</returns>
   
        Dictionary<string, string> GetAllClaimsFromToken(ClaimsPrincipal user);

        /// <summary>
        /// Checks if the user has a specific role.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <param name="role">Role name to check</param>
        /// <returns>True if user has the role</returns>
        bool HasRole(ClaimsPrincipal user, string role);

        /// <summary>
        /// Checks if the user has any of the specified roles.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <param name="roles">Role names to check</param>
        /// <returns>True if user has at least one of the roles</returns>
        bool HasAnyRole(ClaimsPrincipal user, params string[] roles);

        /// <summary>
        /// Checks if the user has all of the specified roles.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal from HttpContext.User</param>
        /// <param name="roles">Role names to check</param>
        /// <returns>True if user has all the roles</returns>
        bool HasAllRoles(ClaimsPrincipal user, params string[] roles);

    }
}