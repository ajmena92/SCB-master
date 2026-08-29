/* Registra la modalidad real del consumo para separar becas y tiquetes. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'comedor') IS NULL EXEC(N'CREATE SCHEMA comedor');
IF OBJECT_ID(N'comedor.registro', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.registro (
        id_registro BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_registro PRIMARY KEY,
        id_estudiante INT NOT NULL,
        fecha DATE NOT NULL,
        registrado_por INT NOT NULL,
        creado_en DATETIME2 NOT NULL CONSTRAINT DF_comedor_registro_creado_en DEFAULT SYSUTCDATETIME(),
        modalidad VARCHAR(20) NOT NULL CONSTRAINT DF_comedor_registro_modalidad DEFAULT 'beca',
        CONSTRAINT UQ_comedor_registro_estudiante_fecha UNIQUE(id_estudiante,fecha),
        CONSTRAINT CK_comedor_registro_modalidad CHECK (modalidad IN ('beca','tiquete','otro'))
    );
END
ELSE IF COL_LENGTH(N'comedor.registro', N'modalidad') IS NULL
BEGIN
    ALTER TABLE comedor.registro ADD modalidad varchar(20) NOT NULL
        CONSTRAINT DF_comedor_registro_modalidad DEFAULT 'beca';
    ALTER TABLE comedor.registro ADD CONSTRAINT CK_comedor_registro_modalidad
        CHECK (modalidad IN ('beca','tiquete','otro'));
END;
COMMIT TRANSACTION;
