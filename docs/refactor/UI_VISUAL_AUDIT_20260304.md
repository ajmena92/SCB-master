# Auditoria Visual UI (2026-03-04)

## Alcance
Revision de consistencia visual en formularios WinForms bajo estandar hibrido 2026.

## Estado General
- Modelo adoptado: `Designer-first` + tematizacion runtime segura.
- Baseline canonico: `LOGIN`.
- Resultado: fase visual principal y formularios de parametros/reportes alineados; queda validacion visual final en DPI altos.

## Completado (alineado)
1. Login y autenticacion:
   - `SCSC/Seguridad/LOGIN.vb`

2. Shell principal y dashboard:
   - `SCSC/FrmPrincipal.vb`
   - `SCSC/Clases/UIShellHost.vb`

3. Comedor (kiosko/operacion):
   - `SCSC/Formularios/ControlComedor.vb`

4. Transporte (kiosko/operacion):
   - `SCSC/Formularios/ControlTransporte.vb`

5. CRUD estandarizados (cromado comun):
   - `SCSC/Formularios/FrmEstudiantes.vb`
   - `SCSC/Formularios/FrmBecas.vb`
   - `SCSC/Formularios/FrmRutas.vb`
   - `SCSC/Formularios/FrmRecargas.vb`
   - `SCSC/Formularios/FrmAgregarEstudiante.vb`
   - `SCSC/Formularios/FrmImportarDatos.vb`
   - `SCSC/Formularios/FrmImportarExcel.vb`
   - `SCSC/Formularios/FrmSeguridadRBAC.vb`
   - `SCSC/Formularios/FrmParametrosSistema.vb`

6. Parametros de reportes alineados:
   - `SCSC/Reportes/Parametros/FrmReporteComedor.vb`
   - `SCSC/Reportes/Parametros/FrmReporteRutas.vb`
   - `SCSC/Reportes/Parametros/FrmBecados.vb`
   - `SCSC/Reportes/Parametros/FrmProyeccionComedor.vb`

7. Utilitarios alineados:
   - `SCSC/Formularios/FrmAyuda.vb`
   - `SCSC/Formularios/IMPRIMIR.vb`

8. Infraestructura visual comun:
   - `SCSC/Clases/UIThemeManagerV2.vb` (`ApplyCrudModuleChrome`, layout de barra de acciones, estilo de campos)

## Pendientes de diseno (siguiente fase)
1. Ajuste fino final por DPI:
   - Validacion visual en 100%, 125%, 150%.
2. Smoke visual manual de flujos kiosko:
   - confirmar botoneras y estado operativo en pantalla fisica.

## Riesgos vigentes
1. Diferencias de render por DPI/escala de Windows en equipos no validados.
2. Cambios visuales directos fuera de helper comun pueden generar regresion futura.

## Criterio de cierre de fase visual
- Designer abre sin error en formularios criticos.
- Flujo de login + principal + comedor + CRUD sin cortes visuales.
- Botoneras de accion con texto visible y no superpuesto.
- Dashboard estable al redimensionar.

## Siguiente lote recomendado
1. Ejecutar smoke visual completo (kiosko + CRUD + reportes parametros) en VS.
2. Validar DPI 100/125/150 y capturar evidencia.
3. Congelar estandar visual hibrido 2026 para nuevas pantallas.
