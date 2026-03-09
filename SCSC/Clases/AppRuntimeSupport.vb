Option Explicit On
Option Strict On

Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Text

Public Module AppRuntimeSupport
    Private Const SecretPlaceholderPrefix As String = "__SET_IN_ENV__"

    Public Function GetAppSettingValue(ByVal key As String, Optional ByVal defaultValue As String = "") As String
        If String.Equals(key, "Conexion", StringComparison.OrdinalIgnoreCase) Then
            Dim externalConnectionString As String = DeploymentConfigService.GetConnectionString()
            If Not String.IsNullOrWhiteSpace(externalConnectionString) Then
                Return externalConnectionString
            End If
        End If

        Dim envValue As String = ReadEnvironmentOverride(key)
        If Not String.IsNullOrWhiteSpace(envValue) Then
            Return envValue.Trim()
        End If

        Try
            Dim raw As String = Convert.ToString(ConfigurationManager.AppSettings(key))
            If IsPlaceholderValue(raw) Then
                Return defaultValue
            End If
            If Not String.IsNullOrWhiteSpace(raw) Then
                Return raw.Trim()
            End If
        Catch
        End Try

        Return defaultValue
    End Function

    Public Function GetAppSettingBoolean(ByVal key As String, ByVal defaultValue As Boolean) As Boolean
        Dim raw As String = GetAppSettingValue(key, String.Empty)
        If String.IsNullOrWhiteSpace(raw) Then
            Return defaultValue
        End If

        If String.Equals(raw, "1", StringComparison.Ordinal) Then
            Return True
        End If

        If String.Equals(raw, "0", StringComparison.Ordinal) Then
            Return False
        End If

        Dim parsed As Boolean
        If Boolean.TryParse(raw, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Public Function GetAppSettingInteger(ByVal key As String, ByVal defaultValue As Integer) As Integer
        Dim raw As String = GetAppSettingValue(key, String.Empty)
        If String.IsNullOrWhiteSpace(raw) Then
            Return defaultValue
        End If

        Dim parsed As Integer
        If Integer.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, parsed) Then
            Return parsed
        End If

        If Integer.TryParse(raw, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Public Function GetAppSettingDouble(ByVal key As String, ByVal defaultValue As Double) As Double
        Dim raw As String = GetAppSettingValue(key, String.Empty)
        If String.IsNullOrWhiteSpace(raw) Then
            Return defaultValue
        End If

        Dim parsed As Double
        If Double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then
            Return parsed
        End If

        If Double.TryParse(raw, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Public Function GetAppSettingDecimal(ByVal key As String, ByVal defaultValue As Decimal) As Decimal
        Dim raw As String = GetAppSettingValue(key, String.Empty)
        If String.IsNullOrWhiteSpace(raw) Then
            Return defaultValue
        End If

        Dim parsed As Decimal
        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, parsed) Then
            Return parsed
        End If

        If Decimal.TryParse(raw, NumberStyles.Any, CultureInfo.CurrentCulture, parsed) Then
            Return parsed
        End If

        Return defaultValue
    End Function

    Public Function ResolveResourcePath(ByVal fileName As String) As String
        If String.IsNullOrWhiteSpace(fileName) Then
            Return String.Empty
        End If

        If Path.IsPathRooted(fileName) AndAlso File.Exists(fileName) Then
            Return fileName
        End If

        Dim candidates As String() = {
            Path.Combine(Application.StartupPath, "Resources", fileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Resources", fileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Resources", fileName),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources", fileName)
        }

        For Each candidate As String In candidates
            Dim fullPath As String = Path.GetFullPath(candidate)
            If File.Exists(fullPath) Then
                Return fullPath
            End If
        Next

        Return Path.GetFullPath(candidates(0))
    End Function

    Public Function ResolveApplicationIconPath() As String
        Dim candidates As String() = {
            Path.Combine(Application.StartupPath, "favicon.ico"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favicon.ico"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "favicon.ico"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "favicon.ico")
        }

        For Each candidate As String In candidates
            Dim fullPath As String = Path.GetFullPath(candidate)
            If File.Exists(fullPath) Then
                Return fullPath
            End If
        Next

        Return Path.GetFullPath(candidates(0))
    End Function

    Private Function ReadEnvironmentOverride(ByVal key As String) As String
        Dim names As New List(Of String)()
        If String.Equals(key, "Conexion", StringComparison.OrdinalIgnoreCase) Then
            names.Add("SCSC_CONNECTION_STRING")
        End If

        Dim normalizedKey As String = NormalizeEnvironmentKey(key)
        names.Add("SCSC_APPSETTING_" & normalizedKey)
        names.Add("SCSC_" & normalizedKey)
        names.Add(key)

        For Each name As String In names
            Try
                Dim value As String = Environment.GetEnvironmentVariable(name)
                If Not String.IsNullOrWhiteSpace(value) Then
                    Return value
                End If
            Catch
            End Try
        Next

        Return String.Empty
    End Function

    Private Function NormalizeEnvironmentKey(ByVal key As String) As String
        Dim builder As New StringBuilder(key.Length)
        For Each ch As Char In key
            If Char.IsLetterOrDigit(ch) Then
                builder.Append(Char.ToUpperInvariant(ch))
            Else
                builder.Append("_"c)
            End If
        Next
        Return builder.ToString()
    End Function

    Private Function IsPlaceholderValue(ByVal raw As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(raw) AndAlso raw.Trim().StartsWith(SecretPlaceholderPrefix, StringComparison.OrdinalIgnoreCase)
    End Function
End Module
