from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TripViewSet, ExpenseViewSet, ReceiptViewSet, VehicleMileageViewSet

# Use a single router with proper ordering to avoid conflicts
router = DefaultRouter()
# Register expenses first to avoid conflicts with the empty string route
router.register('expenses', ExpenseViewSet, basename='expenses')  
router.register('receipts', ReceiptViewSet, basename='receipts')
router.register('vehicle-mileage', VehicleMileageViewSet, basename='vehicle-mileage')
# Register trips last with empty string
router.register('', TripViewSet, basename='trips')

urlpatterns = [
    path('', include(router.urls)),
]