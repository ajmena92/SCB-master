Option Strict On
Option Explicit On

Imports CrystalDecisions.CrystalReports.Engine

Public NotInheritable Class CrystalReportCatalog
    Public Shared Function Create(ByVal request As ReportRequest) As ReportDocument
        If request Is Nothing OrElse String.IsNullOrWhiteSpace(request.ReportKey) Then
            Throw New InvalidOperationException("No se especifico el reporte solicitado.")
        End If

        Select Case request.ReportKey.Trim().ToLowerInvariant()
            Case "frmreportemarcas"
                Return New RptFechaComedor()
            Case "frmproyeccioncomedor"
                Return New RptProyecionComedor()
            Case "frmreporterutas"
                If String.Equals(request.ReportVariant, "DETALLADO", StringComparison.OrdinalIgnoreCase) Then
                    Return New RptRuta_detallado()
                End If
                Return New RptRuta_general()
            Case "frmbecados"
                Select Case request.ReportVariant
                    Case "FrmBecadosTransporteGeneral"
                        Return New RptBecadosTransporte()
                    Case "FrmBecadosTansporteDetallado"
                        Return New RptBecadosTransporteDetallado()
                    Case "BecadosComedor"
                        Return New RptBecadosComedor()
                End Select
        End Select

        Throw New InvalidOperationException("No existe un reporte registrado para la clave '" & request.ReportKey & "'.")
    End Function
End Class
