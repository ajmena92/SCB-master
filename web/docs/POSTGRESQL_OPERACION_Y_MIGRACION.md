# Operación PostgreSQL, respaldo e importación

## Preparación

PostgreSQL no se instala en el host: se ejecuta con la imagen oficial fijada a
`postgres:17.6-bookworm`. Desde `web/`:

```bash
./scripts/preparar_postgresql.sh
```

Edite `ops/.env`; `POSTGRES_BACKUP_PATH` y `POSTGRES_WAL_ARCHIVE_PATH` deben ser
montajes cifrados fuera del volumen Docker, con propietario UID 999 y copia o
replicación fuera del servidor. Complete `ops/secrets/sql_server_origen` solo
para el corte. Ningún secreto se versiona.

`WEB_PUERTO` publica únicamente en `127.0.0.1` (8081 de forma predeterminada)
para que el proxy inverso institucional sea el único punto de entrada externo.

```bash
docker compose --env-file ops/.env -f ops/compose.production.yml config
docker compose --env-file ops/.env -f ops/compose.production.yml up -d postgres
MIGRACION_MANUAL_DBA=confirmada docker compose --env-file ops/.env \
  -f ops/compose.production.yml --profile migracion run --rm migracion upgrade head
docker compose --env-file ops/.env -f ops/compose.production.yml up -d --build api web
```

La API no ejecuta DDL. Si el volumen ya existía antes de crear los roles, no se
debe borrar: créelos manualmente o restaure en un volumen nuevo revisado.

## Respaldo y restauración

El servicio conserva un `pg_dump` lógico, roles globales, un `pg_basebackup`
físico con WAL y sumas SHA-256. El `archive_command` copia continuamente WAL al
montaje externo.

```bash
docker compose --env-file ops/.env -f ops/compose.production.yml \
  --profile respaldo run --rm respaldo
docker compose --env-file ops/.env -f ops/compose.production.yml \
  --profile verificacion_respaldo run --rm verificar_restauracion
```

La segunda orden verifica hashes y restaura el último dump en una base temporal,
que elimina al terminar. Programe el primer comando fuera de Compose (systemd o
la plataforma de respaldos), vigile la edad del último directorio `COMPLETADO`,
el crecimiento del archivo WAL, espacio, conexiones y latencia. Ensaye una
restauración mensualmente y antes de cada actualización.

Para recuperación a un punto en el tiempo, prepare un host aislado con la misma
versión mayor, extraiga `base.tar.gz`, incorpore `pg_wal.tar.gz`, configure
`restore_command` contra el archivo WAL externo y defina el objetivo de
recuperación. Nunca ensaye PITR sobre el volumen productivo.

## Corte único desde SQL Server

El usuario ODBC debe tener solo `SELECT`. La simulación es el modo por defecto:

```bash
docker compose --env-file ops/.env -f ops/compose.production.yml \
  --profile importacion run --rm importacion \
  /usr/local/bin/importar_sqlserver_postgresql.py \
  --reporte /trabajo/simulacion-2026.json
```

Solo si `errores` está vacío: respalde PostgreSQL, congele escrituras de
WinForms, repita la simulación y aplique:

```bash
docker compose --env-file ops/.env -f ops/compose.production.yml \
  --profile importacion run --rm importacion \
  /usr/local/bin/importar_sqlserver_postgresql.py --aplicar \
  --reporte /trabajo/aplicacion-2026.json \
  --credenciales /trabajo/credenciales-2026.csv
```

Se importan estudiantes/profesores activos, matrícula 2026, ruta, beca vigente,
plantillas y componentes. Se excluyen explícitamente marcas, ventas, saldos,
reservas, ingresos, contraseñas, PIN y auditoría. El CSV de PIN nuevos tiene
datos sensibles: entréguelo por canal seguro y destrúyalo después.

Reconcile conteos por tipo, sección, beca y ruta contra el reporte. Una
cédula ausente o duplicada y una ruta inválida bloquean toda la aplicación.

## Padrón anual XLSX

La primera hoja debe contener `cedula`, `nombres` y `tipo`; admite además
`seccion`, `becado`, `ruta` y `estado`. El padrón anual se registra en la única
población operativa y no admite selección de horario. Primero genere la vista previa:

```bash
docker compose --env-file ops/.env -f ops/compose.production.yml \
  --profile importacion run --rm importacion \
  /usr/local/bin/importar_padron_anual.py /trabajo/padron-2027.xlsx \
  --anio 2027 --reporte /trabajo/vista-previa-2027.json
```

Después de corregir todas las filas, repita con `--aplicar` y `--credenciales`.
La operación es transaccional, actualiza por cédula y crea una sola matrícula
por persona y año. El año importado no se activa automáticamente.

## Evidencia de validación 2026-08-30

- PostgreSQL `17.6` quedó saludable, sin publicar `5432`, con Alembic
  `0007_colores_rutas`.
- Se reconstruyeron las imágenes de API y web; `GET /api/v1/salud` respondió
  `{"estado":"ok","baseDatos":"postgresql"}`.
- El respaldo lógico, global, físico y WAL pasó la restauración temporal y la
  verificación de sus cuatro sumas SHA-256. El respaldo definitivo posterior
  al refuerzo del esquema es `/respaldos/20260830T232007Z`.
- La simulación de solo lectura contra `SCSC_MIGRACION_20260828` encontró 1.093
  personas activas: 943 estudiantes, 150 profesores, 596 estudiantes becados,
  19 rutas y 25 plantillas; no reportó errores bloqueantes.
- La aplicación transaccional creó 1.093 personas y credenciales, 943
  matrículas, 19 rutas, 943 asignaciones vigentes, 25 plantillas y 99
  componentes. La reconciliación confirmó 596 becados y cero marcas, ingresos
  o ventas históricas.
- Una segunda aplicación real actualizó las 1.093 personas sin crear ninguna,
  mantuvo todos los conteos y produjo un CSV sin credenciales nuevas. Esto
  verifica la idempotencia del corte.
- El primer intento de aplicación detectó un título histórico de 129 caracteres
  frente al límite anterior de 120; PostgreSQL revirtió la transacción completa.
  El contrato se amplió a 180, la simulación incorporó validación preventiva y
  la migración `0003` se aplicó antes del reintento satisfactorio.
- La vista previa del padrón XLSX 2027 se verificó con dos filas controladas y
  cero errores, sin aplicar escrituras.
- El esquema se reforzó con 13 restricciones de dominio, 13 índices de acceso
  y dos triggers de integridad. PostgreSQL ahora impide matricular profesores,
  cambiar a profesor una persona ya matriculada, abrir dos rutas vigentes para
  la misma matrícula, crear sesiones sin un único propietario y guardar
  estados o valores operativos fuera de contrato. Las credenciales y sesiones
  se eliminan en cascada con su propietario; los datos históricos conservan
  relaciones restrictivas.
- La migración inicial dejó de depender de la metadata ORM mutable y quedó
  congelada como DDL explícito. En una base temporal vacía se verificó la
  cadena completa `0001` → `0007` y su rollback hasta
  `base`; la base temporal se eliminó al finalizar.
- Pruebas: backend 21/21 y frontend 100/100; TypeScript y la compilación de
  producción fueron aprobados. Alembic completó `upgrade`, `downgrade base` y
  un segundo `upgrade` sobre una base temporal vacía.
- La validación final del 2026-08-31 aprobó Ruff, MyPy sobre los 39 módulos
  PostgreSQL activos, las reglas arquitectónicas y `alembic check`. La carga
  diferida dejó todos los fragmentos JavaScript de producción por debajo de
  400 kB sin comprimir.

### Depuración de nocturno 2026-08-30

- Se creó `web/ops/backups/scb_antes_eliminar_nocturno_20260830.dump` antes del cambio.
- Se eliminaron 214 matrículas con `turno = '2'`, sus 214 asignaciones y las 214
  personas exclusivamente nocturnas junto con sus dependencias.
- No existían personas con matrículas de ambos horarios.
- Se eliminaron `02-NARANJO` y `08-CONCEPCIÓN`, demostradas por el respaldo como
  rutas exclusivamente nocturnas; las rutas con al menos una asignación diurna se
  conservaron.
- La reconciliación final dejó 729 matrículas, cero matrículas nocturnas y cero
  identidades estudiantiles sin matrícula.

Después de la aplicación se creó y comprobó la cuenta `administrador`.
El CSV de 1.093 PIN temporales y la contraseña administrativa se retiraron del
workspace y se guardaron con permisos `0600` en
`/home/dev/scb-entrega-segura/`. No deben copiarse al repositorio ni enviarse
por canales sin cifrado.

La simulación no autoriza por sí sola el corte productivo. Antes de `--aplicar`
se mantienen como puertas manuales el respaldo inmediato, congelamiento de
WinForms, aprobación del reporte y custodia del CSV temporal de credenciales.
