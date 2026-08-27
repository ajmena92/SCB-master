Option Explicit On
Option Strict On

Public NotInheritable Class ServerClock
    Private Shared ReadOnly SyncRoot As New Object()
    Private Shared _serverBase As DateTime = DateTime.MinValue
    Private Shared _localBase As DateTime = DateTime.MinValue

    Private Sub New()
    End Sub

    Public Shared Sub Sync(ByVal serverNow As DateTime)
        SyncLock SyncRoot
            _serverBase = serverNow
            _localBase = DateTime.Now
            FechaServer = serverNow
        End SyncLock
    End Sub

    Public Shared Function Now() As DateTime
        SyncLock SyncRoot
            If _serverBase = DateTime.MinValue OrElse _localBase = DateTime.MinValue Then
                If FechaServer <> DateTime.MinValue Then
                    Return FechaServer
                End If
                Return DateTime.Now
            End If

            Return _serverBase.Add(DateTime.Now.Subtract(_localBase))
        End SyncLock
    End Function

    Public Shared Function Today() As Date
        Return Now().Date
    End Function
End Class
