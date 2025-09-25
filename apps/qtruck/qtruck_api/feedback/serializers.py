from rest_framework import serializers
from .models import Feedback
from users.serializers import CustomUserSerializer


class FeedbackSerializer(serializers.ModelSerializer):
    user = CustomUserSerializer(read_only=True)
    responded_by = CustomUserSerializer(read_only=True)
    
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        
        # Handle include parameter for selective data inclusion
        context = kwargs.get('context', {})
        include_params = context.get('include', [])
        
        if 'response_details' not in include_params:
            # Remove response fields if not specifically requested
            if not self.context.get('request') or self.context['request'].user.user_type != 'admin':
                # Non-admins need explicit include to see response details
                pass  # Keep all fields for now - could be refined
        
        if 'user_profile' not in include_params:
            # Could simplify user data if not requested
            pass  # Keep full user data for now
    
    def to_representation(self, instance):
        data = super().to_representation(instance)
        
        # Handle include parameter for response
        context = self.context or {}
        include_params = context.get('include', [])
        request = context.get('request')
        
        # Add enhanced response details if requested
        if 'response_details' in include_params and instance.admin_response:
            data['response_metadata'] = {
                'response_length': len(instance.admin_response) if instance.admin_response else 0,
                'response_date_formatted': instance.response_date.strftime('%Y-%m-%d %H:%M:%S') if instance.response_date else None,
                'days_to_respond': (instance.response_date - instance.created_at).days if instance.response_date else None
            }
        
        # Add user profile details if requested
        if 'user_profile' in include_params and instance.user:
            data['user_profile'] = {
                'full_name': f"{instance.user.first_name} {instance.user.last_name}".strip(),
                'user_type': instance.user.user_type,
                'date_joined': instance.user.date_joined.strftime('%Y-%m-%d') if instance.user.date_joined else None
            }
        
        return data
    
    class Meta:
        model = Feedback
        fields = '__all__'
        read_only_fields = ('user', 'responded_by', 'response_date', 'status')


class FeedbackResponseSerializer(serializers.Serializer):
    feedback_id = serializers.UUIDField()
    status = serializers.ChoiceField(choices=Feedback.STATUS_CHOICES)
    admin_response = serializers.CharField(required=False, allow_blank=True)
    
    def validate_feedback_id(self, value):
        try:
            feedback = Feedback.objects.get(id=value)
            return value
        except Feedback.DoesNotExist:
            raise serializers.ValidationError('Feedback not found.')


class FeedbackRespondSerializer(serializers.Serializer):
    """Serializer for responding to feedback"""
    admin_response = serializers.CharField(required=True)
    status = serializers.ChoiceField(choices=Feedback.STATUS_CHOICES, default='reviewed')


class FeedbackResolveSerializer(serializers.Serializer):
    """Serializer for resolving feedback"""
    admin_response = serializers.CharField(required=False, allow_blank=True)


class FeedbackRejectSerializer(serializers.Serializer):
    """Serializer for rejecting feedback"""
    admin_response = serializers.CharField(required=False, allow_blank=True)