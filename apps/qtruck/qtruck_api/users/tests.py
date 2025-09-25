"""
Comprehensive test suite for the updated user management system.

Test scenarios covered:
1. CustomUser model functionality with new status system
2. Email validation and user type extraction from email aliases
3. Status transitions (preapproval -> approved -> rejected -> preapproval)
4. Auto-approval settings functionality
5. Admin-only permissions for status changes and activation/deactivation
6. Prevention of admin lockout scenarios
7. RESTful UserViewSet CRUD operations
8. Full_name property functionality
9. Email uniqueness and USERNAME_FIELD validation
10. Backward compatibility with is_approved property
"""

import json
from django.test import TestCase
from django.core.exceptions import ValidationError
from rest_framework.test import APIClient
from rest_framework import status
from django.contrib.auth import get_user_model
from users.models import CustomUser, UserProfile
from settings.models import SystemSettings

CustomUser = get_user_model()


class CustomUserModelTestCase(TestCase):
    """Test CustomUser model functionality"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        SystemSettings.objects.all().delete()

    def test_email_alias_validation_admin(self):
        """Test email alias validation for admin user type"""
        user_data = {
            'email': 'testuser+admin@example.com',
            'password': 'testpass123',
            'first_name': 'Test',
            'last_name': 'User'
        }
        
        user = CustomUser.objects.create_user(**user_data)
        
        self.assertEqual(user.user_type, 'admin')
        self.assertEqual(user.status, 'approved')  # First user becomes admin and approved
        self.assertTrue(user.is_staff)
        self.assertTrue(user.is_superuser)

    def test_email_alias_validation_driver(self):
        """Test email alias validation for driver user type"""
        # Create first admin user
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        user_data = {
            'email': 'testuser+driver@example.com',
            'password': 'testpass123',
            'first_name': 'Test',
            'last_name': 'Driver'
        }
        
        user = CustomUser.objects.create_user(**user_data)
        
        self.assertEqual(user.user_type, 'driver')
        self.assertEqual(user.status, 'preapproval')  # Default status
        self.assertFalse(user.is_staff)
        self.assertFalse(user.is_superuser)

    def test_email_alias_validation_tester(self):
        """Test email alias validation for tester user type"""
        # Create first admin user
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        user_data = {
            'email': 'testuser+tester@example.com',
            'password': 'testpass123'
        }
        
        user = CustomUser.objects.create_user(**user_data)
        
        self.assertEqual(user.user_type, 'tester')
        self.assertEqual(user.status, 'preapproval')

    def test_email_without_alias_raises_validation_error(self):
        """Test that email without alias raises ValidationError"""
        with self.assertRaises(ValidationError):
            user = CustomUser(
                email='testuser@example.com',  # No alias
                password='testpass123'
            )
            user.save()

    def test_email_with_invalid_alias_raises_validation_error(self):
        """Test that email with invalid alias raises ValidationError"""
        with self.assertRaises(ValidationError):
            user = CustomUser(
                email='testuser+invalid@example.com',  # Invalid alias
                password='testpass123'
            )
            user.save()

    def test_full_name_property_with_names(self):
        """Test full_name property when first and last names are provided"""
        user = CustomUser.objects.create_user(
            email='testuser+admin@example.com',
            password='testpass123',
            first_name='John',
            last_name='Doe'
        )
        
        self.assertEqual(user.full_name, 'John Doe')

    def test_full_name_property_fallback_to_email(self):
        """Test full_name property fallback to email base when no names provided"""
        user = CustomUser.objects.create_user(
            email='johndoe+admin@example.com',
            password='testpass123'
        )
        
        self.assertEqual(user.full_name, 'johndoe')

    def test_base_email_property(self):
        """Test base_email property strips alias from email"""
        user = CustomUser.objects.create_user(
            email='testuser+admin@example.com',
            password='testpass123'
        )
        
        self.assertEqual(user.base_email, 'testuser@example.com')

    def test_is_approved_backward_compatibility_property(self):
        """Test is_approved property for backward compatibility"""
        user = CustomUser.objects.create_user(
            email='testuser+admin@example.com',
            password='testpass123'
        )
        
        # First user is auto-approved
        self.assertTrue(user.is_approved)
        
        # Change status to preapproval
        user.status = 'preapproval'
        user.save()
        self.assertFalse(user.is_approved)
        
        # Change status to rejected
        user.status = 'rejected'
        user.save()
        self.assertFalse(user.is_approved)

    def test_email_unique_constraint(self):
        """Test email unique constraint"""
        # Create first user
        user1 = CustomUser.objects.create_user(
            email='testuser+admin@example.com',
            password='testpass123'
        )
        
        # Try to create another user with same email
        with self.assertRaises(Exception):  # Should raise IntegrityError
            user2 = CustomUser.objects.create_user(
                email='testuser+admin@example.com',
                password='differentpass'
            )

    def test_username_field_is_email(self):
        """Test that USERNAME_FIELD is set to email"""
        self.assertEqual(CustomUser.USERNAME_FIELD, 'email')

    def test_auto_approval_settings_integration(self):
        """Test integration with auto-approval settings"""
        # Create settings with auto-approve for testers
        settings = SystemSettings.objects.create(
            auto_approve_user_types=['tester']
        )
        
        # Create first admin
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        # Create tester - should be auto-approved
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123'
        )
        
        self.assertEqual(tester.status, 'approved')
        
        # Create driver - should not be auto-approved
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        
        self.assertEqual(driver.status, 'preapproval')


class StatusTransitionTestCase(TestCase):
    """Test status transition logic and validation"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        self.client = APIClient()
        
        # Create admin user
        self.admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='testpass123',
            first_name='Test',
            last_name='Admin'
        )
        
        # Login admin
        login_response = self.client.post('/auth/login/', {
            'email': 'admin+admin@example.com',
            'password': 'testpass123'
        })
        self.admin_token = login_response.data['access']
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {self.admin_token}')
        
        # Create test driver
        self.driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            first_name='Test',
            last_name='Driver'
        )

    def test_preapproval_to_approved_transition(self):
        """Test transition from preapproval to approved"""
        self.assertEqual(self.driver.status, 'preapproval')
        
        response = self.client.patch(f'/users/{self.driver.id}/', {
            'status': 'approved'
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.driver.refresh_from_db()
        self.assertEqual(self.driver.status, 'approved')

    def test_preapproval_to_rejected_transition(self):
        """Test transition from preapproval to rejected"""
        self.assertEqual(self.driver.status, 'preapproval')
        
        response = self.client.patch(f'/users/{self.driver.id}/', {
            'status': 'rejected'
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.driver.refresh_from_db()
        self.assertEqual(self.driver.status, 'rejected')

    def test_approved_to_rejected_transition(self):
        """Test transition from approved to rejected"""
        # First approve the driver
        self.driver.status = 'approved'
        self.driver.save()
        
        response = self.client.patch(f'/users/{self.driver.id}/', {
            'status': 'rejected'
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.driver.refresh_from_db()
        self.assertEqual(self.driver.status, 'rejected')

    def test_rejected_to_preapproval_transition(self):
        """Test transition from rejected back to preapproval"""
        # First reject the driver
        self.driver.status = 'rejected'
        self.driver.save()
        
        response = self.client.patch(f'/users/{self.driver.id}/', {
            'status': 'preapproval'
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.driver.refresh_from_db()
        self.assertEqual(self.driver.status, 'preapproval')

    def test_invalid_transition_approved_to_preapproval(self):
        """Test invalid transition from approved directly to preapproval"""
        # First approve the driver
        self.driver.status = 'approved'
        self.driver.save()
        
        response = self.client.patch(f'/users/{self.driver.id}/', {
            'status': 'preapproval'
        })
        
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Invalid status transition', response.data['error'])

    def test_non_admin_cannot_change_status(self):
        """Test that non-admin users cannot change status"""
        # Login as driver
        driver_login = self.client.post('/auth/login/', {
            'email': 'driver+driver@example.com',
            'password': 'testpass123'
        })
        
        # This will fail because driver is not approved, but let's create an approved driver
        approved_driver = CustomUser.objects.create_user(
            email='approved+driver@example.com',
            password='testpass123',
            status='approved'
        )
        
        driver_login = self.client.post('/auth/login/', {
            'email': 'approved+driver@example.com',
            'password': 'testpass123'
        })
        
        if driver_login.status_code == status.HTTP_200_OK:
            self.client.credentials(HTTP_AUTHORIZATION=f"Bearer {driver_login.data['access']}")
            
            # Try to change another user's status
            response = self.client.patch(f'/users/{self.driver.id}/', {
                'status': 'approved'
            })
            
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)


class AdminPermissionsTestCase(TestCase):
    """Test admin-only permissions and lockout prevention"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        self.client = APIClient()
        
        # Create admin user
        self.admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='testpass123'
        )
        
        # Login admin
        login_response = self.client.post('/auth/login/', {
            'email': 'admin+admin@example.com',
            'password': 'testpass123'
        })
        self.admin_token = login_response.data['access']
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {self.admin_token}')

    def test_only_admin_can_activate_deactivate_users(self):
        """Test that only admins can activate/deactivate users"""
        # Create test driver
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            status='approved'  # Make it approved so they can login
        )
        
        # Admin can deactivate
        response = self.client.patch(f'/users/{driver.id}/', {
            'is_active': False
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertFalse(driver.is_active)
        
        # Admin can reactivate
        response = self.client.patch(f'/users/{driver.id}/', {
            'is_active': True
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertTrue(driver.is_active)

    def test_cannot_deactivate_last_admin(self):
        """Test prevention of deactivating the last admin"""
        # Try to deactivate the only admin
        response = self.client.patch(f'/users/{self.admin.id}/', {
            'is_active': False
        })
        
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Cannot deactivate the last admin user', response.data['error'])

    def test_can_deactivate_admin_when_multiple_exist(self):
        """Test that admin can be deactivated when multiple admins exist"""
        # Create second admin
        second_admin = CustomUser.objects.create_user(
            email='admin2+admin@example.com',
            password='testpass123',
            status='approved'
        )
        
        # Now can deactivate first admin
        response = self.client.patch(f'/users/{self.admin.id}/', {
            'is_active': False
        })
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.admin.refresh_from_db()
        self.assertFalse(self.admin.is_active)

    def test_cannot_delete_last_admin(self):
        """Test prevention of deleting the last admin"""
        # First deactivate admin (this should fail because it's the last one)
        response = self.client.patch(f'/users/{self.admin.id}/', {
            'is_active': False
        })
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        
        # Create second admin and approve it
        second_admin = CustomUser.objects.create_user(
            email='admin2+admin@example.com',
            password='testpass123',
            status='approved'
        )
        
        # Now deactivate first admin
        response = self.client.patch(f'/users/{self.admin.id}/', {
            'is_active': False
        })
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Try to delete the deactivated admin - but it's still the last admin by count
        # So this should still fail
        response = self.client.delete(f'/users/{self.admin.id}/')
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Cannot delete the last admin user', response.data['error'])

    def test_cannot_delete_active_approved_user(self):
        """Test that active approved users cannot be deleted"""
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            status='approved'
        )
        
        response = self.client.delete(f'/users/{driver.id}/')
        
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Cannot delete active and approved users', response.data['error'])

    def test_can_delete_deactivated_user(self):
        """Test that deactivated users can be deleted"""
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            status='approved'
        )
        
        # First deactivate
        response = self.client.patch(f'/users/{driver.id}/', {
            'is_active': False
        })
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Then delete
        response = self.client.delete(f'/users/{driver.id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Verify deleted
        with self.assertRaises(CustomUser.DoesNotExist):
            CustomUser.objects.get(id=driver.id)

    def test_can_delete_rejected_user(self):
        """Test that rejected users can be deleted"""
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            status='rejected'
        )
        
        response = self.client.delete(f'/users/{driver.id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Verify deleted
        with self.assertRaises(CustomUser.DoesNotExist):
            CustomUser.objects.get(id=driver.id)


class UserViewSetTestCase(TestCase):
    """Test UserViewSet CRUD operations"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        self.client = APIClient()
        
        # Create admin user
        self.admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='testpass123'
        )
        
        # Login admin
        login_response = self.client.post('/auth/login/', {
            'email': 'admin+admin@example.com',
            'password': 'testpass123'
        })
        self.admin_token = login_response.data['access']
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {self.admin_token}')

    def test_list_users_with_status_filter(self):
        """Test listing users with status filter"""
        # Create users with different statuses
        driver1 = CustomUser.objects.create_user(
            email='driver1+driver@example.com',
            password='pass123',
            status='preapproval'
        )
        
        driver2 = CustomUser.objects.create_user(
            email='driver2+driver@example.com',
            password='pass123',
            status='approved'
        )
        
        driver3 = CustomUser.objects.create_user(
            email='driver3+driver@example.com',
            password='pass123',
            status='rejected'
        )
        
        # Test filtering by status
        response = self.client.get('/users/?status=preapproval')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find driver1
        self.assertTrue(any(user['id'] == str(driver1.id) for user in response.data['results']))
        
        response = self.client.get('/users/?status=approved')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find admin and driver2
        approved_ids = [user['id'] for user in response.data['results']]
        self.assertIn(str(driver2.id), approved_ids)
        self.assertIn(str(self.admin.id), approved_ids)
        
        response = self.client.get('/users/?status=rejected')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find driver3
        self.assertTrue(any(user['id'] == str(driver3.id) for user in response.data['results']))

    def test_list_users_with_user_type_filter(self):
        """Test listing users with user_type filter"""
        # Create users with different types
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123'
        )
        
        # Test filtering by user_type
        response = self.client.get('/users/?user_type=admin')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find only admin
        self.assertEqual(len(response.data['results']), 1)
        self.assertEqual(response.data['results'][0]['user_type'], 'admin')
        
        response = self.client.get('/users/?user_type=driver')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find only driver
        self.assertTrue(any(user['id'] == str(driver.id) for user in response.data['results']))
        
        response = self.client.get('/users/?user_type=tester')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should find only tester
        self.assertTrue(any(user['id'] == str(tester.id) for user in response.data['results']))

    def test_get_user_me_endpoint(self):
        """Test /users/me/ endpoint"""
        response = self.client.get('/users/me/')
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['email'], 'admin+admin@example.com')
        self.assertEqual(response.data['user_type'], 'admin')
        self.assertEqual(response.data['status'], 'approved')
        self.assertIn('full_name', response.data)

    def test_serializer_includes_all_required_fields(self):
        """Test that serializer includes all required fields"""
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123',
            first_name='John',
            last_name='Doe'
        )
        
        response = self.client.get(f'/users/{driver.id}/')
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        required_fields = [
            'id', 'email', 'base_email', 'first_name', 'last_name', 'full_name',
            'user_type', 'status', 'is_active', 'created_at', 'updated_at'
        ]
        
        for field in required_fields:
            self.assertIn(field, response.data, f"Field '{field}' missing from serializer")
        
        # Check that username and is_approved are NOT in the response
        self.assertNotIn('username', response.data)
        self.assertNotIn('is_approved', response.data)
        
        # Check full_name property works correctly
        self.assertEqual(response.data['full_name'], 'John Doe')

    def test_non_admin_user_can_only_see_own_record(self):
        """Test that non-admin users can only see their own user record"""
        # Create and approve a driver
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123',
            status='approved'
        )
        
        # Login as driver
        driver_login = self.client.post('/auth/login/', {
            'email': 'driver+driver@example.com',
            'password': 'pass123'
        })
        
        self.client.credentials(HTTP_AUTHORIZATION=f"Bearer {driver_login.data['access']}")
        
        # Driver should only see their own record in list
        response = self.client.get('/users/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 1)
        self.assertEqual(response.data['results'][0]['id'], str(driver.id))
        
        # Driver should not be able to access admin's record
        response = self.client.get(f'/users/{self.admin.id}/')
        self.assertEqual(response.status_code, status.HTTP_404_NOT_FOUND)


class AutoApprovalSettingsTestCase(TestCase):
    """Test auto-approval settings functionality"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        SystemSettings.objects.all().delete()

    def test_auto_approve_settings_for_testers(self):
        """Test auto-approval settings for tester user type"""
        # Create settings to auto-approve testers
        settings = SystemSettings.objects.create(
            auto_approve_user_types=['tester']
        )
        
        # Create first admin
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        # Create tester - should be auto-approved
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123'
        )
        
        self.assertEqual(tester.status, 'approved')
        
        # Create driver - should NOT be auto-approved
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        
        self.assertEqual(driver.status, 'preapproval')

    def test_auto_approve_settings_for_multiple_types(self):
        """Test auto-approval settings for multiple user types"""
        # Create settings to auto-approve both testers and drivers
        settings = SystemSettings.objects.create(
            auto_approve_user_types=['tester', 'driver']
        )
        
        # Create first admin
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        # Both should be auto-approved
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123'
        )
        self.assertEqual(tester.status, 'approved')
        
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        self.assertEqual(driver.status, 'approved')

    def test_auto_approve_settings_empty_list(self):
        """Test auto-approval with empty settings list"""
        # Create settings with empty auto-approve list
        settings = SystemSettings.objects.create(
            auto_approve_user_types=[]
        )
        
        # Create first admin
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        
        # Only admin should be auto-approved (because it's the first user)
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        
        self.assertEqual(driver.status, 'preapproval')

    def test_auto_approve_settings_fallback_when_no_settings(self):
        """Test fallback behavior when no SystemSettings exist"""
        # Don't create any SystemSettings
        
        # Create first admin
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        self.assertEqual(admin.status, 'approved')  # First user always approved
        
        # Create another admin - should be auto-approved due to fallback logic
        admin2 = CustomUser.objects.create_user(
            email='admin2+admin@example.com',
            password='pass123'
        )
        self.assertEqual(admin2.status, 'approved')  # Admin fallback
        
        # Create driver - should not be auto-approved
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        self.assertEqual(driver.status, 'preapproval')


class IntegrationTestCase(TestCase):
    """Integration tests for complete user lifecycle scenarios"""

    def setUp(self):
        """Set up test environment"""
        CustomUser.objects.all().delete()
        SystemSettings.objects.all().delete()
        self.client = APIClient()

    def test_complete_user_lifecycle_scenario(self):
        """Test complete user lifecycle from registration to deletion"""
        print("\n=== INTEGRATION TEST: Complete User Lifecycle ===")
        
        # 1. Register first admin
        print("1. Creating first admin user...")
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='testpass123',
            first_name='Test',
            last_name='Admin'
        )
        self.assertEqual(admin.status, 'approved')
        self.assertEqual(admin.user_type, 'admin')
        print(f"    Admin created: {admin.full_name} ({admin.status})")
        
        # 2. Login admin
        print("2. Admin login...")
        login_response = self.client.post('/auth/login/', {
            'email': 'admin+admin@example.com',
            'password': 'testpass123'
        })
        self.assertEqual(login_response.status_code, status.HTTP_200_OK)
        admin_token = login_response.data['access']
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        print("    Admin logged in successfully")
        
        # 3. Create driver (pending approval)
        print("3. Creating driver user...")
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='testpass123',
            first_name='Test',
            last_name='Driver'
        )
        self.assertEqual(driver.status, 'preapproval')
        print(f"    Driver created: {driver.full_name} ({driver.status})")
        
        # 4. Admin approves driver
        print("4. Admin approving driver...")
        approve_response = self.client.patch(f'/users/{driver.id}/', {
            'status': 'approved'
        })
        self.assertEqual(approve_response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertEqual(driver.status, 'approved')
        print(f"    Driver approved: {driver.status}")
        
        # 5. Driver can now login
        print("5. Driver login...")
        self.client.credentials()  # Clear admin credentials
        driver_login = self.client.post('/auth/login/', {
            'email': 'driver+driver@example.com',
            'password': 'testpass123'
        })
        self.assertEqual(driver_login.status_code, status.HTTP_200_OK)
        print("    Driver logged in successfully")
        
        # 6. Admin rejects driver
        print("6. Admin rejecting driver...")
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        reject_response = self.client.patch(f'/users/{driver.id}/', {
            'status': 'rejected'
        })
        self.assertEqual(reject_response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertEqual(driver.status, 'rejected')
        print(f"    Driver rejected: {driver.status}")
        
        # 7. Move driver from rejected back to preapproval
        print("7. Moving driver from rejected to preapproval...")
        back_to_pending_response = self.client.patch(f'/users/{driver.id}/', {
            'status': 'preapproval'
        })
        self.assertEqual(back_to_pending_response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertEqual(driver.status, 'preapproval')
        print(f"    Driver moved back to: {driver.status}")
        
        # 8. Approve and then deactivate driver
        print("8. Approve and deactivate driver...")
        self.client.patch(f'/users/{driver.id}/', {'status': 'approved'})
        deactivate_response = self.client.patch(f'/users/{driver.id}/', {
            'is_active': False
        })
        self.assertEqual(deactivate_response.status_code, status.HTTP_200_OK)
        driver.refresh_from_db()
        self.assertFalse(driver.is_active)
        print("    Driver deactivated")
        
        # 9. Delete deactivated driver
        print("9. Deleting deactivated driver...")
        delete_response = self.client.delete(f'/users/{driver.id}/')
        self.assertEqual(delete_response.status_code, status.HTTP_200_OK)
        print("    Driver deleted")
        
        # 10. Verify driver is gone
        with self.assertRaises(CustomUser.DoesNotExist):
            CustomUser.objects.get(id=driver.id)
        print("    Driver deletion verified")
        
        print("\n=== INTEGRATION TEST PASSED ===\n")

    def test_auto_approval_integration_scenario(self):
        """Test auto-approval settings integration scenario"""
        print("\n=== INTEGRATION TEST: Auto-Approval Settings ===")
        
        # 1. Create settings to auto-approve testers
        print("1. Setting up auto-approval for testers...")
        settings = SystemSettings.objects.create(
            auto_approve_user_types=['tester']
        )
        print("    Auto-approval configured for testers")
        
        # 2. Create admin
        print("2. Creating admin...")
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        print(f"    Admin created: {admin.status}")
        
        # 3. Create tester (should be auto-approved)
        print("3. Creating tester (should auto-approve)...")
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123',
            first_name='Auto',
            last_name='Tester'
        )
        self.assertEqual(tester.status, 'approved')
        print(f"    Tester created: {tester.full_name} ({tester.status})")
        
        # 4. Create driver (should not be auto-approved)
        print("4. Creating driver (should not auto-approve)...")
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        self.assertEqual(driver.status, 'preapproval')
        print(f"    Driver created: {driver.status}")
        
        # 5. Tester can login immediately
        print("5. Testing tester login...")
        tester_login = self.client.post('/auth/login/', {
            'email': 'tester+tester@example.com',
            'password': 'pass123'
        })
        self.assertEqual(tester_login.status_code, status.HTTP_200_OK)
        print("    Tester login successful")
        
        # 6. Driver cannot login (not approved)
        print("6. Testing driver login (should fail)...")
        driver_login = self.client.post('/auth/login/', {
            'email': 'driver+driver@example.com',
            'password': 'pass123'
        })
        self.assertEqual(driver_login.status_code, status.HTTP_401_UNAUTHORIZED)
        print("    Driver login correctly failed (not approved)")
        
        print("\n=== AUTO-APPROVAL INTEGRATION TEST PASSED ===\n")


if __name__ == '__main__':
    import django
    from django.conf import settings
    from django.test.utils import get_runner
    
    if not settings.configured:
        settings.configure(
            DEBUG=True,
            DATABASES={
                'default': {
                    'ENGINE': 'django.db.backends.sqlite3',
                    'NAME': ':memory:',
                }
            },
            INSTALLED_APPS=[
                'django.contrib.auth',
                'django.contrib.contenttypes',
                'rest_framework',
                'users',
                'settings',
            ],
        )
    
    django.setup()
    TestRunner = get_runner(settings)
    test_runner = TestRunner()
    failures = test_runner.run_tests(["users.tests"])