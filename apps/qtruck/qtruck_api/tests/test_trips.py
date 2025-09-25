"""
Comprehensive test suite for trips, expenses, receipts, and vehicle mileage endpoints.

Test scenarios covered:
1. Trip CRUD operations with proper permissions
2. Trip status management and cost calculation
3. Expense management with receipt attachment
4. Vehicle mileage tracking
5. Material and material variant integration
6. Driver and truck assignment
7. Filtering and search functionality
8. Permission-based access control
"""

import json
import os
from decimal import Decimal
from django.test import TestCase
from django.urls import reverse
from django.core.files.uploadedfile import SimpleUploadedFile
from rest_framework.test import APIClient
from rest_framework import status
from django.contrib.auth import get_user_model
from django.utils import timezone
from datetime import datetime, timedelta

from users.models import CustomUser
from drivers.models import Driver
from fleet.models import Truck, Material, MaterialVariant
from trips.models import Trip, Expense, Receipt, VehicleMileage

CustomUser = get_user_model()


class TripsAPITestCase(TestCase):
    def setUp(self):
        """Set up test environment with users, drivers, trucks, materials"""
        self.client = APIClient()
        
        # Create test users with proper email aliases
        self.admin_user = CustomUser.objects.create_user(
            email='admin+test@test.com',
            password='testpass123',
            first_name='Admin',
            last_name='User',
            user_type='admin',
            status='approved'
        )
        
        self.driver_user = CustomUser.objects.create_user(
            email='driver+test@test.com',
            password='testpass123',
            first_name='Driver',
            last_name='User',
            user_type='driver',
            status='approved'
        )
        
        self.tester_user = CustomUser.objects.create_user(
            email='tester+test@test.com',
            password='testpass123',
            first_name='Tester',
            last_name='User',
            user_type='tester',
            status='approved'
        )
        
        # Get or create driver profile (might already exist from user creation)
        self.driver, created = Driver.objects.get_or_create(
            user=self.driver_user,
            defaults={
                'name': f"{self.driver_user.first_name} {self.driver_user.last_name}",
                'phone': "+1234567890",
                'license_number': "DL123456"
            }
        )
        
        # Link driver profile to user if not already linked
        if not self.driver_user.driver_profile:
            self.driver_user.driver_profile = self.driver
            self.driver_user.save()
        
        # Create test truck
        self.truck = Truck.objects.create(
            license_plate='TEST123',
            model='Toyota Hilux'
        )
        
        # Create test materials
        self.material = Material.objects.create(
            name='Sand',
            description='Fine construction sand'
        )
        
        self.material_variant = MaterialVariant.objects.create(
            material=self.material,
            name='River Sand',
            description='High quality river sand'
        )
        
        # Create test image file (minimal valid JPEG)
        # This is a minimal 1x1 pixel JPEG image
        jpeg_data = b'\xff\xd8\xff\xe0\x00\x10JFIF\x00\x01\x01\x01\x00H\x00H\x00\x00\xff\xdb\x00C\x00\x03\x02\x02\x03\x02\x02\x03\x03\x03\x03\x04\x03\x03\x04\x05\x08\x05\x05\x04\x04\x05\n\x07\x07\x06\x08\x0c\n\x0c\x0c\x0b\n\x0b\x0b\r\x0e\x12\x10\r\x0e\x11\x0e\x0b\x0b\x10\x16\x10\x11\x13\x14\x15\x15\x15\x0c\x0f\x17\x18\x16\x14\x18\x12\x14\x15\x14\xff\xc0\x00\x11\x08\x00\x01\x00\x01\x01\x01\x11\x00\x02\x11\x01\x03\x11\x01\xff\xc4\x00\x14\x00\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x08\xff\xc4\x00\x14\x10\x01\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xff\xda\x00\x0c\x03\x01\x00\x02\x11\x03\x11\x00\x3f\x00\x00\xff\xd9'
        self.test_image = SimpleUploadedFile(
            "test_image.jpg",
            jpeg_data,
            content_type="image/jpeg"
        )
        
        # URLs
        self.trips_url = '/api/trips/trips/'
        self.expenses_url = '/api/trips/expenses/'
        self.receipts_url = '/api/trips/receipts/'
        self.vehicle_mileage_url = '/api/trips/vehicle-mileage/'

    def authenticate_user(self, user):
        """Helper method to authenticate a user"""
        self.client.force_authenticate(user=user)

    def test_trip_create_as_admin(self):
        """Test admin can create trips"""
        self.authenticate_user(self.admin_user)
        
        trip_data = {
            'truck_id': str(self.truck.id),
            'driver_id': str(self.driver.id),
            'date': timezone.now().isoformat(),
            'start_location': 'Test Start Location',
            'end_location': 'Test End Location',
            'start_mileage': 1000,
            'end_mileage': 1150,
            'material_id': str(self.material.id),
            'material_variant_id': str(self.material_variant.id),
            'material_cost': '250.00',
            'status': 'pending'
        }
        
        response = self.client.post(self.trips_url, trip_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify trip creation
        trip = Trip.objects.get(id=response.data['id'])
        self.assertEqual(trip.truck, self.truck)
        self.assertEqual(trip.driver, self.driver)
        self.assertEqual(trip.material, self.material)
        self.assertEqual(trip.material_variant, self.material_variant)
        self.assertEqual(trip.total_mileage, 150)  # Auto-calculated
        self.assertEqual(float(trip.material_cost), 250.00)

    def test_trip_create_as_driver_own_profile(self):
        """Test driver can create trips for themselves"""
        self.authenticate_user(self.driver_user)
        
        trip_data = {
            'truck_id': str(self.truck.id),
            'driver_id': str(self.driver.id),
            'date': timezone.now().isoformat(),
            'start_location': 'Driver Start Location',
            'end_location': 'Driver End Location',
            'status': 'in_progress'
        }
        
        response = self.client.post(self.trips_url, trip_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)

    def test_trip_list_filtering_by_status(self):
        """Test trip listing with status filtering"""
        self.authenticate_user(self.admin_user)
        
        # Clear any existing trips to ensure clean test
        Trip.objects.all().delete()
        
        # Create trips with different statuses
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now(),
            status='pending'
        )
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now(),
            status='completed'
        )
        
        # Filter by pending status
        response = self.client.get(f'{self.trips_url}?status=pending')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 1)
        self.assertEqual(response.data['results'][0]['status'], 'pending')

    def test_trip_calculate_cost_action(self):
        """Test trip cost calculation endpoint"""
        self.authenticate_user(self.admin_user)
        
        # Create trip with material cost
        trip = Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now(),
            material_cost=Decimal('100.00')
        )
        
        # Add expense to trip
        Expense.objects.create(
            trip=trip,
            description='Fuel',
            amount=Decimal('50.00')
        )
        
        # Calculate cost
        response = self.client.post(f'{self.trips_url}{trip.id}/calculate_cost/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(float(response.data['total_cost']), 150.00)

    def test_trip_by_status_action(self):
        """Test getting trips grouped by status"""
        self.authenticate_user(self.admin_user)
        
        Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now(), status='pending')
        Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now(), status='completed')
        
        response = self.client.get(f'{self.trips_url}by_status/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIn('pending', response.data)
        self.assertIn('completed', response.data)

    def test_expense_crud_operations(self):
        """Test expense CRUD operations"""
        self.authenticate_user(self.admin_user)
        
        # Create trip first
        trip = Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now()
        )
        
        # Create expense
        expense_data = {
            'trip_id': str(trip.id),
            'description': 'Test Expense',
            'amount': '75.50'
        }
        
        response = self.client.post(self.expenses_url, expense_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        expense_id = response.data['id']
        
        # Read expense
        response = self.client.get(f'{self.expenses_url}{expense_id}/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['description'], 'Test Expense')
        
        # Update expense
        update_data = {'description': 'Updated Expense', 'amount': '80.00'}
        response = self.client.patch(f'{self.expenses_url}{expense_id}/', update_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(response.data['description'], 'Updated Expense')
        
        # Delete expense
        response = self.client.delete(f'{self.expenses_url}{expense_id}/')
        self.assertEqual(response.status_code, status.HTTP_204_NO_CONTENT)

    def test_receipt_with_image_upload(self):
        """Test receipt creation with image upload"""
        self.authenticate_user(self.admin_user)
        
        # Create trip and expense
        trip = Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now())
        expense = Expense.objects.create(trip=trip, description='Fuel', amount=Decimal('50.00'))
        
        # Create receipt with image
        receipt_data = {
            'expense': str(expense.id),
            'note': 'Fuel receipt',
            'image': self.test_image
        }
        
        response = self.client.post(self.receipts_url, receipt_data, format='multipart')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        self.assertEqual(response.data['note'], 'Fuel receipt')

    def test_receipt_extract_details_action(self):
        """Test OCR receipt details extraction"""
        self.authenticate_user(self.admin_user)
        
        # Create receipt
        trip = Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now())
        expense = Expense.objects.create(trip=trip, description='Fuel', amount=Decimal('50.00'))
        receipt = Receipt.objects.create(expense=expense, image='test.jpg')
        
        # Extract details (mocked OCR)
        response = self.client.post(f'{self.receipts_url}{receipt.id}/extract_details/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertIn('vendor', response.data)
        self.assertIn('amount', response.data)

    def test_vehicle_mileage_operations(self):
        """Test vehicle mileage CRUD operations"""
        self.authenticate_user(self.admin_user)
        
        mileage_data = {
            'truck_id': str(self.truck.id),
            'driver_id': str(self.driver.id),
            'start_mileage': 5000,
            'end_mileage': 5200,
            'date': timezone.now().isoformat()
        }
        
        # Create vehicle mileage
        response = self.client.post(self.vehicle_mileage_url, mileage_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_201_CREATED)
        
        # Verify auto-calculated mileage
        mileage_id = response.data['id']
        mileage = VehicleMileage.objects.get(id=mileage_id)
        self.assertEqual(mileage.mileage, 200)

    def test_permission_access_control(self):
        """Test permission-based access control"""
        # Create trip as admin
        self.authenticate_user(self.admin_user)
        trip = Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now())
        
        # Test unauthenticated access
        self.client.logout()
        response = self.client.get(self.trips_url)
        self.assertEqual(response.status_code, status.HTTP_401_UNAUTHORIZED)
        
        # Test driver access to own trips
        self.authenticate_user(self.driver_user)
        response = self.client.get(self.trips_url)
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Driver should see their own trips
        self.assertEqual(len(response.data['results']), 1)

    def test_material_variant_validation(self):
        """Test material variant must belong to selected material"""
        self.authenticate_user(self.admin_user)
        
        # Create another material and variant
        other_material = Material.objects.create(name='Cement')
        other_variant = MaterialVariant.objects.create(
            material=other_material,
            name='Portland Cement'
        )
        
        # Try to create trip with mismatched material and variant
        trip_data = {
            'truck_id': str(self.truck.id),
            'driver_id': str(self.driver.id),
            'date': timezone.now().isoformat(),
            'material_id': str(self.material.id),  # Sand
            'material_variant_id': str(other_variant.id),  # Portland Cement (belongs to Cement)
        }
        
        response = self.client.post(self.trips_url, trip_data, format='json')
        self.assertEqual(response.status_code, status.HTTP_400_BAD_REQUEST)
        self.assertIn('Material variant must belong to the selected material', str(response.data))

    def test_trip_search_functionality(self):
        """Test trip search in locations"""
        self.authenticate_user(self.admin_user)
        
        # Clear any existing trips to ensure clean test
        Trip.objects.all().delete()
        
        # Create trips with different locations
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now(),
            start_location='Nairobi Central',
            end_location='Mombasa Port'
        )
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.now(),
            start_location='Kisumu Town',
            end_location='Eldoret Market'
        )
        
        # Search for trips with 'Nairobi' in location
        response = self.client.get(f'{self.trips_url}?search=Nairobi')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 1)
        self.assertIn('Nairobi', response.data['results'][0]['start_location'])

    def test_expense_filtering_by_driver_and_truck(self):
        """Test expense filtering by driver and truck"""
        self.authenticate_user(self.admin_user)
        
        # Create trips for different scenarios
        trip1 = Trip.objects.create(truck=self.truck, driver=self.driver, date=timezone.now())
        
        # Create another driver and truck
        other_user = CustomUser.objects.create_user(
            email='other+driver@test.com',
            password='testpass123',
            first_name='Other',
            last_name='Driver',
            user_type='driver',
            status='approved'
        )
        other_driver, created = Driver.objects.get_or_create(
            user=other_user,
            defaults={
                'name': 'Other Driver',
                'phone': '+9876543210',
                'license_number': 'DL789012'
            }
        )
        
        trip2 = Trip.objects.create(truck=self.truck, driver=other_driver, date=timezone.now())
        
        # Create expenses
        Expense.objects.create(trip=trip1, description='Fuel 1', amount=Decimal('50.00'))
        Expense.objects.create(trip=trip2, description='Fuel 2', amount=Decimal('60.00'))
        
        # Filter expenses by driver
        response = self.client.get(f'{self.expenses_url}by_driver/?driver_id={self.driver.id}')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        
        # Filter expenses by truck
        response = self.client.get(f'{self.expenses_url}by_truck/?truck_id={self.truck.id}')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)

    def test_date_range_filtering(self):
        """Test filtering trips by date range"""
        self.authenticate_user(self.admin_user)
        
        # Clear any existing trips to ensure clean test
        Trip.objects.all().delete()
        
        # Create trips on different dates
        today = timezone.now().date()
        yesterday = today - timedelta(days=1)
        tomorrow = today + timedelta(days=1)
        
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.make_aware(datetime.combine(yesterday, datetime.min.time()))
        )
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.make_aware(datetime.combine(today, datetime.min.time()))
        )
        Trip.objects.create(
            truck=self.truck,
            driver=self.driver,
            date=timezone.make_aware(datetime.combine(tomorrow, datetime.min.time()))
        )
        
        # Filter trips for today only
        response = self.client.get(f'{self.trips_url}?date_from={today}&date_to={today}')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 1)

    def test_pagination_functionality(self):
        """Test API pagination"""
        self.authenticate_user(self.admin_user)
        
        # Create multiple trips
        for i in range(25):
            Trip.objects.create(
                truck=self.truck,
                driver=self.driver,
                date=timezone.now(),
                start_location=f'Location {i}'
            )
        
        # Test first page
        response = self.client.get(f'{self.trips_url}?page=1&page_size=20')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 20)
        self.assertIsNotNone(response.data['next'])
        
        # Test second page
        response = self.client.get(f'{self.trips_url}?page=2&page_size=20')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data['results']), 5)
        self.assertIsNone(response.data['next'])

    def tearDown(self):
        """Clean up after tests"""
        # Clean up any uploaded files
        if hasattr(self, 'test_image'):
            self.test_image.close()