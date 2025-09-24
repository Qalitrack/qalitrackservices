from django.contrib import admin
from .models import SystemSettings, LicenseClass


@admin.register(SystemSettings)
class SystemSettingsAdmin(admin.ModelAdmin):
    list_display = ('tester_registration_enabled', 'tester_login_enabled', 'updated_at')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(LicenseClass)
class LicenseClassAdmin(admin.ModelAdmin):
    list_display = ('name', 'description', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('name', 'description')
    readonly_fields = ('created_at', 'updated_at')