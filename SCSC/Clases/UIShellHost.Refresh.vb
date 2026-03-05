Imports System.Drawing
Imports System.Windows.Forms

Public Partial Class UIShellHost
    Public Sub SetDashboardLastUpdate(ByVal value As Date)
        If _lastUpdateBadge Is Nothing Then
            Exit Sub
        End If
        _lastUpdateBadge.Text = "Actualizacion: " & value.ToString("yyyy/MM/dd HH:mm:ss")
    End Sub

    Public Sub SetDashboardLastSuccessUpdate(ByVal value As Date)
        If _lastSuccessBadge Is Nothing Then
            Exit Sub
        End If
        _lastSuccessBadge.Text = "Ultimo OK: " & value.ToString("yyyy/MM/dd HH:mm:ss")
    End Sub

    Public Sub SetDashboardRefreshing(ByVal isRefreshing As Boolean)
        If _refreshButton Is Nothing Then
            Exit Sub
        End If

        _refreshButton.Enabled = Not isRefreshing
        If _refreshIntervalCombo IsNot Nothing Then
            _refreshIntervalCombo.Enabled = Not isRefreshing
        End If
        _refreshButton.Text = If(isRefreshing, "Actualizando...", "Actualizar ahora")
        If _lastUpdateBadge IsNot Nothing Then
            _lastUpdateBadge.ForeColor = If(isRefreshing, Color.FromArgb(28, 93, 158), Color.FromArgb(76, 90, 112))
        End If
        If isRefreshing AndAlso _refreshStatusBadge IsNot Nothing Then
            _refreshStatusBadge.Text = "Estado: sincronizando datos..."
            _refreshStatusBadge.ForeColor = Color.FromArgb(28, 93, 158)
        End If
    End Sub

    Public Sub SetDashboardRefreshResult(ByVal success As Boolean, ByVal message As String)
        If _refreshStatusBadge Is Nothing Then
            Exit Sub
        End If

        Dim safeMessage As String = If(String.IsNullOrWhiteSpace(message), "sin detalle", message.Trim())
        If success Then
            _refreshStatusBadge.Text = "Estado: actualizado (" & safeMessage & ")"
            _refreshStatusBadge.ForeColor = Color.FromArgb(24, 121, 78)
        Else
            _refreshStatusBadge.Text = "Estado: error de sincronizacion (" & safeMessage & ")"
            _refreshStatusBadge.ForeColor = Color.FromArgb(173, 52, 61)
        End If
    End Sub

    Public Sub SetDashboardAutoRefreshEnabled(ByVal enabled As Boolean)
        If _autoRefreshToggleButton Is Nothing Then
            Exit Sub
        End If

        If enabled Then
            _autoRefreshToggleButton.Text = "Auto ON"
            _autoRefreshToggleButton.ForeColor = Color.FromArgb(24, 121, 78)
            _autoRefreshToggleButton.BackColor = Color.FromArgb(226, 245, 233)
            _autoRefreshToggleButton.FlatAppearance.BorderColor = Color.FromArgb(181, 227, 198)
        Else
            _autoRefreshToggleButton.Text = "Auto OFF"
            _autoRefreshToggleButton.ForeColor = Color.FromArgb(116, 80, 17)
            _autoRefreshToggleButton.BackColor = Color.FromArgb(255, 245, 218)
            _autoRefreshToggleButton.FlatAppearance.BorderColor = Color.FromArgb(244, 219, 166)
        End If
    End Sub

    Public Sub SetDashboardNextRefreshText(ByVal text As String)
        If _nextRefreshBadge Is Nothing Then
            Exit Sub
        End If
        _nextRefreshBadge.Text = "Prox. refresh: " & If(String.IsNullOrWhiteSpace(text), "--", text.Trim())
    End Sub

    Public Sub SetDashboardRefreshQueueCount(ByVal count As Integer)
        If _refreshQueueBadge Is Nothing Then
            Exit Sub
        End If

        Dim safeCount As Integer = Math.Max(0, count)
        _refreshQueueBadge.Text = "Cola refresh: " & safeCount.ToString()
        If safeCount > 0 Then
            _refreshQueueBadge.ForeColor = Color.FromArgb(116, 80, 17)
        Else
            _refreshQueueBadge.ForeColor = Color.FromArgb(98, 111, 129)
        End If
    End Sub

    Public Sub SetDashboardRefreshIntervalSeconds(ByVal seconds As Integer)
        If _refreshIntervalCombo Is Nothing Then
            Exit Sub
        End If

        Dim token As String = Math.Max(30, seconds).ToString() & "s"
        _suppressRefreshIntervalEvent = True
        Try
            If _refreshIntervalCombo.Items.Contains(token) Then
                _refreshIntervalCombo.SelectedItem = token
            Else
                _refreshIntervalCombo.SelectedItem = "60s"
            End If
        Finally
            _suppressRefreshIntervalEvent = False
        End Try
    End Sub

    Private Sub RefreshIntervalCombo_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If _suppressRefreshIntervalEvent OrElse _refreshIntervalCombo Is Nothing Then
            Exit Sub
        End If

        Dim raw As String = CStr(_refreshIntervalCombo.SelectedItem)
        Select Case raw
            Case "30s"
                SafeNavigate("dashboard_interval_30")
            Case "60s"
                SafeNavigate("dashboard_interval_60")
            Case "120s"
                SafeNavigate("dashboard_interval_120")
        End Select
    End Sub
End Class
