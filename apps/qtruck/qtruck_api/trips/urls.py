from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TripViewSet, ExpenseViewSet, ReceiptViewSet, VehicleMileageViewSet

# Create router for trips endpoints (will be nested under /fleet/trips/)
router = DefaultRouter()
router.register(r'', TripViewSet, basename='trips')  # /fleet/trips/
router.register(r'expenses', ExpenseViewSet)  # /fleet/trips/expenses/
router.register(r'receipts', ReceiptViewSet)  # /fleet/trips/receipts/
router.register(r'vehicle-mileage', VehicleMileageViewSet)  # /fleet/trips/vehicle-mileage/

urlpatterns = [
    # Trips API endpoints (will be available at /fleet/trips/)
    path('', include(router.urls)),
]