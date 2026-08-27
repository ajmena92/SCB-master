SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH(N'estudiantes.estudiante', N'hash_contrasena') IS NULL
    ALTER TABLE estudiantes.estudiante ADD hash_contrasena nvarchar(255) NULL;
IF COL_LENGTH(N'estudiantes.estudiante', N'debe_cambiar_pin') IS NULL
    ALTER TABLE estudiantes.estudiante ADD debe_cambiar_pin bit NOT NULL CONSTRAINT DF_estudiantes_estudiante_cambiar_pin DEFAULT 0;
IF COL_LENGTH(N'estudiantes.estudiante', N'seccion') IS NULL
    ALTER TABLE estudiantes.estudiante ADD seccion nvarchar(30) NULL;
IF COL_LENGTH(N'estudiantes.estudiante', N'id_beneficio') IS NULL
    ALTER TABLE estudiantes.estudiante ADD id_beneficio int NULL;
IF COL_LENGTH(N'estudiantes.estudiante', N'id_ruta') IS NULL
    ALTER TABLE estudiantes.estudiante ADD id_ruta int NULL;
IF COL_LENGTH(N'estudiantes.estudiante', N'turno') IS NULL
    ALTER TABLE estudiantes.estudiante ADD turno nvarchar(30) NULL;
COMMIT TRANSACTION;
