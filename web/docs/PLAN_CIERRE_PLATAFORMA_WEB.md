# Plan de cierre de la plataforma web (sin migración WinForms)

## Propósito y límite

Este documento controla el cierre técnico y funcional de la plataforma web con datos,
contratos, permisos y persistencia canónicos. Su objetivo es llevar la aplicación web a
producción sin depender de módulos antiguos ni de una migración de datos desde WinForms.

Queda explícitamente fuera de este plan:

- extracción, modificación o migración de `escritorio/`;
- carga de datos históricos desde WinForms;
- doble escritura, sincronización o convivencia operativa con WinForms;
- retiro físico o archivado del sistema local.

Esas actividades se registrarán como una fase posterior, después de la aceptación formal de
la plataforma web. “100 % web” significa que la funcionalidad nueva, sus datos de operación,
API, interfaz, autorización y pruebas funcionan en la plataforma web; no significa que los
datos históricos ya hayan sido migrados.

## Inventario y estado de los dominios

| Dominio | Backend modular | Frontend modular | Datos y contratos canónicos | Estado de cierre |
|---|---:|---:|---:|---|
| Identidad, sesiones y permisos | Parcial | Pendiente | Parcial | Abierto |
| Estudiantes | Parcial | Parcial | Parcial | Abierto |
| Menú, calendario y sustituciones | Pendiente | Parcial | Pendiente | Abierto |
| Comedor | Pendiente | Parcial | Pendiente | Abierto |
| Asistencia y correcciones | Parcial | Pendiente | Parcial | Abierto |
| Transporte y rutas | Parcial | Parcial | Parcial | Abierto |
| Beneficios | Pendiente | Pendiente | Pendiente | Abierto |
| Cuentas, recargas y saldos | Pendiente | Pendiente | Pendiente | Abierto |
| Importaciones y reversión | Pendiente | Pendiente | Pendiente | Abierto |
| Reportes y exportaciones | Pendiente | Pendiente | Pendiente | Abierto |
| Auditoría y administración | Pendiente | Pendiente | Pendiente | Abierto |
| Ayuda y soporte | Pendiente | Pendiente | Pendiente | Abierto |

La clasificación es de control, no una afirmación de funcionalidad terminada. En particular,
la presencia de módulos iniciales de Identidad, Estudiantes, Asistencia o Transporte no cierra
un dominio hasta cumplir todos los criterios de aceptación de este documento.

## Secuencia web-only

### 1. Línea base y frontera de ejecución

- Confirmar `web/` como única aplicación objetivo y documentar el punto de entrada de producción.
- Inventariar cada ruta, permiso, tabla, pantalla y prueba que aún pertenezca al servidor central.
- Prohibir nuevas dependencias desde módulos web hacia `escritorio/` y conservar las referencias
  históricas únicamente en documentación de alcance.

**Verificación:** guardas arquitectónicas, inventario de rutas OpenAPI y prueba que la entrada
modular arranca sin importar componentes históricos ni repositorios centrales.

### 2. Núcleo transversal canónico

- Completar configuración, base de datos, transacciones y manejo uniforme de errores.
- Conectar autenticación, sesiones opacas, CSRF, Argon2id, restablecimiento obligatorio y RBAC
  a la API activa.
- Incorporar auditoría, salud, observabilidad y trazabilidad de actor, acción y resultado.

**Verificación:** pruebas de autorización por endpoint, expiración y revocación de sesión,
CSRF, hash rechazado, auditoría y health check.

### 3. Persistencia web canónica

- Crear los esquemas independientes de todos los dominios requeridos por la operación web.
- Adoptar SQLAlchemy 2 y Alembic según la arquitectura aprobada, o registrar un ADR si existe
  una limitación técnica justificada.
- Hacer repetibles las migraciones desde base vacía, con reversión ensayada y datos de prueba
  no históricos.

**Verificación:** instalación limpia, migración arriba/abajo, restricciones, índices,
transacciones y pruebas de repositorio contra SQL Server desechable.

### 4. Cortes verticales restantes

Completar en este orden, cerrando un dominio antes de iniciar el siguiente:

1. menú, calendario y sustituciones;
2. estudiantes y sus relaciones operativas;
3. comedor y beneficios;
4. asistencia y correcciones;
5. transporte y rutas;
6. cuentas, recargas y saldos;
7. reportes y exportaciones;
8. importaciones y reversión;
9. administración, roles, permisos y auditoría;
10. ayuda y soporte.

Cada corte debe incluir `api.py`, esquemas tipados, servicio, repositorio, modelos, errores,
pruebas, README del módulo, página o componentes React, consultas, estados, permisos,
estados vacíos/error/carga y documentación del contrato.

**Verificación:** pruebas unitarias y de integración del módulo, contrato OpenAPI, cliente
TypeScript generado, recorrido UI y autorización positiva/negativa.

### 5. Frontend y contratos

- Reubicar las pantallas restantes bajo `src/aplicacion`, `src/funcionalidades` y `src/compartido`.
- Completar TypeScript estricto sin HTTP directo en componentes.
- Generar contratos TypeScript desde OpenAPI y eliminar payloads sin tipo.
- Cubrir teclado, lector de pantalla, contraste, movimiento reducido, responsive y estados
  de error o sesión expirada.

**Verificación:** typecheck, ESLint, formato, build reproducible, pruebas de componentes y
Playwright de los recorridos críticos en escritorio y teléfono.

### 6. Puertas de producción web

- Medir cobertura mínima global de 80 % y de 90 % para seguridad, saldos y asistencia.
- Ejecutar pruebas de rendimiento, límites, concurrencia, backups de la base web y recuperación.
- Validar despliegue en staging con configuración segura y sin credenciales versionadas.
- Obtener acta de aceptación funcional por cada dominio y una revisión final de accesibilidad,
  seguridad y observabilidad.

**Criterio de 100 % web:** todas las puertas anteriores aprobadas, todos los dominios cerrados,
la entrada activa es modular, las rutas canónicas son las únicas publicadas y no existe ninguna
dependencia de ejecución de WinForms. No se requiere ni se ejecuta la migración de datos local
para declarar este hito.

## Puertas de calidad y evidencia

| Puerta | Evidencia exigida | Estado actual |
|---|---|---|
| Arquitectura y límites | Guardas y revisión de dependencias | Parcial |
| Tipos y estilo | TypeScript, ESLint, Prettier, Ruff, mypy | Parcial |
| Pruebas | Unitarias, integración, contratos y Playwright | Parcial |
| Cobertura | 80 % global; 90 % seguridad/saldos/asistencia | Pendiente |
| Persistencia | SQLAlchemy/Alembic, base vacía y reversión | Pendiente |
| Seguridad | Argon2id, sesiones, CSRF y RBAC en API activa | Pendiente |
| Accesibilidad | WCAG AA, teclado, lector y movimiento reducido | Pendiente |
| Operación | Staging, observabilidad, respaldo y recuperación | Pendiente |
| Aceptación | Acta por dominio y acta de plataforma | Pendiente |

La evidencia debe quedar enlazada desde el plan global o desde el README del módulo. No se
marcará una casilla por la mera existencia de archivos o documentación.

## Fase posterior: migración WinForms (pospuesta)

Solo después del hito “100 % web” se abrirá un plan separado para ensayos anonimizados,
respaldo, congelamiento, migración única, reconciliación, invalidación de credenciales y
retiro de accesos. Ese plan no forma parte del cierre web y no autoriza modificar las fuentes
históricas mientras permanezca pospuesto.

## Registro de control

| Fecha | Decisión | Evidencia |
|---|---|---|
| 2026-08-26 | Se separa el cierre de la plataforma web de la migración posterior de WinForms. | Este documento y enlace en `README.md` |
