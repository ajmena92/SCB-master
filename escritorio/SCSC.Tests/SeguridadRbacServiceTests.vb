Option Strict On
Option Explicit On

Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Data
Imports System.Security.Cryptography
Imports System.Text
Imports SCSC

<TestClass>
Public Class SeguridadRbacServiceTests
    <TestMethod>
    Public Sub IsUsuarioAutorizadoParaParametros_OnlyAllowsAdminAndAmenaa()
        Assert.IsTrue(SeguridadPermisosSistema.IsUsuarioAutorizadoParaParametros("admin"))
        Assert.IsTrue(SeguridadPermisosSistema.IsUsuarioAutorizadoParaParametros("AMENAA"))
        Assert.IsFalse(SeguridadPermisosSistema.IsUsuarioAutorizadoParaParametros("consulta"))
    End Sub

    <TestMethod>
    Public Sub GetModulePermissionKeys_ReturnsConfiguredKeysForSecurityModule()
        Dim keys As String() = SeguridadPermisosSistema.GetModulePermissionKeys("seguridad")

        CollectionAssert.Contains(keys, "Seguridad.Ver")
        CollectionAssert.Contains(keys, "Usuarios.Ver")
        CollectionAssert.Contains(keys, "Roles.Ver")
        CollectionAssert.Contains(keys, "Roles.Permisos.Gestionar")
        CollectionAssert.DoesNotContain(keys, "Permisos.Ver")
    End Sub

    <TestMethod>
    Public Sub UserAccessContext_TienePermisoHonorsExplicitAndSuperUserPermissions()
        Dim context As New SeguridadRbacService.UserAccessContext()
        context.Permisos.Add("Modulos.Estudiantes.Acceso")

        Assert.IsTrue(context.TienePermiso("Modulos.Estudiantes.Acceso"))
        Assert.IsFalse(context.TienePermiso("Modulos.Rutas.Acceso"))

        context.EsSuperUsuario = True

        Assert.IsTrue(context.TienePermiso("Cualquier.Cosa"))
    End Sub

    <TestMethod>
    Public Sub ValidarContrasena_AcceptsLegacySha512Format()
        Dim service As New SeguridadRbacService()
        Dim salt As String = "LEGACY-SALT"
        Dim raw As String = "ClaveSegura123:" & salt
        Dim expectedHash As String

        Using sha As SHA512 = SHA512.Create()
            expectedHash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw))).Replace("-", "")
        End Using

        Dim serialized As String = "LEGACY_SHA2_512:" & expectedHash
        Dim result As Boolean = service.ValidarContrasena("ClaveSegura123", serialized, salt)

        Assert.IsTrue(result)
    End Sub

    <TestMethod>
    Public Sub ValidarContrasena_AcceptsPbkdf2Format()
        Dim service As New SeguridadRbacService()
        Dim password As String = "ClaveSegura123"
        Dim saltBytes As Byte() = Encoding.UTF8.GetBytes("SALT-PRUEBA-1234")
        Dim hashBytes As Byte()

        Using pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 120000)
            hashBytes = pbkdf2.GetBytes(32)
        End Using

        Dim serialized As String = String.Format("PBKDF2${0}${1}${2}",
                                                 120000,
                                                 Convert.ToBase64String(saltBytes),
                                                 Convert.ToBase64String(hashBytes))

        Dim result As Boolean = service.ValidarContrasena(password, serialized, "")

        Assert.IsTrue(result)
    End Sub

    <TestMethod>
    Public Sub EstaBloqueado_ReturnsTrue_WhenBlockedUntilIsFuture()
        Dim service As New SeguridadRbacService()
        Dim table As New DataTable()
        table.Columns.Add("BloqueadoHasta", GetType(DateTime))
        Dim row As DataRow = table.NewRow()
        row("BloqueadoHasta") = DateTime.UtcNow.AddMinutes(5)
        table.Rows.Add(row)

        Dim message As String = String.Empty
        Dim result As Boolean = service.EstaBloqueado(row, message)

        Assert.IsTrue(result)
        StringAssert.Contains(message, "Usuario bloqueado temporalmente.")
    End Sub

    <TestMethod>
    Public Sub EstaBloqueado_ReturnsFalse_WhenBlockedUntilExpired()
        Dim service As New SeguridadRbacService()
        Dim table As New DataTable()
        table.Columns.Add("BloqueadoHasta", GetType(DateTime))
        Dim row As DataRow = table.NewRow()
        row("BloqueadoHasta") = DateTime.UtcNow.AddMinutes(-5)
        table.Rows.Add(row)

        Dim message As String = String.Empty
        Dim result As Boolean = service.EstaBloqueado(row, message)

        Assert.IsFalse(result)
        Assert.AreEqual(String.Empty, message)
    End Sub
End Class
