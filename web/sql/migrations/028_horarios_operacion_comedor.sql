/* Catálogo web de horario y hora límite exclusiva del comedor. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'comedor.horario_operacion', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.horario_operacion(
        id_horario INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_horario_operacion PRIMARY KEY,
        codigo VARCHAR(20) NOT NULL CONSTRAINT UQ_comedor_horario_codigo UNIQUE,
        descripcion NVARCHAR(100) NOT NULL,
        hora_limite TIME NOT NULL,
        origen VARCHAR(30) NOT NULL CONSTRAINT DF_comedor_horario_origen DEFAULT 'configuracion_web',
        hora_limite_origen TIME NULL,
        id_horario_origen INT NULL,
        activo BIT NOT NULL CONSTRAINT DF_comedor_horario_activo DEFAULT 1,
        actualizado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_horario_actualizado DEFAULT SYSUTCDATETIME()
    );
END;
IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'HoraLimite') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'IdHorario') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Horario WHERE HoraLimite IS NULL)
        THROW 50060, 'Horario contiene registros sin HoraLimite; corte abortado', 1;
    IF (SELECT COUNT(*) FROM dbo.Horario) > 2
        THROW 50064, 'dbo.Horario contiene más de dos horarios; mapeo explícito requerido', 1;
    EXEC sys.sp_executesql N'
        INSERT comedor.horario_operacion(codigo,descripcion,hora_limite,origen,hora_limite_origen,id_horario_origen)
        SELECT CASE WHEN ROW_NUMBER() OVER (ORDER BY IdHorario)=1 THEN ''diurno'' ELSE ''nocturno'' END,
               CONCAT(N''Horario '',CONVERT(nvarchar(20),IdHorario)),HoraLimite,''dbo.Horario'',HoraLimite,IdHorario
        FROM dbo.Horario h
        WHERE NOT EXISTS (SELECT 1 FROM comedor.horario_operacion o WHERE o.id_horario_origen=h.IdHorario);';
    END;
IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'HoraLimite') IS NULL
    THROW 50062, 'dbo.Horario no contiene HoraLimite; corte abortado', 1;
IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'IdHorario') IS NULL
    THROW 50063, 'dbo.Horario no contiene IdHorario; corte abortado', 1;
IF NOT EXISTS (SELECT 1 FROM comedor.horario_operacion WHERE codigo='diurno')
    INSERT comedor.horario_operacion(codigo,descripcion,hora_limite,origen) VALUES('diurno',N'Diurno','12:00','valor_predeterminado');
IF NOT EXISTS (SELECT 1 FROM comedor.horario_operacion WHERE codigo='nocturno')
    INSERT comedor.horario_operacion(codigo,descripcion,hora_limite,origen) VALUES('nocturno',N'Nocturno','20:00','valor_predeterminado');
COMMIT TRANSACTION;
