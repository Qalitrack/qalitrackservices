from django.urls import path, include
from rest_framework.routers import DefaultRouter
from rest_framework_simplejwt.views import TokenRefreshView

from .views import (
    UserRegistrationView, UserApprovalView, PendingUsersView, UsersByTypeView, CreateAdminView,
    SystemSettingsViewSet, DriverViewSet, TruckViewSet, TripViewSet,
    MaterialViewSet, MaterialCostViewSet, ExpenseViewSet, ReceiptViewSet,
    VehicleMileageViewSet, UserProfileViewSet, FeedbackViewSet, user_info,
    flush_database
)
from .authentication import EmailAliasTokenObtainPairView

router = DefaultRouter()
router.register(r'system-settings', SystemSettingsViewSet)
router.register(r'drivers', DriverViewSet)
router.register(r'trucks', TruckViewSet)
router.register(r'trips', TripViewSet)
router.register(r'materials', MaterialViewSet)
router.register(r'material-costs', MaterialCostViewSet)
router.register(r'expenses', ExpenseViewSet)
router.register(r'receipts', ReceiptViewSet)
router.register(r'vehicle-mileage', VehicleMileageViewSet)
router.register(r'user-profiles', UserProfileViewSet)
router.register(r'feedback', FeedbackViewSet)

urlpatterns = [
    # Authentication
    path('auth/register/', UserRegistrationView.as_view(), name='register'),
    path('auth/login/', EmailAliasTokenObtainPairView.as_view(), name='token_obtain_pair'),
    path('auth/refresh/', TokenRefreshView.as_view(), name='token_refresh'),
    path('auth/user-info/', user_info, name='user_info'),
    
    # Admin functions  
    path('api/admin/approve-user/', UserApprovalView.as_view(), name='approve_user'),
    path('api/admin/pending-users/', PendingUsersView.as_view(), name='pending_users'),
    path('api/admin/users/', UsersByTypeView.as_view(), name='users_by_type'),
    path('api/admin/create-admin/', CreateAdminView.as_view(), name='create_admin'),
    path('api/admin/flush-database/', flush_database, name='flush_database'),
    
    # API endpoints
    path('api/', include(router.urls)),
]