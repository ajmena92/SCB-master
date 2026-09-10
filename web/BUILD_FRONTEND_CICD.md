# Build Frontend en CI/CD — Flujo Real

## Principio Fundamental

**El Dockerfile es la fuente única de verdad para construir imágenes.**

- CI/CD **NO** ejecuta npm build antes del Dockerfile
- El Dockerfile contiene la fase multi-stage que compila el frontend
- CI valida sintaxis Compose y esquema de variables; el build real ocurre en `docker build`
- Las imágenes se publican con digest SHA256 inmutable

---

## Flujo Actual (Correcto)

### 1. Push a rama (PR o merge a master)

```bash
git push origin feature/cambio
# o: git push origin master
```

### 2. GitHub Actions ejecuta `.github/workflows/docker-build.yml`

**Job: validate**
- ✅ Valida sintaxis Compose: `docker compose config --quiet`
- ✅ Valida estructura de variables: `python3 web/scripts/validar_variables.py --template local|production`
- ✅ Busca `:latest` prohibido: `grep ':latest' web/ops/compose.production.yml`

**Job: build-frontend**
- ✅ Ejecuta `docker build web -f web/ops/Dockerfile.frontend`
  - **Stage 1 (build)**: Ejecuta `npm ci` + `npm run build`
  - **Stage 2 (runtime)**: Copia `/app/dist` → `/usr/share/nginx/html`
- ✅ Publica imagen: `ghcr.io/org/repo-web:sha-<commit-hash>`
- ✅ Publica digest: `ghcr.io/org/repo-web@sha256:abc123...`

### 3. Deploy en producción (manual o automático)

```bash
# En el servidor, registrar los digests en el archivo protegido ops/.env:
SCB_WEB_IMAGE=ghcr.io/org/repo-web@sha256:abc123...
SCB_API_IMAGE=ghcr.io/org/repo-api@sha256:def456...
SCB_MIGRACIONES_IMAGE=ghcr.io/org/repo-migrations@sha256:ghi789...
SCB_TRABAJADOR_IMPORTACION_IMAGE=ghcr.io/org/repo-trabajador@sha256:jkl012...

# Usar override con digests inmutables
docker compose \
  -f web/ops/compose.production.yml \
  -f web/ops/compose.prod-deploy.yml \
  up -d
```

---

## Cambios de Arquitectura (Fixes)

| Cambio | Antes | Después | Razón |
|--------|-------|---------|-------|
| **Build duplicado** | CI + Dockerfile | Solo Dockerfile | Elimina inconsistencias |
| **npm global** | `npm install --global npm@12.0.2` | `corepack enable npm` | Usa versión de Node + lockfile |
| **Imagen en Compose** | `build: context: .. dockerfile: ...` | `image: ${SCB_WEB_IMAGE}` | Produce deploy inmutable |
| **Producción** | Recompila en deploy | Usa digest prefabricado | Reproducible, no sorpresas |

---

## Validación Local (Antes de Push)

### 1. Validar estructura Compose

```bash
cd web/ops
docker compose config --quiet
python3 ../scripts/validar_variables.py --template local
python3 ../scripts/validar_variables.py --template production
grep ':latest' *.yml  # No debe encontrar nada
```

### 2. Construir frontend localmente (sin CI)

```bash
cd web
docker build -f ops/Dockerfile.frontend -t scb-web:local .
docker run --rm -p 8080:8080 scb-web:local
# Visitar: http://localhost:8080
```

### 3. Validar producción (preflight)

```bash
# Solo si tienes .env real con secretos
cd web/ops
python3 ../scripts/validar_variables.py --preflight /ruta/protegida/.env
```

---

## Deploy en Producción (Checklist)

```bash
[ ] Merge a master → CI ejecuta workflows
[ ] CI publica imágenes con digest (ve a GHCR)
[ ] Copiar digest desde release notes de CI
[ ] Registrar variables SCB_*_IMAGE=...@sha256:... en ops/.env protegido
[ ] docker compose -f compose.production.yml -f compose.prod-deploy.yml up -d
[ ] Verificar: docker ps (deben estar en estado healthy)
[ ] Verificar: curl http://localhost:8081 (o tu puerto)
[ ] Verificar: docker compose logs web (sin errores de dist)
```

---

## Troubleshooting

### "dist no encontrado en imagen"
→ El Dockerfile Stage 2 no copiaba correctamente. Verificar:
```dockerfile
COPY --from=build /app/dist /usr/share/nginx/html
```

### "sha-<commit> tiene 2-3 imágenes con el mismo tag"
→ Correcta operación. Cada push recibe un digest único. Los tags son referencias humanas.

### "Imagen no encontrada al hacer docker compose pull"
→ Variable `SCB_WEB_IMAGE` no exportada o malformada. Revisar:
```bash
echo $SCB_WEB_IMAGE  # Debe estar: ghcr.io/...@sha256:...
```

### "Validador dice que faltan secretos en .env.example"
→ No debe faltar. El validador ahora distingue `--template` (no exige secretos) vs `--preflight` (sí).
```bash
python3 validar_variables.py --template production  # OK sin secretos
python3 validar_variables.py --preflight /ruta/protegida/.env  # Exige secretos reales
```

---

## Nota: Esta Documentación Reemplaza

- ❌ Promesa de "Dockerfile referencia sha-<commit>" → ✅ Ahora: Compose.prod-deploy.yml override
- ❌ "Build duplicado en CI + Dockerfile" → ✅ Ahora: Solo Dockerfile
- ❌ "npm @12.0.2 global" → ✅ Ahora: Corepack + Node 24.19
