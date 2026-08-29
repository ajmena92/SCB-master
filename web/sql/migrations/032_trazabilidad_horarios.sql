IF COL_LENGTH(N'comedor.horario_operacion', N'origen') IS NULL
    ALTER TABLE comedor.horario_operacion ADD origen VARCHAR(30) NOT NULL CONSTRAINT DF_comedor_horario_origen DEFAULT 'migracion_0028';
IF COL_LENGTH(N'comedor.horario_operacion', N'hora_limite_origen') IS NULL
    ALTER TABLE comedor.horario_operacion ADD hora_limite_origen TIME NULL;
IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'HoraLimite') IS NOT NULL AND COL_LENGTH(N'dbo.Horario', N'IdHorario') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM dbo.Horario WHERE HoraLimite IS NULL) THROW 50061, 'Horario contiene registros sin HoraLimite; trazabilidad abortada', 1;
    UPDATE o SET hora_limite_origen=h.HoraLimite, origen='dbo.Horario'
    FROM comedor.horario_operacion o
    CROSS APPLY (SELECT TOP (1) HoraLimite FROM dbo.Horario ORDER BY IdHorario) h
    WHERE o.hora_limite_origen IS NULL;
END;
