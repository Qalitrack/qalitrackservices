# 🧪 QTruck API Comprehensive Test Plan

## 🎯 **Complete Testing Sequence (Real Deployment Scenario)**

### **Phase 1: System Setup & First User Edge Cases**

#### **1. First User Registration (Auto-Admin)**
```bash
POST /auth/register/
{
  "email": "admin@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "System",
  "last_name": "Admin"
}
```
**Expected**: ✅ `user_type: 'admin'`, `is_approved: true`, `is_staff: true`, `is_superuser: true`

#### **2. Second User Registration (Should NOT be Auto-Admin)**
```bash
POST /auth/register/
{
  "email": "manager@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "Manager",
  "last_name": "User"
}
```
**Expected**: ✅ `user_type: 'admin'` (no alias), `is_approved: false` (NOT auto-approved)

#### **3. Verify Second User Cannot Login**
```bash
POST /auth/login/
{
  "username": "manager@qtruck.com",
  "password": "SecurePass123!"
}
```
**Expected**: ❌ "Account pending approval"

---

### **Phase 2: Tester Registration Edge Cases**

#### **4. Tester Registration BEFORE Admin Allows (Default: Enabled)**
```bash
POST /auth/register/
{
  "email": "test+tester@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "Test",
  "last_name": "User"
}
```
**Expected**: ✅ Registration successful (tester registration enabled by default)

#### **5. Admin Disables Tester Registration**
```bash
PUT /api/system-settings/
Headers: Authorization: Bearer <admin_token>
{
  "tester_registration_enabled": false
}
```

#### **6. Tester Registration AFTER Admin Disables**
```bash
POST /auth/register/
{
  "email": "blocked+tester@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "Blocked",
  "last_name": "Tester"
}
```
**Expected**: ❌ "Tester registration is currently disabled"

#### **7. Admin Re-enables Tester Registration**
```bash
PUT /api/system-settings/
{
  "tester_registration_enabled": true
}
```

#### **8. Tester Registration AFTER Re-enabling**
```bash
POST /auth/register/
{
  "email": "allowed+tester@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "Allowed",
  "last_name": "Tester"
}
```
**Expected**: ✅ Registration successful

---

### **Phase 3: Login Edge Cases with Testers**

#### **9. Admin Disables Tester Login (While Registration Enabled)**
```bash
PUT /api/system-settings/
{
  "tester_registration_enabled": true,
  "tester_login_enabled": false
}
```

#### **10. Approve First Tester**
```bash
POST /admin/approve-user/
{
  "user_id": "<test_tester_id>",
  "action": "approve"
}
```

#### **11. Approved Tester Login Attempt (Login Disabled)**
```bash
POST /auth/login/
{
  "username": "test+tester@qtruck.com",
  "password": "SecurePass123!"
}
```
**Expected**: ❌ "Tester login is currently disabled"

#### **12. Admin Re-enables Tester Login**
```bash
PUT /api/system-settings/
{
  "tester_login_enabled": true
}
```

#### **13. Tester Login After Re-enabling**
```bash
POST /auth/login/
{
  "username": "test+tester@qtruck.com",
  "password": "SecurePass123!"
}
```
**Expected**: ✅ Successful login

---

### **Phase 4: Driver Registration & Profile Creation**

#### **14. Driver Registration with Alias**
```bash
POST /auth/register/
{
  "email": "john+driver@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "John",
  "last_name": "Driver"
}
```

#### **15. Admin Approves Driver**
```bash
POST /admin/approve-user/
{
  "user_id": "<john_driver_id>",
  "action": "approve"
}
```

#### **16. Driver Creates Profile**
```bash
POST /api/drivers/
Headers: Authorization: Bearer <driver_token>
{
  "name": "John Driver",
  "phone": "+1234567890",
  "license_number": "DL123456"
}
```

#### **17. Tester Creates Profile (Should Work)**
```bash
POST /api/drivers/
Headers: Authorization: Bearer <tester_token>
{
  "name": "Test Driver Profile",
  "phone": "+0987654321",
  "license_number": "TL123456"
}
```
**Expected**: ✅ Testers can create driver profiles

---

### **Phase 5: Fleet Data Setup (Admin Only)**

#### **18. Admin Creates Trucks**
```bash
POST /api/trucks/
Headers: Authorization: Bearer <admin_token>
[
  {"license_plate": "TRK-001", "model": "Ford F-150"},
  {"license_plate": "TRK-002", "model": "Chevy Silverado"},
  {"license_plate": "TRK-003", "model": "Toyota Tacoma"}
]
```

#### **19. Admin Creates Base Materials**
```bash
POST /api/materials/
Headers: Authorization: Bearer <admin_token>
[
  {"name": "Sand", "destination": "Construction Site A"},
  {"name": "Gravel", "destination": "Construction Site B"},
  {"name": "Cement", "destination": "Construction Site C"}
]
```

#### **20. Driver Attempts to Create Truck (Should Fail)**
```bash
POST /api/trucks/
Headers: Authorization: Bearer <driver_token>
{
  "license_plate": "TRK-004",
  "model": "Honda Ridgeline"
}
```
**Expected**: ❌ Permission denied (IsAdminOrReadOnly)

#### **21. Tester Attempts to Create Material (Should Fail)**
```bash
POST /api/materials/
Headers: Authorization: Bearer <tester_token>
{
  "name": "Unauthorized Material"
}
```
**Expected**: ❌ Permission denied (IsAdminOrReadOnly)

---

### **Phase 6: Trip Operations & Cost Calculation**

#### **22. Driver Creates Trip**
```bash
POST /api/trips/
Headers: Authorization: Bearer <driver_token>
{
  "truck_id": "<truck_001_id>",
  "driver_id": "<john_driver_profile_id>",
  "start_location": "Warehouse A",
  "end_location": "Site B",
  "start_mileage": 15000,
  "date": "2025-09-17T08:00:00Z",
  "status": "pending"
}
```

#### **23. Driver Adds Multiple Expenses (Test Auto-Calculation)**
```bash
POST /api/expenses/
Headers: Authorization: Bearer <driver_token>
[
  {
    "trip_id": "<trip_id>",
    "driver_id": "<john_driver_profile_id>",
    "description": "Fuel",
    "amount": "85.50"
  },
  {
    "trip_id": "<trip_id>",
    "driver_id": "<john_driver_profile_id>",
    "description": "Toll",
    "amount": "12.00"
  },
  {
    "trip_id": "<trip_id>",
    "driver_id": "<john_driver_profile_id>",
    "description": "Parking",
    "amount": "8.00"
  }
]
```

#### **24. Verify Auto-Calculated Trip Cost**
```bash
GET /api/trips/<trip_id>/
Headers: Authorization: Bearer <driver_token>
```
**Expected**: ✅ `total_cost: 105.50` (auto-calculated via signals)

#### **25. Driver Updates Expense (Test Recalculation)**
```bash
PATCH /api/expenses/<fuel_expense_id>/
Headers: Authorization: Bearer <driver_token>
{
  "amount": "95.50"
}
```

#### **26. Verify Recalculated Trip Cost**
```bash
GET /api/trips/<trip_id>/
```
**Expected**: ✅ `total_cost: 115.50` (recalculated automatically)

---

### **Phase 7: Receipt Management & OCR**

#### **27. Driver Adds Receipt with OCR Placeholder**
```bash
POST /api/receipts/
Headers: Authorization: Bearer <driver_token>
{
  "expense_id": "<fuel_expense_id>",
  "image": "<base64_image>",
  "note": "Gas station receipt"
}
```

#### **28. Extract Receipt Details (OCR Simulation)**
```bash
POST /api/receipts/<receipt_id>/extract_details/
Headers: Authorization: Bearer <driver_token>
```
**Expected**: ✅ Returns extracted JSON data (placeholder implementation)

---

### **Phase 8: Feedback System Testing**

#### **29. Driver Submits Feature Request**
```bash
POST /api/feedback/
Headers: Authorization: Bearer <driver_token>
{
  "feedback_type": "feature_request",
  "subject": "GPS Integration",
  "description": "Would like real-time GPS tracking for trips to optimize routes"
}
```

#### **30. Tester Submits Bug Report**
```bash
POST /api/feedback/
Headers: Authorization: Bearer <tester_token>
{
  "feedback_type": "bug_report",
  "subject": "App Crashes on Photo Upload",
  "description": "App crashes when trying to upload large receipt images"
}
```

#### **31. Driver Submits General Feedback**
```bash
POST /api/feedback/
Headers: Authorization: Bearer <driver_token>
{
  "feedback_type": "suggestion",
  "subject": "Dark Mode",
  "description": "Please add dark mode for night driving"
}
```

#### **32. Admin Views All Pending Feedback**
```bash
GET /api/feedback/pending/
Headers: Authorization: Bearer <admin_token>
```
**Expected**: ✅ Shows all 3 feedback items

#### **33. Admin Responds to Feature Request**
```bash
POST /api/feedback/<gps_feedback_id>/respond/
Headers: Authorization: Bearer <admin_token>
{
  "feedback_id": "<gps_feedback_id>",
  "status": "reviewed",
  "admin_response": "Great suggestion! GPS integration is planned for Q1 2026."
}
```

#### **34. Admin Marks Bug as In Progress**
```bash
POST /api/feedback/<bug_feedback_id>/respond/
{
  "status": "in_progress",
  "admin_response": "Bug confirmed. Our team is working on a fix."
}
```

---

### **Phase 9: Access Control Verification**

#### **35. Driver Views Own Trips Only**
```bash
GET /api/trips/
Headers: Authorization: Bearer <driver_token>
```
**Expected**: ✅ Only sees own trips

#### **36. Admin Views All Trips**
```bash
GET /api/trips/
Headers: Authorization: Bearer <admin_token>
```
**Expected**: ✅ Sees all trips from all drivers

#### **37. Tester Cannot Access Other User's Data**
```bash
GET /api/expenses/
Headers: Authorization: Bearer <tester_token>
```
**Expected**: ✅ Only sees own expenses (none yet)

#### **38. Driver Cannot Access Admin Functions**
```bash
GET /admin/pending-users/
Headers: Authorization: Bearer <driver_token>
```
**Expected**: ❌ Permission denied

---

### **Phase 10: Email Aliasing Edge Cases**

#### **39. Invalid Role Alias Registration**
```bash
POST /auth/register/
{
  "email": "user+invalid@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "Invalid",
  "last_name": "Role"
}
```
**Expected**: ❌ "Invalid role alias: invalid. Use +admin, +driver, or +tester."

#### **40. Login with Wrong Alias**
```bash
POST /auth/login/
{
  "username": "john+admin@qtruck.com",
  "password": "SecurePass123!"
}
```
**Expected**: ❌ "Invalid credentials" (user registered as driver, not admin)

#### **41. Duplicate User Same Email Different Role**
```bash
POST /auth/register/
{
  "email": "john+admin@qtruck.com",
  "password": "SecurePass123!",
  "password_confirm": "SecurePass123!",
  "first_name": "John",
  "last_name": "Admin"
}
```
**Expected**: ✅ Should work (different role, same base email)

---

### **Phase 11: System Analytics & Reporting**

#### **42. Filter Trips by Status**
```bash
GET /api/trips/by_status/?status=completed
Headers: Authorization: Bearer <admin_token>
```

#### **43. Trip Cost Calculation Endpoint**
```bash
POST /api/trips/<trip_id>/calculate_cost/
Headers: Authorization: Bearer <driver_token>
```

#### **44. Admin Views System Settings**
```bash
GET /api/system-settings/
Headers: Authorization: Bearer <admin_token>
```

---

## 🎭 **Playwright Test Summary**

**Total Tests**: 44 test cases covering:
- ✅ First user auto-admin behavior
- ✅ Subsequent user approval workflow  
- ✅ Tester registration/login controls
- ✅ Email aliasing with all role combinations
- ✅ Access control enforcement
- ✅ Auto trip cost calculation
- ✅ Feedback system workflow
- ✅ Edge cases and error handling
- ✅ Profile creation for drivers/testers
- ✅ Fleet data management (admin-only)

**Expected Outcomes**:
- 29 ✅ Successful operations
- 15 ❌ Expected failures (permissions, validations)