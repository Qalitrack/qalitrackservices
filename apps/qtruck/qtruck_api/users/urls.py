from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import UserViewSet

router = DefaultRouter()
router.register(r'', UserViewSet, basename='users')

urlpatterns = [
    # RESTful endpoints via UserViewSet:
    # GET    /users/                          - List users (with query filters: ?status=pending&user_type=driver)
    # POST   /users/                          - Create admin user (admin only)
    # GET    /users/{id}/                     - Get specific user
    # PUT    /users/{id}/                     - Update user (full update)
    # PATCH  /users/{id}/                     - Update user (partial update, includes approve/reject/activate/deactivate)
    #                                          Use: {"is_approved": true/false} for approve/reject
    #                                          Use: {"is_active": true/false} for activate/deactivate
    # DELETE /users/{id}/                     - Delete user (admin only, deactivated/rejected users only)
    # GET    /users/me/                       - Get current user info
    
    path('', include(router.urls)),
]