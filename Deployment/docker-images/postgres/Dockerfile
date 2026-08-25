# Postgres 16 with pgBackRest installed, for full + incremental physical backups.
FROM postgres:16

RUN apt-get update && \
    apt-get install -y --no-install-recommends pgbackrest && \
    rm -rf /var/lib/apt/lists/* && \
    mkdir -p /var/lib/pgbackrest /var/log/pgbackrest && \
    chown -R postgres:postgres /var/lib/pgbackrest /var/log/pgbackrest && \
    which pgbackrest && pgbackrest version

COPY pgbackrest.conf /etc/pgbackrest/pgbackrest.conf
RUN chown postgres:postgres /etc/pgbackrest/pgbackrest.conf && \
    chmod 640 /etc/pgbackrest/pgbackrest.conf
