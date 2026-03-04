# Dependency Cleanup Report

Fecha: 2026-03-04

## Referencias removidas del proyecto
Se removieron referencias .NET no usadas en código VB del proyecto:
- `System.Data.Entity`
- `System.Deployment`
- `System.Management`
- `System.Web`
- `System.Web.Extensions`
- `System.Web.Services`

Archivo afectado:
- `SCSC/SCSC_Marcas.vbproj`

## Referencias mantenidas por compatibilidad
- `CrystalDecisions.*` (motor de reportes activo)

## Refactor de dependencia UI legacy completado (Fase actual)
- Se eliminaron todas las referencias `Bunifu.*` del archivo de proyecto:
  - `SCSC/SCSC_Marcas.vbproj`
- Se removió el paquete NuGet `Bunifu.UI.WinForms` de:
  - `SCSC/packages.config`
- Se agregó capa interna de compatibilidad para no romper los `*.Designer.vb` actuales:
  - `SCSC/Clases/BunifuLegacyCompat.vb`
  - Namespace mantenido: `Bunifu.Framework.UI`

## Próxima limpieza
- Migrar gradualmente los `*.Designer.vb` para reemplazar tipos `Bunifu.Framework.UI` por controles WinForms nativos.
- Al completar esa migración, retirar la capa `BunifuLegacyCompat.vb`.
- Ejecutar validación visual en entorno Windows 11 (100%, 125%, 150% DPI).
