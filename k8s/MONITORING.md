# Guía de Monitoreo y Observabilidad - SCB Portal Web

> **Estado operativo (2026-09-08): no desplegable.** Este documento y
> `observability-stack.yaml` describen una propuesta histórica, no una
> configuración aprobada. El manifiesto contiene imágenes sin digest,
> almacenamiento `emptyDir`, una contraseña de Grafana fija, permisos de nodo
> para Promtail y un servicio Grafana público. No ejecutar los comandos de
> despliegue que aparecen más abajo. La puerta y el alcance de la observabilidad
> actual están en `web/docs/OBSERVABILIDAD_MINIMA.md`.

## Índice
1. [Visión General](#visión-general)
2. [Stack de Observabilidad](#stack-de-observabilidad)
3. [Métricas Clave](#métricas-clave)
4. [Configuración de Alertas](#configuración-de-alertas)
5. [Dashboards](#dashboards)
6. [Troubleshooting](#troubleshooting)

---

## Visión General

La observabilidad en SCB Portal Web se compone de tres pilares:

- **Metrics** (Prometheus): Rendimiento, disponibilidad, uso de recursos
- **Logs** (Loki): Rastreo de eventos, errores, auditoría
- **Traces** (Jaeger): Tracing distribuido de requests (futuro)

```
┌─────────────────────────────────────────────────────┐
│  Kubernetes Cluster (SCB Portal Web)                │
├─────────────────────────────────────────────────────┤
│  API Pods                                           │
│  └─> Emit metrics (Prometheus format)              │
│  └─> Write logs (stdout/stderr)                    │
│                                                     │
│  Frontend Pods                                      │
│  └─> Emit logs (Nginx)                             │
│                                                     │
│  PostgreSQL                                         │
│  └─> Emit metrics (exporter)                       │
└─────────────────────────────────────────────────────┘
           ↓
┌─────────────────────────────────────────────────────┐
│  Observability Namespace                            │
├─────────────────────────────────────────────────────┤
│  Prometheus ←────── scrape configs                  │
│  Loki ←────────────── logs (via Promtail)           │
│  Grafana ←────────── data source (Prom + Loki)      │
│  Promtail ←────────── DaemonSet (colecta logs)      │
└─────────────────────────────────────────────────────┘
           ↓
    Dashboard & Alerting
```

---

## Stack de Observabilidad

### Despliegue en Kubernetes (bloqueado)

No aplicar este stack hasta cumplir todas las condiciones de
`web/docs/OBSERVABILIDAD_MINIMA.md`. La sección siguiente se conserva solo como
referencia técnica de los componentes previstos; no constituye un procedimiento
operativo.

```bash
# No ejecutar hasta sustituir el manifiesto por uno aprobado.
```

### Componentes

| Componente | Puerto | Descripción |
|-----------|--------|-------------|
| **Prometheus** | 9090 | Base de datos de series temporales |
| **Loki** | 3100 | Agregador de logs |
| **Promtail** | DaemonSet | Recolector de logs en cada nodo |
| **Grafana** | 3000 | Visualización y dashboards |

---

## Métricas Clave

### API (FastAPI + Uvicorn)

```python
# Métricas disponibles automáticamente
from prometheus_client import Counter, Histogram, Gauge

request_duration = Histogram(
    'http_request_duration_seconds',
    'HTTP request latency (seconds)',
    ['method', 'endpoint'],
    buckets=(0.1, 0.5, 1.0, 2.0, 5.0)
)

requests_total = Counter(
    'http_requests_total',
    'Total HTTP requests',
    ['method', 'endpoint', 'status']
)

connections_active = Gauge(
    'db_connections_active',
    'Active database connections'
)
```

### Queries Prometheus

```promql
# CPU API
rate(container_cpu_usage_seconds_total{pod=~"api-.*"}[5m])

# Memoria API
container_memory_usage_bytes{pod=~"api-.*"} / 1024 / 1024

# Latencia de requests
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Tasa de errores (5xx)
rate(http_requests_total{status=~"5.."}[5m])

# Disponibilidad API
(1 - (rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m]))) * 100

# Conexiones a BD
db_connections_active

# Query latencia (p99)
histogram_quantile(0.99, rate(db_query_duration_seconds_bucket[5m]))
```

### Frontend (Nginx)

```nginx
# Logs para Loki
log_format json escape=json
'{
  "time_local": "$time_local",
  "remote_addr": "$remote_addr",
  "request": "$request",
  "status": "$status",
  "bytes_sent": "$bytes_sent",
  "request_time": "$request_time",
  "upstream_response_time": "$upstream_response_time"
}';

access_log /dev/stdout json;
error_log /dev/stderr warn;
```

### PostgreSQL

Monitorear mediante `postgres_exporter`:

```bash
# Desplegar exporter
kubectl -n scb-portal create deployment postgres-exporter \
  --image prometheuscommunity/postgres-exporter \
  -e DATA_SOURCE_NAME="postgresql://scb_api:password@postgresql:5432/scb"
```

---

## Configuración de Alertas

### 1. AlertManager Rules

Crear `k8s/alerting-rules.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-alert-rules
  namespace: observability
data:
  alert-rules.yaml: |
    groups:
    - name: api
      interval: 30s
      rules:
      
      # API no disponible
      - alert: APIDown
        expr: up{job="api"} == 0
        for: 5m
        annotations:
          summary: "API está caída"
      
      # Tasa de errores alta
      - alert: HighErrorRate
        expr: |
          rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 10m
        annotations:
          summary: "Tasa de errores > 5%"
      
      # Latencia alta
      - alert: HighLatency
        expr: |
          histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 2
        for: 10m
        annotations:
          summary: "P95 latency > 2s"
      
      # CPU alta
      - alert: HighCPU
        expr: |
          rate(container_cpu_usage_seconds_total{pod=~"api-.*"}[5m]) > 0.8
        for: 5m
        annotations:
          summary: "CPU API > 80%"
      
      # Memoria alta
      - alert: HighMemory
        expr: |
          (container_memory_usage_bytes{pod=~"api-.*"} / 256000000) > 0.9
        for: 5m
        annotations:
          summary: "Memory API > 90% of limit"
      
      # Pod restarteándose
      - alert: PodRestartingTooOften
        expr: |
          rate(kube_pod_container_status_restarts_total{pod=~"api-.*"}[15m]) > 0.1
        for: 5m
        annotations:
          summary: "Pod restarting frequently"
      
      # Base de datos no disponible
      - alert: DatabaseDown
        expr: up{job="postgres"} == 0
        for: 5m
        annotations:
          summary: "PostgreSQL no disponible"
      
      # Conexiones a BD casi al máximo
      - alert: HighDatabaseConnections
        expr: |
          pg_stat_activity_count / 100 > 0.8
        for: 10m
        annotations:
          summary: "80% of max DB connections in use"
```

### 2. Notificaciones

Configurar AlertManager (`k8s/alertmanager-config.yaml`):

```yaml
global:
  resolve_timeout: 5m
  slack_api_url: 'https://hooks.slack.com/services/YOUR/WEBHOOK/URL'

route:
  receiver: 'team-slack'
  group_by: ['alertname', 'cluster']
  group_wait: 30s
  group_interval: 5m
  repeat_interval: 12h
  routes:
  - match:
      severity: critical
    receiver: 'oncall'
    continue: true

receivers:
- name: 'team-slack'
  slack_configs:
  - channel: '#scb-alerts'
    title: 'SCB Alert'
    
- name: 'oncall'
  slack_configs:
  - channel: '#scb-oncall'
    title: 'CRITICAL: {{ .GroupLabels.alertname }}'
```

---

## Dashboards

### Dashboard Principal (Grafana)

```json
{
  "dashboard": {
    "title": "SCB Portal Web - Production Overview",
    "panels": [
      {
        "title": "API Availability",
        "targets": [
          {
            "expr": "(1 - rate(http_requests_total{status=~\"5..\"}[5m]) / rate(http_requests_total[5m])) * 100"
          }
        ]
      },
      {
        "title": "Request Latency (p95)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))"
          }
        ]
      },
      {
        "title": "API Pod Restarts",
        "targets": [
          {
            "expr": "rate(kube_pod_container_status_restarts_total{pod=~\"api-.*\"}[15m])"
          }
        ]
      },
      {
        "title": "Database Connections",
        "targets": [
          {
            "expr": "pg_stat_activity_count"
          }
        ]
      }
    ]
  }
}
```

### Crear Dashboard en Grafana UI

1. **Ir a**: http://localhost:3000/d/new
2. **Agregar panel** con queries:
   ```promql
   up{job="api"}  # API health
   rate(http_requests_total[5m])  # Request rate
   ```

---

## Troubleshooting

### Prometheus no scrapeea métricas

```bash
# 1. Verificar conectividad
kubectl -n observability exec -it prometheus-0 -- \
  curl http://api.scb-portal:8000/metrics

# 2. Ver logs de Prometheus
kubectl -n observability logs -f deployment/prometheus

# 3. Validar config
kubectl -n observability exec -it prometheus-0 -- \
  promtool check config /etc/prometheus/prometheus.yml
```

### Loki no recibe logs

```bash
# 1. Verificar Promtail
kubectl -n observability logs -f ds/promtail

# 2. Verificar conectividad a Loki
kubectl -n observability exec -it promtail-xxxxx -- \
  curl http://loki:3100/api/prom/ready

# 3. Ver logs en Grafana
# Ir a: Explore > Logs > {namespace="scb-portal"}
```

### Alertas no se disparan

```bash
# 1. Verificar AlertManager
kubectl -n observability logs -f deployment/alertmanager

# 2. Test webhook
curl -X POST http://alertmanager:9093/api/v1/alerts \
  -H "Content-Type: application/json" \
  -d '[{"labels":{"alertname":"TestAlert"}}]'

# 3. Ver reglas en Prometheus
http://prometheus:9090/alerts
```

---

## Comandos Útiles

```bash
# Port-forward a Prometheus
kubectl -n observability port-forward svc/prometheus 9090:9090

# Port-forward a Loki
kubectl -n observability port-forward svc/loki 3100:3100

# Port-forward a Grafana
kubectl -n observability port-forward svc/grafana 3000:3000

# Ver eventos de cluster
kubectl -n scb-portal get events --sort-by='.lastTimestamp'

# Monitorear recursos de pods
kubectl -n scb-portal top pods

# Stream logs de API
kubectl -n scb-portal logs -f deployment/api --all-containers=true

# Debugging de conectividad
kubectl -n observability run -it debug --image=busybox -- sh
# Adentro: wget -O- http://prometheus:9090/api/v1/query?query=up
```

---

## Referencias

- [Prometheus Documentation](https://prometheus.io/docs/)
- [Loki Documentation](https://grafana.com/docs/loki/)
- [Grafana Dashboards](https://grafana.com/grafana/dashboards/)
- [Kubernetes Monitoring](https://kubernetes.io/docs/tasks/debug-application-cluster/resource-metrics-pipeline/)

---

**Última actualización**: 2025
**Responsable**: DevOps / SRE Team
