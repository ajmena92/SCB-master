# Composición de la plataforma web

`entrada.py` es el punto de entrada canónico. `crear_aplicacion` compone las
rutas versionadas de salud, identidad y transporte mediante fábricas explícitas.

- En producción, `DependenciasAplicacion` se construye desde `Settings` y usa
  `FabricaConexionSql`; no existen dobles ni datos de desarrollo implícitos.
- En pruebas, se puede inyectar una fábrica compatible para inspeccionar rutas
  o ejecutar casos aislados sin conexión SQL.
- La autorización y CSRF se resuelven como dependencias FastAPI en la entrada,
  mientras cada módulo conserva sus casos de uso y repositorios.
- Este paquete no importa componentes históricos ni módulos de persistencia globales ni
  `database.py`. Las rutas históricas no se registran en esta aplicación.

La instancia ASGI de despliegue debe invocar `crear_aplicacion` después de cargar
las variables de entorno obligatorias (`SQL_CONNECTION_STRING` y `CORS_ORIGIN`).
