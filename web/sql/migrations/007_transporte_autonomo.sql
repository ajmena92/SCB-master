/*
  Esquema canónico independiente para el dominio de transporte.
  Repetible: cada objeto se crea solo si no existe. No lee ni modifica dbo,
  Seguridad ni ninguna tabla del sistema WinForms.
  Prerrequisitos: SQL Server 2019+, cuenta DBA para aplicar DDL y respaldo
  verificado. La cuenta de ejecución de la API solo requiere CRUD sobre
  transporte.ruta, transporte.asignacion_ruta y transporte.auditoria.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'transporte') IS NULL EXEC(N'CREATE SCHEMA transporte');

IF OBJECT_ID(N'transporte.ruta', N'U') IS NULL
BEGIN
    CREATE TABLE transporte.ruta (
        id_ruta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_transporte_ruta PRIMARY KEY,
        codigo nvarchar(50) NOT NULL,
        descripcion nvarchar(500) NOT NULL,
        color_hex char(7) NOT NULL CONSTRAINT DF_transporte_ruta_color DEFAULT '#CBD5E1',
        activo bit NOT NULL CONSTRAINT DF_transporte_ruta_activo DEFAULT 1,
        creado_por int NOT NULL,
        actualizado_por int NULL,
        direccion_ip varchar(64) NOT NULL,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_transporte_ruta_creacion DEFAULT SYSUTCDATETIME(),
        fecha_actualizacion datetime2(3) NOT NULL CONSTRAINT DF_transporte_ruta_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_transporte_ruta_codigo UNIQUE (codigo),
        CONSTRAINT CK_transporte_ruta_color CHECK (color_hex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
    );
END;

IF OBJECT_ID(N'transporte.asignacion_ruta', N'U') IS NULL
BEGIN
    CREATE TABLE transporte.asignacion_ruta (
        id_asignacion bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_transporte_asignacion PRIMARY KEY,
        id_ruta int NOT NULL,
        id_estudiante int NOT NULL,
        activa bit NOT NULL CONSTRAINT DF_transporte_asignacion_activa DEFAULT 1,
        fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_transporte_asignacion_creacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_transporte_asignacion_ruta FOREIGN KEY (id_ruta) REFERENCES transporte.ruta(id_ruta),
        CONSTRAINT UQ_transporte_asignacion_estudiante UNIQUE (id_estudiante)
    );
END;

IF OBJECT_ID(N'transporte.auditoria', N'U') IS NULL
BEGIN
    CREATE TABLE transporte.auditoria (
        id_evento bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_transporte_auditoria PRIMARY KEY,
        id_ruta int NULL,
        id_usuario int NOT NULL,
        evento varchar(30) NOT NULL,
        direccion_ip varchar(64) NOT NULL,
        fecha_evento datetime2(3) NOT NULL CONSTRAINT DF_transporte_auditoria_fecha DEFAULT SYSUTCDATETIME(),
        detalle nvarchar(500) NULL,
        CONSTRAINT FK_transporte_auditoria_ruta FOREIGN KEY (id_ruta) REFERENCES transporte.ruta(id_ruta)
    );
END;

COMMIT TRANSACTION;
