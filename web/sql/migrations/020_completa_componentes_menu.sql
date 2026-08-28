/* Completa el traslado de componentes históricos si aún faltan. */
ALTER TABLE menu.componente ALTER COLUMN nombre nvarchar(500) NOT NULL;
IF OBJECT_ID(N'ComedorPortal.MenuComponente', N'U') IS NOT NULL
BEGIN
    INSERT INTO menu.componente (id_plantilla, nombre, tipo, orden)
    SELECT cp.id_plantilla, hc.Nombre, hc.TipoComponente, hc.Orden
    FROM ComedorPortal.MenuComponente AS hc
    INNER JOIN ComedorPortal.MenuPlantilla AS hp
        ON hp.IdMenuPlantilla = hc.IdMenuPlantilla
    INNER JOIN menu.plantilla AS cp
        ON cp.semana = hp.SemanaMes AND cp.dia = hp.DiaSemana
    WHERE NOT EXISTS (
        SELECT 1 FROM menu.componente AS cc
        WHERE cc.id_plantilla = cp.id_plantilla AND cc.orden = hc.Orden
    );
END
