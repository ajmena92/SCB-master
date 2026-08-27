/*
  Carnet digital estudiantil: fotografía protegida, catálogo de beneficios
  reutilizado desde dbo.Usuario.TipoBeca y eventos de auditoría.

  Ejecutar manualmente por el DBA, primero en staging y con respaldo verificado.
  La API no ejecuta DDL.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'ComedorPortal.FotoEstudiante', N'U') IS NULL
CREATE TABLE ComedorPortal.FotoEstudiante (
    IdUsuario int NOT NULL CONSTRAINT PK_FotoEstudiante PRIMARY KEY,
    Contenido varbinary(max) NOT NULL,
    TipoMime varchar(50) NOT NULL,
    TamanoBytes int NOT NULL,
    Ancho int NOT NULL,
    Alto int NOT NULL,
    HashSha256 varbinary(32) NOT NULL,
    FechaCarga datetime2 NOT NULL CONSTRAINT DF_FotoEstudiante_Fecha DEFAULT SYSUTCDATETIME(),
    IdUsuarioCarga int NULL,
    Activa bit NOT NULL CONSTRAINT DF_FotoEstudiante_Activa DEFAULT 1,
    CONSTRAINT FK_FotoEstudiante_Usuario FOREIGN KEY(IdUsuario) REFERENCES dbo.Usuario(IdUsuario) ON DELETE CASCADE,
    CONSTRAINT FK_FotoEstudiante_Carga FOREIGN KEY(IdUsuarioCarga) REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL,
    CONSTRAINT CK_FotoEstudiante_Tamano CHECK (TamanoBytes BETWEEN 1 AND 5242880),
    CONSTRAINT CK_FotoEstudiante_Dimensiones CHECK (Ancho BETWEEN 120 AND 5000 AND Alto BETWEEN 120 AND 5000)
);

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id=OBJECT_ID(N'ComedorPortal.FotoEstudiante') AND name=N'IX_FotoEstudiante_Activa'
)
CREATE INDEX IX_FotoEstudiante_Activa ON ComedorPortal.FotoEstudiante(Activa);

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'ComedorPortal.AuditoriaConfirmacion')
      AND name=N'CK_AuditoriaConfirmacion_Evento'
)
BEGIN
    ALTER TABLE ComedorPortal.AuditoriaConfirmacion DROP CONSTRAINT CK_AuditoriaConfirmacion_Evento;
END;

ALTER TABLE ComedorPortal.AuditoriaConfirmacion ADD CONSTRAINT CK_AuditoriaConfirmacion_Evento CHECK(Evento IN(
    N'Confirmacion',N'Cancelacion',N'Correccion',N'PinAsignado',N'PinCambiado',
    N'FotoCargada',N'FotoEliminada',N'BeneficioActualizado',N'CarnetGenerado',N'ParametrosPortal'
));

COMMIT TRANSACTION;
