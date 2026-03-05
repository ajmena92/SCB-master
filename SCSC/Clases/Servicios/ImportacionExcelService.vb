Imports System.Data.SqlClient

Public Class ImportacionExcelService
    Private ReadOnly _cls As FuncionesDB

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        If cls Is Nothing Then
            _cls = New FuncionesDB()
        Else
            _cls = cls
        End If
    End Sub

    Public Function ObtenerHorariosActivos(ByVal cn As SqlConnection) As DataSet
        Dim valores() As FuncionesDB.Campos = _cls.InicializarArray
        Dim llave() As FuncionesDB.Campos = _cls.InicializarArray

        _cls.ArmaValor(valores, "IdHorario")
        _cls.ArmaValor(valores, "Descripcion")
        _cls.ArmaValor(valores, "Activo")
        _cls.ArmaValor(llave, "Activo", 1)

        Return _cls.Consultar("Horario", valores, llave, cn)
    End Function

    Public Sub MarcarUsuariosComoNoActualizados(ByVal cn As SqlConnection,
                                                ByVal pTransac As SqlTransaction,
                                                ByVal tipoUsuario As Integer,
                                                ByVal idHorario As Integer)
        _cls.AplicaSQL("Update Usuario set Actualizado = 0 Where Codtipo = " & tipoUsuario & " and IdHorario = " & idHorario, cn, pTransac)
    End Sub

    Public Sub DesactivarNoActualizados(ByVal cn As SqlConnection,
                                        ByVal pTransac As SqlTransaction,
                                        ByVal tipoUsuario As Integer,
                                        ByVal idHorario As Integer)
        _cls.AplicaSQL("Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = " & tipoUsuario & " and IdHorario = " & idHorario, cn, pTransac)
    End Sub

    Public Sub GuardarUsuarioDesdeFila(ByVal row As DataRow,
                                       ByVal tipoUsuario As Integer,
                                       ByVal idHorario As Integer,
                                       ByVal cn As SqlConnection,
                                       ByVal pTransac As SqlTransaction)
        Dim valores() As FuncionesDB.Campos = _cls.InicializarArray
        Dim llave() As FuncionesDB.Campos = _cls.InicializarArray

        Dim ced As String = Replace(CType(row(0), String), "-", "")
        _cls.ArmaValor(llave, "cedula", ced)
        _cls.ArmaValor(valores, "cedula", ced)
        _cls.ArmaValor(valores, "PrimerApellido", row(1))
        _cls.ArmaValor(valores, "SegundoApellido", row(2))
        _cls.ArmaValor(valores, "Nombre", row(3))
        _cls.ArmaValor(valores, "IdHorario", idHorario)
        _cls.ArmaValor(valores, "CodTipo", tipoUsuario)
        _cls.ArmaValor(valores, "Actualizado", 1)
        _cls.ArmaValor(valores, "Activo", 1)

        If tipoUsuario = 1 Then
            _cls.ArmaValor(valores, "Seccion", row(4))
            If row(5).ToString.Length() > 0 Then
                _cls.ArmaValor(valores, "Especialidad", row(5).ToString.ToUpper())
            Else
                _cls.ArmaValor(valores, "Especialidad", "III CICLO")
            End If

            Dim fechaNac As Date = Now.Date
            Try
                fechaNac = Convert.ToDateTime(row(6).ToString.Trim())
            Catch
            End Try
            _cls.ArmaValor(valores, "FechaNac", fechaNac)
            _cls.ArmaValor(valores, "Telefono", row(8))
        Else
            _cls.ArmaValor(valores, "Seccion", "NA")
            _cls.ArmaValor(valores, "Especialidad", "NA")
        End If

        _cls.GuardarActualizar("Usuario", valores, llave, cn, pTransac)
    End Sub

    Public Sub GuardarUsuarioNormalizado(ByVal row As DataRow,
                                         ByVal tipoUsuario As Integer,
                                         ByVal idHorario As Integer,
                                         ByVal cn As SqlConnection,
                                         ByVal pTransac As SqlTransaction)
        Dim valores() As FuncionesDB.Campos = _cls.InicializarArray
        Dim llave() As FuncionesDB.Campos = _cls.InicializarArray

        Dim ced As String = Convert.ToString(row("Cedula")).Replace("-", String.Empty).Trim()
        If String.IsNullOrWhiteSpace(ced) Then
            Throw New InvalidOperationException("La fila no contiene una cédula válida.")
        End If

        _cls.ArmaValor(llave, "cedula", ced)
        _cls.ArmaValor(valores, "cedula", ced)
        _cls.ArmaValor(valores, "PrimerApellido", Convert.ToString(row("PrimerApellido")).Trim())
        _cls.ArmaValor(valores, "SegundoApellido", Convert.ToString(row("SegundoApellido")).Trim())
        _cls.ArmaValor(valores, "Nombre", Convert.ToString(row("Nombre")).Trim())
        _cls.ArmaValor(valores, "IdHorario", idHorario)
        _cls.ArmaValor(valores, "CodTipo", tipoUsuario)
        _cls.ArmaValor(valores, "Actualizado", 1)
        _cls.ArmaValor(valores, "Activo", 1)

        If tipoUsuario = 1 Then
            _cls.ArmaValor(valores, "Seccion", Convert.ToString(row("Seccion")).Trim())

            Dim especialidad As String = Convert.ToString(row("Especialidad")).Trim()
            If especialidad.Length > 0 Then
                _cls.ArmaValor(valores, "Especialidad", especialidad.ToUpperInvariant())
            Else
                _cls.ArmaValor(valores, "Especialidad", "III CICLO")
            End If

            Dim fechaNac As Date = Now.Date
            Try
                fechaNac = Convert.ToDateTime(row("FechaNac"))
            Catch
            End Try
            _cls.ArmaValor(valores, "FechaNac", fechaNac)
            _cls.ArmaValor(valores, "Telefono", Convert.ToString(row("Telefono")).Trim())
        Else
            _cls.ArmaValor(valores, "Seccion", "NA")
            _cls.ArmaValor(valores, "Especialidad", "NA")
        End If

        _cls.GuardarActualizar("Usuario", valores, llave, cn, pTransac)
    End Sub
End Class
