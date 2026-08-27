Option Explicit On
Option Strict On

Imports System.Configuration
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Public Module AppRuntimeSupport
    Private Const SecretPlaceholderPrefix As String = "__SET_IN_ENV__"
    Public Const DbProfileKey As String = "DB_PROFILE"
    Public Const DbProfileLocal As String = "LOCAL"
    Public Const DbProfileInstalled As String = "INSTALLED"
    Public Const DbProfileLegacy As String = "LEGACY"
    Public Const LocalConnectionKey As String = "ConexionLocal"
    Public Const LegacyConnectionKey As String = "Conexion"

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Private Function DestroyIcon(ByVal handle As IntPtr) As Boolean
    End Function

    Public Function GetAppSettingValue(ByVal key As String, Optional ByVal defaultValue As String = "") As String
        If String.Equals(key, "Conexion", StringComparison.OrdinalIgnoreCase) Then
            Return ResolveConfiguredConnectionString(defaultValue)
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

    Public Function GetConnectionProfile() As String
        Dim rawProfile As String = GetRawAppSetting(DbProfileKey)
        Dim normalizedProfile As String = NormalizeConnectionProfile(rawProfile)

        If String.Equals(normalizedProfile, DbProfileLocal, StringComparison.OrdinalIgnoreCase) AndAlso Not HasLocalConnectionConfigured() Then
            Return DbProfileInstalled
        End If

        Return normalizedProfile
    End Function

    Public Function ResolveConfiguredConnectionString(Optional ByVal defaultValue As String = "") As String
        Dim localConnection As String = GetRawAppSetting(LocalConnectionKey)
        Dim legacyConnection As String = GetRawAppSetting(LegacyConnectionKey)
        Dim installedConnection As String = DeploymentConfigService.GetConnectionString()
        Return ResolveConnectionStringForProfile(GetConnectionProfile(), localConnection, installedConnection, legacyConnection, defaultValue)
    End Function

    Public Function ResolveConnectionStringForProfile(ByVal profile As String,
                                                      ByVal localConnection As String,
                                                      ByVal installedConnection As String,
                                                      ByVal legacyConnection As String,
                                                      Optional ByVal defaultValue As String = "") As String
        Select Case NormalizeConnectionProfile(profile)
            Case DbProfileLocal
                If Not String.IsNullOrWhiteSpace(localConnection) Then
                    Return localConnection.Trim()
                End If
            Case DbProfileLegacy
                If Not String.IsNullOrWhiteSpace(legacyConnection) Then
                    Return legacyConnection.Trim()
                End If
            Case Else
                If Not String.IsNullOrWhiteSpace(installedConnection) Then
                    Return installedConnection.Trim()
                End If
        End Select

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

    Public Function CreateApplicationIcon() As Icon
        Try
            If My.Resources.LogoIcon IsNot Nothing Then
                Dim runtimeIcon As Icon = CreateIconFromBitmap(My.Resources.LogoIcon, 64)
                If runtimeIcon IsNot Nothing Then
                    Return runtimeIcon
                End If
            End If
        Catch
        End Try

        Try
            If My.Resources.Login IsNot Nothing Then
                Dim runtimeIcon As Icon = CreateIconFromBitmap(My.Resources.Login, 64)
                If runtimeIcon IsNot Nothing Then
                    Return runtimeIcon
                End If
            End If
        Catch
        End Try

        Try
            Dim iconPath As String = ResolveApplicationIconPath()
            If File.Exists(iconPath) Then
                Return New Icon(iconPath)
            End If
        Catch
        End Try

        Return Nothing
    End Function

    Private Function CreateIconFromBitmap(ByVal source As Bitmap, ByVal size As Integer) As Icon
        If source Is Nothing Then
            Return Nothing
        End If

        Dim hIcon As IntPtr = IntPtr.Zero
        Try
            Using resized As New Bitmap(source, New Size(size, size))
                hIcon = resized.GetHicon()
                Using tmp As Icon = Icon.FromHandle(hIcon)
                    Return DirectCast(tmp.Clone(), Icon)
                End Using
            End Using
        Catch
            Return Nothing
        Finally
            If hIcon <> IntPtr.Zero Then
                DestroyIcon(hIcon)
            End If
        End Try
    End Function

    Private Function ReadEnvironmentOverride(ByVal key As String) As String
        Dim names As New List(Of String)()

        Dim normalizedKey As String = NormalizeEnvironmentKey(key)
        names.Add("SCSC_APPSETTING_" & normalizedKey)
        names.Add("SCSC_" & normalizedKey)
        names.Add(key)

        For Each name As String In names
            Dim value As String = ReadEnvironmentValue(name)
            If Not String.IsNullOrWhiteSpace(value) Then
                Return value
            End If
        Next

        Return String.Empty
    End Function

    Private Function GetRawAppSetting(ByVal key As String) As String
        Try
            Dim raw As String = Convert.ToString(ConfigurationManager.AppSettings(key))
            If IsPlaceholderValue(raw) Then
                Return String.Empty
            End If

            If Not String.IsNullOrWhiteSpace(raw) Then
                Return raw.Trim()
            End If
        Catch
        End Try

        Return String.Empty
    End Function

    Private Function HasLocalConnectionConfigured() As Boolean
        Return Not String.IsNullOrWhiteSpace(GetRawAppSetting(LocalConnectionKey))
    End Function

    Private Function ReadEnvironmentValue(ByVal name As String) As String
        Try
            Return Environment.GetEnvironmentVariable(name)
        Catch
            Return String.Empty
        End Try
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

    Public Function NormalizeConnectionProfile(ByVal profile As String) As String
        Dim normalized As String = If(profile, String.Empty).Trim().ToUpperInvariant()
        Select Case normalized
            Case DbProfileLocal, DbProfileInstalled, DbProfileLegacy
                Return normalized
            Case Else
                Return DbProfileInstalled
        End Select
    End Function

    Private Function IsPlaceholderValue(ByVal raw As String) As Boolean
        Return Not String.IsNullOrWhiteSpace(raw) AndAlso raw.Trim().StartsWith(SecretPlaceholderPrefix, StringComparison.OrdinalIgnoreCase)
    End Function
End Module
