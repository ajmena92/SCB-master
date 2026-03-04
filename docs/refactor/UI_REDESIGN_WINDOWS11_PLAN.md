# Plan de Rediseño UI Windows 11

## Objetivo
Modernizar la interfaz completa para que se perciba nativa de Windows 11 y reducir dependencia de componentes legacy que degradan rendimiento o mantenimiento.

## Documento rector vigente
- Estandar visual y tecnico oficial: `docs/refactor/UI_HYBRID_STANDARD_2026.md`.
- Prototipo canonico implementado: `SCSC/Seguridad/LOGIN.vb` + `SCSC/Seguridad/LOGIN.designer.vb`.

## Estado actual
- UI legacy desacoplada de binarios `Bunifu.UI.WinForms.*` mediante compatibilidad interna.
- `*.Designer.vb` aún conservan tipos `Bunifu.Framework.UI` (resueltos por shim local temporal).
- Reportería depende de Crystal Reports (se mantiene por compatibilidad funcional).

## Estrategia recomendada

## Fase A (ya iniciada)
- Tema global runtime (`UIThemeManager`) aplicado a formularios principales.
- Ajustes visuales base: tipografía Segoe UI, superficies claras, menú moderno, tablas estilizadas.
- Mejora de render: doble buffer en formularios y grillas.
- Baseline visual cerrado en Login bajo enfoque `Designer-first + runtime-theme safe`.

## Fase B (migración de controles legacy)
- Reemplazar controles `Bunifu.Framework.UI` por controles WinForms nativos en:
  1. `LOGIN`
  2. `FrmPrincipal`
  3. `ControlTransporte`
  4. `ControlComedor`
- Mantener identidad visual con paleta y componentes estándar.

## Fase C (limpieza de dependencias)
- Completada parcialmente:
  - Referencias `Bunifu.*` removidas del `.vbproj`.
  - Paquete NuGet `Bunifu.UI.WinForms` removido.
- Pendiente:
  - Eliminar uso de tipos `Bunifu.Framework.UI` en diseñadores.
  - Retirar `BunifuLegacyCompat.vb` al finalizar la migración visual total.

## Fase D (optimización de experiencia)
- Reorganizar layouts para DPI alto y escalado 125%-150%.
- Unificar espaciados, tamaños y jerarquía visual.
- Mejorar navegación de teclado y estados de foco.

## Métricas de éxito
- Tiempo de arranque percibido menor.
- Menor consumo de memoria al abrir módulos críticos.
- Cero referencias a controles Bunifu en formularios migrados.
- Consistencia visual entre login, dashboard y módulos operativos.

## Riesgos
- Cambios directos en archivos Designer pueden generar regresiones de layout.
- Dependencias indirectas de recursos `.resx` al retirar controles third-party.
- Crystal Reports no debe tocarse en esta fase.

## Orden de ejecución recomendado
1. `LOGIN` (alto impacto visual, bajo riesgo funcional).
2. `FrmPrincipal` (navegación global).
3. `ControlTransporte`.
4. `ControlComedor`.
5. Limpieza final de referencias y paquetes.
