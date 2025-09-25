"""
Comprehensive test suite for the settings app.

Test scenarios covered:
1. SystemSettings model functionality
2. Auto-approval user types configuration
3. Default license classes creation
4. Settings singleton pattern
5. License class model functionality
"""

from django.test import TestCase
from settings.models import SystemSettings, LicenseClass


class SystemSettingsTestCase(TestCase):
    """Test SystemSettings model functionality"""

    def setUp(self):
        """Set up test environment"""
        SystemSettings.objects.all().delete()
        LicenseClass.objects.all().delete()

    def test_get_settings_creates_default_instance(self):
        """Test that get_settings() creates default instance if none exists"""
        # Ensure no settings exist
        self.assertEqual(SystemSettings.objects.count(), 0)
        
        # Get settings should create default instance
        settings = SystemSettings.get_settings()
        
        self.assertIsNotNone(settings)
        self.assertEqual(SystemSettings.objects.count(), 1)
        self.assertEqual(settings.pk, 1)
        
        # Check default values
        self.assertTrue(settings.tester_registration_enabled)
        self.assertTrue(settings.tester_login_enabled)
        self.assertEqual(settings.license_expiry_warning_days, 30)
        self.assertTrue(settings.require_profile_photo)
        self.assertTrue(settings.require_license_images)
        self.assertTrue(settings.require_id_images)
        self.assertFalse(settings.auto_approve_profile_updates)
        self.assertEqual(settings.auto_approve_user_types, [])

    def test_get_settings_returns_existing_instance(self):
        """Test that get_settings() returns existing instance"""
        # Create settings instance
        settings1 = SystemSettings.objects.create(
            tester_registration_enabled=False,
            auto_approve_user_types=['admin', 'tester']
        )
        
        # Get settings should return existing instance
        settings2 = SystemSettings.get_settings()
        
        self.assertEqual(settings1.id, settings2.id)
        self.assertFalse(settings2.tester_registration_enabled)
        self.assertEqual(settings2.auto_approve_user_types, ['admin', 'tester'])

    def test_default_license_classes_creation(self):
        """Test that default license classes are created on first setup"""
        # Ensure no license classes exist
        self.assertEqual(LicenseClass.objects.count(), 0)
        
        # Get settings for first time should create default license classes
        settings = SystemSettings.get_settings()
        
        # Check that default license classes were created
        expected_classes = [
            'Class A CDL',
            'Class B CDL', 
            'Class C License',
            'Motorcycle License',
            'Regular License',
            'Commercial License'
        ]
        
        self.assertEqual(LicenseClass.objects.count(), len(expected_classes))
        
        for class_name in expected_classes:
            self.assertTrue(
                LicenseClass.objects.filter(name=class_name).exists(),
                f"License class '{class_name}' was not created"
            )

    def test_license_classes_not_recreated_on_subsequent_calls(self):
        """Test that license classes are not duplicated on subsequent get_settings() calls"""
        # First call creates defaults
        settings1 = SystemSettings.get_settings()
        initial_count = LicenseClass.objects.count()
        
        # Second call should not create more
        settings2 = SystemSettings.get_settings()
        final_count = LicenseClass.objects.count()
        
        self.assertEqual(initial_count, final_count)
        self.assertEqual(settings1.id, settings2.id)

    def test_auto_approve_user_types_field(self):
        """Test auto_approve_user_types JSONField functionality"""
        # Test empty list
        settings = SystemSettings.objects.create(
            auto_approve_user_types=[]
        )
        self.assertEqual(settings.auto_approve_user_types, [])
        
        # Test with user types
        settings.auto_approve_user_types = ['admin', 'tester']
        settings.save()
        
        # Refresh from database
        settings.refresh_from_db()
        self.assertEqual(settings.auto_approve_user_types, ['admin', 'tester'])
        
        # Test adding more user types
        settings.auto_approve_user_types.append('driver')
        settings.save()
        
        settings.refresh_from_db()
        self.assertEqual(settings.auto_approve_user_types, ['admin', 'tester', 'driver'])

    def test_settings_singleton_behavior(self):
        """Test that only one SystemSettings instance can effectively be used"""
        # Create first settings
        settings1 = SystemSettings.objects.create(
            tester_registration_enabled=False
        )
        
        # Create second settings (should be possible but get_settings always returns pk=1)
        settings2 = SystemSettings.objects.create(
            tester_registration_enabled=True
        )
        
        # get_settings() should always return the one with pk=1
        retrieved_settings = SystemSettings.get_settings()
        self.assertEqual(retrieved_settings.pk, 1)
        self.assertEqual(retrieved_settings.id, settings1.id)

    def test_settings_verbose_names(self):
        """Test SystemSettings verbose names"""
        meta = SystemSettings._meta
        self.assertEqual(meta.verbose_name_plural, "System Settings")

    def test_all_settings_fields_have_defaults(self):
        """Test that all settings fields have appropriate defaults"""
        settings = SystemSettings()
        
        # Test that we can save without setting any fields (all have defaults)
        settings.save()
        
        # Verify defaults
        self.assertTrue(settings.tester_registration_enabled)
        self.assertTrue(settings.tester_login_enabled)
        self.assertEqual(settings.license_expiry_warning_days, 30)
        self.assertTrue(settings.require_profile_photo)
        self.assertTrue(settings.require_license_images)
        self.assertTrue(settings.require_id_images)
        self.assertFalse(settings.auto_approve_profile_updates)
        self.assertEqual(settings.auto_approve_user_types, [])


class LicenseClassTestCase(TestCase):
    """Test LicenseClass model functionality"""

    def setUp(self):
        """Set up test environment"""
        LicenseClass.objects.all().delete()

    def test_license_class_creation(self):
        """Test creating license classes"""
        license_class = LicenseClass.objects.create(
            name="Test License",
            description="Test license description"
        )
        
        self.assertEqual(license_class.name, "Test License")
        self.assertEqual(license_class.description, "Test license description")

    def test_license_class_name_uniqueness(self):
        """Test that license class names are unique"""
        LicenseClass.objects.create(name="Unique License")
        
        # Should raise an error when trying to create another with the same name
        with self.assertRaises(Exception):  # IntegrityError
            LicenseClass.objects.create(name="Unique License")

    def test_license_class_str_method(self):
        """Test LicenseClass string representation"""
        license_class = LicenseClass.objects.create(name="CDL Class A")
        self.assertEqual(str(license_class), "CDL Class A")

    def test_license_class_ordering(self):
        """Test that license classes are ordered by name"""
        # Create classes in reverse alphabetical order
        class_c = LicenseClass.objects.create(name="Class C")
        class_a = LicenseClass.objects.create(name="Class A") 
        class_b = LicenseClass.objects.create(name="Class B")
        
        # Should be returned in alphabetical order
        classes = list(LicenseClass.objects.all())
        self.assertEqual(classes[0].name, "Class A")
        self.assertEqual(classes[1].name, "Class B") 
        self.assertEqual(classes[2].name, "Class C")

    def test_license_class_verbose_names(self):
        """Test LicenseClass verbose names"""
        meta = LicenseClass._meta
        self.assertEqual(meta.verbose_name, "License Class")
        self.assertEqual(meta.verbose_name_plural, "License Classes")

    def test_license_class_description_optional(self):
        """Test that description field is optional"""
        license_class = LicenseClass.objects.create(name="Basic License")
        self.assertEqual(license_class.description, "")

    def test_license_class_help_text(self):
        """Test that help text is properly set on fields"""
        name_field = LicenseClass._meta.get_field('name')
        description_field = LicenseClass._meta.get_field('description')
        
        self.assertIn("License class name", name_field.help_text)
        self.assertIn("Description of what this license allows", description_field.help_text)

    def test_default_license_classes_content(self):
        """Test the content and structure of default license classes"""
        # Trigger creation of default license classes
        SystemSettings.get_settings()
        
        # Test specific license classes and their descriptions
        class_a_cdl = LicenseClass.objects.get(name='Class A CDL')
        self.assertIn('Heavy trucks', class_a_cdl.description)
        self.assertIn('tractor-trailers', class_a_cdl.description)
        
        class_b_cdl = LicenseClass.objects.get(name='Class B CDL')
        self.assertIn('Large trucks', class_b_cdl.description)
        self.assertIn('buses', class_b_cdl.description)
        
        motorcycle = LicenseClass.objects.get(name='Motorcycle License')
        self.assertIn('Motorcycles', motorcycle.description)

    def test_get_or_create_license_classes_idempotent(self):
        """Test that creating license classes multiple times doesn't create duplicates"""
        # Call the creation method multiple times
        SystemSettings._create_default_license_classes()
        initial_count = LicenseClass.objects.count()
        
        SystemSettings._create_default_license_classes()
        SystemSettings._create_default_license_classes()
        final_count = LicenseClass.objects.count()
        
        # Count should remain the same
        self.assertEqual(initial_count, final_count)
        
        # Should have the expected number of default classes
        expected_count = 6  # Based on the default classes defined
        self.assertEqual(final_count, expected_count)


class SettingsIntegrationTestCase(TestCase):
    """Integration tests for settings functionality"""

    def setUp(self):
        """Set up test environment"""
        SystemSettings.objects.all().delete()
        LicenseClass.objects.all().delete()

    def test_complete_settings_workflow(self):
        """Test complete settings workflow"""
        print("\n=== INTEGRATION TEST: Complete Settings Workflow ===")
        
        # 1. Initial state - no settings exist
        print("1. Initial state check...")
        self.assertEqual(SystemSettings.objects.count(), 0)
        self.assertEqual(LicenseClass.objects.count(), 0)
        print("   ✓ No settings or license classes exist initially")
        
        # 2. First get_settings() call creates everything
        print("2. First get_settings() call...")
        settings = SystemSettings.get_settings()
        print(f"   ✓ Settings created with ID: {settings.id}")
        print(f"   ✓ License classes created: {LicenseClass.objects.count()}")
        
        # 3. Verify default license classes
        print("3. Verifying default license classes...")
        expected_classes = [
            'Class A CDL',
            'Class B CDL', 
            'Class C License',
            'Motorcycle License',
            'Regular License',
            'Commercial License'
        ]
        
        for class_name in expected_classes:
            self.assertTrue(LicenseClass.objects.filter(name=class_name).exists())
        print(f"   ✓ All {len(expected_classes)} default license classes created")
        
        # 4. Modify auto-approval settings
        print("4. Modifying auto-approval settings...")
        settings.auto_approve_user_types = ['admin', 'tester']
        settings.tester_registration_enabled = False
        settings.license_expiry_warning_days = 45
        settings.save()
        print("   ✓ Settings updated")
        
        # 5. Subsequent get_settings() calls return same instance
        print("5. Testing settings persistence...")
        settings2 = SystemSettings.get_settings()
        self.assertEqual(settings.id, settings2.id)
        self.assertEqual(settings2.auto_approve_user_types, ['admin', 'tester'])
        self.assertFalse(settings2.tester_registration_enabled)
        self.assertEqual(settings2.license_expiry_warning_days, 45)
        print("   ✓ Settings persisted correctly")
        
        # 6. License classes remain unchanged
        print("6. Verifying license classes unchanged...")
        final_license_count = LicenseClass.objects.count()
        self.assertEqual(final_license_count, len(expected_classes))
        print(f"   ✓ License classes remain stable: {final_license_count}")
        
        print("\n=== SETTINGS INTEGRATION TEST PASSED ===\n")

    def test_settings_and_user_creation_integration(self):
        """Test integration between settings and user creation"""
        print("\n=== INTEGRATION TEST: Settings + User Creation ===")
        
        # Import here to avoid circular imports in test
        from users.models import CustomUser
        
        # 1. Set up auto-approval for testers
        print("1. Setting up auto-approval...")
        settings = SystemSettings.get_settings()
        settings.auto_approve_user_types = ['tester']
        settings.save()
        print("   ✓ Auto-approval configured for testers")
        
        # 2. Create admin first
        print("2. Creating first admin...")
        admin = CustomUser.objects.create_user(
            email='admin+admin@example.com',
            password='pass123'
        )
        self.assertEqual(admin.status, 'approved')  # First user always approved
        print("   ✓ Admin created and approved")
        
        # 3. Create tester (should be auto-approved due to settings)
        print("3. Creating tester (should auto-approve)...")
        tester = CustomUser.objects.create_user(
            email='tester+tester@example.com',
            password='pass123'
        )
        self.assertEqual(tester.status, 'approved')  # Should be auto-approved
        print("   ✓ Tester auto-approved based on settings")
        
        # 4. Create driver (should not be auto-approved)
        print("4. Creating driver (should not auto-approve)...")
        driver = CustomUser.objects.create_user(
            email='driver+driver@example.com',
            password='pass123'
        )
        self.assertEqual(driver.status, 'preapproval')  # Should remain pending
        print("   ✓ Driver not auto-approved (correctly)")
        
        # 5. Change settings to auto-approve drivers too
        print("5. Updating settings to include drivers...")
        settings.auto_approve_user_types = ['tester', 'driver']
        settings.save()
        
        # 6. Create another driver (should now be auto-approved)
        print("6. Creating another driver (should now auto-approve)...")
        driver2 = CustomUser.objects.create_user(
            email='driver2+driver@example.com',
            password='pass123'
        )
        self.assertEqual(driver2.status, 'approved')  # Should be auto-approved now
        print("   ✓ Second driver auto-approved after settings change")
        
        print("\n=== SETTINGS + USER CREATION INTEGRATION TEST PASSED ===\n")
