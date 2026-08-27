Imports System.IO
Imports System.Text

Public NotInheritable Class ErrorLogger
    Private Shared ReadOnly SyncRoot As New Object()

    Private Sub New()
    End Sub

    Public Shared Sub LogInfo(ByVal source As String, ByVal message As String)
        WriteEntry("INFO", source, message, Nothing)
    End Sub

    Public Shared Sub LogError(ByVal source As String, ByVal message As String)
        WriteEntry("ERROR", source, message, Nothing)
    End Sub

    Public Shared Sub LogException(ByVal source As String, ByVal ex As Exception, Optional ByVal extra As String = "")
        If ex Is Nothing Then
            WriteEntry("ERROR", source, If(String.IsNullOrWhiteSpace(extra), "Excepcion nula.", extra), Nothing)
            Exit Sub
        End If

        Dim details As New StringBuilder()
        If Not String.IsNullOrWhiteSpace(extra) Then
            details.AppendLine(extra.Trim())
        End If
        details.AppendLine("Mensaje: " & ex.Message)
        details.AppendLine("Tipo: " & ex.GetType().FullName)
        details.AppendLine("StackTrace:")
        details.AppendLine(ex.StackTrace)

        If Not ex.InnerException Is Nothing Then
            details.AppendLine("InnerException:")
            details.AppendLine(ex.InnerException.ToString())
        End If

        WriteEntry("EXCEPTION", source, details.ToString(), ex)
    End Sub

    Public Shared Function GetCurrentLogPath() As String
        Return BuildLogPath()
    End Function

    Private Shared Sub WriteEntry(ByVal level As String, ByVal source As String, ByVal message As String, ByVal ex As Exception)
        Try
            Dim logPath As String = BuildLogPath()
            Dim logDir As String = Path.GetDirectoryName(logPath)
            If String.IsNullOrWhiteSpace(logDir) Then
                Exit Sub
            End If

            If Not Directory.Exists(logDir) Then
                Directory.CreateDirectory(logDir)
            End If

            Dim line As String = String.Format(
                "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] [{2}] {3}",
                DateTime.Now,
                level,
                If(String.IsNullOrWhiteSpace(source), "General", source.Trim()),
                If(message, String.Empty).Replace(vbCr, " ").Replace(vbLf, " "))

            SyncLock SyncRoot
                File.AppendAllText(logPath, line & Environment.NewLine, Encoding.UTF8)
                If ex IsNot Nothing Then
                    File.AppendAllText(logPath, ex.ToString() & Environment.NewLine, Encoding.UTF8)
                End If
            End SyncLock
        Catch
        End Try
    End Sub

    Private Shared Function BuildLogPath() As String
        Dim baseDir As String = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        If String.IsNullOrWhiteSpace(baseDir) Then
            baseDir = AppDomain.CurrentDomain.BaseDirectory
        End If

        Return Path.Combine(baseDir, "SCSC", "logs", "scsc_" & DateTime.Now.ToString("yyyyMMdd") & ".log")
    End Function
End Class
