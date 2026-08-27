# Despliegue seguro del portal de comedor

## Alcance y arquitectura

Estos artefactos despliegan el portal dentro de la infraestructura institucional. El contenedor `web` sirve la aplicación React y reenvía `/api` al contenedor `api`; la API es la única que puede conectar a SQL Server. El puerto SQL no se publica, ni se agregan credenciales a las imágenes o al repositorio.

El puerto local `127.0.0.1:8080` debe ser publicado al exterior exclusivamente por el proxy institucional con HTTPS y el dominio aprobado. El proxy debe preservar `Host` y `X-Forwarded-Proto`, aplicar redirección HTTP→HTTPS y limitar solicitudes según la política institucional. La API no tiene puertos publicados; el firewall de la red institucional debe permitir desde el host únicamente la salida necesaria hacia SQL Server (TCP 1433 o el puerto que indique el DBA).

La entrada única de producción es `aplicacion.entrada:crear_aplicacion`, cargada por Uvicorn con
`--factory`. El contenedor no inicia ningún módulo histórico del sistema local. El healthcheck interno consulta `GET /api/ready` y el
proxy expone el mismo contrato bajo `/api/health` y `/api/ready`.

`COMPOSE_PRIVATE_SUBNET`, `FORWARDED_ALLOW_IPS` y `TRUSTED_PROXY_CIDRS` deben describir la misma red privada del proxy Nginx. Infraestructura debe reemplazar el ejemplo por una red no superpuesta con las redes institucionales; no se permite `*`.

Variables requeridas para producción: `SQL_CONNECTION_STRING` (ODBC Driver 18 con `Encrypt=yes`),
`CORS_ORIGIN` (un único origen HTTPS) y `COMPOSE_PRIVATE_SUBNET`. Deben definirse también
`FORWARDED_ALLOW_IPS` y `TRUSTED_PROXY_CIDRS` con la red exacta del proxy. `COOKIE_SECURE` debe
permanecer en `true`; los límites de sesión y bloqueo tienen valores seguros documentados en
`web/ops/.env.example`.

## Prerrequisitos de staging

- Docker Engine y Docker Compose v2 en un host Linux institucional.
- DNS y certificado TLS administrados por infraestructura; la aplicación no termina TLS directamente.
- Conectividad privada API→SQL Server con TLS validado. El usuario SQL debe tener mínimo privilegio: lectura de las tablas existentes indispensables y lectura/escritura únicamente de `ComedorPortal`; sin DDL, `db_owner` ni acceso de navegador.
- La migración se ejecuta manualmente por el DBA, primero sobre una copia restaurada o staging. La API no ejecuta migraciones ni datos semilla al iniciar.
- El backend expone `GET /api/health` y `GET /api/ready`; ambos validan la configuración y ejecutan una consulta SQL mínima. Devuelven `200` solo si la aplicación puede operar. El proxy publica estas rutas bajo `/api` y no publica `/ready` sin el prefijo.

## Preparación y despliegue en staging

Desde `web/ops`, crear el archivo local de secretos y completar únicamente valores de staging:

```bash
cp .env.example .env
chmod 600 .env
docker compose --env-file .env -f compose.production.yml config
docker compose --env-file .env -f compose.production.yml build
docker compose --env-file .env -f compose.production.yml up -d
docker compose --env-file .env -f compose.production.yml ps
curl --fail http://127.0.0.1:8080/api/health
curl --fail http://127.0.0.1:8080/api/ready
```

Antes de abrir el proxy externo, ejecutar el smoke test con cuentas autorizadas: inicio de sesión, cambio de PIN, menú, confirmación, cancelación, permisos de Operador/Administrador, auditoría y una confirmación concurrente. En la prueba de estudiante, verificar además que al confirmar se muestre la hora de registro, **Confirmar almuerzo** quede deshabilitado y **No asistiré** retire la marca antes del cierre; sin confirmar deben mostrarse reloj y aviso de tiempo. Comprobar también `docker compose ... logs --tail=200 api` para confirmar que no aparezcan PINes, contraseñas, cookies ni cadenas de conexión.

## Promoción a producción

1. Obtener aprobación del DBA, respaldo verificado, plan de reversión y ventana de cambio.
2. Usar una cuenta de producción separada de staging y secretos entregados por el almacén institucional. La primera conexión de la aplicación debe ser solo de lectura para validar esquema, certificados y roles.
3. Ejecutar manualmente `001_menu_storage.sql`, `002_portal_state.sql` y `003_portal_settings.sql` en orden, tras la validación de staging. La última inicializa los cierres exclusivos del portal desde los horarios existentes y no modifica `dbo.Horario`. La API nunca ejecuta DDL y no se usan scripts prototipo.
4. Desplegar con el proxy público aún deshabilitado, ejecutar las pruebas de staging contra producción con cuentas autorizadas y verificar que la marca aparece en el reporte existente.
5. Habilitar el dominio HTTPS para un piloto controlado y monitorear errores, latencia, sesiones revocadas y duplicados.

### Comando automatizado

Tras completar los controles anteriores, el despliegue de código puede ejecutarse desde la raíz del repositorio con un único comando:

```bash
./web/scripts/deploy-production.sh api
```

El script usa el alias SSH `scsc-production`, sincroniza solo el componente solicitado, preserva `ops/.env` y demás secretos del servidor, reconstruye los contenedores necesarios y espera `GET /api/ready`. Los destinos `web` y `all` están disponibles cuando corresponda. Para inspeccionar archivos sin cambiar producción: `./web/scripts/deploy-production.sh api --dry-run`.

No guarda contraseñas ni ejecuta migraciones SQL. Estas siguen requiriendo el procedimiento y la aprobación del DBA.

## Reversión e incidentes

Para detener el servicio sin alterar datos históricos:

```bash
docker compose --env-file .env -f compose.production.yml down
```

Deshabilitar primero la ruta del proxy público y revocar las sesiones desde la herramienta administrativa prevista. No borrar confirmaciones ni auditoría para revertir. Una cancelación solo elimina una fila de `RegistroTransporte` si fue creada por el portal y está explícitamente vinculada con `MarcaCreadaPorPortal=1`; una marca originada por escritorio nunca se elimina ni se reinterpreta. Una restauración de SQL Server solo puede decidirla y ejecutarla el DBA según el respaldo y la ventana aprobados.

Si falla SQL Server, `ready` debe devolver error y las operaciones de asistencia deben fallar de forma atómica, sin reintentos ciegos. Guardar los registros de contenedor y el identificador de solicitud, rotar secretos potencialmente expuestos y seguir el procedimiento institucional de incidentes.

### Reversión de la entrada de aplicación

La reversión operativa consiste en detener el stack web, deshabilitar el proxy público y restaurar
la imagen previamente aprobada (`docker compose ... down` y despliegue de la etiqueta anterior).
No se activa ningún fallback histórico ni se ejecutan escrituras o migraciones desde
el contenedor. Si la etiqueta anterior no corresponde a la entrada modular, la reversión requiere
una decisión formal de operación; no se permite convivencia ni doble escritura.

## Controles previos a publicar

No se permite ejecutar contra producción hasta que staging complete las pruebas de confirmación, cancelación, concurrencia y recuperación de SQL. Configure `CORS_ORIGIN` como un único dominio HTTPS y `FORWARDED_ALLOW_IPS`/`TRUSTED_PROXY_CIDRS` con la red exacta del proxy inverso; nunca use `*`. Los Dockerfile canónicos son `web/ops/Dockerfile.api` y `web/ops/Dockerfile.migracion`; Nginx usa `web/ops/Dockerfile.frontend` y `web/ops/nginx/default.conf`.

Antes de fijar los límites de memoria de Compose, ejecute la medición operativa con la carga
representativa de staging. El resultado debe conservarse junto con la aprobación del entorno;
no se aceptan valores basados únicamente en el arranque en reposo. La puerta debe confirmar
que el uso de memoria no supera 70 %, no hay reinicios ni `OOMKilled`, la latencia no aumenta
fuera del objetivo aprobado y no aparecen errores 5xx. Deben conservarse los TSV fechados con
usuarios concurrentes, workers, duración, picos, latencia y errores.

La imagen `Dockerfile.migracion` no es una imagen de servicio: su entrada solo ejecuta Alembic
y rechaza el inicio sin `MIGRACION_MANUAL_DBA=confirmada`. El servicio Compose está bajo el
perfil `migracion`, excluido del arranque normal, sin reinicio automático. El DBA ejecuta
exclusivamente `CONFIRMAR_MIGRACION_DBA=SI ./web/scripts/validar_alembic_docker.sh upgrade`
desde una cuenta perteneciente a `GRUPO_DBA_MIGRACION` (por defecto, `dba`).
