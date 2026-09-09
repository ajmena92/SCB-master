# Runbook de Incident Response - SCB Portal Web

## Índice
1. [Escalación](#escalación)
2. [Incident Severities](#incident-severities)
3. [Playbooks por Tipo de Incidente](#playbooks-por-tipo-de-incidente)
4. [Checklist Post-Incident](#checklist-post-incident)
5. [Contactos Clave](#contactos-clave)

---

## Escalación

### Niveles de Escalación

```
Level 1 (On-Call)
    ↓
Level 2 (Backend/DevOps Lead)
    ↓
Level 3 (Engineering Manager)
    ↓
Level 4 (Director/CTO)
```

### Tiempos de Respuesta SLA

| Severidad | Tiempo Respuesta | Tiempo Resolución | Escalación |
|-----------|------------------|------------------|------------|
| P1 (Critical) | 5 min | 30 min | Inmediata |
| P2 (High) | 15 min | 2 horas | 30 min sin progreso |
| P3 (Medium) | 1 hora | 8 horas | Si se requiere |
| P4 (Low) | 4 horas | 48 horas | No automática |

---

## Incident Severities

### P1 - CRITICAL
- **Descripción**: Servicio completamente caído, datos comprometidos
- **Ejemplos**:
  - API no responde (< 1 min)
  - Base de datos inaccesible
  - Fuga de datos
  - Login/Auth no funciona

### P2 - HIGH  
- **Descripción**: Funcionalidad degradada, afecta muchos usuarios
- **Ejemplos**:
  - API responde lentamente (> 5s)
  - 50%+ de requests fallan
  - Memory leak en API
  - Certificado SSL expirando < 7 días

### P3 - MEDIUM
- **Descripción**: Funcionalidad parcialmente afectada
- **Ejemplos**:
  - Algunos endpoints lentos
  - Logs no se agregan correctamente
  - Pequeño número de usuarios afectados

### P4 - LOW
- **Descripción**: Inconveniences menores
- **Ejemplos**:
  - Typos en mensajes
  - Documentación desactualizada
  - Métrica de monitoreo faltando

---

## Playbooks por Tipo de Incidente

### 1. API No Responde (P1)

#### Fase 1: Confirmación
```bash
# 1. Verificar estado del pod
kubectl -n scb-portal get pods -l app=api

# 2. Verificar logs
kubectl -n scb-portal logs deployment/api --all-containers=true --tail=100

# 3. Verificar health
kubectl -n scb-portal port-forward svc/api 8000:8000
curl http://localhost:8000/health
```

#### Fase 2: Diagnostico
```bash
# Si los pods están Down:
# a) Ver events
kubectl -n scb-portal describe pod <pod-name>

# b) Ver si está OOMKilled
kubectl -n scb-portal top pods

# c) Ver logs de inicialización
kubectl -n scb-portal logs <pod-name> --previous

# Si los pods están Up pero no responden:
# a) Verificar conectividad a BD
kubectl -n scb-portal exec -it <api-pod> -- \
  python -c "import psycopg; c=psycopg.connect('postgresql://...'); print('OK')"

# b) Verificar recursos
kubectl -n scb-portal top pod <pod-name>
```

#### Fase 3: Mitigation Inmediata
```bash
# Opción 1: Restart pods
kubectl -n scb-portal rollout restart deployment/api

# Opción 2: Rollback a versión anterior
kubectl -n scb-portal rollout history deployment/api
kubectl -n scb-portal rollout undo deployment/api

# Opción 3: Escalar a 0 y luego a 3
kubectl -n scb-portal scale deployment/api --replicas=0
kubectl -n scb-portal scale deployment/api --replicas=3
```

#### Fase 4: Comunicación
- [ ] Notificar a Slack #scb-incidents
- [ ] Actualizar status en status-page (si existe)
- [ ] Enviar mensaje a usuarios afectados

#### Fase 5: Post-Incident
- [ ] Recopilar logs y metricas
- [ ] Identificar causa raíz
- [ ] Crear ticket para prevención

---

### 2. Alta Latencia (P2)

#### Fase 1: Confirmación
```bash
# Verificar métricas en Grafana
# Query: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# O via Prometheus:
kubectl -n observability port-forward svc/prometheus 9090:9090
# Ir a: http://localhost:9090/graph
```

#### Fase 2: Diagnostico
```bash
# a) Verificar CPU/Memory
kubectl -n scb-portal top pods

# b) Verificar conexiones a BD
kubectl -n scb-portal exec -it <api-pod> -- \
  psql postgresql://... -c "SELECT count(*) FROM pg_stat_activity;"

# c) Ver queries lentas
kubectl -n scb-portal exec -it postgresql-0 -- \
  psql -U scb_api -d scb -c "SELECT query, mean_time FROM pg_stat_statements ORDER BY mean_time DESC LIMIT 5;"

# d) Ver si hay lock en BD
kubectl -n scb-portal exec -it postgresql-0 -- \
  psql -U scb_api -d scb -c "SELECT * FROM pg_locks WHERE NOT granted;"
```

#### Fase 3: Mitigation
```bash
# Si es CPU alta:
# Escalar horizontalmente
kubectl -n scb-portal scale deployment/api --replicas=5

# Si es Memory leak:
# Restart pods
kubectl -n scb-portal rollout restart deployment/api

# Si es BD slow:
# a) Matar queries lentas
kubectl -n scb-portal exec -it postgresql-0 -- \
  psql -U scb_api -d scb -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE query ILIKE '%...%' AND pid <> pg_backend_pid();"

# b) ANALYZE table
kubectl -n scb-portal exec -it postgresql-0 -- \
  psql -U scb_api -d scb -c "ANALYZE table_name;"
```

---

### 3. Base de Datos No Disponible (P1)

#### Fase 1: Confirmación
```bash
# Verificar estado del pod
kubectl -n scb-portal get pods -l app=postgresql

# Verificar logs
kubectl -n scb-portal logs statefulset/postgresql

# Test de conectividad
kubectl -n scb-portal exec -it postgresql-0 -- \
  pg_isready -U scb_admin -d scb
```

#### Fase 2: Causas Comunes
```bash
# 1. Disco lleno
kubectl -n scb-portal exec -it postgresql-0 -- df -h /var/lib/postgresql/data

# 2. Out of Memory
kubectl -n scb-portal exec -it postgresql-0 -- free -h

# 3. Too many connections
kubectl -n scb-portal exec -it postgresql-0 -- \
  psql -U scb_admin -d scb -c "SHOW max_connections;"

# 4. Archivos corruptos
kubectl -n scb-portal logs statefulset/postgresql | grep -i corrupt
```

#### Fase 3: Recovery
```bash
# Backup inmediato antes de hacer cambios
kubectl -n scb-portal exec -it postgresql-0 -- \
  pg_dump -U scb_admin -d scb | gzip > /backup/emergency-$(date +%s).sql.gz

# Si está corrupta:
# a) Reiniciar
kubectl -n scb-portal delete pod postgresql-0
kubectl wait --for=condition=ready pod -l app=postgresql -n scb-portal

# b) Si persiste, restaurar desde backup
kubectl -n scb-portal cp /backup/latest.sql.gz postgresql-0:/tmp/
kubectl -n scb-portal exec -it postgresql-0 -- \
  zcat /tmp/latest.sql.gz | psql -U scb_admin
```

---

### 4. Memory Leak / High Memory (P2)

#### Fase 1: Identificación
```bash
# Ver memoria actual
kubectl -n scb-portal top pods

# Ver histórico
kubectl -n observability port-forward svc/prometheus 9090:9090
# Query: container_memory_usage_bytes{pod=~"api-.*"}
```

#### Fase 2: Diagnostico
```bash
# Si un pod específico tiene leak:
# a) Ver edad del pod
kubectl -n scb-portal get pods -o wide

# b) Revisar logs buscando patterns
kubectl -n scb-portal logs <pod-name> | grep -i "allocated\|memory\|warning"

# c) Profiling (si está disponible)
kubectl -n scb-portal exec -it <pod-name> -- \
  curl http://localhost:8000/debug/pprof/heap > heap.dump
```

#### Fase 3: Mitigation
```bash
# Temporary: Restart pod
kubectl -n scb-portal delete pod <pod-name>

# Permanent: Deploy fix
git add src/fix.py
git commit -m "fix: memory leak in connection handler"
git push origin fix/memory-leak

# Después de review y merge a main, nuevo deploy:
kubectl -n scb-portal rollout restart deployment/api
```

---

### 5. High Error Rate / 5xx Errors (P1-P2)

#### Fase 1: Confirmación
```bash
# Ver tasa de errores
kubectl -n observability port-forward svc/prometheus 9090:9090
# Query: rate(http_requests_total{status=~"5.."}[5m])

# Ver errores en logs
kubectl -n scb-portal logs deployment/api --all-containers=true | grep ERROR | tail -20
```

#### Fase 2: Categorización
```bash
# 500 - Server Error (API error)
# 502 - Bad Gateway (proxy/load balancer issue)
# 503 - Service Unavailable (graceful shutdown)
# 504 - Gateway Timeout (slow backend)

# Analizar por endpoint:
# Query: rate(http_requests_total{status="500"}[5m]) by (endpoint)

# Ver si es específico de un endpoint:
kubectl -n scb-portal logs deployment/api | grep "POST /api/v1/endpoint" | grep 500 | head -5
```

#### Fase 3: Fix
```bash
# Si es código:
git log --oneline -5  # Ver commits recientes
git show <commit>     # Revisar qué cambió
git revert <commit>   # Revertir si es necesario

# Después:
git push origin hotfix/500-error
# Crear PR y mergear rápido
```

---

## Checklist Post-Incident

### Inmediato (< 1 hora)
- [ ] Incidente resuelto y confirmado
- [ ] Comunicación a stakeholders
- [ ] Documentación inicial del problema
- [ ] Screenshots/logs capturados

### Corto Plazo (< 24 horas)
- [ ] Causa raíz identificada
- [ ] Post-mortem preliminar creado
- [ ] Mitigación temporal documentada
- [ ] Follow-up permanente asignado

### Medio Plazo (< 1 semana)
- [ ] Fix permanente deployado
- [ ] Post-mortem completado
- [ ] Lecciones documentadas
- [ ] Nuevas alertas/checks creadas (si aplica)

### Largo Plazo (< 1 mes)
- [ ] Action items implementados
- [ ] Documentación actualizada
- [ ] Team training si fue necesario
- [ ] Costo del incidente analizado

---

## Contactos Clave

### On-Call Rotation
```
Lunes-Viernes:
  08:00-18:00 - Backend Lead
  18:00-08:00 - On-Call Engineer

Fines de Semana/Holidays:
  All day - Senior On-Call
```

### Escalation Contacts

| Rol | Nombre | Email | Slack | Teléfono |
|-----|--------|-------|-------|----------|
| Backend Lead | [Nombre] | [email] | @backend-lead | [+506 xxxx xxxx] |
| DevOps Lead | [Nombre] | [email] | @devops-lead | [+506 xxxx xxxx] |
| Eng Manager | [Nombre] | [email] | @eng-manager | [+506 xxxx xxxx] |
| CTO | [Nombre] | [email] | @cto | [+506 xxxx xxxx] |

### Canales de Comunicación

- **Urgente (P1)**: Llamada directa + Slack
- **Alto (P2)**: Slack + Email
- **Medio (P3)**: Ticket en Jira + Slack
- **Bajo (P4)**: Ticket en Jira

### Herramientas de Access

| Herramienta | URL | Acceso |
|-------------|-----|--------|
| Grafana | http://k8s-monitoring:3000 | VPN + 2FA |
| Prometheus | http://k8s-monitoring:9090 | VPN |
| Kubernetes | k8s-prod.example.com | kubeconfig + 2FA |
| PostgreSQL | postgresql.scb-portal.svc | SSH tunnel |

---

## Drill Agenda

Realizar drills mensuales:

- [ ] **Primer viernes**: API outage simulation
- [ ] **Segunda semana**: Database failover drill
- [ ] **Tercera semana**: Security incident response
- [ ] **Cuarta semana**: Communication/notification drill

---

## Ejemplos de Tickets

### Ticket Template para Post-Mortem
```markdown
# Post-Mortem: [Incident Name]

## Summary
[Resumen de 1-2 oraciones]

## Timeline
- 14:23 - Alert fired for high error rate
- 14:25 - On-call engineer paged
- 14:30 - Cause identified: memory leak in handler
- 14:45 - Pods restarted
- 14:50 - Service recovered
- 15:00 - Incident declared over

## Root Cause
[Explicación detallada]

## Impact
- Duration: 27 minutes
- Affected Users: ~5%
- Error Rate Peak: 15%
- Data Loss: None

## Resolution
[Qué se hizo para arreglarlo]

## Action Items
- [ ] Implement memory profiling in production (Owner: Backend Lead, Due: 1 week)
- [ ] Add alert for memory growth rate (Owner: DevOps, Due: 3 days)
- [ ] Add integration test for memory leaks (Owner: QA, Due: 1 week)

## Lessons Learned
1. Memory leak was not caught in staging
2. Alert thresholds were too high
3. Documentation for troubleshooting was outdated
```

---

**Última actualización**: 2025
**Responsable**: DevOps / On-Call Team
**Frecuencia de Review**: Trimestral
