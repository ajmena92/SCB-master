Public Class FrmReportViewer
    Private Sub FrmReportViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            Dim lUsuario, lPwd, NombreServidor, NombreBaseDatos As String
            Dim Rep As Object
            Dim Cn As String = GetAppConfig("Conexion")

            Select Case gSession.Reporte.ToLower
                Case "FrmReporteMarcas".ToLower
                    If gSession.TipoReporte = "DETALLADO" Then
                        Rep = New RptRangoFecha
                    Else
                        Rep = New RptRangoFecha
                    End If
                Case "FrmReporteRutas".ToLower
                    If gSession.TipoReporte = "DETALLADO" Then
                        Rep = New RptRuta_detallado
                    Else
                        Rep = New RptRuta_general
                    End If
                Case Else
                    MsgBox("Error interno asignar reporte, comuniquese con el administrador del sistema.!!", MsgBoxStyle.Critical)
            End Select
            '***********************************
            '****** Datos para el reporte ******
            '***********************************
            lUsuario = ObtenerParametroConexion("user id", "uid")
            lPwd = ObtenerValorParametroConexion("pwd", "password")
            NombreServidor = ObtenerValorParametroConexion("server", "Server")
            NombreBaseDatos = ObtenerValorParametroConexion("database", "Database")

            Rep.DataSourceConnections(0).SetConnection(NombreServidor, NombreBaseDatos, lUsuario, lPwd)

            Rep.RecordSelectionFormula = gSession.Criterio
            Rep.SetParameterValue("Compania", IIf(NomColegio = Nothing, "COLEGIO", NomColegio))
            Rep.SetParameterValue("Titulo", IIf(gSession.Titulo = Nothing, "COLEGIO", gSession.Titulo))
            Rep.SetParameterValue("RangodeFechas", IIf(gSession.RangoDeFecha = Nothing, "", gSession.RangoDeFecha))
            Rep.SetParameterValue("Leyenda", IIf(Leyenda = Nothing, "", Leyenda))
            Rep.SetParameterValue("Ubicacion", IIf(Ubicacion = Nothing, "", Ubicacion))

            ReportViewer.ReportSource = Rep
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical)
        End Try
    End Sub
End Class