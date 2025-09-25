from rest_framework import viewsets, status, permissions, filters
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from django.utils import timezone
from django_filters.rest_framework import DjangoFilterBackend
from django_filters import rest_framework as django_filters
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import Feedback
from .serializers import (
    FeedbackSerializer, FeedbackResponseSerializer,
    FeedbackRespondSerializer, FeedbackResolveSerializer, FeedbackRejectSerializer
)
from settings.permissions import IsAdmin, IsDriverOrTester, IsApproved, IsAdminOrDriverOrTester


class FeedbackFilterSet(django_filters.FilterSet):
    """Advanced filtering for Feedback"""
    status = django_filters.MultipleChoiceFilter(
        choices=Feedback.STATUS_CHOICES,
        help_text="Filter by status (admin only): pending, reviewed, in_progress, resolved, rejected"
    )
    feedback_type = django_filters.MultipleChoiceFilter(
        choices=Feedback.FEEDBACK_TYPE_CHOICES,
        help_text="Filter by type: bug_report, feature_request, general, complaint, suggestion"
    )
    created_after = django_filters.DateFilter(
        field_name='created_at',
        lookup_expr='gte',
        help_text="Filter feedback created after this date (YYYY-MM-DD)"
    )
    created_before = django_filters.DateFilter(
        field_name='created_at',
        lookup_expr='lte',
        help_text="Filter feedback created before this date (YYYY-MM-DD)"
    )
    responded_after = django_filters.DateFilter(
        field_name='response_date',
        lookup_expr='gte',
        help_text="Filter feedback responded after this date (YYYY-MM-DD)"
    )
    responded_before = django_filters.DateFilter(
        field_name='response_date',
        lookup_expr='lte',
        help_text="Filter feedback responded before this date (YYYY-MM-DD)"
    )
    has_response = django_filters.BooleanFilter(
        field_name='admin_response',
        lookup_expr='isnull',
        exclude=True,
        help_text="Filter by response status: true (has response), false (no response)"
    )
    responded_by = django_filters.UUIDFilter(
        field_name='responded_by',
        help_text="Filter by admin who responded (admin only)"
    )
    
    class Meta:
        model = Feedback
        fields = {
            'subject': ['icontains'],
            'description': ['icontains'],
        }


@extend_schema_view(
    list=extend_schema(tags=["Feedback"]),
    retrieve=extend_schema(tags=["Feedback"]),
    create=extend_schema(tags=["Feedback"]),
    update=extend_schema(tags=["Feedback"]),
    partial_update=extend_schema(tags=["Feedback"]),
    destroy=extend_schema(tags=["Feedback"]),
    respond=extend_schema(tags=["Feedback"]),
    resolve=extend_schema(tags=["Feedback"]),
    reject=extend_schema(tags=["Feedback"]),
    me=extend_schema(tags=["Feedback"])
)
class FeedbackViewSet(viewsets.ModelViewSet):
    queryset = Feedback.objects.all().select_related('user', 'responded_by')
    serializer_class = FeedbackSerializer
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsAdminOrDriverOrTester]
    filter_backends = [DjangoFilterBackend, filters.SearchFilter, filters.OrderingFilter]
    filterset_class = FeedbackFilterSet
    search_fields = ['subject', 'description']
    ordering_fields = ['created_at', 'response_date', 'status']
    ordering = ['-created_at']
    
    def get_queryset(self):
        queryset = super().get_queryset()
        
        # Non-admins can only see their own feedback
        if self.request.user.user_type != 'admin':
            queryset = queryset.filter(user=self.request.user)
        
        return queryset
    
    def filter_queryset(self, queryset):
        # Handle admin-only filters for non-admins
        if self.request.user.user_type != 'admin':
            # Create a mutable copy of query parameters
            query_params = self.request.query_params.copy()
            
            # Remove admin-only filter parameters for non-admins
            admin_only_filters = ['status', 'has_response', 'responded_by']
            for param in admin_only_filters:
                if param in query_params:
                    del query_params[param]
            
            # Temporarily replace query_params to exclude admin-only filters
            original_query_params = self.request.query_params
            self.request._request.GET = query_params
            queryset = super().filter_queryset(queryset)
            self.request._request.GET = original_query_params
            
            return queryset
        else:
            return super().filter_queryset(queryset)
    
    def perform_create(self, serializer):
        serializer.save(user=self.request.user)
    
    def get_serializer(self, *args, **kwargs):
        serializer = super().get_serializer(*args, **kwargs)
        
        # Handle include parameter for enhanced data
        include_params = self.request.query_params.get('include', '').split(',') if hasattr(self, 'request') else []
        include_params = [param.strip() for param in include_params if param.strip()]
        
        if include_params:
            serializer.context['include'] = include_params
            
        return serializer
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    def respond(self, request, pk=None):
        """Admin response to feedback"""
        feedback = self.get_object()
        serializer = FeedbackRespondSerializer(data=request.data)
        
        if serializer.is_valid():
            admin_response = serializer.validated_data['admin_response']
            status_update = serializer.validated_data.get('status', 'reviewed')
            
            feedback.respond(request.user, admin_response, status_update)
            
            return Response({
                'message': 'Feedback response saved',
                'feedback': FeedbackSerializer(feedback).data
            })
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    def resolve(self, request, pk=None):
        """Mark feedback as resolved"""
        feedback = self.get_object()
        serializer = FeedbackResolveSerializer(data=request.data)
        
        if serializer.is_valid():
            admin_response = serializer.validated_data.get('admin_response', '')
            
            feedback.resolve(request.user, admin_response)
            
            return Response({
                'message': 'Feedback marked as resolved',
                'feedback': FeedbackSerializer(feedback).data
            })
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    @action(detail=True, methods=['post'], permission_classes=[IsAdmin])
    def reject(self, request, pk=None):
        """Reject feedback"""
        feedback = self.get_object()
        serializer = FeedbackRejectSerializer(data=request.data)
        
        if serializer.is_valid():
            admin_response = serializer.validated_data.get('admin_response', '')
            
            feedback.reject(request.user, admin_response)
            
            return Response({
                'message': 'Feedback rejected',
                'feedback': FeedbackSerializer(feedback).data
            })
        
        return Response(serializer.errors, status=status.HTTP_400_BAD_REQUEST)
    
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved])
    def me(self, request):
        """Get current user's feedback"""
        user_feedback = Feedback.objects.filter(user=request.user)
        
        # Apply filtering, searching, and ordering like the main list view
        queryset = self.filter_queryset(user_feedback)
        
        # Use pagination for consistency with other endpoints
        page = self.paginate_queryset(queryset)
        if page is not None:
            serializer = self.get_serializer(page, many=True)
            return self.get_paginated_response(serializer.data)
            
        serializer = self.get_serializer(queryset, many=True)
        return Response(serializer.data)
    
