from rest_framework import status
from rest_framework.decorators import api_view, permission_classes
from rest_framework.response import Response
from django.conf import settings
from django.core.management import call_command
from drf_spectacular.utils import extend_schema, inline_serializer
from rest_framework import serializers

# Import IsAdmin permission from settings app
from settings.permissions import IsAdmin


@extend_schema(
    summary="Flush Database",
    description="Flush all data from the database and run migrations. Only available when DATABASE_FLUSH_ENABLED=True in environment.",
    responses={
        200: inline_serializer(
            name='DatabaseFlushResponse',
            fields={
                'message': serializers.CharField(),
                'details': serializers.CharField(),
            }
        ),
        403: inline_serializer(
            name='DatabaseFlushDisabledResponse',
            fields={
                'error': serializers.CharField(),
            }
        ),
    },
    tags=["Administration"]
)
@api_view(['POST'])
@permission_classes([IsAdmin])
def flush_database(request):
    """
    Flush all data from the database and run migrations.
    Only available when DATABASE_FLUSH_ENABLED=True in environment.
    This is typically used for testing purposes.
    """
    if not getattr(settings, 'DATABASE_FLUSH_ENABLED', False):
        return Response(
            {'error': 'Database flush is not enabled. Set DATABASE_FLUSH_ENABLED=True in environment to enable this endpoint.'},
            status=status.HTTP_403_FORBIDDEN
        )
    
    try:
        # Flush the database
        call_command('flush', '--noinput')
        
        # Run migrations to ensure database is properly set up
        call_command('migrate', '--noinput')
        
        return Response({
            'message': 'Database flushed successfully',
            'details': 'All data has been removed and migrations have been applied.'
        }, status=status.HTTP_200_OK)
        
    except Exception as e:
        return Response({
            'error': f'Failed to flush database: {str(e)}'
        }, status=status.HTTP_500_INTERNAL_SERVER_ERROR)