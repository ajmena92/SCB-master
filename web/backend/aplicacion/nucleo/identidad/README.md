# Núcleo de identidad

Este paquete contiene la identidad canónica de la plataforma web: hashes Argon2id,
sesiones opacas y evaluación de permisos en español.

## Contratos y límites

- `seguridad.py` solo contiene primitivas criptográficas y no acepta hashes del sistema local.
- `repositorio.py` define puertos tipados e implementaciones SQL concretas para `identidad.*`.
- `servicio.py` contiene autenticación, emisión/validación/revocación de sesiones y permisos.
- `api.py` expone las rutas canónicas `/identidad/autenticacion`, `/identidad/sesion` y
  `/identidad/sesion/cerrar`; la composición superior decide cuándo incluirlas.
- `aplicacion.entrada.crear_aplicacion` compone identidad, salud y transporte con fábricas
  canónicas inyectables; el camino predeterminado obtiene SQL desde `Settings`.
- `esquemas.py` usa contratos camelCase en español (`idUsuario`, `secretoSesion`, `expiraEn`).
- No se importan componentes históricos ni módulos de persistencia globales.
- El secreto de sesión se entrega una sola vez; solo su digest se persiste.
- Las cookies de sesión son `HttpOnly`, `Secure` y `SameSite=Strict`; el token CSRF es
  independiente, se entrega también en cookie legible y se exige en `X-CSRF-Token`.

Las implementaciones `RepositorioSqlUsuarios` y `RepositorioSqlSesiones` reciben una
`FabricaConexionSql`, ejecutan únicamente SQL parametrizado sobre el esquema canónico y no
importan módulos del sistema local. La estructura se crea con `008_identidad_canonica.sql`;
su rollback destructivo está documentado en `008_identidad_canonica_revertir.sql` y requiere
autorización del DBA.

Los permisos son una colección extensible. El primer permiso canónico utilizado por la
migración es `rutas.administrar`; la API deberá aplicar la comprobación en cada endpoint.
