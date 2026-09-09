# Incidente PostgreSQL del 8 de septiembre de 2026

## Causa y recuperación
Al recrear PostgreSQL solo con compose.production.yml, el servicio seleccionó
el volumen Docker antiguo porque el montaje productivo estaba definido en
compose.production.server.yml. La aplicación mostró datos anteriores. La base
operativa seguía en /srv/scsc-postgresql/datos/pgdata; no fue necesario restaurarla
desde un respaldo. La afirmación inicial de pérdida de datos fue incorrecta.

Se detuvo la API durante el cambio, se detuvo PostgreSQL y se copiaron ambas bases
antes de reconectar el servicio. No se ejecutaron migraciones ni se eliminaron
volúmenes. Producción usa ahora el clúster 7681048827487653926. El volumen antiguo
corresponde al clúster 7681534197914152999.

Las copias previas y un respaldo lógico se conservan con acceso restringido en
/var/lib/scsc-incidente-20260909. La copia forense anterior permanece en
/var/lib/scsc-forensic-20260909-OMGPnD. Estos archivos contienen datos sensibles;
no deben publicarse ni incorporarse a Git.

## Verificación
- Las cuatro cuentas administrativas de la base recuperada están activas.
- Las 884 credenciales coinciden con la copia forense en identificador, hash de
  PIN y obligación de cambio (cero diferencias al comprobarlas).
- PostgreSQL y API superaron sus comprobaciones de salud.
- El respaldo scb-recuperada.dump se generó con pg_dump y su índice se leyó con
  pg_restore --list. Esta comprobación no equivale a una restauración completa.
- SHA256 del respaldo: 986806229337dacbfd2eab0c3be0ed0cce1cbfdcb7767a3da7a8b88a49252147.
- No se probó un inicio de sesión con las contraseñas de personas usuarias.

## Prevención aplicada
compose.production.yml define directamente el bind de datos y rechaza crear la
ruta automáticamente. El archivo adicional conserva la misma protección.
El entrypoint postgres/verificar_pgdata.sh exige un clúster existente y compara
pg_controldata con POSTGRES_SYSTEM_IDENTIFIER antes de ejecutar PostgreSQL.
Se probaron tres casos en contenedores aislados sin red: identidad correcta,
identidad incorrecta y PGDATA ausente. Solo el primero pasó.

En el servidor, POSTGRES_SYSTEM_IDENTIFIER=7681048827487653926.
La configuración local sobrescribe el entrypoint y conserva explícitamente su
volumen externo; no usa el disco productivo.

Ambos clústeres compartían la ruta de archivo WAL pese a tener distinta identidad.
Se preservó /srv/scsc-postgresql/wal y se configuró una carpeta exclusiva para
la base recuperada: /srv/scsc-postgresql/wal-7681048827487653926.
No se garantiza la continuidad histórica del WAL compartido; no usarlo para
recuperación automática sin comprobar procedencia y continuidad.

## Ampliación pendiente
El archivo /var/lib/scsc-postgresql.luks y loop0 miden 16 GiB. El mapping
scsc_postgresql sigue en 4 GiB y ext4 en aproximadamente 3,9 GiB.
cryptsetup resize --batch-mode no pudo autenticar sin clave.
No se cerró el mapping, no se reformateó y no se modificaron claves.
Al finalizar el respaldo quedaban aproximadamente 891 MiB libres.

Falta localizar la clave de recuperación. No se debe escribir en esta documentación,
en argumentos del shell ni en el chat. Con la clave disponible, completar
cryptsetup resize mediante entrada segura, verificar el tamaño del mapping y
ejecutar resize2fs /dev/mapper/scsc_postgresql; comprobar después lsblk, df,
la identidad del clúster y /health. No ampliar ext4 antes del mapping.
La documentación de cryptsetup confirma que resize puede requerir autenticación:
https://kernel.googlesource.com/pub/scm/utils/cryptsetup/cryptsetup/+/5d622102c68fc7427d3a41b948be4fe596722f2f/man/cryptsetup-resize.8.adoc

/etc/crypttab no tiene una entrada para este disco. No se ha validado el desbloqueo
tras reiniciar el host. No reiniciar hasta documentar y comprobar la disponibilidad
de la clave y el procedimiento de montaje. Las protecciones de arranque evitan
inicializar una base vacía, pero no sustituyen el montaje ni el monitoreo de espacio.

## Operación futura
Preservar POSTGRES_DATA_PATH, POSTGRES_SYSTEM_IDENTIFIER y POSTGRES_WAL_ARCHIVE_PATH
en el .env del servidor. Nunca corregir un bloqueo cambiando la identidad esperada
sin verificar la procedencia de la base. No usar down -v, initdb, restauraciones
sobre producción ni limpieza de WAL para resolver un fallo de montaje.
Los respaldos previos al cambio de ruta y el archivo WAL compartido deben
conservarse hasta validar una recuperación completa del nuevo conjunto.
