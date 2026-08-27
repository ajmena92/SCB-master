Option Strict On
Option Explicit On

Imports System.Data.SqlClient
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Web.Script.Serialization

Public NotInheritable Class DeploymentConfiguration
    Public Property Server As String
    Public Property Database As String
    Public Property AuthenticationMode As String
    Public Property UserName As String
    Public Property EncryptedPassword As String
    Public Property SchemaVersion As Integer
    Public Property UpdatedAtUtc As DateTime

    Public Sub New()
        AuthenticationMode = "Sql"
        SchemaVersion = 1
        UpdatedAtUtc = DateTime.UtcNow
    End Sub

    Public Function UseIntegratedSecurity() As Boolean
        Return String.Equals(AuthenticationMode, "Windows", StringComparison.OrdinalIgnoreCase)
    End Function

    Public Function GetPlainTextPassword() As String
        If String.IsNullOrWhiteSpace(EncryptedPassword) Then
            Return String.Empty
        End If

        Try
            Dim protectedBytes As Byte() = Convert.FromBase64String(EncryptedPassword)
            Dim clearBytes As Byte() = ProtectedData.Unprotect(protectedBytes, DeploymentConfigService.GetEntropyBytes(), DataProtectionScope.LocalMachine)
            Return Encoding.UTF8.GetString(clearBytes)
        Catch
            Return String.Empty
        End Try
    End Function

    Public Sub SetPlainTextPassword(ByVal password As String)
        If String.IsNullOrEmpty(password) Then
            EncryptedPassword = String.Empty
            Exit Sub
        End If

        Dim clearBytes As Byte() = Encoding.UTF8.GetBytes(password)
        Dim protectedBytes As Byte() = ProtectedData.Protect(clearBytes, DeploymentConfigService.GetEntropyBytes(), DataProtectionScope.LocalMachine)
        EncryptedPassword = Convert.ToBase64String(protectedBytes)
    End Sub

    Public Function BuildConnectionString() As String
        Dim builder As New SqlConnectionStringBuilder()
        builder.DataSource = If(String.IsNullOrWhiteSpace(Server), ".", Server.Trim())
        builder.InitialCatalog = If(String.IsNullOrWhiteSpace(Database), "SCSC", Database.Trim())
        builder.IntegratedSecurity = UseIntegratedSecurity()
        builder.MultipleActiveResultSets = False
        builder.ConnectTimeout = 15
        builder.ApplicationName = "SCSC"

        If Not builder.IntegratedSecurity Then
            builder.UserID = If(UserName, String.Empty).Trim()
            builder.Password = GetPlainTextPassword()
        End If

        Return builder.ConnectionString
    End Function
End Class

Public NotInheritable Class DeploymentConfigService
    Private Shared ReadOnly Serializer As New JavaScriptSerializer()
    Private Const ConfigFileName As String = "deployment.config.json"
    Private Const EntropyValue As String = "SCSC_DEPLOYMENT_CONFIG_V1"

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property ConfigDirectoryPath As String
        Get
            Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SCSC")
        End Get
    End Property

    Public Shared ReadOnly Property ConfigFilePath As String
        Get
            Return Path.Combine(ConfigDirectoryPath, ConfigFileName)
        End Get
    End Property

    Public Shared Function HasExternalConfiguration() As Boolean
        Return File.Exists(ConfigFilePath)
    End Function

    Public Shared Function TryLoad(ByRef config As DeploymentConfiguration, ByRef errorMessage As String) As Boolean
        config = Nothing
        errorMessage = String.Empty

        If Not File.Exists(ConfigFilePath) Then
            errorMessage = "No existe configuración externa de despliegue."
            Return False
        End If

        Try
            Dim json As String = File.ReadAllText(ConfigFilePath, Encoding.UTF8)
            Dim loaded As DeploymentConfiguration = Serializer.Deserialize(Of DeploymentConfiguration)(json)
            If loaded Is Nothing Then
                errorMessage = "El archivo de configuración externa está vacío o inválido."
                Return False
            End If

            config = loaded
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo leer la configuración externa: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function Save(ByVal config As DeploymentConfiguration, ByRef errorMessage As String) As Boolean
        errorMessage = String.Empty

        If config Is Nothing Then
            errorMessage = "La configuración no puede ser nula."
            Return False
        End If

        Try
            Directory.CreateDirectory(ConfigDirectoryPath)
            config.SchemaVersion = 1
            config.UpdatedAtUtc = DateTime.UtcNow
            Dim json As String = Serializer.Serialize(config)
            File.WriteAllText(ConfigFilePath, json, New UTF8Encoding(False))
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo guardar la configuración externa: " & ex.Message
            Return False
        End Try
    End Function

    Public Shared Function BuildConfiguration(ByVal server As String,
                                              ByVal databaseName As String,
                                              ByVal useIntegratedSecurity As Boolean,
                                              ByVal userName As String,
                                              ByVal password As String) As DeploymentConfiguration
        Dim config As New DeploymentConfiguration()
        config.Server = If(server, String.Empty).Trim()
        config.Database = If(databaseName, String.Empty).Trim()
        config.AuthenticationMode = If(useIntegratedSecurity, "Windows", "Sql")
        config.UserName = If(userName, String.Empty).Trim()
        config.SetPlainTextPassword(password)
        Return config
    End Function

    Public Shared Function GetConnectionString() As String
        Dim config As DeploymentConfiguration = Nothing
        Dim errorMessage As String = String.Empty

        If TryLoad(config, errorMessage) AndAlso config IsNot Nothing Then
            Return config.BuildConnectionString()
        End If

        Return String.Empty
    End Function

    Public Shared Function ValidateConfiguration(ByVal config As DeploymentConfiguration, ByRef errorMessage As String) As Boolean
        errorMessage = String.Empty

        If config Is Nothing Then
            errorMessage = "Debe indicar una configuración."
            Return False
        End If

        If String.IsNullOrWhiteSpace(config.Server) Then
            errorMessage = "Debe indicar el servidor SQL."
            Return False
        End If

        If String.IsNullOrWhiteSpace(config.Database) Then
            errorMessage = "Debe indicar la base de datos."
            Return False
        End If

        If Not config.UseIntegratedSecurity() AndAlso String.IsNullOrWhiteSpace(config.UserName) Then
            errorMessage = "Debe indicar el usuario SQL."
            Return False
        End If

        Return True
    End Function

    Public Shared Function TestConnection(ByVal config As DeploymentConfiguration, ByRef errorMessage As String) As Boolean
        If Not ValidateConfiguration(config, errorMessage) Then
            Return False
        End If

        Try
            Using cn As New SqlConnection(config.BuildConnectionString()), _
                  cmd As New SqlCommand("SELECT DB_NAME() AS DbName, CASE WHEN OBJECT_ID('dbo.Parametro', 'U') IS NULL THEN 0 ELSE 1 END AS ParametroOk;", cn)
                cn.Open()
                Using reader As SqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        Dim dbName As String = Convert.ToString(reader("DbName")).Trim()
                        Dim parametroOk As Boolean = Convert.ToInt32(reader("ParametroOk")) = 1
                        errorMessage = "Conexión exitosa a " & dbName & If(parametroOk, " con tabla Parametro detectada.", " sin tabla Parametro; la aplicación intentará inicializar lo necesario.")
                    End If
                End Using
            End Using
            Return True
        Catch ex As Exception
            errorMessage = "No se pudo validar la conexión SQL: " & ex.Message
            Return False
        End Try
    End Function

    Friend Shared Function GetEntropyBytes() As Byte()
        Return Encoding.UTF8.GetBytes(EntropyValue)
    End Function
End Class
