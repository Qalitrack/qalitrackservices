#!/bin/bash
set -e

echo "Initializing database..."

# Wait for PostgreSQL to be ready
echo "Waiting for PostgreSQL to start..."
until PGPASSWORD=$POSTGRES_PASSWORD psql -h "localhost" -U "$POSTGRES_USER" -d "$POSTGRES_DB" -p 5432 -c '\q'; do
  >&2 echo "PostgreSQL is unavailable - sleeping"
  sleep 1
done

# Create additional databases or users if needed
# Example:
# PGPASSWORD=$POSTGRES_PASSWORD psql -h "localhost" -U "$POSTGRES_USER" -p 5432 -c "CREATE DATABASE another_db;"
# PGPASSWORD=$POSTGRES_PASSWORD psql -h "localhost" -U "$POSTGRES_USER" -p 5432 -c "CREATE USER another_user WITH PASSWORD 'another_password';"
# PGPASSWORD=$POSTGRES_PASSWORD psql -h "localhost" -U "$POSTGRES_USER" -p 5432 -c "GRANT ALL PRIVILEGES ON DATABASE another_db TO another_user;"

echo "Database initialization completed!"
