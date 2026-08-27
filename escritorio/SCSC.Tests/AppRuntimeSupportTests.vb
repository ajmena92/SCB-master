Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SCSC

<TestClass>
Public Class AppRuntimeSupportTests
    <TestMethod>
    Public Sub NormalizeConnectionProfile_DefaultsToInstalled_WhenInvalid()
        Assert.AreEqual(AppRuntimeSupport.DbProfileInstalled, AppRuntimeSupport.NormalizeConnectionProfile("desconocido"))
        Assert.AreEqual(AppRuntimeSupport.DbProfileInstalled, AppRuntimeSupport.NormalizeConnectionProfile(""))
    End Sub

    <TestMethod>
    Public Sub ResolveConnectionStringForProfile_UsesLocalProfile()
        Dim resolved As String = AppRuntimeSupport.ResolveConnectionStringForProfile(
            AppRuntimeSupport.DbProfileLocal,
            "Server=LOCALHOST;Database=SCSC_LOCAL;",
            "Server=INSTALADA;Database=SCSC;",
            "Server=LEGACY;Database=SCSC_OLD;")

        Assert.AreEqual("Server=LOCALHOST;Database=SCSC_LOCAL;", resolved)
    End Sub

    <TestMethod>
    Public Sub ResolveConnectionStringForProfile_UsesInstalledProfile()
        Dim resolved As String = AppRuntimeSupport.ResolveConnectionStringForProfile(
            AppRuntimeSupport.DbProfileInstalled,
            "Server=LOCALHOST;Database=SCSC_LOCAL;",
            "Server=INSTALADA;Database=SCSC;",
            "Server=LEGACY;Database=SCSC_OLD;")

        Assert.AreEqual("Server=INSTALADA;Database=SCSC;", resolved)
    End Sub

    <TestMethod>
    Public Sub ResolveConnectionStringForProfile_UsesLegacyProfile()
        Dim resolved As String = AppRuntimeSupport.ResolveConnectionStringForProfile(
            AppRuntimeSupport.DbProfileLegacy,
            "Server=LOCALHOST;Database=SCSC_LOCAL;",
            "Server=INSTALADA;Database=SCSC;",
            "Server=LEGACY;Database=SCSC_OLD;")

        Assert.AreEqual("Server=LEGACY;Database=SCSC_OLD;", resolved)
    End Sub

    <TestMethod>
    Public Sub ResolveConnectionStringForProfile_ReturnsDefault_WhenProfileSourceMissing()
        Dim resolved As String = AppRuntimeSupport.ResolveConnectionStringForProfile(
            AppRuntimeSupport.DbProfileLocal,
            "",
            "Server=INSTALADA;Database=SCSC;",
            "Server=LEGACY;Database=SCSC_OLD;",
            "DEFAULT")

        Assert.AreEqual("DEFAULT", resolved)
    End Sub
End Class
