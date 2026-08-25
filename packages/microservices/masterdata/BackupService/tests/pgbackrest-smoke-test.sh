#!/usr/bin/env bash
# End-to-end validation of the pgBackRest full+incremental backup/restore flow against
# a real Postgres + pgbackrest container — the class of bug (wrong exec user, wrong
# pg1-user, WAL replay going past the intended backup) that mocked unit tests cannot
# catch. Run manually or wire into CI where Docker is available:
#
#   ./pgbackrest-smoke-test.sh
#
# Builds the postgres-prod image from Deployment/docker-images/postgres, runs it
# standalone (not the full compose stack), and exercises: stanza-create, full backup,
# incremental backup, restore, verifying that data written after the last backup does
# NOT survive the restore (i.e. --type=immediate is doing its job).
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../../.." && pwd)"
IMAGE="qalitrack-postgres-prod:latest"
CONTAINER="pgbr-smoke-pg"
DATA_VOL="pgbr-smoke-data"
REPO_VOL="pgbr-smoke-repo"
DB_USER="qalitrack"
DB_NAME="qalitrackdb"

cleanup() {
  docker rm -f "$CONTAINER" >/dev/null 2>&1 || true
  docker volume rm -f "$DATA_VOL" "$REPO_VOL" >/dev/null 2>&1 || true
}
trap cleanup EXIT

echo "==> Building postgres-prod image"
docker build -q -t "$IMAGE" "$REPO_ROOT/Deployment/docker-images/postgres" >/dev/null

cleanup
docker volume create "$DATA_VOL" >/dev/null
docker volume create "$REPO_VOL" >/dev/null

echo "==> Starting Postgres with archive_mode + pgBackRest"
docker run -d --name "$CONTAINER" \
  -e POSTGRES_DB="$DB_NAME" -e POSTGRES_USER="$DB_USER" -e POSTGRES_PASSWORD=test123 \
  -e PGBACKREST_PG1_USER="$DB_USER" \
  -v "$DATA_VOL":/var/lib/postgresql/data \
  -v "$REPO_VOL":/var/lib/pgbackrest \
  "$IMAGE" \
  postgres -c wal_level=replica -c archive_mode=on \
    -c "archive_command=pgbackrest --stanza=qalitrack archive-push %p" >/dev/null

wait_ready() {
  # The official postgres image starts a transient setup server, stops it, then
  # starts the real one — a single successful pg_isready can catch that transient
  # server. Require two successes 2s apart so we don't race the restart.
  for _ in $(seq 1 30); do
    if docker exec "$CONTAINER" pg_isready -U "$DB_USER" -d "$DB_NAME" 2>&1 | grep -q "accepting connections"; then
      sleep 2
      if docker exec "$CONTAINER" pg_isready -U "$DB_USER" -d "$DB_NAME" 2>&1 | grep -q "accepting connections"; then
        return 0
      fi
    fi
    sleep 1
  done
  echo "Postgres never became ready" >&2
  exit 1
}
wait_ready

echo "==> stanza-create + check"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack stanza-create
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack check

echo "==> Seeding data + full backup"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "CREATE TABLE smoke_test (id serial primary key, note text); INSERT INTO smoke_test(note) VALUES ('row-A-full');"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack --type=full backup

echo "==> Seeding more data + incremental backup"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "INSERT INTO smoke_test(note) VALUES ('row-B-incremental');"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack --type=incr backup

echo "==> Writing data AFTER the last backup — must NOT survive restore"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "INSERT INTO smoke_test(note) VALUES ('row-C-should-be-lost');"

echo "==> pgbackrest info"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack info

echo "==> Restoring (stop, restore via one-off container, start)"
docker stop "$CONTAINER" >/dev/null
docker run --rm -u postgres \
  -v "$DATA_VOL":/var/lib/postgresql/data \
  -v "$REPO_VOL":/var/lib/pgbackrest \
  -e PGBACKREST_PG1_USER="$DB_USER" \
  "$IMAGE" \
  pgbackrest --stanza=qalitrack --delta --type=immediate --log-level-console=info restore
docker start "$CONTAINER" >/dev/null
wait_ready

echo "==> Verifying restored data"
RESULT="$(docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -A -c "SELECT string_agg(note, ',' ORDER BY id) FROM smoke_test;")"
EXPECTED="row-A-full,row-B-incremental"

if [ "$RESULT" != "$EXPECTED" ]; then
  echo "FAIL: expected '$EXPECTED' after restore, got '$RESULT'" >&2
  echo "      (if row-C is present, --type=immediate is not doing its job — WAL replayed past the backup)" >&2
  exit 1
fi

echo "PASS: restore recovered exactly the full+incremental chain, nothing written after it."
