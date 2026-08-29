/* Normaliza el beneficio de comedor a catálogo TINYINT. Ejecutar con respaldo. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'comedor.estado_comedor',N'U') IS NULL
BEGIN
    CREATE TABLE comedor.estado_comedor(
        id_estado_comedor TINYINT NOT NULL CONSTRAINT PK_comedor_estado_comedor PRIMARY KEY,
        codigo VARCHAR(30) NOT NULL CONSTRAINT UQ_comedor_estado_codigo UNIQUE,
        descripcion NVARCHAR(80) NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_comedor_estado_activo DEFAULT 1
    );
END;
MERGE comedor.estado_comedor AS destino
USING (VALUES (CONVERT(tinyint,1),'becado_comedor',N'Beneficiario'),(CONVERT(tinyint,2),'no_becado_comedor',N'No beneficiario')) AS origen(id,codigo,descripcion)
ON destino.id_estado_comedor=origen.id
WHEN MATCHED THEN UPDATE SET codigo=origen.codigo,descripcion=origen.descripcion,activo=1
WHEN NOT MATCHED THEN INSERT(id_estado_comedor,codigo,descripcion,activo) VALUES(origen.id,origen.codigo,origen.descripcion,1);
IF COL_LENGTH(N'comedor.persona',N'id_estado_comedor') IS NULL ALTER TABLE comedor.persona ADD id_estado_comedor TINYINT NULL;
IF COL_LENGTH(N'comedor.persona',N'estado_comedor') IS NOT NULL
    UPDATE comedor.persona SET id_estado_comedor=CASE estado_comedor WHEN 'becado_comedor' THEN 1 WHEN 'no_becado_comedor' THEN 2 END WHERE id_estado_comedor IS NULL;
IF EXISTS(SELECT 1 FROM comedor.persona WHERE id_estado_comedor IS NULL) THROW 50068,'Existen personas sin estado de comedor convertible',1;
IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_comedor_persona_estado_comedor') ALTER TABLE comedor.persona ADD CONSTRAINT FK_comedor_persona_estado_comedor FOREIGN KEY(id_estado_comedor) REFERENCES comedor.estado_comedor(id_estado_comedor);
ALTER TABLE comedor.persona ALTER COLUMN id_estado_comedor TINYINT NOT NULL;
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name=N'IX_comedor_persona_estado_comedor') CREATE INDEX IX_comedor_persona_estado_comedor ON comedor.persona(id_estado_comedor);
IF COL_LENGTH(N'comedor.persona',N'estado_comedor') IS NOT NULL
BEGIN
    IF EXISTS(SELECT 1 FROM sys.check_constraints WHERE name=N'CK_comedor_persona_estado') ALTER TABLE comedor.persona DROP CONSTRAINT CK_comedor_persona_estado;
    ALTER TABLE comedor.persona DROP COLUMN estado_comedor;
END;
COMMIT;
