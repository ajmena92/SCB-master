/*
  AUTHORIZED SCOPE: portal state only.

  Required by the portal for student PINs, opaque sessions, attendance linkage,
  and audit. Run manually by the authorized DBA, first in staging, after a
  verified backup. The API account never executes DDL.

  This migration does not alter dbo.* or Seguridad.*. The application may
  insert/delete only web-owned dbo.RegistroTransporte rows that are explicitly
  linked from ConfirmacionAsistencia with MarcaCreadaPorPortal=1.
*/
-- Required for the filtered index IX_SesionWeb_Expira when the script is run
-- through sqlcmd or a client whose default session has QUOTED_IDENTIFIER OFF.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name=N'ComedorPortal') EXEC(N'CREATE SCHEMA ComedorPortal');
IF OBJECT_ID(N'ComedorPortal.CredencialEstudiante',N'U') IS NULL
CREATE TABLE ComedorPortal.CredencialEstudiante (
 IdUsuario int NOT NULL CONSTRAINT PK_CredencialEstudiante PRIMARY KEY, PinHash varbinary(64) NOT NULL, PinSalt varbinary(32) NOT NULL,
 PinIteraciones int NOT NULL, DebeCambiarPin bit NOT NULL CONSTRAINT DF_Credencial_Cambio DEFAULT 1, IntentosFallidos int NOT NULL CONSTRAINT DF_Credencial_Intentos DEFAULT 0,
 BloqueadoHasta datetime2 NULL, FechaCreacion datetime2 NOT NULL CONSTRAINT DF_Credencial_Creada DEFAULT SYSUTCDATETIME(), FechaActualizacion datetime2 NOT NULL CONSTRAINT DF_Credencial_Actualizada DEFAULT SYSUTCDATETIME(),
 CONSTRAINT CK_Credencial_Iteraciones CHECK (PinIteraciones BETWEEN 100000 AND 1000000),
 CONSTRAINT CK_Credencial_Intentos CHECK (IntentosFallidos BETWEEN 0 AND 20),
 CONSTRAINT FK_Credencial_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario) ON DELETE CASCADE
);
IF OBJECT_ID(N'ComedorPortal.ConfirmacionAsistencia',N'U') IS NULL
CREATE TABLE ComedorPortal.ConfirmacionAsistencia (
 IdConfirmacionAsistencia int IDENTITY(1,1) NOT NULL CONSTRAINT PK_ConfirmacionAsistencia PRIMARY KEY, IdUsuario int NOT NULL, FechaServicio date NOT NULL,
 IdRegistroTransporte int NULL, MarcaCreadaPorPortal bit NOT NULL CONSTRAINT DF_Confirmacion_MarcaPortal DEFAULT 0,
 Estado nvarchar(20) NOT NULL CONSTRAINT CK_Confirmacion_Estado CHECK(Estado IN(N'Confirmada',N'Cancelada',N'Corregida')),
 FechaConfirmacion datetime2 NULL, FechaCancelacion datetime2 NULL, IdUsuarioAdmin int NULL, MotivoCorreccion nvarchar(500) NULL,
 CONSTRAINT UQ_Confirmacion_UsuarioFecha UNIQUE(IdUsuario,FechaServicio),
 -- Historical attendance must not become orphaned. A hard delete of a student
 -- with confirmations is therefore rejected rather than losing traceability.
 CONSTRAINT FK_Confirmacion_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario),
 CONSTRAINT FK_Confirmacion_Registro FOREIGN KEY(IdRegistroTransporte) REFERENCES dbo.RegistroTransporte(IdTransaccion),
 CONSTRAINT FK_Confirmacion_UsuarioAdmin FOREIGN KEY(IdUsuarioAdmin) REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL,
 CONSTRAINT CK_Confirmacion_Fechas CHECK (
   (Estado IN(N'Confirmada',N'Corregida') AND FechaConfirmacion IS NOT NULL AND FechaCancelacion IS NULL AND IdRegistroTransporte IS NOT NULL)
   OR (Estado=N'Cancelada' AND FechaCancelacion IS NOT NULL AND IdRegistroTransporte IS NULL)
 ),
 CONSTRAINT CK_Confirmacion_MarcaPortal CHECK (MarcaCreadaPorPortal=0 OR IdRegistroTransporte IS NOT NULL OR Estado=N'Cancelada'),
 CONSTRAINT CK_Confirmacion_Correccion CHECK (Estado<>N'Corregida' OR (IdUsuarioAdmin IS NOT NULL AND LEN(LTRIM(RTRIM(MotivoCorreccion))) > 0))
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'ComedorPortal.ConfirmacionAsistencia') AND name=N'IX_Confirmacion_Fecha') CREATE INDEX IX_Confirmacion_Fecha ON ComedorPortal.ConfirmacionAsistencia(FechaServicio,Estado);
IF OBJECT_ID(N'ComedorPortal.AuditoriaConfirmacion',N'U') IS NULL
CREATE TABLE ComedorPortal.AuditoriaConfirmacion (
 IdAuditoria bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditoriaConfirmacion PRIMARY KEY, IdConfirmacionAsistencia int NULL, IdUsuarioEstudiante int NULL, IdUsuarioAdmin int NULL,
 Evento nvarchar(50) NOT NULL CONSTRAINT CK_AuditoriaConfirmacion_Evento CHECK(Evento IN(N'Confirmacion',N'Cancelacion',N'Correccion',N'PinAsignado',N'PinCambiado')),
 Detalle nvarchar(1000) NULL, FechaEvento datetime2 NOT NULL CONSTRAINT DF_AuditoriaConfirmacion_Fecha DEFAULT SYSUTCDATETIME(), DireccionIp nvarchar(64) NULL,
 -- Audit rows outlive user/session cleanup; identities are nulled, not deleted.
 CONSTRAINT FK_Auditoria_Confirmacion FOREIGN KEY(IdConfirmacionAsistencia) REFERENCES ComedorPortal.ConfirmacionAsistencia(IdConfirmacionAsistencia) ON DELETE SET NULL,
 CONSTRAINT FK_Auditoria_Estudiante FOREIGN KEY(IdUsuarioEstudiante) REFERENCES dbo.Usuario(IdUsuario) ON DELETE SET NULL,
 CONSTRAINT FK_Auditoria_UsuarioAdmin FOREIGN KEY(IdUsuarioAdmin) REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL
);
IF OBJECT_ID(N'ComedorPortal.SesionWeb',N'U') IS NULL
CREATE TABLE ComedorPortal.SesionWeb (
 IdSesion bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_SesionWeb PRIMARY KEY, TipoSujeto nvarchar(20) NOT NULL CONSTRAINT CK_SesionWeb_Tipo CHECK(TipoSujeto IN(N'estudiante',N'administrativo')),
 IdUsuarioEstudiante int NULL, IdUsuarioAdministrativo int NULL,
 TokenHash varbinary(32) NOT NULL CONSTRAINT UQ_SesionWeb_Token UNIQUE, CsrfHash varbinary(32) NOT NULL, CreadaEn datetime2 NOT NULL, ExpiraEn datetime2 NOT NULL, RevocadaEn datetime2 NULL,
 CONSTRAINT CK_SesionWeb_Sujeto CHECK (
   (TipoSujeto=N'estudiante' AND IdUsuarioEstudiante IS NOT NULL AND IdUsuarioAdministrativo IS NULL)
   OR (TipoSujeto=N'administrativo' AND IdUsuarioAdministrativo IS NOT NULL AND IdUsuarioEstudiante IS NULL)
 ),
 CONSTRAINT CK_SesionWeb_Fechas CHECK (ExpiraEn > CreadaEn),
 -- A deleted identity must not retain a live browser session.
 CONSTRAINT FK_SesionWeb_Estudiante FOREIGN KEY(IdUsuarioEstudiante) REFERENCES dbo.Usuario(IdUsuario) ON DELETE CASCADE,
 CONSTRAINT FK_SesionWeb_Administrativo FOREIGN KEY(IdUsuarioAdministrativo) REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE CASCADE
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'ComedorPortal.SesionWeb') AND name=N'IX_SesionWeb_Expira') CREATE INDEX IX_SesionWeb_Expira ON ComedorPortal.SesionWeb(ExpiraEn) WHERE RevocadaEn IS NULL;
COMMIT TRANSACTION;
