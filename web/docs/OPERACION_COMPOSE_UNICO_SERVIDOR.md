# Operación pendiente: Compose en un único servidor

El proyecto se despliega exclusivamente con Docker Compose en un servidor. No
hay manifiestos ni operación Kubernetes.

## Completado en el repositorio

- CI construye y publica las seis imágenes requeridas, con SBOM y provenance.
- Producción solo acepta imágenes por digest mediante `compose.prod-deploy.yml`.
- El script remoto no sincroniza código fuente ni reconstruye imágenes.
- La validación de templates, Compose y el preflight de secretos está cubierta
  por CI y pruebas unitarias.

## Acciones que requieren acceso al entorno externo

1. En GitHub, conceder a `GITHUB_TOKEN` permiso `packages: write` y exigir los
   workflows `Validate Docker Compose`, `Build runtime image` y `Build frontend
   image` antes de fusionar a `main`.
2. En el servidor, guardar los seis `SCB_*_IMAGE` por digest en `ops/.env`, con
   copia del conjunto anterior fuera de Git para rollback.
3. Instalar una tarea diaria del usuario administrador, por ejemplo a las 02:15:

   ```cron
   15 2 * * * cd /home/plat/scsc-comedor && ./web/scripts/deploy-production.sh api --remote --dry-run >> /var/log/scb/preflight.log 2>&1
   ```

   Ajustar la ruta y el usuario. La tarea solo valida; no despliega ni ejecuta
   migraciones. El respaldo y su restauración se programan por separado en la
   ventana DBA aprobada.
4. Configurar una alerta del sistema de monitoreo existente cuando falle ese
   comando, no exista un respaldo verificable o `/health` responda fuera de 2xx.

No se ha creado una tarea programada ni se han tocado credenciales, secretos,
imágenes remotas, base de datos o contenedores del servidor desde el repositorio.
