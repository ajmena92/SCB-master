# Observabilidad mínima en el servidor Compose

## Estado actual

La plataforma se opera exclusivamente con Docker Compose en un servidor.
Kubernetes no forma parte de la arquitectura ni del despliegue autorizado.

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

La observabilidad futura debe integrarse con este mismo servidor y respetar los
límites de recursos, privacidad y respaldos ya definidos para Compose.
