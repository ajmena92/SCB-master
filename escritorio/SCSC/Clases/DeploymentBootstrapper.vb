Option Strict On
Option Explicit On

Imports System.Data.SqlClient
Imports System.Windows.Forms

Public NotInheritable Class DeploymentBootstrapper
    Public Const SetupSwitch As String = "/deployment-setup"

    Private Sub New()
    End Sub

    Public Shared Function ShouldRunSetupOnly() As Boolean
        Dim args As String() = Environment.GetCommandLineArgs()
        For Each arg As String In args
            If String.Equals(arg, SetupSwitch, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Shared Function EnsureDeploymentReady(ByVal owner As IWin32Window) As Boolean
        Dim readiness As DeploymentReadinessResult = EvaluateDeploymentReadiness()
        If readiness.IsReady Then
            Return True
        End If

        Return ShowDeploymentSetup(owner, False) = DialogResult.OK
    End Function

    Public Shared Function EvaluateDeploymentReadiness() As DeploymentReadinessResult
        Dim profile As String = NormalizeConnectionProfile(GetConnectionProfile())
        Dim profileMessage As String = String.Empty

        Select Case profile
            Case DbProfileLocal
                If IsExplicitProfileConnectionReady(profile, profileMessage) Then
                    Return DeploymentReadinessResult.Ready("Perfil LOCAL con conexión SQL válida. Se omite configuración/licencia instalada.")
                End If

                Return DeploymentReadinessResult.Failed(profileMessage, False, True)
            Case DbProfileLegacy
                If IsExplicitProfileConnectionReady(profile, profileMessage) Then
                    Return DeploymentReadinessResult.Ready("Perfil LEGACY con conexión SQL válida. Se omite licenciamiento offline.")
                End If

                Return DeploymentReadinessResult.Failed(profileMessage, False, True)
        End Select

        Dim hasValidConfiguration As Boolean = False
        Dim loadedConfig As DeploymentConfiguration = Nothing
        Dim configurationMessage As String = String.Empty
        If DeploymentConfigService.TryLoad(loadedConfig, configurationMessage) AndAlso loadedConfig IsNot Nothing Then
            hasValidConfiguration = DeploymentConfigService.TestConnection(loadedConfig, configurationMessage)
        End If

        Dim licenseMessage As String = String.Empty
        Dim hasValidLicense As Boolean = False
        If loadedConfig IsNot Nothing Then
            hasValidLicense = LicenseService.ValidateInstalledLicense(loadedConfig.Server, loadedConfig.Database, licenseMessage)
        End If

        If hasValidConfiguration AndAlso hasValidLicense Then
            Return DeploymentReadinessResult.Ready("External deployment configuration and license validated successfully.")
        End If

        Dim failureMessage As String = BuildFailureMessage(hasValidConfiguration, configurationMessage, hasValidLicense, licenseMessage)
        Return DeploymentReadinessResult.Failed(failureMessage, hasValidConfiguration, hasValidLicense)
    End Function

    Public Shared Function ShowDeploymentSetup(ByVal owner As IWin32Window, ByVal setupOnlyMode As Boolean) As DialogResult
        Using setupForm As New FrmDeploymentSetup(setupOnlyMode)
            If owner Is Nothing Then
                Return setupForm.ShowDialog()
            End If
            Return setupForm.ShowDialog(owner)
        End Using
    End Function

    Private Shared Function IsExplicitProfileConnectionReady(ByVal profile As String, ByRef failureMessage As String) As Boolean
        Try
            Dim connectionString As String = GetAppSettingValue("Conexion", String.Empty)
            If String.IsNullOrWhiteSpace(connectionString) Then
                failureMessage = BuildMissingConnectionMessage(profile)
                ErrorLogger.LogInfo("DeploymentBootstrapper", failureMessage)
                Return False
            End If

            Using cn As New SqlConnection(connectionString), _
                  cmd As New SqlCommand("SELECT DB_NAME()", cn)
                cn.Open()
                Dim dbName As String = Convert.ToString(cmd.ExecuteScalar()).Trim()
                If String.IsNullOrWhiteSpace(dbName) Then
                    failureMessage = "La conexión SQL se abrió pero no devolvió el nombre de la base de datos."
                    Return False
                End If
            End Using

            failureMessage = String.Empty
            ErrorLogger.LogInfo("DeploymentBootstrapper", "Perfil " & profile & " con conexión SQL válida.")
            Return True
        Catch ex As Exception
            failureMessage = BuildConnectionFailureMessage(profile, ex)
            ErrorLogger.LogException("DeploymentBootstrapper.IsExplicitProfileConnectionReady", ex, "Perfil=" & profile)
            Return False
        End Try
    End Function

    Private Shared Function BuildMissingConnectionMessage(ByVal profile As String) As String
        Select Case NormalizeConnectionProfile(profile)
            Case DbProfileLocal
                Return "El perfil LOCAL está activo, pero no tiene configurada una cadena válida en app.config para la clave ConexionLocal."
            Case DbProfileLegacy
                Return "El perfil LEGACY está activo, pero no tiene configurada una cadena válida en app.config para la clave Conexion."
            Case Else
                Return "No existe una cadena de conexión configurada para el perfil actual."
        End Select
    End Function

    Private Shared Function BuildConnectionFailureMessage(ByVal profile As String, ByVal ex As Exception) As String
        Dim prefix As String

        Select Case NormalizeConnectionProfile(profile)
            Case DbProfileLocal
                prefix = "No se pudo conectar a SQL usando el perfil LOCAL."
            Case DbProfileLegacy
                prefix = "No se pudo conectar a SQL usando el perfil LEGACY."
            Case Else
                prefix = "No se pudo conectar a SQL usando el perfil configurado."
        End Select

        If ex Is Nothing OrElse String.IsNullOrWhiteSpace(ex.Message) Then
            Return prefix
        End If

        Return prefix & " " & ex.Message
    End Function

    Private Shared Function BuildFailureMessage(ByVal hasValidConfiguration As Boolean,
                                                ByVal configurationMessage As String,
                                                ByVal hasValidLicense As Boolean,
                                                ByVal licenseMessage As String) As String
        If Not hasValidConfiguration Then
            If Not String.IsNullOrWhiteSpace(configurationMessage) Then
                Return configurationMessage.Trim()
            End If

            Return "No se pudo validar la conexión SQL con la configuración actual."
        End If

        If Not hasValidLicense Then
            If Not String.IsNullOrWhiteSpace(licenseMessage) Then
                Return licenseMessage.Trim()
            End If

            Return "No existe una licencia válida para la configuración actual."
        End If

        Return "La configuración de despliegue no está lista."
    End Function
End Class

Public NotInheritable Class DeploymentReadinessResult
    Public Property IsReady As Boolean
    Public Property FailureMessage As String
    Public Property HasValidConfiguration As Boolean
    Public Property HasValidLicense As Boolean

    Public Shared Function Ready(ByVal message As String) As DeploymentReadinessResult
        Return New DeploymentReadinessResult With {
            .IsReady = True,
            .FailureMessage = If(message, String.Empty),
            .HasValidConfiguration = True,
            .HasValidLicense = True
        }
    End Function

    Public Shared Function Failed(ByVal message As String,
                                  ByVal hasValidConfiguration As Boolean,
                                  ByVal hasValidLicense As Boolean) As DeploymentReadinessResult
        Return New DeploymentReadinessResult With {
            .IsReady = False,
            .FailureMessage = If(message, String.Empty),
            .HasValidConfiguration = hasValidConfiguration,
            .HasValidLicense = hasValidLicense
        }
    End Function
End Class
