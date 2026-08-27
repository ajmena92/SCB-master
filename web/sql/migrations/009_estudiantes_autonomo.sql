/* Esquema canónico independiente para estudiantes. Repetible y sin referencias
   a dbo, Seguridad ni a tablas del sistema local. La API nunca ejecuta este DDL. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'estudiantes') IS NULL EXEC(N'CREATE SCHEMA estudiantes');

IF OBJECT_ID(N'estudiantes.estudiante', N'U') IS NULL
BEGIN
    CREATE TABLE estudiantes.estudiante (
        id_estudiante int IDENTITY(1,1) NOT NULL CONSTRAINT PK_estudiantes_estudiante PRIMARY KEY,
        carne nvarchar(30) NOT NULL,
        nombre nvarchar(100) NOT NULL,
        primer_apellido nvarchar(100) NOT NULL,
        segundo_apellido nvarchar(100) NULL,
        cedula nvarchar(30) NULL,
        seccion nvarchar(30) NULL,
        activo bit NOT NULL CONSTRAINT DF_estudiantes_estudiante_activo DEFAULT 1,
        creado_por int NULL,
        actualizado_por int NULL,
        direccion_ip varchar(64) NULL,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_estudiantes_estudiante_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL CONSTRAINT DF_estudiantes_estudiante_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_estudiantes_estudiante_carne UNIQUE (carne)
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'estudiantes.estudiante') AND name=N'IX_estudiantes_estudiante_activo')
    CREATE INDEX IX_estudiantes_estudiante_activo ON estudiantes.estudiante(activo, primer_apellido, nombre);

COMMIT TRANSACTION;
