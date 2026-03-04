Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient

Public Class DashboardDataService
    Private ReadOnly _cls As FuncionesDB

    Public Class DailyMetric
        Public Property Label As String
        Public Property Comedor As Integer
        Public Property ComedorBecados As Integer
        Public Property Transporte As Integer
    End Class

    Public Class Snapshot
        Public Property BecadosComedorHoy As Integer
        Public Property BecadosTransporteHoy As Integer
        Public Property MarcasComedorHoy As Integer
        Public Property MarcasTransporteHoy As Integer
        Public Property MarcasComedorAyer As Integer
        Public Property MarcasTransporteAyer As Integer
        Public Property Series As List(Of DailyMetric)
        Public Property Alertas As List(Of String)
        Public Property TopRutas As List(Of String)
    End Class

    Public Sub New(Optional ByVal cls As FuncionesDB = Nothing)
        _cls = If(cls, New FuncionesDB())
    End Sub

    Public Function CargarSnapshot(ByVal fechaReferencia As Date) As Snapshot
        Dim snapshot As New Snapshot() With {
            .Series = New List(Of DailyMetric)(),
            .Alertas = New List(Of String)(),
            .TopRutas = New List(Of String)()
        }

        Using cn As New SqlConnection()
            _cls.AbrirConexion(cn, False)
            Dim hoy As Date = fechaReferencia.Date
            Dim ayer As Date = hoy.AddDays(-1)

            snapshot.BecadosComedorHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiBecadosComedor",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE Beca = 1 AND " & ArmaFechaQueryHora("Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.BecadosTransporteHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiBecadosTransporte",
                "SELECT COUNT(DISTINCT RT.IdUsuario) AS Total " &
                "FROM RegistroTransporte RT " &
                "INNER JOIN Usuario U ON U.IdUsuario = RT.IdUsuario " &
                "WHERE ISNULL(U.IdRuta,1) <> 1 AND " & ArmaFechaQueryHora("RT.Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.MarcasComedorHoy = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasComedor",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE " & ArmaFechaQueryHora("Fecha", hoy, hoy),
                Cn:=cn))

            snapshot.MarcasComedorAyer = SafeScalarInt(_cls.ConsultarTSQL(
                "KpiMarcasComedorAyer",
                "SELECT COUNT(1) AS Total FROM RegistroComedor WHERE " & ArmaFechaQueryHora("Fecha", ayer, ayer),
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
                "SUM(F.Comedor) AS Comedor, SUM(F.ComedorBecados) AS ComedorBecados, SUM(F.Transporte) AS Transporte " &
                "FROM (" &
                "   SELECT CONVERT(date, Fecha) AS Fecha, COUNT(1) AS Comedor, SUM(CASE WHEN ISNULL(Beca,0)=1 THEN 1 ELSE 0 END) AS ComedorBecados, 0 AS Transporte FROM RegistroComedor " &
                "   WHERE Fecha >= DATEADD(DAY, -6, CAST(GETDATE() AS date)) " &
                "   GROUP BY CONVERT(date, Fecha) " &
                "   UNION ALL " &
                "   SELECT CONVERT(date, Fecha) AS Fecha, 0 AS Comedor, 0 AS ComedorBecados, COUNT(1) AS Transporte FROM RegistroTransporte " &
                "   WHERE Fecha >= DATEADD(DAY, -6, CAST(GETDATE() AS date)) " &
                "   GROUP BY CONVERT(date, Fecha) " &
                ") F GROUP BY CONVERT(date, F.Fecha) ORDER BY Dia",
                Cn:=cn)

            Dim indexByDay As New Dictionary(Of Date, DataRow)()
            If dsSerie.Tables.Count > 0 Then
                For Each row As DataRow In dsSerie.Tables(0).Rows
                    indexByDay(CDate(row("Dia")).Date) = row
                Next
            End If

            For i As Integer = 6 To 0 Step -1
                Dim day As Date = hoy.AddDays(-i)
                Dim metric As New DailyMetric() With {
                    .Label = day.ToString("dd/MM"),
                    .Comedor = 0,
                    .ComedorBecados = 0,
                    .Transporte = 0
                }
                If indexByDay.ContainsKey(day) Then
                    Dim row As DataRow = indexByDay(day)
                    metric.Comedor = If(IsDBNull(row("Comedor")), 0, CInt(row("Comedor")))
                    metric.ComedorBecados = If(IsDBNull(row("ComedorBecados")), 0, CInt(row("ComedorBecados")))
                    metric.Transporte = If(IsDBNull(row("Transporte")), 0, CInt(row("Transporte")))
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

            If snapshot.MarcasComedorHoy < snapshot.MarcasComedorAyer Then
                snapshot.Alertas.Add("- Caida en comedor vs ayer.")
            End If
            If snapshot.MarcasTransporteHoy < snapshot.MarcasTransporteAyer Then
                snapshot.Alertas.Add("- Caida en transporte vs ayer.")
            End If
            If snapshot.BecadosComedorHoy = 0 Then
                snapshot.Alertas.Add("- Sin becados en comedor hoy.")
            End If
            If snapshot.BecadosTransporteHoy = 0 Then
                snapshot.Alertas.Add("- Sin becados en transporte hoy.")
            End If

            _cls.CerrarConexion(cn)
        End Using

        Return snapshot
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
End Class
