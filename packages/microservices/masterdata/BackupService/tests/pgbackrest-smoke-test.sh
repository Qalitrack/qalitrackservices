#!/usr/bin/env bash
# End-to-end validation of the pgBackRest full+incremental backup/restore flow against
# a real Postgres + pgbackrest container — the class of bug (wrong exec user, wrong
# pg1-user, WAL replay going past the intended backup) that mocked unit tests cannot
# catch. Run manually or wire into CI where Docker is available:
#
#   ./pgbackrest-smoke-test.sh
#
# Builds the postgres-prod image from Deployment/docker-images/postgres, runs it
# standalone (not the full compose stack), and exercises: stanza-create, a full backup,
# two incremental backups, then two restores — one to "latest" (no --set) and one
# targeting the first incremental specifically (--set=<label>) — verifying each stops
# exactly where it should. The --set case backs the frontend's "restore to this point
# in the chain" feature, not just "restore latest".
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

label_of() {
  # Extracts the backup label pgbackrest just created from its own console output.
  grep "new backup label" | sed -E 's/.*label = //'
}

echo "==> Seeding data + full backup"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "CREATE TABLE smoke_test (id serial primary key, note text); INSERT INTO smoke_test(note) VALUES ('row-A-full');"
FULL_LABEL="$(docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack --type=full --log-level-console=info backup 2>&1 | label_of)"

echo "==> Seeding more data + first incremental backup"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "INSERT INTO smoke_test(note) VALUES ('row-B-incr1');"
INCR1_LABEL="$(docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack --type=incr --log-level-console=info backup 2>&1 | label_of)"

echo "==> Seeding more data + second incremental backup"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "INSERT INTO smoke_test(note) VALUES ('row-C-incr2');"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack --type=incr backup >/dev/null

echo "==> Writing data AFTER the last backup — must NOT survive any restore"
docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
  "INSERT INTO smoke_test(note) VALUES ('row-D-should-always-be-lost');"

echo "==> pgbackrest info"
docker exec -u postgres "$CONTAINER" pgbackrest --stanza=qalitrack info

# Runs a restore, waits for Postgres, and fails loudly if it's still stuck in
# read-only recovery (pg_is_in_recovery()) or a real INSERT doesn't go through.
# Without --recovery-option=recovery_target_action=promote, Postgres reaches the
# recovery target and just PAUSES read-only instead of becoming a writable primary
# — confirmed against a real restore where every subsequent write failed with
# "cannot execute ... in a read-only transaction", and a read-only SELECT-based
# check alone (this script's original version) never caught it.
restore_and_assert_writable() {
  docker stop "$CONTAINER" >/dev/null
  docker run --rm -u postgres \
    -v "$DATA_VOL":/var/lib/postgresql/data -v "$REPO_VOL":/var/lib/pgbackrest \
    -e PGBACKREST_PG1_USER="$DB_USER" "$IMAGE" \
    pgbackrest --stanza=qalitrack --delta --type=immediate \
      --recovery-option=recovery_target_action=promote --log-level-console=info \
      "$@" restore
  docker start "$CONTAINER" >/dev/null
  wait_ready

  local in_recovery
  in_recovery="$(docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -A -c "SELECT pg_is_in_recovery();")"
  if [ "$in_recovery" != "f" ]; then
    echo "FAIL: Postgres is still in read-only recovery after restore (pg_is_in_recovery() = $in_recovery)" >&2
    exit 1
  fi

  if ! docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -c \
    "INSERT INTO smoke_test(note) VALUES ('writable-check'); DELETE FROM smoke_test WHERE note = 'writable-check';" >/dev/null; then
    echo "FAIL: a real write failed after restore — Postgres did not come back as a writable primary" >&2
    exit 1
  fi
}

echo "==> Restore #1: default (no --set) — should recover the LATEST backup (A, B, C, not D)"
restore_and_assert_writable

RESULT1="$(docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -A -c "SELECT string_agg(note, ',' ORDER BY id) FROM smoke_test;")"
EXPECTED1="row-A-full,row-B-incr1,row-C-incr2"
if [ "$RESULT1" != "$EXPECTED1" ]; then
  echo "FAIL (restore #1, latest): expected '$EXPECTED1', got '$RESULT1'" >&2
  echo "      (if row-D is present, --type=immediate is not doing its job)" >&2
  exit 1
fi
echo "PASS: default restore recovered the latest backup, writable, nothing written after it."

echo "==> Restore #2: --set=\$INCR1_LABEL — should recover ONLY A, B (not C, not D)"
restore_and_assert_writable --set="$INCR1_LABEL"

RESULT2="$(docker exec "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -t -A -c "SELECT string_agg(note, ',' ORDER BY id) FROM smoke_test;")"
EXPECTED2="row-A-full,row-B-incr1"
if [ "$RESULT2" != "$EXPECTED2" ]; then
  echo "FAIL (restore #2, --set targeting first incremental): expected '$EXPECTED2', got '$RESULT2'" >&2
  echo "      (--set is supposed to stop the chain exactly at the chosen backup — this is what the frontend's per-row restore relies on)" >&2
  exit 1
fi
echo "PASS: --set correctly restored to the targeted point in the chain, writable, excluding later incrementals."
