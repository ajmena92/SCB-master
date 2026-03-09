Option Strict On
Option Explicit On

Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web.Script.Serialization
Imports Microsoft.Win32

Public Class LicensePayload
    Public Property CustomerName As String
    Public Property SiteName As String
    Public Property ClientId As String
    Public Property LicensedTo As String
    Public Property HardwareFingerprint As String
    Public Property IssueDateUtc As DateTime
    Public Property ExpiryDateUtc As Nullable(Of DateTime)
    Public Property Edition As String
    Public Property MaxSeats As Nullable(Of Integer)
End Class

Public Class LicenseEnvelope
    Public Property PayloadBase64 As String
    Public Property SignatureBase64 As String
End Class

Public NotInheritable Class LicenseService
    Private Shared ReadOnly Serializer As New JavaScriptSerializer()
    Private Const LicenseFolderName As String = "license"
    Private Const LicenseFileName As String = "license.dat"
    Private Const PublicKeyXml As String = "<RSAKeyValue><Modulus>n8K6JQ2NQ9e0ABm0c7vN6LJm0hQ1SckV9Oq3jA2h1gKScx8/0sK3K+JvAkDq6O1b0m6P3hNQK8v9JfH8yP65YyGmQMMhshG1u1kI9Ykif0gL4N9Or0xlN7s1lQTF7g9s7vX9m+7m4Pci7kO2Fv5YP+EsC2S+WvSQjU5p1R2Rj4dN2obP9vXAZshH5tvq4k41wVv5v7E6Y5P4vD8K9EX5E9P7Q7N6w3D8P0gkW0lWQwD6c0v4l8rqYwK8+4L7cEmM4M97cV2m5bYdDoh6y9K4m8GQjCQ2hQdQ4lsPahc4m1rA1FQwYVw9hOymNmtjXy3YvYpK9cL1w==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>"

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

    Public Shared Function ComputeMachineFingerprint() As String
        Dim machineGuid As String = String.Empty
        Try
            Using key As RegistryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\Microsoft\Cryptography", False)
                If key IsNot Nothing Then
                    machineGuid = Convert.ToString(key.GetValue("MachineGuid", String.Empty)).Trim()
                End If
            End Using
        Catch
        End Try

        Dim material As String = Environment.MachineName.Trim().ToUpperInvariant() & "|" &
            Environment.UserDomainName.Trim().ToUpperInvariant() & "|" &
            machineGuid.ToUpperInvariant()

        Using sha As SHA256 = SHA256.Create()
            Dim hash As Byte() = sha.ComputeHash(Encoding.UTF8.GetBytes(material))
            Return Convert.ToBase64String(hash).TrimEnd("="c).Replace("+"c, "-"c).Replace("/"c, "_"c)
        End Using
    End Function

    Public Shared Function BuildRequestCode(ByVal customerName As String, ByVal siteName As String) As String
        Dim payload As New Dictionary(Of String, Object)()
        payload("customerName") = If(customerName, String.Empty).Trim()
        payload("siteName") = If(siteName, String.Empty).Trim()
        payload("machineName") = Environment.MachineName.Trim()
        payload("machineFingerprint") = ComputeMachineFingerprint()
        payload("generatedAtUtc") = DateTime.UtcNow.ToString("o")
        payload("appVersion") = My.Application.Info.Version.ToString()

        Dim json As String = Serializer.Serialize(payload)
        Return Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
    End Function

    Public Shared Function TryImportActivationCode(ByVal activationCode As String, ByRef errorMessage As String) As Boolean
        errorMessage = String.Empty

        If String.IsNullOrWhiteSpace(activationCode) Then
            errorMessage = "Debe indicar un código de activación."
            Return False
        End If

        Dim envelope As LicenseEnvelope = Nothing
        If Not TryParseEnvelope(activationCode, envelope, errorMessage) Then
            Return False
        End If

        Dim payload As LicensePayload = Nothing
        If Not ValidateEnvelope(envelope, payload, errorMessage) Then
            Return False
        End If

        Try
            Directory.CreateDirectory(LicenseDirectoryPath)
            Dim normalized As String = Serializer.Serialize(envelope)
            File.WriteAllText(LicenseFilePath, normalized, New UTF8Encoding(False))
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo guardar la licencia: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function ValidateInstalledLicense(ByRef errorMessage As String, Optional ByRef payload As LicensePayload = Nothing) As Boolean
        errorMessage = String.Empty
        payload = Nothing

        If Not File.Exists(LicenseFilePath) Then
            errorMessage = "No existe licencia instalada."
            Return False
        End If

        Try
            Dim activationCode As String = File.ReadAllText(LicenseFilePath, Encoding.UTF8)
            Dim envelope As LicenseEnvelope = Nothing
            If Not TryParseEnvelope(activationCode, envelope, errorMessage) Then
                Return False
            End If

            Return ValidateEnvelope(envelope, payload, errorMessage)
        Catch ex As Exception
            errorMessage = "No se pudo leer la licencia instalada: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function GetInstalledLicenseSummary() As String
        Dim payload As LicensePayload = Nothing
        Dim errorMessage As String = String.Empty
        If Not ValidateInstalledLicense(errorMessage, payload) OrElse payload Is Nothing Then
            Return errorMessage
        End If

        Dim expiryText As String = If(payload.ExpiryDateUtc.HasValue, payload.ExpiryDateUtc.Value.ToLocalTime().ToString("dd/MM/yyyy"), "Sin vencimiento")
        Return String.Format("Cliente: {0} | Sitio: {1} | Edición: {2} | Vence: {3}",
                             payload.CustomerName,
                             payload.SiteName,
                             payload.Edition,
                             expiryText)
    End Function

    Private Shared Function TryParseEnvelope(ByVal activationCode As String, ByRef envelope As LicenseEnvelope, ByRef errorMessage As String) As Boolean
        envelope = Nothing
        errorMessage = String.Empty

        Try
            Dim trimmed As String = activationCode.Trim()
            Dim json As String
            If trimmed.StartsWith("{", StringComparison.Ordinal) Then
                json = trimmed
            Else
                json = Encoding.UTF8.GetString(Convert.FromBase64String(trimmed))
            End If

            envelope = Serializer.Deserialize(Of LicenseEnvelope)(json)
            If envelope Is Nothing OrElse String.IsNullOrWhiteSpace(envelope.PayloadBase64) OrElse String.IsNullOrWhiteSpace(envelope.SignatureBase64) Then
                errorMessage = "El código de activación no tiene el formato esperado."
                Return False
            End If

            Return True
        Catch ex As Exception
            errorMessage = "No se pudo interpretar el código de activación: " & ex.Message
            Return False
        End Try
    End Function

    Private Shared Function ValidateEnvelope(ByVal envelope As LicenseEnvelope, ByRef payload As LicensePayload, ByRef errorMessage As String) As Boolean
        payload = Nothing
        errorMessage = String.Empty

        Try
            Dim payloadJson As String = Encoding.UTF8.GetString(Convert.FromBase64String(envelope.PayloadBase64))
            Dim signature As Byte() = Convert.FromBase64String(envelope.SignatureBase64)

            If Not VerifySignature(payloadJson, signature) Then
                errorMessage = "La firma de la licencia no es válida."
                Return False
            End If

            payload = Serializer.Deserialize(Of LicensePayload)(payloadJson)
            If payload Is Nothing Then
                errorMessage = "El contenido de la licencia es inválido."
                Return False
            End If

            If Not String.Equals(If(payload.HardwareFingerprint, String.Empty).Trim(), ComputeMachineFingerprint(), StringComparison.Ordinal) Then
                errorMessage = "La licencia no corresponde a este equipo."
                Return False
            End If

            If payload.ExpiryDateUtc.HasValue AndAlso DateTime.UtcNow > payload.ExpiryDateUtc.Value Then
                errorMessage = "La licencia está vencida."
                Return False
            End If

            Return True
        Catch ex As Exception
            errorMessage = "No se pudo validar la licencia: " & ex.Message
            Return False
        End Try
    End Function

    Private Shared Function VerifySignature(ByVal payloadJson As String, ByVal signature As Byte()) As Boolean
        Using rsa As New RSACryptoServiceProvider()
            rsa.FromXmlString(PublicKeyXml)
            Dim data As Byte() = Encoding.UTF8.GetBytes(payloadJson)
            Return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature)
        End Using
    End Function
End Class
