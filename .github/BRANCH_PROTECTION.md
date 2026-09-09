# Protección de `main`

La rama canónica es `main`. La configuración se realiza en GitHub mediante un
ruleset; este repositorio no intenta modificarla desde un workflow ni desde un
script que requiera privilegios administrativos.

## Reglas requeridas

- Exigir pull request antes de fusionar y mantener la rama al día.
- Exigir una revisión aprobada; activar revisión de Code Owners cuando el
  repositorio cuente con el archivo correspondiente.
- Bloquear `force push`, eliminaciones y cambios directos, incluidos los de
  administradores salvo una excepción operativa aprobada.
- Exigir los checks que se ejecutan para todo PR dirigido a `main`:
  `Frontend`, `Backend`, `Integración E2E` y `Secretos`.
- Permitir el bypass únicamente al grupo responsable de incidentes y dejar
  evidencia en el PR.

No se debe requerir un check que se pueda omitir por filtros de rutas,
ejecuciones manuales o dependencias externas. `Memoria de staging` es una
puerta manual y no forma parte de los checks obligatorios.

## Acciones de administración

La persona administradora configura el ruleset en **Settings → Rules → Rulesets**
y verifica los checks publicados por la primera ejecución remota exitosa. No se
crean secretos adicionales para este propósito: GitHub Actions entrega
`GITHUB_TOKEN` con los permisos declarados en cada workflow.

La publicación en GHCR usa ese token efímero. Si se cambia a un registro externo,
la credencial del registro se define como secreto del entorno de publicación y
nunca como variable de build, archivo `.env` versionado o argumento de Docker.
