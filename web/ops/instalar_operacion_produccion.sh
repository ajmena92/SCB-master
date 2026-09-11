#!/usr/bin/env bash
set -euo pipefail

origen="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

install -d -m 700 /etc/scb
install -o root -g root -m 755 "$origen/monitorizar_salud_produccion.sh" /usr/local/sbin/scb-salud
install -o root -g root -m 755 "$origen/sincronizar_respaldo_google_drive.sh" /usr/local/sbin/scb-respaldo-google
install -o root -g root -m 755 "$origen/migrar_respaldos_google_drive.sh" /usr/local/sbin/scb-migrar-respaldos-drive
install -o root -g root -m 755 "$origen/crear_cofre_recuperacion.sh" /usr/local/sbin/scb-crear-cofre-recuperacion
install -o root -g root -m 755 "$origen/enviar_alerta_workspace.sh" /usr/local/sbin/scb-enviar-alerta
install -o root -g root -m 644 "$origen/systemd/"*.service "$origen/systemd/"*.timer /etc/systemd/system/
install -o root -g root -m 600 "$origen/msmtprc.google-workspace.example" /etc/msmtprc
touch /var/log/msmtp.log
chmod 600 /var/log/msmtp.log

systemctl daemon-reload
systemctl enable scb-salud.timer scb-respaldo.timer scb-verificar-restauracion.timer
systemd-analyze verify /etc/systemd/system/scb-*.service /etc/systemd/system/scb-*.timer
