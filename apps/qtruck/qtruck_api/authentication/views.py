from rest_framework import permissions, status
from rest_framework.response import Response
from rest_framework.views import APIView
from rest_framework_simplejwt.views import TokenObtainPairView
from drf_spectacular.utils import extend_schema
from users.serializers import CustomUserSerializer
from .serializers import EmailAliasTokenObtainPairSerializer, UserRegistrationSerializer


class EmailAliasTokenObtainPairView(TokenObtainPairView):
    serializer_class = EmailAliasTokenObtainPairSerializer
    
    @extend_schema(
        summary="Login with email alias",
        description="Login using email (with optional role alias like user+driver@domain.com)",
        tags=["Authentication"]
    )
    def post(self, request, *args, **kwargs):
        return super().post(request, *args, **kwargs)


class UserRegistrationView(APIView):
    permission_classes = [permissions.AllowAny]
    
    @extend_schema(
        request=UserRegistrationSerializer,
        responses={201: CustomUserSerializer},
        tags=["Authentication"]
    )
    def post(self, request):
        serializer = UserRegistrationSerializer(data=request.data)
        if serializer.is_valid():
            user = serializer.save()
            return Response(
                CustomUserSerializer(user).data, 
                status=status.HTTP_201_CREATED
            )
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)