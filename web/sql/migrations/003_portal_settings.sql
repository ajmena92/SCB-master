/*
  AUTHORIZED SCOPE: portal-only confirmation-window settings.

  Run manually by the authorized DBA after 001 and 002, first in staging and
  with a verified backup. This script never changes dbo.Horario: its current
  closing times are copied as the portal defaults.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;

IF OBJECT_ID(N'ComedorPortal.ConfiguracionPortal', N'U') IS NULL
CREATE TABLE ComedorPortal.ConfiguracionPortal (
    IdConfiguracion tinyint NOT NULL CONSTRAINT PK_ConfiguracionPortal PRIMARY KEY,
    MinutosAvisoPrevio tinyint NOT NULL CONSTRAINT DF_ConfiguracionPortal_MinutosAvisoPrevio DEFAULT 15,
    FechaActualizacion datetime2 NOT NULL CONSTRAINT DF_ConfiguracionPortal_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_ConfiguracionPortal_Unica CHECK (IdConfiguracion = 1),
    CONSTRAINT CK_ConfiguracionPortal_MinutosAvisoPrevio CHECK (MinutosAvisoPrevio BETWEEN 1 AND 120)
);

-- Supports a safely re-runnable migration if an earlier staging draft used the
-- shorter column name. No setting is discarded during that upgrade.
IF COL_LENGTH(N'ComedorPortal.ConfiguracionPortal', N'MinutosAvisoPrevio') IS NULL
BEGIN
    EXEC sys.sp_executesql N'ALTER TABLE ComedorPortal.ConfiguracionPortal ADD MinutosAvisoPrevio tinyint NULL;';
    IF COL_LENGTH(N'ComedorPortal.ConfiguracionPortal', N'MinutosAviso') IS NOT NULL
        EXEC sys.sp_executesql N'UPDATE ComedorPortal.ConfiguracionPortal SET MinutosAvisoPrevio=MinutosAviso WHERE MinutosAvisoPrevio IS NULL;';
    EXEC sys.sp_executesql N'UPDATE ComedorPortal.ConfiguracionPortal SET MinutosAvisoPrevio=15 WHERE MinutosAvisoPrevio IS NULL;';
    EXEC sys.sp_executesql N'ALTER TABLE ComedorPortal.ConfiguracionPortal ALTER COLUMN MinutosAvisoPrevio tinyint NOT NULL;';
END;

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRY
BEGIN TRANSACTION;
IF NOT EXISTS (SELECT 1 FROM ComedorPortal.ConfiguracionPortal WHERE IdConfiguracion = 1)
    INSERT INTO ComedorPortal.ConfiguracionPortal(IdConfiguracion, MinutosAvisoPrevio) VALUES (1, 15);

IF OBJECT_ID(N'ComedorPortal.CK_ConfiguracionPortal_MinutosAvisoPrevio', N'C') IS NULL
    ALTER TABLE ComedorPortal.ConfiguracionPortal WITH CHECK ADD CONSTRAINT CK_ConfiguracionPortal_MinutosAvisoPrevio
        CHECK (MinutosAvisoPrevio BETWEEN 1 AND 120);

IF OBJECT_ID(N'ComedorPortal.ConfiguracionHorario', N'U') IS NULL
CREATE TABLE ComedorPortal.ConfiguracionHorario (
    IdHorario int NOT NULL CONSTRAINT PK_ConfiguracionHorario PRIMARY KEY,
    HoraLimite time NOT NULL,
    FechaActualizacion datetime2 NOT NULL CONSTRAINT DF_ConfiguracionHorario_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ConfiguracionHorario_Horario FOREIGN KEY(IdHorario) REFERENCES dbo.Horario(IdHorario) ON DELETE CASCADE,
    CONSTRAINT CK_ConfiguracionHorario_HoraLimite CHECK (HoraLimite >= CAST('00:00:00' AS time))
);

-- Seed only missing portal rows so a re-run cannot overwrite a portal setting.
INSERT INTO ComedorPortal.ConfiguracionHorario(IdHorario, HoraLimite)
SELECT h.IdHorario, h.HoraLimite
FROM dbo.Horario h
LEFT JOIN ComedorPortal.ConfiguracionHorario p ON p.IdHorario = h.IdHorario
WHERE p.IdHorario IS NULL;

-- A first staging draft used the default NO ACTION foreign key. Upgrade it
-- safely so deleting a schedule in the desktop application cannot be blocked
-- by its portal-only setting. This alters only ComedorPortal metadata.
DECLARE @ConfiguracionHorarioFk sysname;
SELECT TOP (1) @ConfiguracionHorarioFk = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
WHERE fk.parent_object_id = OBJECT_ID(N'ComedorPortal.ConfiguracionHorario')
  AND fk.referenced_object_id = OBJECT_ID(N'dbo.Horario')
  AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'IdHorario';

IF @ConfiguracionHorarioFk IS NOT NULL
   AND EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE object_id = OBJECT_ID(N'ComedorPortal.' + @ConfiguracionHorarioFk)
         AND delete_referential_action <> 1
   )
BEGIN
    DECLARE @DropConfiguracionHorarioFkSql nvarchar(max) =
        N'ALTER TABLE ComedorPortal.ConfiguracionHorario DROP CONSTRAINT ' + QUOTENAME(@ConfiguracionHorarioFk) + N';';
    EXEC sys.sp_executesql @DropConfiguracionHorarioFkSql;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    WHERE fk.parent_object_id = OBJECT_ID(N'ComedorPortal.ConfiguracionHorario')
      AND fk.referenced_object_id = OBJECT_ID(N'dbo.Horario')
      AND COL_NAME(fkc.parent_object_id, fkc.parent_column_id) = N'IdHorario'
)
    ALTER TABLE ComedorPortal.ConfiguracionHorario WITH CHECK ADD CONSTRAINT FK_ConfiguracionHorario_Horario
        FOREIGN KEY(IdHorario) REFERENCES dbo.Horario(IdHorario) ON DELETE CASCADE;

-- Settings changes use the existing portal audit trail. Expand its event
-- constraint in this migration instead of introducing a second audit stream.
ALTER TABLE ComedorPortal.AuditoriaConfirmacion ALTER COLUMN Detalle nvarchar(max) NULL;
IF OBJECT_ID(N'ComedorPortal.CK_AuditoriaConfirmacion_Evento', N'C') IS NOT NULL
    ALTER TABLE ComedorPortal.AuditoriaConfirmacion DROP CONSTRAINT CK_AuditoriaConfirmacion_Evento;
ALTER TABLE ComedorPortal.AuditoriaConfirmacion WITH CHECK ADD CONSTRAINT CK_AuditoriaConfirmacion_Evento
    CHECK(Evento IN(N'Confirmacion',N'Cancelacion',N'Correccion',N'PinAsignado',N'PinCambiado',N'ParametrosPortal'));

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
