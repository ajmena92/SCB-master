# Plan de Refactorización SCB (2026-03-04)

## 1. Estado actual validado
- Arquitectura: WinForms VB.NET (.NET Framework 4.6.1) con acoplamiento UI + reglas + acceso SQL.
- Código: 55 archivos `.vb` (22 designer), total aproximado de 13,647 líneas.
- Núcleo técnico: `SCSC/Clases/FunccionesDB.vb` (1,384 líneas) centraliza CRUD y transacciones.
- Dependencias críticas: DigitalPersona (captura/huella) y Crystal Reports.
- Base de datos SQL Server accesible y validada (`SCSC`, host `172.30.210.200`).
- Tablas núcleo operativas confirmadas: `Usuario`, `RegistroComedor`, `RegistroTransporte`, `RegistroDocentes`, `TipoBeca`, `Horario`, `Ruta`, `Parametro`, `TablaSeguridad`.

## 2. Riesgos principales
- Riesgo alto de regresión por lógica de negocio embebida en eventos de formularios.
- Seguridad: credenciales en `SCSC/app.config` y validación de login en texto plano para rutas legacy.
- SQL dinámico por concatenación en varios puntos de formularios/importación.
- Estado global compartido (`VariablesGlobales`, `gSession`) dificulta trazabilidad y pruebas.
- Manejo de errores heterogéneo (`MsgBox`, `On Error GoTo`, `Throw ex`, `End`) con baja consistencia operativa.

## 3. Objetivo de refactor
Reducir riesgo operativo sin romper flujos de producción, separando gradualmente:
- Capa UI (formularios/eventos)
- Capa de casos de uso (servicios)
- Capa de acceso a datos (repositorios/consultas)

## 4. Estrategia por fases

## Fase 0: Línea base y control de cambios
- Definir checklist manual obligatorio por flujo:
  - Login
  - Marca transporte
  - Marca comedor
  - Gestión estudiantes
  - Importación Excel
  - Reportes comedor/rutas/becas
- Congelar baseline funcional con evidencia manual por pantalla.
- Registrar incidentes actuales conocidos para distinguir bug existente vs regresión.

## Fase 1: Endurecimiento de acceso a datos (sin cambiar funcionalidad)
- Crear carpeta `SCSC/Clases/Servicios` con servicios orientados a caso de uso.
- Introducir una fachada de datos para reemplazar SQL ad-hoc en formularios críticos.
- Priorizar consultas de:
  - `ControlComedor`
  - `ControlTransporte`
  - `FrmImportarExcel`
- Estandarizar transacciones (`IniciaSQL/FinalSQL/RollSQL`) con manejo uniforme de errores.
- Meta: cero cambio visible para usuario final.

## Fase 2: Seguridad y configuración
- Eliminar secretos del repositorio y migrar a configuración por entorno.
- Sustituir autenticación legacy en claro por validación segura.
- Eliminar concatenación SQL en rutas críticas.
- Estandarizar mensajes de error para no exponer detalles de infraestructura.

## Fase 3: Modularización de reglas de negocio
- Extraer reglas de comedor/transporte a servicios testables.
- Reducir dependencia directa de `VariablesGlobales`.
- Introducir contratos internos (`Interfaces`) para desacoplar formularios de implementación DB.

## Fase 4: Calidad y mantenimiento
- Añadir pruebas automatizadas para lógica pura (helpers/servicios).
- Crear smoke script manual ejecutable con pasos reproducibles.
- Documentar arquitectura objetivo y guía de contribución técnica.

## 5. Sprint 1 propuesto (inicio inmediato)
Duración sugerida: 1 semana técnica.

Entregables:
- Inventario de consultas SQL por formulario crítico.
- Servicio inicial para `ControlTransporte` (solo lectura + inserción de marca).
- Servicio inicial para `ControlComedor` (validación y registro de marca).
- Unificación de manejo de errores de DB en rutas tocadas.
- Checklist de pruebas manuales ejecutado y documentado.

Criterio de salida:
- Flujos de transporte/comedor operan igual que baseline.
- No hay cambios de esquema ni de UI visual.
- Logs/mensajes de error más consistentes.

## 6. Métricas de seguimiento
- Número de consultas SQL embebidas en formularios críticos.
- Número de métodos con SQL movidos a servicios.
- Número de llamadas directas a `FuncionesDB` desde UI.
- Número de flujos baseline ejecutados sin regresión.

## 7. Secuencia de ejecución recomendada
1. Refactor `ControlTransporte` (menor complejidad relativa).
2. Refactor `ControlComedor` (reglas más sensibles).
3. Refactor `FrmImportarExcel` (transaccional + volumen de datos).
4. Refactor login/configuración segura.

## 8. Decisiones tomadas hoy
- Se confirma conectividad real a SQL Server de producción técnica del sistema (`172.30.210.200:1433`).
- Se instala `sqlcmd` local para diagnóstico y validaciones futuras.
- Se establece estrategia de refactor incremental sin Big Bang.
