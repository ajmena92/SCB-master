Option Strict On
Option Explicit On

Imports System.Collections.Generic

Public Module SeguridadPermisosSistema
    Public Const UsuarioParametrosAdmin As String = "admin"
    Public Const UsuarioParametrosAmenaa As String = "amenaa"
    Public Const RolAdministrador As String = "Administrador"

    Public NotInheritable Class PermissionDefinition
        Public Sub New(ByVal key As String, ByVal description As String)
            Me.Key = key
            Me.Description = description
        End Sub

        Public ReadOnly Property Key As String
        Public ReadOnly Property Description As String
    End Class

    Private ReadOnly _permissionCatalog As List(Of PermissionDefinition) = BuildPermissionCatalog()
    Private ReadOnly _modulePermissionMap As Dictionary(Of String, String()) = BuildModulePermissionMap()

    Public Function GetPermissionCatalog() As IList(Of PermissionDefinition)
        Return _permissionCatalog.AsReadOnly()
    End Function

    Public Function GetModulePermissionKeys(ByVal moduleKey As String) As String()
        If String.IsNullOrWhiteSpace(moduleKey) Then
            Return New String() {}
        End If

        Dim normalizedKey As String = moduleKey.Trim().ToLowerInvariant()
        If _modulePermissionMap.ContainsKey(normalizedKey) Then
            Return _modulePermissionMap(normalizedKey)
        End If

        Return New String() {}
    End Function

    Public Function NormalizeUserName(ByVal userName As String) As String
        If String.IsNullOrWhiteSpace(userName) Then
            Return String.Empty
        End If

        Return userName.Trim().ToLowerInvariant()
    End Function

    Public Function IsUsuarioAutorizadoParaParametros(ByVal userName As String) As Boolean
        Dim normalizedUser As String = NormalizeUserName(userName)
        Return normalizedUser = UsuarioParametrosAdmin OrElse normalizedUser = UsuarioParametrosAmenaa
    End Function

    Public Function IsModuleAccessibleWithoutLogin(ByVal moduleKey As String) As Boolean
        Dim normalizedKey As String = If(moduleKey, String.Empty).Trim().ToLowerInvariant()
        Select Case normalizedKey
            Case "comedor", "transporte"
                Return True
            Case Else
                Return False
        End Select
    End Function

    Private Function BuildPermissionCatalog() As List(Of PermissionDefinition)
        Return New List(Of PermissionDefinition) From {
            New PermissionDefinition("Usuarios.Ver", "Ver usuarios del sistema"),
            New PermissionDefinition("Usuarios.Crear", "Crear usuarios del sistema"),
            New PermissionDefinition("Usuarios.Editar", "Editar usuarios del sistema"),
            New PermissionDefinition("Usuarios.Eliminar", "Eliminar usuarios del sistema"),
            New PermissionDefinition("Usuarios.CambiarClave", "Cambiar claves de usuarios del sistema"),
            New PermissionDefinition("Roles.Ver", "Ver roles de seguridad"),
            New PermissionDefinition("Roles.Crear", "Crear roles de seguridad"),
            New PermissionDefinition("Roles.Editar", "Editar roles de seguridad"),
            New PermissionDefinition("Roles.Eliminar", "Eliminar roles de seguridad"),
            New PermissionDefinition("Roles.Permisos.Gestionar", "Asignar o revocar permisos en roles"),
            New PermissionDefinition("Permisos.Ver", "Ver permisos de seguridad"),
            New PermissionDefinition("Permisos.Crear", "Crear permisos de seguridad"),
            New PermissionDefinition("Permisos.Editar", "Editar permisos de seguridad"),
            New PermissionDefinition("Permisos.Eliminar", "Eliminar permisos de seguridad"),
            New PermissionDefinition("Seguridad.Ver", "Acceder al modulo de seguridad"),
            New PermissionDefinition("Modulos.Estudiantes.Acceso", "Acceder al modulo de estudiantes"),
            New PermissionDefinition("Modulos.Rutas.Acceso", "Acceder al modulo de rutas"),
            New PermissionDefinition("Modulos.Becas.Acceso", "Acceder al modulo de becas"),
            New PermissionDefinition("Modulos.Comedor.Acceso", "Acceder al control de marcas de comedor"),
            New PermissionDefinition("Modulos.Transporte.Acceso", "Acceder al control de marcas de transporte"),
            New PermissionDefinition("Modulos.Importacion.Acceso", "Acceder a la importacion de datos PIAD"),
            New PermissionDefinition("Modulos.AgregarEstudiante.Acceso", "Acceder al alta manual de estudiantes"),
            New PermissionDefinition("Modulos.Recargas.Acceso", "Acceder al modulo de recargas"),
            New PermissionDefinition("Reportes.Ver", "Ver reportes del sistema"),
            New PermissionDefinition("Reportes.Comedor.Ver", "Ver el reporte de servicio comedor"),
            New PermissionDefinition("Reportes.Transporte.Ver", "Ver el reporte de servicio transporte"),
            New PermissionDefinition("Reportes.Proyeccion.Ver", "Ver el reporte de proyeccion de comedor"),
            New PermissionDefinition("Reportes.Becados.Ver", "Ver el reporte de estudiantes becados"),
            New PermissionDefinition("Carnets.Imprimir", "Imprimir carnets o documentos habilitados"),
            New PermissionDefinition("Configuracion.Modificar", "Modificar configuracion del sistema")
        }
    End Function

    Private Function BuildModulePermissionMap() As Dictionary(Of String, String())
        Return New Dictionary(Of String, String())(StringComparer.OrdinalIgnoreCase) From {
            {"estudiantes", New String() {"Modulos.Estudiantes.Acceso"}},
            {"rutas", New String() {"Modulos.Rutas.Acceso"}},
            {"becas", New String() {"Modulos.Becas.Acceso"}},
            {"comedor", New String() {"Modulos.Comedor.Acceso"}},
            {"transporte", New String() {"Modulos.Transporte.Acceso"}},
            {"importacion", New String() {"Modulos.Importacion.Acceso"}},
            {"agregar_estudiante", New String() {"Modulos.AgregarEstudiante.Acceso", "Modulos.Estudiantes.Acceso"}},
            {"recargas", New String() {"Modulos.Recargas.Acceso"}},
            {"reporte_comedor", New String() {"Reportes.Comedor.Ver", "Reportes.Ver"}},
            {"reporte_transporte", New String() {"Reportes.Transporte.Ver", "Reportes.Ver"}},
            {"reporte_proyeccion", New String() {"Reportes.Proyeccion.Ver", "Reportes.Ver"}},
            {"reporte_becados", New String() {"Reportes.Becados.Ver", "Reportes.Ver"}},
            {"seguridad", New String() {
                "Seguridad.Ver",
                "Usuarios.Ver",
                "Usuarios.Crear",
                "Usuarios.Editar",
                "Usuarios.Eliminar",
                "Usuarios.CambiarClave",
                "Roles.Ver",
                "Roles.Crear",
                "Roles.Editar",
                "Roles.Eliminar",
                "Roles.Permisos.Gestionar"}},
            {"parametros", New String() {"Configuracion.Modificar"}},
            {"imprimir", New String() {"Carnets.Imprimir"}}
        }
    End Function
End Module
