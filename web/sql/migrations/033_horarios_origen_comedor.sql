/* Mapeo determinista de todos los horarios soportados desde dbo.Horario. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH(N'comedor.horario_operacion', N'id_horario_origen') IS NULL
    ALTER TABLE comedor.horario_operacion ADD id_horario_origen INT NULL;
IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.Horario', N'HoraLimite') IS NOT NULL
   AND COL_LENGTH(N'dbo.Horario', N'IdHorario') IS NOT NULL
   AND COL_LENGTH(N'dbo.Horario', N'Descripcion') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Horario WHERE HoraLimite IS NULL)
        THROW 50061, 'Horario contiene registros sin HoraLimite; trazabilidad abortada', 1;
    IF (SELECT COUNT(*) FROM dbo.Horario) > 2
        THROW 50064, 'dbo.Horario contiene más de dos horarios; mapeo explícito requerido', 1;
    EXEC sys.sp_executesql N'
        IF EXISTS (SELECT 1 FROM dbo.Horario
                   WHERE UPPER(LTRIM(RTRIM(Descripcion))) NOT LIKE N''%DIURN%''
                     AND UPPER(LTRIM(RTRIM(Descripcion))) NOT LIKE N''%NOCTURN%'')
            THROW 50071, ''Horario contiene una descripción no soportada'', 1;
        ;WITH origen AS (
            SELECT IdHorario,HoraLimite,
                   CASE WHEN UPPER(LTRIM(RTRIM(Descripcion))) LIKE N''%NOCTURN%'' THEN ''nocturno'' ELSE ''diurno'' END codigo
            FROM dbo.Horario
        )
        UPDATE destino SET destino.id_horario_origen=origen.IdHorario,
               destino.hora_limite_origen=origen.HoraLimite,destino.origen=''dbo.Horario''
        FROM comedor.horario_operacion destino
        INNER JOIN origen ON origen.codigo=destino.codigo;';
END;
IF EXISTS (SELECT 1 FROM comedor.horario_operacion WHERE origen='dbo.Horario' AND id_horario_origen IS NULL)
    THROW 50065, 'Horario operativo sin IdHorario de origen; corte abortado', 1;
COMMIT TRANSACTION;
