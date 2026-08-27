Imports System.Collections.Generic

Public Class DashboardContracts
    Public Enum DashboardPeriodKind
        CalendarDays = 0
        SchoolDays = 1
    End Enum

    Public Enum ComparisonTrend
        Flat = 0
        Up = 1
        Down = 2
        NoBase = 3
    End Enum

    Public Enum AlertSeverity
        Info = 0
        Warning = 1
        Critical = 2
    End Enum

    Public Class DashboardAlert
        Public Property Code As String
        Public Property Title As String
        Public Property Message As String
        Public Property Severity As AlertSeverity
        Public Property SortOrder As Integer
    End Class

    Public Class DashboardComparisonItem
        Public Property Label As String
        Public Property CurrentValue As Integer
        Public Property PreviousValue As Integer
        Public Property DeltaText As String
        Public Property StatusText As String
        Public Property Trend As ComparisonTrend
    End Class

    Public Class DailyMetric
        Public Property MetricDate As Date
        Public Property DayNameShort As String
        Public Property Label As String
        Public Property Comedor As Integer
        Public Property ComedorBecados As Integer
        Public Property Transporte As Integer
        Public Property TransporteConRuta As Integer
    End Class

    Public Class Snapshot
        Public Property BecadosComedorHoy As Integer
        Public Property EstudiantesConRutaHoy As Integer
        Public Property EstudiantesConRutaAyer As Integer
        Public Property MarcasComedorHoy As Integer
        Public Property MarcasTransporteHoy As Integer
        Public Property MarcasComedorAyer As Integer
        Public Property MarcasTransporteAyer As Integer
        Public Property Series As List(Of DailyMetric)
        Public Property Alertas As List(Of DashboardAlert)
        Public Property Comparativos As List(Of DashboardComparisonItem)
        Public Property TopRutas As List(Of String)
        Public Property PeriodLabel As String
        Public Property ComparisonLabel As String
        Public Property PeriodKind As DashboardPeriodKind
    End Class
End Class
