"""
Driver-related admin configurations for the drivers app

This module contains all driver-related admin configurations migrated from the settings app:
- DriverAdmin: Basic driver administration
- DriverProfileAdmin: Driver profile management with approval workflow
- DriverActivityAdmin: Driver activity tracking
- DriverProfileChangeAdmin: Profile change tracking
"""

from django.contrib import admin
from django.utils.html import format_html
from django.urls import reverse
from django.utils.safestring import mark_safe

from .models import Driver, DriverProfile, DriverActivity, DriverProfileChange


@admin.register(Driver)
class DriverAdmin(admin.ModelAdmin):
    """Admin configuration for Driver model"""
    list_display = ('name', 'user', 'phone', 'license_number', 'current_profile_status', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('name', 'user__email', 'license_number')
    readonly_fields = ('created_at', 'updated_at')
    
    def current_profile_status(self, obj):
        """Show current profile status"""
        current_profile = obj.current_profile
        if current_profile:
            color = {
                'approved': 'green',
                'pending': 'orange',
                'rejected': 'red',
                'draft': 'gray'
            }.get(current_profile.status, 'gray')
            return format_html(
                '<span style="color: {};">{}</span>',
                color,
                current_profile.get_status_display()
            )
        return format_html('<span style="color: red;">No Profile</span>')
    
    current_profile_status.short_description = 'Current Profile Status'


@admin.register(DriverProfile)
class DriverProfileAdmin(admin.ModelAdmin):
    """Admin configuration for DriverProfile model"""
    list_display = (
        'driver_name', 'full_name', 'version_number', 'status', 'is_current',
        'license_expiry_date', 'license_status', 'submitted_at', 'reviewed_by'
    )
    list_filter = (
        'status', 'is_current', 'license_expiry_date', 'created_at', 'reviewed_at'
    )
    search_fields = (
        'driver__name', 'driver__user__email', 'full_name', 
        'phone_number', 'license_number', 'id_number'
    )
    readonly_fields = (
        'id', 'driver', 'version_number', 'created_at', 'updated_at', 
        'submitted_at', 'reviewed_at', 'reviewed_by', 'image_previews'
    )
    
    fieldsets = (
        ('Basic Information', {
            'fields': ('id', 'driver', 'version_number', 'status', 'is_current')
        }),
        ('Personal Details', {
            'fields': ('full_name', 'phone_number', 'id_number')
        }),
        ('License Information', {
            'fields': ('license_number', 'license_expiry_date', 'license_classes')
        }),
        ('Document Uploads', {
            'fields': ('image_previews', 'profile_photo', 'license_front_image', 'license_back_image', 'id_front_image', 'id_back_image')
        }),
        ('Approval Workflow', {
            'fields': ('submitted_at', 'reviewed_at', 'reviewed_by', 'approval_notes', 'rejection_reason')
        }),
        ('Timestamps', {
            'fields': ('created_at', 'updated_at'),
            'classes': ('collapse',)
        })
    )
    
    actions = ['approve_profiles', 'mark_as_current']
    
    def driver_name(self, obj):
        """Get driver name"""
        return obj.driver.name
    driver_name.short_description = 'Driver'
    
    def license_status(self, obj):
        """Show license status with color coding"""
        if obj.is_license_expired:
            return format_html('<span style="color: red;">Expired</span>')
        elif obj.is_license_expiring_soon():
            return format_html('<span style="color: orange;">Expiring Soon</span>')
        else:
            return format_html('<span style="color: green;">Valid</span>')
    
    license_status.short_description = 'License Status'
    
    def image_previews(self, obj):
        """Show image previews in admin"""
        html_parts = []
        
        image_fields = [
            ('profile_photo', 'Profile Photo'),
            ('license_front_image', 'License Front'),
            ('license_back_image', 'License Back'),
            ('id_front_image', 'ID Front'),
            ('id_back_image', 'ID Back')
        ]
        
        for field_name, label in image_fields:
            image_field = getattr(obj, field_name)
            if image_field:
                html_parts.append(f'''
                    <div style="display: inline-block; margin: 10px; text-align: center;">
                        <div><strong>{label}</strong></div>
                        <img src="{image_field.url}" style="max-width: 150px; max-height: 150px; border: 1px solid #ccc;">
                    </div>
                ''')
            else:
                html_parts.append(f'''
                    <div style="display: inline-block; margin: 10px; text-align: center; color: #999;">
                        <div><strong>{label}</strong></div>
                        <div style="width: 150px; height: 100px; border: 1px solid #ccc; display: flex; align-items: center; justify-content: center;">
                            No image
                        </div>
                    </div>
                ''')
        
        return mark_safe(''.join(html_parts)) if html_parts else 'No images'
    
    image_previews.short_description = 'Document Images'
    
    def approve_profiles(self, request, queryset):
        """Bulk approve profiles"""
        approved_count = 0
        for profile in queryset.filter(status='pending'):
            profile.approve(request.user, "Bulk approved via admin")
            approved_count += 1
        
        self.message_user(request, f'{approved_count} profiles were approved.')
    
    approve_profiles.short_description = "Approve selected profiles"
    
    def mark_as_current(self, request, queryset):
        """Mark selected profiles as current (if approved)"""
        updated_count = 0
        for profile in queryset.filter(status='approved'):
            # Deactivate other current profiles for the same driver
            DriverProfile.objects.filter(driver=profile.driver, is_current=True).update(is_current=False)
            profile.is_current = True
            profile.save()
            updated_count += 1
        
        self.message_user(request, f'{updated_count} profiles marked as current.')
    
    mark_as_current.short_description = "Mark as current profile"


@admin.register(DriverActivity)
class DriverActivityAdmin(admin.ModelAdmin):
    """Admin configuration for DriverActivity model"""
    list_display = ('driver_name', 'activity_type', 'activity_type_display', 'created_at', 'ip_address')
    list_filter = ('activity_type', 'created_at')
    search_fields = ('driver__name', 'driver__user__email', 'activity_type', 'ip_address')
    readonly_fields = ('id', 'driver', 'activity_type', 'created_at', 'activity_data', 'ip_address', 'user_agent')
    
    date_hierarchy = 'created_at'
    
    def driver_name(self, obj):
        """Get driver name"""
        return obj.driver.name
    driver_name.short_description = 'Driver'
    
    def activity_type_display(self, obj):
        """Show activity type display name"""
        return obj.get_activity_type_display()
    activity_type_display.short_description = 'Activity'
    
    def has_add_permission(self, request):
        """Disable manual addition of activities"""
        return False


@admin.register(DriverProfileChange)
class DriverProfileChangeAdmin(admin.ModelAdmin):
    """Admin configuration for DriverProfileChange model"""
    list_display = (
        'driver_name', 'field_name', 'change_type', 'old_value_preview', 
        'new_value_preview', 'created_at'
    )
    list_filter = ('change_type', 'field_name', 'created_at')
    search_fields = (
        'new_profile__driver__name', 'field_name', 
        'old_value', 'new_value'
    )
    readonly_fields = (
        'id', 'old_profile', 'new_profile', 'field_name', 
        'old_value', 'new_value', 'change_type', 'created_at'
    )
    
    date_hierarchy = 'created_at'
    
    def driver_name(self, obj):
        """Get driver name"""
        return obj.new_profile.driver.name
    driver_name.short_description = 'Driver'
    
    def old_value_preview(self, obj):
        """Show preview of old value"""
        if obj.old_value:
            return obj.old_value[:50] + '...' if len(obj.old_value) > 50 else obj.old_value
        return '-'
    old_value_preview.short_description = 'Old Value'
    
    def new_value_preview(self, obj):
        """Show preview of new value"""
        if obj.new_value:
            return obj.new_value[:50] + '...' if len(obj.new_value) > 50 else obj.new_value
        return '-'
    new_value_preview.short_description = 'New Value'
    
    def has_add_permission(self, request):
        """Disable manual addition of changes"""
        return False