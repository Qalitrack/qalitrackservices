from django.contrib import admin
from django.utils.html import format_html
from django.utils import timezone
from .models import Feedback


@admin.register(Feedback)
class FeedbackAdmin(admin.ModelAdmin):
    list_display = [
        'subject', 'user', 'feedback_type', 'status', 
        'created_at', 'responded_by', 'response_date'
    ]
    list_filter = [
        'feedback_type', 'status', 'created_at', 'response_date'
    ]
    search_fields = [
        'subject', 'description', 'user__username', 
        'user__email', 'admin_response'
    ]
    readonly_fields = ['created_at', 'updated_at']
    ordering = ['-created_at']
    
    fieldsets = (
        ('Feedback Information', {
            'fields': (
                'user', 'feedback_type', 'subject', 'description',
                'status', 'created_at', 'updated_at'
            )
        }),
        ('Admin Response', {
            'fields': (
                'admin_response', 'responded_by', 'response_date'
            )
        }),
    )
    
    def get_readonly_fields(self, request, obj=None):
        readonly_fields = list(self.readonly_fields)
        if obj:  # Editing existing object
            readonly_fields.extend(['user', 'feedback_type'])
        return readonly_fields
    
    def save_model(self, request, obj, form, change):
        if change and 'admin_response' in form.changed_data:
            # If admin response was changed, update responded_by and response_date
            if not obj.responded_by:
                obj.responded_by = request.user
                obj.response_date = timezone.now()
        super().save_model(request, obj, form, change)
    
    actions = ['mark_as_reviewed', 'mark_as_resolved', 'mark_as_rejected']
    
    def mark_as_reviewed(self, request, queryset):
        updated = queryset.update(status='reviewed')
        self.message_user(request, f'{updated} feedback items marked as reviewed.')
    mark_as_reviewed.short_description = 'Mark selected feedback as reviewed'
    
    def mark_as_resolved(self, request, queryset):
        for feedback in queryset:
            feedback.resolve(request.user)
        self.message_user(request, f'{queryset.count()} feedback items marked as resolved.')
    mark_as_resolved.short_description = 'Mark selected feedback as resolved'
    
    def mark_as_rejected(self, request, queryset):
        for feedback in queryset:
            feedback.reject(request.user)
        self.message_user(request, f'{queryset.count()} feedback items marked as rejected.')
    mark_as_rejected.short_description = 'Mark selected feedback as rejected'