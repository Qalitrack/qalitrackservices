# Role and Permission Management Documentation

Role and Permission Management allows administrators to manage user roles and permissions, granting access to resources based on the user's assigned roles. This section provides endpoints for role creation, updating, deletion, and permission assignments.

## Key Components:

1. **RolesController**: Exposes endpoints for managing roles (create, update, delete, and retrieve roles).
2. **UserRoleController**: Manages user-role assignments (assigning/removing roles to/from users).
3. **PermissionsController**: Exposes endpoints for managing permissions (create, update, delete, and retrieve permissions).
4. **RoleService**: Handles the business logic related to roles.
5. **PermissionsService**: Handles the business logic related to permissions.
6. **UserRoleService**: Manages the user-role relationships and validations.

---

## Role Management Endpoints

### **1. RolesController**

The **RolesController** is responsible for the following operations:

#### Endpoints:

* **GET /api/roles**

  * **Description**: Retrieves all roles.
  * **Authorization**: Requires **`roles.view`** permission.
  * **Response**: Returns a list of all roles.

  #### Example:

  ```csharp
  [HttpGet]
  [Authorize(Policy = "roles.view")]
  public async Task<ActionResult<IEnumerable<RoleDto>>> GetAll()
  {
      try
      {
          var roles = await _roleService.GetAllAsync();
          return Ok(roles);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error getting all roles");
          return StatusCode(500, "An error occurred while retrieving roles");
      }
  }
  ```

* **POST /api/roles**

  * **Description**: Creates a new role.
  * **Authorization**: Requires **`roles.manage`** permission.
  * **Request Body**:

    ```json
    {
      "name": "Admin",
      "description": "Administrator role with full access."
    }
    ```
  * **Response**: Returns the created role details.

  #### Example:

  ```csharp
  [HttpPost]
  [Authorize(Policy = "roles.manage")]
  public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleDto createRoleDto)
  {
      try
      {
          var role = await _roleService.CreateAsync(createRoleDto);
          return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error creating role");
          return StatusCode(500, "An error occurred while creating the role");
      }
  }
  ```

* **PUT /api/roles/{id}**

  * **Description**: Updates an existing role.
  * **Authorization**: Requires **`roles.manage`** permission.
  * **Request Body**:

    ```json
    {
      "name": "SuperAdmin",
      "description": "Updated admin role with additional permissions."
    }
    ```
  * **Response**: Returns the updated role details.

  #### Example:

  ```csharp
  [HttpPut("{id}")]
  [Authorize(Policy = "roles.manage")]
  public async Task<IActionResult> Update(string id, [FromBody] UpdateRoleDto updateRoleDto)
  {
      try
      {
          var updatedRole = await _roleService.UpdateAsync(id, updateRoleDto);
          return Ok(updatedRole);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error updating role");
          return StatusCode(500, "An error occurred while updating the role");
      }
  }
  ```

* **DELETE /api/roles/{id}**

  * **Description**: Deletes a role by ID.
  * **Authorization**: Requires **`roles.manage`** permission.
  * **Response**: Success message or error.

  #### Example:

  ```csharp
  [HttpDelete("{id}")]
  [Authorize(Policy = "roles.manage")]
  public async Task<IActionResult> Delete(string id)
  {
      try
      {
          var result = await _roleService.DeleteAsync(id);
          return result ? Ok("Role deleted successfully") : NotFound("Role not found");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error deleting role");
          return StatusCode(500, "An error occurred while deleting the role");
      }
  }
  ```

---

## Permission Management Endpoints

### **2. PermissionsController**

The **PermissionsController** is responsible for the following operations:

#### Endpoints:

* **GET /api/permissions**

  * **Description**: Retrieves all permissions.
  * **Authorization**: Requires **`permissions.view`** permission.
  * **Response**: Returns a list of all permissions.

  #### Example:

  ```csharp
  [HttpGet]
  [Authorize(Policy = "permissions.view")]
  public async Task<IActionResult> GetAll()
  {
      try
      {
          var permissions = await _permissionService.GetAllAsync();
          return Ok(permissions);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error getting all permissions");
          return StatusCode(500, "An error occurred while retrieving permissions");
      }
  }
  ```

* **POST /api/permissions**

  * **Description**: Creates a new permission.
  * **Authorization**: Requires **`permissions.manage`** permission.
  * **Request Body**:

    ```json
    {
      "name": "view_reports",
      "description": "Permission to view reports."
    }
    ```
  * **Response**: Returns the created permission details.

  #### Example:

  ```csharp
  [HttpPost]
  [Authorize(Policy = "permissions.manage")]
  public async Task<IActionResult> Create([FromBody] CreatePermissionDto createPermissionDto)
  {
      try
      {
          var permission = await _permissionService.CreateAsync(createPermissionDto);
          return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error creating permission");
          return StatusCode(500, "An error occurred while creating the permission");
      }
  }
  ```

* **PUT /api/permissions/{id}**

  * **Description**: Updates an existing permission.
  * **Authorization**: Requires **`permissions.manage`** permission.
  * **Request Body**:

    ```json
    {
      "name": "edit_reports",
      "description": "Permission to edit reports."
    }
    ```
  * **Response**: Returns the updated permission details.

  #### Example:

  ```csharp
  [HttpPut("{id}")]
  [Authorize(Policy = "permissions.manage")]
  public async Task<IActionResult> Update(string id, [FromBody] UpdatePermissionDto updatePermissionDto)
  {
      try
      {
          var permission = await _permissionService.UpdateAsync(id, updatePermissionDto);
          return Ok(permission);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error updating permission");
          return StatusCode(500, "An error occurred while updating the permission");
      }
  }
  ```

* **DELETE /api/permissions/{id}**

  * **Description**: Deletes a permission by ID.
  * **Authorization**: Requires **`permissions.manage`** permission.
  * **Response**: Success message or error.

  #### Example:

  ```csharp
  [HttpDelete("{id}")]
  [Authorize(Policy = "permissions.manage")]
  public async Task<IActionResult> Delete(string id)
  {
      try
      {
          var result = await _permissionService.DeleteAsync(id);
          return result ? Ok("Permission deleted successfully") : NotFound("Permission not found");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error deleting permission");
          return StatusCode(500, "An error occurred while deleting the permission");
      }
  }
  ```

---

## Role and Permission Management Logic

### RoleService Methods:

* **GetAllAsync**: Retrieves all roles and the users assigned to each role.
* **CreateAsync**: Creates a new role after validating input data.
* **UpdateAsync**: Updates an existing role's details, ensuring no duplicate role names.
* **DeleteAsync**: Deletes a role, checking if it is assigned to any users.
* **AssignPermissionToRoleAsync**: Assigns a permission to a role.
* **RemovePermissionFromRoleAsync**: Removes a permission from a role.

### PermissionsService Methods:

* **GetAllAsync**: Retrieves all permissions.
* **CreateAsync**: Creates a new permission after validating its uniqueness.
* **UpdateAsync**: Updates an existing permission, ensuring the name does not already exist.
* **DeleteAsync**: Deletes a permission by ID.

---

## Error Handling & Logging

* All controllers, services, and repositories include robust error handling with **try-catch** blocks.
* **Serilog** is used for logging, providing detailed logs for debugging and tracing issues.
* Standardized HTTP status codes are used for consistent error responses (e.g., `400` for bad requests, `404` for not found, `500` for server errors).

---

## Summary

* **Role Management** includes endpoints for **creating**, **updating**, **deleting**, and **retrieving roles**.
* **Permission Management** includes endpoints for **creating**, **updating**, \*\*
