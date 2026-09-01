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

Los secretos se suministran mediante `web/ops/.env` y archivos de Docker
secrets, nunca en imágenes, commits, logs ni parámetros visibles. En producción
`COOKIE_SECURE=true`, `CORS_ORIGIN` es un único origen HTTPS y las redes de
proxy se declaran explícitamente, sin comodines.

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

Después se levantan las imágenes y se comprueba el servicio:

```bash
docker compose --env-file .env -f compose.production.yml up -d --build api web
docker compose --env-file .env -f compose.production.yml ps
curl --fail --silent --show-error http://127.0.0.1:8081/health
```

El procedimiento ampliado, incluidas las puertas de datos y reversión, está en
[RUNBOOK_DEPLOY_PRODUCCION.md](RUNBOOK_DEPLOY_PRODUCCION.md).

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

El script preserva secretos, reconstruye los servicios solicitados y espera
`GET /health`. No ejecuta migraciones ni elimina datos. Para inspeccionar la
sincronización use `--dry-run`.

Si falla el smoke test, cerrar el proxy, conservar logs y detener `api` y `web`.
Se vuelve a la imagen aprobada anterior; no se activa WinForms, no se habilita
doble escritura y no se ejecuta un downgrade automático. La restauración de
PostgreSQL se decide según el respaldo verificado y la ventana del DBA.
