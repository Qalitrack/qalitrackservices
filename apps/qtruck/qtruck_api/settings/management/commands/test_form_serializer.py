from django.core.management.base import BaseCommand
from django.test import RequestFactory
from django.contrib.auth.models import User
from core.serializers import DriverProfileUpdateSerializer
from django.core.files.uploadedfile import SimpleUploadedFile
import io


class Command(BaseCommand):
    help = 'Test the DriverProfileUpdateSerializer with form data'

    def handle(self, *args, **options):
        # Simulate form data as it would come from a multipart request
        form_data = {
            'full_name': 'Brian Test Updated',
            'phone_number': '+254700000001', 
            'id_number': '30000001',
            'license_number': 'DL30000001',
            'license_expiry_date': '2025-12-31',
            'license_classes': '278f330f-8c38-48ef-9d53-c85f28d57b51,e01d791b-24c6-4cf4-b0ed-0e862859c5a0'
        }
        
        # Test 1: Regular form data
        self.stdout.write("Test 1: Regular form data")
        serializer = DriverProfileUpdateSerializer()
        try:
            internal_data = serializer.to_internal_value(form_data)
            self.stdout.write(self.style.SUCCESS(f"Success: {internal_data}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error: {e}"))
        
        # Test 2: Simulate bytes data (as might come from some form parsers)
        self.stdout.write("\nTest 2: Bytes data")
        bytes_data = {
            'full_name': b'Brian Test Updated',
            'phone_number': b'+254700000001', 
            'id_number': b'30000001',
            'license_number': b'DL30000001',
            'license_expiry_date': b'2025-12-31',
            'license_classes': b'278f330f-8c38-48ef-9d53-c85f28d57b51,e01d791b-24c6-4cf4-b0ed-0e862859c5a0'
        }
        
        try:
            internal_data = serializer.to_internal_value(bytes_data)
            self.stdout.write(self.style.SUCCESS(f"Success: {internal_data}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error: {e}"))
        
        # Test 3: Simulate file-like objects (as Django might create for form fields)
        self.stdout.write("\nTest 3: File-like objects")
        file_like_data = {}
        
        for key, value in form_data.items():
            if isinstance(value, str):
                # Create a simple uploaded file object to simulate how Django might parse form data
                file_like_data[key] = SimpleUploadedFile(
                    name=key,
                    content=value.encode('utf-8'),
                    content_type='text/plain'
                )
            else:
                file_like_data[key] = value
        
        try:
            internal_data = serializer.to_internal_value(file_like_data)
            self.stdout.write(self.style.SUCCESS(f"Success: {internal_data}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error: {e}"))
        
        # Test 4: Edge case - license_classes with brackets (as might come from some frontend serializations)
        self.stdout.write("\nTest 4: License classes with brackets")
        edge_case_data = {
            'full_name': 'Brian Test Updated',
            'phone_number': '+254700000001', 
            'id_number': '30000001',
            'license_number': 'DL30000001',
            'license_expiry_date': '2025-12-31',
            'license_classes': "['e01d791b-24c6-4cf4-b0ed-0e862859c5a0']"  # This is the problematic format
        }
        
        try:
            internal_data = serializer.to_internal_value(edge_case_data)
            self.stdout.write(self.style.SUCCESS(f"Success: {internal_data}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Error: {e}"))
        
        # Test 5: Date format variations
        self.stdout.write("\nTest 5: Different date formats")
        date_formats_test = [
            ('2025-12-31', 'YYYY-MM-DD'),
            ('12/31/2025', 'MM/DD/YYYY'),
            ('31/12/2025', 'DD/MM/YYYY'),
            ('2025/12/31', 'YYYY/MM/DD')
        ]
        
        for date_str, format_name in date_formats_test:
            test_data = {
                'full_name': 'Brian Test Updated',
                'phone_number': '+254700000001', 
                'id_number': '30000001',
                'license_number': 'DL30000001',
                'license_expiry_date': date_str,
                'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0'
            }
            
            try:
                internal_data = serializer.to_internal_value(test_data)
                self.stdout.write(self.style.SUCCESS(f"Date {format_name} ({date_str}): Success - {internal_data['license_expiry_date']}"))
            except Exception as e:
                self.stdout.write(self.style.ERROR(f"Date {format_name} ({date_str}): Error - {e}"))
        
        # Test 6: Edge case dates
        self.stdout.write("\nTest 6: Edge case dates")
        edge_date_tests = [
            ('01-01-2025', '01-01-2025'),
            ('1/1/2025', '1/1/2025'),
            ('2025-1-1', '2025-1-1'),
            ('', 'Empty string'),
            ('invalid-date', 'Invalid date'),
        ]
        
        for date_str, description in edge_date_tests:
            test_data = {
                'full_name': 'Brian Test Updated',
                'phone_number': '+254700000001', 
                'id_number': '30000001',
                'license_number': 'DL30000001',
                'license_expiry_date': date_str,
                'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0'
            }
            
            try:
                internal_data = serializer.to_internal_value(test_data)
                self.stdout.write(self.style.SUCCESS(f"{description}: Success - {internal_data.get('license_expiry_date', 'None')}"))
            except Exception as e:
                self.stdout.write(self.style.WARNING(f"{description}: Expected error - {e}"))
        
        # Test 7: Real multipart form data with carriage returns
        self.stdout.write("\nTest 7: Real multipart form data simulation")
        
        # Simulate the exact data format from the curl request
        multipart_data = {
            'full_name': "Brian Onang'o",
            'license_number': '28581439',
            'license_expiry_date': '2025-12-12',  # This should work but was failing
            'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0',
            'id_number': '28581443',
            'phone_number': '+254706662011'
        }
        
        try:
            internal_data = serializer.to_internal_value(multipart_data)
            self.stdout.write(self.style.SUCCESS(f"Real multipart simulation: Success - {internal_data.get('license_expiry_date', 'None')}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Real multipart simulation: Error - {e}"))
        
        # Test 8: Date with carriage returns and newlines (simulating raw multipart)
        self.stdout.write("\nTest 8: Date with control characters")
        
        multipart_data_dirty = {
            'full_name': "Brian Onang'o",
            'license_number': '28581439',
            'license_expiry_date': '2025-12-12\r\n',  # With carriage return and newline
            'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0',
            'id_number': '28581443',
            'phone_number': '+254706662011'
        }
        
        try:
            internal_data = serializer.to_internal_value(multipart_data_dirty)
            self.stdout.write(self.style.SUCCESS(f"Dirty data: Success - {internal_data.get('license_expiry_date', 'None')}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"Dirty data: Error - {e}"))
        
        # Test 9: List input (the exact issue we discovered)
        self.stdout.write("\nTest 9: Date as list input (the actual problem)")
        
        multipart_data_list = {
            'full_name': "Brian Onang'o",
            'license_number': '28581439',
            'license_expiry_date': ['2025-12-12'],  # This is the format Django was sending
            'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0',
            'id_number': '28581443',
            'phone_number': '+254706662011'
        }
        
        try:
            internal_data = serializer.to_internal_value(multipart_data_list)
            self.stdout.write(self.style.SUCCESS(f"List input: Success - {internal_data.get('license_expiry_date', 'None')}"))
        except Exception as e:
            self.stdout.write(self.style.ERROR(f"List input: Error - {e}"))
        
        # Test 10: Empty list input
        self.stdout.write("\nTest 10: Empty list input")
        
        multipart_data_empty_list = {
            'full_name': "Brian Onang'o",
            'license_number': '28581439',
            'license_expiry_date': [],  # Empty list
            'license_classes': 'e01d791b-24c6-4cf4-b0ed-0e862859c5a0',
            'id_number': '28581443',
            'phone_number': '+254706662011'
        }
        
        try:
            internal_data = serializer.to_internal_value(multipart_data_empty_list)
            self.stdout.write(self.style.SUCCESS(f"Empty list: Success - {internal_data.get('license_expiry_date', 'None')}"))
        except Exception as e:
            self.stdout.write(self.style.WARNING(f"Empty list: Expected error - {e}"))
        
        self.stdout.write(self.style.SUCCESS('\nAll tests completed!'))