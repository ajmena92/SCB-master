Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports SCSC

<TestClass>
Public Class CodigoGeneralTests
    <TestMethod>
    Public Sub SCM_WrapsValueInSingleQuotes()
        Assert.AreEqual("'ABC'", CodigoGeneral.SCM("ABC"))
    End Sub

    <TestMethod>
    Public Sub Sen_RemovesCurrencySymbolAndCommas()
        Assert.AreEqual("123456", CodigoGeneral.sen("¢123,456"))
    End Sub

    <TestMethod>
    Public Sub ArmaFechaReporte_BuildsExpectedFormula()
        Dim startDate As New Date(2026, 3, 1)
        Dim endDate As New Date(2026, 3, 9)

        Dim formula As String = CodigoGeneral.ArmaFechaReporte("Fecha", startDate, endDate)

        Assert.AreEqual("Fecha>=date(2026,3,1) and Fecha<=date(2026,3,9)", formula)
    End Sub
End Class
