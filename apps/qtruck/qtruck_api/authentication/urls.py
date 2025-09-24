from django.urls import path
from rest_framework_simplejwt.views import TokenRefreshView
from .views import EmailAliasTokenObtainPairView, UserRegistrationView

urlpatterns = [
    # Auth URLs: /auth/
    path('register/', UserRegistrationView.as_view(), name='auth_register'),
    path('login/', EmailAliasTokenObtainPairView.as_view(), name='auth_login'),
    path('refresh/', TokenRefreshView.as_view(), name='auth_refresh'),
]