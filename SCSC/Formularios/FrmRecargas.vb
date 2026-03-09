Option Strict On
Option Explicit On

Public Class FrmRecarga

    Dim Cn As New SqlClient.SqlConnection
    Dim Cls As New FuncionesDB
    Private ReadOnly _recargaService As New RecargaService()
    Private _becas As DataTable
    Dim TipoUsuario As String
    Dim TipoUsuarioCod As Integer

    Dim Precio As Decimal
    'Private Property Ticket As Ticket


    Sub LimpiarPantalla()
        txtCedula.Clear()
        TxtPrimerApellido.Clear()
        TxtSegundoApellido.Clear()
        TxtNombre.Clear()
        TxtRecarga.Clear()
        LblCantTiques.Text = ("0 Tiquetes")
        LblTotal.Text = "0"
        txtCedula.Focus()
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
            _becas = _recargaService.CargarBecas(Cn)
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose() 'Cierro el formulario
        End Try
        txtCedula.Focus()
    End Sub



    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            txtCedula.Clear()
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "Cedula", "Cédula")
            Cls.ArmaValor(Valores, "Nombre", "Nombre")
            Cls.ArmaValor(Valores, "PrimerApellido", "1° Apellido")
            Cls.ArmaValor(Valores, "SegundoApellido", "2­­° Apellido")
            'Armado de la llave primaria, 1=1 para todos los registros
            Cls.ArmaValor(Llave, "1", "1")
            Dim request As New SearchRequest()
            request.Title = "Usuarios del Sistema"
            request.TableName = "Usuario"
            request.OrderBy = "IdUsuario"
            request.ReturnFieldsCsv = "Cedula"
            request.DefaultFilterField = "Nombre"
            request.Values = Valores
            request.Keys = Llave
            Using frm As New Global.SCSC.Busqueda()
                frm.Request = request
                frm.ShowDialog(Me)
                If frm.SelectedValues Is Nothing OrElse frm.SelectedValues.Length = 0 Then
                    Exit Sub
                End If
                Dim cedulaSeleccionada As String = frm.SelectedValues(0)
                If String.IsNullOrWhiteSpace(cedulaSeleccionada) Then
                    Exit Sub
                End If
                txtCedula.Text = cedulaSeleccionada.Trim()
            End Using

            If String.IsNullOrWhiteSpace(txtCedula.Text) Then
                Exit Sub
            End If
            TxtCedula_Validated(txtCedula, EventArgs.Empty)
        Catch ex As Exception
            ErrorLogger.LogException("FrmRecargas.Buscar_Click", ex)
            MsgBox("No fue posible completar la búsqueda.", MsgBoxStyle.Exclamation)
        End Try

    End Sub

    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles txtCedula.Validated
        Dim Cedula As String = Replace(txtCedula.Text.Trim, ControlCarnet, "")
        Cedula = Replace(Cedula, "CTPP", "")
        If Cedula.Trim().Length > 0 Then
            Try
                DiaSemana = Weekday(Now).ToString()
                Dim usuario As RecargaService.UsuarioRecargaInfo = _recargaService.ObtenerUsuarioPorCedula(Cn, Cedula)
                If usuario IsNot Nothing Then
                    TxtNombre.Text = usuario.Nombre
                    TxtPrimerApellido.Text = usuario.PrimerApellido
                    TxtSegundoApellido.Text = usuario.SegundoApellido
                    If usuario.CodTipo = 1 Then
                        TipoUsuario = "ESTUDIANTE"
                        Precio = PrecioEstudiante
                        TipoUsuarioCod = 1
                    Else
                        TipoUsuario = "PROFESOR"
                        Precio = PrecioDocente
                        TipoUsuarioCod = 2
                    End If
                    If Not usuario.Activo Then
                        MsgBox("El usuario ingresado esta inactivo, no puede realizar recargas", MsgBoxStyle.Critical)
                        LimpiarPantalla()
                        Exit Sub
                    End If
                    LblTipoBeca.Text = String.Empty
                    For Each Beca As DataRow In _becas.Rows
                        If CInt(Beca("IdBeca")) = usuario.TipoBeca Then
                            LblTipoBeca.Text = CStr(Beca("Descripcion"))
                            Exit For
                        End If
                    Next
                    LblTipoUsuario.Text = TipoUsuario
                    txtCedula.Tag = usuario.IdUsuario
                    LblCantTiques.Text = usuario.CantidadTiquetes.ToString() & " Disponibles "
                Else
                    LimpiarPantalla()
                    MsgBox("Usuario no ingresado en el sistema", MsgBoxStyle.Information)
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        Else
            LimpiarPantalla()
        End If

    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCedula.KeyDown
        If e.KeyCode = Keys.Enter Then
            TxtRecarga.Focus()
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        End If
    End Sub

    Private Sub TxtRecarga_Validated(sender As Object, e As EventArgs) Handles TxtRecarga.Validated
        Dim Cantidad As Integer = ObtenerCantidadRecarga()
        LblTotal.Text = Format(CType(Cantidad * Precio, Decimal), "#,##0.00")

    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
        txtCedula.Clear()
        txtCedula.Focus()
    End Sub

    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtRecarga.KeyDown
        If e.KeyCode = Keys.Enter Then
            BtnGuardar.Focus()
        End If
    End Sub

    Private Function Validacion() As Boolean
        If txtCedula.Text.Trim().Length = 0 Then
            MsgBox("Ingrese el numero de cedula", MsgBoxStyle.Critical)
            txtCedula.Focus()
            Return False
        ElseIf ObtenerCantidadRecarga() < 1 Then
            MsgBox("Ingrese la cantidad de tiquetes a ingresar", MsgBoxStyle.Critical)
            TxtRecarga.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function ObtenerCantidadRecarga() As Integer
        Dim raw As String = sen(TxtRecarga.Text)
        Dim cantidad As Integer
        If Integer.TryParse(raw, cantidad) Then
            Return cantidad
        End If
        Return 0
    End Function

    Private Sub BtnGuardar_Click(sender As Object, e As EventArgs) Handles BtnGuardar.Click
        If Validacion() Then
            Try
                Dim recargaCantidad As Integer = ObtenerCantidadRecarga()
                Dim cantidadActualizada As Integer = _recargaService.AplicarRecarga(Cn, CInt(txtCedula.Tag), Precio, TipoUsuarioCod, recargaCantidad)
                MsgBox("Recarga realizada con exitosamente.", MsgBoxStyle.Information)
                LblCantTiques.Text = cantidadActualizada.ToString() & " Disponibles "

                TxtRecarga.Clear()
                LblTotal.Text = "0"
                txtCedula.Focus()
                'BtnCancelar_Click(sender, e)
            Catch ex As Exception
                MsgBox("Error al recargar: " & ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Close()
    End Sub
    'Private Sub Imprimir_Click_1(sender As Object, e As EventArgs) Handles Imprimir.Click



    '    Ticket = New Ticket()



    '    Ticket.AddHeaderLine("CTP GUAYCARA")
    '    Ticket.AddHeaderLine("")
    '    Ticket.AddHeaderLine(TxtNombre.Text & "" & TxtPrimerApellido.Text & "" & TxtSegundoApellido.Text)
    '    'Ticket.AddHeaderLine("Cantidad: " & TxtRecarga.Text)
    '    'Ticket.AddHeaderLine("Total a pagar:  ₡" & LblTotal.Text)


    '    Ticket.AddItem(TxtRecarga.Text, "Tiquetes", LblTotal.Text)

    '    Ticket.AddSubHeaderLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString())



    '    Ticket.AddTotal("SUBTOTAL", "")
    '    Ticket.AddTotal("IVA", " ₡ 0")
    '    Ticket.AddTotal("TOTAL", LblTotal.Text)
    '    'Ticket.AddTotal("", "")
    '    ''Ticket.AddTotal("RECIBIDO", "0")
    '    ''Ticket.AddTotal("CAMBIO", "0")
    '    'Ticket.AddTotal("", "")


    '    Ticket.AddFooterLine("Tiquetes: " & LblCantTiques.Text)
    '    'Ticket.AddFooterLine("RECARGA")

    '    Ticket.PrintTicket("POS-58")
    '    BtnCancelar_Click(sender, e)

    'End Sub




End Class
