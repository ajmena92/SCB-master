# Despliegue seguro de la plataforma web

## Alcance

`web/` es el único producto desplegable. El sistema WinForms y SQL Server son
fuentes históricas del corte y no participan en tiempo de ejecución. El stack
usa React/Nginx, FastAPI y PostgreSQL 17.6; solo Nginx publica
`127.0.0.1:8081`. La API y PostgreSQL permanecen en la red privada de Compose.

La entrada del backend es `aplicacion.entrada:crear_aplicacion`. La API no
ejecuta DDL al arrancar y su cuenta PostgreSQL no tiene permisos para hacerlo.
Las migraciones se ejecutan con la imagen aislada `migracion`. El proxy publica
la comprobación canónica `GET /health`, que reenvía a `GET /api/v1/salud`.

Los secretos se suministran mediante referencias en `web/ops/.env` y archivos de Docker
secrets, nunca en imágenes, commits, logs ni parámetros visibles. En producción
`COOKIE_SECURE=true`, `CORS_ORIGIN` es un único origen HTTPS y las redes de
proxy se declaran explícitamente, sin comodines.

`CARNET_QR_CLAVE_FILE` y `CSRF_SECRET_FILE` deben referir archivos de secreto
con permisos mínimos. La clave QR no se rota como parte de esta migración: su
rotación exige la ventana operativa y la reemisión de carnets aprobadas.
`CSRF_ANONYMOUS_TTL_SECONDS` es obligatoria y debe estar entre 60 y 3600; el
valor recomendado es 600. No contiene un secreto y controla únicamente la
vida de la cookie CSRF previa al inicio de sesión.

## Preparación

Desde `web/ops`:

```bash
cp .env.example .env
chmod 600 .env
docker compose --env-file .env -f compose.production.yml config --quiet
docker compose --env-file .env -f compose.production.yml build
```

Antes de cada migración se crea y se restaura un respaldo de verificación:

```bash
docker compose --env-file .env -f compose.production.yml \
  --profile respaldo run --rm respaldo
docker compose --env-file .env -f compose.production.yml \
  --profile verificacion_respaldo run --rm verificar_restauracion
```

El DBA aplica Alembic con el proxy todavía cerrado:

```bash
CONFIRMAR_MIGRACION_DBA=SI ../scripts/validar_alembic_docker.sh current
CONFIRMAR_MIGRACION_DBA=SI ../scripts/validar_alembic_docker.sh check
CONFIRMAR_MIGRACION_DBA=SI ../scripts/validar_alembic_docker.sh upgrade
```

Después de la migración aprobada, actualice cada servicio sin evaluar ni
recrear dependencias. El script ejecuta un preflight de solo lectura antes de
construir: valida Compose, secretos sin imprimirlos, salud de API/PostgreSQL y,
en el servidor, almacenamiento, respaldo y checksum.

```bash
./web/scripts/deploy-production.sh api --remote
./web/scripts/deploy-production.sh web --remote
```

Cada actualización usa `compose build <servicio>` y
`compose up -d --no-deps <servicio>`; no use `up -d --build api web` como
rutina de publicación. `all` requiere la confirmación DBA y conserva la
secuencia de migración explícita.

El procedimiento ampliado, incluidas las puertas de datos y reversión, está en
[RUNBOOK_DEPLOY_PRODUCCION.md](RUNBOOK_DEPLOY_PRODUCCION.md).

## Desarrollo local canónico

Para desarrollo local, el único comando de arranque admitido es:

```bash
./web/scripts/levantar-desarrollo-local.sh
```

Este comando usa el volumen externo `scb-web_postgres_datos`, levanta
PostgreSQL, API y web en `http://127.0.0.1:8082`, no ejecuta migraciones y no
borra volúmenes. Para reutilizar imágenes ya construidas use
`--sin-construir`; para otro puerto, `--puerto 8083`. No use `docker compose
down -v` en el entorno de desarrollo. Las migraciones y restauraciones se
ejecutan únicamente mediante sus procedimientos DBA documentados.

## Cuentas administrativas y permisos

Toda cuenta administrativa nueva se vincula uno a uno con una persona activa
registrada como profesor. Un administrador puede seleccionar un profesor sin
cuenta o registrar uno nuevo. En este último caso se generan por separado un
PIN temporal del portal docente y una contraseña administrativa temporal; solo
se muestran una vez y deben entregarse por un canal seguro.

La cuenta administradora creada antes de esta migración conserva su usuario y
contraseña. Tras el despliegue queda en vinculación pendiente y solo puede
consultar su sesión, cerrar sesión, cambiar su contraseña y completar la
vinculación inicial. No se elige automáticamente uno de los profesores de
producción. Las contraseñas temporales obligan a un cambio antes de operar.

La API consulta la cuenta, el profesor y los permisos vigentes en PostgreSQL en
cada solicitud. El rol `administrador` tiene acceso completo. El rol `operador`
solo accede a los permisos asignados explícitamente; ocultar una opción del menú
no reemplaza esta comprobación. Cambiar rol, permisos, estado o contraseña
revoca todas las sesiones de la cuenta.

## Smoke test previo a publicar

Con cuentas controladas, verificar:

- vinculación inicial del administrador sin cambiar su contraseña existente;
- creación de operador con profesor existente y con profesor nuevo;
- visualización única de credenciales temporales y cambio obligatorio;
- rechazo de módulos y URLs sin permiso, y acceso inmediato al concederlo;
- revocación inmediata al retirar permisos, desactivar o restablecer contraseña;
- protección contra auto-desactivación y contra eliminar el último administrador;
- login estudiantil con cédula/PIN, carné, menú, comedor, rutas, gráficos y reportes;
- ausencia de contraseñas, PIN, cookies o datos personales innecesarios en logs.

## Promoción y reversión

El código puede promoverse desde la raíz con:

```bash
./web/scripts/deploy-production.sh all
```

El script preserva secretos, reconstruye solo los servicios solicitados y los
actualiza con `--no-deps`; por tanto una publicación de frontend o API no
recrea PostgreSQL. Antes de publicar realiza un preflight bloqueante de solo
lectura. No elimina datos. Para inspeccionar la sincronización use `--dry-run`.

Si falla el smoke test, cerrar el proxy, conservar logs y detener `api` y `web`.
Se vuelve a la imagen aprobada anterior; no se activa WinForms, no se habilita
doble escritura y no se ejecuta un downgrade automático. La restauración de
PostgreSQL se decide según el respaldo verificado y la ventana del DBA.
