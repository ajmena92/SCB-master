Imports System.Data.SqlClient

Public Class TransporteDataService
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
        Return _cls.ConsultarTSQL("Ruta", "SELECT * FROM Ruta", Cn:=cn)
    End Function

    Public Sub RegistrarMarca(ByVal usuario As DataRow, ByVal cn As SqlConnection, ByVal fechaServer As Date)
        Dim valores() As FuncionesDB.Campos = _cls.InicializarArray
        Dim idUsuario As Integer = CInt(usuario("IdUsuario"))
        Dim idHorario As Integer = CInt(usuario("IdHorario"))
        _cls.ArmaValor(valores, "IdUsuario", idUsuario)
        _cls.ArmaValor(valores, "IdHorario", idHorario)

        If CInt(usuario("CodTipo")) = 1 Then
            _cls.ArmaValor(valores, "IdRuta", CInt(usuario("IdRuta")))
            _cls.Insert("RegistroTransporte", valores, cn)
            Exit Sub
        End If

        Dim ds As DataSet = _cls.ConsultarTSQL("RegistroTransporte",
                                               "Select IdTransaccion From RegistroDocentes Where IdUsuario = " & idUsuario & " and " & ArmaFechaQueryHora("Fecha", fechaServer, fechaServer),
                                               Cn:=cn)
        If ds.Tables(0).Rows.Count > 0 Then
            _cls.ArmaValor(valores, "TipoMarca", 2)
        Else
            _cls.ArmaValor(valores, "TipoMarca", 1)
        End If
        _cls.Insert("RegistroDocentes", valores, cn)
    End Sub

    Public Sub RegistrarMarcaEnTransaccion(ByVal usuario As DataRow,
                                           ByVal cn As SqlConnection,
                                           ByVal fechaServer As Date,
                                           ByVal tx As SqlTransaction)
        Dim idUsuario As Integer = CInt(usuario("IdUsuario"))
        Dim idHorario As Integer = CInt(usuario("IdHorario"))
        Dim codTipo As Short = CShort(usuario("CodTipo"))
        If codTipo = 1S Then
            Using cmd As New SqlCommand("INSERT INTO RegistroTransporte (IdUsuario, IdHorario, IdRuta) VALUES (@IdUsuario, @IdHorario, @IdRuta);", cn, tx)
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario)
                cmd.Parameters.AddWithValue("@IdHorario", idHorario)
                cmd.Parameters.AddWithValue("@IdRuta", CInt(usuario("IdRuta")))
                cmd.ExecuteNonQuery()
            End Using
            Exit Sub
        End If

        Dim existeHoy As Boolean = False
        Using cmdCheck As New SqlCommand("SELECT TOP 1 IdTransaccion FROM RegistroDocentes WHERE IdUsuario = @IdUsuario AND CAST(Fecha AS date) = CAST(@Fecha AS date);", cn, tx)
            cmdCheck.Parameters.AddWithValue("@IdUsuario", idUsuario)
            cmdCheck.Parameters.AddWithValue("@Fecha", fechaServer)
            Using reader As SqlDataReader = cmdCheck.ExecuteReader()
                existeHoy = reader.Read()
            End Using
        End Using

        Using cmd As New SqlCommand("INSERT INTO RegistroDocentes (IdUsuario, IdHorario, TipoMarca) VALUES (@IdUsuario, @IdHorario, @TipoMarca);", cn, tx)
            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario)
            cmd.Parameters.AddWithValue("@IdHorario", idHorario)
            cmd.Parameters.AddWithValue("@TipoMarca", If(existeHoy, 2, 1))
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
