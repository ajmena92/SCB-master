/*
  AUTHORIZED SCOPE: menu storage only.
  Run manually in a restored staging copy first, then only with DBA approval.
  This migration does not alter dbo.* or Seguridad.* tables. Menu history is
  retained if an administrative user is deleted: modifier references become NULL.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'ComedorPortal') EXEC(N'CREATE SCHEMA ComedorPortal');

IF OBJECT_ID(N'ComedorPortal.MenuPlantilla',N'U') IS NULL
CREATE TABLE ComedorPortal.MenuPlantilla (
  IdMenuPlantilla int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuPlantilla PRIMARY KEY,
  SemanaMes tinyint NOT NULL CONSTRAINT CK_MenuPlantilla_Semana CHECK (SemanaMes BETWEEN 1 AND 5),
  DiaSemana tinyint NOT NULL CONSTRAINT CK_MenuPlantilla_Dia CHECK (DiaSemana BETWEEN 1 AND 5),
  Titulo nvarchar(150) NOT NULL, Observaciones nvarchar(500) NULL, Activo bit NOT NULL CONSTRAINT DF_MenuPlantilla_Activo DEFAULT 1,
  IdUsuarioModifica int NULL, FechaModificacion datetime2 NOT NULL CONSTRAINT DF_MenuPlantilla_Fecha DEFAULT SYSUTCDATETIME(),
  CONSTRAINT UQ_MenuPlantilla_SemanaDia UNIQUE (SemanaMes,DiaSemana),
  CONSTRAINT FK_MenuPlantilla_UsuarioModifica FOREIGN KEY (IdUsuarioModifica)
    REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL
);
IF OBJECT_ID(N'ComedorPortal.MenuComponente',N'U') IS NULL
CREATE TABLE ComedorPortal.MenuComponente (
  IdMenuComponente int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuComponente PRIMARY KEY,
  IdMenuPlantilla int NOT NULL, Orden tinyint NOT NULL CONSTRAINT CK_MenuComponente_Orden CHECK (Orden BETWEEN 1 AND 50),
  Nombre nvarchar(150) NOT NULL, TipoComponente nvarchar(50) NOT NULL,
  CONSTRAINT FK_MenuComponente_Plantilla FOREIGN KEY (IdMenuPlantilla) REFERENCES ComedorPortal.MenuPlantilla(IdMenuPlantilla) ON DELETE CASCADE,
  CONSTRAINT UQ_MenuComponente_Orden UNIQUE(IdMenuPlantilla,Orden)
);
IF OBJECT_ID(N'ComedorPortal.MenuSustitucion',N'U') IS NULL
CREATE TABLE ComedorPortal.MenuSustitucion (
  IdMenuSustitucion int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuSustitucion PRIMARY KEY,
  Fecha date NOT NULL CONSTRAINT UQ_MenuSustitucion_Fecha UNIQUE, Titulo nvarchar(150) NOT NULL, Observaciones nvarchar(500) NULL,
  IdUsuarioModifica int NULL, FechaModificacion datetime2 NOT NULL CONSTRAINT DF_MenuSustitucion_Fecha DEFAULT SYSUTCDATETIME(),
  CONSTRAINT FK_MenuSustitucion_UsuarioModifica FOREIGN KEY (IdUsuarioModifica)
    REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL
);
IF OBJECT_ID(N'ComedorPortal.MenuSustitucionComponente',N'U') IS NULL
CREATE TABLE ComedorPortal.MenuSustitucionComponente (
  IdMenuSustitucionComponente int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MenuSustitucionComponente PRIMARY KEY,
  IdMenuSustitucion int NOT NULL, Orden tinyint NOT NULL CONSTRAINT CK_MenuSustitucionComponente_Orden CHECK (Orden BETWEEN 1 AND 50),
  Nombre nvarchar(150) NOT NULL, TipoComponente nvarchar(50) NOT NULL,
  CONSTRAINT FK_MenuSustitucionComponente_Sustitucion FOREIGN KEY (IdMenuSustitucion) REFERENCES ComedorPortal.MenuSustitucion(IdMenuSustitucion) ON DELETE CASCADE,
  CONSTRAINT UQ_MenuSustitucionComponente_Orden UNIQUE(IdMenuSustitucion,Orden)
);
COMMIT TRANSACTION;
