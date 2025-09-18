#!/bin/bash
set -e

echo "Starting QTruck API entrypoint..."

# Function to wait for database
wait_for_db() {
    echo "Waiting for database to be ready..."
    
    # Wait for PostgreSQL to be ready
    until python -c "
import psycopg2
import os
import sys
try:
    conn = psycopg2.connect(
        host=os.environ.get('DB_HOST', 'localhost'),
        port=os.environ.get('DB_PORT', '5432'),
        user=os.environ.get('DB_USER', 'postgres'),
        password=os.environ.get('DB_PASSWORD', ''),
        dbname=os.environ.get('DB_NAME', 'postgres')
    )
    conn.close()
    print('Database is ready!')
except psycopg2.OperationalError as e:
    print(f'Database not ready: {e}')
    sys.exit(1)
"; do
        echo "Database is unavailable - sleeping"
        sleep 2
    done
    
    echo "Database is ready!"
}

# Function to run migrations
run_migrations() {
    echo "Running database migrations..."
    python manage.py migrate --noinput
    echo "Migrations completed!"
}

# Function to collect static files
collect_static() {
    echo "Checking if static files need to be collected..."
    
    # Get DEBUG setting from Django
    DEBUG_VALUE=$(python -c "
from django.conf import settings
print(settings.DEBUG)
" 2>/dev/null || echo "True")

    echo "DEBUG is set to: $DEBUG_VALUE"
    
    if [ "$DEBUG_VALUE" = "False" ]; then
        echo "DEBUG=False: Collecting static files..."
        python manage.py collectstatic --noinput --clear
        echo "Static files collected!"
    else
        echo "DEBUG=True: Skipping static collection (served by Django)"
    fi
}

# Function to create superuser if needed
create_superuser() {
    if [ "$DJANGO_SUPERUSER_USERNAME" ] && [ "$DJANGO_SUPERUSER_PASSWORD" ] && [ "$DJANGO_SUPERUSER_EMAIL" ]; then
        echo "Creating superuser..."
        python manage.py shell -c "
from django.contrib.auth import get_user_model
User = get_user_model()
if not User.objects.filter(username='$DJANGO_SUPERUSER_USERNAME').exists():
    User.objects.create_superuser('$DJANGO_SUPERUSER_USERNAME', '$DJANGO_SUPERUSER_EMAIL', '$DJANGO_SUPERUSER_PASSWORD')
    print('Superuser created successfully!')
else:
    print('Superuser already exists.')
" 2>/dev/null || echo "Could not create superuser"
    fi
}

# Main execution
main() {
    echo "=== QTruck API Startup ==="
    
    # Wait for database
    wait_for_db
    
    # Run migrations
    run_migrations
    
    # Collect static files if needed
    collect_static
    
    # Create superuser if environment variables are set
    create_superuser
    
    echo "=== Starting Django Server ==="
    
    # Start the main process
    exec "$@"
}

# Execute main function with all arguments
main "$@"