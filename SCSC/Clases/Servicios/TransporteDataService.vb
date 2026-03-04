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
        _cls.ArmaValor(valores, "IdUsuario", usuario!IdUsuario)
        _cls.ArmaValor(valores, "IdHorario", usuario!IdHorario)

        If (usuario!CodTipo = 1) Then
            _cls.ArmaValor(valores, "IdRuta", usuario!IdRuta)
            _cls.Insert("RegistroTransporte", valores, cn)
            Exit Sub
        End If

        Dim ds As DataSet = _cls.ConsultarTSQL("RegistroTransporte",
                                               "Select IdTransaccion From RegistroDocentes Where IdUsuario = " & usuario!IdUsuario & " and " & ArmaFechaQueryHora("Fecha", fechaServer, fechaServer),
                                               Cn:=cn)
        If ds.Tables(0).Rows.Count > 0 Then
            _cls.ArmaValor(valores, "TipoMarca", 2)
        Else
            _cls.ArmaValor(valores, "TipoMarca", 1)
        End If
        _cls.Insert("RegistroDocentes", valores, cn)
    End Sub
End Class
