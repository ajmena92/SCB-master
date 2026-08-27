/*
  AUTHORIZED SCOPE: route catalog descriptions and portal route administration.

  Run manually by the DBA, first in staging and after a verified backup.
  The API never executes this migration. It updates the existing dbo.Ruta
  description column and leaves student assignments untouched.
*/
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Rutas TABLE (Codigo nvarchar(50) NOT NULL PRIMARY KEY, Descripcion nvarchar(500) NOT NULL);
INSERT INTO @Rutas (Codigo, Descripcion) VALUES
    (N'1115306', N'Aguas Buenas - La Suiza - San Juan Bosco'),
    (N'1115307', N'Bolivia'),
    (N'1115308', N'La Sierra - Las Bonitas - San Gerardo - Villa Argentina'),
    (N'1115309', N'Los Reyes - El Pilar'),
    (N'1115311', N'San Carlos - San Pablo - Mollejones'),
    (N'1115336', N'Concepción - Oratorio - El Socorro'),
    (N'5369', N'Barrio Los Ángeles - Mollejoncitos - Buenos Aires'),
    (N'5370', N'Vista de Mar - Las Bonitas'),
    (N'5371', N'Cristo Rey - Mollejones');

IF EXISTS (
    SELECT 1 FROM @Rutas source
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Ruta target WHERE CONVERT(nvarchar(50), target.Codigo)=source.Codigo)
)
    THROW 51006, 'No se encontraron todos los códigos de ruta esperados en dbo.Ruta.', 1;

UPDATE target
SET target.Descripcion=source.Descripcion
FROM dbo.Ruta target
JOIN @Rutas source ON CONVERT(nvarchar(50), target.Codigo)=source.Codigo;

/* Official fixed route identity palette. Colors are accents, not status values. */
IF OBJECT_ID(N'ComedorPortal.RutaCarnetConfiguracion', N'U') IS NOT NULL
BEGIN
    DECLARE @Colores TABLE (Codigo nvarchar(50) NOT NULL PRIMARY KEY, Color char(7) NOT NULL);
    INSERT INTO @Colores (Codigo, Color) VALUES
        (N'5369', '#EF4444'),
        (N'5370', '#F472B6'),
        (N'5371', '#D946EF'),
        (N'1115306', '#F59E0B'),
        (N'1115307', '#FACC15'),
        (N'1115308', '#38BDF8'),
        (N'1115309', '#FB8C6A'),
        (N'1115311', '#A78BFA'),
        (N'1115336', '#4ADE80');

    UPDATE config
    SET config.ColorCarnetHex=colors.Color, config.FechaActualizacion=SYSUTCDATETIME()
    FROM ComedorPortal.RutaCarnetConfiguracion config
    JOIN dbo.Ruta route ON route.IdRuta=config.IdRuta
    JOIN @Colores colors ON CONVERT(nvarchar(50), route.Codigo)=colors.Codigo;

    INSERT INTO ComedorPortal.RutaCarnetConfiguracion (IdRuta,ColorCarnetHex)
    SELECT route.IdRuta,colors.Color
    FROM dbo.Ruta route
    JOIN @Colores colors ON CONVERT(nvarchar(50), route.Codigo)=colors.Codigo
    WHERE NOT EXISTS (
        SELECT 1 FROM ComedorPortal.RutaCarnetConfiguracion config WHERE config.IdRuta=route.IdRuta
    );
END;

IF OBJECT_ID(N'ComedorPortal.AuditoriaConfirmacion', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE parent_object_id=OBJECT_ID(N'ComedorPortal.AuditoriaConfirmacion')
          AND name=N'CK_AuditoriaConfirmacion_Evento'
    )
        ALTER TABLE ComedorPortal.AuditoriaConfirmacion DROP CONSTRAINT CK_AuditoriaConfirmacion_Evento;

    ALTER TABLE ComedorPortal.AuditoriaConfirmacion ADD CONSTRAINT CK_AuditoriaConfirmacion_Evento CHECK(Evento IN(
        N'Confirmacion',N'Cancelacion',N'Correccion',N'PinAsignado',N'PinCambiado',
        N'FotoCargada',N'FotoEliminada',N'BeneficioActualizado',N'CarnetGenerado',
        N'RutaActualizada',N'RutaCreada',N'RutaDesactivada',N'ParametrosPortal'
    ));
END;

COMMIT TRANSACTION;
