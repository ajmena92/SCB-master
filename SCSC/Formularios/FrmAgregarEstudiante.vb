Option Strict On
Option Explicit On

Imports System.IO
Imports System.Linq

Public Class FrmAgregarEstudiante

    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
            Cls.AbrirConexion(Cn, False)
            TxtFecNac.Value = Now.Date
            CargaHorarios(CBHorario)
            CargaEspecialidad(CBEspecialidad)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()  'Cierro el formulario
        End Try
        TxtCedula.Focus()
    End Sub

    Sub CargaEspecialidad(ByRef Combo As ComboBox)
        Try
            Dim Ds As New DataSet
            Dim Cls As New FuncionesDB

            Ds = Cls.ConsultarTSQL("Especialidad", "Select DISTINCT  Especialidad From Usuario where Activo= 1", Cn:=Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                Combo.DataSource = Ds.Tables(0)
                Combo.DisplayMember = "Especialidad"
                Combo.ValueMember = "Especialidad"
            End If
            Combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
    Sub CargaHorarios(ByRef Combo As ComboBox)

        Try

            Dim Ds As New DataSet
            Dim Cls As New FuncionesDB
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "IdHorario")
            Cls.ArmaValor(Valores, "Descripcion")
            Cls.ArmaValor(Valores, "Activo")
            Cls.ArmaValor(Llave, "Activo", 1)
            Ds = Cls.Consultar("Horario", Valores, Llave, Cn)
            If Ds.Tables(0).Rows.Count > 0 Then
                Combo.DataSource = Ds.Tables(0)
                Combo.DisplayMember = "Descripcion"
                Combo.ValueMember = "IdHorario"
            End If
            Combo.SelectedIndex = 0
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
    Sub LimpiarPantalla(Optional pFocus As Boolean = True)
        TxtCedula.Tag = 0
        TxtApe1.Clear()
        TxtApe2.Clear()
        TxtNombre.Clear()
        TxtFecNac.Value = Now.Date
        TxtSeccion.Clear()
        TxtTelefono.Clear()
        CBEspecialidad.SelectedIndex = -1
        CBHorario.SelectedIndex = -1
        If pFocus Then
            TxtCedula.Focus()
        End If

    End Sub

    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            TxtCedula.Clear()
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            LimpiarSession()
            gSession.Titulo = "Usuarios del Sistema"
            gSession.Valor1 = "Usuario"   '  TABLA utilizada.
            gSession.Valor2 = "IdUsuario" ' ORDER BY
            gSession.Valor3 = "Cedula" ' codigo devuelto a la aplicacion en propiedad gsession.resultado
            'gSession.Valor4 = "Cedula,Nombre,PrimerApellido,SegundoApellido" ' Valor presentado al usuario
            gSession.Valor5 = "Nombre" ' campo para el filtro utilizado
            Cls.ArmaValor(Valores, "Cedula", "Cédula")
            Cls.ArmaValor(Valores, "Nombre", "Nombre")
            Cls.ArmaValor(Valores, "PrimerApellido", "1° Apellido")
            Cls.ArmaValor(Valores, "SegundoApellido", "2­­° Apellido")
            'Armado de la llave primaria, 1=1 para todos los registros
            Cls.ArmaValor(Llave, "Activo", "1")
            gSession.Valores = Valores
            gSession.Llave = Llave
            Using frm As New Global.SCSC.Busqueda()
                frm.ShowDialog(Me)
            End Using

            If gSession.Resultado Is Nothing OrElse gSession.Resultado.Length = 0 Then
                Exit Sub
            End If

            Dim cedulaSeleccionada As String = gSession.Resultado(0)
            If String.IsNullOrWhiteSpace(cedulaSeleccionada) Then
                Exit Sub
            End If

            TxtCedula.Text = cedulaSeleccionada.Trim()
            TxtCedula_Validated(TxtCedula, EventArgs.Empty)
            BtnGuardar.Select()
        Catch ex As Exception
            ErrorLogger.LogException("FrmAgregarEstudiante.Buscar_Click", ex)
            MsgBox("No fue posible completar la búsqueda.", MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles TxtCedula.Validated
        LimpiarPantalla(False)
        Dim Cedula As String = Replace(TxtCedula.Text.Trim, ControlCarnet, "")
        Dim Ds As New DataSet
        Dim Valores(), Llave() As FuncionesDB.Campos
        If Cedula.Trim().Length > 0 Then
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "Cedula", Cedula)
                Cls.ArmaValor(Valores, "IdUsuario")
                Cls.ArmaValor(Valores, "Nombre")
                Cls.ArmaValor(Valores, "PrimerApellido")
                Cls.ArmaValor(Valores, "SegundoApellido")
                Cls.ArmaValor(Valores, "CodTipo")
                Cls.ArmaValor(Valores, "Seccion")
                Cls.ArmaValor(Valores, "Telefono")
                Cls.ArmaValor(Valores, "FechaNac")
                Cls.ArmaValor(Valores, "IdHorario")
                Cls.ArmaValor(Valores, "Especialidad")
                Ds = Cls.Consultar("Usuario", Valores, Llave, Cn)
                If Ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = Ds.Tables(0).Rows(0)
                    TxtCedula.Tag = CInt(row("IdUsuario"))
                    TxtNombre.Text = CStr(row("Nombre"))
                    TxtApe1.Text = CStr(row("PrimerApellido"))
                    TxtApe2.Text = CStr(row("SegundoApellido"))
                    TxtFecNac.Value = CDate(row("FechaNac"))
                    TxtTelefono.Text = Convert.ToString(row("Telefono"))
                    TxtSeccion.Text = Convert.ToString(row("Seccion"))
                    CBHorario.SelectedValue = CInt(row("IdHorario"))
                    CBEspecialidad.SelectedValue = Convert.ToString(row("Especialidad"))
                Else
                    Dim resp As MsgBoxResult = MsgBox("Usuario no ingresado en el sistema. Desea agregar el estudiante ?", MsgBoxStyle.OkCancel Or MsgBoxStyle.Question)
                    If resp = MsgBoxResult.Cancel Then
                        BtnCancelar_Click(sender, e)
                    Else
                        LimpiarPantalla(False)
                        TxtNombre.Focus()
                    End If

                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            LimpiarPantalla(False)
        End If
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtCedula.KeyDown
        If e.KeyCode = Keys.Enter Then
            TxtNombre.Focus()
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Close()
    End Sub

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        Dim Valores(), Llave() As FuncionesDB.Campos
        Try
            Dim especialidad As String = If(CBEspecialidad.SelectedValue Is Nothing, String.Empty, CStr(CBEspecialidad.SelectedValue))
            Dim idHorario As Integer = If(CBHorario.SelectedValue Is Nothing, 0, CInt(CBHorario.SelectedValue))
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Llave, "Cedula", TxtCedula.Text.Trim)
            Cls.ArmaValor(Valores, "Cedula", TxtCedula.Text.Trim)
            Cls.ArmaValor(Valores, "Nombre", TxtNombre.Text.Trim.ToUpper)
            Cls.ArmaValor(Valores, "PrimerApellido", TxtApe1.Text.Trim.ToUpper)
            Cls.ArmaValor(Valores, "SegundoApellido", TxtApe2.Text.Trim.ToUpper)
            Cls.ArmaValor(Valores, "CodTipo", 1) 'Codigo estudiante
            Cls.ArmaValor(Valores, "Seccion", TxtSeccion.Text.Trim.ToUpper)
            Cls.ArmaValor(Valores, "Telefono", TxtTelefono.Text.Trim)
            Cls.ArmaValor(Valores, "FechaNac", TxtFecNac.Value)
            Cls.ArmaValor(Valores, "Especialidad", especialidad)
            Cls.ArmaValor(Valores, "IdHorario", idHorario)
            Cls.ArmaValor(Valores, "Actualizado", True)
            Cls.ArmaValor(Valores, "Activo", True)

            Cls.GuardarActualizar("Usuario", Valores, Llave, Cn)
            BtnCancelar_Click(sender, e)
        Catch ex As Exception
            MsgBox("Error al ingresar estudiante : " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
        TxtCedula.Clear()
        TxtCedula.Focus()
    End Sub

    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click
        Dim Llave() As FuncionesDB.Campos
        Try
            If TxtCedula.Text.Trim().Length > 0 Then
                If Not CrudOperationHelper.ConfirmarEliminacion("el estudiante") Then
                    Exit Sub
                End If

                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdUsuario", TxtCedula.Tag)
                Cls.Delete("Usuario", Llave, Cn)
                MsgBox("Registro eliminado correctamente", MsgBoxStyle.Information)
                BtnCancelar.PerformClick()
            Else
                MsgBox("Ingrese el Registro que desea eliminar.", MsgBoxStyle.Exclamation)
                TxtCedula.Focus()
            End If
        Catch ex As Exception
            Dim resp As MsgBoxResult = MsgBox("No se puede borrar el usuario porque tiene marcas asociadas. Desea desactivarlo del sistema ?", MsgBoxStyle.OkCancel Or MsgBoxStyle.Question)
            If resp = MsgBoxResult.Cancel Then
                BtnCancelar_Click(sender, e)
                Exit Try
            End If
            Try
                Dim Valores() As FuncionesDB.Campos
                Llave = Cls.InicializarArray
                Valores = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdUsuario", TxtCedula.Tag)
                Cls.ArmaValor(Valores, "Activo", False)
                Cls.Update("Usuario", Valores, Llave, Cn)
                MsgBox("Registro desactivado correctamente", MsgBoxStyle.Information)
            Catch ex2 As Exception
                MsgBox(ex2.Message, MsgBoxStyle.Critical)
            End Try
        End Try
    End Sub


    Private Sub CaptureForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
        Me.Dispose()
    End Sub
End Class
