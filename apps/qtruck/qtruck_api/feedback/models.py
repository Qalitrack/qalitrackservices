from django.db import models
from django.utils import timezone

# Import from users app
from users.models import BaseModel, CustomUser


class Feedback(BaseModel):
    FEEDBACK_TYPE_CHOICES = [
        ('bug_report', 'Bug Report'),
        ('feature_request', 'Feature Request'),
        ('general', 'General Feedback'),
        ('complaint', 'Complaint'),
        ('suggestion', 'Suggestion'),
    ]
    
    STATUS_CHOICES = [
        ('pending', 'Pending'),
        ('reviewed', 'Reviewed'),
        ('in_progress', 'In Progress'),
        ('resolved', 'Resolved'),
        ('rejected', 'Rejected'),
    ]
    
    user = models.ForeignKey(CustomUser, on_delete=models.CASCADE, related_name='feedbacks')
    feedback_type = models.CharField(max_length=20, choices=FEEDBACK_TYPE_CHOICES, default='general')
    subject = models.CharField(max_length=255)
    description = models.TextField()
    status = models.CharField(max_length=20, choices=STATUS_CHOICES, default='pending')
    admin_response = models.TextField(null=True, blank=True)
    responded_by = models.ForeignKey(CustomUser, on_delete=models.SET_NULL, null=True, blank=True, related_name='feedback_responses')
    response_date = models.DateTimeField(null=True, blank=True)

    def __str__(self):
        return f"{self.feedback_type}: {self.subject} by {self.user.username}"
    
    def respond(self, admin_user, response_text, status_update='reviewed'):
        """Method to respond to feedback"""
        self.admin_response = response_text
        self.responded_by = admin_user
        self.response_date = timezone.now()
        self.status = status_update
        self.save()
    
    def resolve(self, admin_user, response_text=''):
        """Method to resolve feedback"""
        if response_text:
            self.admin_response = response_text
        self.responded_by = admin_user
        self.response_date = timezone.now()
        self.status = 'resolved'
        self.save()
    
    def reject(self, admin_user, response_text=''):
        """Method to reject feedback"""
        if response_text:
            self.admin_response = response_text
        self.responded_by = admin_user
        self.response_date = timezone.now()
        self.status = 'rejected'
        self.save()

    class Meta:
        ordering = ['-created_at']