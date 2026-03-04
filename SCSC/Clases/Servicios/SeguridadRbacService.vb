Option Explicit On
Option Strict On

Imports System.Data
Imports System.Data.SqlClient
Imports System.Security.Cryptography
Imports System.Text

Public Class SeguridadRbacService
    Private Const MaxIntentosFallidos As Integer = 5
    Private Shared ReadOnly TiempoBloqueo As TimeSpan = TimeSpan.FromMinutes(15)

    Private Structure HashConSalt
        Public Hash As String
        Public Salt As String
    End Structure

    Private Function CrearConexion() As SqlConnection
        Return New SqlConnection(GetAppConfig("Conexion"))
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

    Public Sub RegistrarLoginFallido(ByVal nombreUsuario As String, ByVal idUsuario As Integer?, ByVal detalle As String, ByVal direccionIP As String)
        Using cn As SqlConnection = CrearConexion()
            cn.Open()
            Using tx As SqlTransaction = cn.BeginTransaction()
                If idUsuario.HasValue Then
                    Dim cmdIntentos As New SqlCommand("UPDATE Seguridad.Usuario SET IntentosFallidos = IntentosFallidos + 1 WHERE IdUsuario = @IdUsuario;", cn, tx)
                    cmdIntentos.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                    cmdIntentos.ExecuteNonQuery()

                    Dim cmdBloqueo As New SqlCommand("UPDATE Seguridad.Usuario SET BloqueadoHasta = DATEADD(MINUTE, @MinutosBloqueo, SYSUTCDATETIME()) WHERE IdUsuario = @IdUsuario AND IntentosFallidos >= @MaxIntentos;", cn, tx)
                    cmdBloqueo.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                    cmdBloqueo.Parameters.Add("@MaxIntentos", SqlDbType.Int).Value = MaxIntentosFallidos
                    cmdBloqueo.Parameters.Add("@MinutosBloqueo", SqlDbType.Int).Value = CInt(TiempoBloqueo.TotalMinutes)
                    cmdBloqueo.ExecuteNonQuery()
                End If

                Dim cmdAudit As New SqlCommand("INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP) VALUES (@IdUsuario, N'LoginFallido', @Detalle, @DireccionIP);", cn, tx)
                If idUsuario.HasValue Then
                    cmdAudit.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario.Value
                Else
                    cmdAudit.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = DBNull.Value
                End If

                cmdAudit.Parameters.Add("@Detalle", SqlDbType.NVarChar, 1000).Value = String.Format("Usuario='{0}'. {1}", nombreUsuario, detalle)
                cmdAudit.Parameters.Add("@DireccionIP", SqlDbType.NVarChar, 45).Value = If(String.IsNullOrWhiteSpace(direccionIP), "LOCAL", direccionIP)
                cmdAudit.ExecuteNonQuery()

                tx.Commit()
            End Using
        End Using
    End Sub

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
            mensaje = "Usuario bloqueado hasta " & bloqueadoHasta.ToLocalTime().ToString("dd/MM/yyyy HH:mm") & "."
            Return True
        End If

        mensaje = ""
        Return False
    End Function

    Public Function ListarUsuarios() As DataTable
        Const sql As String = "SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, u.EsActivo, u.IntentosFallidos, u.BloqueadoHasta, u.FechaCreacion, u.FechaUltimoIngreso, STUFF((SELECT N', ' + r2.NombreRol FROM Seguridad.UsuarioRol ur2 INNER JOIN Seguridad.Rol r2 ON r2.IdRol = ur2.IdRol WHERE ur2.IdUsuario = u.IdUsuario ORDER BY r2.NombreRol FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Roles FROM Seguridad.Usuario u ORDER BY u.NombreUsuario;"
        Return EjecutarConsulta(sql)
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

    Public Sub ActualizarUsuario(ByVal idUsuario As Integer, ByVal nombreCompleto As String, ByVal esActivo As Boolean)
        Using cn As SqlConnection = CrearConexion(), cmd As New SqlCommand("UPDATE Seguridad.Usuario SET NombreCompleto = @NombreCompleto, EsActivo = @EsActivo WHERE IdUsuario = @IdUsuario;", cn)
            cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario
            cmd.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 200).Value = nombreCompleto.Trim()
            cmd.Parameters.Add("@EsActivo", SqlDbType.Bit).Value = esActivo
            cn.Open()
            cmd.ExecuteNonQuery()
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
End Class
