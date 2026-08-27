/* Esquema canónico independiente para beneficios. No importa datos del sistema local. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'beneficios') IS NULL EXEC(N'CREATE SCHEMA beneficios');
IF OBJECT_ID(N'beneficios.tipo_beneficio', N'U') IS NULL
BEGIN
    CREATE TABLE beneficios.tipo_beneficio (
        id_beneficio int IDENTITY(1,1) NOT NULL CONSTRAINT PK_beneficios_tipo PRIMARY KEY,
        nombre nvarchar(100) NOT NULL,
        descripcion nvarchar(500) NULL,
        dias_permitidos tinyint NOT NULL CONSTRAINT DF_beneficios_dias DEFAULT 5,
        activo bit NOT NULL CONSTRAINT DF_beneficios_activo DEFAULT 1,
        creado_por int NULL, actualizado_por int NULL, direccion_ip varchar(64) NULL,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_beneficios_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL CONSTRAINT DF_beneficios_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_beneficios_tipo_nombre UNIQUE (nombre),
        CONSTRAINT CK_beneficios_dias CHECK (dias_permitidos BETWEEN 0 AND 7)
    );
END;
IF OBJECT_ID(N'beneficios.asignacion', N'U') IS NULL
BEGIN
    CREATE TABLE beneficios.asignacion (
        id_estudiante int NOT NULL CONSTRAINT PK_beneficios_asignacion PRIMARY KEY,
        id_beneficio int NULL,
        creado_por int NULL, actualizado_por int NULL, direccion_ip varchar(64) NULL,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_beneficios_asignacion_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL CONSTRAINT DF_beneficios_asignacion_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_beneficios_asignacion_beneficio FOREIGN KEY (id_beneficio) REFERENCES beneficios.tipo_beneficio(id_beneficio)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'beneficios.tipo_beneficio') AND name=N'IX_beneficios_tipo_activo')
    CREATE INDEX IX_beneficios_tipo_activo ON beneficios.tipo_beneficio(activo, nombre);
COMMIT TRANSACTION;
