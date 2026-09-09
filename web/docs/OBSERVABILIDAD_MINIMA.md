# Observabilidad mínima y puerta para Kubernetes

## Estado actual

La plataforma se opera con Compose. Kubernetes y el archivo
`k8s/observability-stack.yaml` son un borrador de referencia y **no están
autorizados para despliegue**. No ejecutar `kubectl apply` contra ese archivo
en ningún entorno con datos reales.

La comprobación disponible para disponibilidad del proceso es
`GET /api/v1/salud`, publicado externamente solo mediante `GET /health` del
proxy. La disponibilidad de PostgreSQL se comprueba en el `HEALTHCHECK` de la
imagen de API mediante una consulta interna; no se expone una ruta HTTP de
readiness con detalles de infraestructura. Esto evita filtrar el motor o el
estado de la base de datos y respeta la regla de que los endpoints no ejecutan
SQL.

## Medición antes de añadir infraestructura

El proceso operativo debe conservar y revisar, por cada despliegue:

- estado de los contenedores y resultado de `GET /health`;
- códigos 4xx/5xx, latencia P95 y memoria de API mediante los scripts de
  medición existentes;
- espacio de PostgreSQL, WAL y antigüedad del último respaldo verificado;
- vencimiento del certificado TLS desde una comprobación externa;
- logs de API y Nginx sin PIN, cookies, contraseñas, fotografías ni otros datos
  personales.

Las alertas iniciales se habilitarán únicamente sobre esos cinco indicadores:
API no disponible, errores 5xx, latencia P95, respaldo vencido y disco/WAL.
No se incorporan trazas distribuidas ni un segundo sistema de logs mientras una
sola instancia de Compose cubra la operación.

## Condiciones para habilitar Kubernetes

Kubernetes se considera solo cuando exista una necesidad aprobada de varias
réplicas, alta disponibilidad, escalado o despliegues coordinados que Compose
no cubra. Antes de promover el stack se requiere evidencia de:

1. imágenes con digest inmutable, SBOM y análisis de vulnerabilidades;
2. almacenamiento persistente, cifrado y política de retención para Prometheus,
   Loki y Grafana; nunca `emptyDir` para información que deba sobrevivir un
   reinicio;
3. secreto de Grafana gestionado fuera del manifiesto, sin contraseñas por
   defecto, y acceso privado mediante Ingress autenticado o VPN; nunca un
   `LoadBalancer` público por defecto;
4. RBAC mínimo por namespace y una alternativa al acceso de Promtail a
   `hostPID`, `hostNetwork` y directorios del nodo;
5. `NetworkPolicy`, límites de recursos, respaldo/restauración comprobados y
   alertas entregadas a un canal con responsable definido;
6. métricas de aplicación implementadas con una dependencia fijada, pruebas de
   formato Prometheus y un presupuesto explícito de etiquetas para impedir alta
   cardinalidad. La API actual no declara esa dependencia, por lo que no se
   simulan métricas con una implementación casera.

Al cumplir estas condiciones se debe reemplazar el manifiesto de referencia por
una configuración revisada, versionada y validada en staging. Esta decisión se
registra en un ADR antes de tocar producción.
