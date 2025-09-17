from rest_framework.permissions import BasePermission


class IsAdmin(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.user_type == 'admin' and
            request.user.is_approved
        )


class IsDriver(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.user_type == 'driver'
        )


class IsTester(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.user_type == 'tester'
        )


class IsAdminOrDriver(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.user_type in ['admin', 'driver']
        )


class IsAdminOrOwner(BasePermission):
    def has_permission(self, request, view):
        return request.user and request.user.is_authenticated
    
    def has_object_permission(self, request, view, obj):
        if request.user.user_type == 'admin':
            return True
        
        # Check if user owns the object
        if hasattr(obj, 'user'):
            return obj.user == request.user
        elif hasattr(obj, 'driver') and hasattr(obj.driver, 'user'):
            return obj.driver.user == request.user
        
        return False


class IsApproved(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.is_approved
        )


class IsAdminOrReadOnly(BasePermission):
    """
    Admin can do everything, others can only read
    """
    def has_permission(self, request, view):
        if not (request.user and request.user.is_authenticated and request.user.is_approved):
            return False
        
        if request.method in ['GET', 'HEAD', 'OPTIONS']:
            return True
        
        return request.user.user_type == 'admin'


class IsDriverOrTester(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.is_approved and
            request.user.user_type in ['driver', 'tester']
        )


class IsAdminOrDriverOrTester(BasePermission):
    def has_permission(self, request, view):
        return (
            request.user and 
            request.user.is_authenticated and 
            request.user.is_approved and
            request.user.user_type in ['admin', 'driver', 'tester']
        )