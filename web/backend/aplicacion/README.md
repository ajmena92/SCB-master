# Composición de la plataforma web

`entrada.py` es el punto de entrada canónico. `crear_aplicacion` compone las
rutas versionadas mediante fábricas explícitas y servicios por dominio.

- En producción, `Settings` exige `DATABASE_URL` con PostgreSQL y construye un
  motor SQLAlchemy; ningún endpoint abre conexiones a SQL Server.
- En pruebas se inyecta un motor PostgreSQL aislado para verificar rutas, reglas,
  persistencia y permisos sin dobles de producción.
- La autorización y las sesiones se resuelven en `dependencias_v1.py`; la API
  consulta los permisos vigentes en PostgreSQL en cada solicitud protegida.
- Este paquete no importa componentes históricos ni módulos de persistencia globales ni
  `database.py`. Las rutas históricas no se registran en esta aplicación.

La instancia ASGI de despliegue debe invocar `crear_aplicacion` después de cargar
las variables de entorno obligatorias (`DATABASE_URL` y `CORS_ORIGIN`).
