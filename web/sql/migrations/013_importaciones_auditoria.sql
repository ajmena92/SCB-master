SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'importaciones') IS NULL EXEC(N'CREATE SCHEMA importaciones');
IF SCHEMA_ID(N'auditoria') IS NULL EXEC(N'CREATE SCHEMA auditoria');
IF OBJECT_ID(N'importaciones.lote', N'U') IS NULL
BEGIN
 CREATE TABLE importaciones.lote (
  id_lote bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_importaciones_lote PRIMARY KEY,
  nombre_archivo nvarchar(260) NOT NULL, estado varchar(12) NOT NULL CONSTRAINT DF_importaciones_estado DEFAULT 'aplicado',
  total_filas int NOT NULL, errores_json nvarchar(max) NOT NULL CONSTRAINT DF_importaciones_errores DEFAULT '[]',
  creado_por int NULL, creado_en datetime2(3) NOT NULL CONSTRAINT DF_importaciones_creado DEFAULT SYSUTCDATETIME(),
  revertido_por int NULL, revertido_en datetime2(3) NULL,
  CONSTRAINT CK_importaciones_estado CHECK (estado IN ('aplicado','revertido')),
  CONSTRAINT CK_importaciones_total CHECK (total_filas >= 0)
 );
END;
IF OBJECT_ID(N'importaciones.fila', N'U') IS NULL
BEGIN
 CREATE TABLE importaciones.fila (
  id_fila bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_importaciones_fila PRIMARY KEY,
  id_lote bigint NOT NULL CONSTRAINT FK_importaciones_fila_lote REFERENCES importaciones.lote(id_lote),
  datos_json nvarchar(max) NOT NULL
 );
END;
IF OBJECT_ID(N'auditoria.evento', N'U') IS NULL
BEGIN
 CREATE TABLE auditoria.evento (
  id_evento bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_auditoria_evento PRIMARY KEY,
  modulo varchar(80) NOT NULL, accion varchar(80) NOT NULL, entidad varchar(100) NOT NULL,
  id_entidad varchar(100) NULL, detalle_json nvarchar(max) NOT NULL CONSTRAINT DF_auditoria_detalle DEFAULT '{}',
  id_usuario int NULL, direccion_ip varchar(64) NULL,
  creado_en datetime2(3) NOT NULL CONSTRAINT DF_auditoria_creado DEFAULT SYSUTCDATETIME()
 );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'auditoria.evento') AND name=N'IX_auditoria_evento_fecha')
 CREATE INDEX IX_auditoria_evento_fecha ON auditoria.evento(creado_en DESC, id_evento DESC);
COMMIT TRANSACTION;
