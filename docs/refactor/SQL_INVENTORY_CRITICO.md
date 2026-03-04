# Inventario SQL Crítico (2026-03-04)

## Alcance
- `SCSC/Formularios/ControlTransporte.vb`
- `SCSC/Formularios/ControlComedor.vb`
- `SCSC/Formularios/FrmImportarExcel.vb`

## 1) ControlTransporte

Consultas detectadas:
- Carga usuarios activos:
  - `SELECT IdUsuario,HuellaDactilar,Nombre,PrimerApellido,SegundoApellido,CodTipo,IdRuta,Seccion,Cedula,IdHorario,PermisoSalida FROM Usuario WHERE Activo = 1`
- Carga rutas:
  - `SELECT * FROM Ruta`
- Validación marca docente del día (con concatenación):
  - `Select IdTransaccion From RegistroDocentes Where IdUsuario = <valor> and <filtro_fecha>`

Operaciones de escritura:
- `Insert("RegistroTransporte", ...)`
- `Insert("RegistroDocentes", ...)`

Riesgo:
- Medio: la consulta de validación docente concatena `IdUsuario` y fecha en string SQL.

## 2) ControlComedor

Consultas detectadas:
- Carga usuarios activos:
  - `SELECT IdUsuario,TipoBeca,HuellaDactilar,Nombre,PrimerApellido,SegundoApellido,CodTipo,Cedula,IdHorario FROM Usuario WHERE Activo = 1`
- Carga marcas de transporte del día (filtro fecha en texto):
  - `SELECT IdUsuario,Fecha FROM RegistroTransporte WHERE <filtro_fecha>`
- Carga becas:
  - `Select IdBeca,DiasBeca From TipoBeca`
- Carga horarios:
  - `Select IdHorario,HoraLimite From Horario`
- Consulta de tiquetes por usuario (parametrizada):
  - `SELECT CantidadTiquetes FROM Usuario WHERE IdUsuario = @IdUsuario`

Operaciones de escritura:
- Update de tiquetes:
  - `UPDATE Usuario set CantidadTiquetes = CantidadTiquetes - 1 WHERE IdUsuario = @IdUsuario`
- `Insert("RegistroComedor", ...)`

Riesgo:
- Medio: filtro de fecha construido por helper string, no por parámetro de rango.

## 3) FrmImportarExcel

Lectura de Excel:
- `Select * from [Lista$]` vía `OleDbDataAdapter`.

Operaciones SQL detectadas:
- `Update Usuario set Actualizado = 0 Where Codtipo = <tipo> and IdHorario = <horario>`
- `Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = <tipo> and IdHorario = <horario>`
- `GuardarActualizar("Usuario", ...)` por cada fila importada.

Riesgo:
- Medio-Alto: updates con concatenación y dependencia de `CBHorario.SelectedIndex`.

## Prioridad de migración a servicios (Sprint 1)
1. `ControlTransporte`: extraer carga inicial + registro de marca.
2. `ControlComedor`: extraer validaciones de beca/tiquetes + registro transaccional.
3. `FrmImportarExcel`: encapsular importación y updates de estado en servicio transaccional.

## Criterio de aceptación técnico para mover cada consulta
- La firma del servicio recibe parámetros tipados.
- La consulta queda encapsulada fuera del formulario.
- La ejecución mantiene el mismo resultado funcional (baseline manual).
- Errores se propagan con mensajes consistentes (sin `MsgBox` dentro del servicio).
