from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TripViewSet, TripTypeViewSet, ExpenseViewSet, ReceiptViewSet, VehicleMileageViewSet, TripMaterialViewSet

# Use a single router with proper ordering to avoid conflicts
router = DefaultRouter()
# Register trip types first
router.register('trip-types', TripTypeViewSet, basename='trip-types')
# Register trip materials
router.register('trip-materials', TripMaterialViewSet, basename='trip-materials')
# Register expenses first to avoid conflicts with the empty string route
router.register('expenses', ExpenseViewSet, basename='expenses')
router.register('receipts', ReceiptViewSet, basename='receipts')
router.register('vehicle-mileage', VehicleMileageViewSet, basename='vehicle-mileage')
# Register trips last with empty string
router.register('', TripViewSet, basename='trips')

urlpatterns = [
    path('', include(router.urls)),
]