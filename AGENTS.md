# Guía del repositorio

## Dirección del producto

- `web/` es la plataforma activa y el destino único del producto.
- `escritorio/` contiene el sistema WinForms de origen. Es referencia histórica durante la migración, no una dependencia permitida en ejecución.
- La migración será única y auditable: no se permite doble escritura, sincronización permanente, integración en tiempo de ejecución ni nuevas funciones en WinForms.
- La arquitectura aprobada está en `web/docs/ARQUITECTURA.md` y su decisión formal en `web/docs/decisiones/0001-monolito-modular-por-dominios.md`.

## Estructura y límites

- `web/frontend/`: aplicación React/Vite. La estructura objetivo usa `src/aplicacion`, `src/funcionalidades` y `src/compartido`.
- `web/backend/`: API FastAPI. La estructura objetivo usa `aplicacion/nucleo` y `aplicacion/modulos`.
- `web/sql/`: scripts y migraciones de datos; una migración debe ser repetible, verificable y documentada.
- `web/docs/`: arquitectura, convenciones, planes, manuales y decisiones ADR.
- `web/ops/` y `web/scripts/`: despliegue y operación.
- `escritorio/`, `Utilitarios/` y archivos de importación no se modifican salvo tarea expresa de extracción o validación histórica.

Un módulo no puede acceder al repositorio privado de otro dominio. Los componentes React no llaman HTTP directamente, los endpoints no ejecutan SQL y los repositorios no contienen reglas de negocio.

## Convenciones de nombres

- El idioma predeterminado del sistema es español.
- Código, rutas, contratos, permisos, tablas y columnas nuevos usan términos españoles sin tildes ni `ñ`: `autenticacion`, `contrasena`, `anio`.
- Carpetas, archivos, módulos y funciones Python: `snake_case`.
- Variables y funciones TypeScript: `camelCase`; componentes, clases y tipos: `PascalCase`.
- Constantes: `MAYUSCULAS_CON_GUION_BAJO`; tablas y columnas: `snake_case`.
- La interfaz, mensajes y documentación usan español correcto, con tildes y `ñ`.
- Se conserva inglés solo por obligación de lenguaje, protocolo, biblioteca o herramienta. Toda excepción propia del proyecto debe justificarse en `web/docs/CONVENCIONES_NOMBRES.md`.

## Desarrollo y comprobación

Frontend:

```bash
cd web/frontend
npm test
npm run build
```

Backend:

```bash
cd web/backend
pytest -q
```

Despliegue local de producción:

```bash
cd web/ops
docker compose --env-file .env -f compose.production.yml config
docker compose --env-file .env -f compose.production.yml build
docker compose --env-file .env -f compose.production.yml up -d
```

Antes de declarar una fase completada, ejecutar las comprobaciones proporcionales al cambio y registrar evidencia en el plan de control correspondiente.

## Estilo y calidad

- Frontend nuevo en TypeScript estricto; backend con tipos explícitos y esquemas Pydantic.
- Mantener reglas de dominio dentro de su módulo y contratos públicos explícitos.
- Evitar archivos mayores de 300 líneas; si un archivo los supera, dividirlo o documentar la excepción.
- No introducir dependencias circulares, SQL fuera de repositorios, payloads `dict` sin tipo ni autorización exclusiva en la interfaz.
- No agregar nombres alternativos ni adaptadores permanentes para conservar contratos heredados.
- Nunca registrar ni versionar credenciales, PIN, cookies, cadenas de conexión, fotografías o datos personales.

## Pruebas y cambios

- Cada corte vertical incluye datos, API, interfaz, permisos, estados, pruebas y documentación.
- Añadir pruebas unitarias para reglas, integración para persistencia y autorización, y Playwright para recorridos críticos.
- Los commits usan resúmenes breves en español, en modo imperativo y con un solo cambio lógico.
- Los PR describen propósito, módulos, preparación de datos, evidencia automatizada/manual, capturas para UI y limitaciones conocidas.
- No editar artefactos generados ni archivos `*.Designer.vb` del histórico.

## Seguridad y migración

- La API es siempre la autoridad de permisos; ocultar una opción en el menú no sustituye la validación del endpoint.
- El nuevo modelo de datos web debe quedar separado del esquema operativo de WinForms.
- El corte definitivo exige respaldo, congelamiento de escrituras, reconciliación, invalidación de sesiones y retiro de accesos del sistema local.
- DigitalPersona y Crystal Reports se sustituyen por capacidades web aprobadas; no se integran en tiempo de ejecución.
