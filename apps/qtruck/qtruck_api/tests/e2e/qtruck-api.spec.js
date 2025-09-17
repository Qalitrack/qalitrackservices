// @ts-check
const { test, expect } = require('@playwright/test');

// Test data storage
let adminToken, driverToken, testerToken;
let adminUserId, driverUserId, testerUserId;
let driverProfileId, testerProfileId;
let truckIds = [], materialIds = [], tripId, expenseIds = [];

// Helper function to make API requests
async function apiRequest(request, method, endpoint, data = null, token = null) {
  const options = {
    method,
    headers: {
      'Content-Type': 'application/json',
    },
  };
  
  if (token) {
    options.headers['Authorization'] = `Bearer ${token}`;
  }
  
  if (data) {
    options.data = data;
  }
  
  return await request[method.toLowerCase()](endpoint, options);
}

// Helper function to make file upload requests  
async function apiFileRequest(request, method, endpoint, fields = {}, files = {}, token = null) {
  const fs = require('fs');
  const path = require('path');
  
  const multipartData = {};
  
  // Add regular fields
  for (const [key, value] of Object.entries(fields)) {
    multipartData[key] = value;
  }
  
  // Add files
  for (const [fieldName, filePath] of Object.entries(files)) {
    const fullPath = path.resolve(filePath);
    multipartData[fieldName] = {
      name: path.basename(fullPath),
      mimeType: 'image/jpeg',
      buffer: fs.readFileSync(fullPath)
    };
  }
  
  const options = {
    method,
    headers: {},
    multipart: multipartData
  };
  
  if (token) {
    options.headers['Authorization'] = `Bearer ${token}`;
  }
  
  return await request.fetch(endpoint, options);
}

// Clean database before tests
test.beforeAll(async ({ request }) => {
  // Delete all users (requires database access - in real scenario you'd use a test database)
  console.log('🧹 Setting up clean test environment...');
});

test.describe('QTruck API Complete Test Suite', () => {
  
  test.describe('Phase 1: System Setup & First User Edge Cases', () => {
    
    test('1. First User Registration (Auto-Admin)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'admin@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'System',
        last_name: 'Admin'
      });
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      
      adminUserId = data.id;
      expect(data.user_type).toBe('admin');
      expect(data.is_approved).toBe(true);
      expect(data.base_email).toBe('admin@qtruck.com');
    });
    
    test('2. Admin Login', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/login/', {
        username: 'admin@qtruck.com',
        password: 'SecurePass123!'
      });
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      
      adminToken = data.access;
      expect(data.user_type).toBe('admin');
      expect(data.user_id).toBe(adminUserId);
    });
    
    test('3. Second User Registration (Security: No Admin via Public Registration)', async ({ request }) => {
      // FEATURE: Security - No more admins can be created via public registration after first user
      // Admin accounts can only be created by existing admins for security
      await test.step('Attempt to register without alias (should fail - no more public admins)', async () => {
        const response = await apiRequest(request, 'POST', '/auth/register/', {
          email: 'manager@qtruck.com',
          password: 'SecurePass123!',
          password_confirm: 'SecurePass123!',
          first_name: 'Manager',
          last_name: 'User'
        });
        
        expect(response.status()).toBe(400);
        const data = await response.json();
        expect(data.email[0]).toContain('Admin accounts can only be created by existing admins');
      });
    });
    
    test('4. Verify Admin Registration via Public Endpoint is Blocked', async ({ request }) => {
      // FEATURE: Security - Prevent admin account creation through public registration
      // Only existing admins should be able to create new admin accounts
      await test.step('Attempt to register with +admin alias (should fail)', async () => {
        const response = await apiRequest(request, 'POST', '/auth/register/', {
          email: 'hacker+admin@qtruck.com',
          password: 'SecurePass123!',
          password_confirm: 'SecurePass123!',
          first_name: 'Hacker',
          last_name: 'User'
        });
        
        expect(response.status()).toBe(400);
        const data = await response.json();
        expect(data.email[0]).toContain('Admin accounts can only be created by existing admins');
      });
    });

    test('5. Valid Driver Registration with +driver Alias', async ({ request }) => {
      // FEATURE: Role-based Registration - Drivers can register with +driver alias
      await test.step('Register as driver using +driver alias', async () => {
        const response = await apiRequest(request, 'POST', '/auth/register/', {
          email: 'manager+driver@qtruck.com',
          password: 'SecurePass123!',
          password_confirm: 'SecurePass123!',
          first_name: 'Manager',
          last_name: 'User'
        });
        
        expect(response.status()).toBe(201);
        const data = await response.json();
        expect(data.user_type).toBe('driver');
        expect(data.is_approved).toBe(false); // NOT auto-approved
      });
    });
  });

  test.describe('Phase 2: Tester Registration Edge Cases', () => {
    
    test('5. Tester Registration BEFORE Admin Disables (Default: Enabled)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'test+tester@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'Test',
        last_name: 'User'
      });
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      
      testerUserId = data.id;
      expect(data.user_type).toBe('tester');
      expect(data.base_email).toBe('test@qtruck.com');
      expect(data.is_approved).toBe(false);
    });
    
    test('6. Admin Disables Tester Registration', async ({ request }) => {
      const response = await apiRequest(request, 'PATCH', '/api/system-settings/1/', {
        tester_registration_enabled: false
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('7. Tester Registration AFTER Admin Disables (Should Fail)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'blocked+tester@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'Blocked',
        last_name: 'Tester'
      });
      
      expect(response.status()).toBe(400);
      const data = await response.json();
      expect(JSON.stringify(data)).toContain('disabled');
    });
    
    test('8. Admin Re-enables Tester Registration', async ({ request }) => {
      const response = await apiRequest(request, 'PATCH', '/api/system-settings/1/', {
        tester_registration_enabled: true
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('9. Tester Registration AFTER Re-enabling', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'allowed+tester@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'Allowed',
        last_name: 'Tester'
      });
      
      expect(response.status()).toBe(201);
    });
  });

  test.describe('Phase 3: Login Edge Cases with Testers', () => {
    
    test('10. Admin Disables Tester Login', async ({ request }) => {
      const response = await apiRequest(request, 'PATCH', '/api/system-settings/1/', {
        tester_registration_enabled: true,
        tester_login_enabled: false
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('11. Approve First Tester', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/admin/approve-user/', {
        user_id: testerUserId,
        action: 'approve'
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('12. Approved Tester Login Attempt (Login Disabled - Should Fail)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/login/', {
        username: 'test+tester@qtruck.com',
        password: 'SecurePass123!'
      });
      
      expect(response.status()).toBe(400);
      const data = await response.json();
      expect(JSON.stringify(data)).toContain('disabled');
    });
    
    test('13. Admin Re-enables Tester Login', async ({ request }) => {
      const response = await apiRequest(request, 'PATCH', '/api/system-settings/1/', {
        tester_login_enabled: true
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('14. Tester Login After Re-enabling', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/login/', {
        username: 'test+tester@qtruck.com',
        password: 'SecurePass123!'
      });
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      
      testerToken = data.access;
      expect(data.user_type).toBe('tester');
    });
  });

  test.describe('Phase 4: Driver Registration & Profile Creation', () => {
    
    test('15. Driver Registration with Alias', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'john+driver@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'John',
        last_name: 'Driver'
      });
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      
      driverUserId = data.id;
      expect(data.user_type).toBe('driver');
      expect(data.base_email).toBe('john@qtruck.com');
    });
    
    test('16. Admin Approves Driver', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/admin/approve-user/', {
        user_id: driverUserId,
        action: 'approve'
      }, adminToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('17. Driver Login', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/login/', {
        username: 'john+driver@qtruck.com',
        password: 'SecurePass123!'
      });
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      
      driverToken = data.access;
    });
    
    test('18. Driver Creates Profile', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/drivers/', {
        name: 'John Driver',
        phone: '+1234567890',
        license_number: 'DL123456'
      }, driverToken);
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      driverProfileId = data.id;
    });
    
    test('19. Tester Creates Profile (Should Work)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/drivers/', {
        name: 'Test Driver Profile',
        phone: '+0987654321',
        license_number: 'TL123456'
      }, testerToken);
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      testerProfileId = data.id;
    });
  });

  test.describe('Phase 5: Fleet Data Setup (Admin Only)', () => {
    
    test('20. Admin Creates Trucks', async ({ request }) => {
      const trucks = [
        { license_plate: 'TRK-001', model: 'Ford F-150' },
        { license_plate: 'TRK-002', model: 'Chevy Silverado' },
        { license_plate: 'TRK-003', model: 'Toyota Tacoma' }
      ];
      
      for (const truck of trucks) {
        const response = await apiRequest(request, 'POST', '/api/trucks/', truck, adminToken);
        expect(response.status()).toBe(201);
        const data = await response.json();
        truckIds.push(data.id);
      }
    });
    
    test('21. Admin Creates Base Materials', async ({ request }) => {
      const materials = [
        { name: 'Sand' },
        { name: 'Gravel' },
        { name: 'Cement' }
      ];
      
      for (const material of materials) {
        const response = await apiRequest(request, 'POST', '/api/materials/', material, adminToken);
        expect(response.status()).toBe(201);
        const data = await response.json();
        materialIds.push(data.id);
      }
    });
    
    test('22. Driver Attempts to Create Truck (Should Fail)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/trucks/', {
        license_plate: 'TRK-004',
        model: 'Honda Ridgeline'
      }, driverToken);
      
      expect(response.status()).toBe(403);
    });
    
    test('23. Tester Attempts to Create Material (Should Fail)', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/materials/', {
        name: 'Unauthorized Material'
      }, testerToken);
      
      expect(response.status()).toBe(403);
    });
  });

  test.describe('Phase 6: Admin User Management Functions', () => {
    
    test('24. Admin Views All Users', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/admin/users/', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.length).toBeGreaterThan(0);
    });
    
    test('25. Admin Views Users by Type - Testers', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/admin/users/?user_type=tester', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.length).toBeGreaterThanOrEqual(1);
      expect(data[0].user_type).toBe('tester');
    });
    
    test('26. Admin Views Users by Type - Drivers', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/admin/users/?user_type=driver', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.length).toBeGreaterThanOrEqual(1);
      expect(data[0].user_type).toBe('driver');
    });
    
    test('27. Admin Views Users by Type - Admins', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/admin/users/?user_type=admin', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.length).toBeGreaterThanOrEqual(1);
      expect(data[0].user_type).toBe('admin');
    });

    test('28. Admin Creates Secondary Admin Account (Security Feature)', async ({ request }) => {
      // FEATURE: Secure Admin Creation - Only existing admins can create new admin accounts
      // This prevents privilege escalation through public registration
      await test.step('Create new admin account via admin-only endpoint', async () => {
        const response = await apiRequest(request, 'POST', '/api/admin/create-admin/', {
          email: 'secondadmin@qtruck.com',
          password: 'AdminPass123!',
          first_name: 'Second',
          last_name: 'Admin'
        }, adminToken);
        
        expect(response.status()).toBe(201);
        const data = await response.json();
        expect(data.user.user_type).toBe('admin');
        expect(data.user.is_approved).toBe(true); // Auto-approved when created by admin
        expect(data.message).toContain('Admin user created successfully');
      });
    });

    test('29. Verify Non-Admin Cannot Create Admin Accounts', async ({ request }) => {
      // FEATURE: Authorization - Only admins can access admin creation endpoint
      await test.step('Driver attempts to create admin (should fail)', async () => {
        const response = await apiRequest(request, 'POST', '/api/admin/create-admin/', {
          email: 'hackadmin@qtruck.com',
          password: 'HackPass123!',
          first_name: 'Hack',
          last_name: 'Admin'
        }, driverToken);
        
        expect(response.status()).toBe(403); // Forbidden
      });
    });
  });

  test.describe('Phase 7: Trip Operations & Cost Calculation', () => {
    
    test('30. Driver Creates Trip', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/trips/', {
        truck_id: truckIds[0],
        driver_id: driverProfileId,
        start_location: 'Warehouse A',
        end_location: 'Site B',
        start_mileage: 15000,
        date: '2025-09-17T08:00:00Z',
        status: 'pending'
      }, driverToken);
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      tripId = data.id;
      expect(data.total_cost).toBe('0.00');
    });
    
    test('29. Driver Adds Multiple Expenses (Test Auto-Calculation)', async ({ request }) => {
      const expenses = [
        { description: 'Fuel', amount: '85.50' },
        { description: 'Toll', amount: '12.00' },
        { description: 'Parking', amount: '8.00' }
      ];
      
      for (const expense of expenses) {
        const response = await apiRequest(request, 'POST', '/api/expenses/', {
          trip_id: tripId,
          ...expense
        }, driverToken);
        
        expect(response.status()).toBe(201);
        const data = await response.json();
        expenseIds.push(data.id);
      }
    });
    
    test('30. Verify Auto-Calculated Trip Cost', async ({ request }) => {
      const response = await apiRequest(request, 'GET', `/api/trips/${tripId}/`, null, driverToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.total_cost).toBe('105.50'); // 85.50 + 12.00 + 8.00
    });
    
    test('31. Driver Updates Expense (Test Recalculation)', async ({ request }) => {
      const response = await apiRequest(request, 'PATCH', `/api/expenses/${expenseIds[0]}/`, {
        amount: '95.50'
      }, driverToken);
      
      expect(response.status()).toBe(200);
    });
    
    test('32. Verify Recalculated Trip Cost', async ({ request }) => {
      const response = await apiRequest(request, 'GET', `/api/trips/${tripId}/`, null, driverToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.total_cost).toBe('115.50'); // 95.50 + 12.00 + 8.00
    });
  });

  test.describe('Phase 7.5: Image Upload Testing', () => {
    let receiptId;
    
    test('32.1. Driver Updates Trip with Proof Images', async ({ request }) => {
      // Update trip with start and end proof images  
      const response = await apiFileRequest(request, 'PATCH', `/api/trips/${tripId}/`, 
        {}, 
        {
          proof_image: 'tests/sample-images/trip_proof_start.jpg',
          proof_end_image: 'tests/sample-images/trip_proof_end.jpg'
        }, 
        driverToken
      );
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.proof_image).toBeTruthy();
      expect(data.proof_end_image).toBeTruthy();
    });

    test('32.2. Driver Creates Receipt with Image', async ({ request }) => {
      // First create an expense to attach receipt to
      const expenseResponse = await apiRequest(request, 'POST', '/api/expenses/', {
        trip_id: tripId,
        description: 'Gas with Receipt',
        amount: '75.25'
      }, driverToken);
      
      expect(expenseResponse.status()).toBe(201);
      const expenseData = await expenseResponse.json();
      
      // Now create receipt with image
      const receiptResponse = await apiFileRequest(request, 'POST', '/api/receipts/',
        {
          expense_id: expenseData.id,
          note: 'Gas station receipt with image'
        },
        {
          image: 'tests/sample-images/receipt_image.jpg'
        },
        driverToken
      );
      
      expect(receiptResponse.status()).toBe(201);
      const receiptData = await receiptResponse.json();
      receiptId = receiptData.id;
      expect(receiptData.image).toBeTruthy();
      expect(receiptData.note).toBe('Gas station receipt with image');
    });

    test('32.3. Driver Creates Vehicle Mileage with Proof Images', async ({ request }) => {
      const response = await apiFileRequest(request, 'POST', '/api/vehicle-mileage/',
        {
          truck_id: truckIds[0],
          driver_id: driverProfileId,
          start_mileage: '25000',
          end_mileage: '25150',
          date: '2025-09-17T10:00:00Z'
        },
        {
          proof_image: 'tests/sample-images/mileage_proof_start.jpg',
          proof_end_image: 'tests/sample-images/mileage_proof_end.jpg'
        },
        driverToken
      );
      
      expect(response.status()).toBe(201);
      const data = await response.json();
      expect(data.proof_image).toBeTruthy();
      expect(data.proof_end_image).toBeTruthy();
      expect(data.mileage).toBe(150.0); // Auto-calculated
    });

    test('32.4. Tester Can Also Upload Trip Images (Same as Driver)', async ({ request }) => {
      // Tester can also upload trip proof images
      const response = await apiFileRequest(request, 'PATCH', `/api/trips/${tripId}/`, 
        {}, 
        {
          proof_image: 'tests/sample-images/trip_proof_start.jpg'
        }, 
        testerToken
      );
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.proof_image).toBeTruthy();
    });
  });

  test.describe('Phase 8: Feedback System Testing', () => {
    
    test('33. Driver Submits Feature Request', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/feedback/', {
        feedback_type: 'feature_request',
        subject: 'GPS Integration',
        description: 'Would like real-time GPS tracking for trips'
      }, driverToken);
      
      expect(response.status()).toBe(201);
    });
    
    test('34. Tester Submits Bug Report', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/api/feedback/', {
        feedback_type: 'bug_report',
        subject: 'App Crashes on Photo Upload',
        description: 'App crashes when trying to upload large receipt images'
      }, testerToken);
      
      expect(response.status()).toBe(201);
    });
    
    test('35. Admin Views Pending Feedback', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/feedback/pending/', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      expect(data.length).toBeGreaterThanOrEqual(2);
    });
  });

  test.describe('Phase 9: Edge Cases & Error Handling', () => {
    
    test('36. Invalid Role Alias Registration', async ({ request }) => {
      const response = await apiRequest(request, 'POST', '/auth/register/', {
        email: 'user+invalid@qtruck.com',
        password: 'SecurePass123!',
        password_confirm: 'SecurePass123!',
        first_name: 'Invalid',
        last_name: 'Role'
      });
      
      expect(response.status()).toBe(400);
      const data = await response.json();
      expect(JSON.stringify(data)).toContain('Invalid role alias');
    });
    
    test('37. Driver Cannot Access Admin Functions', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/admin/pending-users/', null, driverToken);
      
      expect(response.status()).toBe(403);
    });
    
    test('38. Tester Cannot Access Other User Data', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/expenses/', null, testerToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      // Should only see own expenses (none for tester in this test)
      expect(data.length).toBe(0);
    });
  });

  test.describe('Phase 10: Access Control Verification', () => {
    
    test('39. Driver Views Own Trips Only', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/trips/', null, driverToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      // Should only see own trip
      expect(data.length).toBe(1);
      expect(data[0].id).toBe(tripId);
    });
    
    test('40. Admin Views All Trips', async ({ request }) => {
      const response = await apiRequest(request, 'GET', '/api/trips/', null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      // Admin should see all trips
      expect(data.length).toBeGreaterThanOrEqual(1);
    });

    test('41. Admin Filters Expenses by Driver', async ({ request }) => {
      const response = await apiRequest(request, 'GET', `/api/expenses/by_driver/?driver_id=${driverProfileId}`, null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      // Should see the 3 expenses created for this driver
      expect(data.length).toBe(3);
      expect(data[0].driver.id).toBe(driverProfileId);
    });

    test('42. Admin Filters Expenses by Truck', async ({ request }) => {
      // Get truck ID from trip
      const tripResponse = await apiRequest(request, 'GET', `/api/trips/${tripId}/`, null, adminToken);
      const tripData = await tripResponse.json();
      const truckId = tripData.truck.id;

      const response = await apiRequest(request, 'GET', `/api/expenses/by_truck/?truck_id=${truckId}`, null, adminToken);
      
      expect(response.status()).toBe(200);
      const data = await response.json();
      // Should see the 3 expenses for trips using this truck
      expect(data.length).toBe(3);
    });

    test('43. Driver Cannot Use Admin-Only Filtering Endpoints', async ({ request }) => {
      const response = await apiRequest(request, 'GET', `/api/expenses/by_driver/?driver_id=${driverProfileId}`, null, driverToken);
      
      expect(response.status()).toBe(400);
      const data = await response.json();
      expect(data.error).toContain('admin access needed');
    });

  });

  // Test Summary
  test.afterAll(async () => {
    console.log('\\n🎯 Test Summary:');
    console.log(`✅ Admin Token: ${adminToken ? 'Generated' : 'Failed'}`);
    console.log(`✅ Driver Token: ${driverToken ? 'Generated' : 'Failed'}`);
    console.log(`✅ Tester Token: ${testerToken ? 'Generated' : 'Failed'}`);
    console.log(`✅ Trucks Created: ${truckIds.length}`);
    console.log(`✅ Materials Created: ${materialIds.length}`);
    console.log(`✅ Trip Created: ${tripId ? 'Yes' : 'No'}`);
    console.log(`✅ Expenses Created: ${expenseIds.length}`);
  });
});