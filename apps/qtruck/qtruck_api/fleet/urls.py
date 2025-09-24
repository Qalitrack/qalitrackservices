from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TruckViewSet, MaterialViewSet, MaterialCostViewSet

# Create router for fleet endpoints
router = DefaultRouter()
router.register(r'trucks', TruckViewSet)
router.register(r'materials', MaterialViewSet)
router.register(r'material-costs', MaterialCostViewSet)

urlpatterns = [
    # Fleet API endpoints at /fleet/
    path('', include(router.urls)),
    
    # Include trips endpoints at /fleet/trips/
    path('trips/', include('trips.urls')),
]