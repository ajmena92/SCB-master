/*
  Color visual por ruta para el carnet digital.
  dbo.Usuario.IdRuta y dbo.Ruta continúan siendo la fuente operativa local;
  esta tabla solo agrega configuración visual propiedad del portal.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'ComedorPortal.RutaCarnetConfiguracion', N'U') IS NULL
CREATE TABLE ComedorPortal.RutaCarnetConfiguracion (
    IdRuta int NOT NULL CONSTRAINT PK_RutaCarnetConfiguracion PRIMARY KEY,
    ColorCarnetHex char(7) NOT NULL CONSTRAINT DF_RutaCarnet_Color DEFAULT '#CBD5E1',
    FechaActualizacion datetime2 NOT NULL CONSTRAINT DF_RutaCarnet_Fecha DEFAULT SYSUTCDATETIME(),
    IdUsuarioActualizacion int NULL,
    CONSTRAINT FK_RutaCarnet_Ruta FOREIGN KEY(IdRuta) REFERENCES dbo.Ruta(IdRuta) ON DELETE CASCADE,
    CONSTRAINT FK_RutaCarnet_Usuario FOREIGN KEY(IdUsuarioActualizacion) REFERENCES Seguridad.Usuario(IdUsuario) ON DELETE SET NULL,
    CONSTRAINT CK_RutaCarnet_Color CHECK (ColorCarnetHex LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]')
);

;WITH Rutas AS (
    SELECT r.IdRuta, ROW_NUMBER() OVER (ORDER BY r.IdRuta) - 1 AS Posicion
    FROM dbo.Ruta r
    WHERE ISNULL(r.Activo, 1) = 1
)
INSERT INTO ComedorPortal.RutaCarnetConfiguracion (IdRuta,ColorCarnetHex)
SELECT IdRuta,
    '#CBD5E1'
FROM Rutas source
WHERE NOT EXISTS (SELECT 1 FROM ComedorPortal.RutaCarnetConfiguracion target WHERE target.IdRuta=source.IdRuta);

IF EXISTS (
    SELECT 1 FROM sys.check_constraints
    WHERE parent_object_id=OBJECT_ID(N'ComedorPortal.AuditoriaConfirmacion')
      AND name=N'CK_AuditoriaConfirmacion_Evento'
)
    ALTER TABLE ComedorPortal.AuditoriaConfirmacion DROP CONSTRAINT CK_AuditoriaConfirmacion_Evento;

ALTER TABLE ComedorPortal.AuditoriaConfirmacion ADD CONSTRAINT CK_AuditoriaConfirmacion_Evento CHECK(Evento IN(
    N'Confirmacion',N'Cancelacion',N'Correccion',N'PinAsignado',N'PinCambiado',
    N'FotoCargada',N'FotoEliminada',N'BeneficioActualizado',N'CarnetGenerado',N'RutaActualizada',N'ParametrosPortal'
));

COMMIT TRANSACTION;
