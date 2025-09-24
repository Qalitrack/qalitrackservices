from rest_framework import viewsets, status, permissions
from rest_framework.decorators import action
from rest_framework.response import Response
from rest_framework_simplejwt.authentication import JWTAuthentication
from django.utils import timezone
from drf_spectacular.utils import extend_schema, extend_schema_view

from .models import Feedback
from .serializers import (
    FeedbackSerializer, FeedbackResponseSerializer,
    FeedbackRespondSerializer, FeedbackResolveSerializer, FeedbackRejectSerializer
)
from settings.permissions import IsAdmin, IsDriverOrTester, IsApproved


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
    pending=extend_schema(tags=["Feedback"]),
    my_feedback=extend_schema(tags=["Feedback"]),
    by_status=extend_schema(tags=["Feedback"])
)
class FeedbackViewSet(viewsets.ModelViewSet):
    queryset = Feedback.objects.all()
    serializer_class = FeedbackSerializer
    authentication_classes = [JWTAuthentication]
    permission_classes = [IsApproved, IsDriverOrTester]
    
    def get_queryset(self):
        if self.request.user.user_type == 'admin':
            return Feedback.objects.all()
        else:
            return Feedback.objects.filter(user=self.request.user)
    
    def perform_create(self, serializer):
        serializer.save(user=self.request.user)
    
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
    
    @action(detail=False, methods=['get'], permission_classes=[IsAdmin])
    def pending(self, request):
        """Get all pending feedback for admin"""
        pending_feedback = Feedback.objects.filter(status='pending')
        serializer = self.get_serializer(pending_feedback, many=True)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get'], permission_classes=[IsApproved])
    def my_feedback(self, request):
        """Get current user's feedback"""
        user_feedback = Feedback.objects.filter(user=request.user)
        serializer = self.get_serializer(user_feedback, many=True)
        return Response(serializer.data)
    
    @action(detail=False, methods=['get'], permission_classes=[IsAdmin])
    def by_status(self, request):
        """Get feedback grouped by status"""
        status_param = request.query_params.get('status')
        
        if status_param:
            feedback = Feedback.objects.filter(status=status_param)
            serializer = self.get_serializer(feedback, many=True)
            return Response(serializer.data)
        else:
            # Return feedback grouped by status
            feedback_by_status = {}
            for feedback in Feedback.objects.all():
                status_key = feedback.status or 'unknown'
                if status_key not in feedback_by_status:
                    feedback_by_status[status_key] = []
                feedback_by_status[status_key].append(self.get_serializer(feedback).data)
            return Response(feedback_by_status)