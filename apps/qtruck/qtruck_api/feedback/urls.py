from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import FeedbackViewSet

router = DefaultRouter()
router.register(r'', FeedbackViewSet, basename='feedback')

urlpatterns = [
    # Feedback API endpoints
    path('', include(router.urls)),
]

# URL patterns created by the router:
# GET    /feedback/                    - List all feedback with advanced filtering
# POST   /feedback/                    - Create new feedback
# GET    /feedback/{id}/               - Retrieve specific feedback
# PUT    /feedback/{id}/               - Update feedback (full update)
# PATCH  /feedback/{id}/               - Update feedback (partial update)  
# DELETE /feedback/{id}/               - Delete feedback
# POST   /feedback/{id}/respond/       - Admin responds to feedback
# POST   /feedback/{id}/resolve/       - Admin resolves feedback
# POST   /feedback/{id}/reject/        - Admin rejects feedback
# GET    /feedback/my_feedback/        - Get current user's feedback (shortcut)
#
# Advanced Filtering Examples:
# GET    /feedback/?status=pending                     - Filter by status (admin only)
# GET    /feedback/?feedback_type=bug_report          - Filter by type
# GET    /feedback/?search=login                       - Search subject/description
# GET    /feedback/?created_after=2024-01-01          - Date filtering
# GET    /feedback/?has_response=false                 - Response status (admin only)
# GET    /feedback/?include=response_details          - Include enhanced data
# GET    /feedback/?status=pending&feedback_type=bug_report&search=login - Combined filters