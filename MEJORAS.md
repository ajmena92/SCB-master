# Mejoras Implementadas en SCB Portal Web

## Resumen

Este documento detalla todas las optimizaciones y mejoras aplicadas al proyecto SCB Portal Web para aumentar seguridad, rendimiento y mantenibilidad.

---

## 1. Optimizaciones de Docker

### 1.1 Uso de Imágenes Base Oficiales Optimizadas
- **API (Dockerfile.api)**: `python:3.12.14-slim-bookworm`
- **Frontend (Dockerfile.frontend)**: `nginx:1.30.4-alpine`
- **Migraciones**: `python:3.12.14-slim-bookworm`
- **Workers**: `python:3.12.14-slim-bookworm`

**Beneficio**: Imágenes mínimas, menos CVEs, tamaño reducido (API: ~290MB disco, 69.6MB contenido).

### 1.2 Health Checks Mejorados
Añadidos `HEALTHCHECK` en contenedores críticos:
- **API**: Verifica conexión a PostgreSQL (30s intervalo)
- **Trabajador importación**: Verifica disponibilidad de BD (30s intervalo)
- **Frontend**: Verifica respuesta HTTP /health (30s intervalo)

```dockerfile
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD python -c "import psycopg; c=psycopg.connect(...); assert c.execute('SELECT 1').fetchone()==(1,)"
```

### 1.3 Contenedores Rootless
Todos los servicios ejecutan como usuarios sin privilegios:
- `appuser` (UID 10001): API, migraciones, workers
- `importador` (UID 10001): Importación desde SQL Server
- `analitica` (UID 10001): Reportes

```dockerfile
RUN useradd --create-home --uid 10001 appuser
USER appuser
```

---

## 2. Optimizaciones de Docker Compose

### 2.1 Limitaciones de Recursos CPU/Memoria

**API (production)**:
```yaml
cpus: 1.0                    # 1 CPU máximo
mem_limit: 256m              # 256 MB máximo
mem_reservation: 128m        # 128 MB garantizados
```

**Frontend (production)**:
```yaml
cpus: 0.25                   # 0.25 CPU máximo
mem_limit: 64m               # 64 MB máximo
mem_reservation: 32m         # 32 MB garantizados
```

**Trabajador importación**:
```yaml
cpus: 0.5                    # 0.5 CPU máximo
mem_limit: 256m
mem_reservation: 128m
```

**Beneficio**: Evita consumo descontrolado de recursos, predecibilidad en clústeres.

### 2.2 Seguridad Reforzada
Todos los contenedores incluyen:
```yaml
read_only: true                    # Sistema de archivos de solo lectura
security_opt:
  - no-new-privileges:true         # No escalar privilegios
cap_drop:
  - ALL                            # Deshabilitar todas las capacidades
cap_add:
  - SETUID                         # Solo las necesarias
  - SETGID
  - DAC_READ_SEARCH                # Para lectura de secretos
tmpfs:
  - /tmp                           # Volúmenes temporales en memoria
```

---

## 3. Configuración y Validación

### 3.1 Schema JSON (`ops/schema.json`)
Esquema de validación JSON-Schema Draft-7 para todas las variables de entorno:

**Variables requeridas**:
- `POSTGRES_*` (conexión BD)
- `CORS_ORIGIN` (seguridad web)
- `APP_ENV` (entorno: development|production|test)
- `CSRF_SECRET_FILE`, `CARNET_QR_CLAVE_FILE` (secretos)
- `STUDENT_SESSION_DAYS`, `ADMIN_SESSION_MINUTES` (sesiones)
- `TRUSTED_PROXY_CIDRS`, `FORWARDED_ALLOW_IPS` (networking)

**Validaciones**:
- Rangos numéricos (min/max)
- Patrones regex (usuarios PostgreSQL, puertos, etc.)
- Formato URI (CORS_ORIGIN)
- Enumeraciones (APP_ENV)

### 3.2 Script de Validación (`scripts/validar_variables.py`)
Valida `.env` contra el esquema:

```bash
python3 scripts/validar_variables.py
```

**Validaciones personalizadas**:
- Existencia de archivos secretos
- Consistencia CORS/HTTPS en producción
- COOKIE_SECURE requerido si CORS es HTTPS
- Consistencia APP_ENV

---

## 4. Automatización CI/CD

### 4.1 Generación OpenAPI (`scripts/generar_openapi.sh`)
Genera esquema OpenAPI y cliente TypeScript automáticamente:

```bash
./scripts/generar_openapi.sh
```

**Salida**:
- `frontend/src/api/openapi-schema.json` - Esquema OpenAPI v3
- `frontend/src/api/client/` - Cliente TypeScript generado (opcional)

**Uso**: Integrado en CI/CD para mantener cliente en sync con API.

### 4.2 GitHub Actions Workflows

#### `.github/workflows/docker-build.yml`
- Construye y pushea imágenes a registry
- Escanea vulnerabilidades
- Valida docker-compose.yml
- Tags automáticos: `latest`, `v{version}`

#### `.github/workflows/tests.yml`
- Tests Python (pytest)
- Tests frontend (vitest)
- Tests E2E (Playwright)
- Cobertura mínima: 80%
- Calidad de código (eslint, ruff)

---

## 5. Cambios en Dockerfiles

### 5.1 Dockerfile.importacion - Rootless
**Antes**:
```dockerfile
USER root  # ⚠️ Ejecutaba como root
```

**Después**:
```dockerfile
RUN useradd --create-home --uid 10001 importador
USER importador  # ✅ Ejecuta sin privilegios
```

**Beneficio**: Reduce superficie de ataque en importación de datos sensibles.

---

## 6. Comparativa de Seguridad

| Aspecto | Antes | Después |
|--------|-------|---------|
| **Contenedores rootless** | Solo API | API, migraciones, importación, analytics, workers ✅ |
| **Limitación CPU** | No | api: 1.0, web: 0.25, worker: 0.5 ✅ |
| **Health checks** | API solo | API, worker, web ✅ |
| **Validación config** | Manual | Script automático ✅ |
| **Secretos sin auditoría** | Implícita | Schema JSON explícito ✅ |
| **OpenAPI en sync** | Manual | Generación automática ✅ |
| **CI/CD** | No | GitHub Actions ✅ |

---

## 7. Cómo Usar las Mejoras

### Validar Configuración
```bash
cd web
python3 scripts/validar_variables.py
```

### Generar Cliente OpenAPI
```bash
cd web
./scripts/generar_openapi.sh
```

### Iniciar en Producción
```bash
cd web/ops
docker compose -f compose.production.yml up --pull always
```

### Ejecutar Tests
```bash
# Backend
cd web/backend
pytest --cov=aplicacion

# Frontend
cd web/frontend
npm run test:coverage
npm run test:e2e
```

---

## 8. Próximos Pasos Recomendados

1. **Implementar Secret Scanning**: GitGuardian, TruffleHog en CI/CD
2. **SBOM Generación**: syft para cada imagen Docker
3. **Kubernetes Ready**: Crear manifests YAML (Deployment, Service, PVC)
4. **Observabilidad**: Prometheus + Grafana para métricas
5. **Auditoría**: Logs centralizados (ELK Stack o Loki)
6. **Backup Automatizado**: Verificación periódica de restauración
7. **Disaster Recovery**: Plan de failover documentado
8. **Load Testing**: k6 o Locust para pruebas de carga

---

## 9. Referencias

- [Docker Security Best Practices](https://docs.docker.com/engine/security/)
- [Docker Compose Security](https://docs.docker.com/compose/security/)
- [JSON Schema Validation](https://json-schema.org/)
- [OWASP Container Security](https://owasp.org/www-community/docker)
- [PostgreSQL Security](https://www.postgresql.org/docs/current/sql-syntax.html)

---

**Fecha de implementación**: 2025
**Versión del documento**: 1.0
**Responsable**: DevOps / Arquitectura
