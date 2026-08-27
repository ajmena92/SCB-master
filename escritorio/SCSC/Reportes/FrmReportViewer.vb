Option Strict On
Option Explicit On

Imports System.Data.SqlClient
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared

Public Class FrmReportViewer
    Public Property Request As ReportRequest

    Private Sub FrmReportViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If CrudVisualHelper.IsInDesignMode(Me) Then
            Return
        End If
        Try
            CrudVisualHelper.ApplyReportStandard(Me)
            If Request Is Nothing Then
                MsgBox("No se especifico la solicitud de reporte.", MsgBoxStyle.Critical)
                Me.Dispose()
                Return
            End If
            Dim rep As ReportDocument = CrystalReportCatalog.Create(Request)
            Dim builder As SqlConnectionStringBuilder = GetReportConnectionBuilder()
            ApplyReportConnection(rep, builder)

            rep.RecordSelectionFormula = Request.SelectionFormula
            rep.SetParameterValue("Compania", If(String.IsNullOrEmpty(NomColegio), "COLEGIO", NomColegio))
            rep.SetParameterValue("Titulo", If(String.IsNullOrEmpty(Request.Title), "COLEGIO", Request.Title))
            rep.SetParameterValue("RangodeFechas", If(String.IsNullOrEmpty(Request.DateRangeLabel), "", Request.DateRangeLabel))
            rep.SetParameterValue("Leyenda", If(String.IsNullOrEmpty(Leyenda), "", Leyenda))
            rep.SetParameterValue("Ubicacion", If(String.IsNullOrEmpty(Ubicacion), "", Ubicacion))
            rep.SetParameterValue("Horario", If(String.IsNullOrEmpty(Request.ScheduleLabel), "", Request.ScheduleLabel))

            ReportViewer.ReportSource = rep
        Catch ex As Exception
            ErrorLogger.LogException("FrmReportViewer.Load", ex, BuildReportFailureContext())
            MsgBox(BuildUserFriendlyErrorMessage(ex), MsgBoxStyle.Critical)
        End Try
    End Sub

    Private Shared Function GetReportConnectionBuilder() As SqlConnectionStringBuilder
        Dim connectionString As String = GetAppConfig("Conexion")
        If String.IsNullOrWhiteSpace(connectionString) Then
            Throw New InvalidOperationException("No existe una cadena de conexion valida para los reportes.")
        End If

        Return New SqlConnectionStringBuilder(connectionString)
    End Function

    Private Shared Sub ApplyReportConnection(ByVal report As ReportDocument, ByVal builder As SqlConnectionStringBuilder)
        If report Is Nothing Then
            Throw New InvalidOperationException("No se pudo inicializar el reporte.")
        End If
        ApplyConnectionToTables(report.Database.Tables, builder)

        For Each section As Section In report.ReportDefinition.Sections
            For Each reportObject As ReportObject In section.ReportObjects
                If reportObject.Kind <> ReportObjectKind.SubreportObject Then
                    Continue For
                End If

                Dim subreportObject As SubreportObject = DirectCast(reportObject, SubreportObject)
                Dim subreport As ReportDocument = subreportObject.OpenSubreport(subreportObject.SubreportName)
                ApplyConnectionToTables(subreport.Database.Tables, builder)
            Next
        Next
    End Sub

    Private Shared Sub ApplyConnectionToTables(ByVal tables As Tables, ByVal builder As SqlConnectionStringBuilder)
        For Each table As Table In tables
            Dim logOnInfo As TableLogOnInfo = table.LogOnInfo
            Dim connectionInfo As ConnectionInfo = logOnInfo.ConnectionInfo

            connectionInfo.ServerName = builder.DataSource
            connectionInfo.DatabaseName = builder.InitialCatalog
            connectionInfo.IntegratedSecurity = builder.IntegratedSecurity
            connectionInfo.UserID = If(builder.IntegratedSecurity, String.Empty, builder.UserID)
            connectionInfo.Password = If(builder.IntegratedSecurity, String.Empty, builder.Password)

            logOnInfo.ConnectionInfo = connectionInfo
            table.ApplyLogOnInfo(logOnInfo)

            table.Location = BuildTableLocation(table.Location, builder.InitialCatalog)
        Next
    End Sub

    Private Shared Function BuildTableLocation(ByVal currentLocation As String, ByVal databaseName As String) As String
        If String.IsNullOrWhiteSpace(currentLocation) OrElse String.IsNullOrWhiteSpace(databaseName) Then
            Return currentLocation
        End If

        Dim parts As String() = currentLocation.Split("."c)
        If parts.Length >= 3 Then
            parts(0) = databaseName
            Return String.Join(".", parts)
        End If

        Return currentLocation
    End Function

    Private Function BuildUserFriendlyErrorMessage(ByVal ex As Exception) As String
        Dim builder As SqlConnectionStringBuilder = Nothing
        Try
            builder = GetReportConnectionBuilder()
        Catch
        End Try

        Dim message As String = "No se pudo abrir el reporte."
        If Request IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(Request.Title) Then
            message &= vbCrLf & "Reporte: " & Request.Title.Trim()
        End If

        If builder IsNot Nothing Then
            message &= vbCrLf & "Servidor: " & builder.DataSource
            message &= vbCrLf & "Base de datos: " & builder.InitialCatalog
            message &= vbCrLf & "Autenticacion: " & If(builder.IntegratedSecurity, "Windows", "SQL")
            If Not builder.IntegratedSecurity Then
                message &= vbCrLf & "Usuario SQL: " & builder.UserID
            End If
        End If

        message &= vbCrLf & "Detalle: " & ex.Message
        message &= vbCrLf & "Revise el log en: " & ErrorLogger.GetCurrentLogPath()
        Return message
    End Function

    Private Function BuildReportFailureContext() As String
        Dim details As New Text.StringBuilder()
        If Request IsNot Nothing Then
            details.AppendLine("ReportKey=" & If(Request.ReportKey, String.Empty))
            details.AppendLine("ReportVariant=" & If(Request.ReportVariant, String.Empty))
            details.AppendLine("Title=" & If(Request.Title, String.Empty))
        End If

        Try
            Dim builder As SqlConnectionStringBuilder = GetReportConnectionBuilder()
            details.AppendLine("Server=" & builder.DataSource)
            details.AppendLine("Database=" & builder.InitialCatalog)
            details.AppendLine("IntegratedSecurity=" & builder.IntegratedSecurity.ToString())
            If Not builder.IntegratedSecurity Then
                details.AppendLine("UserID=" & builder.UserID)
            End If
        Catch ex As Exception
            details.AppendLine("ConnectionContextError=" & ex.Message)
        End Try

        Return details.ToString().Trim()
    End Function
End Class
