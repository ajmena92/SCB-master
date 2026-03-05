# Auditoria Visual UI (2026-03-04)

## Alcance
Revision de consistencia visual en formularios WinForms bajo estandar hibrido 2026.

## Estado General
- Modelo adoptado: `Designer-first` + tematizacion runtime segura.
- Baseline canonico: `LOGIN`.
- Resultado: fase visual principal avanzada, con pendiente de cierre en modulos operativos restantes.

## Completado (alineado)
1. Login y autenticacion:
   - `SCSC/Seguridad/LOGIN.vb`

2. Shell principal y dashboard:
   - `SCSC/FrmPrincipal.vb`
   - `SCSC/Clases/UIShellHost.vb`

3. Comedor (kiosko/operacion):
   - `SCSC/Formularios/ControlComedor.vb`

4. CRUD estandarizados (cromado comun):
   - `SCSC/Formularios/FrmEstudiantes.vb`
   - `SCSC/Formularios/FrmBecas.vb`
   - `SCSC/Formularios/FrmRutas.vb`
   - `SCSC/Formularios/FrmRecargas.vb`
   - `SCSC/Formularios/FrmAgregarEstudiante.vb`
   - `SCSC/Formularios/FrmImportarDatos.vb`
   - `SCSC/Formularios/FrmImportarExcel.vb`

5. Utilitarios alineados:
   - `SCSC/Formularios/FrmAyuda.vb`
   - `SCSC/Formularios/IMPRIMIR.vb`

6. Infraestructura visual comun:
   - `SCSC/Clases/UIThemeManagerV2.vb` (`ApplyCrudModuleChrome`, layout de barra de acciones, estilo de campos)

## Pendientes de diseno (siguiente fase)
1. `ControlTransporte`:
   - Aun usa recalc de geometria runtime amplio.
   - Requiere converger al mismo nivel de `designer-first` estricto aplicado en `ControlComedor`.

2. Parametros/reportes:
   - Revisar y unificar estilo en formularios de `SCSC/Reportes/Parametros/*`.

3. Ajuste fino final por DPI:
   - Validacion visual en 100%, 125%, 150%.

## Riesgos vigentes
1. Diferencias entre Designer y runtime en formularios con recalc agresivo.
2. Cambios visuales dispersos fuera de helper comun pueden generar regresion futura.

## Criterio de cierre de fase visual
- Designer abre sin error en formularios criticos.
- Flujo de login + principal + comedor + CRUD sin cortes visuales.
- Botoneras de accion con texto visible y no superpuesto.
- Dashboard estable al redimensionar.

## Siguiente lote recomendado
1. Migrar `ControlTransporte` a modo estricto de layout hibrido.
2. Estandarizar `Reportes/Parametros`.
3. Ejecutar smoke visual completo y congelar estandar.
