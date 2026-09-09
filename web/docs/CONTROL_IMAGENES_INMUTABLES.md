# Control de avance: imágenes inmutables y operación Compose

**Fecha:** 8 de septiembre de 2026  
**Alcance:** desarrollo local, CI y despliegue Compose en un único servidor.

## Avance aplicado

- El frontend se construye dentro de `Dockerfile.frontend` mediante etapas
  Node/Nginx; CI no ejecuta una compilación duplicada antes de crear la imagen.
- `compose.local.yml` declara los builds de los seis servicios construibles. El
  entorno local sigue construyendo desde el código y conserva su volumen externo.
- `compose.prod-deploy.yml` sustituye los builds por imágenes de API, frontend,
  migración, trabajador, importación y analítica identificadas por digest.
- `deploy-production.sh --remote` valida los seis digests, hace `pull` y usa
  `up -d --no-build --no-deps`. No construye código en el servidor productivo.
- Los templates local y de producción se validan por separado. Los secretos solo
  se comprueban en un preflight contra un archivo real, sin mostrar su contenido.

## Puertas obligatorias antes de publicar

1. Validar ambos templates y las composiciones local y de producción en CI.
2. Publicar imágenes, SBOM y provenance desde CI.
3. Registrar los seis digests aprobados en el archivo de entorno protegido del
   servidor; nunca en Git ni como argumentos de línea de comando.
4. Ejecutar el preflight, respaldar cuando aplique y desplegar por digest.
5. Conservar el conjunto anterior de digests como rollback explícito.

## Límites y pendientes externos

- La configuración externa de GitHub (reglas de rama, secretos y credenciales
  de GHCR) requiere autorización y se realiza fuera del repositorio.
- Las acciones de GitHub están fijadas a SHA verificados contra sus etiquetas de
  versión; se revisan de forma deliberada al actualizar cada acción.
- Kubernetes no forma parte de este proyecto. La operación y observabilidad se
  mantienen en el servidor Compose.

## Evidencia de esta fase

- `validar_variables.py --template local`
- `validar_variables.py --template production`
- validación sintáctica de Bash, YAML y `git diff --check`

No se inició Docker, no se aplicaron migraciones y no se modificaron secretos
reales durante esta fase.
