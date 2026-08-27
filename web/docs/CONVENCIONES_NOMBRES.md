# Convenciones de nombres

## Regla principal

El español es el idioma predeterminado de SCB. Todo nombre controlado por el proyecto se escribe en español cuando la plataforma técnica lo permite.

- En código, rutas, contratos, permisos y base de datos se usa español ASCII: sin tildes ni `ñ`.
- En la interfaz, mensajes, manuales y documentación narrativa se usa español correcto, con tildes y `ñ`.
- El inglés solo se acepta por obligación del lenguaje, biblioteca, protocolo, formato o herramienta.

Ejemplos: `autenticacion`, `contrasena`, `anio`, `numeroIdentificacion`, mientras la interfaz muestra “Autenticación”, “Contraseña” y “Año”.

## Convenciones por superficie

| Superficie | Convención | Ejemplo |
| --- | --- | --- |
| Carpetas y archivos propios | `snake_case` | `repositorio_estudiantes.py` |
| Módulos, variables y funciones Python | `snake_case` | `crear_estudiante` |
| Variables y funciones TypeScript | `camelCase` | `obtenerPlantillasMenu` |
| Componentes, clases y tipos | `PascalCase` | `FormularioEstudiante` |
| Hooks React | `use` + `PascalCase` español | `usePlantillasMenu` |
| Constantes | `MAYUSCULAS_CON_GUION_BAJO` | `MAXIMO_INTENTOS_PIN` |
| Tablas y columnas nuevas | `snake_case` español | `fecha_registro` |
| Rutas API | minúsculas, sustantivos españoles | `/api/v1/estudiantes` |
| Campos JSON | `camelCase` español | `nombreCompleto` |
| Permisos | `dominio.accion` | `estudiantes.editar` |
| Pruebas | comportamiento esperado en español | `rechaza_pin_incorrecto` |
| Documentación y UI | español ortográfico | `Cambiar contraseña` |

No se usan tildes, `ñ`, espacios ni abreviaturas ambiguas en identificadores técnicos. Los acrónimos institucionales aprobados pueden conservarse: `PIAD`, `SCB`, `CTP`.

## Vocabulario canónico

| Usar | Evitar en nombres propios |
| --- | --- |
| `aplicacion` | `app` |
| `funcionalidades` | `features` |
| `compartido` | `shared`, `common` |
| `nucleo` | `core` |
| `modulos` | `modules` |
| `identidad`, `autenticacion` | `identity`, `auth` |
| `estudiantes` | `students` |
| `comedor`, `menu` | `meals`, `menuTemplates` |
| `asistencia`, `marcas` | `attendance`, `marks` |
| `transporte`, `rutas` | `transport`, `routes` |
| `beneficios` | `benefits` |
| `cuentas`, `saldos`, `recargas` | `accounts`, `balances`, `topups` |
| `importaciones` | `imports` |
| `reportes` | `reports` |
| `auditoria` | `audit` |
| `repositorio`, `servicio`, `esquema` | `repository`, `service`, `schema` |

El vocabulario se aplica a código nuevo y a archivos modificados durante la migración. No se realizan renombrados masivos sin pruebas ni se crean aliases para ocultar nombres anteriores.

## Excepciones técnicas permitidas

Se permiten sin justificación adicional:

- archivos y directorios exigidos por herramientas: `package.json`, `Dockerfile`, `node_modules`, `dist`, `build`, `main.tsx`;
- palabras reservadas y firmas impuestas por Python, TypeScript, React, FastAPI, SQLAlchemy o Alembic;
- prefijo `use` requerido por hooks React;
- métodos y conceptos de protocolos: `GET`, `POST`, `HTTP`, `CSRF`, `JSON`, `OpenAPI`;
- formatos y tecnologías con nombre propio: `PDF`, `CSV`, `Excel`, `QR`, `SQL Server`;
- símbolos importados de dependencias externas;
- nombres de variables de entorno exigidos por una herramienta o proveedor.

Una excepción propia del proyecto se registra en la tabla siguiente. No basta la preferencia personal ni la costumbre del equipo.

| Término | Superficie | Motivo técnico | Fecha de revisión |
| --- | --- | --- | --- |
| Ninguno registrado | — | — | — |

Los controles automatizados, su alcance y las excepciones transitorias de longitud se mantienen en [EXCEPCIONES_VERIFICADORES.md](EXCEPCIONES_VERIFICADORES.md). La lista leída por el comando está versionada junto al verificador para que CI y la documentación usen el mismo criterio.

## API, contratos y permisos

Rutas canónicas:

```text
/api/v1/autenticacion
/api/v1/estudiantes
/api/v1/menu
/api/v1/asistencia
/api/v1/transporte
/api/v1/reportes
/api/v1/administracion
```

Contrato de ejemplo:

```json
{
  "idUsuario": 10,
  "nombreCompleto": "Nombre del usuario",
  "debeCambiarPin": true
}
```

Permisos de ejemplo:

```text
menu.leer
menu.editar
estudiantes.leer
estudiantes.editar
rutas.administrar
reportes.transporte.exportar
auditoria.leer
```

Los nombres anteriores se retiran al completar el corte correspondiente. No se publican dos nombres canónicos para la misma operación.

## Revisión

En cada PR se comprueba:

1. que los identificadores propios nuevos estén en español ASCII;
2. que la interfaz y documentación usen español ortográfico;
3. que cualquier inglés propio esté en la lista de excepciones;
4. que no se hayan agregado aliases heredados;
5. que rutas, contratos y permisos coincidan con el vocabulario del dominio.
