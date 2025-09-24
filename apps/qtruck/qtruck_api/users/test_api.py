"""
Comprehensive API tests for user management using pytest and fixtures.

These tests use actual HTTP API calls instead of direct model manipulation
to ensure proper testing of authentication, permissions, and business logic.
"""
import pytest
from django.urls import reverse
from users.models import CustomUser


@pytest.mark.api
class TestUserRegistrationAPI:
    """Test user registration through the API"""

    def test_admin_registration_first_user(self, api_client, db):
        """Test that first user with admin email gets auto-approved as admin"""
        # Ensure no users exist
        assert CustomUser.objects.count() == 0
        
        response = api_client.post('/auth/register/', {
            'email': 'admin+admin@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Admin',
            'last_name': 'User'
        })
        
        assert response.status_code == 201
        
        # Verify first user is admin and approved
        user = CustomUser.objects.get(email='admin+admin@test.com')
        assert user.user_type == 'admin'
        assert user.status == 'approved'
        assert user.is_staff == True
        assert user.is_superuser == True

    def test_driver_registration_pending_approval(self, api_client, admin_user, db):
        """Test that driver registration requires approval"""
        response = api_client.post('/auth/register/', {
            'email': 'driver+driver@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Driver',
            'last_name': 'User'
        })
        
        assert response.status_code == 201
        
        # Verify driver needs approval
        user = CustomUser.objects.get(email='driver+driver@test.com')
        assert user.user_type == 'driver'
        assert user.status == 'preapproval'
        assert user.is_staff == False
        assert user.is_superuser == False

    def test_tester_registration_pending_approval(self, api_client, admin_user, db):
        """Test that tester registration requires approval by default"""
        response = api_client.post('/auth/register/', {
            'email': 'tester+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Tester',
            'last_name': 'User'
        })
        
        assert response.status_code == 201
        
        # Verify tester needs approval
        user = CustomUser.objects.get(email='tester+tester@test.com')
        assert user.user_type == 'tester'
        assert user.status == 'preapproval'

    def test_tester_auto_approval_with_settings(self, api_client, admin_user, auto_approve_testers_settings):
        """Test that testers get auto-approved when configured in settings"""
        response = api_client.post('/auth/register/', {
            'email': 'tester+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Tester',
            'last_name': 'User'
        })
        
        assert response.status_code == 201
        
        # Verify tester is auto-approved
        user = CustomUser.objects.get(email='tester+tester@test.com')
        assert user.user_type == 'tester'
        assert user.status == 'approved'


@pytest.mark.api
class TestUserAuthenticationAPI:
    """Test user authentication through the API"""

    def test_approved_user_can_login(self, api_client, approved_driver_user):
        """Test that approved users can login successfully"""
        response = api_client.post('/auth/login/', {
            'email': approved_driver_user.email,
            'password': 'testpass123'
        })
        
        assert response.status_code == 200
        assert 'access' in response.data
        assert 'refresh' in response.data

    def test_pending_user_cannot_login(self, api_client, driver_user):
        """Test that pending approval users cannot login"""
        response = api_client.post('/auth/login/', {
            'email': driver_user.email,
            'password': 'testpass123'
        })
        
        assert response.status_code == 401

    def test_rejected_user_cannot_login(self, api_client, driver_user, admin_client):
        """Test that rejected users cannot login"""
        # First reject the user via admin
        admin_client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'rejected'
        })
        
        # Try to login as rejected user
        response = api_client.post('/auth/login/', {
            'email': driver_user.email,
            'password': 'testpass123'
        })
        
        assert response.status_code == 401

    def test_inactive_user_cannot_login(self, api_client, approved_driver_user, admin_client):
        """Test that inactive users cannot login"""
        # First deactivate the user via admin
        admin_client.patch(f'/api/users/{approved_driver_user.id}/', {
            'is_active': False
        })
        
        # Try to login as inactive user
        response = api_client.post('/auth/login/', {
            'email': approved_driver_user.email,
            'password': 'testpass123'
        })
        
        assert response.status_code == 401


@pytest.mark.api
class TestUserManagementAPI:
    """Test user management operations through the API"""

    def test_admin_can_list_users(self, admin_client, approved_driver_user, approved_tester_user):
        """Test that admins can list all users"""
        response = admin_client.get('/api/users/')
        
        assert response.status_code == 200
        assert len(response.data['results']) >= 3  # admin + driver + tester

    def test_non_admin_cannot_list_users(self, driver_client):
        """Test that non-admins cannot list users"""
        response = driver_client.get('/api/users/')
        
        assert response.status_code == 403

    def test_admin_can_view_user_details(self, admin_client, approved_driver_user):
        """Test that admins can view user details"""
        response = admin_client.get(f'/api/users/{approved_driver_user.id}/')
        
        assert response.status_code == 200
        assert response.data['email'] == approved_driver_user.email
        assert response.data['status'] == 'approved'

    def test_user_can_view_own_details(self, driver_client):
        """Test that users can view their own details"""
        response = driver_client.get(f'/api/users/{driver_client.user.id}/')
        
        assert response.status_code == 200
        assert response.data['email'] == driver_client.user.email

    def test_user_cannot_view_other_user_details(self, driver_client, approved_tester_user):
        """Test that users cannot view other users' details"""
        response = driver_client.get(f'/api/users/{approved_tester_user.id}/')
        
        assert response.status_code == 403


@pytest.mark.api
class TestUserApprovalWorkflowAPI:
    """Test user approval workflow through the API"""

    def test_admin_can_approve_pending_user(self, admin_client, driver_user):
        """Test that admins can approve pending users"""
        assert driver_user.status == 'preapproval'
        
        response = admin_client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'approved'
        })
        
        assert response.status_code == 200
        
        # Verify user is approved
        driver_user.refresh_from_db()
        assert driver_user.status == 'approved'

    def test_admin_can_reject_pending_user(self, admin_client, driver_user):
        """Test that admins can reject pending users"""
        response = admin_client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'rejected'
        })
        
        assert response.status_code == 200
        
        # Verify user is rejected
        driver_user.refresh_from_db()
        assert driver_user.status == 'rejected'

    def test_admin_can_move_rejected_to_preapproval(self, admin_client, driver_user):
        """Test that admins can move rejected users back to preapproval"""
        # First reject the user
        driver_user.status = 'rejected'
        driver_user.save()
        
        response = admin_client.patch(f'/api/users/{driver_user.id}/', {
            'status': 'preapproval'
        })
        
        assert response.status_code == 200
        
        # Verify user is back to preapproval
        driver_user.refresh_from_db()
        assert driver_user.status == 'preapproval'

    def test_admin_cannot_disapprove_approved_user(self, admin_client, approved_driver_user):
        """Test that approved users cannot be moved back to pending"""
        response = admin_client.patch(f'/api/users/{approved_driver_user.id}/', {
            'status': 'preapproval'
        })
        
        # Should either reject the request or keep status unchanged
        assert response.status_code in [400, 200]
        
        approved_driver_user.refresh_from_db()
        assert approved_driver_user.status == 'approved'

    def test_non_admin_cannot_change_user_status(self, driver_client, tester_user):
        """Test that non-admins cannot change user status"""
        response = driver_client.patch(f'/api/users/{tester_user.id}/', {
            'status': 'approved'
        })
        
        assert response.status_code == 403


@pytest.mark.api
class TestUserActivationAPI:
    """Test user activation/deactivation through the API"""

    def test_admin_can_deactivate_user(self, admin_client, approved_driver_user):
        """Test that admins can deactivate users"""
        response = admin_client.patch(f'/api/users/{approved_driver_user.id}/', {
            'is_active': False
        })
        
        assert response.status_code == 200
        
        # Verify user is deactivated
        approved_driver_user.refresh_from_db()
        assert approved_driver_user.is_active == False

    def test_admin_can_reactivate_user(self, admin_client, approved_driver_user):
        """Test that admins can reactivate users"""
        # First deactivate
        approved_driver_user.is_active = False
        approved_driver_user.save()
        
        response = admin_client.patch(f'/api/users/{approved_driver_user.id}/', {
            'is_active': True
        })
        
        assert response.status_code == 200
        
        # Verify user is reactivated
        approved_driver_user.refresh_from_db()
        assert approved_driver_user.is_active == True

    def test_non_admin_cannot_deactivate_user(self, driver_client, approved_tester_user):
        """Test that non-admins cannot deactivate users"""
        response = driver_client.patch(f'/api/users/{approved_tester_user.id}/', {
            'is_active': False
        })
        
        assert response.status_code == 403

    def test_user_cannot_deactivate_self(self, driver_client):
        """Test that users cannot deactivate themselves to prevent lockout"""
        response = driver_client.patch(f'/api/users/{driver_client.user.id}/', {
            'is_active': False
        })
        
        # Should be forbidden or ignored
        assert response.status_code in [403, 200]
        
        # If 200, verify is_active wasn't actually changed
        if response.status_code == 200:
            driver_client.user.refresh_from_db()
            assert driver_client.user.is_active == True


@pytest.mark.api
class TestUserDeletionAPI:
    """Test user deletion through the API"""

    def test_admin_can_delete_user(self, admin_client, driver_user):
        """Test that admins can delete users"""
        user_id = driver_user.id
        
        response = admin_client.delete(f'/api/users/{user_id}/')
        
        assert response.status_code == 204
        
        # Verify user is deleted
        assert not CustomUser.objects.filter(id=user_id).exists()

    def test_non_admin_cannot_delete_user(self, driver_client, tester_user):
        """Test that non-admins cannot delete users"""
        response = driver_client.delete(f'/api/users/{tester_user.id}/')
        
        assert response.status_code == 403

    def test_user_cannot_delete_self(self, driver_client):
        """Test that users cannot delete themselves"""
        response = driver_client.delete(f'/api/users/{driver_client.user.id}/')
        
        assert response.status_code == 403


@pytest.mark.api
class TestUserProfileUpdatesAPI:
    """Test user profile updates through the API"""

    def test_user_can_update_own_profile(self, driver_client):
        """Test that users can update their own profile"""
        response = driver_client.patch(f'/api/users/{driver_client.user.id}/', {
            'first_name': 'Updated',
            'last_name': 'Name'
        })
        
        assert response.status_code == 200
        
        # Verify profile was updated
        driver_client.user.refresh_from_db()
        assert driver_client.user.first_name == 'Updated'
        assert driver_client.user.last_name == 'Name'

    def test_user_cannot_update_sensitive_fields(self, driver_client):
        """Test that users cannot update sensitive fields"""
        original_user_type = driver_client.user.user_type
        original_status = driver_client.user.status
        
        response = driver_client.patch(f'/api/users/{driver_client.user.id}/', {
            'user_type': 'admin',
            'status': 'approved',
            'is_staff': True,
            'is_superuser': True
        })
        
        # Should either be forbidden or ignore sensitive fields
        assert response.status_code in [200, 403]
        
        # Verify sensitive fields weren't changed
        driver_client.user.refresh_from_db()
        assert driver_client.user.user_type == original_user_type
        assert driver_client.user.status == original_status
        assert driver_client.user.is_staff == False
        assert driver_client.user.is_superuser == False

    def test_full_name_property_works(self, driver_client):
        """Test that the full_name property works correctly"""
        response = driver_client.get(f'/api/users/{driver_client.user.id}/')
        
        assert response.status_code == 200
        expected_full_name = f"{driver_client.user.first_name} {driver_client.user.last_name}"
        assert response.data.get('full_name') == expected_full_name


@pytest.mark.api
@pytest.mark.integration
class TestUserWorkflowIntegration:
    """Integration tests for complete user workflows"""

    def test_complete_driver_approval_workflow(self, api_client, admin_client, db):
        """Test complete workflow from registration to approval to login"""
        # 1. Driver registers
        response = api_client.post('/auth/register/', {
            'email': 'newdriver+driver@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'New',
            'last_name': 'Driver'
        })
        assert response.status_code == 201
        
        # 2. Driver cannot login initially
        login_response = api_client.post('/auth/login/', {
            'email': 'newdriver+driver@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 401
        
        # 3. Admin approves driver
        driver = CustomUser.objects.get(email='newdriver+driver@test.com')
        approve_response = admin_client.patch(f'/api/users/{driver.id}/', {
            'status': 'approved'
        })
        assert approve_response.status_code == 200
        
        # 4. Driver can now login
        login_response = api_client.post('/auth/login/', {
            'email': 'newdriver+driver@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 200
        assert 'access' in login_response.data
        
        # 5. Driver can access their profile
        token = login_response.data['access']
        api_client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
        
        profile_response = api_client.get(f'/api/users/{driver.id}/')
        assert profile_response.status_code == 200
        assert profile_response.data['email'] == 'newdriver+driver@test.com'

    def test_rejection_and_reapproval_workflow(self, api_client, admin_client, db):
        """Test workflow of rejecting and then reapproving a user"""
        # 1. Create and approve driver
        api_client.post('/auth/register/', {
            'email': 'workflow+driver@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Workflow',
            'last_name': 'Driver'
        })
        
        driver = CustomUser.objects.get(email='workflow+driver@test.com')
        admin_client.patch(f'/api/users/{driver.id}/', {'status': 'approved'})
        
        # 2. Reject the driver
        reject_response = admin_client.patch(f'/api/users/{driver.id}/', {
            'status': 'rejected'
        })
        assert reject_response.status_code == 200
        
        # 3. Driver cannot login when rejected
        login_response = api_client.post('/auth/login/', {
            'email': 'workflow+driver@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 401
        
        # 4. Move back to preapproval
        preapproval_response = admin_client.patch(f'/api/users/{driver.id}/', {
            'status': 'preapproval'
        })
        assert preapproval_response.status_code == 200
        
        # 5. Still cannot login in preapproval
        login_response = api_client.post('/auth/login/', {
            'email': 'workflow+driver@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 401
        
        # 6. Approve again
        approve_response = admin_client.patch(f'/api/users/{driver.id}/', {
            'status': 'approved'
        })
        assert approve_response.status_code == 200
        
        # 7. Now can login
        login_response = api_client.post('/auth/login/', {
            'email': 'workflow+driver@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 200