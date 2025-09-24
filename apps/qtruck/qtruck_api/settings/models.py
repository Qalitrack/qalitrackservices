import uuid
from django.db import models
from django.contrib.gis.db import models as gis_models
from django.db.models.signals import post_save, post_delete
from django.dispatch import receiver
from django.core.validators import FileExtensionValidator
from django.utils import timezone
from datetime import timedelta

# Import from users app
from users.models import BaseModel, CustomUser, UserProfile


class LicenseClass(BaseModel):
    name = models.CharField(max_length=100, unique=True, help_text="License class name (e.g., Class A CDL, Class B CDL, Regular License)")
    description = models.TextField(blank=True, help_text="Description of what this license allows")
    
    class Meta:
        ordering = ['name']
        verbose_name = "License Class"
        verbose_name_plural = "License Classes"
    
    def __str__(self):
        return self.name


class SystemSettings(BaseModel):
    tester_registration_enabled = models.BooleanField(default=True)
    tester_login_enabled = models.BooleanField(default=True)
    
    # Driver Profile Settings
    license_expiry_warning_days = models.PositiveIntegerField(
        default=30,
        help_text="Number of days before license expiry to show warning"
    )
    require_profile_photo = models.BooleanField(default=True)
    require_license_images = models.BooleanField(default=True)
    require_id_images = models.BooleanField(default=True)
    auto_approve_profile_updates = models.BooleanField(default=False)

    class Meta:
        verbose_name_plural = "System Settings"

    @classmethod
    def get_settings(cls):
        settings, created = cls.objects.get_or_create(pk=1)
        if created:
            # Create default license classes on first setup
            cls._create_default_license_classes()
        return settings
    
    @staticmethod
    def _create_default_license_classes():
        """Create default license classes"""
        default_classes = [
            ('Class A CDL', 'Heavy trucks, tractor-trailers'),
            ('Class B CDL', 'Large trucks, buses'),
            ('Class C License', 'Regular passenger vehicles'),
            ('Motorcycle License', 'Motorcycles'),
            ('Regular License', 'Standard passenger vehicles'),
            ('Commercial License', 'Commercial driving'),
        ]
        for name, desc in default_classes:
            LicenseClass.objects.get_or_create(
                name=name,
                defaults={'description': desc}
            )