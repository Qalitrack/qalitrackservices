from django.urls import path, include
from rest_framework.routers import DefaultRouter

from .views import TruckViewSet, MaterialViewSet, MaterialVariantViewSet, MaterialCostViewSet, MaterialPhotoViewSet, MaterialVariantPhotoViewSet

# Create router for fleet endpoints (trucks only, no trips)
fleet_router = DefaultRouter()
fleet_router.register(r'trucks', TruckViewSet)

# Create separate router for materials (used in /api/materials/)
materials_router = DefaultRouter()
# Register variants, costs, and photos first so they don't conflict with material detail patterns
materials_router.register(r'variants', MaterialVariantViewSet)
materials_router.register(r'variant-photos', MaterialVariantPhotoViewSet, basename='materialvariantphoto')
materials_router.register(r'costs', MaterialCostViewSet, basename='materialcost')
materials_router.register(r'photos', MaterialPhotoViewSet, basename='materialphoto')
materials_router.register(r'', MaterialViewSet, basename='material')

# Default fleet URLs (for /api/fleet/)
urlpatterns = [
    # Fleet endpoints (trucks)
    path('', include(fleet_router.urls)),
]