# Shift Management Documentation

Shift management allows administrators to create, assign, and validate shifts. This section provides endpoints for shift creation and assignment, as well as shift validations for active shifts.

## Key Components:

1. **ShiftController**: Exposes endpoints for managing shifts (create, update, delete, and retrieve shifts).
2. **UserShiftController**: Manages assignments of shifts to users.
3. **ShiftService**: Handles the business logic related to shifts.
4. **ShiftRepository**: Interacts with the database to store and retrieve shift data.

---

## Shift Management Endpoints

### **1. ShiftController**

The **ShiftController** is responsible for the following operations:

#### Endpoints:

* **GET /api/shift**

  * **Description**: Retrieves all shifts.
  * **Authorization**: Requires **`shifts.view`** permission.
  * **Response**: Returns a list of all shifts.

  #### Example:

  ```csharp
  [HttpGet]
  [Authorize(Policy = "shifts.view")]
  [ProducesResponseType(typeof(IEnumerable<ShiftDto>), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetAll()
  {
      try
      {
          var shifts = await _shiftService.GetAllAsync();
          return Ok(shifts);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error getting all shifts");
          return StatusCode(500, "An error occurred while retrieving shifts");
      }
  }
  ```

* **GET /api/shift/{id}**

  * **Description**: Retrieves a specific shift by its ID.
  * **Authorization**: Requires **`shifts.view`** permission.
  * **Response**: Returns the details of the specified shift.

  #### Example:

  ```csharp
  [HttpGet("{id}")]
  [Authorize(Policy = "shifts.view")]
  [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
  public async Task<IActionResult> GetById(string id)
  {
      try
      {
          var shift = await _shiftService.GetByIdAsync(id);
          return shift == null ? NotFound() : Ok(shift);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error getting shift with ID: {id}");
          return StatusCode(500, "An error occurred while retrieving the shift");
      }
  }
  ```

* **POST /api/shift**

  * **Description**: Creates a new shift.
  * **Authorization**: Requires **`shifts.manage`** permission.
  * **Request Body**:

    ```json
    {
      "name": "Morning Shift",
      "startTime": "2025-07-17T08:00:00",
      "endTime": "2025-07-17T16:00:00",
      "description": "A shift during the morning hours",
      "mode": "Strict"
    }
    ```
  * **Response**: Returns the created shift details.

  #### Example:

  ```csharp
  [HttpPost]
  [Authorize(Policy = "shifts.manage")]
  public async Task<IActionResult> Create([FromBody] CreateShiftDto dto)
  {
      try
      {
          var shift = await _shiftService.CreateAsync(dto);
          return CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error creating shift");
          return BadRequest("An error occurred while creating the shift");
      }
  }
  ```

* **PUT /api/shift/{id}**

  * **Description**: Updates an existing shift.
  * **Authorization**: Requires **`shifts.manage`** permission.
  * **Request Body**: Same as `POST` with fields to update.

  #### Example:

  ```csharp
  [HttpPut("{id}")]
  [Authorize(Policy = "shifts.manage")]
  public async Task<IActionResult> Update(string id, [FromBody] UpdateShiftDto dto)
  {
      try
      {
          var shift = await _shiftService.UpdateAsync(id, dto);
          return Ok(shift);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error updating shift with ID: {id}");
          return StatusCode(500, "An error occurred while updating the shift");
      }
  }
  ```

* **DELETE /api/shift/{id}**

  * **Description**: Deletes a specific shift.
  * **Authorization**: Requires **`shifts.manage`** permission.
  * **Response**: Success message or error.

  #### Example:

  ```csharp
  [HttpDelete("{id}")]
  [Authorize(Policy = "shifts.manage")]
  public async Task<IActionResult> Delete(string id)
  {
      try
      {
          var result = await _shiftService.DeleteAsync(id);
          return result ? Ok("Shift deleted successfully") : NotFound("Shift not found");
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error deleting shift with ID: {id}");
          return StatusCode(500, "An error occurred while deleting the shift");
      }
  }
  ```

---

### **2. UserShiftController**

The **UserShiftController** is responsible for managing shift assignments to users.

#### Endpoints:

* **GET /api/users/{userId}/shift/{shiftId}**

  * **Description**: Checks if a user is assigned to a particular shift.
  * **Response**: Returns a boolean indicating the assignment status.

* **POST /api/users/{userId}/shift/{shiftId}**

  * **Description**: Assigns a shift to a user.
  * **Authorization**: Requires **`users.manage`** permission.
  * **Response**: Returns a success message or error.

* **DELETE /api/users/{userId}/shift/{shiftId}**

  * **Description**: Removes a shift assignment from a user.
  * **Authorization**: Requires **`users.manage`** permission.

---

### **3. Assign Shifts to Roles**

#### Endpoints for Mass Assignment by Role:

* **POST /api/roles/{roleId}/shift/{shiftId}**

  * **Description**: Assigns a specific shift to all users in a given role.
  * **Authorization**: Requires **`roles.manage`** permission.
  * **Response**: Success message when the shift is assigned to users in the role.

  #### Example:

  ```csharp
  [HttpPost("role/{roleId}/shift/{shiftId}")]
  [Authorize(Policy = "roles.manage")]
  public async Task<ActionResult> MassAssignShiftToRole(string roleId, string shiftId)
  {
      try
      {
          var result = await _shiftService.MassAssignShiftToRoleAsync(roleId, shiftId);
          return Ok(result);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error in mass assigning shift {ShiftId} to role {RoleId}", shiftId, roleId);
          throw;
      }
  }
  ```

* **DELETE /api/roles/{roleId}/shift/{shiftId}**

  * **Description**: Removes a specific shift from all users in a role.
  * **Authorization**: Requires **`roles.manage`** permission.
  * **Response**: Success message when the shift is removed from users in the role.

  #### Example:

  ```csharp
  [HttpDelete("role/{roleId}/shift/{shiftId}")]
  [Authorize(Policy = "roles.manage")]
  public async Task<ActionResult> MassRemoveUsersFromShiftByRole(string roleId, string shiftId)
  {
      try
      {
          var result = await _shiftService.MassRemoveUsersFromShiftByRoleAsync(roleId, shiftId);
          return Ok(result);
      }
      catch (Exception ex)
      {
          _logger.LogError(ex, "Error in mass removing shift {ShiftId} from role {RoleId}", shiftId, roleId);
          throw;
      }
  }
  ```

---

## Shift Service (Business Logic Layer)

The **ShiftService** handles the core logic for managing shifts. It interacts with the **ShiftRepository** and **UserShiftRepository**.

### Key Methods in **ShiftService**:

* **GetAllAsync**: Retrieves all shifts from the database and returns them as `ShiftDto` objects.
* **GetByIdAsync**: Retrieves a specific shift by its ID.
* **CreateAsync**: Creates a new shift after validating the input data (e.g., start time must be before end time).
* **UpdateAsync**: Updates an existing shift's details (e.g., name, description, timings).
* **DeleteAsync**: Deletes a shift, ensuring that no users are assigned to it.
* **AssignUserToShiftAsync**: Assigns a specific user to a shift, checking for overlapping shifts.
* **RemoveUserFromShiftAsync**: Removes a user from a shift assignment.
* **MassAssignShiftToRoleAsync**: Assigns a shift to all users in a specific role.
* **MassRemoveUsersFromShiftByRoleAsync**: Removes a shift from all users in a specific role.

---

## Shift Repository (Data Access Layer)

The **ShiftRepository** is responsible for interacting with the database and performing CRUD operations on the **Shift** entity. It ensures that shifts are stored, retrieved, updated, and deleted correctly.

### Key Methods in **ShiftRepository**:

* **GetAllAsync**: Retrieves all shifts from the database.
* **GetByIdAsync**: Retrieves a specific shift by its ID.
* **CreateAsync**: Creates a new shift in the database.
* **UpdateAsync**: Updates an existing shift.
* **DeleteAsync**: Deletes a shift from the database.

---

## Error Handling & Logging

* **Serilog**: Detailed logging for each operation to track errors and ensure traceability.
* **Standardized Error Responses**: HTTP status codes like `400` for bad requests, `404` for not found, `500` for server errors are used.

---

## Summary

* **Shift Management** includes endpoints for **creating**, **assigning**, **updating**, and **deleting shifts**.
* **UserShiftController** manages shift assignments to users, both individually and by role.
* **ShiftService** handles the logic for assigning and removing shifts, including mass assignments/removals for roles.
* **ShiftRepository** interacts with the database to persist shift data.
* The system ensures **authorization** using **Role-Based Access Control (RBAC)** to restrict access to certain actions based on permissions.

