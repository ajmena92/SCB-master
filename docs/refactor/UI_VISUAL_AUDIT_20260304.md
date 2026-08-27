# Auditoria Visual UI (2026-03-04)

## Alcance
Revision de consistencia visual en formularios WinForms bajo estandar hibrido 2026.

## Estado General
- Modelo adoptado: `Designer-first` + tematizacion runtime segura.
- Baseline canonico: `LOGIN`.
- Resultado: fase visual principal y formularios de parametros/reportes alineados; queda validacion visual final en DPI altos.

## Completado (alineado)
1. Login y autenticacion:
   - `escritorio/SCSC/Seguridad/LOGIN.vb`

2. Shell principal y dashboard:
   - `escritorio/SCSC/FrmPrincipal.vb`
   - `escritorio/SCSC/Clases/UIShellHost.vb`

3. Comedor (kiosko/operacion):
   - `escritorio/SCSC/Formularios/ControlComedor.vb`

4. Transporte (kiosko/operacion):
   - `escritorio/SCSC/Formularios/ControlTransporte.vb`

5. CRUD estandarizados (cromado comun):
   - `escritorio/SCSC/Formularios/FrmEstudiantes.vb`
   - `escritorio/SCSC/Formularios/FrmBecas.vb`
   - `escritorio/SCSC/Formularios/FrmRutas.vb`
   - `escritorio/SCSC/Formularios/FrmRecargas.vb`
   - `escritorio/SCSC/Formularios/FrmAgregarEstudiante.vb`
   - `escritorio/SCSC/Formularios/FrmImportarDatos.vb`
   - `escritorio/SCSC/Formularios/FrmImportarExcel.vb`
   - `escritorio/SCSC/Formularios/FrmSeguridadRBAC.vb`
   - `escritorio/SCSC/Formularios/FrmParametrosSistema.vb`

6. Parametros de reportes alineados:
   - `escritorio/SCSC/Reportes/Parametros/FrmReporteComedor.vb`
   - `escritorio/SCSC/Reportes/Parametros/FrmReporteRutas.vb`
   - `escritorio/SCSC/Reportes/Parametros/FrmBecados.vb`
   - `escritorio/SCSC/Reportes/Parametros/FrmProyeccionComedor.vb`

7. Utilitarios alineados:
   - `escritorio/SCSC/Formularios/FrmAyuda.vb`
   - `escritorio/SCSC/Formularios/IMPRIMIR.vb`

8. Infraestructura visual comun:
   - `escritorio/SCSC/Clases/UIThemeManagerV2.vb` (`ApplyCrudModuleChrome`, layout de barra de acciones, estilo de campos)

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
