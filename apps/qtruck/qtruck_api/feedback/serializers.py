from rest_framework import serializers
from .models import Feedback
from users.serializers import CustomUserSerializer


class FeedbackSerializer(serializers.ModelSerializer):
    user = CustomUserSerializer(read_only=True)
    responded_by = CustomUserSerializer(read_only=True)
    
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