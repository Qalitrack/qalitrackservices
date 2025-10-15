#!/bin/bash
set -e

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE USER masterdata WITH PASSWORD 'masterdata123';
    GRANT ALL PRIVILEGES ON DATABASE masterdatadb TO masterdata;
    \c masterdatadb;
    GRANT ALL ON SCHEMA public TO masterdata;
EOSQL
