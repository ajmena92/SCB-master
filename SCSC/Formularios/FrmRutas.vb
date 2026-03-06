Option Strict On
Option Explicit On

Public Class FrmRutas
    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Sub LimpiarPantalla(Optional ByVal PCodigo As Boolean = True)
        TxtDescripcion.Clear()
        CkActivo.Checked = False
        If PCodigo Then
            txtCodRuta.Clear()
            txtCodRuta.Tag = 0
            txtCodRuta.Focus()
        End If

    End Sub

    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch ex As Exception
            ''Se cierre el formulario            
        End Try
        Me.Dispose()  'Cierro el formulario
    End Sub

    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "dialogo")
            Cls.AbrirConexion(Cn, False)
            CkActivo.Checked = False
            txtCodRuta.Focus()
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose() 'Cierro el formulario
        End Try
        txtCodRuta.Focus()
    End Sub



    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            txtCodRuta.Clear()
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            LimpiarSession()
            gSession.Titulo = "Rutas del Sistema"
            gSession.Valor1 = "Ruta"   '  TABLA utilizada.
            gSession.Valor2 = "IdRuta" ' ORDER BY
            gSession.Valor3 = "Codigo" ' codigo devuelto a la aplicacion en propiedad gsession.resultado
            'gSession.Valor4 = "Cedula,Nombre,PrimerApellido,SegundoApellido" ' Valor presentado al usuario
            gSession.Valor5 = "Descripcion" ' campo para el filtro utilizado
            Cls.ArmaValor(Valores, "Codigo", "Código")
            Cls.ArmaValor(Valores, "Descripcion", "Descripción")
            'Armado de la llave primaria, 1=1 para todos los registros
            Cls.ArmaValor(Llave, "1", "1")
            gSession.Valores = Valores
            gSession.Llave = Llave
            Using frm As New Global.SCSC.Busqueda()
                frm.ShowDialog(Me)
            End Using
            txtCodRuta.Text = CStr(gSession.Resultado(0))
            txtCodRuta_Validated(sender, e)
        Catch ex As Exception
            'MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub txtCodRuta_Validated(sender As Object, e As EventArgs) Handles txtCodRuta.Validated
        Dim Cedula As String = Replace(txtCodRuta.Text.Trim, ControlCarnet, "")
        Dim Ds As New DataSet
        Dim Valores(), Llave() As FuncionesDB.Campos
        If Cedula.Trim().Length > 0 Then
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "Codigo", Cedula)
                Cls.ArmaValor(Valores, "IdRuta")
                Cls.ArmaValor(Valores, "Descripcion")
                Cls.ArmaValor(Valores, "Activo")
                Ds = Cls.Consultar("Ruta", Valores, Llave, Cn)
                If Ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = Ds.Tables(0).Rows(0)
                    TxtDescripcion.Text = CStr(row("Descripcion"))
                    CkActivo.Checked = CBool(row("Activo"))
                    txtCodRuta.Tag = CInt(row("IdRuta"))
                Else
                    LimpiarPantalla(False)
                    txtCodRuta.Tag = ObtenerIdRutaDesdeTag()
                    TxtDescripcion.Focus()
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            LimpiarPantalla()
        End If

    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCodRuta.KeyDown
        If e.KeyCode = Keys.Enter Then
            TxtDescripcion.Focus()
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        End If
    End Sub


    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
        txtCodRuta.Clear()
        txtCodRuta.Focus()
    End Sub

    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode = Keys.Enter Then
            BtnGuardar.Focus()
        End If
    End Sub

    Private Function Validacion() As Boolean
        If txtCodRuta.Text.Trim().Length = 0 Then
            MsgBox("Ingrese el numero de ruta", MsgBoxStyle.Critical)
            txtCodRuta.Focus()
            Return False
        ElseIf TxtDescripcion.Text.Trim().Length <= 5 Then
            MsgBox("La descripción no es valida, no cumple con el minimo necesario", MsgBoxStyle.Critical)
            TxtDescripcion.Focus()
            Return False
        ElseIf ObtenerIdRutaDesdeTag() = 1 Then
            MsgBox("La ruta 0 no puede ser editada", MsgBoxStyle.Critical)
            txtCodRuta.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validacion() Then
            Dim Valores(), Llave() As FuncionesDB.Campos
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdRuta", ObtenerIdRutaDesdeTag())
                Cls.ArmaValor(Valores, "Codigo", txtCodRuta.Text)
                Cls.ArmaValor(Valores, "Descripcion", TxtDescripcion.Text.ToUpper.Trim)
                Cls.ArmaValor(Valores, "Activo", CkActivo.Checked)
                Cls.GuardarActualizar("Ruta", Valores, Llave, Cn)
                MsgBox("Ruta guardada con exito", MsgBoxStyle.Information)

                BtnCancelar_Click(sender, e)
            Catch ex As Exception
                MsgBox("Error al guardar datos: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Close()
    End Sub

    Private Sub txtCodRuta_TextChanged(sender As Object, e As EventArgs) Handles txtCodRuta.TextChanged

    End Sub

    Private Sub CkActivo_CheckedChanged(sender As Object, e As EventArgs) Handles CkActivo.CheckedChanged
        If (CkActivo.Checked = True) Then
            CkActivo.ForeColor = Color.Green
            CkActivo.Text = "Activo"
        Else
            CkActivo.ForeColor = Color.Red
            CkActivo.Text = "Inactivo"
        End If
    End Sub

    Private Sub BtnEliminar_Click(sender As Object, e As EventArgs) Handles BtnEliminar.Click
        Try
            If txtCodRuta.Text.Trim().Length > 0 Then
                If ObtenerIdRutaDesdeTag() = 1 Then
                    Throw New Exception("La ruta 0 no puede ser editada")
                End If
                If Not CrudOperationHelper.ConfirmarEliminacion("la ruta") Then
                    Exit Sub
                End If
                Dim Llave() As FuncionesDB.Campos
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdRuta", ObtenerIdRutaDesdeTag())
                Cls.Delete("Ruta", Llave, Cn)
                MsgBox("Registro eliminado correctamente", MsgBoxStyle.Information)
                BtnCancelar.PerformClick()
            Else
                MsgBox("Ingrese el Registro que desea eliminar.", MsgBoxStyle.Exclamation)
                txtCodRuta.Focus()
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Function ObtenerIdRutaDesdeTag() As Integer
        Dim rawTag As String = Convert.ToString(txtCodRuta.Tag)
        Dim parsed As Integer
        If Integer.TryParse(rawTag, parsed) Then
            Return parsed
        End If
        If Integer.TryParse(txtCodRuta.Text.Trim(), parsed) Then
            Return parsed
        End If
        Return 0
    End Function
End Class
