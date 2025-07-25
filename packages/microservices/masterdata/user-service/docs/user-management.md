# User Management Documentation

User management allows administrators to manage user accounts, including creating, updating, retrieving, deleting, and restoring users. This section provides endpoints for user management, along with business logic related to roles, permissions, and validation.

## Key Components:
1. **UsersController**: Exposes endpoints for managing users (create, update, delete, and retrieve users).
2. **UserService**: Handles the business logic related to users.
3. **UserRepository**: Interacts with the database to store and retrieve user data.
4. **UserRoleService**: Manages user roles and assigns them to users.

---

## User Management Endpoints

### **1. UsersController**
The **UsersController** is responsible for the following operations:

#### Endpoints:

- **GET /api/users**
  - **Description**: Retrieves all users.
  - **Authorization**: Requires **`users.view`** permission.
  - **Response**: Returns a list of all users.
  
  #### Example:
  ```csharp
  [HttpGet]
  [Authorize(Policy = "users.view")]
  public async Task<IEnumerable<UserReadDto>> GetAll()
  {
      try
      {
          return (IEnumerable<UserReadDto>)await _userService.GetAllAsync();
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error getting all users");
          throw;
      }
  }



* **GET /api/users/{id}**

  * **Description**: Retrieves a specific user by their ID.
  * **Authorization**: Requires **`users.view`** permission.
  * **Response**: Returns the user details.

  #### Example:

  ```csharp
  [HttpGet("{id}")]
  [Authorize(Policy = "users.view")]
  public async Task<UserReadDto> GetById(string id)
  {
      var user = await _userService.GetByIdAsync(id);
      if (user == null)
      {
          throw new KeyNotFoundException($"User with ID {id} not found");
      }
      return user;
  }
  ```

* **POST /api/users**

  * **Description**: Creates a new user.
  * **Authorization**: Open to all (anonymous).
  * **Request Body**:

    ```json
    {
      "firstName": "John",
      "lastName": "Doe",
      "email": "johndoe@example.com",
      "password": "password123",
      "mobileNumber": "1234567890"
    }
    ```
  * **Response**: Returns the created user details.

  #### Example:

  ```csharp
  [HttpPost]
  [AllowAnonymous]
  public async Task<UserReadDto> Create([FromBody] CreateUserDto createUserDto)
  {
      return await _userService.CreateAsync(createUserDto);
  }
  ```

* **PUT /api/users/{id}**

  * **Description**: Updates an existing user.
  * **Authorization**: Requires **`users.manage`** permission.
  * **Request Body**:

    ```json
    {
      "firstName": "John",
      "lastName": "Smith",
      "email": "johnsmith@example.com",
      "mobileNumber": "9876543210"
    }
    ```
  * **Response**: Returns the updated user details.

  #### Example:

  ```csharp
  [HttpPut("{id}")]
  [Authorize(Policy = "users.manage")]
  public async Task<UserReadDto> Update(string id, [FromBody] UpdateUserDto updateUserDto)
  {
      UserReadDto result = await _userService.UpdateAsync(id, updateUserDto);
      if (result == null)
      {
          throw new KeyNotFoundException($"User with ID {id} not found");
      }
      return result;
  }
  ```

* **DELETE /api/users/{id}**

  * **Description**: Deletes a user by ID.
  * **Authorization**: Requires **`users.manage`** permission.
  * **Response**: Success message or error.

  #### Example:

  ```csharp
  [HttpDelete("{id}")]
  [Authorize(Policy = "users.manage")]
  public async Task Delete(string id)
  {
      var result = await _userService.DeleteAsync(id);
      if (!result)
      {
          throw new KeyNotFoundException($"User with ID {id} not found");
      }
  }
  ```

* **PATCH /api/users/{id}/restore**

  * **Description**: Restores a deleted user.
  * **Authorization**: Requires **`users.manage`** permission.
  * **Response**: Success message or error.

  #### Example:

  ```csharp
  [HttpPatch("{id}/restore")]
  [Authorize(Policy = "users.manage")]
  public async Task Restore(string id)
  {
      var result = await _userService.RestoreAsync(id);
      if (!result)
      {
          throw new KeyNotFoundException($"Deleted user with ID {id} not found");
      }
  }
  ```

---

## User Service (Business Logic Layer)

The **UserService** contains all the business logic for managing users. It handles creating, updating, retrieving, and deleting users, as well as managing user roles.

### Key Methods in **UserService**:

* **GetAllAsync**: Retrieves all users and maps them to `UserReadDto` objects.
* **GetByIdAsync**: Retrieves a specific user by their ID and maps them to `UserReadDto`.
* **CreateAsync**: Creates a new user with the provided data, including hashing the password and setting the `IsFirstLogin` flag.
* **UpdateAsync**: Updates an existing user's details.
* **DeleteAsync**: Deletes a user and marks them as deleted.
* **RestoreAsync**: Restores a deleted user.

### Example of **CreateAsync** Method:

```csharp
public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
{
    var user = new User
    {
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        MobileNumber = dto.MobileNumber,
        Email = dto.Email,
        Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        IsFirstLogin = true,
        IsActive = false
    };

    await _userRepository.CreateAsync(user);
    return _mapper.Map<UserReadDto>(user);
}
```

---

## User Role Service

The **UserRoleService** manages roles for users, assigning and removing roles based on business rules.

### Key Methods in **UserRoleService**:

* **GetUserRolesAsync**: Retrieves all roles for a specific user.
* **IsUserInRoleAsync**: Checks if a user is assigned to a specific role.
* **AddUserToRoleAsync**: Adds a role to a user.
* **RemoveUserFromRoleAsync**: Removes a role from a user.

---

## Error Handling & Logging

* All controllers, services, and repositories include robust error handling with **try-catch** blocks.
* **Serilog** is used for logging, providing detailed logs for debugging and tracing issues.
* Standardized HTTP status codes are used for consistent error responses (e.g., `400` for bad requests, `404` for not found, `500` for server errors).

---

## Summary

* **User Management** includes endpoints for **creating**, **updating**, **deleting**, and **restoring users**.
* **UserService** handles the core business logic for user management, including role assignments and password validation.
* **UserRoleService** manages user roles, ensuring that users are correctly assigned or removed from roles.
* The system ensures authorization using **Role-Based Access Control (RBAC)** to restrict access to certain actions based on permissions.

---