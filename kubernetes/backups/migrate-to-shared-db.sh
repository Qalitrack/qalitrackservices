#!/bin/bash
# Migrate data from the old per-service PostgreSQL databases into the new
# shared qalitrackdb (schema-per-service). Run this ON THE SERVER against
# whatever the old per-service Postgres instances actually are in your
# cluster (StatefulSet, standalone container, managed DB — this script
# just needs host/port/db/user/password for each).
#
# PRECONDITIONS (do these first, in order):
#   1. The new shared Postgres is up and schemas exist:
#      kubectl exec -it -n qalitrack-prod qalitrack-postgresql-0 -- \
#        psql -U qalitrack -d qalitrackdb -c "\dn"
#      -> must list: masterdata, transactions, users, backup
#   2. EF migrations have been applied to the (still-empty) new schemas —
#      easiest way: deploy the new service images once with the shared-DB
#      env vars, let each service's `Database.Migrate()` run on startup,
#      confirm __EFMigrationsHistory exists per schema, THEN scale the
#      services to 0 replicas before running this script. Loading data
#      into tables the app hasn't created yet will fail; loading data
#      into tables that already have rows will fail on PK conflicts.
#   3. Put the app in a maintenance window — old databases must not
#      accept writes while this dumps them (point-in-time consistency).
#
# This does a DATA-ONLY migration (COPY + sequence values). It does not
# touch schema/DDL — the new schemas already own that via EF migrations.
#
# Usage: fill in the OLD_* vars below (or export them before running),
# then: ./migrate-to-shared-db.sh [--dry-run]

set -euo pipefail

DRY_RUN=false
[[ "${1:-}" == "--dry-run" ]] && DRY_RUN=true

# --- New shared database ----------------------------------------------
NEW_DB_HOST="${NEW_DB_HOST:-qalitrack-postgresql}"
NEW_DB_PORT="${NEW_DB_PORT:-5432}"
NEW_DB_NAME="${NEW_DB_NAME:-qalitrackdb}"
NEW_DB_USER="${NEW_DB_USER:-qalitrack}"
NEW_DB_PASSWORD="${NEW_DB_PASSWORD:?set NEW_DB_PASSWORD}"

# --- Old per-service databases ------------------------------------------
# Fill these in with wherever each old database actually lives.
# Format per service: HOST PORT DBNAME USER PASSWORD TARGET_SCHEMA
declare -A SERVICES=(
  [masterdata]="${OLD_MASTERDATA_HOST:?set OLD_MASTERDATA_HOST}|${OLD_MASTERDATA_PORT:-5432}|${OLD_MASTERDATA_DB:?set OLD_MASTERDATA_DB}|${OLD_MASTERDATA_USER:?set OLD_MASTERDATA_USER}|${OLD_MASTERDATA_PASSWORD:?set OLD_MASTERDATA_PASSWORD}|masterdata"
  [transaction]="${OLD_TRANSACTION_HOST:?set OLD_TRANSACTION_HOST}|${OLD_TRANSACTION_PORT:-5432}|${OLD_TRANSACTION_DB:?set OLD_TRANSACTION_DB}|${OLD_TRANSACTION_USER:?set OLD_TRANSACTION_USER}|${OLD_TRANSACTION_PASSWORD:?set OLD_TRANSACTION_PASSWORD}|transactions"
  [user-service]="${OLD_USER_HOST:?set OLD_USER_HOST}|${OLD_USER_PORT:-5432}|${OLD_USER_DB:?set OLD_USER_DB}|${OLD_USER_USER:?set OLD_USER_USER}|${OLD_USER_PASSWORD:?set OLD_USER_PASSWORD}|users"
  [backup]="${OLD_BACKUP_HOST:?set OLD_BACKUP_HOST}|${OLD_BACKUP_PORT:-5432}|${OLD_BACKUP_DB:?set OLD_BACKUP_DB}|${OLD_BACKUP_USER:?set OLD_BACKUP_USER}|${OLD_BACKUP_PASSWORD:?set OLD_BACKUP_PASSWORD}|backup"
)

WORKDIR=$(mktemp -d)
trap 'rm -rf "$WORKDIR"' EXIT

echo "Workdir: $WORKDIR"
$DRY_RUN && echo "*** DRY RUN — dumping and rewriting only, nothing will be loaded ***"

for svc in "${!SERVICES[@]}"; do
  IFS='|' read -r OLD_HOST OLD_PORT OLD_DB OLD_USER OLD_PASSWORD SCHEMA <<< "${SERVICES[$svc]}"

  echo ""
  echo "=== $svc  (old: $OLD_USER@$OLD_HOST:$OLD_PORT/$OLD_DB  ->  new schema: $SCHEMA) ==="

  DUMP_FILE="$WORKDIR/$svc.sql"

  echo "-> dumping data (public schema, data-only, triggers disabled)..."
  PGPASSWORD="$OLD_PASSWORD" pg_dump \
    -h "$OLD_HOST" -p "$OLD_PORT" -U "$OLD_USER" -d "$OLD_DB" \
    --data-only --disable-triggers --no-owner --no-privileges \
    -n public -f "$DUMP_FILE"

  echo "-> rewriting schema references (public -> $SCHEMA) on COPY/setval lines only..."
  sed -i \
    -e "s/^COPY public\./COPY ${SCHEMA}./" \
    -e "s/pg_catalog\.setval('public\./pg_catalog.setval('${SCHEMA}./" \
    "$DUMP_FILE"

  ROWS_IN_DUMP=$(grep -c '^COPY ' "$DUMP_FILE" || true)
  echo "-> $ROWS_IN_DUMP tables in dump for $svc"

  if $DRY_RUN; then
    echo "-> dry run: skipping load. Rewritten dump at $DUMP_FILE"
    continue
  fi

  echo "-> loading into $NEW_DB_NAME (schema $SCHEMA)..."
  PGPASSWORD="$NEW_DB_PASSWORD" psql \
    -h "$NEW_DB_HOST" -p "$NEW_DB_PORT" -U "$NEW_DB_USER" -d "$NEW_DB_NAME" \
    -v ON_ERROR_STOP=1 -f "$DUMP_FILE"

  echo "-> verifying row counts (old vs new) for $svc..."
  for TBL in $(PGPASSWORD="$OLD_PASSWORD" psql -h "$OLD_HOST" -p "$OLD_PORT" -U "$OLD_USER" -d "$OLD_DB" -Atqc \
      "SELECT tablename FROM pg_tables WHERE schemaname='public'"); do
    OLD_COUNT=$(PGPASSWORD="$OLD_PASSWORD" psql -h "$OLD_HOST" -p "$OLD_PORT" -U "$OLD_USER" -d "$OLD_DB" -Atqc \
      "SELECT count(*) FROM public.\"$TBL\"")
    NEW_COUNT=$(PGPASSWORD="$NEW_DB_PASSWORD" psql -h "$NEW_DB_HOST" -p "$NEW_DB_PORT" -U "$NEW_DB_USER" -d "$NEW_DB_NAME" -Atqc \
      "SELECT count(*) FROM ${SCHEMA}.\"$TBL\"")
    if [[ "$OLD_COUNT" != "$NEW_COUNT" ]]; then
      echo "   MISMATCH: $SCHEMA.$TBL  old=$OLD_COUNT new=$NEW_COUNT"
    else
      echo "   OK: $SCHEMA.$TBL  ($OLD_COUNT rows)"
    fi
  done
done

echo ""
echo "Done. Review any MISMATCH lines above before pointing services at the new DB / scaling them back up."
