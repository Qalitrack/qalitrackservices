from django.test import TestCase
from django.urls import reverse
from rest_framework.test import APITestCase
from rest_framework import status
from django.contrib.auth import get_user_model
from .models import Feedback

User = get_user_model()


class FeedbackAPIConsolidationTest(APITestCase):
    """Test the consolidated feedback API functionality"""
    
    def setUp(self):
        # Create test users with approved status
        self.admin_user = User.objects.create_user(
            email='admin+test@example.com',
            password='testpass123',
            user_type='admin',
            first_name='Admin',
            last_name='User',
            status='approved'
        )
        
        self.driver_user = User.objects.create_user(
            email='driver+test@example.com',
            password='testpass123',
            user_type='driver',
            first_name='Driver',
            last_name='User',
            status='approved'
        )
        
        # Create test feedback
        self.feedback1 = Feedback.objects.create(
            user=self.driver_user,
            feedback_type='bug_report',
            subject='Login Issue',
            description='Cannot login to the app',
            status='pending'
        )
        
        self.feedback2 = Feedback.objects.create(
            user=self.driver_user,
            feedback_type='feature_request', 
            subject='Mobile App Feature',
            description='Add dark mode to mobile app',
            status='reviewed'
        )
        
        # Respond to feedback2
        self.feedback2.respond(
            self.admin_user,
            'We will consider this for the next release',
            'reviewed'
        )
    
    def test_status_filtering_admin_only(self):
        """Test that status filtering works for admin users only"""
        # Admin can filter by status
        self.client.force_authenticate(user=self.admin_user)
        response = self.client.get('/api/feedback/?status=pending')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['status'], 'pending')
        
        # Driver cannot use status filter (silently ignored)
        self.client.force_authenticate(user=self.driver_user)
        response = self.client.get('/api/feedback/?status=pending')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        # Should return all driver's feedback, ignoring status filter
        self.assertEqual(len(response.data), 2)
    
    def test_feedback_type_filtering(self):
        """Test feedback type filtering for all users"""
        self.client.force_authenticate(user=self.driver_user)
        response = self.client.get('/api/feedback/?feedback_type=bug_report')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertEqual(response.data[0]['feedback_type'], 'bug_report')
    
    def test_search_functionality(self):
        """Test search across subject and description"""
        self.client.force_authenticate(user=self.driver_user)
        response = self.client.get('/api/feedback/?search=login')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertTrue('Login Issue' in response.data[0]['subject'])
    
    def test_include_response_details(self):
        """Test include parameter for response details"""
        self.client.force_authenticate(user=self.driver_user)
        response = self.client.get('/api/feedback/?include=response_details')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Find the feedback with response
        responded_feedback = next(
            (f for f in response.data if f['admin_response']), 
            None
        )
        self.assertIsNotNone(responded_feedback)
        self.assertIn('response_metadata', responded_feedback)
        self.assertIn('response_length', responded_feedback['response_metadata'])
    
    def test_include_user_profile(self):
        """Test include parameter for user profile details"""
        self.client.force_authenticate(user=self.admin_user)
        response = self.client.get('/api/feedback/?include=user_profile')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        feedback = response.data[0]
        self.assertIn('user_profile', feedback)
        self.assertIn('full_name', feedback['user_profile'])
        self.assertEqual(feedback['user_profile']['full_name'], 'Driver User')
    
    def test_has_response_filter_admin_only(self):
        """Test has_response filter for admin users only"""
        self.client.force_authenticate(user=self.admin_user)
        
        # Test has_response=true
        response = self.client.get('/api/feedback/?has_response=true')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertIsNotNone(response.data[0]['admin_response'])
        
        # Test has_response=false
        response = self.client.get('/api/feedback/?has_response=false')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        self.assertIsNone(response.data[0]['admin_response'])
    
    def test_combined_filters(self):
        """Test combining multiple filters"""
        self.client.force_authenticate(user=self.admin_user)
        response = self.client.get(
            '/api/feedback/?feedback_type=bug_report&search=login&include=response_details'
        )
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 1)
        
        feedback = response.data[0]
        self.assertEqual(feedback['feedback_type'], 'bug_report')
        self.assertTrue('login' in feedback['subject'].lower())
    
    def test_my_feedback_endpoint_preserved(self):
        """Test that my_feedback shortcut endpoint still works"""
        self.client.force_authenticate(user=self.driver_user)
        response = self.client.get('/api/feedback/my_feedback/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        self.assertEqual(len(response.data), 2)
        
        # All feedback should belong to authenticated user
        for feedback in response.data:
            self.assertEqual(feedback['user']['email'], 'driver+test@example.com')
    
    def test_ordering_functionality(self):
        """Test ordering by different fields"""
        self.client.force_authenticate(user=self.driver_user)
        
        # Test ordering by created_at (default)
        response = self.client.get('/api/feedback/')
        self.assertEqual(response.status_code, status.HTTP_200_OK)
        
        # Test ordering by status
        response = self.client.get('/api/feedback/?ordering=status')
        self.assertEqual(response.status_code, status.HTTP_200_OK)