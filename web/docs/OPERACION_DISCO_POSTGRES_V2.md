# Disco cifrado PostgreSQL v2

## Resultado y motivo
El 9 de septiembre de 2026 UTC (noche del 8 en Costa Rica) se sustituyó el
almacenamiento activo porque no se disponía de la clave del volumen original.
Se creó /dev/ubuntu-vg/scb-postgresql-v2 usando 16 GiB libres del grupo LVM;
no se sobrescribió ni se cerró el volumen original. Posteriormente, el VHDX de
Hyper-V se amplió a 256 GiB y se extendieron LVM, LUKS y ext4. El volumen
PostgreSQL tiene ahora 128 GiB, aproximadamente 121 GiB libres, y el grupo LVM
conserva cerca de 61 GiB sin asignar para crecimiento futuro.

- LUKS2: /dev/ubuntu-vg/scb-postgresql-v2.
- Mapping: /dev/mapper/scb_postgresql_v2.
- Montaje ext4: /srv/scb-postgresql-v2.
- Datos: /srv/scb-postgresql-v2/datos/pgdata.
- Identidad PostgreSQL conservada: 7681048827487653926.
- WAL activo: /srv/scb-postgresql-v2/wal-7681048827487653926.
- Respaldos activos: /srv/scb-postgresql-v2/respaldos (root:root, 0700).
- Archivo histórico preservado: /srv/scb-postgresql-v2/archivo-anterior.
- Origen intacto: /srv/scsc-postgresql y /var/lib/scsc-postgresql.luks.

No se amplió ni modificó el contenedor original: se migró a uno nuevo y se
amplió de forma controlada después de validar los respaldos.

## Claves y arranque automático
El usuario eligió arranque automático por la frecuencia de cortes eléctricos.
La clave de arranque está en /etc/cryptsetup-keys.d/scb-postgresql-v2.key,
root:root, 0600, dentro de una carpeta 0700. Hay una segunda clave de recuperación
en otro slot LUKS, conservada fuera del servidor.

La máquina de trabajo conserva los archivos en
/home/dev/.local/share/scb-recovery/20260909, carpeta 0700, claves 0600:
- postgres-v2.key: copia de la clave de arranque.
- postgres-v2-recovery.key: segunda clave; también descifra los respaldos GPG.
- postgres-v2-header.luks: copia de cabecera LUKS, verificada por checksum.
- respaldo-antes-volumen-v2.tar.gz.gpg: respaldo lógico previo y roles.
- respaldo-final-v2.tar.gz.gpg: respaldo final lógico, roles y base física.
- respaldo-final-v2.sha256: checksum del archivo cifrado final.

No copiar estos archivos al repositorio. Trasladar las claves a un gestor de
secretos o almacenamiento externo protegido bajo custodia de la persona responsable.
La copia local es independiente del servidor; no se configuró una copia externa
recurrente. El disco raíz del servidor no está cifrado: conservar allí la clave
permite automatizar el arranque, pero no protege contra quien obtenga una copia
completa del disco del host. Esta limitación se explicó antes de elegir la opción.

El 10 de septiembre se creó además un paquete de recuperación que incluye la
clave de arranque LUKS, la cabecera actual del volumen y la configuración cifrada
de Google Drive. Se cifró con AES-256 usando la clave de recuperación que sigue
fuera de Drive, se verificó antes de retirar sus temporales y se subió al Drive
de Workspace en la ruta visible:

    SCB-RECUPERACION-2026/scb-recuperacion-google.tar.gz.gpg

Su checksum está junto al archivo como
`scb-recuperacion-20260910.tar.gz.gpg.sha256`. Esta carpeta usa el remoto Drive
directo para que pueda localizarse incluso sin `rclone.conf`; el contenido sigue
cifrado y no debe compartirse la clave de recuperación ni copiarla a Drive.

En /etc/crypttab, la entrada scb_postgresql_v2 usa el UUID LUKS y el archivo de
clave. /etc/fstab monta /dev/mapper/scb_postgresql_v2 en /srv/scb-postgresql-v2.
Ambas entradas usan nofail para permitir iniciar el host ante fallos de disco.
El drop-in /etc/systemd/system/docker.service.d/scb-storage.conf declara Wants
y After para la unidad de montaje. Docker espera el intento de montaje; otros
servicios pueden arrancar si falla. SCB rechaza la ausencia de sus datos mediante
bind.create_host_path=false y el entrypoint que verifica la identidad del clúster.

Se probó desmontar y cerrar SOLO el volumen nuevo, luego abrirlo y montarlo
mediante systemd con la clave permanente, antes de copiar los datos.
No se reinició el servidor ni se simuló un apagón.

## Comprobaciones y respaldos
Se detuvo la API, se comprobaron cero conexiones cliente adicionales y se detuvo
PostgreSQL. Se copió el clúster completo y se verificaron todos sus archivos con
rsync en modo simulación y checksum: cero diferencias.
Después de arrancar en el disco nuevo, las cuentas y las 884 credenciales
coincidieron exactamente con las referencias previas. Los valores y hashes
individuales no se mostraron en la salida.

El respaldo lógico previo se restauró con pg_restore --exit-on-error en un
contenedor sin red sobre almacenamiento cifrado. Contenía cuatro cuentas activas
y 884 credenciales. El contenedor quedó detenido.
El respaldo operativo del disco nuevo está en respaldos/20260909T043713Z:
pg_dump, pg_dumpall --globals-only y pg_basebackup. Sus cuatro checksums pasaron.
El archivo final cifrado también se descargó, descifró en memoria y comprobó
fuera del servidor.
También pasó pg_verifybackup sobre la copia física extraída, usando el
backup_manifest separado que genera pg_basebackup en formato tar.
Se corrigió el propietario de los archivos WAL extraídos al usuario PostgreSQL
(999:999), y la instancia aislada arrancó: cuatro cuentas activas y 884
credenciales. Las instancias de prueba quedaron detenidas y los archivos
temporales de claves de /run fueron retirados.

Los servicios systemd programados están habilitados: comprobación de salud cada
hora, respaldo diario a las 02:15 y restauración aislada el primer día de cada
mes a las 03:30. Los respaldos usan la configuración endurecida de Compose.
El 10 de septiembre se ejecutó el respaldo `20260910T141639Z`; sus cuatro
artefactos (dump, globals y copia física con WAL) tienen checksum SHA-256.
También pasó una restauración aislada de prueba con 37 tablas.

Cada respaldo correcto se copia después, cifrado en el servidor, al remoto de
Google Workspace `SCB-PostgreSQL-Backups`. Se conservan 31 días diarios y 13
meses mensuales. La configuración y las claves de cifrado de rclone viven en
`/etc/scb/rclone.conf` con permisos de root; no se guardan en el repositorio.
Google Drive no aporta inmutabilidad, por lo que la cuenta técnica debe tener
MFA y no debe compartirse con usuarios cotidianos.

Las unidades de salud, respaldo local, copia a Drive y verificación de
restauración activan un aviso de fallo por SMTP Relay de Google Workspace a
`amenaa@ctpplatanares.ed.cr`. El relay usa TLS, la IP pública de salida
`181.193.106.93` y autenticación por IP autorizada, sin contraseña almacenada.
El 10 de septiembre la prueba fue aceptada por Google con respuesta SMTP
`250 2.0.0 OK`.

El mismo día se instaló la actualización de seguridad pendiente y se reinició
el portal de forma controlada, después de crear el respaldo
`20260910T182623Z` y su copia cifrada externa. El arranque posterior confirmó
el montaje automático de `/srv/scb-postgresql-v2`, el kernel
`6.8.0-139-generic`, los cuatro servicios SCB saludables y los tres
temporizadores activos.

Para crear un respaldo manual con la configuración actual:

    cd /home/plat/scsc-comedor/ops
    sudo docker compose --env-file .env -f compose.production.yml -f compose.production.hardening.yml --profile respaldo run --rm --no-deps respaldo

El servicio usa la ruta POSTGRES_BACKUP_PATH del .env. Falta configurar un
destino externo para cumplir 3-2-1 y un canal de alertas para los avisos del
monitor; la copia actual sigue en el mismo host Hyper-V. El WAL puede llenar
cualquier volumen si no se monitorea; no eliminarlo automáticamente sin
vincular su retención a respaldos recuperables.

## Recuperación del montaje
Comprobar:

    sudo systemctl status systemd-cryptsetup@scb_postgresql_v2.service
    sudo findmnt /srv/scb-postgresql-v2
    sudo df -h /srv/scb-postgresql-v2
    curl --fail http://127.0.0.1:8081/health

Si se pierde la clave de arranque, usar la segunda clave desde un archivo
temporal protegido para desbloquear el MISMO dispositivo. No reformatear:

    sudo cryptsetup open --key-file /ruta/segura/postgres-v2-recovery.key /dev/ubuntu-vg/scb-postgresql-v2 scb_postgresql_v2
    sudo mount /srv/scb-postgresql-v2

Solo ejecutar open si el mapping está cerrado. No cerrar un volumen en uso.
La segunda clave fue validada mediante open --test-passphrase.

## Acceso SSH interno de administración
Usar autenticación por clave/firma desde la red interna autorizada. Para estas
cuentas de dominio, OpenSSH del servidor resuelve el usuario por su nombre
simple. El formato es:

    ssh -o BatchMode=yes amenaa@172.30.210.200

No incluir el dominio en el nombre SSH: los formatos
`ctpplatanares\\amenaa`, `ctpplatanares.local\\amenaa` y
`amenaa@ctpplatanares.local` fueron registrados por OpenSSH como usuarios
inválidos. No se documentan ni se usan contraseñas.

Para consultar fallos de autenticación en Windows Server:

    Get-WinEvent -LogName 'OpenSSH/Operational' -MaxEvents 20 |
      Format-Table TimeCreated, Id, LevelDisplayName, Message -Wrap

El mismo formato de usuario simple debe probarse para el host Hyper-V interno:

    ssh -o BatchMode=yes amenaa@172.30.210.50

## Fuentes técnicas
- Copia consistente con PostgreSQL detenido:
  https://www.postgresql.org/docs/17/backup-file.html
- Desbloqueo con archivo de clave:
  https://www.freedesktop.org/software/systemd/man/250/crypttab.html
