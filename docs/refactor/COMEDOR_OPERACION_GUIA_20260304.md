# Guia Operativa - Control Comedor (Mejoras UX y Supervisor)

## Atajos de teclado
- `Enter`: procesa lectura del carnet.
- `Esc`: salir del formulario.
- `F2`: limpiar ultimo registro manualmente.
- `F3`: reenfocar el campo de escaneo.
- `F4`: mostrar/ocultar historial de ultimos eventos.
- `F6`: alternar vista supervisor en caliente.

## Politica visual operativa
- Semaforo moderno:
  - `ACCESO PERMITIDO` en verde.
  - `PERMITIDO CON ADVERTENCIA` en ambar.
  - `ACCESO DENEGADO` / errores en rojo.
- Mitigaciones obligatorias para no depender solo del color:
  - mensaje principal,
  - icono de estado,
  - banner de resultado,
  - sonido.
- El ultimo estado se mantiene visible hasta nuevo escaneo o limpieza manual (`F2`).
- Se muestra `Ultima lectura` y `Estado visible: hace ...` para evitar ambiguedad temporal.

## Configuracion en `app.config`
- `SonidosHabilitados`: activa/desactiva sonidos.
- `SonidoOkRuta`: ruta de WAV para `EXITO`.
- `SonidoWarnRuta`: ruta de WAV para advertencias.
- `SonidoErrorRuta`: ruta de WAV para errores.
- `DuracionEstadoExitoSegundos`, `DuracionEstadoAdvertenciaSegundos`, `DuracionEstadoErrorSegundos`:
  - se mantienen por compatibilidad, pero no controlan limpieza automatica en la politica actual.
- `ColorExitoHex`, `ColorAdvertenciaHex`, `ColorErrorHex`, `ColorNeutroHex`, `ColorProcesandoHex`:
  - personalizan paleta visual del semaforo moderno.
- `RepeticionesSonidoOk`: cantidad de repeticiones de sonido en exito.
- `RepeticionesSonidoWarn`: cantidad de repeticiones de sonido en advertencias.
- `RepeticionesSonidoError`: cantidad de repeticiones de sonido en errores.
- `IntervaloRepeticionSonidoMs`: espera en milisegundos entre repeticiones.
- `ForzarSonidoSistemaFallback`: ignora WAV y usa `SystemSounds` siempre.
- `ModoAccesible`: tipografia y contraste reforzados.
- `MostrarVistaSupervisor`: muestra KPI, antifila, alertas y recomendacion.
- `MetaDiariaComedor`: meta de registros exitosos.
- `UmbralDuplicadosPct`: umbral para alerta de duplicados.
- `UmbralErrores`: umbral para alerta de errores.

## Configuracion en tabla `Parametro`
- `PermitirSinMarcaTransporte` (`BIT`):
  - `1` permite acceso con advertencia si no hay marca.
  - `0` deniega por regla operativa.

## Persistencia operativa
- Tabla: `dbo.OperacionComedorEvento`.
- Script: `docs/refactor/sql/20260304_Comedor_Operacion_Eventos.sql`.
- Se registra:
  - estado de lectura (codigo en espanol tecnico),
  - motivo/codigo (espanol tecnico),
  - tiempo de atencion,
  - duplicado/advertencia/error,
  - incidencias manuales.

## Incidencia rapida
- Boton: `Incidencia Rapida`.
- Solicita motivo corto y registra evento manual en historial y base de datos.

## Checklist de pruebas manuales
1. Escaneo valido: estado verde, historial actualizado, evento persistido.
2. Escaneo duplicado: advertencia, contador duplicados y evento persistido.
3. Sin tiquetes: error y alerta operacional.
4. Inactividad 60s: limpieza de ultimo registro.
5. `F2/F3/F4/F6`: comportamiento esperado sin romper el escaneo.
6. `MostrarVistaSupervisor=true`: se ven KPI, antifila, alertas y recomendacion.
7. `MostrarVistaSupervisor=false`: se ocultan paneles supervisor.
8. Simular DB offline: indicador offline y reconexion automatica (heartbeat).
9. Sonidos con rutas WAV y fallback a `SystemSounds` (y repeticiones configuradas).
10. Resolucion 1366x768: controles visibles y usables.
