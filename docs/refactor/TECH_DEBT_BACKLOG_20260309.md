# Backlog de Deuda Tecnica Pendiente

Fecha: 2026-03-09

## P0
### 1. Sacar secretos y credenciales del repositorio
- Estado: mitigado parcialmente en esta revision.
- Evidencia: `SCSC/app.config`, `SCSC/My Project/Settings.settings`, `SCSC/My Project/Settings.Designer.vb`
- Riesgo remanente: despliegues sin variables de entorno y credenciales historicas ya expuestas fuera de git.
- Accion recomendada:
  - rotar credenciales ya expuestas;
  - definir variables de entorno en cada ambiente;
  - validar arranque real despues del cambio.

### 2. Rehabilitar warnings del compilador
- Evidencia: `SCSC/SCSC_Marcas.vbproj`
- Riesgo: conversiones peligrosas, late binding y warnings reales ocultos en CI/build local.
- Accion recomendada:
  - subir `WarningLevel`;
  - corregir warnings por lotes empezando por `Clases` y `Servicios`;
  - usar build Windows como gate de integracion.

## P1
### 3. Reducir dependencia de `FuncionesDB`
- Evidencia: `SCSC/Clases/FunccionesDB.vb`
- Riesgo: clase dios para CRUD, transacciones, configuracion y consultas dinamicas.
- Accion recomendada:
  - congelar nuevas llamadas directas desde formularios;
  - crear repositorios/servicios especificos por modulo;
  - dejar `FuncionesDB` solo como compatibilidad temporal.

### 4. Romper formularios monoliticos
- Evidencia:
  - `SCSC/Formularios/ControlComedor.vb`
  - `SCSC/Formularios/ControlTransporte.vb`
  - `SCSC/Formularios/FrmImportarExcel.vb`
  - `SCSC/Formularios/FrmSeguridadRBAC.vb`
  - `SCSC/Clases/UIShellHost.vb`
- Riesgo: regresiones frecuentes, baja legibilidad y pruebas muy costosas.
- Accion recomendada:
  - extraer componentes de UI;
  - mover reglas a servicios;
  - separar persistencia de feedback visual.

### 5. Eliminar estado global compartido
- Evidencia:
  - `SCSC/Clases/VariablesGlobales.vb`
  - `SCSC/Clases/CodigoGeneral.vb`
  - usos de `gSession` en formularios y reportes
- Riesgo: efectos secundarios entre pantallas, trazabilidad baja y acoplamiento fuerte.
- Accion recomendada:
  - introducir objetos de contexto por flujo;
  - pasar parametros explicitamente a reportes y busquedas.

### 6. Completar migracion a consultas parametrizadas
- Evidencia:
  - `SCSC/Formularios/FrmRecargas.vb`
  - `SCSC/Clases/FunccionesDB.vb`
  - llamadas a `ConsultarTSQL` con SQL texto desde formularios
- Riesgo: SQL fragil, mas dificil de revisar y de testear.
- Accion recomendada:
  - centralizar SQL en servicios;
  - usar `SqlCommand` parametrizado en rutas nuevas y al tocar legacy.

## P2
### 7. Formalizar estrategia de pruebas
- Evidencia: no existe proyecto de tests automatizados.
- Riesgo: cada refactor depende de humo manual y conocimiento tribal.
- Accion recomendada:
  - documentar checklist reproducible por modulo;
  - agregar pruebas a helpers y servicios puros donde ya existe aislamiento.

### 8. Reducir deuda de Crystal Reports
- Evidencia: wrappers `Rpt/*.vb` con `Option Strict Off`.
- Riesgo: zona fuera de los estandares de tipado y dificil de modernizar.
- Accion recomendada:
  - aislar wrappers autogenerados;
  - documentar que no se editan manualmente;
  - mover preparacion de parametros fuera del viewer.

### 9. Consolidar acceso a recursos visuales y archivos
- Evidencia:
  - `SCSC/Seguridad/LOGIN.vb`
  - `SCSC/Clases/UIShellHost.vb`
  - `SCSC/Formularios/ControlComedor.vb`
- Riesgo: resolucion duplicada de rutas y fallos dependientes del directorio de ejecucion.
- Accion recomendada:
  - crear un helper unico para ubicacion de recursos;
  - priorizar `My.Resources` cuando sea viable.

### 10. Documentar baseline de build y smoke operativo
- Evidencia: la documentacion historica mezcla estados de distintas sesiones.
- Riesgo: no hay una unica fuente de verdad para saber si un cambio esta listo.
- Accion recomendada:
  - registrar build Windows valido por fecha;
  - fijar humo minimo por login, comedor, transporte, estudiantes, importacion y reportes.

## Orden sugerido de ejecucion
1. Configuracion y credenciales.
2. Warning level y baseline de build.
3. Extraccion gradual desde `FuncionesDB`.
4. Reduccion de globals.
5. Particion de formularios grandes.
6. Pruebas y smoke automatizable.
