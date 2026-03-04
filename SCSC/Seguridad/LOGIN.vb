Option Strict Off
Option Explicit On
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Partial Friend Class Login
    Private Const WM_NCLBUTTONDOWN As Integer = &HA1
    Private Const HTCAPTION As Integer = 2
    Private Const EnableForceOpenKey As String = "EnableForceOpen"
    Private Const PlaceholderUser As String = "Ingrese usuario"
    Private Const PlaceholderPassword As String = "Ingrese contraseña"
    Private Const RightBlockLeft As Integer = 560
    Private Const RightBlockMargin As Integer = 64
    Private Const RightBlockMaxWidth As Integer = 620
    Private Const RightBlockMinWidth As Integer = 480
    Private Const PasswordToggleGap As Integer = 10
    Private Const OverlayOpacity As Single = 0.28F

    Private Const DefaultAdminUser As String = "admin"
    Private Const DefaultAdminSupportPassword As String = "System10."
    Private Cls As FuncionesDB
    Private Cn As SqlClient.SqlConnection
    Private DsParametro As DataSet
    Private SeguridadService As SeguridadRbacService

    Private Verificado As Boolean = False
    Private LoginStartTime As DateTime
    Private AllowClose As Boolean = False
    Private IsUserPlaceholderActive As Boolean = False
    Private IsPasswordPlaceholderActive As Boolean = False
    Private IsPasswordVisible As Boolean = False

    Private Sub ClavePaso_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Enter
        If IsPasswordPlaceholderActive Then
            IsPasswordPlaceholderActive = False
            ClavePaso.Text = ""
            ClavePaso.ForeColor = SystemColors.WindowText
            ClavePaso.PasswordChar = If(IsPasswordVisible, ControlChars.NullChar, "*"c)
        End If
        SetFieldFocusState(SeparatorClave, True)
        ClavePaso.SelectAll()
    End Sub

    Private Sub ClavePaso_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles ClavePaso.Validated
        SetFieldFocusState(SeparatorClave, False)
        ClavePaso.Text = ClavePaso.Text.Trim()
        If ClavePaso.Text.Length = 0 Then
            ApplyPasswordPlaceholder()
        End If
    End Sub

    Private Sub CodUsuario_Enter(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Enter
        If IsUserPlaceholderActive Then
            IsUserPlaceholderActive = False
            CodUsuario.Text = ""
            CodUsuario.ForeColor = SystemColors.WindowText
        End If
        SetFieldFocusState(SeparatorUsuario, True)
        CodUsuario.SelectAll()
    End Sub

    Private Sub CodUsuario_Leave(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles CodUsuario.Validated
        SetFieldFocusState(SeparatorUsuario, False)
        EnsureRuntimeState()
        CodUsuario.Text = CodUsuario.Text.Trim()
        If Len(CodUsuario.Text.Trim()) = 0 Then
            ApplyUserPlaceholder()
            Exit Sub
        End If

        If Len(CodUsuario.Text.Trim()) <> 0 Then
            Try
                Dim dsFecha As DataSet = Cls.ConsultarTSQL("Fecha", "Select GETDATE() as Fecha")
                If dsFecha.Tables(0).Rows.Count > 0 Then
                    FechaServer = CDate(dsFecha.Tables(0).Rows(0)!Fecha).Date
                End If
            Catch ex As Exception
                ErrorLogger.LogException("Login.CodUsuario_Leave", ex)
                MsgBox(ex.Message, MsgBoxStyle.Critical)
            End Try
        End If
    End Sub

    Private Sub Login_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
        If IsInDesignMode() Then
            Exit Sub
        End If

        EnsureRuntimeState()
        LoginStartTime = DateTime.Now
        ErrorLogger.LogInfo("Login_Load", "Iniciando carga de Login.")
        UIThemeManagerV2.Apply(Me, "login")
        ChkOpen.Visible = IsDebugOrDevMode()
        Me.AcceptButton = BtnLogin
        InitializeInputState()
        BtnLogin.Image = CreateMonochromeIcon(SystemIcons.Shield.ToBitmap(), 20, 20, Color.White)
        BtnLogin.ImageAlign = ContentAlignment.MiddleLeft
        BtnLogin.Padding = New Padding(12, 0, 12, 0)
        BtnComedor.Image = ResolveModuleIcon(BtnComedor.Image, SystemIcons.Information.ToBitmap())
        BtnTransporte.Image = ResolveModuleIcon(BtnTransporte.Image, SystemIcons.Application.ToBitmap())
        BtnTogglePassword.Image = CreateMonochromeIcon(SystemIcons.Information.ToBitmap(), 16, 16, Color.FromArgb(67, 82, 99))
        BtnTogglePassword.Text = ""
        BtnTogglePassword.TabStop = False
        ApplyPanelOverlay()
        ApplyResponsiveRightBlockLayout()
        ' Designer-first: el layout y estilo del Login se controla desde LOGIN.designer.vb
        Me.MinimumSize = Me.Size

        Try
            Cls.AbrirConexion(Cn, False)
            Dim Valores(), Llave() As FuncionesDB.Campos
            Valores = Cls.InicializarArray
            Llave = Cls.InicializarArray
            Cls.ArmaValor(Valores, "Id")
            Cls.ArmaValor(Valores, "Institucion")
            Cls.ArmaValor(Valores, "CodPresupuestario")
            Cls.ArmaValor(Valores, "Ubicacion")
            Cls.ArmaValor(Valores, "Leyenda")
            Cls.ArmaValor(Valores, "ControlCarnet")
            Cls.ArmaValor(Valores, "PrecioDocente")
            Cls.ArmaValor(Valores, "PrecioEstudiante")
            Cls.ArmaValor(Valores, "Getdate() as Fecha")
            DsParametro = Cls.Consultar("Parametro", Valores, Llave, Cn)
            If DsParametro.Tables(0).Rows.Count > 0 Then
                CodVersion = My.Application.Info.Version.ToString
                LlaveIncriptacion = GetAppConfig("LlaveEncriptacion")
                NomColegio = DsParametro.Tables(0).Rows(0)!Institucion
                CodColegio = DsParametro.Tables(0).Rows(0)!CodPresupuestario
                Ubicacion = DsParametro.Tables(0).Rows(0)!Ubicacion
                Leyenda = DsParametro.Tables(0).Rows(0)!Leyenda.Replace("{CODIGO}", CodColegio)
                ControlCarnet = DsParametro.Tables(0).Rows(0)!ControlCarnet
                PrecioDocente = DsParametro.Tables(0).Rows(0)!PrecioDocente
                PrecioEstudiante = DsParametro.Tables(0).Rows(0)!PrecioEstudiante
                Version.Text = CodVersion
                LblInstitucion.Text = NomColegio
                LbFecha.Text = Format(DsParametro.Tables(0).Rows(0)!Fecha, "dd/MMM/yyyy")
                ApplyResponsiveRightBlockLayout()
                DiaSemana = Weekday(DsParametro.Tables(0).Rows(0)!Fecha).ToString
            Else
                Throw New Exception("No se encontarón los parametros de la aplicación. Imposible Iniciar !0x000020")
            End If
            Cls.CerrarConexion(Cn)
            ErrorLogger.LogInfo("Login_Load", "Login cargado correctamente.")
        Catch ex As Exception
            If Cn.State = ConnectionState.Open Then
                Cls.CerrarConexion(Cn)
            End If
            ErrorLogger.LogException("Login_Load", ex)
            MsgBox("Error al cargar Login: " & ex.Message, MsgBoxStyle.Critical)
            MsgBox("Revise el log: " & ErrorLogger.GetCurrentLogPath(), MsgBoxStyle.Information)
            Verificado = False
            AllowClose = True
            Me.DialogResult = DialogResult.Cancel
            Me.Close()
            Exit Sub
        End Try
    End Sub

    Private Sub Salir_ClickEvent(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles BtnCerrar.Click
        ErrorLogger.LogInfo("Login.BtnCerrar", "Click en cerrar login.")
        AllowClose = True
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub BtnAceptar_Click(sender As Object, e As EventArgs) Handles BtnLogin.Click
        Dim usuarioIngreso As String = GetUserInputValue()
        ErrorLogger.LogInfo("Login.BtnLogin", "Intento de ingreso. Usuario='" & usuarioIngreso & "'")
        If ChkOpen.Checked Then
            CodigoUsuario = usuarioIngreso
            Verificado = True
            AllowClose = True
            Me.DialogResult = DialogResult.OK
            Me.Close()
            Exit Sub
        End If

        If Len(usuarioIngreso) = 0 Then
            MsgBox("Debe indicar el usuario.", MsgBoxStyle.Exclamation)
            CodUsuario.Focus()
            Exit Sub
        End If

        If Len(GetPasswordInputValue()) = 0 Then
            MsgBox("Debe indicar la contraseña.", MsgBoxStyle.Exclamation)
            ClavePaso.Focus()
            Exit Sub
        End If

        If AutenticarUsuario() Then
            CodigoUsuario = usuarioIngreso
            Verificado = True
            AllowClose = True
            Me.DialogResult = DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Function AutenticarUsuario() As Boolean
        EnsureRuntimeState()
        Dim usuarioIngresado As String = GetUserInputValue()
        Dim contrasenaIngresada As String = GetPasswordInputValue()

        If String.Equals(usuarioIngresado, ObtenerUsuarioAdmin(), StringComparison.OrdinalIgnoreCase) AndAlso
           String.Equals(contrasenaIngresada, ObtenerClaveSoporteAdmin(), StringComparison.Ordinal) Then
            Return True
        End If

        Try
            Dim usuario As DataRow = SeguridadService.ObtenerUsuarioPorNombre(usuarioIngresado)
            If usuario Is Nothing Then
                SeguridadService.RegistrarLoginFallido(usuarioIngresado, Nothing, "Usuario no registrado.", "LOCAL")
                MsgBox("Usuario o contraseña inválida.", MsgBoxStyle.Critical)
                Return False
            End If

            If Not CBool(usuario("EsActivo")) Then
                SeguridadService.RegistrarLoginFallido(usuarioIngresado, CInt(usuario("IdUsuario")), "Usuario inactivo.", "LOCAL")
                MsgBox("El usuario está inactivo.", MsgBoxStyle.Exclamation)
                Return False
            End If

            Dim mensajeBloqueo As String = ""
            If SeguridadService.EstaBloqueado(usuario, mensajeBloqueo) Then
                SeguridadService.RegistrarLoginFallido(usuarioIngresado, CInt(usuario("IdUsuario")), "Usuario bloqueado.", "LOCAL")
                MsgBox(mensajeBloqueo, MsgBoxStyle.Exclamation)
                Return False
            End If

            Dim hashGuardado As String = CStr(usuario("HashContrasena"))
            Dim saltGuardada As String = CStr(usuario("SaltContrasena"))
            If Not SeguridadService.ValidarContrasena(contrasenaIngresada, hashGuardado, saltGuardada) Then
                SeguridadService.RegistrarLoginFallido(usuarioIngresado, CInt(usuario("IdUsuario")), "Credenciales inválidas.", "LOCAL")
                MsgBox("Usuario o contraseña inválida.", MsgBoxStyle.Critical)
                Return False
            End If

            SeguridadService.RegistrarLoginCorrecto(CInt(usuario("IdUsuario")), "LOCAL")
            NombreUsuario = CStr(usuario("NombreCompleto"))
            Return True
        Catch ex As Exception
            ErrorLogger.LogException("Login.AutenticarUsuario", ex, "Usuario=" & usuarioIngresado)
            MsgBox("Error en autenticación: " & ex.Message, MsgBoxStyle.Critical)
            MsgBox("Revise el log: " & ErrorLogger.GetCurrentLogPath(), MsgBoxStyle.Information)
            Return False
        End Try
    End Function

    Private Sub BtnComedor_Click(sender As Object, e As EventArgs) Handles BtnComedor.Click
        UIThemeManagerV2.Apply(ControlComedor, "operativo")
        ControlComedor.ShowDialog()
    End Sub

    Private Sub BtnTransporte_Click(sender As Object, e As EventArgs) Handles BtnTransporte.Click
        UIThemeManagerV2.Apply(ControlTransporte, "operativo")
        ControlTransporte.ShowDialog()
    End Sub

    Private Sub ClavePaso_KeyDown_1(sender As Object, e As KeyEventArgs) Handles ClavePaso.KeyDown
        If e.KeyCode = Keys.Enter Then
            BtnLogin.PerformClick()
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub CodUsuario_KeyPress(sender As Object, e As KeyEventArgs) Handles CodUsuario.KeyDown
        If e.KeyCode = Keys.Enter Then
            ClavePaso.Focus()
        End If
    End Sub

    Private Sub Login_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If e.KeyCode = Keys.Escape Then
            Salir_ClickEvent(BtnCerrar, EventArgs.Empty)
        End If
    End Sub

    Private Sub DragSurface_MouseDown(sender As Object, e As MouseEventArgs) Handles MyBase.MouseDown, Panel1.MouseDown, Label1.MouseDown, Label2.MouseDown, Label3.MouseDown
        If e.Button <> MouseButtons.Left Then
            Exit Sub
        End If
        ReleaseCapture()
        SendMessage(Me.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0)
    End Sub

    Private Sub BtnTogglePassword_Click(sender As Object, e As EventArgs) Handles BtnTogglePassword.Click
        If IsPasswordPlaceholderActive Then
            Exit Sub
        End If

        IsPasswordVisible = Not IsPasswordVisible
        ClavePaso.PasswordChar = If(IsPasswordVisible, ControlChars.NullChar, "*"c)
        BtnTogglePassword.Image = CreateMonochromeIcon(
            If(IsPasswordVisible, SystemIcons.Warning.ToBitmap(), SystemIcons.Information.ToBitmap()),
            16,
            16,
            Color.FromArgb(67, 82, 99))
    End Sub

    Private Sub CodUsuario_TextChanged(sender As Object, e As EventArgs) Handles CodUsuario.TextChanged
        UpdateLoginButtonState()
    End Sub

    Private Sub ClavePaso_TextChanged(sender As Object, e As EventArgs) Handles ClavePaso.TextChanged
        UpdateLoginButtonState()
    End Sub

    Private Sub ChkOpen_CheckedChanged(sender As Object, e As EventArgs) Handles ChkOpen.CheckedChanged
        UpdateLoginButtonState()
    End Sub

    Private Function ObtenerAppSettingConDefault(ByVal key As String, ByVal defaultValue As String) As String
        Try
            Dim value As String = System.Configuration.ConfigurationManager.AppSettings(key)
            If String.IsNullOrWhiteSpace(value) Then
                Return defaultValue
            End If
            Return value.Trim()
        Catch
            Return defaultValue
        End Try
    End Function

    Private Function IsDebugOrDevMode() As Boolean
#If DEBUG Then
        Return True
#Else
        Return String.Equals(ObtenerAppSettingConDefault(EnableForceOpenKey, "false"), "true", StringComparison.OrdinalIgnoreCase)
#End If
    End Function

    Private Sub InitializeInputState()
        ApplyUserPlaceholder()
        ApplyPasswordPlaceholder()
        SetFieldFocusState(SeparatorUsuario, False)
        SetFieldFocusState(SeparatorClave, False)
        UpdateLoginButtonState()
    End Sub

    Private Sub ApplyUserPlaceholder()
        IsUserPlaceholderActive = True
        CodUsuario.ForeColor = Color.FromArgb(138, 149, 161)
        CodUsuario.Text = PlaceholderUser
    End Sub

    Private Sub ApplyPasswordPlaceholder()
        IsPasswordPlaceholderActive = True
        IsPasswordVisible = False
        ClavePaso.PasswordChar = ControlChars.NullChar
        ClavePaso.ForeColor = Color.FromArgb(138, 149, 161)
        ClavePaso.Text = PlaceholderPassword
        BtnTogglePassword.Image = CreateMonochromeIcon(SystemIcons.Information.ToBitmap(), 16, 16, Color.FromArgb(67, 82, 99))
    End Sub

    Private Function GetUserInputValue() As String
        If IsUserPlaceholderActive Then
            Return ""
        End If
        Return CodUsuario.Text.Trim()
    End Function

    Private Function GetPasswordInputValue() As String
        If IsPasswordPlaceholderActive Then
            Return ""
        End If
        Return ClavePaso.Text.Trim()
    End Function

    Private Sub SetFieldFocusState(ByVal separator As Panel, ByVal hasFocus As Boolean)
        If separator Is Nothing Then
            Exit Sub
        End If
        separator.Height = If(hasFocus, 2, 1)
        separator.BackColor = If(hasFocus, Color.FromArgb(24, 119, 242), Color.FromArgb(205, 216, 230))
    End Sub

    Private Sub UpdateLoginButtonState()
        Dim canLogin As Boolean = (Len(GetUserInputValue()) > 0 AndAlso Len(GetPasswordInputValue()) > 0) OrElse ChkOpen.Checked
        BtnLogin.Enabled = canLogin
        If canLogin Then
            BtnLogin.BackColor = Color.FromArgb(24, 119, 242)
            BtnLogin.FlatAppearance.BorderColor = Color.FromArgb(24, 119, 242)
        Else
            BtnLogin.BackColor = Color.FromArgb(168, 180, 194)
            BtnLogin.FlatAppearance.BorderColor = Color.FromArgb(168, 180, 194)
        End If
    End Sub

    Private Function ObtenerUsuarioAdmin() As String
        Return ObtenerAppSettingConDefault("AdminUsuario", DefaultAdminUser)
    End Function

    Private Function ObtenerClaveSoporteAdmin() As String
        Return ObtenerAppSettingConDefault("AdminClaveSoporte", DefaultAdminSupportPassword)
    End Function

    Private Sub Login_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If IsInDesignMode() Then
            Exit Sub
        End If
        ApplyResponsiveRightBlockLayout()
    End Sub

    Private Sub Login_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If IsInDesignMode() Then
            Exit Sub
        End If

        Dim elapsedMs As Long = 0
        If LoginStartTime <> DateTime.MinValue Then
            elapsedMs = CLng((DateTime.Now - LoginStartTime).TotalMilliseconds)
        End If

        ErrorLogger.LogInfo(
            "Login.FormClosing",
            "Reason=" & e.CloseReason.ToString() &
            ", DialogResult=" & Me.DialogResult.ToString() &
            ", AllowClose=" & AllowClose.ToString() &
            ", Verificado=" & Verificado.ToString() &
            ", ElapsedMs=" & elapsedMs.ToString())

        If Not AllowClose Then
            e.Cancel = True
            ErrorLogger.LogInfo("Login.FormClosing", "Cierre bloqueado: intento no autorizado.")
            Exit Sub
        End If

        If Not Verificado Then
            Me.DialogResult = DialogResult.Cancel
        End If
    End Sub

    Private Sub EnsureRuntimeState()
        If Cls Is Nothing Then
            Cls = New FuncionesDB()
        End If

        If Cn Is Nothing Then
            Cn = New SqlClient.SqlConnection()
        End If

        If DsParametro Is Nothing Then
            DsParametro = New DataSet()
        End If

        If SeguridadService Is Nothing Then
            SeguridadService = New SeguridadRbacService()
        End If
    End Sub

    Private Function IsInDesignMode() As Boolean
        If String.Equals(System.Diagnostics.Process.GetCurrentProcess().ProcessName, "devenv", StringComparison.OrdinalIgnoreCase) Then
            Return True
        End If

        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Designtime Then
            Return True
        End If

        If Not Me.Site Is Nothing AndAlso Me.Site.DesignMode Then
            Return True
        End If

        Return False
    End Function

    Private Function ResizeButtonIcon(ByVal source As Image, ByVal targetWidth As Integer, ByVal targetHeight As Integer) As Image
        If source Is Nothing OrElse targetWidth <= 0 OrElse targetHeight <= 0 Then
            Return source
        End If

        Dim bmp As New Bitmap(targetWidth, targetHeight)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.InterpolationMode = InterpolationMode.HighQualityBicubic
            g.SmoothingMode = SmoothingMode.AntiAlias
            g.PixelOffsetMode = PixelOffsetMode.HighQuality
            g.DrawImage(source, New Rectangle(0, 0, targetWidth, targetHeight))
        End Using
        Return bmp
    End Function

    Private Function ResolveModuleIcon(ByVal source As Image, ByVal fallback As Image) As Image
        Dim baseImage As Image = If(source, fallback)
        Return CreateMonochromeIcon(baseImage, 72, 72, Color.FromArgb(24, 73, 120))
    End Function

    Private Function CreateMonochromeIcon(ByVal source As Image, ByVal targetWidth As Integer, ByVal targetHeight As Integer, ByVal tint As Color) As Image
        If source Is Nothing OrElse targetWidth <= 0 OrElse targetHeight <= 0 Then
            Return source
        End If

        Dim baseBmp As Bitmap = DirectCast(ResizeButtonIcon(source, targetWidth, targetHeight), Bitmap)
        For y As Integer = 0 To baseBmp.Height - 1
            For x As Integer = 0 To baseBmp.Width - 1
                Dim px As Color = baseBmp.GetPixel(x, y)
                If px.A > 0 Then
                    baseBmp.SetPixel(x, y, Color.FromArgb(px.A, tint.R, tint.G, tint.B))
                End If
            Next
        Next
        Return baseBmp
    End Function

    Private Sub ApplyPanelOverlay()
        If Panel1.BackgroundImage Is Nothing Then
            Exit Sub
        End If

        Dim base As New Bitmap(Panel1.BackgroundImage)
        Dim result As New Bitmap(base.Width, base.Height)

        Using g As Graphics = Graphics.FromImage(result)
            g.DrawImage(base, 0, 0, base.Width, base.Height)
            Using overlayBrush As New SolidBrush(Color.FromArgb(CInt(255 * OverlayOpacity), 0, 0, 0))
                g.FillRectangle(overlayBrush, 0, 0, base.Width, base.Height)
            End Using
        End Using

        Panel1.BackgroundImage = result
        base.Dispose()
    End Sub

    Private Sub ApplyResponsiveRightBlockLayout()
        Dim available As Integer = Me.ClientSize.Width - RightBlockLeft - RightBlockMargin
        Dim contentWidth As Integer = Math.Max(RightBlockMinWidth, Math.Min(RightBlockMaxWidth, available))

        Label1.Left = RightBlockLeft
        Label3.Left = RightBlockLeft
        Label3.Width = contentWidth
        LblUsuarioCaption.Left = RightBlockLeft
        LblClaveCaption.Left = RightBlockLeft

        CodUsuario.Left = RightBlockLeft
        CodUsuario.Width = contentWidth
        SeparatorUsuario.Left = RightBlockLeft
        SeparatorUsuario.Width = contentWidth

        BtnTogglePassword.Top = ClavePaso.Top
        BtnTogglePassword.Left = RightBlockLeft + contentWidth - BtnTogglePassword.Width
        ClavePaso.Left = RightBlockLeft
        ClavePaso.Width = Math.Max(320, contentWidth - BtnTogglePassword.Width - PasswordToggleGap)

        SeparatorClave.Left = RightBlockLeft
        SeparatorClave.Width = contentWidth

        BtnLogin.Left = RightBlockLeft + contentWidth - BtnLogin.Width
        LbFecha.Left = RightBlockLeft
        LbFecha.Top = Math.Max(0, Me.ClientSize.Height - LbFecha.Height - 20)
    End Sub

    <DllImport("user32.dll")>
    Private Shared Function ReleaseCapture() As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Integer, ByVal wParam As Integer, ByVal lParam As Integer) As Integer
    End Function
End Class
