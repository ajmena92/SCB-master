/*
    Archivo: 20260304_RBAC_Login_Migracion.sql
    Objetivo:
      1) Crear esquema RBAC en schema Seguridad para no afectar tablas operativas actuales.
      2) Migrar usuarios legacy desde dbo.TablaSeguridad a Seguridad.Usuario.
      3) Eliminar uso de contraseña en texto plano para login.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* ==============================
       0) Schema de seguridad
       ============================== */
    IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'Seguridad')
    BEGIN
        EXEC(N'CREATE SCHEMA Seguridad AUTHORIZATION dbo;');
    END;

    /* ==============================
       1) Tablas núcleo RBAC
       ============================== */
    IF OBJECT_ID(N'Seguridad.Usuario', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.Usuario
        (
            IdUsuario            INT IDENTITY(1,1) NOT NULL,
            NombreUsuario        NVARCHAR(100) NOT NULL,
            NombreCompleto       NVARCHAR(200) NOT NULL,
            HashContrasena       NVARCHAR(512) NOT NULL,
            SaltContrasena       NVARCHAR(255) NOT NULL,
            EsActivo             BIT NOT NULL CONSTRAINT DF_SeguridadUsuario_EsActivo DEFAULT (1),
            IntentosFallidos     INT NOT NULL CONSTRAINT DF_SeguridadUsuario_IntentosFallidos DEFAULT (0),
            BloqueadoHasta       DATETIME2(0) NULL,
            FechaCreacion        DATETIME2(0) NOT NULL CONSTRAINT DF_SeguridadUsuario_FechaCreacion DEFAULT (SYSUTCDATETIME()),
            FechaUltimoIngreso   DATETIME2(0) NULL,

            CONSTRAINT PK_SeguridadUsuario PRIMARY KEY CLUSTERED (IdUsuario),
            CONSTRAINT UQ_SeguridadUsuario_NombreUsuario UNIQUE (NombreUsuario),
            CONSTRAINT CK_SeguridadUsuario_IntentosFallidos CHECK (IntentosFallidos >= 0)
        );
    END;

    IF OBJECT_ID(N'Seguridad.Rol', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.Rol
        (
            IdRol                INT IDENTITY(1,1) NOT NULL,
            NombreRol            NVARCHAR(100) NOT NULL,
            Descripcion          NVARCHAR(500) NULL,
            EsActivo             BIT NOT NULL CONSTRAINT DF_SeguridadRol_EsActivo DEFAULT (1),
            FechaCreacion        DATETIME2(0) NOT NULL CONSTRAINT DF_SeguridadRol_FechaCreacion DEFAULT (SYSUTCDATETIME()),

            CONSTRAINT PK_SeguridadRol PRIMARY KEY CLUSTERED (IdRol),
            CONSTRAINT UQ_SeguridadRol_NombreRol UNIQUE (NombreRol)
        );
    END;

    IF OBJECT_ID(N'Seguridad.Permiso', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.Permiso
        (
            IdPermiso            INT IDENTITY(1,1) NOT NULL,
            ClavePermiso         NVARCHAR(150) NOT NULL,
            Descripcion          NVARCHAR(500) NULL,
            FechaCreacion        DATETIME2(0) NOT NULL CONSTRAINT DF_SeguridadPermiso_FechaCreacion DEFAULT (SYSUTCDATETIME()),

            CONSTRAINT PK_SeguridadPermiso PRIMARY KEY CLUSTERED (IdPermiso),
            CONSTRAINT UQ_SeguridadPermiso_ClavePermiso UNIQUE (ClavePermiso)
        );
    END;

    IF OBJECT_ID(N'Seguridad.UsuarioRol', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.UsuarioRol
        (
            IdUsuario            INT NOT NULL,
            IdRol                INT NOT NULL,
            FechaAsignacion      DATETIME2(0) NOT NULL CONSTRAINT DF_SeguridadUsuarioRol_FechaAsignacion DEFAULT (SYSUTCDATETIME()),

            CONSTRAINT PK_SeguridadUsuarioRol PRIMARY KEY CLUSTERED (IdUsuario, IdRol),
            CONSTRAINT FK_SeguridadUsuarioRol_Usuario FOREIGN KEY (IdUsuario) REFERENCES Seguridad.Usuario (IdUsuario),
            CONSTRAINT FK_SeguridadUsuarioRol_Rol FOREIGN KEY (IdRol) REFERENCES Seguridad.Rol (IdRol)
        );
    END;

    IF OBJECT_ID(N'Seguridad.RolPermiso', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.RolPermiso
        (
            IdRol                INT NOT NULL,
            IdPermiso            INT NOT NULL,

            CONSTRAINT PK_SeguridadRolPermiso PRIMARY KEY CLUSTERED (IdRol, IdPermiso),
            CONSTRAINT FK_SeguridadRolPermiso_Rol FOREIGN KEY (IdRol) REFERENCES Seguridad.Rol (IdRol),
            CONSTRAINT FK_SeguridadRolPermiso_Permiso FOREIGN KEY (IdPermiso) REFERENCES Seguridad.Permiso (IdPermiso)
        );
    END;

    IF OBJECT_ID(N'Seguridad.AuditoriaSeguridad', N'U') IS NULL
    BEGIN
        CREATE TABLE Seguridad.AuditoriaSeguridad
        (
            IdAuditoria          BIGINT IDENTITY(1,1) NOT NULL,
            IdUsuario            INT NULL,
            Evento               NVARCHAR(50) NOT NULL,
            Detalle              NVARCHAR(1000) NULL,
            DireccionIP          NVARCHAR(45) NULL,
            FechaEvento          DATETIME2(0) NOT NULL CONSTRAINT DF_SeguridadAuditoria_FechaEvento DEFAULT (SYSUTCDATETIME()),

            CONSTRAINT PK_SeguridadAuditoria PRIMARY KEY CLUSTERED (IdAuditoria),
            CONSTRAINT FK_SeguridadAuditoria_Usuario FOREIGN KEY (IdUsuario) REFERENCES Seguridad.Usuario (IdUsuario),
            CONSTRAINT CK_SeguridadAuditoria_Evento CHECK
            (
                Evento IN (N'LoginCorrecto', N'LoginFallido', N'CambioContrasena', N'AsignacionRol', N'RevocacionRol')
            )
        );
    END;

    /* ==============================
       2) Índices recomendados
       ============================== */
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.Usuario') AND name = N'IX_SeguridadUsuario_NombreUsuario')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_SeguridadUsuario_NombreUsuario
            ON Seguridad.Usuario (NombreUsuario);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.Rol') AND name = N'IX_SeguridadRol_NombreRol')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_SeguridadRol_NombreRol
            ON Seguridad.Rol (NombreRol);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.Permiso') AND name = N'IX_SeguridadPermiso_ClavePermiso')
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_SeguridadPermiso_ClavePermiso
            ON Seguridad.Permiso (ClavePermiso);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.UsuarioRol') AND name = N'IX_SeguridadUsuarioRol_IdRol')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SeguridadUsuarioRol_IdRol
            ON Seguridad.UsuarioRol (IdRol);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.RolPermiso') AND name = N'IX_SeguridadRolPermiso_IdPermiso')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SeguridadRolPermiso_IdPermiso
            ON Seguridad.RolPermiso (IdPermiso);
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'Seguridad.AuditoriaSeguridad') AND name = N'IX_SeguridadAuditoria_IdUsuario_FechaEvento')
    BEGIN
        CREATE NONCLUSTERED INDEX IX_SeguridadAuditoria_IdUsuario_FechaEvento
            ON Seguridad.AuditoriaSeguridad (IdUsuario, FechaEvento DESC);
    END;

    /* ==============================
       3) Semillas de RBAC
       ============================== */
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol = N'Administrador')
        INSERT INTO Seguridad.Rol (NombreRol, Descripcion) VALUES (N'Administrador', N'Acceso total al sistema');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol = N'Operador')
        INSERT INTO Seguridad.Rol (NombreRol, Descripcion) VALUES (N'Operador', N'Operación diaria de marcación y consultas operativas');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol = N'Consulta')
        INSERT INTO Seguridad.Rol (NombreRol, Descripcion) VALUES (N'Consulta', N'Solo lectura de reportes y datos permitidos');

    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Usuarios.Ver')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Usuarios.Ver', N'Ver usuarios');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Usuarios.Crear')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Usuarios.Crear', N'Crear usuarios');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Usuarios.Editar')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Usuarios.Editar', N'Editar usuarios');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Usuarios.Eliminar')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Usuarios.Eliminar', N'Eliminar usuarios');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Carnets.Imprimir')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Carnets.Imprimir', N'Imprimir carnets');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Reportes.Ver')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Reportes.Ver', N'Ver reportes');
    IF NOT EXISTS (SELECT 1 FROM Seguridad.Permiso WHERE ClavePermiso = N'Configuracion.Modificar')
        INSERT INTO Seguridad.Permiso (ClavePermiso, Descripcion) VALUES (N'Configuracion.Modificar', N'Modificar configuración del sistema');

    INSERT INTO Seguridad.RolPermiso (IdRol, IdPermiso)
    SELECT r.IdRol, p.IdPermiso
    FROM Seguridad.Rol r
    CROSS JOIN Seguridad.Permiso p
    WHERE r.NombreRol = N'Administrador'
      AND NOT EXISTS
      (
          SELECT 1
          FROM Seguridad.RolPermiso rp
          WHERE rp.IdRol = r.IdRol
            AND rp.IdPermiso = p.IdPermiso
      );

    /* ==============================
       4) Migración desde tabla legacy
       ============================== */
    IF OBJECT_ID(N'dbo.TablaSeguridad', N'U') IS NOT NULL
    BEGIN
        ;WITH Legacy AS
        (
            SELECT
                LTRIM(RTRIM(CONVERT(NVARCHAR(100), t.Nombre))) AS NombreUsuario,
                LTRIM(RTRIM(CONVERT(NVARCHAR(200), t.Usuario))) AS NombreCompleto,
                LTRIM(RTRIM(CONVERT(NVARCHAR(4000), t.Contrasena))) AS ContrasenaLegacy,
                CASE WHEN ISNULL(t.Activo, 0) = 1 THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS EsActivo
            FROM dbo.TablaSeguridad t
            WHERE t.Nombre IS NOT NULL
        )
        INSERT INTO Seguridad.Usuario
        (
            NombreUsuario,
            NombreCompleto,
            HashContrasena,
            SaltContrasena,
            EsActivo,
            IntentosFallidos,
            BloqueadoHasta,
            FechaCreacion,
            FechaUltimoIngreso
        )
        SELECT
            l.NombreUsuario,
            CASE WHEN l.NombreCompleto = N'' THEN l.NombreUsuario ELSE l.NombreCompleto END,
            N'LEGACY_SHA2_512:' +
            CONVERT(NVARCHAR(256), HASHBYTES('SHA2_512', CONVERT(VARBINARY(MAX), l.ContrasenaLegacy + N':' + v.SaltGenerada)), 2),
            v.SaltGenerada,
            l.EsActivo,
            0,
            NULL,
            SYSUTCDATETIME(),
            NULL
        FROM Legacy l
        CROSS APPLY (SELECT CONVERT(NVARCHAR(255), CONVERT(VARCHAR(128), CRYPT_GEN_RANDOM(16), 2))) v(SaltGenerada)
        WHERE l.NombreUsuario <> N''
          AND l.ContrasenaLegacy IS NOT NULL
          AND NOT EXISTS
          (
              SELECT 1
              FROM Seguridad.Usuario u
              WHERE u.NombreUsuario = l.NombreUsuario
          );

        INSERT INTO Seguridad.UsuarioRol (IdUsuario, IdRol, FechaAsignacion)
        SELECT u.IdUsuario, r.IdRol, SYSUTCDATETIME()
        FROM Seguridad.Usuario u
        INNER JOIN Seguridad.Rol r
            ON r.NombreRol = N'Administrador'
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM Seguridad.UsuarioRol ur
            WHERE ur.IdUsuario = u.IdUsuario
              AND ur.IdRol = r.IdRol
        );

        INSERT INTO Seguridad.AuditoriaSeguridad (IdUsuario, Evento, Detalle, DireccionIP)
        SELECT
            u.IdUsuario,
            N'CambioContrasena',
            N'Usuario migrado desde TablaSeguridad con hash transitorio; requiere cambio de contraseña.',
            N'127.0.0.1'
        FROM Seguridad.Usuario u
        WHERE u.HashContrasena LIKE N'LEGACY_SHA2_512:%'
          AND NOT EXISTS
          (
              SELECT 1
              FROM Seguridad.AuditoriaSeguridad a
              WHERE a.IdUsuario = u.IdUsuario
                AND a.Evento = N'CambioContrasena'
                AND a.Detalle LIKE N'Usuario migrado desde TablaSeguridad%'
          );
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMensaje NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorNumero INT = ERROR_NUMBER();
    DECLARE @ErrorLinea INT = ERROR_LINE();

    RAISERROR(N'Error en migración RBAC. Numero: %d, Línea: %d, Mensaje: %s', 16, 1, @ErrorNumero, @ErrorLinea, @ErrorMensaje);
END CATCH;
GO

/* Validaciones rápidas
SELECT COUNT(*) AS TotalUsuariosSeguridad FROM Seguridad.Usuario;
SELECT COUNT(*) AS TotalRoles FROM Seguridad.Rol;
SELECT COUNT(*) AS TotalPermisos FROM Seguridad.Permiso;
SELECT TOP 20 * FROM Seguridad.AuditoriaSeguridad ORDER BY IdAuditoria DESC;
*/
