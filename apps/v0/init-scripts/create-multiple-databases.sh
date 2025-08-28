#!/bin/bash
set -e
set -u

function create_user_and_database() {
    local database=$1
    local user=$2
    local password=$3
    echo "Creating database '$database' with user '$user'..."
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
        CREATE USER $user WITH PASSWORD '$password';
        CREATE DATABASE $database OWNER $user;
        GRANT ALL PRIVILEGES ON DATABASE $database TO $user;
EOSQL
}

# Create schema for backup service
function create_backup_schema() {
    echo "Creating schema 'backup' in database 'backupservicedb'..."
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "backupservicedb" <<-EOSQL
        CREATE SCHEMA IF NOT EXISTS backup;
        GRANT ALL ON SCHEMA backup TO backupservice;
EOSQL
}

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
    echo "Multiple database creation requested: $POSTGRES_MULTIPLE_DATABASES"
    for db in $(echo $POSTGRES_MULTIPLE_DATABASES | tr ',' ' '); do
        IFS=':' read -r database user password <<< "$db"
        create_user_and_database $database $user $password
    done
    echo "Multiple databases created"
fi

# Always create the backup schema for backupservicedb
create_backup_schema