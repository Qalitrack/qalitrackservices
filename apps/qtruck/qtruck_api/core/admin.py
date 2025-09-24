from django.contrib import admin
from django.contrib.auth.admin import UserAdmin
from .models import (
    CustomUser, UserProfile, SystemSettings, Driver, Truck, Trip,
    Material, MaterialPhoto, MaterialCost, Expense, Receipt, VehicleMileage, Feedback
)


@admin.register(CustomUser)
class CustomUserAdmin(UserAdmin):
    list_display = ('email', 'username', 'user_type', 'is_approved', 'created_at')
    list_filter = ('user_type', 'is_approved', 'is_staff', 'is_active')
    search_fields = ('email', 'username', 'base_email')
    ordering = ('created_at',)
    
    fieldsets = UserAdmin.fieldsets + (
        ('Custom Fields', {
            'fields': ('user_type', 'base_email', 'is_approved', 'created_at', 'updated_at')
        }),
    )
    readonly_fields = ('created_at', 'updated_at')
    
    actions = ['approve_users', 'reject_users']
    
    def approve_users(self, request, queryset):
        updated = queryset.update(is_approved=True)
        self.message_user(request, f'{updated} users were approved.')
    approve_users.short_description = "Approve selected users"
    
    def reject_users(self, request, queryset):
        count = queryset.count()
        queryset.delete()
        self.message_user(request, f'{count} users were rejected and deleted.')
    reject_users.short_description = "Reject and delete selected users"


@admin.register(UserProfile)
class UserProfileAdmin(admin.ModelAdmin):
    list_display = ('user', 'approval_date', 'approved_by')
    list_filter = ('approval_date',)
    search_fields = ('user__email', 'user__username')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(SystemSettings)
class SystemSettingsAdmin(admin.ModelAdmin):
    list_display = ('tester_registration_enabled', 'tester_login_enabled', 'updated_at')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(Driver)
class DriverAdmin(admin.ModelAdmin):
    list_display = ('name', 'user', 'phone', 'license_number', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('name', 'user__email', 'license_number')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(Truck)
class TruckAdmin(admin.ModelAdmin):
    list_display = ('license_plate', 'model', 'driver', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('license_plate', 'model', 'driver__name')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(Trip)
class TripAdmin(admin.ModelAdmin):
    list_display = ('id', 'truck', 'driver', 'status', 'total_cost', 'date', 'created_at')
    list_filter = ('status', 'date', 'created_at')
    search_fields = ('truck__license_plate', 'driver__name', 'start_location', 'end_location')
    readonly_fields = ('total_cost', 'total_mileage', 'created_at', 'updated_at')
    date_hierarchy = 'date'


@admin.register(Material)
class MaterialAdmin(admin.ModelAdmin):
    list_display = ('name', 'description', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('name', 'description')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(MaterialPhoto)
class MaterialPhotoAdmin(admin.ModelAdmin):
    list_display = ('material', 'caption', 'created_at')
    list_filter = ('created_at',)
    search_fields = ('material__name', 'caption')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(MaterialCost)
class MaterialCostAdmin(admin.ModelAdmin):
    list_display = ('material', 'cost', 'location', 'synced', 'created_at')
    list_filter = ('synced', 'created_at')
    search_fields = ('material__name', 'location')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(Expense)
class ExpenseAdmin(admin.ModelAdmin):
    list_display = ('description', 'amount', 'trip', 'synced', 'created_at')
    list_filter = ('synced', 'created_at')
    search_fields = ('description', 'trip__driver__name')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(Receipt)
class ReceiptAdmin(admin.ModelAdmin):
    list_display = ('expense', 'note', 'synced', 'created_at')
    list_filter = ('synced', 'created_at')
    search_fields = ('expense__description', 'note')
    readonly_fields = ('created_at', 'updated_at')


@admin.register(VehicleMileage)
class VehicleMileageAdmin(admin.ModelAdmin):
    list_display = ('truck', 'driver', 'start_mileage', 'end_mileage', 'mileage', 'date', 'synced')
    list_filter = ('synced', 'date', 'created_at')
    search_fields = ('truck__license_plate', 'driver__name')
    readonly_fields = ('mileage', 'created_at', 'updated_at')
    date_hierarchy = 'date'


@admin.register(Feedback)
class FeedbackAdmin(admin.ModelAdmin):
    list_display = ('subject', 'feedback_type', 'user', 'status', 'created_at', 'response_date')
    list_filter = ('feedback_type', 'status', 'created_at')
    search_fields = ('subject', 'user__username', 'description')
    readonly_fields = ('created_at', 'updated_at')
    date_hierarchy = 'created_at'
    
    actions = ['mark_as_reviewed', 'mark_as_resolved']
    
    def mark_as_reviewed(self, request, queryset):
        updated = queryset.update(status='reviewed')
        self.message_user(request, f'{updated} feedback items marked as reviewed.')
    mark_as_reviewed.short_description = "Mark as reviewed"
    
    def mark_as_resolved(self, request, queryset):
        updated = queryset.update(status='resolved')
        self.message_user(request, f'{updated} feedback items marked as resolved.')
    mark_as_resolved.short_description = "Mark as resolved"