"""
Driver app configuration

This app handles all driver-related functionality including:
- Driver profiles and management
- Driver profile approval workflow
- Driver activity tracking and analytics
- Driver document management
"""

from django.apps import AppConfig


class DriversConfig(AppConfig):
    default_auto_field = 'django.db.models.BigAutoField'
    name = 'drivers'
    verbose_name = 'Driver Management'
    
    def ready(self):
        """Import signal handlers when the app is ready"""
        try:
            import drivers.models  # noqa F401
        except ImportError:
            pass