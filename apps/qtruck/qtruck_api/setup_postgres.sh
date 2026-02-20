#!/bin/bash

# =============================================
# 🔹 Django + PostgreSQL Setup Script
# =============================================

# Activate virtualenv
source venv/bin/activate

# Load environment variables from .env
export $(grep -v '^#' .env | xargs)

# Check if required DB vars exist
if [[ -z "$DB_NAME" || -z "$DB_USER" || -z "$DB_PASSWORD" || -z "$DB_HOST" || -z "$DB_PORT" ]]; then
    echo "❌ Missing DB environment variables in .env"
    exit 1
fi

# Step 1: Create database if it doesn't exist
echo "🔹 Step 1: Creating database $DB_NAME..."
psql -U postgres -h $DB_HOST -tc "SELECT 1 FROM pg_database WHERE datname = '$DB_NAME'" | grep -q 1 || \
createdb -U postgres -h $DB_HOST $DB_NAME
echo "✅ Database $DB_NAME ready."

# Step 2: Ensure __init__.py in all migrations
echo "🔹 Step 2: Ensuring __init__.py in all migrations..."
for app in users authentication drivers administration feedback fleet trips settings; do
    mkdir -p $app/migrations
    touch $app/migrations/__init__.py
done

# Step 3: Apply migrations
echo "🔹 Step 3: Applying migrations..."
python manage.py migrate --fake-initial

# Step 4: Create superuser
echo "🔹 Step 4: Creating superuser..."
python manage.py createsuperuser --noinput || echo "✅ Superuser creation skipped (already exists)"

echo "🎉 Setup complete!"
