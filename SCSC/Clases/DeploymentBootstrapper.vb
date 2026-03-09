Option Strict On
Option Explicit On

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
        Dim hasValidConfiguration As Boolean = False
        Dim loadedConfig As DeploymentConfiguration = Nothing
        Dim configurationMessage As String = String.Empty
        If DeploymentConfigService.TryLoad(loadedConfig, configurationMessage) AndAlso loadedConfig IsNot Nothing Then
            hasValidConfiguration = DeploymentConfigService.TestConnection(loadedConfig, configurationMessage)
        End If

        Dim licenseMessage As String = String.Empty
        Dim hasValidLicense As Boolean = LicenseService.ValidateInstalledLicense(licenseMessage)

        If hasValidConfiguration AndAlso hasValidLicense Then
            Return True
        End If

        Using setupForm As New FrmDeploymentSetup(False)
            If owner Is Nothing Then
                Return setupForm.ShowDialog() = DialogResult.OK
            End If
            Return setupForm.ShowDialog(owner) = DialogResult.OK
        End Using
    End Function
End Class
