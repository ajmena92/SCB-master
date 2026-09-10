#!/usr/bin/env bash
set -euo pipefail

DESTINO=amenaa@ctpplatanares.ed.cr
UNIDAD=${1:-scb-desconocido}
HOST=$(hostname -f 2>/dev/null || hostname)
MOMENTO=$(date --iso-8601=seconds)

if ! /usr/bin/msmtp -t <<EOF
To: ${DESTINO}
From: ${DESTINO}
Subject: [SCB] Fallo operativo en ${HOST}: ${UNIDAD}

SCB detectó un fallo operativo.

Servidor: ${HOST}
Unidad: ${UNIDAD}
Fecha: ${MOMENTO}

Revise el estado con:
sudo systemctl status ${UNIDAD}
sudo journalctl -u ${UNIDAD} --since '2 hours ago'
EOF
then
    logger -t scb-alerta "No fue posible enviar alerta para ${UNIDAD}."
    exit 1
fi

logger -t scb-alerta "Alerta enviada para ${UNIDAD}."
