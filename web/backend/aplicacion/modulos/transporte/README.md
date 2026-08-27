# Transporte

Administra el catálogo canónico de rutas de transporte web.

## Contratos

- `GET /api/v1/transporte/rutas`
- `GET /api/v1/transporte/rutas/paleta`
- `POST /api/v1/transporte/rutas`
- `PUT /api/v1/transporte/rutas/{id_ruta}`

Los cuerpos y respuestas usan camelCase en español (`idRuta`, `colorHex`,
`estudiantesAsignados`). Todas las operaciones administrativas requieren
`rutas.administrar` y protección CSRF en escrituras.

## Dependencias

El módulo depende únicamente del puerto `RepositorioRutas`, la fábrica
`aplicacion.nucleo.base_datos.FabricaConexionSql` y su paleta de colores. Su persistencia usa exclusivamente
el esquema SQL `transporte`; no consulta el sistema local ni repositorios de
otros dominios. Aplicar `web/sql/migrations/007_transporte_autonomo.sql` antes
de componerlo en la aplicación.
