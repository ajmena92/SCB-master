# Backlog de Deuda Tecnica Pendiente

Fecha: 2026-03-24

## Estado actual
- Se cerraron las deudas operativas mas urgentes en marcas:
  - reloj servidor;
  - foco del lector;
  - apertura limpia de formularios;
  - DDL fuera del runtime;
  - SQL tipado en servicios criticos;
  - snapshots tipados en el hot path;
  - dialogo operativo para incidencia rapida;
  - meta dinamica y asistencia unica diaria en comedor.
- Lo pendiente ya no es hotfix operativo inmediato; queda como trabajo de mantenimiento controlado.

## P1
### 1. Reducir dependencia de `FuncionesDB`
- Evidencia: `escritorio/SCSC/Clases/FunccionesDB.vb`
- Riesgo: sigue siendo la mayor clase legacy transversal para consultas, datasets, conversiones y utilidades mezcladas.
- Alcance sugerido:
  - congelar nuevas llamadas directas desde formularios;
  - seguir moviendo consultas tocadas a servicios especificos;
  - ir reemplazando metodos criticos por capas mas tipadas.

### 2. Unificar timestamps del logger con `ServerClock`
- Evidencia: `escritorio/SCSC/Clases/ErrorLogger.vb`
- Riesgo: los logs siguen fechados con hora local del equipo, mientras la operacion ya corre con hora del servidor.
- Alcance sugerido:
  - cambiar el timestamp base del logger a `ServerClock.Now()` cuando exista sincronizacion;
  - mantener fallback local solo si aun no hay hora servidor disponible.

### 3. Formalizar baseline de build y smoke Windows
- Evidencia: no hay pipeline ni proyecto de pruebas automatizadas para WinForms/.NET Framework.
- Riesgo: seguimos dependiendo de validacion manual despues de cambios funcionales y visuales.
- Alcance sugerido:
  - registrar build valido por fecha;
  - documentar humo minimo reproducible;
  - fijar checklist de login, comedor, transporte, recargas, importacion y reportes.

## P2
### 4. Completar saneamiento de `Catch` defensivos
- Evidencia:
  - `escritorio/SCSC/FrmPrincipal.vb`
  - `escritorio/SCSC/Clases/Servicios/ParametroSistemaService.vb`
  - otros modulos secundarios legacy
- Riesgo: aun existen capturas defensivas que silencian comportamiento o devuelven defaults amplios.
- Alcance sugerido:
  - revisar `Catch` que todavia no registran contexto suficiente;
  - distinguir entre fallback intencional y error real.

### 5. Completar parametrizacion SQL y tipado en modulos secundarios
- Evidencia:
  - `escritorio/SCSC/Clases/FunccionesDB.vb`
  - formularios no operativos que aun usan SQL legacy
- Riesgo: planes de ejecucion menos predecibles y consultas mas fragiles fuera del hot path ya saneado.
- Alcance sugerido:
  - priorizar formularios aun activos en operacion;
  - convertir por lotes consultas nuevas o tocadas a `SqlCommand` parametrizado.

### 6. Normalizar aperturas por instancia nueva fuera de marcas
- Evidencia: en el proyecto aun hay formularios secundarios con patrones legacy de instancia por defecto.
- Riesgo: posibles arrastres de estado al reabrir modulos menos usados.
- Alcance sugerido:
  - revisar aperturas desde menu principal y shell;
  - migrar por lotes solo los formularios con estado mutable relevante.

### 7. Reducir conversiones legacy en modulos secundarios
- Evidencia: persisten `CDate`, `CInt`, `Val`, `DataSet` y parseos amplios fuera de comedor/transporte/login.
- Riesgo: fragilidad ante datos inesperados y mas costo de mantenimiento.
- Alcance sugerido:
  - atacar primero formularios de recarga, importacion y reportes auxiliares;
  - preferir snapshots o DTOs cuando se toque codigo existente.

## P3
### 8. Consolidar resolucion de recursos y rutas
- Evidencia: el proyecto aun resuelve rutas/recursos en varios puntos.
- Riesgo: fallos dependientes del directorio de ejecucion o del despliegue.
- Alcance sugerido:
  - unificar helpers para recursos locales, logos, sonidos y archivos auxiliares;
  - priorizar `My.Resources` donde aplique.

### 9. Mejorar cobertura de validacion visual
- Evidencia: no existe automatizacion para layout/DPI y la verificacion sigue siendo manual.
- Riesgo: regresiones visuales tardias en resoluciones o escalados distintos.
- Alcance sugerido:
  - documentar matriz minima por resolucion;
  - capturar screenshots de referencia por modulo operativo.

## No incluido en esta etapa
- Refactorizacion mayor de formularios monoliticos.
- Reescritura integral de `FuncionesDB`.
- Introduccion de una suite automatizada completa para UI WinForms.

## Orden sugerido
1. `FuncionesDB` por fases y sin cambios masivos.
2. `ErrorLogger` alineado a `ServerClock`.
3. Baseline de build y smoke Windows.
4. Saneamiento progresivo de `Catch` y SQL legacy en modulos secundarios.
