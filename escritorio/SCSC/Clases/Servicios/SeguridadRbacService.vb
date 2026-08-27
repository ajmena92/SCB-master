Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text
Imports System.Linq

Public Class SeguridadRbacService
    Public Structure LoginFailureStatus
        Public CurrentAttempts As Integer
        Public RemainingAttempts As Integer
        Public IsBlocked As Boolean
        Public BlockedUntilUtc As Nullable(Of DateTime)
        Public UserMessage As String
    End Structure

    Private Structure HashConSalt
        Public Hash As String
        Public Salt As String
    End Structure

    Public NotInheritable Class UserAccessContext
        Public Sub New()
            Roles = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Permisos = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        End Sub

        Public Property IdUsuario As Integer?
        Public Property NombreUsuario As String
        Public Property NombreCompleto As String
        Public Property EsSuperUsuario As Boolean
        Public ReadOnly Property Roles As HashSet(Of String)
        Public ReadOnly Property Permisos As HashSet(Of String)

        Public Function TieneRol(ParamArray nombresRol() As String) As Boolean
            If nombresRol Is Nothing Then
                Return False
            End If

            For Each nombreRol As String In nombresRol
                If String.IsNullOrWhiteSpace(nombreRol) Then
                    Continue For
                End If
                If Roles.Contains(nombreRol.Trim()) Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function TienePermiso(ParamArray clavesPermiso() As String) As Boolean
            If EsSuperUsuario Then
                Return True
            End If

            If clavesPermiso Is Nothing Then
                Return False
            End If

            For Each clavePermiso As String In clavesPermiso
                If String.IsNullOrWhiteSpace(clavePermiso) Then
                    Continue For
                End If
                If Permisos.Contains(clavePermiso.Trim()) Then
                    Return True
                End If
            Next

            Return False
        End Function
    End Class

    Private Function CrearConexion() As SqlConnection
        Return New SqlConnection(GetAppConfig("Conexion"))
    End Function

    Private Function ObtenerMaxIntentosFallidos() As Integer
        Dim value As Integer = GetAppSettingInteger("SeguridadMaxIntentosFallidos", 8)
        Return Math.Max(3, Math.Min(20, value))
    End Function

    Private Function ObtenerMinutosBloqueo() As Integer
        Dim value As Integer = GetAppSettingInteger("SeguridadMinutosBloqueo", 5)
        Return Math.Max(1, Math.Min(120, value))
    End Function

    Private Function DebeMostrarIntentosRestantes() As Boolean
        Return GetAppSettingBoolean("SeguridadMostrarIntentosRestantes", True)
    End Function

    Public Function ObtenerUsuarioPorNombre(ByVal nombreUsuario As String) As DataRow
        Const sql As String = "SELECT TOP 1 IdUsuario, NombreUsuario, NombreCompleto, HashContrasena, SaltContrasena, EsActivo, IntentosFallidos, BloqueadoHasta, FechaUltimoIngreso FROM Seguridad.Usuario WHERE NombreUsuario = @NombreUsuario"

        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand(sql, cn), da As New SqlDataAdapter(cmd)
            cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 100).Value = nombreUsuario
            Dim dt As New DataTable("Usuario")
            cn.Open()
            da.Fill(dt)
            If dt.Rows.Count = 0 Then
                Return Nothing
            End If
            Return dt.Rows(0)
        End Using
    End Function

    Public Function ValidarContrasena(ByVal contrasenaIngresada As String, ByVal hashGuardado As String, ByVal saltGuardada As String) As Boolean
        If String.IsNullOrWhiteSpace(hashGuardado) Then
            Return False
        End If

        If hashGuardado.StartsWith("LEGACY_SHA2_512:", StringComparison.OrdinalIgnoreCase) Then
            Dim hashEsperado As String = hashGuardado.Substring("LEGACY_SHA2_512:".Length)
            Dim hashActual As String = CalcularSha512Hex(contrasenaIngresada & ":" & saltGuardada)
            Return String.Equals(hashEsperado, hashActual, StringComparison.OrdinalIgnoreCase)
        End If

        If hashGuardado.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase) Then
            Return VerificarPbkdf2(hashGuardado, contrasenaIngresada)
        End If

        Return False
    End Function

    Private Function HashPbkdf2(ByVal contrasena As String) As HashConSalt
        Dim saltBytes(15) As Byte
        Using rng As RandomNumberGenerator = RandomNumberGenerator.Create()
            rng.GetBytes(saltBytes)
        End Using

        Dim iteraciones As Integer = 120000
        Dim hashBytes() As Byte
        Using pbkdf2 As New Rfc2898DeriveBytes(contrasena, saltBytes, iteraciones)
            hashBytes = pbkdf2.GetBytes(32)
        End Using

        Dim saltBase64 As String = Convert.ToBase64String(saltBytes)
        Dim hashBase64 As String = Convert.ToBase64String(hashBytes)
        Dim hashFinal As String = "PBKDF2$" & iteraciones.ToString() & "$" & saltBase64 & "$" & hashBase64

        Dim resultado As New HashConSalt
        resultado.Hash = hashFinal
        resultado.Salt = saltBase64
        Return resultado
    End Function

    Public Sub RegistrarLoginCorrecto(ByVal idUsuario As Integer, ByVal direccionIP As String)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand()
            cmd.Connection = cn
            cmd.CommandText = "UPDATE Seguridad.Usuario SET IntentosFallidos = 0, BloqueadoHasta = NULL, FechaUltimoIngreso = SYSUTCDATETIME() WHERE IdUsuario = @IdUsuario;" & _
                              "INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP) VALUES (@IdUsuario, N'LoginCorrecto', N'Autenticación exitosa.', @DireccionIP);"
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@DireccionIP", SqlDbType.NVarChar, 45).Value = If(String.IsNullOrWhiteSpace(direccionIP), "LOCAL", direccionIP)
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function RegistrarLoginFallido(ByVal nombreUsuario As String, ByVal idUsuario As Integer?, ByVal detalle As String, ByVal direccionIP As String) As LoginFailureStatus
        Dim status As New LoginFailureStatus
        Dim maxIntentos As Integer = ObtenerMaxIntentosFallidos()
        Dim minutosBloqueo As Integer = ObtenerMinutosBloqueo()
        Dim mostrarRestantes As Boolean = DebeMostrarIntentosRestantes()

        Using cn As SqlConnection = CrearConexion()
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Dim auditDetail As String = detalle

                If idUsuario.HasValue Then
                    Dim cmdEstado As New SqlCommand("SELECT IntentosFallidos, BloqueadoHasta FROM Seguridad.Usuario WHERE IdUsuario = @IdUsuario;", cn, tx)
                    cmdEstado.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value

                    Dim intentosActuales As Integer = 0
                    Dim bloqueadoHastaUtc As Nullable(Of DateTime) = Nothing

                    Using reader As SqlDataReader = cmdEstado.ExecuteReader()
                        If reader.Read() Then
                            intentosActuales = If(reader("IntentosFallidos") Is DBNull.Value, 0, Convert.ToInt32(reader("IntentosFallidos")))
                            If reader("BloqueadoHasta") IsNot DBNull.Value Then
                                bloqueadoHastaUtc = CType(reader("BloqueadoHasta"), DateTime)
                            End If
                        End If
                    End Using

                    If bloqueadoHastaUtc.HasValue AndAlso bloqueadoHastaUtc.Value > DateTime.UtcNow Then
                        status.IsBlocked = True
                        status.CurrentAttempts = intentosActuales
                        status.RemainingAttempts = 0
                        status.BlockedUntilUtc = bloqueadoHastaUtc
                        auditDetail = "Intento durante bloqueo. " & detalle
                        status.UserMessage = ConstruirMensajeBloqueo(bloqueadoHastaUtc.Value)
                    Else
                        Dim cmdResetExpirado As New SqlCommand("UPDATE Seguridad.Usuario SET IntentosFallidos = 0, BloqueadoHasta = NULL WHERE IdUsuario = @IdUsuario AND BloqueadoHasta IS NOT NULL AND BloqueadoHasta <= SYSUTCDATETIME();", cn, tx)
                        cmdResetExpirado.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                        cmdResetExpirado.ExecuteNonQuery()

                        Dim cmdIntentos As New SqlCommand("UPDATE Seguridad.Usuario SET IntentosFallidos = IntentosFallidos + 1 WHERE IdUsuario = @IdUsuario;", cn, tx)
                        cmdIntentos.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                        cmdIntentos.ExecuteNonQuery()

                        Dim cmdLectura As New SqlCommand("SELECT IntentosFallidos FROM Seguridad.Usuario WHERE IdUsuario = @IdUsuario;", cn, tx)
                        cmdLectura.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                        status.CurrentAttempts = Convert.ToInt32(cmdLectura.ExecuteScalar())

                        If status.CurrentAttempts >= maxIntentos Then
                            Dim cmdBloqueo As New SqlCommand("UPDATE Seguridad.Usuario SET BloqueadoHasta = DATEADD(MINUTE, @MinutosBloqueo, SYSUTCDATETIME()) WHERE IdUsuario = @IdUsuario;", cn, tx)
                            cmdBloqueo.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                            cmdBloqueo.Parameters.Add("@MinutosBloqueo", SqlDbType.Int).Value = minutosBloqueo
                            cmdBloqueo.ExecuteNonQuery()

                            status.IsBlocked = True
                            status.RemainingAttempts = 0
                            status.BlockedUntilUtc = DateTime.UtcNow.AddMinutes(minutosBloqueo)
                            auditDetail = String.Format("Bloqueo aplicado tras {0} intentos. {1}", status.CurrentAttempts, detalle)
                            status.UserMessage = "Cuenta bloqueada temporalmente. " & ConstruirMensajeBloqueo(status.BlockedUntilUtc.Value)
                        Else
                            status.IsBlocked = False
                            status.RemainingAttempts = Math.Max(0, maxIntentos - status.CurrentAttempts)
                            auditDetail = String.Format("Intento fallido {0}/{1}. {2}", status.CurrentAttempts, maxIntentos, detalle)
                            If mostrarRestantes Then
                                status.UserMessage = String.Format("Usuario o contraseña inválida. Le quedan {0} intento(s) antes de un bloqueo temporal de {1} minuto(s).",
                                                                   status.RemainingAttempts,
                                                                   minutosBloqueo)
                            Else
                                status.UserMessage = "Usuario o contraseña inválida."
                            End If
                        End If
                    End If
                End If

                Dim cmdAudit As New SqlCommand("INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP) VALUES (@IdUsuario, N'LoginFallido', @Detalle, @DireccionIP);", cn, tx)
                If idUsuario.HasValue Then
                    cmdAudit.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                Else
                    cmdAudit.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = DBNull.Value
                End If

                cmdAudit.Parameters.Add("@Detalle", SqlDbType.NVarChar, 1000).Value = String.Format("Usuario='{0}'. {1}", nombreUsuario, auditDetail)
                cmdAudit.Parameters.Add("@DireccionIP", SqlDbType.NVarChar, 45).Value = If(String.IsNullOrWhiteSpace(direccionIP), "LOCAL", direccionIP)
                cmdAudit.ExecuteNonQuery()

                tx.Commit()
            End Using
        End Using
        Return status
    End Function

    Public Function EstaBloqueado(ByVal usuario As DataRow, ByRef mensaje As String) As Boolean
        If usuario Is Nothing Then
            mensaje = ""
            Return False
        End If

        Dim bloqueadoHastaObj As Object = usuario("BloqueadoHasta")
        If bloqueadoHastaObj Is DBNull.Value Then
            mensaje = ""
            Return False
        End If

        Dim bloqueadoHasta As DateTime = CDate(bloqueadoHastaObj)
        Dim ahora As DateTime = DateTime.UtcNow
        If bloqueadoHasta > ahora Then
            mensaje = ConstruirMensajeBloqueo(bloqueadoHasta)
            Return True
        End If

        mensaje = ""
        Return False
    End Function

    Public Sub DesbloquearUsuario(ByVal idUsuario As Integer, ByVal detalle As String, ByVal actor As String)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand()
            cmd.Connection = cn
            cmd.CommandText = "UPDATE Seguridad.Usuario SET IntentosFallidos = 0, BloqueadoHasta = NULL WHERE IdUsuario = @IdUsuario;" & _
                              "INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP) VALUES (@IdUsuario, N'DesbloqueoManual', @Detalle, @Actor);"
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@Detalle", SqlDbType.NVarChar, 1000).Value = If(String.IsNullOrWhiteSpace(detalle), "Desbloqueo manual.", detalle.Trim())
            cmd.Parameters.Add("@Actor", SqlDbType.NVarChar, 45).Value = If(String.IsNullOrWhiteSpace(actor), "LOCAL", actor.Trim())
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub EnsurePermissionCatalog()
        Using cn As SqlConnection = CrearConexion()
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Using cmdRol As New SqlCommand("IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol = @NombreRol) INSERT INTO Seguridad.Rol (NombreRol, Descripcion, EsActivo, FechaCreacion) VALUES (@NombreRol, @Descripcion, 1, SYSUTCDATETIME());", cn, tx)
                        cmdRol.Parameters.Add("@NombreRol", SqlDbType.NVarChar, 100).Value = SeguridadPermisosSistema.RolAdministrador
                        cmdRol.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = "Acceso total al sistema"
                        cmdRol.ExecuteNonQuery()
                    End Using

                    For Each definition As SeguridadPermisosSistema.PermissionDefinition In SeguridadPermisosSistema.GetPermissionCatalog()
                        Using cmdPermiso As New SqlCommand("IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = @ClavePermiso) INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion, FechaCreacion) VALUES (@ClavePermiso, @Descripcion, SYSUTCDATETIME());", cn, tx)
                            cmdPermiso.Parameters.Add("@ClavePermiso", SqlDbType.NVarChar, 150).Value = definition.Key
                            cmdPermiso.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = definition.Description
                            cmdPermiso.ExecuteNonQuery()
                        End Using
                    Next

                    Using cmdAdmin As New SqlCommand(
                        "INSERT INTO Seguridad.RolPermiso (IdRol, IdPermiso) " &
                        "SELECT r.IdRol, p.IdPermiso " &
                        "FROM Seguridad.Rol r " &
                        "INNER JOIN Seguridad.Permiso p ON 1 = 1 " &
                        "WHERE r.NombreRol = @NombreRol " &
                        "  AND NOT EXISTS (SELECT 1 FROM Seguridad.RolPermiso rp WHERE rp.IdRol = r.IdRol AND rp.IdPermiso = p.IdPermiso);", cn, tx)
                        cmdAdmin.Parameters.Add("@NombreRol", SqlDbType.NVarChar, 100).Value = SeguridadPermisosSistema.RolAdministrador
                        cmdAdmin.ExecuteNonQuery()
                    End Using

                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Function GetUserAccessContext(ByVal nombreUsuario As String) As UserAccessContext
        Dim context As New UserAccessContext()
        context.NombreUsuario = If(nombreUsuario, String.Empty).Trim()
        context.NombreCompleto = context.NombreUsuario

        If context.NombreUsuario.Length = 0 Then
            Return context
        End If

        If IsSupportAdminUser(context.NombreUsuario) Then
            GrantFullAccess(context)
            Return context
        End If

        Const sql As String =
            "SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, r.NombreRol, p.ClavePermiso " &
            "FROM Seguridad.Usuario u " &
            "LEFT JOIN Seguridad.UsuarioRol ur ON ur.IdUsuario = u.IdUsuario " &
            "LEFT JOIN Seguridad.Rol r ON r.IdRol = ur.IdRol AND ISNULL(r.EsActivo, 1) = 1 " &
            "LEFT JOIN Seguridad.RolPermiso rp ON rp.IdRol = r.IdRol " &
            "LEFT JOIN Seguridad.Permiso p ON p.IdPermiso = rp.IdPermiso " &
            "WHERE u.NombreUsuario = @NombreUsuario AND u.EsActivo = 1;"

        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 100).Value = context.NombreUsuario
            cn.Open()

            Using reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    If Not context.IdUsuario.HasValue AndAlso reader("IdUsuario") IsNot DBNull.Value Then
                        context.IdUsuario = Convert.ToInt32(reader("IdUsuario"))
                    End If

                    If reader("NombreCompleto") IsNot DBNull.Value Then
                        Dim nombreCompleto As String = Convert.ToString(reader("NombreCompleto")).Trim()
                        If nombreCompleto.Length > 0 Then
                            context.NombreCompleto = nombreCompleto
                        End If
                    End If

                    If reader("NombreRol") IsNot DBNull.Value Then
                        Dim roleName As String = Convert.ToString(reader("NombreRol")).Trim()
                        If roleName.Length > 0 Then
                            context.Roles.Add(roleName)
                        End If
                    End If

                    If reader("ClavePermiso") IsNot DBNull.Value Then
                        Dim permissionKey As String = Convert.ToString(reader("ClavePermiso")).Trim()
                        If permissionKey.Length > 0 Then
                            context.Permisos.Add(permissionKey)
                        End If
                    End If
                End While
            End Using
        End Using

        If context.TieneRol(SeguridadPermisosSistema.RolAdministrador) Then
            GrantFullAccess(context)
        End If

        Return context
    End Function

    Private Function ConstruirMensajeBloqueo(ByVal bloqueadoHastaUtc As DateTime) As String
        Dim minutosRestantes As Integer = Math.Max(1, CInt(Math.Ceiling((bloqueadoHastaUtc - DateTime.UtcNow).TotalMinutes)))
        Return String.Format("Usuario bloqueado temporalmente. Intente nuevamente en {0} minuto(s) o después de las {1}.",
                             minutosRestantes,
                             bloqueadoHastaUtc.ToLocalTime().ToString("HH:mm"))
    End Function

    Public Function ListarUsuarios() As DataTable
        Const sql As String = "SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, u.EsActivo, u.IntentosFallidos, u.BloqueadoHasta, u.FechaCreacion, u.FechaUltimoIngreso, STUFF((SELECT N', ' + r2.NombreRol FROM Seguridad.UsuarioRol ur2 INNER JOIN Seguridad.Rol r2 ON r2.IdRol = ur2.IdRol WHERE ur2.IdUsuario = u.IdUsuario ORDER BY r2.NombreRol FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Roles FROM Seguridad.Usuario u ORDER BY u.NombreUsuario;"
        Return EjecutarConsulta(sql)
    End Function

    Public Function ObtenerUsuarioPorId(ByVal idUsuario As Integer) As DataRow
        Const sql As String = "SELECT TOP 1 IdUsuario, NombreUsuario, NombreCompleto, EsActivo, IntentosFallidos, BloqueadoHasta, FechaCreacion, FechaUltimoIngreso FROM Seguridad.Usuario WHERE IdUsuario = @IdUsuario"

        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand(sql, cn), da As New SqlDataAdapter(cmd)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            Dim dt As New DataTable("Usuario")
            cn.Open()
            da.Fill(dt)
            If dt.Rows.Count = 0 Then
                Return Nothing
            End If
            Return dt.Rows(0)
        End Using
    End Function

    Public Function CrearUsuario(ByVal nombreUsuario As String, ByVal nombreCompleto As String, ByVal contrasena As String, ByVal esActivo As Boolean) As Integer
        Dim hashData As HashConSalt = HashPbkdf2(contrasena)

        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("INSERT INTO Seguridad.Usuario (NombreUsuario, NombreCompleto, HashContrasena, SaltContrasena, EsActivo, IntentosFallidos, FechaCreacion) VALUES (@NombreUsuario, @NombreCompleto, @HashContrasena, @SaltContrasena, @EsActivo, 0, SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn)
            cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 100).Value = nombreUsuario.Trim()
            cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = nombreCompleto.Trim()
            cmd.Parameters.Add("@HashContrasena", SqlDbType.NVarChar, 512).Value = hashData.Hash
            cmd.Parameters.Add("@SaltContrasena", SqlDbType.NVarChar, 255).Value = hashData.Salt
            cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
            cn.Open()
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    Public Function CrearUsuarioConRoles(ByVal nombreUsuario As String,
                                         ByVal nombreCompleto As String,
                                         ByVal contrasena As String,
                                         ByVal esActivo As Boolean,
                                         ByVal idRoles As IEnumerable(Of Integer)) As Integer
        If String.IsNullOrWhiteSpace(nombreUsuario) Then
            Throw New Exception("NombreUsuario es obligatorio.")
        End If
        If String.IsNullOrWhiteSpace(nombreCompleto) Then
            Throw New Exception("NombreCompleto es obligatorio.")
        End If
        If String.IsNullOrWhiteSpace(contrasena) Then
            Throw New Exception("Contraseña es obligatoria.")
        End If

        Dim rolesNormalizados As Integer() = NormalizarIds(idRoles)
        If rolesNormalizados.Length = 0 Then
            Throw New Exception("Debe asignar al menos un rol al usuario.")
        End If

        Dim hashData As HashConSalt = HashPbkdf2(contrasena)

        Using cn As SqlConnection = CrearConexion()
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Dim idUsuario As Integer
                    Using cmd As New SqlCommand("INSERT INTO Seguridad.Usuario (NombreUsuario, NombreCompleto, HashContrasena, SaltContrasena, EsActivo, IntentosFallidos, FechaCreacion) VALUES (@NombreUsuario, @NombreCompleto, @HashContrasena, @SaltContrasena, @EsActivo, 0, SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn, tx)
                        cmd.Parameters.Add("@NombreUsuario", SqlDbType.NVarChar, 100).Value = nombreUsuario.Trim()
                        cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = nombreCompleto.Trim()
                        cmd.Parameters.Add("@HashContrasena", SqlDbType.NVarChar, 512).Value = hashData.Hash
                        cmd.Parameters.Add("@SaltContrasena", SqlDbType.NVarChar, 255).Value = hashData.Salt
                        cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
                        idUsuario = CInt(cmd.ExecuteScalar())
                    End Using

                    ReemplazarRolesUsuario(cn, tx, idUsuario, rolesNormalizados)
                    tx.Commit()
                    Return idUsuario
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Function

    Public Sub ActualizarUsuario(ByVal idUsuario As Integer, ByVal nombreCompleto As String, ByVal esActivo As Boolean)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("UPDATE Seguridad.Usuario SET NombreCompleto = @NombreCompleto, EsActivo = @EsActivo WHERE IdUsuario = @IdUsuario;", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = nombreCompleto.Trim()
            cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub ActualizarUsuarioConRoles(ByVal idUsuario As Integer,
                                         ByVal nombreCompleto As String,
                                         ByVal esActivo As Boolean,
                                         ByVal idRoles As IEnumerable(Of Integer))
        If idUsuario <= 0 Then
            Throw New Exception("Debe seleccionar un usuario válido.")
        End If
        If String.IsNullOrWhiteSpace(nombreCompleto) Then
            Throw New Exception("NombreCompleto es obligatorio.")
        End If

        Dim rolesNormalizados As Integer() = NormalizarIds(idRoles)
        If rolesNormalizados.Length = 0 Then
            Throw New Exception("Debe asignar al menos un rol al usuario.")
        End If

        Using cn As SqlConnection = CrearConexion()
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                Try
                    Using cmd As New SqlCommand("UPDATE Seguridad.Usuario SET NombreCompleto = @NombreCompleto, EsActivo = @EsActivo WHERE IdUsuario = @IdUsuario;", cn, tx)
                        cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
                        cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = nombreCompleto.Trim()
                        cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
                        cmd.ExecuteNonQuery()
                    End Using

                    ReemplazarRolesUsuario(cn, tx, idUsuario, rolesNormalizados)
                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Sub CambiarContrasenaUsuario(ByVal idUsuario As Integer, ByVal nuevaContrasena As String)
        Dim hashData As HashConSalt = HashPbkdf2(nuevaContrasena)

        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("UPDATE Seguridad.Usuario SET HashContrasena = @HashContrasena, SaltContrasena = @SaltContrasena, IntentosFallidos = 0, BloqueadoHasta = NULL WHERE IdUsuario = @IdUsuario; INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP) VALUES (@IdUsuario, N'CambioContrasena', N'Contraseña actualizada por mantenimiento.', N'LOCAL');", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@HashContrasena", SqlDbType.NVarChar, 512).Value = hashData.Hash
            cmd.Parameters.Add("@SaltContrasena", SqlDbType.NVarChar, 255).Value = hashData.Salt
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub EliminarUsuario(ByVal idUsuario As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("DELETE FROM Seguridad.Usuario WHERE IdUsuario = @IdUsuario;", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ListarRoles() As DataTable
        Const sql As String = "SELECT IdRol, NombreRol, Descripcion, EsActivo, FechaCreacion FROM Seguridad.Rol ORDER BY NombreRol;"
        Return EjecutarConsulta(sql)
    End Function

    Public Function ObtenerRolPorId(ByVal idRol As Integer) As DataRow
        Const sql As String = "SELECT TOP 1 IdRol, NombreRol, Descripcion, EsActivo, FechaCreacion FROM Seguridad.Rol WHERE IdRol = @IdRol;"
        Dim dt As DataTable = EjecutarConsultaConParametro(sql, "@IdRol", idRol)
        If dt.Rows.Count = 0 Then
            Return Nothing
        End If
        Return dt.Rows(0)
    End Function

    Public Function CrearRol(ByVal nombreRol As String, ByVal descripcion As String, ByVal esActivo As Boolean) As Integer
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("INSERT INTO Seguridad.Rol (NombreRol, Descripcion, EsActivo, FechaCreacion) VALUES (@NombreRol, @Descripcion, @EsActivo, SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn)
            cmd.Parameters.Add("@NombreRol", SqlDbType.NVarChar, 100).Value = nombreRol.Trim()
            cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(descripcion), CType(DBNull.Value, Object), descripcion.Trim())
            cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
            cn.Open()
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    Public Sub ActualizarRol(ByVal idRol As Integer, ByVal descripcion As String, ByVal esActivo As Boolean)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("UPDATE Seguridad.Rol SET Descripcion = @Descripcion, EsActivo = @EsActivo WHERE IdRol = @IdRol;", cn)
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(descripcion), CType(DBNull.Value, Object), descripcion.Trim())
            cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub EliminarRol(ByVal idRol As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("DELETE FROM Seguridad.Rol WHERE IdRol = @IdRol;", cn)
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ListarPermisos() As DataTable
        Const sql As String = "SELECT IdPermiso, ClavePermiso, Descripcion, FechaCreacion FROM Seguridad.Permiso ORDER BY ClavePermiso;"
        Return EjecutarConsulta(sql)
    End Function

    Public Function CrearPermiso(ByVal clavePermiso As String, ByVal descripcion As String) As Integer
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion, FechaCreacion) VALUES (@ClavePermiso, @Descripcion, SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS INT);", cn)
            cmd.Parameters.Add("@ClavePermiso", SqlDbType.NVarChar, 150).Value = clavePermiso.Trim()
            cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(descripcion), CType(DBNull.Value, Object), descripcion.Trim())
            cn.Open()
            Return CInt(cmd.ExecuteScalar())
        End Using
    End Function

    Public Sub ActualizarPermiso(ByVal idPermiso As Integer, ByVal descripcion As String)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("UPDATE Seguridad.Permiso SET Descripcion = @Descripcion WHERE IdPermiso = @IdPermiso;", cn)
            cmd.Parameters.Add("@IdPermiso", SqlDbType.Int).Value = idPermiso
            cmd.Parameters.Add("@Descripcion", SqlDbType.NVarChar, 500).Value = If(String.IsNullOrWhiteSpace(descripcion), CType(DBNull.Value, Object), descripcion.Trim())
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub EliminarPermiso(ByVal idPermiso As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("DELETE FROM Seguridad.Permiso WHERE IdPermiso = @IdPermiso;", cn)
            cmd.Parameters.Add("@IdPermiso", SqlDbType.Int).Value = idPermiso
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub AsignarRolAUsuario(ByVal idUsuario As Integer, ByVal idRol As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("IF NOT EXISTS (SELECT 1 FROM Seguridad.UsuarioRol WHERE IdUsuario = @IdUsuario AND IdRol = @IdRol) INSERT INTO Seguridad.UsuarioRol (IdUsuario, IdRol, FechaAsignacion) VALUES (@IdUsuario, @IdRol, SYSUTCDATETIME());", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub RevocarRolAUsuario(ByVal idUsuario As Integer, ByVal idRol As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("DELETE FROM Seguridad.UsuarioRol WHERE IdUsuario = @IdUsuario AND IdRol = @IdRol;", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub AsignarPermisoARol(ByVal idRol As Integer, ByVal idPermiso As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("IF NOT EXISTS (SELECT 1 FROM Seguridad.RolPermiso WHERE IdRol = @IdRol AND IdPermiso = @IdPermiso) INSERT INTO Seguridad.RolPermiso (IdRol, IdPermiso) VALUES (@IdRol, @IdPermiso);", cn)
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cmd.Parameters.Add("@IdPermiso", SqlDbType.Int).Value = idPermiso
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Sub RevocarPermisoARol(ByVal idRol As Integer, ByVal idPermiso As Integer)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("DELETE FROM Seguridad.RolPermiso WHERE IdRol = @IdRol AND IdPermiso = @IdPermiso;", cn)
            cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
            cmd.Parameters.Add("@IdPermiso", SqlDbType.Int).Value = idPermiso
            cn.Open()
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Function ListarRolesDeUsuario(ByVal idUsuario As Integer) As DataTable
        Const sql As String = "SELECT r.IdRol, r.NombreRol FROM Seguridad.Rol r INNER JOIN Seguridad.UsuarioRol ur ON ur.IdRol = r.IdRol WHERE ur.IdUsuario = @IdUsuario ORDER BY r.NombreRol;"
        Return EjecutarConsultaConParametro(sql, "@IdUsuario", idUsuario)
    End Function

    Public Function ListarPermisosDeRol(ByVal idRol As Integer) As DataTable
        Const sql As String = "SELECT p.IdPermiso, p.ClavePermiso FROM Seguridad.Permiso p INNER JOIN Seguridad.RolPermiso rp ON rp.IdPermiso = p.IdPermiso WHERE rp.IdRol = @IdRol ORDER BY p.ClavePermiso;"
        Return EjecutarConsultaConParametro(sql, "@IdRol", idRol)
    End Function

    Private Function EjecutarConsulta(ByVal sql As String) As DataTable
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand(sql, cn), da As New SqlDataAdapter(cmd)
            Dim dt As New DataTable()
            cn.Open()
            da.Fill(dt)
            Return dt
        End Using
    End Function

    Private Function EjecutarConsultaConParametro(ByVal sql As String, ByVal nombreParametro As String, ByVal valor As Integer) As DataTable
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand(sql, cn), da As New SqlDataAdapter(cmd)
            cmd.Parameters.Add(nombreParametro, SqlDbType.Int).Value = valor
            Dim dt As New DataTable()
            cn.Open()
            da.Fill(dt)
            Return dt
        End Using
    End Function

    Private Function NormalizarIds(ByVal ids As IEnumerable(Of Integer)) As Integer()
        If ids Is Nothing Then
            Return New Integer() {}
        End If

        Return ids.Where(Function(id) id > 0).Distinct().ToArray()
    End Function

    Private Sub ReemplazarRolesUsuario(ByVal cn As SqlConnection, ByVal tx As SqlTransaction, ByVal idUsuario As Integer, ByVal idRoles As IEnumerable(Of Integer))
        Dim rolesNormalizados As Integer() = NormalizarIds(idRoles)
        If rolesNormalizados.Length = 0 Then
            Throw New Exception("Debe asignar al menos un rol al usuario.")
        End If

        Using cmdDelete As New SqlCommand("DELETE FROM Seguridad.UsuarioRol WHERE IdUsuario = @IdUsuario;", cn, tx)
            cmdDelete.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmdDelete.ExecuteNonQuery()
        End Using

        For Each idRol As Integer In rolesNormalizados
            Using cmdInsert As New SqlCommand("INSERT INTO Seguridad.UsuarioRol (IdUsuario, IdRol, FechaAsignacion) VALUES (@IdUsuario, @IdRol, SYSUTCDATETIME());", cn, tx)
                cmdInsert.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
                cmdInsert.Parameters.Add("@IdRol", SqlDbType.Int).Value = idRol
                cmdInsert.ExecuteNonQuery()
            End Using
        Next
    End Sub

    Private Function CalcularSha512Hex(ByVal texto As String) As String
        Using sha As SHA512 = SHA512.Create()
            Dim bytes() As Byte = Encoding.UTF8.GetBytes(texto)
            Dim hash() As Byte = sha.ComputeHash(bytes)
            Return BitConverter.ToString(hash).Replace("-", "")
        End Using
    End Function

    Private Function VerificarPbkdf2(ByVal hashSerializado As String, ByVal contrasenaIngresada As String) As Boolean
        Try
            Dim partes() As String = hashSerializado.Split("$"c)
            If partes.Length <> 4 Then
                Return False
            End If

            Dim iteraciones As Integer = Integer.Parse(partes(1))
            Dim saltBytes() As Byte = Convert.FromBase64String(partes(2))
            Dim hashEsperado() As Byte = Convert.FromBase64String(partes(3))

            Dim hashActual() As Byte
            Using pbkdf2 As New Rfc2898DeriveBytes(contrasenaIngresada, saltBytes, iteraciones)
                hashActual = pbkdf2.GetBytes(hashEsperado.Length)
            End Using

            Return ComparacionConstante(hashEsperado, hashActual)
        Catch
            Return False
        End Try
    End Function

    Private Function ComparacionConstante(ByVal a() As Byte, ByVal b() As Byte) As Boolean
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then
            Return False
        End If

        Dim resultado As Integer = 0
        For i As Integer = 0 To a.Length - 1
            resultado = resultado Or (a(i) Xor b(i))
        Next
        Return resultado = 0
    End Function

    Private Sub GrantFullAccess(ByVal context As UserAccessContext)
        If context Is Nothing Then
            Exit Sub
        End If

        context.EsSuperUsuario = True
        context.Roles.Add(SeguridadPermisosSistema.RolAdministrador)

        For Each definition As SeguridadPermisosSistema.PermissionDefinition In SeguridadPermisosSistema.GetPermissionCatalog()
            context.Permisos.Add(definition.Key)
        Next
    End Sub

    Private Function IsSupportAdminUser(ByVal nombreUsuario As String) As Boolean
        Dim configuredAdmin As String = GetAppSettingValue("AdminUsuario", String.Empty)
        If String.IsNullOrWhiteSpace(configuredAdmin) Then
            Return False
        End If

        Return String.Equals(nombreUsuario.Trim(), configuredAdmin.Trim(), StringComparison.OrdinalIgnoreCase)
    End Function
End Class
