using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserModule.Data;
using UserModule.Models;
using System.IdentityModel.Tokens.Jwt;
using UserModule.Services;
using AutoMapper;
using System.Security.Cryptography;
using System.Text;
using UserModule.Authorization;
using UserModule.Dtos.Users;
using System.Threading.Tasks;
using System.Linq;

namespace UserModule.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Sanctum")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public UserController(AppDbContext dbContext, ITokenService tokenService, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // GET: api/users
        [HttpGet]
        [RequirePermission("users.read")]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            var users = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var userDtos = users.Select(u => new UserReadDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Department = u.Department,
                IsActive = u.IsActive,
                LoginStatus = u.LoginStatus,
                Role = u.UserRoles?.FirstOrDefault()?.Role?.Name
            }).ToList();

            return Ok(userDtos);
        }

        // GET: api/users/{id}
        [HttpGet("{id:guid}")]
        [RequirePermission("users.read")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userIdFromToken))
            {
                return Unauthorized(new { Error = "Invalid token" });
            }

            var isAdmin = await _tokenService.HasRoleAsync(
                HttpContext.Request.Headers["Authorization"].ToString().Replace("Sanctum ", ""), "Admin");
            if (!isAdmin && userIdFromToken != id)
            {
                return StatusCode(403, new { Error = "You can only view your own account" });
            }

            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            var userDto = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                IsActive = user.IsActive,
                LoginStatus = user.LoginStatus,
                Role = user.UserRoles?.FirstOrDefault()?.Role?.Name
            };

            return Ok(userDto);
        }

        // POST: api/users
        [HttpPost]
        [RequirePermission("users.create")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest(new { Error = "User data is required" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userIdFromToken))
            {
                return Unauthorized(new { Error = "Invalid token" });
            }

            if (string.IsNullOrWhiteSpace(userDto.Email) ||
                await _dbContext.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                return BadRequest(new { Error = "Email already exists or is invalid" });
            }

            if (string.IsNullOrWhiteSpace(userDto.Username) ||
                await _dbContext.Users.AnyAsync(u => u.Username == userDto.Username))
            {
                return BadRequest(new { Error = "Username already exists or is invalid" });
            }

            if (string.IsNullOrWhiteSpace(userDto.Password))
            {
                return BadRequest(new { Error = "Password is required" });
            }

            string roleName = userDto.Role ?? "User";
            var role = await _dbContext.Roles
                .SingleOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
            {
                return BadRequest(new { Error = $"Role '{roleName}' does not exist" });
            }

            var user = _mapper.Map<User>(userDto);
            user.Id = Guid.NewGuid();
            user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            user.IsActive = true;
            user.IsFirstLogin = true;
            user.LoginStatus = false;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.CreatedBy = userIdFromToken;

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };

            _dbContext.Users.Add(user);
            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();

            await LogAuditEvent(userIdFromToken, "CreateUser", $"User {user.Email} created", user.Email);

            var responseDto = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                IsActive = user.IsActive,
                LoginStatus = user.LoginStatus,
                Role = role.Name
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, responseDto);
        }

        // POST: api/users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) ||
                string.IsNullOrWhiteSpace(loginDto.Password))
            {
                await LogAuditEvent(null, "Login", "Failed login attempt: Email or password missing", loginDto?.Email);
                return BadRequest(new { Error = "Email and password are required" });
            }

            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.UserShifts)
                .ThenInclude(us => us.Shift)
                .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                await LogAuditEvent(null, "Login", "Failed login attempt: Invalid email or password", loginDto.Email);
                return Unauthorized(new { Error = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                await LogAuditEvent(user.Id, "Login", "Failed login attempt: Account is inactive", user.Email);
                return StatusCode(403, new { Error = "Account is inactive. Contact an administrator." });
            }

            if (user.IsFirstLogin)
            {
                await LogAuditEvent(user.Id, "Login", "First login requires password change", user.Email);
                return StatusCode(403, new
                {
                    Error = "First login requires password change. Use the endpoint PUT api/users/{id}/first-login-password.",
                    UserId = user.Id
                });
            }

            var shiftService = HttpContext.RequestServices.GetRequiredService<IShiftService>();
            var (canLogin, message) = await shiftService.CanUserLoginAsync(user.Id);

            if (!canLogin)
            {
                await LogAuditEvent(user.Id, "Login", $"Failed login attempt: {message}", user.Email);
                return StatusCode(403, new { Error = message });
            }

            user.LoginStatus = true;
            user.LastLoginAt = DateTime.UtcNow;
            _dbContext.Entry(user).State = EntityState.Modified;

            var token = _tokenService.GenerateToken(user);
            var tokenHash = ComputeSha256Hash(token);
            var hasAdminRole = await _tokenService.HasRoleAsync(token, "Admin");

            var personalAccessToken = new PersonalAccessToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = user.Username,
                Token = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsRevoked = false
            };

            _dbContext.PersonalAccessTokens.Add(personalAccessToken);
            await LogAuditEvent(user.Id, "Login", "Successful login", user.Email);
            await _dbContext.SaveChangesAsync();

            var userDto = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                IsActive = user.IsActive,
                LoginStatus = user.LoginStatus,
                Role = user.UserRoles?.FirstOrDefault()?.Role?.Name
            };

            return Ok(new
            {
                Token = token,
                HasAdminRole = hasAdminRole,
                IsFirstLogin = user.IsFirstLogin,
                User = userDto
            });
        }

        // POST: api/users/logout
        [HttpPost("logout")]
        [Authorize(AuthenticationSchemes = "Sanctum")]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { Error = "Invalid token" });
            }

            var user = await _dbContext.Users
                .Include(u => u.PersonalAccessTokens)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            if (!user.IsActive)
            {
                await LogAuditEvent(user.Id, "Logout", "Attempted logout on inactive account", user.Email);
                return StatusCode(403, new { Error = "Account is inactive. Contact an administrator." });
            }

            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { Error = "Invalid or missing Bearer token" });
            }

            var tokenString = authHeader.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(tokenString))
            {
                return Unauthorized(new { Error = "Authorization token missing" });
            }

            var tokenHash = ComputeSha256Hash(tokenString);

            var token = user.PersonalAccessTokens.FirstOrDefault(t => t.Token == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
            if (token == null)
            {
                await LogAuditEvent(user.Id, "Logout", "Invalid or expired token", user.Email);
                return Unauthorized(new { Error = "Invalid or expired token" });
            }

            token.IsRevoked = true;
            user.LoginStatus = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userId;

            _dbContext.Entry(token).State = EntityState.Modified;
            _dbContext.Entry(user).State = EntityState.Modified;
            await LogAuditEvent(user.Id, "Logout", "Successful logout", user.Email);
            await _dbContext.SaveChangesAsync();

            return Ok(new { Message = "Logged out successfully" });
        }

        // PUT: api/users/{id}
        [HttpPut("{id:guid}")]
        [RequirePermission("users.update")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userDto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID" });
            }
            if (userDto == null)
            {
                return BadRequest(new { Error = "User data is required" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userIdFromToken))
            {
                return Unauthorized(new { Error = "Invalid token" });
            }

            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Include(u => u.PersonalAccessTokens)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { Error = "Invalid or missing Bearer token" });
            }

            var tokenString = authHeader.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(tokenString))
            {
                return Unauthorized(new { Error = "Authorization token missing" });
            }

            var isAdmin = await _tokenService.HasRoleAsync(tokenString, "Admin");

            var updateResult = await UpdateUserFields(user, userDto, isAdmin, userIdFromToken, id);
            if (updateResult != null)
            {
                return updateResult;
            }

            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userIdFromToken;

            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            var responseDto = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                IsActive = user.IsActive,
                LoginStatus = user.LoginStatus,
                Role = user.UserRoles?.FirstOrDefault()?.Role?.Name
            };

            await LogAuditEvent(userIdFromToken, "UpdateUser", $"User {user.Email} updated", user.Email);

            return Ok(responseDto);
        }

        // PUT: api/users/{id}/password
        [HttpPut("{id:guid}/password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] PasswordUpdateDto passwordDto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID" });
            }
            if (passwordDto == null || string.IsNullOrWhiteSpace(passwordDto.CurrentPassword) || string.IsNullOrWhiteSpace(passwordDto.NewPassword) || string.IsNullOrWhiteSpace(passwordDto.ConfirmNewPassword))
            {
                return BadRequest(new { Error = "Current password, new password, and confirmation are required" });
            }
            if (passwordDto.NewPassword != passwordDto.ConfirmNewPassword)
            {
                return BadRequest(new { Error = "New password and confirmation must match" });
            }

            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userIdFromToken))
            {
                return Unauthorized(new { Error = "Invalid token" });
            }

            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Error = "User not found" });
            }

            if (!user.IsActive)
            {
                await LogAuditEvent(user.Id, "ChangePassword", "Attempted password change on inactive account", user.Email);
                return StatusCode(403, new { Error = "Account is inactive. Contact an administrator." });
            }

            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { Error = "Invalid or missing Bearer token" });
            }

            var tokenString = authHeader.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(tokenString))
            {
                return Unauthorized(new { Error = "Authorization token missing" });
            }

            var isAdmin = await _tokenService.HasRoleAsync(tokenString, "Admin");

            if (!isAdmin && userIdFromToken != id)
            {
                return StatusCode(403, new { Error = "You can only change your own password" });
            }

            if (!isAdmin && !BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, user.Password))
            {
                return BadRequest(new { Error = "Current password is incorrect" });
            }

            if (passwordDto.NewPassword.Length < 8 ||
                !passwordDto.NewPassword.Any(char.IsUpper) ||
                !passwordDto.NewPassword.Any(char.IsLower) ||
                !passwordDto.NewPassword.Any(char.IsDigit) || passwordDto.NewPassword.All(char.IsLetterOrDigit))
            {
                return BadRequest(new { Error = "New password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            user.IsFirstLogin = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = userIdFromToken;

            _dbContext.Entry(user).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            await LogAuditEvent(userIdFromToken, "ChangePassword", $"Password updated for user {user.Email}", user.Email);

            return Ok(new { Message = "Password updated successfully" });
        }

        // PUT: api/users/{id}/first-login-password
        [HttpPut("{id:guid}/first-login-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangeFirstLoginPassword(Guid id, [FromBody] FirstLoginPasswordChangeDto passwordDto)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid user ID" });
            }

            if (passwordDto == null || string.IsNullOrWhiteSpace(passwordDto.NewPassword) ||
                string.IsNullOrWhiteSpace(passwordDto.ConfirmNewPassword))
            {
                return BadRequest(new { Error = "New password and confirmation password are required" });
            }

            var password = passwordDto.NewPassword;

            if (password.Length < 8)
            {
                return BadRequest(new { Error = "New password must be at least 8 characters long" });
            }
            if (!password.Any(char.IsUpper))
            {
                return BadRequest(new { Error = "New password must contain at least one uppercase letter" });
            }
            if (!password.Any(char.IsLower))
            {
                return BadRequest(new { Error = "New password must contain at least one lowercase letter" });
            }
            if (!password.Any(char.IsDigit))
            {
                return BadRequest(new { Error = "New password must contain at least one digit" });
            }
            if (password.All(char.IsLetterOrDigit))
            {
                return BadRequest(new { Error = "New password must contain at least one special character" });
            }

            var user = await _dbContext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(new { Error = "User not found or email does not match the provided ID" });
            }

            if (!user.IsFirstLogin)
            {
                return BadRequest(new { Error = "This endpoint is only for first login password changes" });
            }

            if (!user.IsActive)
            {
                await LogAuditEvent(user.Id, "ChangeFirstLoginPassword", "Attempted first login password change on inactive account", user.Email);
                return StatusCode(403, new { Error = "Account is inactive. Contact an administrator." });
            }

            if (passwordDto.NewPassword != passwordDto.ConfirmNewPassword)
            {
                return BadRequest(new { Error = "New password and confirmation password do not match" });
            }

            var shiftService = HttpContext.RequestServices.GetRequiredService<IShiftService>();
            var (canLogin, message) = await shiftService.CanUserLoginAsync(user.Id);

            if (!canLogin)
            {
                await LogAuditEvent(user.Id, "ChangeFirstLoginPassword", $"Failed first login password change: {message}", user.Email);
                return StatusCode(403, new { Error = message });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);
            user.IsFirstLogin = false;
            user.LoginStatus = true;
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = id;

            _dbContext.Entry(user).State = EntityState.Modified;

            var token = _tokenService.GenerateToken(user);
            var tokenHash = ComputeSha256Hash(token);
            var hasAdminRole = await _tokenService.HasRoleAsync(token, "Admin");

            var personalAccessToken = new PersonalAccessToken
            {
                Id = Guid.NewGuid(),
                Name = user.Username,
                UserId = user.Id,
                Token = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsRevoked = false
            };

            _dbContext.PersonalAccessTokens.Add(personalAccessToken);
            await LogAuditEvent(user.Id, "ChangeFirstLoginPassword", "Successful first login password change", user.Email);
            await _dbContext.SaveChangesAsync();

            var userDto = new UserReadDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                IsActive = user.IsActive,
                LoginStatus = user.LoginStatus,
                Role = user.UserRoles?.FirstOrDefault()?.Role?.Name
            };

            return Ok(new
            {
                Message = "First login password changed successfully",
                Token = token,
                HasAdminRole = hasAdminRole,
                IsFirstLogin = user.IsFirstLogin,
                User = userDto
            });
        }

        private async Task<IActionResult?> UpdateUserFields(User user, UserUpdateDto userDto, bool isAdmin, Guid userIdFromToken, Guid id)
        {
            // Non-admins can only update their own profile
            if (!isAdmin && userIdFromToken != id)
            {
                return StatusCode(403, new { Error = "You can only update your own profile" });
            }

            // Validate email format and uniqueness
            if (!string.IsNullOrWhiteSpace(userDto.email) && userDto.email != user.Email)
            {
                // Validate email format
                var emailValidator = new System.ComponentModel.DataAnnotations.EmailAddressAttribute();
                if (!emailValidator.IsValid(userDto.email))
                {
                    return BadRequest(new { Error = "Invalid email format" });
                }

                // Check for email uniqueness
                if (await _dbContext.Users.AnyAsync(u => u.Email == userDto.email && u.Id != id))
                {
                    return BadRequest(new { Error = "Email already exists" });
                }
                user.Email = userDto.email;
            }

            // Update other fields
            user.FirstName = userDto.FirstName ?? user.FirstName;
            user.LastName = userDto.LastName ?? user.LastName;
            user.Department = userDto.Department ?? user.Department;

            return null; // Return null if no errors, indicating success
        }

        private async Task LogAuditEvent(Guid? userId, string action, string description, string email)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Action = action,
              //  Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.AuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}