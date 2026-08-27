/*
  Identidad canónica de la plataforma web.
  Repetible e independiente de dbo, Seguridad y WinForms.
  Aplicar manualmente en staging, con respaldo verificado, usando una cuenta DBA.
  La cuenta de ejecución de la API requiere CRUD únicamente sobre identidad.*.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'identidad') IS NULL EXEC(N'CREATE SCHEMA identidad');

IF OBJECT_ID(N'identidad.usuario', N'U') IS NULL
BEGIN
    CREATE TABLE identidad.usuario (
        id_usuario int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_identidad_usuario PRIMARY KEY,
        nombre_usuario nvarchar(100) NOT NULL,
        hash_contrasena nvarchar(255) NOT NULL,
        activo bit NOT NULL CONSTRAINT DF_identidad_usuario_activo DEFAULT 1,
        fecha_creacion datetime2(3) NOT NULL
            CONSTRAINT DF_identidad_usuario_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL
            CONSTRAINT DF_identidad_usuario_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_identidad_usuario_nombre UNIQUE (nombre_usuario),
        CONSTRAINT CK_identidad_usuario_nombre_no_vacio CHECK (LEN(LTRIM(RTRIM(nombre_usuario))) > 0),
        CONSTRAINT CK_identidad_usuario_hash_argon2 CHECK (hash_contrasena LIKE '$argon2id$%')
    );
END;

IF OBJECT_ID(N'identidad.permiso', N'U') IS NULL
BEGIN
    CREATE TABLE identidad.permiso (
        id_permiso int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_identidad_permiso PRIMARY KEY,
        clave nvarchar(150) NOT NULL,
        descripcion nvarchar(300) NULL,
        activo bit NOT NULL CONSTRAINT DF_identidad_permiso_activo DEFAULT 1,
        CONSTRAINT UQ_identidad_permiso_clave UNIQUE (clave),
        CONSTRAINT CK_identidad_permiso_clave_no_vacia CHECK (LEN(LTRIM(RTRIM(clave))) > 0)
    );
END;

IF OBJECT_ID(N'identidad.usuario_permiso', N'U') IS NULL
BEGIN
    CREATE TABLE identidad.usuario_permiso (
        id_usuario int NOT NULL,
        id_permiso int NOT NULL,
        fecha_asignacion datetime2(3) NOT NULL
            CONSTRAINT DF_identidad_usuario_permiso_fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_identidad_usuario_permiso PRIMARY KEY (id_usuario, id_permiso),
        CONSTRAINT FK_identidad_usuario_permiso_usuario FOREIGN KEY (id_usuario)
            REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE,
        CONSTRAINT FK_identidad_usuario_permiso_permiso FOREIGN KEY (id_permiso)
            REFERENCES identidad.permiso(id_permiso) ON DELETE CASCADE
    );
END;

IF OBJECT_ID(N'identidad.sesion', N'U') IS NULL
BEGIN
    CREATE TABLE identidad.sesion (
        id_sesion varchar(64) NOT NULL
            CONSTRAINT PK_identidad_sesion PRIMARY KEY,
        id_usuario int NOT NULL,
        secreto_hash char(64) NOT NULL,
        expira_en datetime2(3) NOT NULL,
        csrf_hash char(64) NULL,
        revocada bit NOT NULL CONSTRAINT DF_identidad_sesion_revocada DEFAULT 0,
        fecha_creacion datetime2(3) NOT NULL
            CONSTRAINT DF_identidad_sesion_creacion DEFAULT SYSUTCDATETIME(),
        fecha_revocacion datetime2(3) NULL,
        CONSTRAINT FK_identidad_sesion_usuario FOREIGN KEY (id_usuario)
            REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE,
        CONSTRAINT CK_identidad_sesion_secreto_hash_hex CHECK (secreto_hash NOT LIKE '%[^0-9A-Fa-f]%'),
        CONSTRAINT CK_identidad_sesion_csrf_hash_hex CHECK (csrf_hash IS NULL OR csrf_hash NOT LIKE '%[^0-9A-Fa-f]%')
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_identidad_sesion_vigente'
               AND object_id = OBJECT_ID(N'identidad.sesion'))
    CREATE INDEX IX_identidad_sesion_vigente
        ON identidad.sesion (id_usuario, revocada, expira_en);

IF NOT EXISTS (SELECT 1 FROM identidad.permiso WHERE clave = N'rutas.administrar')
    INSERT INTO identidad.permiso (clave, descripcion)
    VALUES (N'rutas.administrar', N'Administrar rutas de transporte');

COMMIT TRANSACTION;

/*
  Reversión controlada: ejecutar 008_identidad_canonica_revertir.sql únicamente
  después de revocar accesos de la API y confirmar que no existen datos que deban
  conservarse. El script no se ejecuta automáticamente como parte de esta migración.
*/
