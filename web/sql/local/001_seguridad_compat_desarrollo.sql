/*
  LOCAL DEVELOPMENT ONLY — NEVER RUN IN PRODUCTION.

  This compatibility schema is solely for an older local SCSC copy that has
  dbo.Usuario but does not have the Seguridad RBAC schema used by the desktop
  and the web portal. It does not change dbo.* tables or reuse local student
  identities as administrative users.

  Invoke only with: sqlcmd ... -v LOCAL_DEV_ONLY=true -i this-file.sql
*/
IF N'$(LOCAL_DEV_ONLY)' <> N'true'
    THROW 51000, 'This is a local-development-only script. Set LOCAL_DEV_ONLY=true explicitly.', 1;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'Seguridad') IS NULL EXEC(N'CREATE SCHEMA Seguridad');

IF OBJECT_ID(N'Seguridad.Usuario', N'U') IS NULL
CREATE TABLE Seguridad.Usuario (
    IdUsuario int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeguridadUsuario PRIMARY KEY,
    NombreUsuario nvarchar(100) NOT NULL CONSTRAINT UQ_SeguridadUsuario_Nombre UNIQUE,
    NombreCompleto nvarchar(200) NOT NULL,
    HashContrasena nvarchar(512) NOT NULL,
    SaltContrasena nvarchar(256) NULL,
    EsActivo bit NOT NULL CONSTRAINT DF_SeguridadUsuario_Activo DEFAULT 1,
    IntentosFallidos int NOT NULL CONSTRAINT DF_SeguridadUsuario_Intentos DEFAULT 0,
    BloqueadoHasta datetime2 NULL,
    FechaUltimoIngreso datetime2 NULL,
    CONSTRAINT CK_SeguridadUsuario_Intentos CHECK (IntentosFallidos BETWEEN 0 AND 20)
);

IF OBJECT_ID(N'Seguridad.Rol', N'U') IS NULL
CREATE TABLE Seguridad.Rol (
    IdRol int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SeguridadRol PRIMARY KEY,
    NombreRol nvarchar(100) NOT NULL CONSTRAINT UQ_SeguridadRol_Nombre UNIQUE,
    EsActivo bit NOT NULL CONSTRAINT DF_SeguridadRol_Activo DEFAULT 1
);

IF OBJECT_ID(N'Seguridad.UsuarioRol', N'U') IS NULL
CREATE TABLE Seguridad.UsuarioRol (
    IdUsuario int NOT NULL,
    IdRol int NOT NULL,
    CONSTRAINT PK_SeguridadUsuarioRol PRIMARY KEY (IdUsuario, IdRol),
    CONSTRAINT FK_SeguridadUsuarioRol_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE CASCADE,
    CONSTRAINT FK_SeguridadUsuarioRol_Rol FOREIGN KEY (IdRol)
        REFERENCES Seguridad.Rol(IdRol) ON DELETE CASCADE
);

IF OBJECT_ID(N'Seguridad.AuditoriaSeguridad', N'U') IS NULL
CREATE TABLE Seguridad.AuditoriaSeguridad (
    IdAuditoria bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditoriaSeguridad PRIMARY KEY,
    IdUsuario int NULL,
    Evento nvarchar(50) NOT NULL,
    Detalle nvarchar(1000) NULL,
    DireccionIP nvarchar(64) NULL,
    FechaEvento datetime2 NOT NULL CONSTRAINT DF_AuditoriaSeguridad_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AuditoriaSeguridad_Usuario FOREIGN KEY (IdUsuario)
        REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL
);

IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol=N'Administrador')
    INSERT INTO Seguridad.Rol (NombreRol, EsActivo) VALUES (N'Administrador', 1);
IF NOT EXISTS (SELECT 1 FROM Seguridad.Rol WHERE NombreRol=N'Operador')
    INSERT INTO Seguridad.Rol (NombreRol, EsActivo) VALUES (N'Operador', 1);

-- Isolated smoke-test account. The legacy hash matches the current portal
-- verifier; it has no relationship to any desktop or production identity.
IF NOT EXISTS (SELECT 1 FROM Seguridad.Usuario WHERE NombreUsuario=N'portal_dev_admin')
    INSERT INTO Seguridad.Usuario
        (NombreUsuario, NombreCompleto, HashContrasena, SaltContrasena, EsActivo)
    VALUES
        (N'portal_dev_admin', N'Administrador de desarrollo local',
         N'LEGACY_SHA2_512:' + CONVERT(varchar(128), HASHBYTES('SHA2_512', CONVERT(varbinary(max), 'PortalDev!2026:local-dev-v1')), 2),
         N'local-dev-v1', 1);

DECLARE @IdAdmin int = (SELECT IdUsuario FROM Seguridad.Usuario WHERE NombreUsuario=N'portal_dev_admin');
DECLARE @IdRolAdmin int = (SELECT IdRol FROM Seguridad.Rol WHERE NombreRol=N'Administrador');
IF NOT EXISTS (SELECT 1 FROM Seguridad.UsuarioRol WHERE IdUsuario=@IdAdmin AND IdRol=@IdRolAdmin)
    INSERT INTO Seguridad.UsuarioRol (IdUsuario, IdRol) VALUES (@IdAdmin, @IdRolAdmin);

COMMIT TRANSACTION;
