Option Strict On
Option Explicit On

Public Class FrmReportViewer
    Public Property Request As ReportRequest

    Private Sub FrmReportViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyReportStandard(Me)
            Dim lUsuario As String
            Dim lPwd As String
            Dim NombreServidor As String
            Dim NombreBaseDatos As String
            If Request Is Nothing Then
                MsgBox("No se especifico la solicitud de reporte.", MsgBoxStyle.Critical)
                Me.Dispose()
                Return
            End If
            Dim rep As CrystalDecisions.CrystalReports.Engine.ReportDocument = CrystalReportCatalog.Create(Request)
            '***********************************
            '****** Datos para el reporte ******
            '***********************************
            lUsuario = ObtenerParametroConexion("user id", "uid")
            lPwd = ObtenerValorParametroConexion("pwd", "password")
            NombreServidor = ObtenerValorParametroConexion("server", "Server")
            NombreBaseDatos = ObtenerValorParametroConexion("database", "Database")

            rep.DataSourceConnections(0).SetConnection(NombreServidor, NombreBaseDatos, lUsuario, lPwd)

            rep.RecordSelectionFormula = Request.SelectionFormula
            rep.SetParameterValue("Compania", If(String.IsNullOrEmpty(NomColegio), "COLEGIO", NomColegio))
            rep.SetParameterValue("Titulo", If(String.IsNullOrEmpty(Request.Title), "COLEGIO", Request.Title))
            rep.SetParameterValue("RangodeFechas", If(String.IsNullOrEmpty(Request.DateRangeLabel), "", Request.DateRangeLabel))
            rep.SetParameterValue("Leyenda", If(String.IsNullOrEmpty(Leyenda), "", Leyenda))
            rep.SetParameterValue("Ubicacion", If(String.IsNullOrEmpty(Ubicacion), "", Ubicacion))
            rep.SetParameterValue("Horario", If(String.IsNullOrEmpty(Request.ScheduleLabel), "", Request.ScheduleLabel))

            ReportViewer.ReportSource = rep
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
