from django.urls import path
from .views import EmailAliasTokenObtainPairView, UserRegistrationView, CustomTokenRefreshView

urlpatterns = [
    # Auth URLs: /auth/
    path('register/', UserRegistrationView.as_view(), name='auth_register'),
    path('login/', EmailAliasTokenObtainPairView.as_view(), name='auth_login'),
    path('refresh/', CustomTokenRefreshView.as_view(), name='auth_refresh'),
]