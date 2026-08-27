Imports System.Data.SqlClient

Public Class ComedorDataService
    Public Structure MarcaComedorResultado
        Public TextoTiquetes As String
        Public ErrorTiquetes As Boolean
        Public RegistroGuardado As Boolean
    End Structure

    Private ReadOnly _cls As FuncionesDB

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        If cls Is Nothing Then
            _cls = New FuncionesDB()
        Else
            _cls = cls
        End If
    End Sub

    Public Function CargarUsuariosConMarcaTransporte(ByVal cn As SqlConnection, ByVal fechaServer As Date) As DataSet
        Const sql As String =
"SELECT
    U.IdUsuario,
    U.TipoBeca,
    U.CantidadTiquetes,
    U.HuellaDactilar,
    U.Nombre,
    U.PrimerApellido,
    U.SegundoApellido,
    U.CodTipo,
    U.Cedula,
    U.IdHorario,
    CASE WHEN MT.Fecha IS NULL THEN 0 ELSE 1 END AS MarcaTransporte,
    MT.Fecha AS HoraMarca
FROM Usuario U
LEFT JOIN (
    SELECT RT.IdUsuario, MIN(RT.Fecha) AS Fecha
    FROM RegistroTransporte RT
    WHERE RT.Fecha >= @FechaInicio
      AND RT.Fecha < @FechaFin
    GROUP BY RT.IdUsuario
) MT ON MT.IdUsuario = U.IdUsuario
WHERE U.Activo = 1;"

        Dim dsUsuarios As New DataSet()

        ' La relacion con transporte se resuelve en SQL para evitar un Select() por usuario en memoria.
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@FechaInicio", System.Data.SqlDbType.DateTime).Value = fechaServer.Date
            cmd.Parameters.Add("@FechaFin", System.Data.SqlDbType.DateTime).Value = fechaServer.Date.AddDays(1)

            Using da As New SqlDataAdapter(cmd)
                da.Fill(dsUsuarios, "Usuario")
            End Using
        End Using

        Return dsUsuarios
    End Function

    Public Function CargarBecas(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Becas", "Select IdBeca,DiasBeca From TipoBeca", Cn:=cn)
    End Function

    Public Function CargarHorarios(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Horarios", "Select IdHorario,HoraLimite From Horario", Cn:=cn)
    End Function

    Public Function CargarAsistenciasComedorDia(ByVal cn As SqlConnection, ByVal fechaBase As Date) As HashSet(Of Integer)
        Const sql As String =
"SELECT DISTINCT RC.IdUsuario
FROM RegistroComedor RC
INNER JOIN Usuario U ON U.IdUsuario = RC.IdUsuario
WHERE RC.TipoPago = 2
  AND U.CodTipo = 1
  AND RC.Fecha >= @FechaInicio
  AND RC.Fecha < @FechaFin;"

        Dim asistencias As New HashSet(Of Integer)()

        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@FechaInicio", SqlDbType.DateTime).Value = fechaBase.Date
            cmd.Parameters.Add("@FechaFin", SqlDbType.DateTime).Value = fechaBase.Date.AddDays(1)

            Using rd As SqlDataReader = cmd.ExecuteReader()
                While rd.Read()
                    asistencias.Add(CInt(rd("IdUsuario")))
                End While
            End Using
        End Using

        Return asistencias
    End Function

    Public Function RegistrarMarca(ByVal idUsuario As Integer,
                                   ByVal codTipo As Integer,
                                   ByVal esBecado As Boolean,
                                   ByVal cn As SqlConnection) As MarcaComedorResultado
        Dim pTransac As SqlTransaction = Nothing
        Dim resultado As MarcaComedorResultado

        Try
            Dim guardarTransaccion As Boolean = True
            Dim cantTiquetes As Integer = 0
            Dim valores() As FuncionesDB.Campos

            _cls.IniciaSQL(cn, pTransac)
            valores = _cls.InicializarArray
            _cls.ArmaValor(valores, "IdUsuario", idUsuario)

            If esBecado Then
                resultado.TextoTiquetes = " Usuario Becado"
                _cls.ArmaValor(valores, "Beca", 1)
            Else
                ' El rebajo se hace en un solo UPDATE para evitar carreras entre lectura de saldo y descuento.
                If Not TryDescontarTiquete(idUsuario, cn, pTransac, cantTiquetes) Then
                    guardarTransaccion = False
                    resultado.ErrorTiquetes = True
                Else
                    resultado.TextoTiquetes = cantTiquetes & " Tiquetes"
                End If
                If String.IsNullOrWhiteSpace(resultado.TextoTiquetes) Then
                    resultado.TextoTiquetes = "0 Tiquetes"
                End If
                _cls.ArmaValor(valores, "Beca", 0)
            End If

            _cls.ArmaValor(valores, "TipoPago", 2)
            _cls.ArmaValor(valores, "Cantidad", 1)
            _cls.ArmaValor(valores, "TipoUsuario", codTipo)
            If guardarTransaccion Then
                _cls.Insert("RegistroComedor", valores, cn, pTransac)
                resultado.RegistroGuardado = True
            End If

            _cls.FinalSQL(pTransac)
            Return resultado
        Catch
            If Not pTransac Is Nothing Then
                Try
                    _cls.RollSQL(pTransac)
                Catch
                    ' omitir error secundario de rollback para conservar comportamiento actual
                End Try
            End If
            Throw
        End Try
    End Function

    Private Function TryDescontarTiquete(ByVal idUsuario As Integer,
                                         ByVal cn As SqlConnection,
                                         ByVal transac As SqlTransaction,
                                         ByRef cantidadRestante As Integer) As Boolean
        Const sql As String =
"UPDATE Usuario
SET CantidadTiquetes = CantidadTiquetes - 1
OUTPUT INSERTED.CantidadTiquetes
WHERE IdUsuario = @IdUsuario
  AND CantidadTiquetes > 0;"

        Using cmd As New SqlCommand(sql, cn, transac)
            cmd.Parameters.Add("@IdUsuario", System.Data.SqlDbType.Int).Value = idUsuario
            Dim raw As Object = cmd.ExecuteScalar()
            If raw Is Nothing OrElse raw Is DBNull.Value Then
                cantidadRestante = 0
                Return False
            End If

            cantidadRestante = CInt(raw)
            Return True
        End Using
    End Function
End Class
