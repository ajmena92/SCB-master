# SCB / SCSC_Marcas

Aplicación WinForms en VB.NET (.NET Framework 4.8) para control de marcas de comedor y transporte, mantenimiento de estudiantes, recargas, reportes Crystal Reports e importación de datos desde Excel/PIAD.

## Estado actual
- Solución principal de escritorio: `escritorio/SCSC_Marcas.sln`
- Proyecto principal: `escritorio/SCSC/SCSC_Marcas.vbproj`
- Código VB detectado: 80 archivos `.vb`
- Código VB no autogenerado: 64 archivos `.vb`
- Módulos/pantallas/reportes operativos: 33 archivos `.vb` bajo `Formularios`, `Seguridad` y `Reportes`
- Framework: `.NET Framework 4.8`
- Dependencias sensibles de entorno: SQL Server, Crystal Reports, SDK DigitalPersona

## Módulos principales
- `escritorio/SCSC/FrmPrincipal.vb`: shell principal, navegación y dashboard.
- `escritorio/SCSC/Seguridad/LOGIN.vb`: autenticación y carga inicial de parámetros.
- `escritorio/SCSC/Formularios/ControlComedor.vb`: registro operativo de comedor.
- `escritorio/SCSC/Formularios/ControlTransporte.vb`: registro operativo de transporte.
- `escritorio/SCSC/Formularios/FrmEstudiantes.vb`: mantenimiento principal de estudiantes.
- `escritorio/SCSC/Formularios/FrmImportarExcel.vb` y `escritorio/SCSC/Formularios/FrmImportarDatos.vb`: importación masiva.
- `escritorio/SCSC/Reportes/FrmReportViewer.vb`: salida de reportes Crystal Reports.
- `escritorio/SCSC/Clases/FunccionesDB.vb`: utilidades CRUD/SQL legacy.
- `escritorio/SCSC/Clases/Servicios/`: capa de servicios introducida para desacoplar lógica crítica.

## Estructura
- `escritorio/`: solución WinForms, pruebas, instalador, utilitarios y dependencias.
- `web/`: documentación y futura aplicación web del comedor; no contiene código generado todavía.
- `RESPALDO_BD/`, `backups/`, `Lista inicial/` y `Lista 2023/`: datos y respaldos operativos mantenidos en la raíz.
- `docs/refactor`: documentación técnica, roadmap y backlog.
- `escritorio/build/` y `escritorio/scripts/`: configuración y automatización de build en Windows.

## Build y ejecución
Restaurar paquetes:

```bash
nuget restore escritorio/SCSC_Marcas.sln
```

Compilar en Windows con MSBuild/Visual Studio:

```bash
msbuild escritorio/SCSC_Marcas.sln /p:Configuration=Debug /p:Platform="Any CPU"
```

En este workspace WSL no hay `msbuild` ni `nuget` disponibles por defecto, así que la validación completa de compilación sigue dependiendo del entorno Windows/Visual Studio.

## Comando deploy
Para generar el instalador completo desde Windows y abrir la carpeta final del artefacto:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1"
```

`deploy.ps1` ahora:
- sincroniza version
- sincroniza icono embebido
- valida prerequisitos de build y despliegue
- compila la aplicacion
- ejecuta las pruebas de `SCSC.Tests`
- solo si las pruebas pasan, genera MSI y `SCSC-Setup.exe`
- crea una carpeta de release versionada con artefactos, payloads externos del bundle, checksums y manifest
- puede firmar `MSI` y `Setup.exe` si se usa `-SignArtifacts`

Si ocupas saltar las pruebas manualmente:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1" -SkipTests
```

Firma digital opcional:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\deploy.ps1" -SignArtifacts -PfxPath "C:\Certs\scsc-code-sign.pfx" -PfxPassword "CLAVE_AQUI"
```

Smoke automático de build y despliegue:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\smoke-build.ps1"
```

Proyecto base de pruebas:
- `escritorio/SCSC.Tests/SCSC.Tests.vbproj`
- Cobertura inicial: licencia, configuracion de despliegue, `CodigoGeneral` y seguridad pura

Ejecutar pruebas desde Windows con MSBuild/VSTest de Visual Studio:

```powershell
powershell -ExecutionPolicy Bypass -File "C:\Dev\SCB-master\escritorio\scripts\test.ps1"
```

Artefacto final esperado:
- `C:\Dev\SCB-master\escritorio\Installer\Bundle\bin\Release\SCSC-Setup.exe`
- `C:\Dev\SCB-master\artifacts\releases\<version>\SCSC-Setup.exe`

## Configuración sensible
El repositorio ya no debe guardar secretos reales en `escritorio/SCSC/app.config`. Antes de ejecutar en un entorno real, definir como variables de entorno:

- `SCSC_APPSETTING_LLAVEENCRIPTACION`
- `SCSC_APPSETTING_ADMINUSUARIO`
- `SCSC_APPSETTING_ADMINCLAVESOPORTE`

Resolución actual de conexión:

- `DB_PROFILE=LOCAL`: usa `ConexionLocal` en `app.config`.
- `DB_PROFILE=INSTALLED`: usa `%ProgramData%\SCSC\deployment.config.json`.
- `DB_PROFILE=LEGACY`: usa `Conexion` en `app.config`.
- Si el perfil es inválido, se usa `INSTALLED`.

## Documentación recomendada
- [Análisis actual del proyecto](docs/refactor/PROJECT_ANALYSIS_20260309.md)
- [Baseline de build y smoke manual](docs/refactor/BUILD_SMOKE_BASELINE_20260309.md)
- [Backlog de deuda técnica pendiente](docs/refactor/TECH_DEBT_BACKLOG_20260309.md)
- [Índice técnico histórico](docs/refactor/PROJECT_INDEX.md)
- [Roadmap de refactor](docs/refactor/REFACTOR_ROADMAP.md)
- [Guía Designer-first](docs/refactor/DESIGNER_FIRST_GUIDE.md)
- [Politica de versionado y actualizacion](docs/deployment/VERSIONING.md)
- [Checklist de smoke test de release](docs/deployment/SMOKE_TEST_CHECKLIST.md)
- [Terminos generales de licencia](docs/legal/TERMINOS_LICENCIA_CR.md)
- [Guía y requisitos del futuro portal web](web/README.md)

## Hallazgos relevantes al 2026-03-09
- El proyecto combina código legacy orientado a formularios con una capa de servicios nueva, pero la separación todavía es parcial.
- Persisten globals compartidos (`VariablesGlobales`) y acceso a datos genérico en `FuncionesDB`.
- Los secretos sensibles se movieron a variables de entorno, pero sigue pendiente rotar cualquier credencial histórica ya expuesta y limpiar referencias legacy/documentales.
- El flujo de reportes ya no depende de `gSession`, y el flujo de búsqueda ya migró a `SearchRequest`.
- El archivo de proyecto vuelve a exponer warnings del compilador; falta validar el baseline real de build en Windows.
