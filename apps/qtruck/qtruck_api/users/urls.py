from django.urls import path, include
from rest_framework.routers import DefaultRouter
from .views import (
    UserApprovalView, PendingUsersView, UsersByTypeView, CreateAdminView,
    UserDeactivateView, UserReactivateView, UserDeleteView, UserProfileViewSet, user_info
)

router = DefaultRouter()
router.register(r'profiles', UserProfileViewSet, basename='user-profiles')

urlpatterns = [
    # User management URLs: /users/
    path('me/', user_info, name='user_info'),
    path('pending/', PendingUsersView.as_view(), name='pending_users'),
    path('create-admin/', CreateAdminView.as_view(), name='create_admin'),
    
    # User actions - these need to be after 'create-admin' to avoid conflicts
    path('approve/', UserApprovalView.as_view(), name='approve_user'),  # Changed to use POST data for user_id
    path('activate/', UserReactivateView.as_view(), name='activate_user'),
    path('deactivate/', UserDeactivateView.as_view(), name='deactivate_user'), 
    path('delete/', UserDeleteView.as_view(), name='delete_user'),
    
    # Include router URLs
    path('', include(router.urls)),
    
    # Users list - this should be last to avoid conflicts
    path('', UsersByTypeView.as_view(), name='users_by_type'),
]