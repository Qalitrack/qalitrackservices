from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TripViewSet, ExpenseViewSet, ReceiptViewSet, VehicleMileageViewSet

# Create router for trips endpoints
router = DefaultRouter()
router.register('trips', TripViewSet, basename='trips')  # /api/trips/trips/
router.register('expenses', ExpenseViewSet, basename='expenses')  # /api/trips/expenses/
router.register('receipts', ReceiptViewSet, basename='receipts')  # /api/trips/receipts/
router.register('vehicle-mileage', VehicleMileageViewSet, basename='vehicle-mileage')  # /api/trips/vehicle-mileage/

urlpatterns = [
    # Trips API endpoints (will be available at /fleet/trips/)
    path('', include(router.urls)),
]