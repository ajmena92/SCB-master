/* Traslada sin duplicar el menú histórico al almacenamiento canónico. */
IF OBJECT_ID(N'ComedorPortal.MenuPlantilla', N'U') IS NOT NULL
BEGIN
    INSERT INTO menu.plantilla
        (semana, dia, titulo, observaciones, activo, creado_por, actualizado_por)
    SELECT SemanaMes, DiaSemana, Titulo, Observaciones, Activo,
           COALESCE(IdUsuarioModifica, 1), IdUsuarioModifica
    FROM ComedorPortal.MenuPlantilla AS hp
    WHERE NOT EXISTS (
        SELECT 1 FROM menu.plantilla AS cp
        WHERE cp.semana = hp.SemanaMes AND cp.dia = hp.DiaSemana
    );

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
END
