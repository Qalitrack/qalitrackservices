"""
Comprehensive test suite for authentication and user management endpoints.

Test scenarios covered:
1. Admin registration flow
2. Driver registration and approval workflow  
3. Authentication and authorization flows
4. User management operations (activate/deactivate/delete)
5. Proper REST endpoint consolidation
"""

import json
from django.test import TestCase
from django.urls import reverse
from rest_framework.test import APIClient
from rest_framework import status
from django.contrib.auth import get_user_model
from users.models import CustomUser, UserProfile

CustomUser = get_user_model()


class AuthenticationAndUserManagementTestCase(TestCase):
    def setUp(self):
        """Set up test environment"""
        self.client = APIClient()
        
        # Clear any existing users to ensure clean state
        CustomUser.objects.all().delete()
        
        # Test data
        self.admin_data = {
            'email': 'testadmin+admin@example.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Test',
            'last_name': 'Admin'
        }
        
        self.driver_data = {
            'email': 'testdriver+driver@example.com', 
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Test',
            'last_name': 'Driver'
        }
        
        self.second_admin_data = {
            'email': 'secondadmin+admin@example.com',
            'password': 'testpass123', 
            'password_confirm': 'testpass123',
            'first_name': 'Second',
            'last_name': 'Admin'
        }


    def test_01_register_first_admin_public_endpoint(self):
        """Test registering the first admin via public registration endpoint"""
        print("\n=== TEST: Register first admin via public endpoint ===")
        
        # First admin registration should succeed (auto-approved)
        response = self.client.post('/auth/register/', self.admin_data)
        print(f"Response status: {response.status_code}")
        print(f"Response data: {response.data}")
        
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify user was created and auto-approved
        admin_user = CustomUser.objects.get(email=self.admin_data['email'])
        self.assertEqual(admin_user.user_type, 'admin')
        self.assertTrue(admin_user.is_approved)
        self.assertTrue(admin_user.is_active)
        self.assertTrue(admin_user.is_staff)
        self.assertTrue(admin_user.is_superuser)
        
        return admin_user

    def test_02_admin_login_and_get_info(self):
        """Test admin login and getting admin info"""
        print("\n=== TEST: Admin login and get user info ===")
        
        # First create admin
        admin_user = self.test_01_register_first_admin_public_endpoint()
        
        # Login as admin
        login_response = self.client.post('/auth/login/', {
            'email': self.admin_data['email'],
            'password': self.admin_data['password']
        })
        print(f"Login response status: {login_response.status_code}")
        print(f"Login response data: {login_response.data}")
        
        self.assertEqual(login_response.status_code, status.HTTP_200_OK)
        self.assertIn('access', login_response.data)
        
        # Set authorization header
        token = login_response.data['access']
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
        
        # Get current user info via /api/users/me/
        me_response = self.client.get('/api/users/me/')
        print(f"Me endpoint response: {me_response.data}")
        
        self.assertEqual(me_response.status_code, status.HTTP_200_OK)
        self.assertEqual(me_response.data['email'], self.admin_data['email'])
        self.assertEqual(me_response.data['user_type'], 'admin')
        self.assertEqual(me_response.data['status'], 'approved')
        
        return token

    def test_03_register_driver_via_public_endpoint(self):
        """Test driver registration via public endpoint (should be pending approval)"""
        print("\n=== TEST: Register driver via public endpoint ===")
        
        # Ensure first admin exists (so this driver won't become admin)
        if not CustomUser.objects.exists():
            CustomUser.objects.create_user(
                email='fixture+admin@example.com',
                password='testpass123',
                first_name='Fixture',
                last_name='Admin'
            )
        
        # Register driver
        response = self.client.post('/auth/register/', self.driver_data)
        print(f"Driver registration response: {response.status_code}")
        print(f"Driver registration data: {response.data}")
        
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify driver was created but NOT auto-approved
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        self.assertEqual(driver_user.user_type, 'driver')
        self.assertFalse(driver_user.is_approved)  # Should be pending approval
        self.assertTrue(driver_user.is_active)     # But still active
        
        return driver_user

    def test_04_driver_login_should_fail_not_approved(self):
        """Test that unapproved driver cannot login"""
        print("\n=== TEST: Unapproved driver login should fail ===")
        
        # Ensure first admin exists
        if not CustomUser.objects.exists():
            CustomUser.objects.create_user(
                email='fixture+admin@example.com',
                password='testpass123',
                first_name='Fixture',
                last_name='Admin'
            )
        
        # Register driver first
        response = self.client.post('/auth/register/', self.driver_data)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Attempt login as unapproved driver
        login_response = self.client.post('/auth/login/', {
            'email': self.driver_data['email'],
            'password': self.driver_data['password']
        })
        print(f"Unapproved driver login response: {login_response.status_code}")
        print(f"Response data: {login_response.data}")
        
        # Login should fail for unapproved user
        self.assertEqual(login_response.status_code, status.HTTP_401_UNAUTHORIZED)

    def test_05_register_second_admin_public_should_fail(self):
        """Test that registering second admin via public endpoint should fail"""
        print("\n=== TEST: Second admin registration via public endpoint should fail ===")
        
        # Create first admin
        self.test_01_register_first_admin_public_endpoint()
        
        # Attempt to register second admin via public endpoint
        response = self.client.post('/auth/register/', self.second_admin_data)
        print(f"Second admin registration response: {response.status_code}")
        print(f"Response data: {response.data}")
        
        # Should fail - only first user gets auto-admin privileges
        # Second admin should be registered as driver or require admin creation
        self.assertNotEqual(response.status_code, status.HTTP_201_CREATED)

    def test_06_admin_approve_driver(self):
        """Test admin approving a pending driver"""
        print("\n=== TEST: Admin approve pending driver ===")
        
        # Setup: Create admin and login
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        # Create pending driver
        driver_user = self.test_03_register_driver_via_public_endpoint()
        
        # Admin approves driver via PATCH /users/{id}/
        approve_response = self.client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'approved'
        })
        print(f"Approve driver response: {approve_response.status_code}")
        print(f"Response data: {approve_response.data}")
        
        self.assertEqual(approve_response.status_code, status.HTTP_200_OK)
        
        # Verify driver is now approved
        driver_user.refresh_from_db()
        self.assertTrue(driver_user.is_approved)
        self.assertTrue(driver_user.is_active)
        
        # Verify profile was updated
        profile = UserProfile.objects.get(user=driver_user)
        self.assertIsNotNone(profile.approval_date)
        self.assertIsNotNone(profile.approved_by)

    def test_07_approved_driver_can_login(self):
        """Test that approved driver can now login"""
        print("\n=== TEST: Approved driver can login ===")
        
        # Setup: Admin approves driver
        self.test_06_admin_approve_driver()
        
        # Clear auth credentials
        self.client.credentials()
        
        # Driver should now be able to login
        login_response = self.client.post('/auth/login/', {
            'email': self.driver_data['email'],
            'password': self.driver_data['password']
        })
        print(f"Approved driver login response: {login_response.status_code}")
        print(f"Response data keys: {login_response.data.keys() if hasattr(login_response, 'data') else 'No data'}")
        
        self.assertEqual(login_response.status_code, status.HTTP_200_OK)
        self.assertIn('access', login_response.data)
        
        return login_response.data['access']

    def test_08_admin_reject_driver(self):
        """Test admin rejecting a driver (move from approved back to rejected)"""
        print("\n=== TEST: Admin reject driver ===")
        
        # Setup: Admin approves driver first
        self.test_06_admin_approve_driver()
        
        # Get admin token and driver
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Admin rejects driver via PATCH /users/{id}/
        reject_response = self.client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'rejected'
        })
        print(f"Reject driver response: {reject_response.status_code}")
        print(f"Response data: {reject_response.data}")
        
        self.assertEqual(reject_response.status_code, status.HTTP_200_OK)
        
        # Verify driver is now rejected
        driver_user.refresh_from_db()
        self.assertFalse(driver_user.is_approved)
        self.assertFalse(driver_user.is_active)  # Rejected users are deactivated

    def test_09_move_from_rejected_to_approved(self):
        """Test moving user from rejected back to approved"""
        print("\n=== TEST: Move from rejected back to approved ===")
        
        # Setup: Admin rejects driver
        self.test_08_admin_reject_driver()
        
        # Get admin token and driver
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Admin re-approves the rejected driver via PATCH /users/{id}/
        approve_response = self.client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'approved'
        })
        print(f"Re-approve driver response: {approve_response.status_code}")
        
        self.assertEqual(approve_response.status_code, status.HTTP_200_OK)
        
        # Verify driver is approved and active again
        driver_user.refresh_from_db()
        self.assertTrue(driver_user.is_approved)
        self.assertTrue(driver_user.is_active)

    def test_10_admin_deactivate_user(self):
        """Test admin deactivating a user"""
        print("\n=== TEST: Admin deactivate user ===")
        
        # Setup: Get approved driver
        self.test_09_move_from_rejected_to_approved()
        
        # Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Admin deactivates driver using PATCH /users/{id}/
        deactivate_response = self.client.patch(f'/api/users/{driver_user.id}/', {
            'is_active': False
        })
        print(f"Deactivate user response: {deactivate_response.status_code}")
        
        self.assertEqual(deactivate_response.status_code, status.HTTP_200_OK)
        
        # Verify user is deactivated
        driver_user.refresh_from_db()
        self.assertFalse(driver_user.is_active)

    def test_11_admin_reactivate_user(self):
        """Test admin reactivating a deactivated user"""
        print("\n=== TEST: Admin reactivate user ===")
        
        # Setup: Admin deactivates user
        self.test_10_admin_deactivate_user()
        
        # Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Admin reactivates driver
        reactivate_response = self.client.patch(f'/api/users/{driver_user.id}/', {
            'is_active': True
        })
        print(f"Reactivate user response: {reactivate_response.status_code}")
        
        self.assertEqual(reactivate_response.status_code, status.HTTP_200_OK)
        
        # Verify user is reactivated
        driver_user.refresh_from_db()
        self.assertTrue(driver_user.is_active)

    def test_12_get_driver_details_normal_user_not_profile(self):
        """Test getting driver user details (normal user data, not profile)"""
        print("\n=== TEST: Get driver details (user data, not profile) ===")
        
        # Setup: Get approved driver  
        driver_token = self.test_07_approved_driver_can_login()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {driver_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Driver gets their own user details
        response = self.client.get(f'/api/users/{driver_user.id}/')
        print(f"Get driver details response: {response.status_code}")
        print(f"Driver details data: {response.data}")
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['email'], self.driver_data['email'])
        self.assertEqual(response.data['user_type'], 'driver')
        
        # Should include profile data nested (but this is user data, not separate profile)
        self.assertIn('profile', response.data)

    def test_13_delete_deactivated_user(self):
        """Test deleting a deactivated user (should succeed)"""
        print("\n=== TEST: Delete deactivated user ===")
        
        # Setup: Admin deactivates user
        self.test_10_admin_deactivate_user()
        
        # Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Admin deletes deactivated user
        delete_response = self.client.delete(f'/api/users/{driver_user.id}/')
        print(f"Delete deactivated user response: {delete_response.status_code}")
        
        self.assertEqual(delete_response.status_code, status.HTTP_200_OK)
        
        # Verify user is deleted
        with self.assertRaises(CustomUser.DoesNotExist):
            CustomUser.objects.get(id=driver_user.id)

    def test_14_cannot_delete_active_approved_user(self):
        """Test that you cannot delete an active, approved user"""
        print("\n=== TEST: Cannot delete active approved user ===")
        
        # Setup: Get approved, active driver
        self.test_09_move_from_rejected_to_approved()
        
        # Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        driver_user = CustomUser.objects.get(email=self.driver_data['email'])
        
        # Attempt to delete active approved user (should fail)
        delete_response = self.client.delete(f'/api/users/{driver_user.id}/')
        print(f"Delete active user response: {delete_response.status_code}")
        print(f"Response data: {delete_response.data}")
        
        self.assertEqual(delete_response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Cannot delete active and approved users', delete_response.data['error'])

    def test_15_admin_list_users_with_filters(self):
        """Test admin listing users with status and type filters"""
        print("\n=== TEST: Admin list users with filters ===")
        
        # Setup: Create various user states
        self.test_09_move_from_rejected_to_approved()  # Creates approved driver
        
        # Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        # Test listing all users
        all_users_response = self.client.get('/api/users/')
        print(f"All users count: {len(all_users_response.data['results'])}")
        self.assertEqual(all_users_response.status_code, status.HTTP_200_OK)
        
        # Test filtering by user_type
        drivers_response = self.client.get('/users/?user_type=driver')
        print(f"Drivers count: {len(drivers_response.data['results'])}")
        self.assertEqual(drivers_response.status_code, status.HTTP_200_OK)
        
        # Test filtering by status
        approved_response = self.client.get('/users/?status=approved')
        print(f"Approved users count: {len(approved_response.data['results'])}")
        self.assertEqual(approved_response.status_code, status.HTTP_200_OK)

    def test_16_admin_create_another_admin(self):
        """Test admin creating another admin user"""
        print("\n=== TEST: Admin create another admin user ===")
        
        # Setup: Get admin token
        admin_token = self.test_02_admin_login_and_get_info()
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        
        # Admin creates another admin
        create_admin_response = self.client.post('/api/users/', {
            'email': 'newadmin+admin@example.com',
            'password': 'testpass123',
            'first_name': 'New',
            'last_name': 'Admin',
            'user_type': 'admin'
        })
        print(f"Create admin response: {create_admin_response.status_code}")
        
        self.assertEqual(create_admin_response.status_code, status.HTTP_201_CREATED)
        
        # Verify new admin was created and auto-approved
        new_admin = CustomUser.objects.get(email='newadmin+admin@example.com')
        self.assertEqual(new_admin.user_type, 'admin')
        self.assertTrue(new_admin.is_approved)

    def test_17_run_all_scenarios_in_sequence(self):
        """Run all test scenarios in the correct sequence"""
        print("\n" + "="*60)
        print("RUNNING COMPLETE AUTHENTICATION AND USER MANAGEMENT TEST SUITE")
        print("="*60)
        
        # This test runs all the individual tests in sequence
        # to verify the complete user lifecycle works end-to-end
        
        # 1. Register first admin
        admin_user = self.test_01_register_first_admin_public_endpoint()
        
        # 2. Admin login and get info  
        admin_token = self.test_02_admin_login_and_get_info()
        
        # 3. Register driver (pending)
        driver_user = self.test_03_register_driver_via_public_endpoint()
        
        # 4. Verify unapproved driver can't login
        self.test_04_driver_login_should_fail_not_approved()
        
        # 5. Verify second admin registration fails via public endpoint
        self.test_05_register_second_admin_public_should_fail()
        
        # 6. Admin approves driver
        self.test_06_admin_approve_driver()
        
        # 7. Approved driver can login
        driver_token = self.test_07_approved_driver_can_login()
        
        # 8. Admin rejects driver
        self.test_08_admin_reject_driver()
        
        # 9. Move from rejected back to approved
        self.test_09_move_from_rejected_to_approved()
        
        # 10. Admin deactivates user
        self.test_10_admin_deactivate_user()
        
        # 11. Admin reactivates user  
        self.test_11_admin_reactivate_user()
        
        # 12. Get driver details
        self.test_12_get_driver_details_normal_user_not_profile()
        
        # 13. Test filtering and listing
        self.test_15_admin_list_users_with_filters()
        
        # 14. Admin creates another admin
        self.test_16_admin_create_another_admin()
        
        print("\n" + "="*60)
        print("ALL TESTS PASSED! 🎉")
        print("Authentication and user management working correctly.")
        print("="*60)


# Run tests individually if needed
class IndividualTestCases(TestCase):
    """Individual test cases that can be run separately"""
    
    def setUp(self):
        self.client = APIClient()
        CustomUser.objects.all().delete()

    def test_register_admin(self):
        """Quick test for admin registration"""
        response = self.client.post('/auth/register/', {
            'email': 'quickadmin+admin@example.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123'
        })
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)

    def test_register_driver(self):  
        """Quick test for driver registration"""
        response = self.client.post('/auth/register/', {
            'email': 'quickdriver+driver@example.com',
            'password': 'testpass123', 
            'password_confirm': 'testpass123'
        })
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)