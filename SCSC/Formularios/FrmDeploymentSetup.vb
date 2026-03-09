Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Windows.Forms

Public Class FrmDeploymentSetup
    Inherits Form

    Private ReadOnly _setupOnlyMode As Boolean
    Private ReadOnly _txtServer As New TextBox()
    Private ReadOnly _txtDatabase As New TextBox()
    Private ReadOnly _chkWindowsAuth As New CheckBox()
    Private ReadOnly _txtUser As New TextBox()
    Private ReadOnly _txtPassword As New TextBox()
    Private ReadOnly _txtRequestCode As New TextBox()
    Private ReadOnly _txtActivationCode As New TextBox()
    Private ReadOnly _txtCustomer As New TextBox()
    Private ReadOnly _txtSite As New TextBox()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnTest As New Button()
    Private ReadOnly _btnSave As New Button()
    Private ReadOnly _btnCancel As New Button()
    Private ReadOnly _btnCopyRequest As New Button()
    Private _connectionValidated As Boolean
    Private _hasValidLicense As Boolean

    Public Sub New(ByVal setupOnlyMode As Boolean)
        _setupOnlyMode = setupOnlyMode
        InitializeLayout()
        LoadExistingState()
    End Sub

    Private Sub InitializeLayout()
        Me.Text = "Configuración inicial SCSC"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ClientSize = New Size(760, 700)
        Me.Font = New Font("Segoe UI", 9.5!, FontStyle.Regular, GraphicsUnit.Point)

        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 2
        layout.RowCount = 1
        layout.Padding = New Padding(16)
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 52.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 48.0F))
        Me.Controls.Add(layout)

        Dim dbGroup As New GroupBox()
        dbGroup.Text = "Base de datos"
        dbGroup.Dock = DockStyle.Fill
        dbGroup.Padding = New Padding(12)
        layout.Controls.Add(dbGroup, 0, 0)

        Dim licenseGroup As New GroupBox()
        licenseGroup.Text = "Licenciamiento offline"
        licenseGroup.Dock = DockStyle.Fill
        licenseGroup.Padding = New Padding(12)
        layout.Controls.Add(licenseGroup, 1, 0)

        Dim dbLayout As TableLayoutPanel = CreateSectionLayout()
        dbGroup.Controls.Add(dbLayout)
        AddField(dbLayout, "Servidor SQL", _txtServer)
        AddField(dbLayout, "Base de datos", _txtDatabase)

        _chkWindowsAuth.Text = "Usar autenticación de Windows"
        _chkWindowsAuth.AutoSize = True
        AddHandler _chkWindowsAuth.CheckedChanged, AddressOf OnWindowsAuthChanged
        dbLayout.Controls.Add(_chkWindowsAuth, 0, dbLayout.RowCount)
        dbLayout.SetColumnSpan(_chkWindowsAuth, 2)
        dbLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        dbLayout.RowCount += 1

        AddField(dbLayout, "Usuario SQL", _txtUser)
        AddField(dbLayout, "Contraseña SQL", _txtPassword)
        _txtPassword.PasswordChar = "*"c

        _btnTest.Text = "Probar conexión"
        _btnTest.Width = 150
        AddHandler _btnTest.Click, AddressOf OnTestConnection
        dbLayout.Controls.Add(_btnTest, 1, dbLayout.RowCount)
        dbLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        dbLayout.RowCount += 1

        Dim licenseLayout As TableLayoutPanel = CreateSectionLayout()
        licenseGroup.Controls.Add(licenseLayout)
        AddField(licenseLayout, "Cliente", _txtCustomer)
        AddField(licenseLayout, "Sede", _txtSite)
        AddHandler _txtCustomer.TextChanged, AddressOf OnLicenseRequestContextChanged
        AddHandler _txtSite.TextChanged, AddressOf OnLicenseRequestContextChanged

        _txtRequestCode.Multiline = True
        _txtRequestCode.ScrollBars = ScrollBars.Vertical
        _txtRequestCode.ReadOnly = True
        _txtRequestCode.Height = 120
        AddField(licenseLayout, "Request Code", _txtRequestCode)

        _btnCopyRequest.Text = "Copiar request"
        _btnCopyRequest.Width = 150
        AddHandler _btnCopyRequest.Click, AddressOf OnCopyRequest
        licenseLayout.Controls.Add(_btnCopyRequest, 1, licenseLayout.RowCount)
        licenseLayout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        licenseLayout.RowCount += 1

        _txtActivationCode.Multiline = True
        _txtActivationCode.ScrollBars = ScrollBars.Vertical
        _txtActivationCode.Height = 180
        AddField(licenseLayout, "Código de activación", _txtActivationCode)

        Dim footer As New Panel()
        footer.Dock = DockStyle.Bottom
        footer.Height = 60
        footer.Padding = New Padding(16, 0, 16, 16)
        Me.Controls.Add(footer)

        _lblStatus.AutoSize = False
        _lblStatus.Dock = DockStyle.Left
        _lblStatus.Width = 470
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft
        footer.Controls.Add(_lblStatus)

        _btnSave.Text = "Guardar y continuar"
        _btnSave.Width = 170
        _btnSave.Height = 34
        _btnSave.Anchor = AnchorStyles.Right Or AnchorStyles.Top
        _btnSave.Left = Me.ClientSize.Width - 360
        _btnSave.Top = 10
        AddHandler _btnSave.Click, AddressOf OnSaveAndContinue
        footer.Controls.Add(_btnSave)

        _btnCancel.Text = If(_setupOnlyMode, "Cerrar", "Cancelar")
        _btnCancel.Width = 120
        _btnCancel.Height = 34
        _btnCancel.Anchor = AnchorStyles.Right Or AnchorStyles.Top
        _btnCancel.Left = Me.ClientSize.Width - 180
        _btnCancel.Top = 10
        AddHandler _btnCancel.Click, AddressOf OnCancel
        footer.Controls.Add(_btnCancel)
    End Sub

    Private Function CreateSectionLayout() As TableLayoutPanel
        Dim layout As New TableLayoutPanel()
        layout.Dock = DockStyle.Fill
        layout.ColumnCount = 2
        layout.RowCount = 0
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 36.0F))
        layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 64.0F))
        Return layout
    End Function

    Private Sub AddField(ByVal layout As TableLayoutPanel, ByVal labelText As String, ByVal control As Control)
        Dim label As New Label()
        label.Text = labelText
        label.AutoSize = True
        label.Margin = New Padding(0, 8, 0, 0)

        control.Dock = DockStyle.Top
        control.Margin = New Padding(0, 4, 0, 0)

        layout.Controls.Add(label, 0, layout.RowCount)
        layout.Controls.Add(control, 1, layout.RowCount)
        layout.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        layout.RowCount += 1
    End Sub

    Private Sub LoadExistingState()
        Dim config As DeploymentConfiguration = Nothing
        Dim errorMessage As String = String.Empty
        If DeploymentConfigService.TryLoad(config, errorMessage) AndAlso config IsNot Nothing Then
            _txtServer.Text = config.Server
            _txtDatabase.Text = config.Database
            _chkWindowsAuth.Checked = config.UseIntegratedSecurity()
            _txtUser.Text = config.UserName
            _txtPassword.Text = config.GetPlainTextPassword()
            _connectionValidated = DeploymentConfigService.TestConnection(config, errorMessage)
        End If

        Dim payload As LicensePayload = Nothing
        If LicenseService.ValidateInstalledLicense(errorMessage, payload) AndAlso payload IsNot Nothing Then
            _hasValidLicense = True
            _txtCustomer.Text = payload.CustomerName
            _txtSite.Text = payload.SiteName
            _txtActivationCode.Text = LicenseService.GetInstalledLicenseSummary()
            _txtActivationCode.ReadOnly = True
            SetStatus("Licencia instalada: " & LicenseService.GetInstalledLicenseSummary(), False)
        Else
            _hasValidLicense = False
            SetStatus("Debe validar conexión SQL y cargar una licencia offline antes de continuar.", True)
        End If

        RefreshRequestCode()
        OnWindowsAuthChanged(Me, EventArgs.Empty)
    End Sub

    Private Sub OnWindowsAuthChanged(sender As Object, e As EventArgs)
        Dim enabled As Boolean = Not _chkWindowsAuth.Checked
        _txtUser.Enabled = enabled
        _txtPassword.Enabled = enabled
        If Not enabled Then
            _txtUser.Text = String.Empty
            _txtPassword.Text = String.Empty
        End If
        _connectionValidated = False
    End Sub

    Private Sub OnTestConnection(sender As Object, e As EventArgs)
        Dim config As DeploymentConfiguration = BuildConfigurationFromInputs()
        Dim message As String = String.Empty
        _connectionValidated = DeploymentConfigService.TestConnection(config, message)
        SetStatus(message, Not _connectionValidated)
    End Sub

    Private Sub OnCopyRequest(sender As Object, e As EventArgs)
        RefreshRequestCode()
        If _txtRequestCode.TextLength = 0 Then
            SetStatus("No se pudo generar el request code.", True)
            Exit Sub
        End If

        Clipboard.SetText(_txtRequestCode.Text)
        SetStatus("Request code copiado al portapapeles.", False)
    End Sub

    Private Sub OnSaveAndContinue(sender As Object, e As EventArgs)
        Dim config As DeploymentConfiguration = BuildConfigurationFromInputs()
        Dim message As String = String.Empty

        If Not _connectionValidated Then
            _connectionValidated = DeploymentConfigService.TestConnection(config, message)
            If Not _connectionValidated Then
                SetStatus(message, True)
                Exit Sub
            End If
        End If

        If Not DeploymentConfigService.Save(config, message) Then
            SetStatus(message, True)
            Exit Sub
        End If

        If Not _hasValidLicense Then
            If Not LicenseService.TryImportActivationCode(_txtActivationCode.Text, message) Then
                SetStatus(message, True)
                Exit Sub
            End If
            _hasValidLicense = True
        End If

        SetStatus("Configuración y licencia guardadas correctamente.", False)
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub OnCancel(sender As Object, e As EventArgs)
        Me.DialogResult = DialogResult.Cancel
        Me.Close()
    End Sub

    Private Function BuildConfigurationFromInputs() As DeploymentConfiguration
        Return DeploymentConfigService.BuildConfiguration(
            _txtServer.Text,
            _txtDatabase.Text,
            _chkWindowsAuth.Checked,
            _txtUser.Text,
            _txtPassword.Text)
    End Function

    Private Sub RefreshRequestCode()
        _txtRequestCode.Text = LicenseService.BuildRequestCode(_txtCustomer.Text, _txtSite.Text)
    End Sub

    Private Sub OnLicenseRequestContextChanged(sender As Object, e As EventArgs)
        RefreshRequestCode()
    End Sub

    Private Sub SetStatus(ByVal message As String, ByVal isError As Boolean)
        _lblStatus.ForeColor = If(isError, Color.Firebrick, Color.FromArgb(24, 119, 242))
        _lblStatus.Text = message
    End Sub
End Class
