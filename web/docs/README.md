# Documentación del portal web

## Índice

### Planificación y control

- [Plan de migración total a la plataforma web](PLAN_MIGRACION_TOTAL_WEB.md) — control oficial de la estandarización, modularización, migración única y retiro de WinForms.
- [Plan de cierre de la plataforma web](PLAN_CIERRE_PLATAFORMA_WEB.md) — alcance web-only, inventario de dominios, criterios de 100 % y puertas de calidad; la migración WinForms queda pospuesta.
- [Plan de implementación del menú administrativo](PLAN_IMPLEMENTACION_MENU_ADMIN.md) — fases, responsables, criterios de aceptación, riesgos y registro de evidencias.
- [Matriz de migración del menú administrativo](MATRIZ_MIGRACION_MENU_ADMIN.md) — inventario escritorio→web, rutas, grupos, estados, permisos y vacíos de la Fase 0.

### Arquitectura y convenciones

- [Arquitectura de la plataforma web](ARQUITECTURA.md) — monolito modular, dominios, límites, contratos y estrategia de corte.
- [Contratos de la API](CONTRATOS_API.md) — generación OpenAPI y contratos TypeScript separados por dominio.
- [Convenciones de nombres](CONVENCIONES_NOMBRES.md) — español ASCII en identificadores, español ortográfico en UI/documentación y excepciones técnicas.
- [ADR-0001: monolito modular por dominios](decisiones/0001-monolito-modular-por-dominios.md) — decisión, alternativas y consecuencias.
- [ADR-0002: PostgreSQL y corte único](decisiones/0002-postgresql-y-corte-unico.md) — motor definitivo, separación de roles y alcance del corte.

### Requisitos y operación

- [Requisitos del portal de comedor](REQUISITOS_COMEDOR.md)
- [Manual del estudiante](MANUAL_ESTUDIANTE_COMEDOR.md)
- [Operación del carnet digital](CARNET_DIGITAL_OPERACION.md)
- [Operación PostgreSQL, respaldo e importación](POSTGRESQL_OPERACION_Y_MIGRACION.md)

### Despliegue e integración

- [Despliegue seguro del portal](DESPLIEGUE_PORTAL.md)
- [Runbook de despliegue a producción](RUNBOOK_DEPLOY_PRODUCCION.md) — secuencia aprobada de migración total, despliegue, verificación y retiro controlado del legado.
- [Análisis de integración a producción](ANALISIS_INTEGRACION_PRODUCCION.md)

## Regla de actualización

Todo cambio de alcance, fase, decisión, riesgo o evidencia debe actualizar el plan correspondiente junto con el código. Las fases se marcan como completadas únicamente con pruebas o evidencia verificable. Una decisión arquitectónica nueva o sustituida requiere un ADR.
