/* Lectura diaria de transporte y trazabilidad del ingreso al comedor. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'transporte.uso_diario', N'U') IS NULL
BEGIN
    CREATE TABLE transporte.uso_diario(
        id_uso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_transporte_uso_diario PRIMARY KEY,
        id_estudiante INT NOT NULL,
        fecha DATE NOT NULL,
        marcado_en DATETIME2(3) NOT NULL CONSTRAINT DF_transporte_uso_marcado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_transporte_uso_estudiante_fecha UNIQUE(id_estudiante,fecha),
        CONSTRAINT FK_transporte_uso_estudiante FOREIGN KEY(id_estudiante) REFERENCES estudiantes.estudiante(id_estudiante)
    );
END;
IF COL_LENGTH(N'comedor.ingreso', N'codigo_horario') IS NULL
    ALTER TABLE comedor.ingreso ADD codigo_horario VARCHAR(20) NULL;
IF COL_LENGTH(N'comedor.ingreso', N'hora_marca') IS NULL
    ALTER TABLE comedor.ingreso ADD hora_marca DATETIME2(3) NULL;
IF COL_LENGTH(N'comedor.ingreso', N'marca_transporte_existente') IS NULL
    ALTER TABLE comedor.ingreso ADD marca_transporte_existente BIT NOT NULL CONSTRAINT DF_comedor_ingreso_marca_transporte DEFAULT 0;
IF OBJECT_ID(N'dbo.RegistroTransporte', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.RegistroTransporte', N'IdUsuario') IS NOT NULL
   AND COL_LENGTH(N'dbo.RegistroTransporte', N'Fecha') IS NOT NULL
BEGIN
    EXEC sys.sp_executesql N'
        INSERT INTO transporte.uso_diario(id_estudiante,fecha,marcado_en)
        SELECT rt.IdUsuario,CONVERT(date,rt.Fecha),MIN(rt.Fecha)
        FROM dbo.RegistroTransporte rt
        INNER JOIN estudiantes.estudiante e ON e.id_estudiante=rt.IdUsuario
        WHERE NOT EXISTS(
            SELECT 1 FROM transporte.uso_diario u
            WHERE u.id_estudiante=rt.IdUsuario AND u.fecha=CONVERT(date,rt.Fecha)
        )
        GROUP BY rt.IdUsuario,CONVERT(date,rt.Fecha)';
END;
COMMIT TRANSACTION;
