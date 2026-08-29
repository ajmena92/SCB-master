# Contratos de la API

El backend FastAPI es la fuente de verdad del contrato HTTP. El cliente TypeScript se
genera desde OpenAPI con:

```bash
cd web/frontend
npm run generar:cliente
```

La comprobación de sincronización no modifica archivos:

```bash
cd web/frontend
npm run verificar:cliente
```

## Salidas generadas

Los esquemas se separan por dominio en `frontend/src/compartido/contratos/`:

- `identidad.ts`, `estudiantes.ts`, `transporte.ts`, `asistencia.ts` y `beneficios.ts`;
- `cuentas.ts`, `reportes.ts`, `importaciones.ts`, `auditoria.ts` y `administracion.ts`;
- `menu.ts`, `comedor.ts`, `soporte.ts`, `parametros.ts`, `salud.ts` y `comunes.ts`.

`operaciones/` contiene el inventario generado por dominio. `operaciones.ts` define el tipo
común y compone `OPERACIONES_API`. `api.ts` solo es un barrel generado para exportaciones generales; el código nuevo debe
importar directamente desde el contrato del dominio que consume.

No se editan manualmente las salidas. Si cambia una ruta, un esquema o un alias JSON, se
actualiza el backend, se regenera el cliente y se ejecutan las comprobaciones de frontend.

## Convenciones HTTP

- Las rutas funcionales usan `/api/v1/`.
- Las respuestas y solicitudes usan nombres `camelCase` definidos por los esquemas Pydantic.
- Las mutaciones protegidas requieren sesión válida y CSRF.
- Los errores públicos usan `detail` y estados HTTP apropiados, incluido `429` para bloqueo
  temporal de autenticación.
- El frontend usa cookies de sesión y no almacena credenciales ni tokens de sesión en
  `localStorage`.

## Contrato de comedor

El contrato canónico de comedor define personas, estado (`becado_comedor` o `no_becado_comedor`),
cuentas y movimientos de tiquetes, reservas e ingresos. El estado de comedor es la
única fuente para decidir la autorización de ingreso. La modalidad histórica de `0023`
no constituye por sí sola un endpoint vigente. Las operaciones públicas deben mantenerse separadas
por estado, cuentas, reservas, ingreso por carnet y estadísticas por `tipoPersona`
(`estudiante` o `profesor`).

Las estadísticas estudiantiles deben excluir profesores en el servidor, no solo en la
interfaz. Estos endpoints no se consideran disponibles hasta contar con persistencia
de staging y pruebas HTTP de reserva, consumo atómico y concurrencia.

La referencia operativa de despliegue está en [DESPLIEGUE_PORTAL.md](DESPLIEGUE_PORTAL.md).
