Option Strict On
Option Explicit On

Imports MySql.Data.MySqlClient

Public Class FrmImportarDatos
    Dim CnPiad As New MySqlConnection
    Dim Cls As New FuncionesDB

    Sub LimpiarPantalla()
        LblEstado.Text = "Iniciar Proceso"
        CbCursoLectivo.SelectedIndex = -1
    End Sub

    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            If CnPiad.State = ConnectionState.Open Then
                Cls.CerrarConexion(CnPiad)
            End If
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
    End Sub

    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
            Cls.AbrirConexion(CnPiad, False)
            Dim Ds As New DataSet

            Dim Valores(), Llave() As FuncionesDB.Campos

            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray

            Cls.ArmaValor(Valores, "codigo")
            Cls.ArmaValor(Valores, "anno")
            Cls.ArmaValor(Llave, "1", 1) ' selecciona todos los registros. adicionar llave.

            Ds = Cls.Consultar("tcursolectivo", Valores, Llave, CnPiad, Orderby:="codigo desc")

            If Ds.Tables(0).Rows.Count > 0 Then
                CbCursoLectivo.Items.Clear()

                For I As Integer = 0 To Ds.Tables(0).Rows.Count - 1
                    CbCursoLectivo.Items.Add(New LBItem(CStr(Ds.Tables(0).Rows(I)("codigo")), "Año " & CStr(Ds.Tables(0).Rows(I)("anno"))))
                Next
            End If

            If CbCursoLectivo.Items.Count <> 0 Then
                CbCursoLectivo.SelectedIndex = -1
            End If
        Catch ex As Exception            
            If CnPiad.State = ConnectionState.Open Then
                Cls.CerrarConexion(CnPiad)
            End If
            MsgBox("Error al cargar el Formulario: Verifiqué conexión con servidor PIAD, " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose() 'Cierro el formulario
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
    End Sub

    Private Function Validaciion() As Boolean
        If CbCursoLectivo.SelectedIndex < 0 Then
            MsgBox("Ingrese el curso lectivo a importar", MsgBoxStyle.Critical)
            CbCursoLectivo.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validaciion() Then
            ''Conexiones a base de datos local
            Dim cn As New SqlClient.SqlConnection
            Dim pTransac As SqlClient.SqlTransaction = Nothing
            Dim Valores(), Llave() As FuncionesDB.Campos
            Dim Cmd As String
            Dim Ds As New DataSet
            Dim TipoUsuario As Integer
            Dim Ced As String = ""
            Try
                LblEstado.Text = "Consulta de Datos"
                Progreso.Value = 10
                If RdEst.Checked Then
                    TipoUsuario = 1
                    Llave = Cls.InicializarArray
                    Dim curso As LBItem = TryCast(CbCursoLectivo.SelectedItem, LBItem)
                    If curso Is Nothing Then
                        Throw New InvalidOperationException("Debe seleccionar un curso lectivo valido.")
                    End If
                    Cls.ArmaValor(Llave, "codCursoLectivo", CInt(curso.valor))
                    Cmd = " SELECT  M.cedEstudiante as cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido, P.sexo FROM tpersona as P INNER JOIN " & _
                            "tmatricula as M ON P.cedula = M.cedEstudiante WHERE codCursoLectivo = @codCursoLectivo order by cedEstudiante"
                    ''Suma los tiquetes en usuarios
                    Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad, Llave)
                Else
                    TipoUsuario = 2                    
                    Cmd = "SELECT  M.cedula, P.Nombre, P.PrimerApellido, P.SegundoApellido,P.sexo FROM tpersona as P INNER JOIN tdocente as M ON P.cedula = M.cedula "
                    Ds = Cls.ConsultarTSQL("Personas", Cmd, CnPiad)
                End If

                ''Inicia pase de datos 
                Cls.AbrirConexion(cn, True, pTransac)
                Cmd = "Update Usuario set Actualizado = 0 Where Codtipo = " & TipoUsuario
                Cls.AplicaSQL(Cmd, cn, pTransac)
                LblEstado.Text = "Importando de Datos"
                Progreso.Value = 40
                For Each Row As DataRow In Ds.Tables(0).Rows
                    Llave = Cls.InicializarArray
                    Valores = Cls.InicializarArray
                    Ced = CStr(Row("cedula")).Replace("-", String.Empty)
                    Cls.ArmaValor(Llave, "cedula", Ced)
                    Cls.ArmaValor(Valores, "cedula", Ced)
                    Cls.ArmaValor(Valores, "Nombre", CStr(Row("Nombre")))
                    Cls.ArmaValor(Valores, "PrimerApellido", CStr(Row("PrimerApellido")))
                    Cls.ArmaValor(Valores, "SegundoApellido", CStr(Row("SegundoApellido")))
                    Cls.ArmaValor(Valores, "Sexo", CStr(Row("sexo")))
                    Cls.ArmaValor(Valores, "CodTipo", TipoUsuario)
                    Cls.ArmaValor(Valores, "Actualizado", 1)
                    Cls.ArmaValor(Valores, "Activo", 1)
                    Cls.GuardarActualizar("Usuario", Valores, Llave, cn, pTransac)
                Next
                Cmd = "Update Usuario set Activo = 0 Where Actualizado = 0 and Codtipo = " & TipoUsuario
                Cls.AplicaSQL(Cmd, cn, pTransac)

                Cls.CerrarConexion(cn,pTransac)  
                LblEstado.Text = "Termino el proceso"
                Progreso.Value = 100
                MsgBox("Importacion de datos concluyo con exitó..", MsgBoxStyle.Information)
                Me.Dispose()
            Catch ex As Exception
                If pTransac IsNot Nothing Then
                    Try
                        pTransac.Rollback()
                    Catch
                    End Try
                End If
                If cn.State = ConnectionState.Open Then
                    Cls.CerrarConexion(cn)
                End If
                Progreso.Step = 0
                MsgBox("Error al actulizar: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Dispose()
    End Sub

    Private Sub Label8_Click(sender As Object, e As EventArgs)

    End Sub

    Private Sub GroupBox2_Enter(sender As Object, e As EventArgs)

    End Sub


    Private Sub TxtRecarga_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles LblTituloModulo.Click

    End Sub
End Class
