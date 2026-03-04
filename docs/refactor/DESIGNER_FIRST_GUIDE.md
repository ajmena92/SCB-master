# Modo Operativo: Designer-first (WinForms)

## Contexto
Se detectó desalineación entre lo que Visual Studio Designer muestra y lo que la aplicación ejecuta en runtime (caso `LOGIN`), provocando:
- Errores de diseñador (`none of the classes can be designed`).
- Referencias legacy residuales en `*.designer.vb`/`*.resx`.
- Comportamiento inestable al iniciar (parpadeo/cierre sin trazabilidad suficiente).

Este documento establece el estándar obligatorio para cambios UI WinForms en SCB.

## Regla principal
`Designer-first` significa:
1. La fuente de verdad de estructura UI es el par `Form.vb` + `Form.designer.vb` generado por Designer.
2. Los cambios de controles/propiedades visuales se hacen primero en Designer.
3. El code-behind (`Form.vb`) solo aplica estilo/comportamiento runtime, sin romper diseño.

## Regla operativa estricta (2026-03-04)
Para formularios críticos (`LOGIN`, `FrmSeguridadRBAC`) se aplica modo estricto:
1. Prohibido crear/reubicar contenedores estructurales en runtime (`SplitContainer`, paneles de cabecera, redistribución de filas/columnas).
2. Prohibido recalcular geometría estructural en `Load/Shown/Resize/SelectedIndexChanged`.
3. En runtime solo se permite:
   - Aplicar tema visual (color, fuente, borde, estados de botón).
   - Cargar datos y enlazar grillas/combos.
   - Lógica funcional de eventos.
4. Si un ajuste visual requiere mover controles, se hace exclusivamente en Designer y se valida reabriendo Designer antes de probar runtime.

## Reglas obligatorias
1. No mezclar migraciones de controles legacy con lógica funcional en el mismo commit.
2. No dejar en `*.resx` metadata de componentes eliminados (ej. `BunifuElipse1.*`).
3. Mantener clase de formulario como `Partial` en code-behind y herencia `Inherits Form` en `*.designer.vb`.
4. Evitar inicialización pesada en declaración de campos del formulario (`As New ...`) cuando impacte diseño.
5. En `Load/Resize/FormClosing`, proteger ejecución en diseño (`IsInDesignMode()`).
6. Si un control deja de ser Bunifu y pasa a nativo, renombrarlo a nombre nativo (`SeparatorUsuario`, etc.).
7. Nunca depender de nombres legacy en lógica nueva.

## Anti-patrones prohibidos
- Editar a mano `*.designer.vb` para reintroducir propiedades de controles no existentes (`LineColor`, `LineThickness`, etc.).
- Llamar `Application.Exit()` desde `Load` del login.
- Dejar rutas críticas sin logging.
- Mantener dos fuentes de verdad para un mismo control (Designer vs runtime).

## Flujo de trabajo recomendado
1. Abrir formulario en Designer y aplicar cambios visuales estructurales.
2. Revisar `Form.designer.vb` y `Form.resx` para residuos de componentes legacy.
3. Aplicar estilos runtime en `Form.vb` (tema, layout dinámico, eventos).
4. Ejecutar smoke manual:
   - Abrir Designer del formulario sin error.
   - Ejecutar formulario en runtime.
   - Validar flujo funcional y cierre.
5. Revisar log de arranque si el formulario es crítico (ej. `Login`).

## Checklist de PR (UI WinForms)
- [ ] El formulario abre en Designer sin error.
- [ ] No existen referencias legacy rotas en `*.designer.vb`/`*.resx`.
- [ ] `Form.vb` tiene guardas de diseño para eventos sensibles.
- [ ] Sin `Application.Exit()` en `Load` de formularios de autenticación.
- [ ] Se validó smoke runtime del flujo impactado.
- [ ] Se documentó evidencia (captura o pasos + resultado).

## Logging mínimo requerido (arranque/login)
- Registrar inicio de flujo.
- Registrar resultado de login (`OK/Cancel`).
- Registrar `FormClosing` con `CloseReason`, `DialogResult`, bandera de autorización de cierre.
- Guardar en `%LOCALAPPDATA%\\SCSC\\logs\\`.

## Incidente documentado (2026-03-04)
### Sintoma
- Visual Studio Designer no abre formularios (`LOGIN` y otros) con mensajes como:
  - `The designer could not be shown... base class 'System.Void' cannot be designed`.
  - `Type 'System.Windows.Forms.Form' is not defined`.
  - Cascada de `Type ... is not defined` en `*.designer.vb`.

### Causa probable
- Estado inconsistente del cache interno de Visual Studio Designer/proyecto (no necesariamente error real del código fuente).

### Recuperacion validada
1. Guardar cambios.
2. Cerrar pestañas Designer.
3. Cerrar Visual Studio completamente.
4. Reabrir solución en Visual Studio.
5. Ejecutar `Clean Solution` + `Rebuild Solution`.
6. Abrir nuevamente el formulario en Designer.

### Prevencion
- Evitar abrir Designer inmediatamente despues de cambios grandes en `*.designer.vb`/`*.vbproj`.
- Hacer `Rebuild` antes de abrir Designer cuando se toquen formularios críticos (`LOGIN`, `FrmPrincipal`).
- Mantener `Inherits System.Windows.Forms.Form` estándar en `*.designer.vb` y no usar variaciones manuales.

## Alcance
Este modo aplica a:
- `SCSC/Seguridad/*`
- `SCSC/Formularios/*`
- `SCSC/Reportes/Parametros/*`
- Cualquier formulario WinForms agregado en adelante.
