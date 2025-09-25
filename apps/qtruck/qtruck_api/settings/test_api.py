"""
API tests for settings management using pytest and fixtures.

These tests ensure proper admin-only access to system settings
and test the auto-approval functionality through the API.
"""
import pytest
from settings.models import SystemSettings, LicenseClass


@pytest.mark.api
class TestSystemSettingsAPI:
    """Test system settings API access and permissions"""

    def test_admin_can_list_settings(self, admin_client, system_settings):
        """Test that admins can list system settings"""
        response = admin_client.get('/api/settings/system-settings/')
        
        assert response.status_code == 200
        assert len(response.data) >= 1  # SystemSettings returns list directly, not paginated

    def test_admin_can_view_settings_detail(self, admin_client, system_settings):
        """Test that admins can view settings details"""
        response = admin_client.get(f'/api/settings/system-settings/{system_settings.id}/')
        
        assert response.status_code == 200
        assert response.data['tester_registration_enabled'] == system_settings.tester_registration_enabled
        assert response.data['auto_approve_user_types'] == system_settings.auto_approve_user_types

    def test_admin_can_update_settings(self, admin_client, system_settings):
        """Test that admins can update system settings"""
        response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'tester_registration_enabled': False,
            'license_expiry_warning_days': 45,
            'auto_approve_user_types': ['admin', 'tester']
        }, format='json')
        
        assert response.status_code == 200
        
        # Verify settings were updated
        system_settings.refresh_from_db()
        assert system_settings.tester_registration_enabled == False
        assert system_settings.license_expiry_warning_days == 45
        assert system_settings.auto_approve_user_types == ['admin', 'tester']

    def test_admin_can_update_auto_approve_types(self, admin_client, system_settings):
        """Test that admins can update auto-approval user types"""
        response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': ['tester', 'driver']
        }, format='json')
        
        assert response.status_code == 200
        
        # Verify auto-approval types were updated
        system_settings.refresh_from_db()
        assert 'tester' in system_settings.auto_approve_user_types
        assert 'driver' in system_settings.auto_approve_user_types
        assert 'admin' not in system_settings.auto_approve_user_types

    def test_non_admin_cannot_list_settings(self, driver_client):
        """Test that non-admins cannot list settings"""
        response = driver_client.get('/api/settings/system-settings/')
        
        assert response.status_code == 403

    def test_non_admin_cannot_view_settings(self, driver_client, system_settings):
        """Test that non-admins cannot view settings"""
        response = driver_client.get(f'/api/settings/system-settings/{system_settings.id}/')
        
        assert response.status_code == 403

    def test_non_admin_cannot_update_settings(self, driver_client, system_settings):
        """Test that non-admins cannot update settings"""
        response = driver_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'tester_registration_enabled': False
        })
        
        assert response.status_code == 403

    def test_unauthenticated_cannot_access_settings(self, api_client, system_settings):
        """Test that unauthenticated users cannot access settings"""
        response = api_client.get('/api/settings/system-settings/')
        
        assert response.status_code == 401


@pytest.mark.api
class TestLicenseClassAPI:
    """Test license class API access and permissions"""

    def test_admin_can_list_license_classes(self, admin_client, system_settings):
        """Test that admins can list license classes"""
        response = admin_client.get('/api/settings/license-classes/')
        
        assert response.status_code == 200
        assert len(response.data['results']) >= 6  # Default license classes

    def test_admin_can_create_license_class(self, admin_client):
        """Test that admins can create new license classes"""
        response = admin_client.post('/api/settings/license-classes/', {
            'name': 'Test License',
            'description': 'A test license class'
        })
        
        assert response.status_code == 201
        
        # Verify license class was created
        license_class = LicenseClass.objects.get(name='Test License')
        assert license_class.description == 'A test license class'

    def test_admin_can_update_license_class(self, admin_client, system_settings):
        """Test that admins can update license classes"""
        # Get an existing license class
        license_class = LicenseClass.objects.first()
        
        response = admin_client.patch(f'/api/settings/license-classes/{license_class.id}/', {
            'description': 'Updated description'
        })
        
        assert response.status_code == 200
        
        # Verify license class was updated
        license_class.refresh_from_db()
        assert license_class.description == 'Updated description'

    def test_admin_can_delete_license_class(self, admin_client):
        """Test that admins can delete license classes"""
        # Create a license class to delete
        license_class = LicenseClass.objects.create(
            name='Deletable License',
            description='This will be deleted'
        )
        
        response = admin_client.delete(f'/api/settings/license-classes/{license_class.id}/')
        
        assert response.status_code == 204
        
        # Verify license class was deleted
        assert not LicenseClass.objects.filter(id=license_class.id).exists()

    def test_non_admin_cannot_create_license_class(self, driver_client):
        """Test that non-admins cannot create license classes"""
        response = driver_client.post('/api/settings/license-classes/', {
            'name': 'Unauthorized License',
            'description': 'Should not be created'
        })
        
        assert response.status_code == 403

    def test_non_admin_cannot_update_license_class(self, driver_client, system_settings):
        """Test that non-admins cannot update license classes"""
        license_class = LicenseClass.objects.first()
        
        response = driver_client.patch(f'/api/settings/license-classes/{license_class.id}/', {
            'description': 'Unauthorized update'
        })
        
        assert response.status_code == 403

    def test_non_admin_cannot_delete_license_class(self, driver_client, system_settings):
        """Test that non-admins cannot delete license classes"""
        license_class = LicenseClass.objects.first()
        
        response = driver_client.delete(f'/api/settings/license-classes/{license_class.id}/')
        
        assert response.status_code == 403


@pytest.mark.api
@pytest.mark.integration
class TestAutoApprovalIntegration:
    """Integration tests for auto-approval functionality via API"""

    def test_admin_configures_auto_approval_via_api(self, admin_client, system_settings, api_client):
        """Test that admin can configure auto-approval via API and it affects registration"""
        # 1. Admin configures auto-approval for testers
        settings_response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': ['tester']
        }, format='json')
        assert settings_response.status_code == 200
        
        # 2. Register a tester - should be auto-approved
        register_response = api_client.post('/auth/register/', {
            'email': 'autoapprove+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Auto',
            'last_name': 'Tester'
        })
        assert register_response.status_code == 201
        
        # 3. Verify tester was auto-approved
        from users.models import CustomUser
        tester = CustomUser.objects.get(email='autoapprove+tester@test.com')
        assert tester.status == 'approved'
        
        # 4. Tester should be able to login immediately
        login_response = api_client.post('/auth/login/', {
            'email': 'autoapprove+tester@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 200

    def test_admin_removes_auto_approval_via_api(self, admin_client, auto_approve_testers_settings, api_client):
        """Test that removing auto-approval affects subsequent registrations"""
        # 1. Admin removes tester from auto-approval
        settings_response = admin_client.patch(f'/api/settings/system-settings/{auto_approve_testers_settings.id}/', {
            'auto_approve_user_types': []  # Remove all auto-approval
        }, format='json')
        assert settings_response.status_code == 200
        
        # 2. Register a tester - should NOT be auto-approved
        register_response = api_client.post('/auth/register/', {
            'email': 'noapprove+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'No',
            'last_name': 'Approve'
        })
        assert register_response.status_code == 201
        
        # 3. Verify tester needs approval
        from users.models import CustomUser
        tester = CustomUser.objects.get(email='noapprove+tester@test.com')
        assert tester.status == 'preapproval'
        
        # 4. Tester should NOT be able to login
        login_response = api_client.post('/auth/login/', {
            'email': 'noapprove+tester@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 401

    def test_non_admin_cannot_configure_auto_approval(self, driver_client, system_settings):
        """Test that non-admins cannot configure auto-approval settings"""
        response = driver_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': ['driver']  # Trying to auto-approve drivers
        }, format='json')
        
        assert response.status_code == 403
        
        # Verify settings were not changed
        system_settings.refresh_from_db()
        assert 'driver' not in system_settings.auto_approve_user_types

    def test_multiple_user_types_auto_approval(self, admin_client, system_settings, api_client):
        """Test auto-approval with multiple user types configured"""
        # 1. Configure auto-approval for both testers and drivers
        settings_response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': ['tester', 'driver']
        }, format='json')
        assert settings_response.status_code == 200
        
        # 2. Register a tester - should be auto-approved
        api_client.post('/auth/register/', {
            'email': 'multi+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Multi',
            'last_name': 'Tester'
        })
        
        # 3. Register a driver - should be auto-approved
        api_client.post('/auth/register/', {
            'email': 'multi+driver@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Multi',
            'last_name': 'Driver'
        })
        
        # 4. Verify both were auto-approved
        from users.models import CustomUser
        tester = CustomUser.objects.get(email='multi+tester@test.com')
        driver = CustomUser.objects.get(email='multi+driver@test.com')
        
        assert tester.status == 'approved'
        assert driver.status == 'approved'
        
        # 5. Both should be able to login
        tester_login = api_client.post('/auth/login/', {
            'email': 'multi+tester@test.com',
            'password': 'testpass123'
        })
        assert tester_login.status_code == 200
        
        driver_login = api_client.post('/auth/login/', {
            'email': 'multi+driver@test.com',
            'password': 'testpass123'
        })
        assert driver_login.status_code == 200


@pytest.mark.api
@pytest.mark.slow
class TestSettingsWorkflowIntegration:
    """Comprehensive settings workflow integration tests"""

    def test_complete_settings_management_workflow(self, admin_client, driver_client, api_client, system_settings):
        """Test complete settings management workflow through API"""
        print(f"Admin user: {admin_client.user.user_type}, status: {admin_client.user.status}")
        # 1. Verify initial settings state
        settings_response = admin_client.get(f'/api/settings/system-settings/{system_settings.id}/')
        print(f"Settings response: {settings_response.status_code}")
        assert settings_response.status_code == 200
        assert settings_response.data['auto_approve_user_types'] == []
        
        # 2. Admin adds tester to auto-approval
        update_response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': ['tester'],
            'tester_registration_enabled': True,
            'license_expiry_warning_days': 60
        }, format='json')
        assert update_response.status_code == 200
        
        # 3. Test tester registration with auto-approval
        register_response = api_client.post('/auth/register/', {
            'email': 'workflow+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Workflow',
            'last_name': 'Tester'
        })
        assert register_response.status_code == 201
        
        # 4. Verify tester can login immediately
        login_response = api_client.post('/auth/login/', {
            'email': 'workflow+tester@test.com',
            'password': 'testpass123'
        })
        assert login_response.status_code == 200
        
        # 5. Admin modifies settings to remove auto-approval but keep registration enabled
        remove_approval_response = admin_client.patch(f'/api/settings/system-settings/{system_settings.id}/', {
            'auto_approve_user_types': [],
            'tester_registration_enabled': True  # Keep registration enabled but remove auto-approval
        }, format='json')
        assert remove_approval_response.status_code == 200
        
        # 6. New tester registration should require approval (but succeed)
        register_response2 = api_client.post('/auth/register/', {
            'email': 'workflow2+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Workflow2',
            'last_name': 'Tester'
        })
        assert register_response2.status_code == 201
        
        # 7. New tester cannot login without approval
        login_response2 = api_client.post('/auth/login/', {
            'email': 'workflow2+tester@test.com',
            'password': 'testpass123'
        })
        assert login_response2.status_code == 401
        
        # 8. Verify settings were persisted correctly
        final_settings_response = admin_client.get(f'/api/settings/system-settings/{system_settings.id}/')
        assert final_settings_response.status_code == 200
        assert final_settings_response.data['auto_approve_user_types'] == []
        assert final_settings_response.data['tester_registration_enabled'] == True
        assert final_settings_response.data['license_expiry_warning_days'] == 60