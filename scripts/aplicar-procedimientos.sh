#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
# -b detiene la ejecución ante errores. No contiene creación de tablas ni datos.
docker compose exec -T sqlserver sh -c 'SQLCMDPASSWORD="$MSSQL_SA_PASSWORD" /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -C -b -i /scripts/08_Procedimientos.sql'
