#!/bin/sh
set -eu

raiz="$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)"
cd "$raiz"
exec docker compose --env-file ops/.env -f ops/compose.production.yml --profile analitica \
  run --rm analitica --dias 20 --salida /tmp/analitica-comedor.json
