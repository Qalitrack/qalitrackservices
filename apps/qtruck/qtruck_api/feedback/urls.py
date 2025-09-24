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
# GET    /feedback/                    - List all feedback (filtered by user permissions)
# POST   /feedback/                    - Create new feedback
# GET    /feedback/{id}/               - Retrieve specific feedback
# PUT    /feedback/{id}/               - Update feedback (full update)
# PATCH  /feedback/{id}/               - Update feedback (partial update)  
# DELETE /feedback/{id}/               - Delete feedback
# POST   /feedback/{id}/respond/       - Admin responds to feedback
# POST   /feedback/{id}/resolve/       - Admin resolves feedback
# POST   /feedback/{id}/reject/        - Admin rejects feedback
# GET    /feedback/pending/            - Get all pending feedback (admin only)
# GET    /feedback/my_feedback/        - Get current user's feedback
# GET    /feedback/by_status/          - Get feedback filtered by status (admin only)