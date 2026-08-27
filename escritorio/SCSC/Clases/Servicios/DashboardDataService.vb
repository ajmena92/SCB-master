Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization

Public Class DashboardDataService
    Private ReadOnly _cls As FuncionesDB
    Private Const RutaNoAsignadaSentinel As Integer = 1
    Private Const DashboardSchoolDayCount As Integer = 7
    Private Shared ReadOnly EsCulture As CultureInfo = CultureInfo.GetCultureInfo("es-CR")

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        _cls = If(cls, New FuncionesDB())
    End Sub

    Public Function CargarSnapshot(ByVal fechaReferencia As Date) As DashboardContracts.Snapshot
        Dim snapshot As New DashboardContracts.Snapshot() With {
            .Series = New List(Of DashboardContracts.DailyMetric)(),
            .Alertas = New List(Of DashboardContracts.DashboardAlert)(),
            .Comparativos = New List(Of DashboardContracts.DashboardComparisonItem)(),
            .TopRutas = New List(Of String)(),
            .PeriodKind = DashboardContracts.DashboardPeriodKind.SchoolDays
        }

        Using cn As New SqlConnection()
            _cls.AbrirConexion(cn, False)
            Dim hoy As Date = ResolveCurrentSchoolDay(fechaReferencia)
            Dim ayer As Date = GetPreviousSchoolDay(hoy)
            Dim schoolDays As List(Of Date) = BuildSchoolDayRange(hoy, DashboardSchoolDayCount)
            Dim inicioSerie As Date = schoolDays(0)

            snapshot.PeriodLabel = "Ultimos " & DashboardSchoolDayCount.ToString() & " dias lectivos"
            snapshot.ComparisonLabel = "Comparativo hoy vs dia lectivo previo"

            snapshot.BecadosComedorHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiBecadosComedor",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE TipoPago = 2 AND Beca = 1 AND " & ArmaFechaQueryHora("Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.EstudiantesConRutaHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiEstudiantesConRuta",
                "SELECT COUNT(DISTINCT RT.IdUsuario) AS Total " &
                "FROM RegistroTransporte RT " &
                "WHERE " & BuildRutaRegistradaPredicate("RT") & " AND " & ArmaFechaQueryHora("RT.Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.EstudiantesConRutaAyer = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiEstudiantesConRutaAyer",
                "SELECT COUNT(DISTINCT RT.IdUsuario) AS Total " &
                "FROM RegistroTransporte RT " &
                "WHERE " & BuildRutaRegistradaPredicate("RT") & " AND " & ArmaFechaQueryHora("RT.Fecha", ayer, ayer),
                Cn:=cn))

            snapshot.MarcasComedorHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasComedor",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE TipoPago = 2 AND " & ArmaFechaQueryHora("Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.MarcasComedorAyer = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasComedorAyer",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE TipoPago = 2 AND " & ArmaFechaQueryHora("Fecha", ayer, ayer),
                Cn:=cn))

            snapshot.MarcasTransporteHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasTransporte",
                "SELECT COUNT(1) AS Total FROM RegistroTransporte WHERE " & ArmaFechaQueryHora("Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.MarcasTransporteAyer = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasTransporteAyer",
                "SELECT COUNT(1) AS Total FROM RegistroTransporte WHERE " & ArmaFechaQueryHora("Fecha", ayer, ayer),
                Cn:=cn))

            Dim dsSerie As DataSet = _cls.ConsultarTSQL(
                "SerieSemanal",
                "SELECT CONVERT(date, F.Fecha) AS Dia, " &
                "SUM(F.Comedor) AS Comedor, SUM(F.ComedorBecados) AS ComedorBecados, SUM(F.Transporte) AS Transporte, SUM(F.TransporteConRuta) AS TransporteConRuta " &
                "FROM (" &
                "   SELECT CONVERT(date, Fecha) AS Fecha, COUNT(1) AS Comedor, SUM(CASE WHEN ISNULL(Beca,0)=1 THEN 1 ELSE 0 END) AS ComedorBecados, 0 AS Transporte, 0 AS TransporteConRuta FROM RegistroComedor " &
                "   WHERE " & ArmaFechaQueryHora("Fecha", inicioSerie, hoy) & " AND TipoPago = 2 " &
                "   GROUP BY CONVERT(date, Fecha) " &
                "   UNION ALL " &
                "   SELECT CONVERT(date, Fecha) AS Fecha, 0 AS Comedor, 0 AS ComedorBecados, COUNT(1) AS Transporte, 0 AS TransporteConRuta FROM RegistroTransporte " &
                "   WHERE " & ArmaFechaQueryHora("Fecha", inicioSerie, hoy) & " " &
                "   GROUP BY CONVERT(date, Fecha) " &
                "   UNION ALL " &
                "   SELECT CONVERT(date, RT.Fecha) AS Fecha, 0 AS Comedor, 0 AS ComedorBecados, 0 AS Transporte, COUNT(DISTINCT RT.IdUsuario) AS TransporteConRuta " &
                "   FROM RegistroTransporte RT " &
                "   WHERE " & BuildRutaRegistradaPredicate("RT") & " AND " & ArmaFechaQueryHora("RT.Fecha", inicioSerie, hoy) & " " &
                "   GROUP BY CONVERT(date, RT.Fecha) " &
                ") F GROUP BY CONVERT(date, F.Fecha) ORDER BY Dia",
                Cn:=cn)

            Dim indexByDay As New Dictionary(Of Date, DataRow)()
            If dsSerie.Tables.Count > 0 Then
                For Each row As DataRow In dsSerie.Tables(0).Rows
                    indexByDay(CDate(row("Dia")).Date) = row
                Next
            End If

            For Each day As Date In schoolDays
                Dim metric As New DashboardContracts.DailyMetric() With {
                    .MetricDate = day,
                    .DayNameShort = FormatShortDayName(day),
                    .Label = BuildMetricLabel(day),
                    .Comedor = 0,
                    .ComedorBecados = 0,
                    .Transporte = 0,
                    .TransporteConRuta = 0
                }
                If indexByDay.ContainsKey(day) Then
                    Dim row As DataRow = indexByDay(day)
                    metric.Comedor = If(IsDBNull(row("Comedor")), 0, CInt(row("Comedor")))
                    metric.ComedorBecados = If(IsDBNull(row("ComedorBecados")), 0, CInt(row("ComedorBecados")))
                    metric.Transporte = If(IsDBNull(row("Transporte")), 0, CInt(row("Transporte")))
                    metric.TransporteConRuta = If(IsDBNull(row("TransporteConRuta")), 0, CInt(row("TransporteConRuta")))
                End If
                snapshot.Series.Add(metric)
            Next

            Dim dsRutas As DataSet = _cls.ConsultarTSQL(
                "TopRutas",
                "SELECT TOP 5 R.Descripcion, COUNT(1) AS Total " &
                "FROM RegistroTransporte RT " &
                "INNER JOIN Ruta R ON R.IdRuta = RT.IdRuta " &
                "WHERE " & ArmaFechaQueryHora("RT.Fecha", hoy, hoy) & " " &
                "GROUP BY R.Descripcion ORDER BY Total DESC",
                Cn:=cn)

            If dsRutas.Tables.Count > 0 Then
                For Each row As DataRow In dsRutas.Tables(0).Rows
                    snapshot.TopRutas.Add("- " & CStr(row("Descripcion")) & ": " & CInt(row("Total")).ToString("N0"))
                Next
            End If

            snapshot.Comparativos.Add(CreateComparisonItem("Comedor", snapshot.MarcasComedorHoy, snapshot.MarcasComedorAyer))
            snapshot.Comparativos.Add(CreateComparisonItem("Transporte", snapshot.MarcasTransporteHoy, snapshot.MarcasTransporteAyer))
            snapshot.Comparativos.Add(CreateComparisonItem("Con ruta", snapshot.EstudiantesConRutaHoy, snapshot.EstudiantesConRutaAyer))

            AddOperationalAlerts(snapshot)

            _cls.CerrarConexion(cn)
        End Using

        Return snapshot
    End Function

    Private Function BuildSchoolDayRange(ByVal referenceDay As Date, ByVal dayCount As Integer) As List(Of Date)
        Dim result As New List(Of Date)()
        Dim current As Date = ResolveCurrentSchoolDay(referenceDay)
        Dim safeCount As Integer = Math.Max(1, dayCount)

        While result.Count < safeCount
            If IsSchoolDay(current) Then
                result.Add(current)
            End If
            current = current.AddDays(-1)
        End While

        result.Reverse()
        Return result
    End Function

    Private Sub AddOperationalAlerts(ByVal snapshot As DashboardContracts.Snapshot)
        If snapshot Is Nothing Then
            Exit Sub
        End If

        AddDropAlert(snapshot.Alertas, "comedor", "Comedor", snapshot.MarcasComedorHoy, snapshot.MarcasComedorAyer, 20)
        AddDropAlert(snapshot.Alertas, "transporte", "Transporte", snapshot.MarcasTransporteHoy, snapshot.MarcasTransporteAyer, 20)
        AddDropAlert(snapshot.Alertas, "transporte.ruta", "Cobertura de ruta", snapshot.EstudiantesConRutaHoy, snapshot.EstudiantesConRutaAyer, 10)

        If snapshot.MarcasComedorHoy > 0 AndAlso snapshot.BecadosComedorHoy = 0 Then
            snapshot.Alertas.Add(CreateAlert(
                "comedor.sin_becados",
                "Becados de comedor",
                "Sin becados hoy (0 registros con beca sobre " & snapshot.MarcasComedorHoy.ToString("N0") & " entradas de comedor).",
                DashboardContracts.AlertSeverity.Warning,
                30))
        ElseIf snapshot.MarcasComedorHoy > 0 Then
            Dim ratioBecados As Double = snapshot.BecadosComedorHoy / CDbl(Math.Max(1, snapshot.MarcasComedorHoy))
            If ratioBecados < 0.05R Then
                snapshot.Alertas.Add(CreateAlert(
                    "comedor.becados_bajos",
                    "Becados de comedor",
                    "Cobertura baja de becados en comedor: " & snapshot.BecadosComedorHoy.ToString("N0") & " de " & snapshot.MarcasComedorHoy.ToString("N0") & " entradas.",
                    DashboardContracts.AlertSeverity.Info,
                    40))
            End If
        End If

        If snapshot.MarcasTransporteHoy > 0 AndAlso snapshot.EstudiantesConRutaHoy = 0 Then
            snapshot.Alertas.Add(CreateAlert(
                "transporte.sin_estudiantes_con_ruta",
                "Cobertura de ruta",
                "Sin estudiantes con ruta hoy (0 con ruta sobre " & snapshot.MarcasTransporteHoy.ToString("N0") & " abordajes).",
                DashboardContracts.AlertSeverity.Critical,
                20))
        ElseIf snapshot.MarcasTransporteHoy > 0 AndAlso snapshot.EstudiantesConRutaHoy < snapshot.MarcasTransporteHoy Then
            Dim routeGap As Integer = snapshot.MarcasTransporteHoy - snapshot.EstudiantesConRutaHoy
            Dim routeGapRatio As Double = routeGap / CDbl(Math.Max(1, snapshot.MarcasTransporteHoy))
            If routeGapRatio >= 0.35R Then
                snapshot.Alertas.Add(CreateAlert(
                    "transporte.cobertura_ruta_baja",
                    "Cobertura de ruta",
                    "Cobertura parcial de ruta: " & snapshot.EstudiantesConRutaHoy.ToString("N0") & " con ruta de " & snapshot.MarcasTransporteHoy.ToString("N0") & " abordajes.",
                    DashboardContracts.AlertSeverity.Warning,
                    25))
            End If
        End If
    End Sub

    Private Function ResolveCurrentSchoolDay(ByVal referenceDay As Date) As Date
        Dim current As Date = referenceDay.Date
        While Not IsSchoolDay(current)
            current = current.AddDays(-1)
        End While
        Return current
    End Function

    Private Function GetPreviousSchoolDay(ByVal referenceDay As Date) As Date
        Dim current As Date = referenceDay.Date.AddDays(-1)
        While Not IsSchoolDay(current)
            current = current.AddDays(-1)
        End While
        Return current
    End Function

    Private Function IsSchoolDay(ByVal value As Date) As Boolean
        Return value.DayOfWeek <> DayOfWeek.Saturday AndAlso value.DayOfWeek <> DayOfWeek.Sunday
    End Function

    Private Function BuildMetricLabel(ByVal value As Date) As String
        Return FormatShortDayName(value) & " " & value.ToString("dd/MM")
    End Function

    Private Function FormatShortDayName(ByVal value As Date) As String
        Dim dayName As String = EsCulture.DateTimeFormat.GetAbbreviatedDayName(value.DayOfWeek)
        If String.IsNullOrWhiteSpace(dayName) Then
            Return value.ToString("ddd", EsCulture)
        End If

        dayName = dayName.Trim().TrimEnd("."c)
        If dayName.Length = 0 Then
            Return value.ToString("ddd", EsCulture)
        End If

        Return Char.ToUpperInvariant(dayName(0)) & dayName.Substring(1).ToLowerInvariant()
    End Function

    Private Sub AddDropAlert(ByVal target As List(Of DashboardContracts.DashboardAlert),
                             ByVal code As String,
                             ByVal label As String,
                             ByVal currentValue As Integer,
                             ByVal previousValue As Integer,
                             ByVal minimumPreviousBase As Integer)
        If target Is Nothing Then
            Exit Sub
        End If

        If previousValue <= 0 OrElse previousValue < minimumPreviousBase Then
            Exit Sub
        End If

        Dim deltaRatio As Double = (currentValue - previousValue) / CDbl(previousValue)
        If deltaRatio <= -0.2R Then
            target.Add(CreateAlert(
                code & ".caida_critica",
                label,
                label & " cae " & FormatPercent(deltaRatio) & " vs dia lectivo previo (" & currentValue.ToString("N0") & "/" & previousValue.ToString("N0") & ").",
                DashboardContracts.AlertSeverity.Critical,
                10))
        ElseIf deltaRatio <= -0.1R Then
            target.Add(CreateAlert(
                code & ".caida_moderada",
                label,
                label & " cae " & FormatPercent(deltaRatio) & " vs dia lectivo previo (" & currentValue.ToString("N0") & "/" & previousValue.ToString("N0") & ").",
                DashboardContracts.AlertSeverity.Warning,
                15))
        End If
    End Sub

    Private Function CreateComparisonItem(ByVal label As String, ByVal currentValue As Integer, ByVal previousValue As Integer) As DashboardContracts.DashboardComparisonItem
        Dim item As New DashboardContracts.DashboardComparisonItem() With {
            .Label = If(label, String.Empty),
            .CurrentValue = currentValue,
            .PreviousValue = previousValue,
            .DeltaText = FormatDeltaText(currentValue, previousValue),
            .StatusText = ResolveComparisonStatusText(currentValue, previousValue),
            .Trend = ResolveComparisonTrend(currentValue, previousValue)
        }

        Return item
    End Function

    Private Function ResolveComparisonTrend(ByVal currentValue As Integer, ByVal previousValue As Integer) As DashboardContracts.ComparisonTrend
        If previousValue <= 0 AndAlso currentValue > 0 Then
            Return DashboardContracts.ComparisonTrend.NoBase
        End If
        If currentValue > previousValue Then
            Return DashboardContracts.ComparisonTrend.Up
        End If
        If currentValue < previousValue Then
            Return DashboardContracts.ComparisonTrend.Down
        End If
        Return DashboardContracts.ComparisonTrend.Flat
    End Function

    Private Function ResolveComparisonStatusText(ByVal currentValue As Integer, ByVal previousValue As Integer) As String
        Select Case ResolveComparisonTrend(currentValue, previousValue)
            Case DashboardContracts.ComparisonTrend.Up
                Return "SUBE"
            Case DashboardContracts.ComparisonTrend.Down
                Return "BAJA"
            Case DashboardContracts.ComparisonTrend.NoBase
                Return "SIN BASE"
            Case Else
                Return "SIN CAMBIO"
        End Select
    End Function

    Private Function FormatDeltaText(ByVal currentValue As Integer, ByVal previousValue As Integer) As String
        If previousValue <= 0 Then
            If currentValue <= 0 Then
                Return "sin cambio (0/0)"
            End If
            Return "sin base previa (" & currentValue.ToString("N0") & "/0)"
        End If

        Dim delta As Double = (currentValue - previousValue) / CDbl(previousValue)
        Return FormatPercent(delta) & " (" & currentValue.ToString("N0") & "/" & previousValue.ToString("N0") & ")"
    End Function

    Private Function FormatPercent(ByVal value As Double) As String
        Dim percent As Double = value * 100.0R
        Dim sign As String = If(percent > 0, "+", String.Empty)
        Return sign & percent.ToString("0") & "%"
    End Function

    Private Function SafeScalarInt(ByVal ds As DataSet) As Integer
        If ds Is Nothing OrElse ds.Tables.Count = 0 OrElse ds.Tables(0).Rows.Count = 0 Then
            Return 0
        End If

        Dim first As Object = ds.Tables(0).Rows(0)(0)
        If IsDBNull(first) Then
            Return 0
        End If
        Return CInt(first)
    End Function

    Private Function BuildRutaRegistradaPredicate(ByVal registroAlias As String) As String
        Dim aliasToken As String = If(String.IsNullOrWhiteSpace(registroAlias), "RT", registroAlias.Trim())
        Return "ISNULL(" & aliasToken & ".IdRuta," & RutaNoAsignadaSentinel.ToString() & ") <> " & RutaNoAsignadaSentinel.ToString()
    End Function

    Private Function CreateAlert(ByVal code As String,
                                 ByVal title As String,
                                 ByVal message As String,
                                 ByVal severity As DashboardContracts.AlertSeverity,
                                 Optional ByVal sortOrder As Integer = 100) As DashboardContracts.DashboardAlert
        Return New DashboardContracts.DashboardAlert() With {
            .Code = If(code, String.Empty),
            .Title = If(title, String.Empty),
            .Message = If(message, String.Empty),
            .Severity = severity,
            .SortOrder = sortOrder
        }
    End Function
End Class
