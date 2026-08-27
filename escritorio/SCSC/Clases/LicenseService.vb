Option Strict On
Option Explicit On

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web.Script.Serialization

Public Class LicensePayload
    Public Property CustomerName As String
    Public Property SiteName As String
    Public Property ClientId As String
    Public Property LicensedTo As String
    Public Property DatabaseServer As String
    Public Property DatabaseName As String
    Public Property Edition As String
    Public Property GeneratedAtUtc As DateTime
    Public Property ActivationCode As String
End Class

Public NotInheritable Class LicenseService
    Private Shared ReadOnly Serializer As New JavaScriptSerializer()
    Private Const LicenseFolderName As String = "license"
    Private Const LicenseFileName As String = "license.dat"
    ' Secreto simple para validación local. Cambiar si se quiere invalidar códigos emitidos previamente.
    Private Const ActivationSecret As String = "SCSC_ESCOLAR_2026"

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property LicenseDirectoryPath As String
        Get
            Return Path.Combine(DeploymentConfigService.ConfigDirectoryPath, LicenseFolderName)
        End Get
    End Property

    Public Shared ReadOnly Property LicenseFilePath As String
        Get
            Return Path.Combine(LicenseDirectoryPath, LicenseFileName)
        End Get
    End Property

    Public Shared Function HasInstalledLicenseFile() As Boolean
        Return File.Exists(LicenseFilePath)
    End Function

    Public Shared Function BuildRequestCode(ByVal customerName As String,
                                            ByVal siteName As String,
                                            ByVal clientId As String,
                                            ByVal databaseServer As String,
                                            ByVal databaseName As String) As String
        Dim payload As New Dictionary(Of String, Object)()
        payload("customerName") = NormalizeValue(customerName)
        payload("siteName") = NormalizeValue(siteName)
        payload("clientId") = NormalizeValue(clientId)
        payload("databaseServer") = NormalizeValue(databaseServer)
        payload("databaseName") = NormalizeValue(databaseName)
        payload("machineName") = NormalizeValue(Environment.MachineName)
        payload("generatedAtUtc") = DateTime.UtcNow.ToString("o")
        payload("appVersion") = My.Application.Info.Version.ToString()

        Dim json As String = Serializer.Serialize(payload)
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
    End Function

    Public Shared Function GenerateActivationCode(ByVal customerName As String,
                                                  ByVal siteName As String,
                                                  ByVal clientId As String,
                                                  ByVal databaseServer As String,
                                                  ByVal databaseName As String,
                                                  Optional ByVal edition As String = "Standard") As String
        Dim raw As String = String.Join("|",
                                        NormalizeValue(customerName),
                                        NormalizeValue(siteName),
                                        NormalizeValue(clientId),
                                        NormalizeValue(databaseServer),
                                        NormalizeValue(databaseName),
                                        NormalizeValue(edition),
                                        ActivationSecret)

        Using sha As SHA256 = SHA256.Create()
            Dim hash As Byte() = sha.ComputeHash(Encoding.UTF8.GetBytes(raw))
            Dim encoded As String = Convert.ToBase64String(hash).TrimEnd("="c).Replace("+"c, "-"c).Replace("/"c, "_"c)
            encoded = encoded.ToUpperInvariant()
            If encoded.Length > 24 Then
                encoded = encoded.Substring(0, 24)
            End If
            Return FormatActivationCode(encoded)
        End Using
    End Function

    Public Shared Function TryImportActivationCode(ByVal customerName As String,
                                                   ByVal siteName As String,
                                                   ByVal clientId As String,
                                                   ByVal licensedTo As String,
                                                   ByVal databaseServer As String,
                                                   ByVal databaseName As String,
                                                   ByVal activationCode As String,
                                                   ByRef errorMessage As String,
                                                   Optional ByVal edition As String = "Standard") As Boolean
        errorMessage = String.Empty

        If String.IsNullOrWhiteSpace(activationCode) Then
            errorMessage = "Debe indicar un código de activación."
            Return False
        End If

        Dim expected As String = GenerateActivationCode(customerName, siteName, clientId, databaseServer, databaseName, edition)
        If Not String.Equals(NormalizeActivationCode(activationCode), NormalizeActivationCode(expected), StringComparison.Ordinal) Then
            errorMessage = "El código de activación no coincide con cliente, sede o base de datos."
            Return False
        End If

        Try
            Directory.CreateDirectory(LicenseDirectoryPath)
            Dim payload As New LicensePayload()
            payload.CustomerName = customerName.Trim()
            payload.SiteName = siteName.Trim()
            payload.ClientId = clientId.Trim()
            payload.LicensedTo = If(String.IsNullOrWhiteSpace(licensedTo), customerName.Trim(), licensedTo.Trim())
            payload.DatabaseServer = databaseServer.Trim()
            payload.DatabaseName = databaseName.Trim()
            payload.Edition = edition.Trim()
            payload.GeneratedAtUtc = DateTime.UtcNow
            payload.ActivationCode = NormalizeActivationCode(activationCode)

            Dim json As String = Serializer.Serialize(payload)
            File.WriteAllText(LicenseFilePath, json, New UTF8Encoding(False))
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo guardar la licencia: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function ValidateInstalledLicense(ByVal databaseServer As String,
                                                    ByVal databaseName As String,
                                                    ByRef errorMessage As String,
                                                    Optional ByRef payload As LicensePayload = Nothing) As Boolean
        errorMessage = String.Empty
        payload = Nothing

        Dim loaded As LicensePayload = Nothing
        If Not TryLoadInstalledLicensePayload(loaded, errorMessage) OrElse loaded Is Nothing Then
            Return False
        End If

        Dim expected As String = GenerateActivationCode(loaded.CustomerName,
                                                        loaded.SiteName,
                                                        loaded.ClientId,
                                                        databaseServer,
                                                        databaseName,
                                                        loaded.Edition)

        If Not String.Equals(NormalizeActivationCode(loaded.ActivationCode), NormalizeActivationCode(expected), StringComparison.Ordinal) Then
            errorMessage = "La licencia no corresponde a esta base de datos."
            Return False
        End If

        payload = loaded
        Return True
    End Function

    Public Shared Function TryLoadInstalledLicensePayload(ByRef payload As LicensePayload, ByRef errorMessage As String) As Boolean
        errorMessage = String.Empty
        payload = Nothing

        If Not File.Exists(LicenseFilePath) Then
            errorMessage = "No existe licencia instalada."
            Return False
        End If

        Try
            Dim json As String = File.ReadAllText(LicenseFilePath, Encoding.UTF8)
            Dim loaded As LicensePayload = Serializer.Deserialize(Of LicensePayload)(json)
            If loaded Is Nothing Then
                errorMessage = "La licencia instalada es inválida."
                Return False
            End If

            payload = loaded
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo leer la licencia instalada: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function GetInstalledLicenseSummary(ByVal databaseServer As String, ByVal databaseName As String) As String
        Dim payload As LicensePayload = Nothing
        Dim errorMessage As String = String.Empty
        If Not ValidateInstalledLicense(databaseServer, databaseName, errorMessage, payload) OrElse payload Is Nothing Then
            Return errorMessage
        End If

        Return String.Format("Cliente: {0} | Sitio: {1} | Base: {2}/{3} | Edición: {4}",
                             payload.CustomerName,
                             payload.SiteName,
                             payload.DatabaseServer,
                             payload.DatabaseName,
                             payload.Edition)
    End Function

    Private Shared Function NormalizeValue(ByVal value As String) As String
        If String.IsNullOrWhiteSpace(value) Then
            Return String.Empty
        End If

        Return value.Trim().ToUpperInvariant()
    End Function

    Private Shared Function NormalizeActivationCode(ByVal activationCode As String) As String
        If String.IsNullOrWhiteSpace(activationCode) Then
            Return String.Empty
        End If

        Return activationCode.Trim().Replace("-", String.Empty).Replace(" ", String.Empty).ToUpperInvariant()
    End Function

    Private Shared Function FormatActivationCode(ByVal raw As String) As String
        Dim normalized As String = NormalizeActivationCode(raw)
        Dim groups As New List(Of String)()
        For i As Integer = 0 To normalized.Length - 1 Step 4
            Dim count As Integer = Math.Min(4, normalized.Length - i)
            groups.Add(normalized.Substring(i, count))
        Next
        Return String.Join("-", groups.ToArray())
    End Function
End Class
