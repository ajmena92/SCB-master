# SCB / SCSC_Marcas

Aplicación WinForms en VB.NET (.NET Framework 4.6.1) para control de marcas de comedor y transporte, mantenimiento de estudiantes, recargas, reportes Crystal Reports e importación de datos desde Excel/PIAD.

## Estado actual
- Solución principal: `SCSC_Marcas.sln`
- Proyecto principal: `SCSC/SCSC_Marcas.vbproj`
- Código VB detectado: 80 archivos `.vb`
- Código VB no autogenerado: 64 archivos `.vb`
- Módulos/pantallas/reportes operativos: 33 archivos `.vb` bajo `Formularios`, `Seguridad` y `Reportes`
- Framework: `.NET Framework 4.6.1`
- Dependencias sensibles de entorno: SQL Server, Crystal Reports, SDK DigitalPersona

## Módulos principales
- `SCSC/FrmPrincipal.vb`: shell principal, navegación y dashboard.
- `SCSC/Seguridad/LOGIN.vb`: autenticación y carga inicial de parámetros.
- `SCSC/Formularios/ControlComedor.vb`: registro operativo de comedor.
- `SCSC/Formularios/ControlTransporte.vb`: registro operativo de transporte.
- `SCSC/Formularios/FrmEstudiantes.vb`: mantenimiento principal de estudiantes.
- `SCSC/Formularios/FrmImportarExcel.vb` y `SCSC/Formularios/FrmImportarDatos.vb`: importación masiva.
- `SCSC/Reportes/FrmReportViewer.vb`: salida de reportes Crystal Reports.
- `SCSC/Clases/FunccionesDB.vb`: utilidades CRUD/SQL legacy.
- `SCSC/Clases/Servicios/`: capa de servicios introducida para desacoplar lógica crítica.

## Estructura
- `SCSC/Clases`: utilidades, acceso a datos, globals, theming y servicios.
- `SCSC/Formularios`: CRUDs y pantallas operativas.
- `SCSC/Seguridad`: login.
- `SCSC/Reportes`: formularios de parámetros y `.rpt`.
- `docs/refactor`: documentación técnica, roadmap y backlog.
- `scripts/autobuild.ps1`: automatización de build en entorno Windows.

## Build y ejecución
Restaurar paquetes:

```bash
nuget restore SCSC_Marcas.sln
```

Compilar en Windows con MSBuild/Visual Studio:

```bash
msbuild SCSC_Marcas.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

En este workspace WSL no hay `msbuild` ni `nuget` disponibles por defecto, así que la validación completa de compilación sigue dependiendo del entorno Windows/Visual Studio.

## Configuración sensible
El repositorio ya no debe guardar secretos reales en `SCSC/app.config`. Antes de ejecutar en un entorno real, definir como variables de entorno:

- `SCSC_CONNECTION_STRING`
- `SCSC_APPSETTING_LLAVEENCRIPTACION`
- `SCSC_APPSETTING_ADMINUSUARIO`
- `SCSC_APPSETTING_ADMINCLAVESOPORTE`

El código ahora prioriza variables de entorno sobre `appSettings`.

## Documentación recomendada
- [Análisis actual del proyecto](docs/refactor/PROJECT_ANALYSIS_20260309.md)
- [Backlog de deuda técnica pendiente](docs/refactor/TECH_DEBT_BACKLOG_20260309.md)
- [Índice técnico histórico](docs/refactor/PROJECT_INDEX.md)
- [Roadmap de refactor](docs/refactor/REFACTOR_ROADMAP.md)
- [Guía Designer-first](docs/refactor/DESIGNER_FIRST_GUIDE.md)

## Hallazgos relevantes al 2026-03-09
- El proyecto combina código legacy orientado a formularios con una capa de servicios nueva, pero la separación todavía es parcial.
- Persisten globals compartidos (`VariablesGlobales`, `gSession`) y acceso a datos genérico en `FuncionesDB`.
- Los secretos sensibles se movieron a variables de entorno, pero sigue pendiente rotar cualquier credencial histórica ya expuesta y limpiar referencias legacy/documentales.
- El archivo de proyecto mantiene `WarningLevel=0`, por lo que parte del riesgo de compilación sigue oculto.
