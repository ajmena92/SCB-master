/* Migración repetible de dominios web. */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='menu') EXEC('CREATE SCHEMA menu');
IF OBJECT_ID('menu.plantilla','U') IS NULL CREATE TABLE menu.plantilla(id_plantilla INT IDENTITY PRIMARY KEY,semana TINYINT NOT NULL,dia TINYINT NOT NULL,titulo NVARCHAR(160) NOT NULL,observaciones NVARCHAR(500) NULL,activo BIT NOT NULL DEFAULT 1,creado_por INT NOT NULL,actualizado_por INT NULL,CONSTRAINT uq_menu_dia UNIQUE(semana,dia));
IF OBJECT_ID('menu.componente','U') IS NULL CREATE TABLE menu.componente(id_componente INT IDENTITY PRIMARY KEY,id_plantilla INT NOT NULL REFERENCES menu.plantilla(id_plantilla),nombre NVARCHAR(120) NOT NULL,tipo NVARCHAR(40) NOT NULL,orden TINYINT NOT NULL);
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='comedor') EXEC('CREATE SCHEMA comedor');
IF OBJECT_ID('comedor.registro','U') IS NULL CREATE TABLE comedor.registro(id_registro BIGINT IDENTITY PRIMARY KEY,id_estudiante INT NOT NULL,fecha DATE NOT NULL,registrado_por INT NOT NULL,creado_en DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT uq_comedor_estudiante_fecha UNIQUE(id_estudiante,fecha));
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name='soporte') EXEC('CREATE SCHEMA soporte');
IF OBJECT_ID('soporte.solicitud','U') IS NULL CREATE TABLE soporte.solicitud(id_solicitud BIGINT IDENTITY PRIMARY KEY,asunto NVARCHAR(160) NOT NULL,detalle NVARCHAR(2000) NOT NULL,estado NVARCHAR(20) NOT NULL,creado_por INT NOT NULL,creado_en DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME());
