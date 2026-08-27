Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data.SqlClient
Imports SCSC

<TestClass>
Public Class DeploymentConfigurationTests
    <TestMethod>
    Public Sub BuildConfiguration_IntegratedSecurity_SetsWindowsMode()
        Dim config As DeploymentConfiguration = DeploymentConfigService.BuildConfiguration("SERVIDOR01", "SCSC", True, "", "")

        Assert.AreEqual("Windows", config.AuthenticationMode)
        Assert.IsTrue(config.UseIntegratedSecurity())
    End Sub

    <TestMethod>
    Public Sub BuildConnectionString_ForSqlAuth_ContainsExpectedValues()
        Dim config As DeploymentConfiguration = DeploymentConfigService.BuildConfiguration("SERVIDOR01", "SCSC", False, "sa", "Clave123")
        Dim cs As String = config.BuildConnectionString()
        Dim builder As New SqlConnectionStringBuilder(cs)

        Assert.AreEqual("SERVIDOR01", builder.DataSource)
        Assert.AreEqual("SCSC", builder.InitialCatalog)
        Assert.AreEqual("sa", builder.UserID)
        Assert.AreEqual("Clave123", builder.Password)
        Assert.IsFalse(builder.IntegratedSecurity)
    End Sub

    <TestMethod>
    Public Sub PasswordRoundTrip_ReturnsOriginalValue()
        Dim config As New DeploymentConfiguration()
        config.SetPlainTextPassword("Temporal123!")

        Assert.AreNotEqual(String.Empty, config.EncryptedPassword)
        Assert.AreEqual("Temporal123!", config.GetPlainTextPassword())
    End Sub

    <TestMethod>
    Public Sub ValidateConfiguration_Fails_WhenServerMissing()
        Dim config As DeploymentConfiguration = DeploymentConfigService.BuildConfiguration("", "SCSC", True, "", "")
        Dim errorMessage As String = String.Empty

        Dim result As Boolean = DeploymentConfigService.ValidateConfiguration(config, errorMessage)

        Assert.IsFalse(result)
        Assert.AreEqual("Debe indicar el servidor SQL.", errorMessage)
    End Sub
End Class
