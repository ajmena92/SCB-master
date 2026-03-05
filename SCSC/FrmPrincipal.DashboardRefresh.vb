Imports System
Imports System.Windows.Forms

Partial Class FrmPrincipal
    Private Sub RefreshDashboardDeferred()
        If _shellHost Is Nothing Then
            Exit Sub
        End If

        If _dashboardRefreshInProgress Then
            _dashboardRefreshPending = True
            Try
                _shellHost.SetDashboardRefreshResult(True, "refresh en cola")
                _shellHost.SetDashboardRefreshQueueCount(1)
            Catch ex As Exception
                ErrorLogger.LogException("FrmPrincipal.RefreshDashboardDeferred.Queue", ex)
            End Try
            Exit Sub
        End If

        Try
            _shellHost.SetDashboardRefreshQueueCount(0)
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.RefreshDashboardDeferred.ClearQueue", ex)
        End Try
        Me.BeginInvoke(New Action(AddressOf ExecuteDashboardRefreshSafe))
    End Sub

    Private Sub ExecuteDashboardRefreshSafe()
        If _shellHost Is Nothing OrElse _dashboardRefreshInProgress Then
            Exit Sub
        End If

        _dashboardRefreshInProgress = True
        Try
            _shellHost.SetDashboardRefreshing(True)
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.ExecuteDashboardRefreshSafe.SetRefreshingTrue", ex)
        End Try
        Try
            FechaServer = Date.Now()
            LoadDashboardSnapshot()
            ResetNextRefreshCountdown()
        Catch ex As Exception
            ErrorLogger.LogException("FrmPrincipal.ExecuteDashboardRefreshSafe", ex)
        Finally
            Try
                If _shellHost IsNot Nothing Then
                    _shellHost.SetDashboardRefreshing(False)
                    _shellHost.SetDashboardRefreshResult(_lastDashboardRefreshSuccess, _lastDashboardRefreshMessage)
                End If
            Catch ex As Exception
                ErrorLogger.LogException("FrmPrincipal.ExecuteDashboardRefreshSafe.SetRefreshingFalse", ex)
            End Try
            _dashboardRefreshInProgress = False
            If _dashboardRefreshPending Then
                _dashboardRefreshPending = False
                Try
                    If _shellHost IsNot Nothing Then _shellHost.SetDashboardRefreshQueueCount(0)
                Catch ex As Exception
                    ErrorLogger.LogException("FrmPrincipal.ExecuteDashboardRefreshSafe.QueuePending", ex)
                End Try
                Me.BeginInvoke(New Action(AddressOf ExecuteDashboardRefreshSafe))
            Else
                Try
                    If _shellHost IsNot Nothing Then _shellHost.SetDashboardRefreshQueueCount(0)
                Catch ex As Exception
                    ErrorLogger.LogException("FrmPrincipal.ExecuteDashboardRefreshSafe.QueueClear", ex)
                End Try
            End If
        End Try
    End Sub

    Private Sub EnsureDashboardRefreshTimer()
        If _dashboardRefreshTimer Is Nothing Then
            _dashboardRefreshTimer = New System.Windows.Forms.Timer()
            AddHandler _dashboardRefreshTimer.Tick, AddressOf DashboardRefreshTimer_Tick
        End If
        _dashboardRefreshTimer.Interval = _dashboardRefreshIntervalMs
        If _dashboardAutoRefreshEnabled Then
            _dashboardRefreshTimer.Start()
            ResetNextRefreshCountdown()
        Else
            _dashboardRefreshTimer.Stop()
        End If
    End Sub

    Private Sub EnsureDashboardCountdownTimer()
        If _dashboardCountdownTimer Is Nothing Then
            _dashboardCountdownTimer = New System.Windows.Forms.Timer()
            _dashboardCountdownTimer.Interval = 1000
            AddHandler _dashboardCountdownTimer.Tick, AddressOf DashboardCountdownTimer_Tick
        End If
        _dashboardCountdownTimer.Start()
    End Sub

    Private Sub DashboardCountdownTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        UpdateNextRefreshBadge()
    End Sub

    Private Sub ResetNextRefreshCountdown()
        If Not _dashboardAutoRefreshEnabled Then
            _nextDashboardRefreshAt = Date.MinValue
            UpdateNextRefreshBadge()
            Exit Sub
        End If
        _nextDashboardRefreshAt = Date.Now().AddMilliseconds(_dashboardRefreshIntervalMs)
        UpdateNextRefreshBadge()
    End Sub

    Private Sub UpdateNextRefreshBadge()
        If _shellHost Is Nothing Then
            Exit Sub
        End If

        If Not _dashboardAutoRefreshEnabled Then
            _shellHost.SetDashboardNextRefreshText("pausado")
            Exit Sub
        End If

        If _nextDashboardRefreshAt = Date.MinValue Then
            _shellHost.SetDashboardNextRefreshText("--")
            Exit Sub
        End If

        Dim remaining As TimeSpan = _nextDashboardRefreshAt.Subtract(Date.Now())
        If remaining.TotalSeconds < 0 Then
            remaining = TimeSpan.Zero
        End If
        _shellHost.SetDashboardNextRefreshText(remaining.Minutes.ToString("00") & ":" & remaining.Seconds.ToString("00"))
    End Sub

    Private Sub UpdateDashboardRefreshInterval(ByVal seconds As Integer)
        Dim safeSeconds As Integer = Math.Max(30, Math.Min(120, seconds))
        _dashboardRefreshIntervalMs = safeSeconds * 1000
        EnsureDashboardRefreshTimer()
        If _shellHost IsNot Nothing Then
            _shellHost.SetDashboardRefreshIntervalSeconds(safeSeconds)
            _shellHost.SetDashboardRefreshResult(True, "intervalo=" & safeSeconds.ToString() & "s")
        End If
        RefreshDashboardDeferred()
    End Sub

    Private Sub DashboardRefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)
        If _shellHost Is Nothing OrElse Not Me.Visible OrElse Not _dashboardAutoRefreshEnabled Then
            Exit Sub
        End If
        RefreshDashboardDeferred()
    End Sub

    Private Sub ToggleDashboardAutoRefresh()
        _dashboardAutoRefreshEnabled = Not _dashboardAutoRefreshEnabled
        EnsureDashboardRefreshTimer()
        ResetNextRefreshCountdown()
        If _shellHost IsNot Nothing Then
            _shellHost.SetDashboardAutoRefreshEnabled(_dashboardAutoRefreshEnabled)
            _shellHost.SetDashboardRefreshResult(True, If(_dashboardAutoRefreshEnabled, "auto-refresh habilitado", "auto-refresh pausado"))
        End If
    End Sub

    Private Sub FrmPrincipal_FormClosed(ByVal sender As Object, ByVal e As FormClosedEventArgs) Handles MyBase.FormClosed
        If _dashboardRefreshTimer IsNot Nothing Then
            RemoveHandler _dashboardRefreshTimer.Tick, AddressOf DashboardRefreshTimer_Tick
            _dashboardRefreshTimer.Stop()
            _dashboardRefreshTimer.Dispose()
            _dashboardRefreshTimer = Nothing
        End If
        If _dashboardCountdownTimer IsNot Nothing Then
            RemoveHandler _dashboardCountdownTimer.Tick, AddressOf DashboardCountdownTimer_Tick
            _dashboardCountdownTimer.Stop()
            _dashboardCountdownTimer.Dispose()
            _dashboardCountdownTimer = Nothing
        End If
    End Sub
End Class
