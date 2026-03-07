# Validación UI - FrmSeguridadRBAC (2026-03-04)

## Alcance
- Formulario: `SCSC/Formularios/FrmSeguridadRBAC.vb`
- Designer: `SCSC/Formularios/FrmSeguridadRBAC.Designer.vb`
- Estándares aplicados:
  - `docs/refactor/UI_HYBRID_STANDARD_2026.md`
  - `docs/refactor/DESIGNER_FIRST_GUIDE.md`

## Verificación de cumplimiento técnico

### 1) Designer-first
- Estructura base declarada en `*.Designer.vb`.
- Clase parcial y herencia `Form` correctas.
- Sin dependencia de controles Bunifu en el formulario RBAC.

### 2) Runtime-theme safe
- Se aplica `UIThemeManagerV2.Apply(Me, "dialogo")` en `Load`.
- Estilo visual adicional encapsulado en `ApplyVisualStandard2026()`.
- No se crean controles estructurales en runtime.
- Se desactivaron ajustes estructurales en eventos `Load/Shown/Resize/SelectedIndexChanged` para evitar superposición de grilla/fila 0 y desalineación de botones.

### 3) Paleta/Tipografía
- Uso de `UIConstants` para colores y fuentes.
- Botones de acciones destructivas (`Eliminar`/`Revocar`) con color `Danger`.

### 4) Accesibilidad e interacción
- `Esc` cierra el formulario.
- `Enter` ejecuta acción primaria contextual por pestaña.
- Estado de CTA dinámico (`Enabled/Disabled`) según selección y datos válidos.

### 5) Layout responsive
- `AutoScaleMode = Dpi`.
- `MinimumSize` definido en formulario.
- Estructura por `Dock` + `TableLayoutPanel` + `FlowLayoutPanel`.
- Grillas con altura fija y panel inferior en `Dock.Fill`.

## Evidencia de pruebas

### Estado en este entorno (CLI)
- Validación estática de código: **completa**.
- Validación visual en Designer (VS): **pendiente de confirmar en entorno Windows con Visual Studio**.
- Build local con `msbuild`: **ejecutado y exitoso** (`Debug | Any CPU`, `0 Error(s)`).

### Checklist para cierre en VS
- [ ] Abrir `FrmSeguridadRBAC` en Designer sin error.
- [ ] Verificar visibilidad de botones CRUD en las 3 pestañas.
- [ ] Ejecutar app y validar:
  - [ ] Crear/Actualizar/Eliminar Usuario.
  - [ ] Crear/Actualizar/Eliminar Rol.
  - [ ] Crear/Actualizar/Eliminar Permiso.
  - [ ] Asignar/Revocar Rol a Usuario.
  - [ ] Asignar/Revocar Permiso a Rol.
- [ ] Validar `Esc` y `Enter`.
- [ ] Probar 100% y 125% DPI.

## Notas
- Este documento deja trazabilidad de cumplimiento técnico del estándar híbrido.
- Al completar el checklist en Visual Studio, se considera cierre de DoD UI para este formulario.
