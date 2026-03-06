Option Strict On
Option Explicit On

Public Class FrmReportViewer
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
            Dim rep As CrystalDecisions.CrystalReports.Engine.ReportDocument = Nothing

            Select Case gSession.Reporte.ToLower
                Case "FrmReporteMarcas".ToLower
                    If gSession.TipoReporte = "DETALLADO" Then
                        rep = New RptFechaComedor
                    Else
                        rep = New RptFechaComedor
                    End If
                Case "FrmProyeccionComedor".ToLower
                    rep = New RptProyecionComedor
                Case "FrmReporteRutas".ToLower
                    If gSession.TipoReporte = "DETALLADO" Then
                        rep = New RptRuta_detallado
                    Else
                        rep = New RptRuta_general
                    End If
                Case "FrmBecados".ToLower
                    If gSession.TipoReporte = "FrmBecadosTransporteGeneral" Then
                        rep = New RptBecadosTransporte
                    ElseIf gSession.TipoReporte = "FrmBecadosTansporteDetallado" Then
                        rep = New RptBecadosTransporteDetallado
                    ElseIf gSession.TipoReporte = "BecadosComedor" Then
                        rep = New RptBecadosComedor
                    End If
                Case Else
                    MsgBox("Error interno asignar reporte, comuniquese con el administrador del sistema.!!", MsgBoxStyle.Critical)
                    Me.Dispose()
            End Select
            If rep Is Nothing Then
                MsgBox("No se pudo inicializar el reporte solicitado.", MsgBoxStyle.Critical)
                Me.Dispose()
                Return
            End If
            '***********************************
            '****** Datos para el reporte ******
            '***********************************
            lUsuario = ObtenerParametroConexion("user id", "uid")
            lPwd = ObtenerValorParametroConexion("pwd", "password")
            NombreServidor = ObtenerValorParametroConexion("server", "Server")
            NombreBaseDatos = ObtenerValorParametroConexion("database", "Database")

            rep.DataSourceConnections(0).SetConnection(NombreServidor, NombreBaseDatos, lUsuario, lPwd)

            rep.RecordSelectionFormula = gSession.Criterio
            rep.SetParameterValue("Compania", If(String.IsNullOrEmpty(NomColegio), "COLEGIO", NomColegio))
            rep.SetParameterValue("Titulo", If(String.IsNullOrEmpty(gSession.Titulo), "COLEGIO", gSession.Titulo))
            rep.SetParameterValue("RangodeFechas", If(String.IsNullOrEmpty(gSession.RangoDeFecha), "", gSession.RangoDeFecha))
            rep.SetParameterValue("Leyenda", If(String.IsNullOrEmpty(Leyenda), "", Leyenda))
            rep.SetParameterValue("Ubicacion", If(String.IsNullOrEmpty(Ubicacion), "", Ubicacion))
            rep.SetParameterValue("Horario", If(String.IsNullOrEmpty(gSession.Valor1), "", gSession.Valor1))

            ReportViewer.ReportSource = rep
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class
