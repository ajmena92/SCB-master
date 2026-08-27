Imports System.Data.SqlClient

Public Class TransporteDataService
    Public Structure RegistroMarcaResultado
        Public EsPrimeraMarcaEstudianteDelDia As Boolean
    End Structure

    Private ReadOnly _cls As FuncionesDB

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        If cls Is Nothing Then
            _cls = New FuncionesDB()
        Else
            _cls = cls
        End If
    End Sub

    Public Function CargarUsuariosActivos(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Usuario",
                                  "SELECT IdUsuario,HuellaDactilar,Nombre,PrimerApellido,SegundoApellido,CodTipo,IdRuta,Seccion,Cedula,IdHorario,PermisoSalida FROM Usuario WHERE Activo = 1",
                                  Cn:=cn)
    End Function

    Public Function CargarRutas(ByVal cn As SqlConnection) As DataSet
        Return _cls.ConsultarTSQL("Ruta", "SELECT IdRuta, Codigo, Descripcion FROM Ruta", Cn:=cn)
    End Function

    Public Function RegistrarMarcaEnTransaccion(ByVal idUsuario As Integer,
                                                ByVal idHorario As Integer,
                                                ByVal codTipo As Short,
                                                ByVal idRuta As Integer,
                                                ByVal cn As SqlConnection,
                                                ByVal fechaServer As Date,
                                                ByVal tx As SqlTransaction) As RegistroMarcaResultado
        Dim resultado As New RegistroMarcaResultado()
        Dim fechaInicioDia As DateTime = fechaServer.Date
        Dim fechaFinDia As DateTime = fechaInicioDia.AddDays(1)
        If codTipo = 1S Then
            ' La primera marca del estudiante define entrada; las siguientes quedan disponibles para la regla de salida.
            Using cmdCheck As New SqlCommand("SELECT COUNT(1) FROM RegistroTransporte WHERE IdUsuario = @IdUsuario AND Fecha >= @FechaInicioDia AND Fecha < @FechaFinDia;", cn, tx)
                cmdCheck.Parameters.Add("@IdUsuario", System.Data.SqlDbType.Int).Value = idUsuario
                cmdCheck.Parameters.Add("@FechaInicioDia", System.Data.SqlDbType.DateTime).Value = fechaInicioDia
                cmdCheck.Parameters.Add("@FechaFinDia", System.Data.SqlDbType.DateTime).Value = fechaFinDia
                resultado.EsPrimeraMarcaEstudianteDelDia = CInt(cmdCheck.ExecuteScalar()) = 0
            End Using

            Using cmd As New SqlCommand("INSERT INTO RegistroTransporte (IdUsuario, IdHorario, IdRuta) VALUES (@IdUsuario, @IdHorario, @IdRuta);", cn, tx)
                cmd.Parameters.Add("@IdUsuario", System.Data.SqlDbType.Int).Value = idUsuario
                cmd.Parameters.Add("@IdHorario", System.Data.SqlDbType.Int).Value = idHorario
                cmd.Parameters.Add("@IdRuta", System.Data.SqlDbType.Int).Value = idRuta
                cmd.ExecuteNonQuery()
            End Using
            Return resultado
        End If

        Dim existeHoy As Boolean = False
        Using cmdCheck As New SqlCommand("SELECT TOP 1 IdTransaccion FROM RegistroDocentes WHERE IdUsuario = @IdUsuario AND Fecha >= @FechaInicioDia AND Fecha < @FechaFinDia;", cn, tx)
            cmdCheck.Parameters.Add("@IdUsuario", System.Data.SqlDbType.Int).Value = idUsuario
            cmdCheck.Parameters.Add("@FechaInicioDia", System.Data.SqlDbType.DateTime).Value = fechaInicioDia
            cmdCheck.Parameters.Add("@FechaFinDia", System.Data.SqlDbType.DateTime).Value = fechaFinDia
            Using reader As SqlDataReader = cmdCheck.ExecuteReader()
                existeHoy = reader.Read()
            End Using
        End Using

        Using cmd As New SqlCommand("INSERT INTO RegistroDocentes (IdUsuario, IdHorario, TipoMarca) VALUES (@IdUsuario, @IdHorario, @TipoMarca);", cn, tx)
            cmd.Parameters.Add("@IdUsuario", System.Data.SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@IdHorario", System.Data.SqlDbType.Int).Value = idHorario
            cmd.Parameters.Add("@TipoMarca", System.Data.SqlDbType.Int).Value = If(existeHoy, 2, 1)
            cmd.ExecuteNonQuery()
        End Using
        Return resultado
    End Function
End Class
