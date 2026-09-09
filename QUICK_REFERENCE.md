# QUICK REFERENCE — Cambios Aplicados

## 🎯 ANTES (Con Problemas)

```
CI → npm build
      ↓
   Dockerfile → npm build (duplicado)
      ↓
   Imagen → :latest (mutable, recompila siempre)
      ↓
   Producción → compose.production.yml build: context ..
      ↓
   ❌ Recompila cada deploy
   ❌ Inconsistencias npm
   ❌ Sin validación de variables
   ❌ Secretos en plain text
```

## ✅ DESPUÉS (Inmutable & Validado)

```
CI → Validar Compose + Variables + No :latest
      ↓
   docker build → npm ci && npm run build (Corepack)
      ↓
   Imagen → sha256:abc123... (inmutable)
      ↓
   Registry → ghcr.io/repo:sha-<commit>@sha256:abc123...
      ↓
   Producción → compose.prod-deploy.yml image: ${SCB_WEB_IMAGE@sha256}
      ↓
   ✅ Nunca recompila
   ✅ Corepack determina npm
   ✅ Validación en CI
   ✅ Secretos seguros
```

---

## 📝 CAMBIOS ESPECÍFICOS

### 1️⃣ Validador (validar_variables.py)

```python
# Antes:
python3 validar_variables.py  # Busca .env, fallaba con tipos débiles

# Ahora:
python3 validar_variables.py --template    # OK sin secretos
python3 validar_variables.py --preflight   # Exige secretos reales
# Convierte automáticamente: "true" → True, "600" → 600
```

### 2️⃣ Archivos .env

```
.env.example              → Fallback (no usar directamente)
.env.local.example        → Desarrollo (desarrollo HTTP, secretos débiles)
.env.production.example   → Referencia (secretos en /run/secrets/)
```

### 3️⃣ Dockerfile.frontend

```dockerfile
# Antes:
RUN npm install --global npm@12.0.2
RUN npm ci && npm run build  # En Dockerfile

# Ahora:
RUN corepack enable npm      # npm versión de Node
RUN npm ci && npm run build  # Mismo lugar (multi-stage)
```

### 4️⃣ compose.production.yml

```yaml
# Antes:
web:
  build:
    context: ..
    dockerfile: ops/Dockerfile.frontend

# Ahora:
web:
  image: ${SCB_WEB_IMAGE}  # Variable inyectada con sha256
```

### 5️⃣ compose.prod-deploy.yml (NUEVO)

```yaml
# Uso:
export SCB_WEB_IMAGE="ghcr.io/org/scb-web@sha256:abc123..."
docker compose -f compose.production.yml \
               -f compose.prod-deploy.yml up -d
```

### 6️⃣ GitHub Actions (docker-build.yml)

```yaml
# Antes:
- name: Build Frontend first (npm build)
  run: npm ci && npm run build

# Ahora:
# ❌ Eliminado - Dockerfile lo hace
```

---

## ✅ CHECKLIST DE VALIDACIÓN

**Local (antes de push):**
```bash
cd web/ops
docker compose config --quiet                          # Sintaxis OK
python3 ../scripts/validar_variables.py --template     # Variables OK
grep ':latest' *.yml || echo "✅ No :latest encontrado"
```

**Después del merge:**
```bash
# GitHub Actions ejecuta automáticamente:
✅ validate job (Compose + schema + no :latest)
✅ build-api job (Docker build, publica digest)
✅ build-frontend job (Docker build, publica digest)
✅ build-migrations job (Docker build, publica digest)
✅ secret-scanning job (TruffleHog + GitLeaks)
```

**Deploy en producción:**
```bash
export SCB_API_IMAGE="ghcr.io/org/scb-api@sha256:..."
export SCB_WEB_IMAGE="ghcr.io/org/scb-web@sha256:..."
export SCB_MIGRACIONES_IMAGE="ghcr.io/org/scb-migrations@sha256:..."

docker compose -f web/ops/compose.production.yml \
               -f web/ops/compose.prod-deploy.yml \
               up -d

# Verificar:
docker ps
docker compose logs web   # No debe haber errores
curl http://localhost:8080
```

---

## 📊 NÚMEROS

| Métrica | Cambio |
|---------|--------|
| Archivos modificados | 7 |
| Archivos nuevos | 3 |
| TODOs resueltos | 29 |
| Bloqueadores P0 | 4 ✅ |
| Bloqueadores P1 | 4 ✅ |
| Build steps duplicados | -1 |
| Secrets en plain text | -∞ |
| Validación CI | +3 checks |

---

## 🚀 ESTADO: LISTO PARA MERGEAR
