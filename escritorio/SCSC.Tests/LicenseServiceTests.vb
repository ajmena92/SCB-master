Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SCSC

<TestClass>
Public Class LicenseServiceTests
    <TestMethod>
    Public Sub GenerateActivationCode_IsStable_ForSameInput()
        Dim code1 As String = LicenseService.GenerateActivationCode("CTP Platanares", "San Rafael", "cliente01", "SQL01", "SCSC", "Standard")
        Dim code2 As String = LicenseService.GenerateActivationCode("CTP Platanares", "San Rafael", "cliente01", "SQL01", "SCSC", "Standard")

        Assert.AreEqual(code1, code2)
        Assert.IsTrue(code1.Contains("-"))
    End Sub

    <TestMethod>
    Public Sub GenerateActivationCode_Changes_WhenDatabaseChanges()
        Dim code1 As String = LicenseService.GenerateActivationCode("CTP Platanares", "San Rafael", "cliente01", "SQL01", "SCSC", "Standard")
        Dim code2 As String = LicenseService.GenerateActivationCode("CTP Platanares", "San Rafael", "cliente01", "SQL01", "SCSC_PRUEBA", "Standard")

        Assert.AreNotEqual(code1, code2)
    End Sub

    <TestMethod>
    Public Sub BuildRequestCode_ContainsExpectedPayloadFields()
        Dim requestCode As String = LicenseService.BuildRequestCode("CTP Platanares", "San Rafael", "cliente01", "SQL01", "SCSC")
        Dim json As String = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(requestCode))

        StringAssert.Contains(json, """customerName"":""CTP PLATANARES""")
        StringAssert.Contains(json, """siteName"":""SAN RAFAEL""")
        StringAssert.Contains(json, """clientId"":""CLIENTE01""")
        StringAssert.Contains(json, """databaseServer"":""SQL01""")
        StringAssert.Contains(json, """databaseName"":""SCSC""")
    End Sub
End Class
