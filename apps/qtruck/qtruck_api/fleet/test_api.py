"""
Comprehensive test suite for fleet management API endpoints (Trucks, Materials, MaterialVariants).

Test scenarios covered:
1. Truck CRUD operations with proper permissions
2. Material CRUD operations with proper permissions
3. MaterialVariant CRUD operations with proper permissions
4. Material-Variant relationship validation
5. Permission enforcement (admin vs driver/tester access)
6. Authentication and authorization flows
"""

import json
import tempfile
from PIL import Image
from django.test import TestCase, TransactionTestCase
from django.urls import reverse
from rest_framework.test import APIClient
from django.core.files.uploadedfile import SimpleUploadedFile
from rest_framework import status
from django.contrib.auth import get_user_model
from users.models import CustomUser, UserProfile
from fleet.models import Truck, Material, MaterialVariant, MaterialCost, MaterialPhoto, MaterialVariantPhoto

CustomUser = get_user_model()


class FleetManagementAPITestCase(TestCase):
    def setUp(self):
        """Set up test environment"""
        self.client = APIClient()
        
        # Clear existing data
        CustomUser.objects.all().delete()
        Truck.objects.all().delete()
        Material.objects.all().delete()
        MaterialVariant.objects.all().delete()
        
        # Create test users using registration endpoint (like user tests)
        # Create admin first
        admin_data = {
            'email': 'admin+admin@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Admin',
            'last_name': 'User'
        }
        
        # Register admin (auto-approved as first admin)
        response = self.client.post('/auth/register/', admin_data)
        self.admin_user = CustomUser.objects.get(email=admin_data['email'])
        
        # Create driver via admin
        driver_data = {
            'email': 'driver+driver@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Driver',
            'last_name': 'User'
        }
        response = self.client.post('/auth/register/', driver_data)
        self.driver_user = CustomUser.objects.get(email=driver_data['email'])
        
        # Approve driver user using admin
        admin_token = self._get_token(admin_data['email'], admin_data['password'])
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {admin_token}')
        self.client.put(f'/api/users/{self.driver_user.id}/', {'status': 'approved'})
        self.driver_user.refresh_from_db()
        
        # Create tester via admin  
        tester_data = {
            'email': 'tester+tester@test.com',
            'password': 'testpass123',
            'password_confirm': 'testpass123',
            'first_name': 'Tester',
            'last_name': 'User'
        }
        response = self.client.post('/auth/register/', tester_data)
        self.tester_user = CustomUser.objects.get(email=tester_data['email'])
        
        # Approve tester user using admin
        self.client.put(f'/api/users/{self.tester_user.id}/', {'status': 'approved'})
        self.tester_user.refresh_from_db()
        
        
        # Create test data
        self.truck_data = {
            'license_plate': 'ABC-123',
            'model': 'Ford Transit'
        }
        
        self.material_data = {
            'name': 'Sand',
            'description': 'Construction sand material'
        }
        
        # Clear auth and get fresh login tokens
        self.client.credentials()
        self.admin_token = self._get_token('admin+admin@test.com', 'testpass123')
        self.driver_token = self._get_token('driver+driver@test.com', 'testpass123')
        self.tester_token = self._get_token('tester+tester@test.com', 'testpass123')
    
    def _get_token(self, email, password):
        """Helper to get authentication token"""
        response = self.client.post('/auth/login/', {
            'email': email,
            'password': password
        })
        if response.status_code != 200:
            print(f"Login failed for {email}: {response.status_code}")
            print(f"Response data: {response.data}")
            return None
        return response.data.get('access') or response.data.get('access_token')
    
    def _authenticate(self, token):
        """Helper to set authentication token"""
        self.client.credentials(HTTP_AUTHORIZATION=f'Bearer {token}')
    
    def _clear_auth(self):
        """Helper to clear authentication"""
        self.client.credentials()
    
    def _get_response_data(self, response):
        """Helper to extract data from potentially paginated response"""
        if isinstance(response.data, dict) and 'results' in response.data:
            return response.data['results']
        return response.data
    
    def _create_test_image(self, filename='test.jpg', size=(100, 100)):
        """Helper to create a test image file"""
        image = Image.new('RGB', size, color='red')
        temp_file = tempfile.NamedTemporaryFile(suffix='.jpg', delete=False)
        image.save(temp_file, format='JPEG')
        temp_file.seek(0)
        
        return SimpleUploadedFile(
            filename,
            temp_file.read(),
            content_type='image/jpeg'
        )
    
    def tearDown(self):
        """Clean up after each test"""
        # Clear authentication first
        self._clear_auth()
        
        # Use Django's database flush for better isolation
        from django.core.management import call_command
        from django.db import connection
        
        # Clear all fleet data in correct order (foreign keys)
        MaterialVariant.objects.all().delete()
        MaterialCost.objects.all().delete() 
        Material.objects.all().delete()
        Truck.objects.all().delete()
        CustomUser.objects.all().delete()
        
        # Reset auto-increment sequences
        if connection.vendor == 'postgresql':
            with connection.cursor() as cursor:
                cursor.execute("SELECT setval(pg_get_serial_sequence('fleet_truck','id'), 1, false);")
                cursor.execute("SELECT setval(pg_get_serial_sequence('fleet_material','id'), 1, false);") 
                cursor.execute("SELECT setval(pg_get_serial_sequence('fleet_materialvariant','id'), 1, false);")
        
        # Call parent tearDown
        super().tearDown()

    # ======= TRUCK TESTS =======
    
    def test_truck_list_authenticated_users(self):
        """Test that all authenticated approved users can list trucks"""
        # Create a truck
        truck = Truck.objects.create(
            license_plate='TEST-001',
            model='Test Truck'
        )
        
        # Test admin access
        self._authenticate(self.admin_token)
        response = self.client.get('/api/fleet/trucks/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        trucks_data = self._get_response_data(response)
        self.assertEqual(len(trucks_data), 1)
        self.assertEqual(trucks_data[0]['license_plate'], 'TEST-001')
        
        # Test driver access
        self._authenticate(self.driver_token)
        response = self.client.get('/api/fleet/trucks/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        trucks_data = self._get_response_data(response)
        self.assertEqual(len(trucks_data), 1)
        
        # Test tester access
        self._authenticate(self.tester_token)
        response = self.client.get('/api/fleet/trucks/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        trucks_data = self._get_response_data(response)
        self.assertEqual(len(trucks_data), 1)
    
    def test_truck_list_unauthenticated(self):
        """Test that unauthenticated users cannot list trucks"""
        self._clear_auth()
        response = self.client.get('/api/fleet/trucks/')
        self.assertEqual(response.status_code, status.HTTP_401_UNAUTHORIZED)
    
    def test_truck_create_admin_only(self):
        """Test that only admins can create trucks"""
        # Admin can create
        self._authenticate(self.admin_token)
        response = self.client.post('/api/fleet/trucks/', self.truck_data)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['license_plate'], 'ABC-123')
        
        # Driver cannot create
        self._authenticate(self.driver_token)
        response = self.client.post('/api/fleet/trucks/', {
            'license_plate': 'DEF-456',
            'model': 'Test Truck 2'
        })
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
        
        # Tester cannot create
        self._authenticate(self.tester_token)
        response = self.client.post('/api/fleet/trucks/', {
            'license_plate': 'GHI-789',
            'model': 'Test Truck 3'
        })
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_truck_retrieve_all_users(self):
        """Test that all authenticated users can retrieve truck details"""
        truck = Truck.objects.create(
            license_plate='RETR-001',
            model='Retrieve Test'
        )
        
        # Admin can retrieve
        self._authenticate(self.admin_token)
        response = self.client.get(f'/api/fleet/trucks/{truck.id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['license_plate'], 'RETR-001')
        
        # Driver can retrieve
        self._authenticate(self.driver_token)
        response = self.client.get(f'/api/fleet/trucks/{truck.id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Tester can retrieve
        self._authenticate(self.tester_token)
        response = self.client.get(f'/api/fleet/trucks/{truck.id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
    
    def test_truck_update_admin_only(self):
        """Test that only admins can update trucks"""
        truck = Truck.objects.create(
            license_plate='UPD-001',
            model='Update Test'
        )
        
        update_data = {
            'license_plate': 'UPD-001',
            'model': 'Updated Model'
        }
        
        # Admin can update
        self._authenticate(self.admin_token)
        response = self.client.put(f'/api/fleet/trucks/{truck.id}/', update_data)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['model'], 'Updated Model')
        
        # Driver cannot update
        self._authenticate(self.driver_token)
        response = self.client.put(f'/api/fleet/trucks/{truck.id}/', {
            'license_plate': 'UPD-001',
            'model': 'Driver Update'
        })
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
        
        # Tester cannot update
        self._authenticate(self.tester_token)
        response = self.client.put(f'/api/fleet/trucks/{truck.id}/', {
            'license_plate': 'UPD-001',
            'model': 'Tester Update'
        })
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_truck_delete_admin_only(self):
        """Test that only admins can delete trucks"""
        truck1 = Truck.objects.create(license_plate='DEL-001', model='Delete Test 1')
        truck2 = Truck.objects.create(license_plate='DEL-002', model='Delete Test 2')
        truck3 = Truck.objects.create(license_plate='DEL-003', model='Delete Test 3')
        
        # Admin can delete
        self._authenticate(self.admin_token)
        response = self.client.delete(f'/api/fleet/trucks/{truck1.id}/')
        self.assertEqual(response.status_code, status.HTTP_204_NO_CONTENT)
        
        # Driver cannot delete
        self._authenticate(self.driver_token)
        response = self.client.delete(f'/api/fleet/trucks/{truck2.id}/')
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
        
        # Tester cannot delete
        self._authenticate(self.tester_token)
        response = self.client.delete(f'/api/fleet/trucks/{truck3.id}/')
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)

    # ======= MATERIAL TESTS =======
    
    def test_material_list_authenticated_users(self):
        """Test that all authenticated approved users can list materials"""
        material = Material.objects.create(name='Test Material', description='Test description')
        
        # Test all user types can access
        for token in [self.admin_token, self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.get('/api/materials/')
            self.assertEqual(response.status_code, status.HTTP_200_OK)
            materials_data = self._get_response_data(response)
            self.assertEqual(len(materials_data), 1)
            self.assertEqual(materials_data[0]['name'], 'Test Material')
    
    def test_material_create_admin_only(self):
        """Test that only admins can create materials"""
        # Admin can create
        self._authenticate(self.admin_token)
        response = self.client.post('/api/materials/', self.material_data)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], 'Sand')
        
        # Non-admins cannot create
        for token in [self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.post('/api/materials/', {
                'name': 'Unauthorized Material',
                'description': 'Should not work'
            })
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_material_update_admin_only(self):
        """Test that only admins can update materials"""
        material = Material.objects.create(name='Original', description='Original desc')
        
        # Admin can update
        self._authenticate(self.admin_token)
        response = self.client.put(f'/api/materials/{material.id}/', {
            'name': 'Updated',
            'description': 'Updated desc'
        })
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], 'Updated')
        
        # Non-admins cannot update
        for token in [self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.put(f'/api/materials/{material.id}/', {
                'name': 'Unauthorized Update',
                'description': 'Should not work'
            })
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_material_delete_admin_only(self):
        """Test that only admins can delete materials"""
        material1 = Material.objects.create(name='Delete1', description='Test')
        material2 = Material.objects.create(name='Delete2', description='Test')
        material3 = Material.objects.create(name='Delete3', description='Test')
        
        # Admin can delete
        self._authenticate(self.admin_token)
        response = self.client.delete(f'/api/materials/{material1.id}/')
        self.assertEqual(response.status_code, status.HTTP_204_NO_CONTENT)
        
        # Non-admins cannot delete
        for i, token in enumerate([self.driver_token, self.tester_token], 2):
            material = [material2, material3][i-2]
            self._authenticate(token)
            response = self.client.delete(f'/api/materials/{material.id}/')
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)

    # ======= MATERIAL VARIANT TESTS =======
    
    def test_material_variant_list_authenticated_users(self):
        """Test that all authenticated users can list material variants"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant = MaterialVariant.objects.create(
            material=material,
            name='Darugo',
            description='Local sand variant'
        )
        
        # Test all user types can access
        for token in [self.admin_token, self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.get('/api/materials/variants/')
            self.assertEqual(response.status_code, status.HTTP_200_OK)
            variants_data = self._get_response_data(response)
            self.assertEqual(len(variants_data), 1)
            self.assertEqual(variants_data[0]['name'], 'Darugo')
    
    def test_material_variant_filter_by_material(self):
        """Test filtering variants by material"""
        material1 = Material.objects.create(name='Sand', description='Sand material')
        material2 = Material.objects.create(name='Stone', description='Stone material')
        
        variant1 = MaterialVariant.objects.create(material=material1, name='Darugo')
        variant2 = MaterialVariant.objects.create(material=material1, name='Kajido')
        variant3 = MaterialVariant.objects.create(material=material2, name='Quarry Stone')
        
        self._authenticate(self.admin_token)
        
        # Filter by material1
        response = self.client.get(f'/api/materials/variants/?material={material1.id}')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        variants_data = self._get_response_data(response)
        self.assertEqual(len(variants_data), 2)
        variant_names = [v['name'] for v in variants_data]
        self.assertIn('Darugo', variant_names)
        self.assertIn('Kajido', variant_names)
        self.assertNotIn('Quarry Stone', variant_names)
    
    def test_material_variants_via_material_endpoint(self):
        """Test getting variants via material detail endpoint"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant1 = MaterialVariant.objects.create(material=material, name='Darugo')
        variant2 = MaterialVariant.objects.create(material=material, name='Kajido')
        
        self._authenticate(self.admin_token)
        response = self.client.get(f'/api/materials/{material.id}/variants/')
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)
        variant_names = [v['name'] for v in response.data]
        self.assertIn('Darugo', variant_names)
        self.assertIn('Kajido', variant_names)
    
    def test_material_variant_create_admin_only(self):
        """Test that only admins can create material variants"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant_data = {
            'material': str(material.id),
            'name': 'River Sand',
            'description': 'Fine river sand'
        }
        
        # Admin can create
        self._authenticate(self.admin_token)
        
        response = self.client.post('/api/materials/variants/', variant_data)
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['name'], 'River Sand')
        
        # Non-admins cannot create
        for token in [self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.post('/api/materials/variants/', {
                'material': str(material.id),
                'name': 'Unauthorized Variant',
                'description': 'Should not work'
            })
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_material_variant_unique_constraint(self):
        """Test that material variant names must be unique per material"""
        material = Material.objects.create(name='Sand', description='Test sand')
        
        # Create first variant
        self._authenticate(self.admin_token)
        response = self.client.post('/api/materials/variants/', {
            'material': str(material.id),
            'name': 'Darugo',
            'description': 'First variant'
        })
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Try to create duplicate - should fail
        response = self.client.post('/api/materials/variants/', {
            'material': str(material.id),
            'name': 'Darugo',
            'description': 'Duplicate variant'
        })
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
    
    def test_material_variant_update_admin_only(self):
        """Test that only admins can update material variants"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant = MaterialVariant.objects.create(material=material, name='Original')
        
        # Admin can update
        self._authenticate(self.admin_token)
        response = self.client.put(f'/api/materials/variants/{variant.id}/', {
            'material': str(material.id),
            'name': 'Updated',
            'description': 'Updated description'
        })
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], 'Updated')
        
        # Non-admins cannot update
        for token in [self.driver_token, self.tester_token]:
            self._authenticate(token)
            response = self.client.put(f'/api/materials/variants/{variant.id}/', {
                'material': str(material.id),
                'name': 'Unauthorized Update',
                'description': 'Should not work'
            })
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)
    
    def test_material_variant_delete_admin_only(self):
        """Test that only admins can delete material variants"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant1 = MaterialVariant.objects.create(material=material, name='Delete1')
        variant2 = MaterialVariant.objects.create(material=material, name='Delete2')
        variant3 = MaterialVariant.objects.create(material=material, name='Delete3')
        
        # Admin can delete
        self._authenticate(self.admin_token)
        response = self.client.delete(f'/api/materials/variants/{variant1.id}/')
        self.assertEqual(response.status_code, status.HTTP_204_NO_CONTENT)
        
        # Non-admins cannot delete
        for i, token in enumerate([self.driver_token, self.tester_token], 2):
            variant = [variant2, variant3][i-2]
            self._authenticate(token)
            response = self.client.delete(f'/api/materials/variants/{variant.id}/')
            self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)

    # ======= INTEGRATION TESTS =======
    
    def test_material_serializer_includes_variants(self):
        """Test that material serializer includes variants in response"""
        material = Material.objects.create(name='Sand', description='Test sand')
        variant1 = MaterialVariant.objects.create(material=material, name='Darugo')
        variant2 = MaterialVariant.objects.create(material=material, name='Kajido')
        
        self._authenticate(self.admin_token)
        response = self.client.get(f'/api/materials/{material.id}/')
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['name'], 'Sand')
        self.assertIn('variants', response.data)
        self.assertEqual(len(response.data['variants']), 2)
        
        variant_names = [v['name'] for v in response.data['variants']]
        self.assertIn('Darugo', variant_names)
        self.assertIn('Kajido', variant_names)
    
    def test_truck_basic_crud_operations(self):
        """Test basic truck CRUD operations"""
        self._authenticate(self.admin_token)
        
        # Create truck
        response = self.client.post('/api/fleet/trucks/', {
            'license_plate': 'CRUD-001',
            'model': 'CRUD Test Truck'
        })
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['license_plate'], 'CRUD-001')
        self.assertEqual(response.data['model'], 'CRUD Test Truck')
        
        # Verify truck in response has all expected fields
        truck_id = response.data['id']
        response = self.client.get(f'/api/fleet/trucks/{truck_id}/')
        
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['license_plate'], 'CRUD-001')
        self.assertEqual(response.data['model'], 'CRUD Test Truck')
        self.assertIn('created_at', response.data)
        self.assertIn('updated_at', response.data)

    def test_error_handling_nonexistent_resources(self):
        """Test proper error handling for non-existent resources"""
        self._authenticate(self.admin_token)
        
        fake_uuid = '12345678-1234-5678-9012-123456789012'
        
        # Test non-existent truck
        response = self.client.get(f'/api/fleet/trucks/{fake_uuid}/')
        self.assertEqual(response.status_code, status.HTTP_404_NOT_FOUND)
        
        # Test non-existent material
        response = self.client.get(f'/api/materials/{fake_uuid}/')
        self.assertEqual(response.status_code, status.HTTP_404_NOT_FOUND)
        
        # Test non-existent material variant
        response = self.client.get(f'/api/materials/variants/{fake_uuid}/')
        self.assertEqual(response.status_code, status.HTTP_404_NOT_FOUND)

    # ======= IMAGE UPLOAD TESTS =======
    
    def test_material_creation_with_image_upload(self):
        """Test creating material with image upload via multipart form data"""
        self._authenticate(self.admin_token)
        
        # Create test image
        test_image = self._create_test_image('material_photo.jpg')
        
        # Create material with photo - include files directly in data
        data = {
            'name': 'Sand with Photo',
            'description': 'Sand material with uploaded photo',
            'photos': test_image
        }
        
        response = self.client.post('/api/materials/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify material was created
        material = Material.objects.get(name='Sand with Photo')
        self.assertEqual(material.name, 'Sand with Photo')
        
        # Verify photo was uploaded
        self.assertEqual(material.photos.count(), 1)
        photo = material.photos.first()
        self.assertIsNotNone(photo.photo)
        self.assertTrue(photo.photo.name.endswith('.jpg'))
    
    def test_material_update_with_image_upload(self):
        """Test updating material with additional image upload"""
        self._authenticate(self.admin_token)
        
        # Create material first
        material = Material.objects.create(name='Stone', description='Stone material')
        
        # Update material with photo
        test_image = self._create_test_image('stone_photo.jpg')
        
        data = {
            'name': 'Stone Updated',
            'description': 'Updated stone material with photo',
            'photos': test_image
        }
        
        response = self.client.patch(f'/api/materials/{material.id}/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Verify material was updated
        material.refresh_from_db()
        self.assertEqual(material.name, 'Stone Updated')
        
        # Verify photo was uploaded
        self.assertEqual(material.photos.count(), 1)
    
    def test_material_variant_creation_with_image_upload(self):
        """Test creating material variant with image upload via multipart form data"""
        self._authenticate(self.admin_token)
        
        # Create base material first
        material = Material.objects.create(name='Sand', description='Base sand material')
        
        # Create test image
        test_image = self._create_test_image('variant_photo.jpg')
        
        # Create material variant with photo
        data = {
            'material': material.id,
            'name': 'Darugo with Photo',
            'description': 'Darugo sand variant with uploaded photo',
            'photos': test_image
        }
        
        response = self.client.post('/api/materials/variants/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify variant was created
        variant = MaterialVariant.objects.get(name='Darugo with Photo')
        self.assertEqual(variant.material, material)
        
        # Verify photo was uploaded
        self.assertEqual(variant.photos.count(), 1)
        photo = variant.photos.first()
        self.assertIsNotNone(photo.photo)
        self.assertTrue(photo.photo.name.endswith('.jpg'))
    
    def test_material_variant_update_with_image_upload(self):
        """Test updating material variant with additional image upload"""
        self._authenticate(self.admin_token)
        
        # Create material and variant first
        material = Material.objects.create(name='Sand', description='Sand material')
        variant = MaterialVariant.objects.create(
            material=material,
            name='Kajido',
            description='Kajido sand variant'
        )
        
        # Update variant with photo
        test_image = self._create_test_image('kajido_photo.jpg')
        
        data = {
            'name': 'Kajido Updated',
            'description': 'Updated kajido variant with photo',
            'photos': test_image
        }
        
        response = self.client.patch(f'/api/materials/variants/{variant.id}/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Verify variant was updated
        variant.refresh_from_db()
        self.assertEqual(variant.name, 'Kajido Updated')
        
        # Verify photo was uploaded
        self.assertEqual(variant.photos.count(), 1)
    
    def test_multiple_image_uploads(self):
        """Test uploading multiple images at once"""
        self._authenticate(self.admin_token)
        
        # Create multiple test images
        image1 = self._create_test_image('photo1.jpg')
        image2 = self._create_test_image('photo2.jpg')
        
        # Create material with multiple photos
        data = {
            'name': 'Multi Photo Material',
            'description': 'Material with multiple photos',
            'photos': [image1, image2]
        }
        
        response = self.client.post('/api/materials/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify material was created
        material = Material.objects.get(name='Multi Photo Material')
        
        # Verify multiple photos were uploaded
        self.assertEqual(material.photos.count(), 2)
    
    def test_non_admin_cannot_upload_images(self):
        """Test that non-admin users cannot upload images"""
        self._authenticate(self.driver_token)
        
        test_image = self._create_test_image('unauthorized_photo.jpg')
        
        data = {
            'name': 'Unauthorized Material',
            'description': 'This should not be created',
            'photos': test_image
        }
        
        response = self.client.post('/api/materials/', data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_403_FORBIDDEN)