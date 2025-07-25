
# Authentication Documentation

The authentication system is built using JWT tokens. The key endpoints for authentication are:

## Key Components:

1. **AuthController**: Exposes endpoints for handling user authentication (login, password update, and logout).
2. **TokenService**: Handles the generation, validation, and revocation of JWT tokens.
3. **PermissionAuthorizationHandler**: Handles permission-based authorization, validating if a user is allowed to access a resource based on their roles and permissions.
4. **PermissionPolicyProvider**: Provides permission policies dynamically without the need for pre-registration.

---

## Authentication Endpoints

### **1. AuthController**

The **AuthController** is responsible for the following operations:

#### Endpoints:

* **POST /api/auth/login**

  * **Description**: Authenticates a user with email and password and returns a JWT token.
  * **Authorization**: Open to all (anonymous).
  * **Request Body**:

    ```json
    {
      "email": "johndoe@example.com",
      "password": "password123"
    }
    ```
  * **Response**: Returns a JWT token.

  #### Example:

  ```csharp
  [HttpPost("login")]
  [AllowAnonymous]
  public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
  {
      try
      {
          var token = await _tokenService.GenerateTokenAsync(loginDto.Email, loginDto.Password);
          return Ok(new { Token = token });
      }
      catch (UnauthorizedAccessException ex)
      {
          _logger.LogError(ex, "Authentication failed for {Email}", loginDto.Email);
          return Unauthorized("Invalid credentials");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error during authentication");
          return StatusCode(500, "An error occurred during authentication");
      }
  }
  ```

* **PUT /api/auth/update-password/{userId}**

  * **Description**: Allows users to update their password.
  * **Authorization**: Requires the user to be authenticated and the request should include the JWT token.
  * **Request Body**:

    ```json
    {
      "newPassword": "newpassword123"
    }
    ```
  * **Response**: Returns a success message if the password is updated.

  #### Example:

  ```csharp
  [HttpPut("update-password/{userId}")]
  [Authorize]
  public async Task<IActionResult> UpdatePassword(string userId, [FromBody] UpdatePasswordDto updatePasswordDto)
  {
      try
      {
          var result = await _userService.UpdatePasswordAsync(userId, updatePasswordDto.NewPassword);
          return result ? Ok("Password updated successfully") : BadRequest("Failed to update password");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error updating password for user {UserId}", userId);
          return StatusCode(500, "An error occurred while updating the password");
      }
  }
  ```

* **POST /api/auth/logout**

  * **Description**: Logs out the user by revoking their JWT token.
  * **Authorization**: Requires the user to be authenticated and the request should include the JWT token.
  * **Response**: Returns a success message when the user is logged out.

  #### Example:

  ```csharp
  [HttpPost("logout")]
  [Authorize]
  public async Task<IActionResult> Logout()
  {
      try
      {
          var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
          var result = await _tokenService.RevokeTokenAsync(token);
          return result ? Ok("Logged out successfully") : BadRequest("Failed to logout");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error during logout");
          return StatusCode(500, "An error occurred while logging out");
      }
  }
  ```

---

## TokenService (Business Logic Layer)

The **TokenService** is responsible for generating, validating, and revoking JWT tokens. It ensures that tokens are created securely and are valid during requests.

### Key Methods in **TokenService**:

* **GenerateTokenAsync**: Generates a JWT token for the user after validating the provided credentials (email and password).
* **ValidateTokenAsync**: Validates the provided JWT token to ensure it is not expired, revoked, or tampered with.
* **RevokeTokenAsync**: Marks the JWT token as revoked to prevent future use.
* **RevokeAndDeleteTokenAsync**: Revoke and delete tokens for a specific user.

### Example of **GenerateTokenAsync** Method:

```csharp
public async Task<PersonalAccessToken> GenerateTokenAsync(string email, string password)
{
    var user = await _userService.ValidateUserCredentials(email, password);
    if (user == null)
        throw new UnauthorizedAccessException("Invalid email or password.");

    var jti = Guid.NewGuid().ToString(); // Generate unique token ID

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, jti),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Email)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
        _configuration["Jwt:SecretKey"] ??
        _configuration["JwtSettings:SecretKey"] ??
        throw new InvalidOperationException("JWT Secret Key is not configured")));

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"] ?? "UserService",
        audience: _configuration["Jwt:Audience"] ?? "UserService",
        claims: claims,
        expires: DateTime.UtcNow.AddDays(7),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    var personalAccessToken = new PersonalAccessToken
    {
        Token = tokenString,
        UserId = user.Id.ToString(),
        Jti = jti,
        IsRevoked = false,
    };

    await _tokenRepository.CreateAsync(personalAccessToken);
    return personalAccessToken;
}
```

---

## PermissionAuthorizationHandler

The **PermissionAuthorizationHandler** is responsible for handling permission-based authorization. It validates if a user is authorized to access a resource based on their assigned permissions.

### Key Methods in **PermissionAuthorizationHandler**:

* **HandleRequirementAsync**: This method is called to check if a user has the required permission to access a resource.
* **LogRequestHeaders**: Logs all headers from the request for debugging purposes.
* **GetTokenFromRequest**: Extracts the JWT token from the request headers.
* **CheckUserPermission**: Validates if the user has the required permissions.

---

## PermissionPolicyProvider

The **PermissionPolicyProvider** dynamically creates authorization policies based on permissions. There is no need to pre-register policies for each permission, which allows flexibility in handling different permissions across the API.

### Key Methods in **PermissionPolicyProvider**:

* **GetPolicyAsync**: Creates an authorization policy for a given permission.
* **GetDefaultPolicyAsync**: Returns the default policy.
* **GetFallbackPolicyAsync**: Returns a fallback policy if no policy is found.

---

## PermissionRequirement

The **PermissionRequirement** class represents a specific permission requirement that is used in **PermissionAuthorizationHandler**.

---

## Error Handling & Logging

* All controllers, services, and repositories include robust error handling with **try-catch** blocks.
* **Serilog** is used for logging, providing detailed logs for debugging and tracing issues.
* Standardized HTTP status codes are used for consistent error responses (e.g., `400` for bad requests, `404` for not found, `500` for server errors).

---

## Summary

* **Authentication** includes endpoints for **logging in**, **updating passwords**, and **logging out**.
* **TokenService** handles the core business logic for managing JWT tokens, including generation, validation, and revocation.
* **PermissionAuthorizationHandler** ensures that users are authorized to perform actions based on their assigned permissions.
* The system ensures **authorization** using **Role-Based Access Control (RBAC)**, **permission-based policies**, and **JWT tokens** for secure access control.
