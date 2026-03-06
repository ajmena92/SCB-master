Option Strict On
Option Explicit On

Imports System.Linq


Public Class FrmBecas
    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Sub LimpiarPantalla()
        txtCodBeca.Clear()
        txtCodBeca.Tag = 0
        TxtDescripcion.Clear()
        CkActivo.Checked = False
        LimpiarChek()
        Buscar.Focus()
    End Sub

    Sub LimpiarChek()
        Ck2.Checked = False
        Ck3.Checked = False
        Ck4.Checked = False
        Ck5.Checked = False
        Ck6.Checked = False
    End Sub

    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
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
            LimpiarChek()
            Buscar.Focus()
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose() 'Cierro el formulario
        End Try
        Buscar.Focus()
    End Sub



    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            txtCodBeca.Clear()
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            LimpiarSession()
            gSession.Titulo = "Becas Comedor del Sistema"
            gSession.Valor1 = "TipoBeca"   '  TABLA utilizada.
            gSession.Valor2 = "IdBeca" ' ORDER BY
            gSession.Valor3 = "IdBeca" ' codigo devuelto a la aplicacion en propiedad gsession.resultado
            'gSession.Valor4 = "Cedula,Nombre,PrimerApellido,SegundoApellido" ' Valor presentado al usuario
            gSession.Valor5 = "Descripcion" ' campo para el filtro utilizado
            Cls.ArmaValor(Valores, "IdBeca", "Código")
            Cls.ArmaValor(Valores, "Descripcion", "Descripción")
            'Armado de la llave primaria, 1=1 para todos los registros
            Cls.ArmaValor(Llave, "1", "1")
            gSession.Valores = Valores
            gSession.Llave = Llave
            Using frm As New Global.SCSC.Busqueda()
                frm.ShowDialog(Me)
            End Using
            txtCodBeca.Text = CStr(gSession.Resultado(0))
            txtCodRuta_Validated(sender, e)
        Catch ex As Exception
            'MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try

    End Sub

    Private Sub txtCodRuta_Validated(sender As Object, e As EventArgs) Handles txtCodBeca.Validated
        Dim Codigo As String = Replace(txtCodBeca.Text.Trim, ControlCarnet, "")
        Dim Ds As New DataSet
        Dim Valores(), Llave() As FuncionesDB.Campos
        If Codigo.Trim().Length > 0 Then
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdBeca", Codigo)
                Cls.ArmaValor(Valores, "IdBeca")
                Cls.ArmaValor(Valores, "Descripcion")
                Cls.ArmaValor(Valores, "Activo")
                Cls.ArmaValor(Valores, "DiasBeca")
                Ds = Cls.Consultar("TipoBeca", Valores, Llave, Cn)
                LimpiarChek()
                If Ds.Tables(0).Rows.Count > 0 Then
                    Dim row As DataRow = Ds.Tables(0).Rows(0)
                    TxtDescripcion.Text = CStr(row("Descripcion"))
                    CkActivo.Checked = CBool(row("Activo"))
                    txtCodBeca.Tag = CInt(row("IdBeca"))
                    Dim diasBeca As String = CStr(row("DiasBeca"))
                    For dia As Integer = 2 To 6
                        If InStr(diasBeca, dia.ToString()) > 0 Then
                            Select Case dia
                                Case 2
                                    Ck2.Checked = True
                                Case 3
                                    Ck3.Checked = True
                                Case 4
                                    Ck4.Checked = True
                                Case 5
                                    Ck5.Checked = True
                                Case 6
                                    Ck6.Checked = True
                            End Select
                        End If
                    Next

                Else
                    TxtDescripcion.Focus()
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            LimpiarPantalla()
        End If

    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCodBeca.KeyDown
        If e.KeyCode = Keys.Enter Then
            TxtDescripcion.Focus()
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        End If
    End Sub

    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtDescripcion.KeyDown
        If e.KeyCode = Keys.Enter Then
            BtnGuardar.Focus()
        End If
    End Sub

    Private Function Validacion() As Boolean
        If TxtDescripcion.Text.Trim().Length <= 5 Then
            MsgBox("La descripción no es valida, no cumple con el minimo necesario", MsgBoxStyle.Critical)
            TxtDescripcion.Focus()
            Return False
        ElseIf txtCodBeca.Text = "1" Then
            MsgBox("El código 'SIN BECA' no puede ser editada", MsgBoxStyle.Critical)
            Buscar.Focus()
            Return False
        End If

        Return True
    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validacion() Then
            Dim DiasBecados As String = String.Empty
            Dim Valores(), Llave() As FuncionesDB.Campos
            Try
                Valores = Cls.InicializarArray
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdBeca", txtCodBeca.Text)
                Cls.ArmaValor(Valores, "Descripcion", TxtDescripcion.Text.ToUpper.Trim)
                Cls.ArmaValor(Valores, "Activo", CkActivo.Checked)
                For Each c As Control In Me.GpDiasBeca.Controls
                    If TypeOf c Is CheckBox Then
                        Dim chk As CheckBox = DirectCast(c, CheckBox)
                        If chk.Checked Then
                            DiasBecados &= CStr(chk.Tag)
                        Else
                            DiasBecados &= "0"
                        End If
                    End If
                Next c
                Cls.ArmaValor(Valores, "DiasBeca", DiasBecados)
                Cls.GuardarActualizar("TipoBeca", Valores, Llave, Cn)
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

    Private Sub txtCodRuta_TextChanged(sender As Object, e As EventArgs) Handles txtCodBeca.TextChanged

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
            If txtCodBeca.Text.Trim().Length > 0 Then
                If txtCodBeca.Text = "1" Then
                    Throw New Exception("El código 'SIN BECA' no puede ser editada")
                End If
                If Not CrudOperationHelper.ConfirmarEliminacion("la beca") Then
                    Exit Sub
                End If
                Dim Llave() As FuncionesDB.Campos
                Llave = Cls.InicializarArray
                Cls.ArmaValor(Llave, "IdBeca", txtCodBeca.Text)
                Cls.Delete("TipoBeca", Llave, Cn)
                MsgBox("Registro eliminado correctamente", MsgBoxStyle.Information)
                BtnCancelar.PerformClick()
            Else
                MsgBox("Ingrese el Registro que desea eliminar.", MsgBoxStyle.Exclamation)
                Buscar.Focus()
            End If
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
        txtCodBeca.Clear()
        Buscar.Focus()
    End Sub

    Private Sub dia_CheckedChanged(sender As Object, e As EventArgs) Handles Ck2.CheckedChanged, Ck3.CheckedChanged, Ck4.CheckedChanged, Ck5.CheckedChanged, Ck6.CheckedChanged
        Dim chk As CheckBox = DirectCast(sender, CheckBox)
        If chk.Checked Then
            chk.ForeColor = Color.Green
        Else
            chk.ForeColor = Color.Red
        End If
    End Sub
End Class
