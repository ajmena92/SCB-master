# Runbook de despliegue a producción

Este procedimiento aplica únicamente a `web/`. `escritorio/` es referencia
histórica y no participa en el despliegue ni en la ejecución.

## Puerta de entrada

No iniciar una ventana de producción si alguna condición falla:

- respaldo SQL Server verificado y restaurable;
- ventana aprobada, responsable DBA y plan de reversión;
- staging validado con la misma imagen y configuración de producción;
- revisión Alembic y esquema físico confirmados;
- `SQL_CONNECTION_STRING` con `Encrypt=yes`, usuario de mínimo privilegio y sin DDL para la API;
- `COOKIE_SECURE=true`, `CORS_ORIGIN` HTTPS y redes de proxy coincidentes;
- pruebas de login, permisos, menú, asistencia, reportes y cierre de sesión aprobadas.

La API no ejecuta migraciones al iniciar. No se debe promover el contenedor si el
esquema físico todavía conserva columnas o tablas de una revisión anterior que la
aplicación canónica ya no utiliza.

## 1. Preparar la versión

Desde la raíz del repositorio, en la revisión aprobada:

```bash
git diff --check
cd web/frontend
npm ci
npm run typecheck
npm test
npm run build
cd ../backend
pytest -q
cd ../..
```

La cuenta de despliegue debe tener el alias SSH `scsc-production`. El archivo
`web/ops/.env` se crea únicamente en el host de producción desde el almacén
institucional; nunca se sincroniza desde el repositorio.

## 2. Validar y aplicar la migración total

La migración de datos y estructura se ejecuta antes del código nuevo y por una
cuenta perteneciente a `GRUPO_DBA_MIGRACION` (por defecto, `dba`):

```bash
CONFIRMAR_MIGRACION_DBA=SI \
./web/scripts/validar_alembic_docker.sh current

CONFIRMAR_MIGRACION_DBA=SI \
./web/scripts/validar_alembic_docker.sh check

CONFIRMAR_MIGRACION_DBA=SI \
./web/scripts/validar_alembic_docker.sh upgrade
```

`upgrade` debe ejecutarse primero sobre una copia restaurada y luego sobre
staging. El DBA debe conservar la salida de `current`, el respaldo, la revisión
aplicada y los conteos de cada dominio.

### Transferencia de datos incluida

La migración total comprende estructura y datos existentes. Antes de ejecutar el
despliegue, el DBA debe:

1. registrar conteos y claves de las tablas fuente;
2. ejecutar las revisiones de transferencia idempotentes;
3. validar conteos, claves, relaciones y muestras funcionales;
4. conservar las fuentes hasta completar la reconciliación en producción.

Para el menú, `0015_migra_menu_historico`/`019_migra_menu_historico.sql`
transfiere `ComedorPortal.MenuPlantilla` a `menu.plantilla` y sus componentes a
`menu.componente`. `0016_completa_componentes_menu`/`020_completa_componentes_menu.sql`
completa los componentes y conserva los nombres largos sin truncarlos. Estas
revisiones son idempotentes: repetirlas no debe duplicar plantillas ni
componentes.

La reconciliación mínima del menú debe confirmar que coinciden los conteos de
plantillas y componentes, que cada pareja semana/día existe una sola vez y que
cada componente conserva su orden y plantilla. Para los demás dominios se debe
aplicar la revisión de transferencia correspondiente incluida en la versión
aprobada; no se acepta crear tablas canónicas vacías dejando los datos en
tablas antiguas.

El retiro de una fuente solo se permite después de una prueba funcional contra
el almacenamiento canónico, un respaldo verificado y la aprobación explícita
del DBA. Hasta entonces las tablas históricas permanecen intactas.

La transferencia inicial del padrón institucional se ejecuta con
`web/sql/migrations/021_transfiere_padron_web.sql`. Copia estudiantes del tipo
institucional 2, rutas, becas y asignaciones, conserva las tablas fuente y
requiere reconciliar los conteos antes de retirarlas. Los PIN binarios del
sistema anterior requieren generar credenciales web nuevas; no se convierten
silenciosamente a Argon2.

La aplicación solo se habilita cuando todos los módulos canónicos están
disponibles y sus relaciones funcionan: identidad, estudiantes, transporte,
asistencia, beneficios, cuentas, reportes, importaciones, menú, comedor,
soporte, auditoría y parámetros. No se ejecutan a la vez Alembic y los scripts
SQL manuales para las mismas tablas.

### Puerta específica de comedor

Antes de abrir el proxy público, ejecutar en staging el recorrido de un estudiante
becado_comedor, un estudiante `no_becado_comedor` y un profesor. Verificar que el becado no consume
tiquete, que los otros dos no pueden reservar ni ingresar sin tiquete, que una reserva
cancelada libera el saldo y que el ingreso consume una sola unidad ante solicitudes
concurrentes. Verificar también que los profesores no aparecen en las estadísticas
estudiantiles y sí aparecen al seleccionar explícitamente la vista de profesores.

Las revisiones `0023_registro_comedor_modalidad`, `0028_horarios_operacion_comedor` y
`0029_uso_transporte_y_auditoria_comedor` deben validarse con `current`, `check` y
`upgrade` en una copia restaurada antes de staging. `0028` crea los horarios web que
consume `parametros`; `0029` crea la lectura diaria de transporte y agrega la
trazabilidad del horario y hora propia del ingreso. La API no usa la hora del
transporte para decidir si una marca de comedor es tardía.
La API no ejecuta DDL, no compra tiquetes y no elimina tablas históricas al iniciar.

## 3. Desplegar el código

Con la migración aprobada:

```bash
./web/scripts/deploy-production.sh all
```

El script sincroniza `backend/`, `frontend/` y `ops/`, conserva los secretos del
servidor, reconstruye `api` y `web`, y espera `GET /api/ready`. Para inspeccionar
sin modificar producción:

```bash
./web/scripts/deploy-production.sh all --dry-run
```

No usar `--dry-run` como evidencia de que los contenedores arrancan; después de
la aprobación debe ejecutarse el despliegue real en la ventana autorizada.

## 4. Verificación posterior

Con el proxy público todavía cerrado:

```bash
ssh scsc-production 'cd /home/plat/scsc-comedor && \
  docker compose --env-file ops/.env -f ops/compose.production.yml ps'

ssh scsc-production 'curl --fail --silent --show-error \
  http://127.0.0.1:8081/api/health'
ssh scsc-production 'curl --fail --silent --show-error \
  http://127.0.0.1:8081/api/ready'
```

El smoke test autorizado debe comprobar login administrativo y estudiantil,
CSRF, permisos de operador/administrador, Dashboard, plantillas de menú,
confirmación y cancelación de asistencia, reportes, cambio de PIN y cierre de
sesión. Revisar también:

```bash
ssh scsc-production 'cd /home/plat/scsc-comedor && \
  docker compose --env-file ops/.env -f ops/compose.production.yml logs --tail=200 api'
```

Los logs no pueden contener contraseñas, PIN, cookies, cadenas SQL ni datos
personales innecesarios.

## 5. Retiro de tablas históricas

Antes de activar el kiosco, aplicar Alembic hasta la revisión `head`, confirmar al
menos `0037_valida_horarios_operativos` y hacerlo con respaldo y escrituras
congeladas. Revisar y resolver los casos de
`comedor.reconciliacion_migracion`; no convertir ambigüedades automáticamente.
La revisión `0030` agrega políticas y auditoría, y `0031` registra diferencias de
rutas múltiples y saldos negativos sin borrar históricos.

Ejecutar la reconciliación comparativa en modo lectura y conservar el JSON del
resultado como evidencia del corte:

```bash
docker run --rm --network scsc-comedor-local_private \
  --env-file ops/.env.local -e MIGRACION_MANUAL_DBA=confirmada \
  --entrypoint python scsc-comedor-migracion \
  scripts/reconciliar_migracion_comedor.py
```

Después de revisar las diferencias, repetir con `--apply` para persistirlas en
`comedor.reconciliacion_migracion`. No se debe activar el kiosco mientras existan
hallazgos no resueltos que afecten conteos, saldos, carnés u horarios.

El despliegue no elimina tablas. Después de validar staging, respaldo y conteos
canónicos, el DBA puede ejecutar por separado:

```bash
CONFIRMAR_MIGRACION_DBA=SI \
CONFIRMAR_BORRADO_TABLAS_DEPRECIADAS=SI \
./web/scripts/retirar_tablas_menu_legacy.sh
```

La rutina está limitada a `ComedorPortal.MenuComponente` y
`ComedorPortal.MenuPlantilla`. Comprueba que existan las tablas canónicas,
compara sus conteos, elimina primero la tabla hija y aborta ante cualquier
diferencia. No retirar otras tablas `ComedorPortal` sin una migración y una
aprobación independiente.

## Reversión

Si falla el smoke test, cerrar el proxy, detener el stack y volver a la imagen
aprobada anterior. No hacer downgrade automático ni borrar datos:

```bash
ssh scsc-production 'cd /home/plat/scsc-comedor && \
  docker compose --env-file ops/.env -f ops/compose.production.yml down'
```

La restauración o reversión de SQL Server solo la decide y ejecuta el DBA según
el respaldo y el plan aprobado. La rutina de retiro de tablas no forma parte de
la reversión.
