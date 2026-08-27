Option Strict On
Option Explicit On

Imports System.Data
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms

Public Class FrmRecarga

    Private ReadOnly Cn As New SqlClient.SqlConnection()
    Private ReadOnly Cls As New FuncionesDB()
    Private ReadOnly _recargaService As New RecargaService()
    Private ReadOnly _parametroSvc As New ParametroSistemaService()

    Private _becas As DataTable
    Private _configParametros As ParametroSistemaService.ParametroSistemaConfig
    Private _panelHeader As Panel
    Private _lblClienteSection As Label
    Private _lblRecargaSection As Label
    Private _lblSubtitulo As Label
    Private _lblAtajos As Label
    Private _lblEstadoOperacion As Label
    Private _panelResumen As Panel
    Private _lblResumenEstadoValor As Label
    Private _lblResumenPrecioValor As Label
    Private _lblResumenSaldoValor As Label
    Private _lblResumenSaldoFinalValor As Label
    Private _lblResumenTotalValor As Label
    Private _saldoActual As Integer

    Private TipoUsuario As String
    Private TipoUsuarioCod As Integer
    Private Precio As Decimal

    Private Enum EstadoOperacionVisual
        Neutro = 0
        Exito = 1
        Advertencia = 2
        ErrorCritico = 3
    End Enum

    Private Sub LimpiarPantalla()
        txtCedula.Clear()
        TxtNombre.Clear()
        TxtPrimerApellido.Clear()
        TxtSegundoApellido.Clear()
        TxtRecarga.Clear()
        txtCedula.Tag = Nothing
        TipoUsuario = String.Empty
        TipoUsuarioCod = 0
        Precio = 0D
        _saldoActual = 0

        LblTipoUsuario.Text = "Sin clasificar"
        LblTipoBeca.Text = "Sin beca"
        LblCantTiques.Text = "0 tiquetes"
        LblTotal.Text = "₡ 0.00"

        ActualizarEstadoOperacion("Escanee o escriba una cédula para iniciar la recarga.", EstadoOperacionVisual.Neutro)
        ActualizarResumenOperacion()
        txtCedula.Focus()
    End Sub

    Private Sub FrmRecarga_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        Try
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
        Catch
        End Try
        Me.Dispose()
    End Sub

    Private Sub FrmEstudiantes_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If

        Try
            CrudVisualHelper.ApplyCrudStandard(Me, "operativo")
            Cls.AbrirConexion(Cn, False)
            _becas = _recargaService.CargarBecas(Cn)
            CargarParametrosRecarga()
            ApplyPointOfSaleLayout()
            LimpiarPantalla()
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            MsgBox("Error al cargar el Formulario: " & ex.Message, MsgBoxStyle.Critical)
            Me.Dispose()
        End Try
    End Sub

    Private Sub FrmRecarga_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If CrudVisualHelper.IsInDesignMode(Me) OrElse Me.IsDisposed Then
            Return
        End If
        LayoutPointOfSale()
    End Sub

    Private Sub Buscar_Click(sender As Object, e As EventArgs) Handles Buscar.Click
        Try
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "Cedula", "Cédula")
            Cls.ArmaValor(Valores, "Nombre", "Nombre")
            Cls.ArmaValor(Valores, "PrimerApellido", "1° Apellido")
            Cls.ArmaValor(Valores, "SegundoApellido", "2° Apellido")
            Cls.ArmaValor(Llave, "1", "1")

            Dim request As New SearchRequest()
            request.Title = "Usuarios del Sistema"
            request.TableName = "Usuario"
            request.OrderBy = "IdUsuario"
            request.ReturnFieldsCsv = "Cedula"
            request.DefaultFilterField = "Nombre"
            request.Values = Valores
            request.Keys = Llave

            Dim cedulaActual As String = txtCedula.Text
            Dim cedulaSeleccionada As String = cedulaActual
            If Not SearchDialogHelper.TrySelectSingleValue(Me, request, cedulaActual, cedulaSeleccionada) Then
                txtCedula.Text = cedulaActual
                Exit Sub
            End If

            txtCedula.Text = cedulaSeleccionada
            If String.IsNullOrWhiteSpace(txtCedula.Text) Then
                Exit Sub
            End If

            TxtCedula_Validated(txtCedula, EventArgs.Empty)
        Catch ex As Exception
            ErrorLogger.LogException("FrmRecargas.Buscar_Click", ex)
            ActualizarEstadoOperacion("No fue posible completar la búsqueda.", EstadoOperacionVisual.ErrorCritico)
            MsgBox("No fue posible completar la búsqueda.", MsgBoxStyle.Exclamation)
        End Try
    End Sub

    Private Sub TxtCedula_Validated(sender As Object, e As EventArgs) Handles txtCedula.Validated
        Dim cedula As String = NormalizarCedula(txtCedula.Text)

        If cedula.Trim().Length = 0 Then
            LimpiarPantalla()
            Return
        End If

        Try
            DiaSemana = Weekday(Now).ToString()
            Dim usuario As RecargaService.UsuarioRecargaInfo = _recargaService.ObtenerUsuarioPorCedula(Cn, cedula)
            If usuario Is Nothing Then
                LimpiarPantalla()
                ActualizarEstadoOperacion("La cédula no existe en el sistema.", EstadoOperacionVisual.Advertencia)
                MsgBox("Usuario no ingresado en el sistema", MsgBoxStyle.Information)
                Return
            End If

            If Not usuario.Activo Then
                LimpiarPantalla()
                ActualizarEstadoOperacion("El usuario está inactivo y no puede recibir recargas.", EstadoOperacionVisual.ErrorCritico)
                MsgBox("El usuario ingresado esta inactivo, no puede realizar recargas", MsgBoxStyle.Critical)
                Return
            End If

            TxtPrimerApellido.Text = usuario.PrimerApellido
            TxtSegundoApellido.Text = usuario.SegundoApellido
            TxtNombre.Text = ConstruirNombreCompleto(usuario.Nombre, usuario.PrimerApellido, usuario.SegundoApellido)

            If usuario.CodTipo = 1 Then
                TipoUsuario = "ESTUDIANTE"
                TipoUsuarioCod = 1
            Else
                TipoUsuario = "PROFESOR"
                TipoUsuarioCod = 2
            End If

            Precio = ObtenerPrecioUnitario(TipoUsuarioCod)
            LblTipoBeca.Text = ObtenerDescripcionBeca(usuario.TipoBeca)
            LblTipoUsuario.Text = TipoUsuario
            txtCedula.Tag = usuario.IdUsuario
            _saldoActual = usuario.CantidadTiquetes
            LblCantTiques.Text = _saldoActual.ToString("N0", CultureInfo.InvariantCulture) & " tiquetes"

            ActualizarEstadoOperacion("Usuario listo para recargar. Ingrese la cantidad y confirme el cobro.", EstadoOperacionVisual.Exito)
            ActualizarResumenOperacion()
        Catch ex As Exception
            ActualizarEstadoOperacion("No se pudo cargar la información del usuario.", EstadoOperacionVisual.ErrorCritico)
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub TxtCedula_KeyDown(sender As Object, e As KeyEventArgs) Handles txtCedula.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            TxtCedula_Validated(sender, EventArgs.Empty)
            If txtCedula.Tag IsNot Nothing Then
                TxtRecarga.Focus()
                TxtRecarga.SelectAll()
            End If
        ElseIf e.KeyCode = Keys.F2 Then
            Buscar.PerformClick()
        ElseIf e.KeyCode = Keys.F8 Then
            BtnGuardar.PerformClick()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            BtnCancelar.PerformClick()
        End If
    End Sub

    Private Sub TxtRecarga_TextChanged(sender As Object, e As EventArgs) Handles TxtRecarga.TextChanged
        ActualizarResumenOperacion()
    End Sub

    Private Sub TxtRecarga_Validated(sender As Object, e As EventArgs) Handles TxtRecarga.Validated
        ActualizarResumenOperacion()
    End Sub

    Private Sub BtnCancelar_Click(sender As Object, e As EventArgs) Handles BtnCancelar.Click
        LimpiarPantalla()
    End Sub

    Private Sub TxtRecarga_KeyDown(sender As Object, e As KeyEventArgs) Handles TxtRecarga.KeyDown
        If e.KeyCode = Keys.Enter Then
            e.SuppressKeyPress = True
            BtnGuardar.Focus()
        ElseIf e.KeyCode = Keys.Escape Then
            e.SuppressKeyPress = True
            BtnCancelar.PerformClick()
        End If
    End Sub

    Private Function Validacion() As Boolean
        If txtCedula.Text.Trim().Length = 0 OrElse txtCedula.Tag Is Nothing Then
            ActualizarEstadoOperacion("Primero debe seleccionar un usuario válido.", EstadoOperacionVisual.Advertencia)
            MsgBox("Ingrese el numero de cedula", MsgBoxStyle.Critical)
            txtCedula.Focus()
            Return False
        End If

        If ObtenerCantidadRecarga() < 1 Then
            ActualizarEstadoOperacion("Ingrese una cantidad válida de tiquetes.", EstadoOperacionVisual.Advertencia)
            MsgBox("Ingrese la cantidad de tiquetes a ingresar", MsgBoxStyle.Critical)
            TxtRecarga.Focus()
            Return False
        End If

        If Precio <= 0D Then
            ActualizarEstadoOperacion("No hay un precio configurado para este tipo de usuario.", EstadoOperacionVisual.ErrorCritico)
            MsgBox("No se encontró el precio de recarga en los parámetros del sistema.", MsgBoxStyle.Critical)
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
        If Not Validacion() Then
            Exit Sub
        End If

        Try
            Dim recargaCantidad As Integer = ObtenerCantidadRecarga()
            Dim cantidadActualizada As Integer = _recargaService.AplicarRecarga(Cn, CInt(txtCedula.Tag), Precio, TipoUsuarioCod, recargaCantidad)
            _saldoActual = cantidadActualizada
            LblCantTiques.Text = _saldoActual.ToString("N0", CultureInfo.InvariantCulture) & " tiquetes"
            TxtRecarga.Clear()
            ActualizarEstadoOperacion("Recarga aplicada correctamente. Puede atender al siguiente usuario.", EstadoOperacionVisual.Exito)
            ActualizarResumenOperacion()
            MsgBox("Recarga realizada con exitosamente.", MsgBoxStyle.Information)
            txtCedula.Focus()
            txtCedula.SelectAll()
        Catch ex As Exception
            ActualizarEstadoOperacion("Ocurrió un error al aplicar la recarga.", EstadoOperacionVisual.ErrorCritico)
            MsgBox("Error al recargar: " & ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Sub BtnRegresar_Click(sender As Object, e As EventArgs) Handles BtnRegresar.Click
        Me.Close()
    End Sub

    Private Sub CargarParametrosRecarga()
        Try
            _parametroSvc.CrearFila1(Cn)
            _parametroSvc.MigrarDesdeAppConfigSiCorresponde(Cn)
            _configParametros = _parametroSvc.ObtenerFila1(Cn)

            If _configParametros IsNot Nothing Then
                If _configParametros.PrecioDocente > 0D Then
                    PrecioDocente = _configParametros.PrecioDocente
                End If
                If _configParametros.PrecioEstudiante > 0D Then
                    PrecioEstudiante = _configParametros.PrecioEstudiante
                End If
            End If
        Catch ex As Exception
            ErrorLogger.LogException("FrmRecarga.CargarParametrosRecarga", ex)
        End Try
    End Sub

    Private Sub ApplyPointOfSaleLayout()
        Me.SuspendLayout()
        Try
            Me.Text = "Punto de venta de recargas"
            Me.BackColor = UIConstants.AppBackground
            Me.Font = UIConstants.FontBody()
            Me.BackgroundImage = Nothing
            Me.FormBorderStyle = FormBorderStyle.Sizable
            Me.MaximizeBox = True
            Me.MinimizeBox = True
            Me.MinimumSize = New Size(1080, 640)
            Me.StartPosition = FormStartPosition.CenterScreen
            Me.WindowState = FormWindowState.Maximized

            LblTituloModulo.Text = "Punto de venta"
            LblTituloModulo.Font = New Font("Segoe UI Semibold", 20.0!, FontStyle.Bold, GraphicsUnit.Point)
            LblTituloModulo.ForeColor = UIConstants.TextPrimary
            LblTituloModulo.BackColor = Color.Transparent
            LblTituloModulo.AutoSize = False

            GroupDatosUsuario.Text = " "
            GroupDatosCompra.Text = " "

            For Each grp As GroupBox In New GroupBox() {GroupDatosUsuario, GroupDatosCompra}
                grp.BackColor = UIConstants.Surface
                grp.ForeColor = UIConstants.TextPrimary
                grp.Font = UIConstants.FontBodyStrong()
                grp.Padding = New Padding(UIConstants.SpaceLg, 18, UIConstants.SpaceLg, UIConstants.SpaceLg)
                grp.FlatStyle = FlatStyle.Flat
            Next

            ConfigurarBotonBusqueda()
            ConfigurarCampoEntrada(txtCedula, False)
            ConfigurarCampoEntrada(TxtRecarga, False)
            ConfigurarCampoEntrada(TxtNombre, True)
            TxtNombre.BorderStyle = BorderStyle.FixedSingle
            TxtNombre.BackColor = Color.FromArgb(244, 247, 250)
            TxtRecarga.TextAlign = HorizontalAlignment.Right
            TxtPrimerApellido.Visible = False
            TxtSegundoApellido.Visible = False
            PicUsuario.Visible = False

            LblCedulaBusqueda.Text = "Documento / carnet"
            LblNombreBusqueda.Text = "Nombre completo"
            LblCantidadRecarga.Text = "Cantidad de tiquetes"
            Label5.Text = "Tipo de usuario"
            Label9.Text = "Beca"
            Label8.Text = "Saldo actual"
            Label7.Text = "Total a cobrar"

            For Each caption As Label In New Label() {LblCedulaBusqueda, LblNombreBusqueda, LblCantidadRecarga, Label5, Label9, Label8, Label7}
                caption.AutoSize = False
                caption.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
                caption.ForeColor = UIConstants.TextSecondary
                caption.BackColor = Color.Transparent
            Next

            EstilizarChip(LblTipoUsuario, UIConstants.Accent, Color.White)
            EstilizarChip(LblTipoBeca, Color.FromArgb(241, 245, 249), UIConstants.TextPrimary)
            EstilizarTarjetaMetrica(LblCantTiques, Color.FromArgb(241, 245, 249), UIConstants.TextPrimary, 22.0!)
            EstilizarTarjetaMetrica(LblTotal, Color.FromArgb(220, 252, 231), UIConstants.Success, 36.0!)
            BtnGuardar.Text = "Cobrar"

            PanelAcciones.BackColor = Color.Transparent
            PanelAcciones.BorderStyle = BorderStyle.None
            EnsurePointOfSaleControls()
            ActualizarResumenOperacion()
            LayoutPointOfSale()
        Finally
            Me.ResumeLayout(True)
        End Try
    End Sub

    Private Sub EnsurePointOfSaleControls()
        If _panelHeader Is Nothing Then
            _panelHeader = New Panel()
            _panelHeader.BackColor = Color.Transparent
            Me.Controls.Add(_panelHeader)
            _panelHeader.SendToBack()
        End If

        If _lblSubtitulo Is Nothing Then
            _lblSubtitulo = New Label()
            _lblSubtitulo.AutoSize = False
            _lblSubtitulo.Font = New Font("Segoe UI", 10.0!, FontStyle.Regular, GraphicsUnit.Point)
            _lblSubtitulo.ForeColor = UIConstants.TextSecondary
            _lblSubtitulo.BackColor = Color.Transparent
            _lblSubtitulo.Text = "Recarga rápida de tiquetes con resumen de cobro y saldo proyectado."
            Me.Controls.Add(_lblSubtitulo)
            _lblSubtitulo.BringToFront()
        End If

        If _lblAtajos Is Nothing Then
            _lblAtajos = New Label()
            _lblAtajos.AutoSize = False
            _lblAtajos.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold, GraphicsUnit.Point)
            _lblAtajos.ForeColor = UIConstants.TextSecondary
            _lblAtajos.BackColor = Color.Transparent
            _lblAtajos.TextAlign = ContentAlignment.MiddleRight
            _lblAtajos.Text = "F2 Buscar   Enter Validar   F8 Cobrar   Esc Limpiar"
            Me.Controls.Add(_lblAtajos)
            _lblAtajos.BringToFront()
        End If

        If _lblClienteSection Is Nothing Then
            _lblClienteSection = CrearEncabezadoSeccion("Cliente")
            Me.Controls.Add(_lblClienteSection)
            _lblClienteSection.BringToFront()
        End If

        If _lblRecargaSection Is Nothing Then
            _lblRecargaSection = CrearEncabezadoSeccion("Detalle de recarga")
            Me.Controls.Add(_lblRecargaSection)
            _lblRecargaSection.BringToFront()
        End If

        If _lblEstadoOperacion Is Nothing Then
            _lblEstadoOperacion = New Label()
            _lblEstadoOperacion.AutoSize = False
            _lblEstadoOperacion.Font = New Font("Segoe UI", 9.0!, FontStyle.Bold, GraphicsUnit.Point)
            _lblEstadoOperacion.Padding = New Padding(12, 0, 12, 0)
            _lblEstadoOperacion.TextAlign = ContentAlignment.MiddleLeft
            Me.Controls.Add(_lblEstadoOperacion)
            _lblEstadoOperacion.BringToFront()
        End If

        If _panelResumen Is Nothing Then
            _panelResumen = New Panel()
            _panelResumen.BackColor = Color.FromArgb(252, 253, 255)
            _panelResumen.BorderStyle = BorderStyle.None
            Me.Controls.Add(_panelResumen)
            _panelResumen.BringToFront()

            Dim lblTitulo As New Label()
            lblTitulo.Name = "LblResumenTitulo"
            lblTitulo.Text = "Resumen de cobro"
            lblTitulo.Font = UIConstants.FontSubtitle()
            lblTitulo.ForeColor = UIConstants.TextPrimary
            lblTitulo.AutoSize = False
            _panelResumen.Controls.Add(lblTitulo)

            Dim lblEstado As Label = CrearResumenEtiqueta("Estado de la operación")
            _panelResumen.Controls.Add(lblEstado)
            _lblResumenEstadoValor = CrearResumenValor(11.0!, True, UIConstants.TextPrimary)
            _panelResumen.Controls.Add(_lblResumenEstadoValor)

            Dim lblPrecio As Label = CrearResumenEtiqueta("Precio unitario")
            _panelResumen.Controls.Add(lblPrecio)
            _lblResumenPrecioValor = CrearResumenValor(14.0!, True, UIConstants.TextPrimary)
            _panelResumen.Controls.Add(_lblResumenPrecioValor)

            Dim lblSaldo As Label = CrearResumenEtiqueta("Saldo actual")
            _panelResumen.Controls.Add(lblSaldo)
            _lblResumenSaldoValor = CrearResumenValor(14.0!, True, UIConstants.TextPrimary)
            _panelResumen.Controls.Add(_lblResumenSaldoValor)

            Dim lblSaldoFinal As Label = CrearResumenEtiqueta("Saldo proyectado")
            _panelResumen.Controls.Add(lblSaldoFinal)
            _lblResumenSaldoFinalValor = CrearResumenValor(14.0!, True, UIConstants.TextPrimary)
            _panelResumen.Controls.Add(_lblResumenSaldoFinalValor)

            Dim lblTotal As Label = CrearResumenEtiqueta("Total")
            _panelResumen.Controls.Add(lblTotal)
            _lblResumenTotalValor = CrearResumenValor(22.0!, True, UIConstants.Success)
            _panelResumen.Controls.Add(_lblResumenTotalValor)
        End If
    End Sub

    Private Sub LayoutPointOfSale()
        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then
            Exit Sub
        End If

        If _lblAtajos Is Nothing OrElse _lblSubtitulo Is Nothing OrElse _lblEstadoOperacion Is Nothing OrElse _panelResumen Is Nothing Then
            Exit Sub
        End If

        Dim margin As Integer = 24
        Dim gap As Integer = 18
        Dim top As Integer = 16
        Dim panelResumenWidth As Integer = 284
        Dim contentWidth As Integer = Me.ClientSize.Width - (margin * 2)
        Dim leftWidth As Integer = Math.Max(700, contentWidth - panelResumenWidth - gap)
        Dim actionsHeight As Integer = 52
        Dim actionsTop As Integer = Me.ClientSize.Height - margin - actionsHeight
        Dim detailsTop As Integer
        Dim detailsHeight As Integer

        _panelHeader.SetBounds(margin - 6, top - 4, contentWidth + 12, 92)
        _panelHeader.BackColor = Color.FromArgb(250, 251, 253)

        LblTituloModulo.SetBounds(margin, top, Math.Min(460, leftWidth), 34)
        _lblAtajos.SetBounds(Me.ClientSize.Width - margin - 320, top, 320, 24)
        _lblSubtitulo.SetBounds(margin, LblTituloModulo.Bottom + 2, leftWidth, 20)
        _lblEstadoOperacion.SetBounds(margin, _lblSubtitulo.Bottom + 10, contentWidth, 34)

        GroupDatosUsuario.SetBounds(margin, _lblEstadoOperacion.Bottom + 16, contentWidth, 108)
        _lblClienteSection.SetBounds(margin + 18, GroupDatosUsuario.Top - 10, 160, 22)
        detailsTop = GroupDatosUsuario.Bottom + gap
        detailsHeight = Math.Max(280, actionsTop - detailsTop - gap)
        GroupDatosCompra.SetBounds(margin, GroupDatosUsuario.Bottom + gap, leftWidth, detailsHeight)
        _lblRecargaSection.SetBounds(margin + 18, GroupDatosCompra.Top - 10, 220, 22)
        _panelResumen.SetBounds(GroupDatosCompra.Right + gap, GroupDatosCompra.Top, panelResumenWidth, GroupDatosCompra.Height)

        LayoutGroupDatosUsuario()
        LayoutGroupDatosCompra()
        LayoutPanelResumen()

        PanelAcciones.SetBounds(Me.ClientSize.Width - margin - 472, actionsTop + 4, 472, 44)
        BtnGuardar.SetBounds(0, 0, 156, 38)
        BtnCancelar.SetBounds(164, 0, 146, 38)
        BtnRegresar.SetBounds(318, 0, 146, 38)
    End Sub

    Private Sub LayoutGroupDatosUsuario()
        Dim left As Integer = 24
        Dim top As Integer = 22
        Dim cedulaWidth As Integer = 320
        Dim buscarWidth As Integer = 112
        Dim rightLeft As Integer = left + cedulaWidth + buscarWidth + 24
        Dim nombreWidth As Integer = Math.Max(220, GroupDatosUsuario.ClientSize.Width - rightLeft - 24)

        LblCedulaBusqueda.SetBounds(left, top, cedulaWidth, 18)
        txtCedula.SetBounds(left, top + 24, cedulaWidth, 38)
        Buscar.SetBounds(txtCedula.Right + 10, txtCedula.Top, buscarWidth, 36)

        LblNombreBusqueda.SetBounds(rightLeft, top, nombreWidth, 18)
        TxtNombre.SetBounds(rightLeft, top + 24, nombreWidth, 38)
    End Sub

    Private Sub LayoutGroupDatosCompra()
        Dim margin As Integer = 24
        Dim gap As Integer = 18
        Dim columnWidth As Integer = Math.Max(230, (GroupDatosCompra.ClientSize.Width - (margin * 2) - gap) \ 2)
        Dim yTop As Integer = 22

        LblCantidadRecarga.SetBounds(margin, yTop, columnWidth, 18)
        TxtRecarga.SetBounds(margin, yTop + 26, columnWidth, 48)

        Label5.SetBounds(margin, yTop + 90, columnWidth, 18)
        LblTipoUsuario.SetBounds(margin, yTop + 114, columnWidth, 42)

        Label9.SetBounds(margin, yTop + 160, columnWidth, 18)
        LblTipoBeca.SetBounds(margin, yTop + 184, columnWidth, 42)

        Label8.SetBounds(margin + columnWidth + gap, yTop, columnWidth, 18)
        LblCantTiques.SetBounds(margin + columnWidth + gap, yTop + 26, columnWidth, 76)

        Label7.SetBounds(margin + columnWidth + gap, yTop + 118, columnWidth, 18)
        LblTotal.SetBounds(margin + columnWidth + gap, yTop + 142, columnWidth, 112)
    End Sub

    Private Sub LayoutPanelResumen()
        Dim currentTop As Integer = 20
        Dim innerLeft As Integer = 18
        Dim blockGap As Integer = 10
        Dim title As Label = DirectCast(_panelResumen.Controls("LblResumenTitulo"), Label)
        title.SetBounds(innerLeft, currentTop, _panelResumen.ClientSize.Width - (innerLeft * 2), 28)
        currentTop = title.Bottom + 14

        For i As Integer = 1 To _panelResumen.Controls.Count - 1 Step 2
            Dim lblCaption As Label = TryCast(_panelResumen.Controls(i), Label)
            Dim lblValue As Label = TryCast(_panelResumen.Controls(i + 1), Label)
            If lblCaption Is Nothing OrElse lblValue Is Nothing Then
                Continue For
            End If

            lblCaption.SetBounds(innerLeft, currentTop, _panelResumen.ClientSize.Width - (innerLeft * 2), 16)
            lblValue.SetBounds(innerLeft, lblCaption.Bottom + 4, _panelResumen.ClientSize.Width - (innerLeft * 2), If(lblValue Is _lblResumenTotalValor, 44, 26))
            currentTop = lblValue.Bottom + blockGap
        Next
    End Sub

    Private Sub ConfigurarCampoEntrada(ByVal tb As TextBox, ByVal isReadOnly As Boolean)
        tb.BorderStyle = BorderStyle.FixedSingle
        tb.BackColor = UIConstants.Surface
        tb.ForeColor = UIConstants.TextPrimary
        tb.Font = New Font("Segoe UI", If(isReadOnly, 11.0!, 12.0!), FontStyle.Regular, GraphicsUnit.Point)
        tb.ReadOnly = isReadOnly
    End Sub

    Private Sub ConfigurarBotonBusqueda()
        Buscar.Text = "Buscar"
        Buscar.BackgroundImage = Nothing
        Buscar.Image = Nothing
        Buscar.FlatStyle = FlatStyle.Flat
        Buscar.FlatAppearance.BorderColor = UIConstants.Border
        Buscar.FlatAppearance.BorderSize = 1
        Buscar.BackColor = UIConstants.Surface
        Buscar.ForeColor = UIConstants.TextPrimary
        Buscar.Font = UIConstants.FontBodyStrong()
        Buscar.Cursor = Cursors.Hand
        Buscar.TextAlign = ContentAlignment.MiddleCenter
        Buscar.Width = 112
    End Sub

    Private Sub EstilizarChip(ByVal lbl As Label, ByVal backColor As Color, ByVal foreColor As Color)
        lbl.BorderStyle = BorderStyle.None
        lbl.AutoSize = False
        lbl.BackColor = backColor
        lbl.ForeColor = foreColor
        lbl.Font = New Font("Segoe UI", 10.5!, FontStyle.Bold, GraphicsUnit.Point)
        lbl.Padding = New Padding(16, 0, 16, 0)
        lbl.TextAlign = ContentAlignment.MiddleLeft
    End Sub

    Private Sub EstilizarTarjetaMetrica(ByVal lbl As Label, ByVal backColor As Color, ByVal foreColor As Color, ByVal fontSize As Single)
        lbl.BorderStyle = BorderStyle.None
        lbl.AutoSize = False
        lbl.BackColor = backColor
        lbl.ForeColor = foreColor
        lbl.Font = New Font("Segoe UI Semibold", fontSize, FontStyle.Bold, GraphicsUnit.Point)
        lbl.Padding = New Padding(18, 0, 18, 0)
        lbl.TextAlign = ContentAlignment.MiddleLeft
    End Sub

    Private Function CrearResumenEtiqueta(ByVal texto As String) As Label
        Dim lbl As New Label()
        lbl.AutoSize = False
        lbl.Text = texto
        lbl.Font = New Font("Segoe UI", 8.5!, FontStyle.Bold, GraphicsUnit.Point)
        lbl.ForeColor = UIConstants.TextSecondary
        lbl.BackColor = Color.Transparent
        Return lbl
    End Function

    Private Function CrearEncabezadoSeccion(ByVal texto As String) As Label
        Dim lbl As New Label()
        lbl.AutoSize = False
        lbl.Text = texto
        lbl.Font = New Font("Segoe UI Semibold", 11.0!, FontStyle.Bold, GraphicsUnit.Point)
        lbl.ForeColor = UIConstants.TextPrimary
        lbl.BackColor = UIConstants.AppBackground
        Return lbl
    End Function

    Private Function CrearResumenValor(ByVal fontSize As Single, ByVal isBold As Boolean, ByVal foreColor As Color) As Label
        Dim lbl As New Label()
        lbl.AutoSize = False
        lbl.Font = New Font("Segoe UI", fontSize, If(isBold, FontStyle.Bold, FontStyle.Regular), GraphicsUnit.Point)
        lbl.ForeColor = foreColor
        lbl.BackColor = Color.Transparent
        Return lbl
    End Function

    Private Sub ActualizarResumenOperacion()
        Dim cantidad As Integer = ObtenerCantidadRecarga()
        Dim total As Decimal = cantidad * Precio
        Dim saldoProyectado As Integer = _saldoActual + cantidad

        LblCantTiques.Text = _saldoActual.ToString("N0", CultureInfo.InvariantCulture) & " tiquetes"
        LblTotal.Text = "₡ " & total.ToString("N2", CultureInfo.InvariantCulture)

        If _lblResumenEstadoValor IsNot Nothing Then
            _lblResumenEstadoValor.Text = If(TipoUsuarioCod > 0, "Cliente validado", "Pendiente de validar")
            _lblResumenPrecioValor.Text = If(Precio > 0D, "₡ " & Precio.ToString("N2", CultureInfo.InvariantCulture), "No configurado")
            _lblResumenSaldoValor.Text = _saldoActual.ToString("N0", CultureInfo.InvariantCulture) & " tiquetes"
            _lblResumenSaldoFinalValor.Text = saldoProyectado.ToString("N0", CultureInfo.InvariantCulture) & " tiquetes"
            _lblResumenTotalValor.Text = "₡ " & total.ToString("N2", CultureInfo.InvariantCulture)
        End If
    End Sub

    Private Sub ActualizarEstadoOperacion(ByVal mensaje As String, ByVal estado As EstadoOperacionVisual)
        If _lblEstadoOperacion Is Nothing Then
            Return
        End If

        _lblEstadoOperacion.Text = mensaje

        Select Case estado
            Case EstadoOperacionVisual.Exito
                _lblEstadoOperacion.BackColor = Color.FromArgb(220, 252, 231)
                _lblEstadoOperacion.ForeColor = UIConstants.Success
            Case EstadoOperacionVisual.Advertencia
                _lblEstadoOperacion.BackColor = Color.FromArgb(255, 247, 237)
                _lblEstadoOperacion.ForeColor = UIConstants.Warning
            Case EstadoOperacionVisual.ErrorCritico
                _lblEstadoOperacion.BackColor = Color.FromArgb(254, 226, 226)
                _lblEstadoOperacion.ForeColor = UIConstants.Danger
            Case Else
                _lblEstadoOperacion.BackColor = Color.FromArgb(239, 246, 255)
                _lblEstadoOperacion.ForeColor = UIConstants.Accent
        End Select
    End Sub

    Private Function ConstruirNombreCompleto(ByVal nombre As String, ByVal primerApellido As String, ByVal segundoApellido As String) As String
        Dim partes As String() = {nombre, primerApellido, segundoApellido}
        Return String.Join(" ", partes.Where(Function(p) Not String.IsNullOrWhiteSpace(p))).Trim()
    End Function

    Private Function ObtenerDescripcionBeca(ByVal idBeca As Integer) As String
        If _becas Is Nothing Then
            Return "Sin beca"
        End If

        For Each beca As DataRow In _becas.Rows
            If CInt(beca("IdBeca")) = idBeca Then
                Return CStr(beca("Descripcion"))
            End If
        Next

        Return "Sin beca"
    End Function

    Private Function ObtenerPrecioUnitario(ByVal codTipoUsuario As Integer) As Decimal
        If codTipoUsuario = 1 Then
            If _configParametros IsNot Nothing AndAlso _configParametros.PrecioEstudiante > 0D Then
                Return _configParametros.PrecioEstudiante
            End If
            Return PrecioEstudiante
        End If

        If _configParametros IsNot Nothing AndAlso _configParametros.PrecioDocente > 0D Then
            Return _configParametros.PrecioDocente
        End If

        Return PrecioDocente
    End Function

    Private Function NormalizarCedula(ByVal valor As String) As String
        Dim cedula As String = If(valor, String.Empty).Trim()
        Dim controlCarnetLocal As String = If(ControlCarnet, String.Empty)

        If controlCarnetLocal.Length > 0 Then
            cedula = cedula.Replace(controlCarnetLocal, String.Empty)
        End If

        cedula = cedula.Replace("CTPP", String.Empty)
        Return cedula.Trim()
    End Function

End Class
