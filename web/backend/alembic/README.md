# Migraciones Alembic canónicas

Este directorio versiona exclusivamente el modelo web nuevo. No importa ni
modifica `server.py`, WinForms, `dbo` o `Seguridad`.

El modo online requiere `DATABASE_URL` de PostgreSQL y se ejecuta manualmente por
el DBA. El modo offline no requiere conexión ni credenciales; usa el dialecto
sin secretos declarado en `alembic.ini`:

```bash
cd web/backend
alembic upgrade head --sql > /tmp/migracion-web.sql
```

Para aplicar online, exportar la cadena desde el almacén institucional, sin
guardarla en el repositorio:

```bash
DATABASE_URL='postgresql+psycopg://(obtenida-del-almacen)' alembic upgrade head
```

La aplicación no ejecuta Alembic al iniciar. El `downgrade` es deliberado y
debe revisarse antes de usarlo en una base con datos.

La imagen del API no contiene Alembic. Para validar o ejecutar migraciones se usa la
imagen aislada `Dockerfile.migracion` y el perfil Compose `migracion`; en local, sus
dependencias están en `requirements-migracion.txt`. Las pruebas completas usan
`requirements-desarrollo.txt`.

Para validar staging sin aplicar DDL, exporte la cadena desde el almacén
institucional y ejecute `web/scripts/validar_alembic_staging.sh`. El script
solo ejecuta `alembic current`, exige una URL PostgreSQL y no
imprime la cadena ni sus credenciales.

En el host de staging se recomienda validar dentro de la imagen aislada de
migración ejecutando `web/scripts/validar_alembic_docker.sh current`.
Después de revisar el estado y contar con aprobación del DBA, `... upgrade`
aplica la revisión pendiente. Ambos comandos leen `ops/.env` mediante Compose,
no muestran `DATABASE_URL` y no se ejecutan al iniciar la API.
