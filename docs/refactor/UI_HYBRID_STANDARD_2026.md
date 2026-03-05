# Estandar UI Hibrido 2026 (Windows 11)

## Objetivo
Definir un estandar visual y tecnico unico para modernizar WinForms con apariencia Windows 11 sin romper compatibilidad legacy del sistema SCB.

## Referencia Canonica
- Formulario piloto oficial: `SCSC/Seguridad/LOGIN.vb` + `SCSC/Seguridad/LOGIN.designer.vb`.
- Todo formulario nuevo o migrado debe tomar `LOGIN` como baseline de:
  - Jerarquia visual.
  - Escalado y espaciado.
  - Estados de controles.
  - Integracion Designer-first + tematizacion runtime.

## Modelo Hibrido (obligatorio)
- `Designer-first` para estructura y geometria:
  - Posiciones, tamanos, docking, anchors y tipografia base van en `*.designer.vb`.
- `Runtime-theme` para comportamiento visual dinamico:
  - `UIThemeManagerV2.Apply(...)` y ajustes no destructivos en `Form.vb`.
- Regla de oro:
  - Runtime no debe contradecir ni reemplazar el layout base definido en Designer.

## Paleta Oficial 2026
Fuente principal de tokens: `SCSC/Clases/UIConstants.vb`.

### Base
- `AppBackground`: `#F6F8FB` (246, 248, 251)
- `Surface`: `#FFFFFF` (255, 255, 255)
- `SurfaceAlt`: `#F0F4F8` (240, 244, 248)
- `Border`: `#E5E7EB` (229, 231, 235)

### Texto
- `TextPrimary`: `#111827` (17, 24, 39)
- `TextSecondary`: `#6B7280` (107, 114, 128)

### Accion
- `Accent`: `#0A84FF` (10, 132, 255)
- `AccentHover`: `#0071E3` (0, 113, 227)
- Exito: `#16A34A`
- Advertencia: `#D97706`
- Error: `#DC2626`

### Login (acentos de alto contraste)
- Boton principal activo: `#1877F2` (24, 119, 242)
- Boton principal disabled: `#A8B4C2` (168, 180, 194)
- Overlay panel izquierdo: negro con opacidad aprox `28%`.

## Tipografia Oficial
- Familia: `Segoe UI` / `Segoe UI Semibold` / `Segoe UI Black` (segun jerarquia).
- Escala recomendada:
  - Titulo principal: 32-36 pt.
  - Subtitulo: 12-14 pt.
  - Caption campo: 10 pt bold.
  - Texto de entrada: 13-14 pt.
  - CTA principal: 11-12 pt semibold/bold.

## Layout y Espaciado
- Espaciado base de `UIConstants`:
  - `SpaceXs=6`, `SpaceSm=10`, `SpaceMd=16`, `SpaceLg=24`.
- Radios:
  - `RadiusStandard=8`
  - `RadiusCard=12`
- Escalado objetivo: correcto en 100%, 125% y 150% DPI.
- Para formularios grandes:
  - Definir `MinimumSize` util.
  - Usar `Anchor` consistente.
  - Evitar posiciones hardcoded sin recalc en `Resize`.

## Componentes Estandar
- Boton primario:
  - Fondo accent.
  - Texto blanco.
  - Hover y pressed definidos.
  - Puede llevar icono monocromatico a la izquierda.
- Boton secundario:
  - Fondo `Surface`.
  - Borde `Border`.
  - Texto `TextPrimary`.
- Inputs:
  - Alto visual amplio (lectura comoda).
  - Placeholder controlado en runtime.
  - Estado foco con separador/borde resaltado.
- Modulos tipo tarjeta (ej. Comedor/Transporte en Login):
  - Alto minimo 48-52 px.
  - Icono de alto contraste.
  - Texto siempre visible y legible.

## Interaccion y Accesibilidad
- Teclado:
  - `Enter` en password ejecuta login.
  - `Esc` cierra dialogo de login de forma controlada.
- Estado CTA:
  - Habilitar boton principal solo con datos validos (o modo tecnico habilitado).
- Contraste:
  - Evitar texto sobre imagen sin overlay.
  - No usar `BackColor.Transparent` en controles que no lo soportan.

## Politica Designer-first para Migracion
1. Ajustar primero la vista en Designer.
2. Limpiar residuos legacy en `*.designer.vb` y `*.resx`.
3. Aplicar solo comportamiento/tema seguro en `Form.vb`.
4. Validar:
   - Designer abre sin error.
   - Runtime mantiene la misma composicion visual.
   - No hay cierres inesperados ni parpadeos.

## Definicion de Hecho (UI)
- Formulario abre en Designer sin errores.
- UI respeta paleta y tipografia oficial.
- Botones y textos no se cortan con fuente/tamano actual.
- Soporta al menos 100% y 125% DPI sin superposicion.
- Sin dependencias visuales Bunifu legacy en formularios migrados.
- Smoke test manual documentado.

## Orden de Implementacion Recomendado
1. `LOGIN` (ya baseline).
2. `FrmPrincipal` (shell).
3. `ControlComedor`.
4. `ControlTransporte`.
5. Formularios CRUD de soporte.
6. Reportes parametros/visor (sin tocar motor Crystal).

## Notas de Gobierno
- Cualquier desviacion visual debe quedar documentada como excepcion en PR.
- Si Designer y runtime no coinciden, prevalece `Designer-first` y se corrige runtime.

## Autobuild estandar (MSBuild local)
- Script oficial: `scripts/autobuild.ps1`.
- Objetivo:
  - Detectar cambios de UI/codigo y compilar automaticamente para validar que Designer/runtime siguen coherentes.
- Archivos que disparan build:
  - `.vb`, `.vbproj`, `.resx`, `.config`.
- Parametros principales:
  - `-Repo` (default `C:\Dev\SCB-master`)
  - `-Solution` (default `SCSC_Marcas.sln`)
  - `-MSBuildPath` (ruta local validada)
  - `-Configuration` (default `Debug`)
  - `-Platform` (default `Any CPU`)
  - `-DebounceSeconds` (default `2`)
  - `-MaxLogFiles` (default `120`)
- Ejecucion recomendada:
  - `powershell -ExecutionPolicy Bypass -File C:\Dev\SCB-master\scripts\autobuild.ps1`
- Salida esperada:
  - Consola: `RUNNING`, `OK` o `FAIL`.
  - Logs en `C:\Dev\SCB-master\logs`:
    - `build-YYYYMMDD-HHMMSS.txt`
    - `build-latest.txt`
    - `build-status.txt`
- Criterio de control:
  - Si aparece `FAIL`, revisar primero `build-latest.txt` antes de continuar cambios visuales.

## Estado de adopcion actual (2026-03-04)
- Baseline completo:
  - `SCSC/Seguridad/LOGIN.vb`
- Shell moderno y dashboard:
  - `SCSC/FrmPrincipal.vb`
  - `SCSC/Clases/UIShellHost.vb`
  - Ajuste responsive 2026-03-04: modo ancho/compacto para evitar perdida de graficas en resoluciones menores.
- Operacion comedor (hibrido con `designer-first` estricto):
  - `SCSC/Formularios/ControlComedor.vb`
- Operacion transporte (hibrido con `designer-first` estricto):
  - `SCSC/Formularios/ControlTransporte.vb`
- CRUD con cromado estandar (`ApplyCrudModuleChrome`):
  - `SCSC/Formularios/FrmEstudiantes.vb`
  - `SCSC/Formularios/FrmBecas.vb`
  - `SCSC/Formularios/FrmRutas.vb`
  - `SCSC/Formularios/FrmRecargas.vb`
  - `SCSC/Formularios/FrmAgregarEstudiante.vb`
  - `SCSC/Formularios/FrmImportarDatos.vb`
  - `SCSC/Formularios/FrmImportarExcel.vb`
- Utilitarios alineados al estandar visual:
  - `SCSC/Formularios/FrmAyuda.vb`
  - `SCSC/Formularios/IMPRIMIR.vb`
- Auditoria de cierre de fase:
  - `docs/refactor/UI_VISUAL_AUDIT_20260304.md`
