"""
Pytest fixtures for API testing with proper authentication flows.
"""
import os
import django
from django.conf import settings

# Configure Django settings before importing anything else
if not settings.configured:
    os.environ.setdefault('DJANGO_SETTINGS_MODULE', 'qtruck_api.settings')
    django.setup()

import pytest
from rest_framework.test import APIClient
from django.contrib.auth import get_user_model
from settings.models import SystemSettings

User = get_user_model()


@pytest.fixture
def api_client():
    """Unauthenticated API client."""
    return APIClient()


@pytest.fixture
def first_admin_user(db):
    """Ensure a first admin user exists - used by tests that need admin to exist but don't test first user creation"""
    if not User.objects.exists():
        user = User.objects.create_user(
            email='fixture+admin@test.com',
            password='testpass123',
            first_name='Fixture',
            last_name='Admin'
        )
        return user
    return User.objects.filter(user_type='admin').first()


@pytest.fixture
def admin_user(db):
    """Create an approved admin user."""
    # This will be the first user, so gets auto-approved as admin
    user = User.objects.create_user(
        email='admin+admin@test.com',
        password='testpass123',
        first_name='Admin',
        last_name='User'
    )
    # Ensure admin status (should be automatic for first user)
    user.user_type = 'admin'
    user.status = 'approved'
    user.is_staff = True
    user.is_superuser = True
    user.save()
    return user


@pytest.fixture 
def driver_user(db, first_admin_user):
    """Create a driver user (pending approval by default)."""
    return User.objects.create_user(
        email='driver+driver@test.com',
        password='testpass123',
        user_type='driver',
        first_name='Driver',
        last_name='User'
    )


@pytest.fixture
def approved_driver_user(db, driver_user):
    """Create an approved driver user."""
    driver_user.status = 'approved'
    driver_user.save()
    return driver_user


@pytest.fixture
def tester_user(db, first_admin_user):
    """Create a tester user (pending approval by default)."""
    return User.objects.create_user(
        email='tester+tester@test.com', 
        password='testpass123',
        user_type='tester',
        first_name='Tester',
        last_name='User'
    )


@pytest.fixture
def approved_tester_user(db, tester_user):
    """Create an approved tester user."""
    tester_user.status = 'approved'
    tester_user.save()
    return tester_user


@pytest.fixture
def admin_client(admin_user):
    """API client authenticated as admin user."""
    from rest_framework.test import APIClient
    client = APIClient()
    
    # Login via API to get real JWT token
    login_response = client.post('/auth/login/', {
        'email': admin_user.email,
        'password': 'testpass123'
    })
    assert login_response.status_code == 200
    token = login_response.data['access']
    
    # Set authorization header
    client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
    client.user = admin_user  # Store user reference for tests
    return client


@pytest.fixture
def driver_client(approved_driver_user):
    """API client authenticated as approved driver user."""
    from rest_framework.test import APIClient
    client = APIClient()
    
    login_response = client.post('/auth/login/', {
        'email': approved_driver_user.email,
        'password': 'testpass123'
    })
    assert login_response.status_code == 200
    token = login_response.data['access']
    
    client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
    client.user = approved_driver_user
    return client


@pytest.fixture
def tester_client(approved_tester_user):
    """API client authenticated as approved tester user."""
    from rest_framework.test import APIClient
    client = APIClient()
    
    login_response = client.post('/auth/login/', {
        'email': approved_tester_user.email,
        'password': 'testpass123'
    })
    assert login_response.status_code == 200
    token = login_response.data['access']
    
    client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
    client.user = approved_tester_user
    return client


@pytest.fixture
def system_settings(db):
    """Create default system settings."""
    return SystemSettings.get_settings()


@pytest.fixture
def auto_approve_testers_settings(db):
    """System settings configured to auto-approve testers."""
    settings = SystemSettings.get_settings()
    settings.auto_approve_user_types = ['tester']
    settings.save()
    return settings


@pytest.fixture
def auto_approve_multiple_settings(db):
    """System settings configured to auto-approve multiple user types."""
    settings = SystemSettings.get_settings()
    settings.auto_approve_user_types = ['admin', 'tester']
    settings.save()
    return settings