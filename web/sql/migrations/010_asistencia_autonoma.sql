/* Esquema canónico independiente para asistencia. Repetible y sin referencias a dbo, Seguridad ni tablas locales. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'asistencia') IS NULL EXEC(N'CREATE SCHEMA asistencia');
IF OBJECT_ID(N'asistencia.marca', N'U') IS NULL
BEGIN
    CREATE TABLE asistencia.marca (
        id_marca bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_asistencia_marca PRIMARY KEY,
        id_estudiante int NOT NULL, fecha date NOT NULL, estado varchar(20) NOT NULL,
        observacion nvarchar(500) NULL, corregida bit NOT NULL CONSTRAINT DF_asistencia_marca_corregida DEFAULT 0,
        creado_por int NOT NULL, actualizado_por int NULL, direccion_ip varchar(64) NOT NULL,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_asistencia_marca_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL CONSTRAINT DF_asistencia_marca_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_asistencia_marca_estudiante_fecha UNIQUE (id_estudiante, fecha),
        CONSTRAINT CK_asistencia_marca_estado CHECK (estado IN ('presente', 'ausente', 'tardanza', 'justificada'))
    );
END;
IF OBJECT_ID(N'asistencia.correccion', N'U') IS NULL
BEGIN
    CREATE TABLE asistencia.correccion (
        id_correccion bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_asistencia_correccion PRIMARY KEY,
        id_marca bigint NOT NULL, motivo nvarchar(500) NOT NULL, id_usuario int NOT NULL,
        direccion_ip varchar(64) NOT NULL,
        fecha_correccion datetime2(3) NOT NULL CONSTRAINT DF_asistencia_correccion_fecha DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_asistencia_correccion_marca FOREIGN KEY (id_marca) REFERENCES asistencia.marca(id_marca)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'asistencia.marca') AND name = N'IX_asistencia_marca_fecha')
    CREATE INDEX IX_asistencia_marca_fecha ON asistencia.marca(fecha, id_estudiante);
COMMIT TRANSACTION;
