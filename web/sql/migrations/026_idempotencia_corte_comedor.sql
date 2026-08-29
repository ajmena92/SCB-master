/* Agrega la huella de idempotencia sin alterar movimientos históricos. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF OBJECT_ID(N'comedor.movimiento_tiquetes', N'U') IS NULL
    THROW 50028, 'No existe comedor.movimiento_tiquetes para endurecer idempotencia', 1;
IF COL_LENGTH(N'comedor.movimiento_tiquetes', N'huella_idempotencia') IS NULL
    ALTER TABLE comedor.movimiento_tiquetes ADD huella_idempotencia VARBINARY(32) NULL;
UPDATE m
SET huella_idempotencia = HASHBYTES(
    'SHA2_256',
    CONCAT(m.id_cuenta, '|', m.tipo, '|', m.cantidad, '|', ISNULL(m.concepto, ''), '|', ISNULL(m.creado_por, 0))
)
FROM comedor.movimiento_tiquetes m
WHERE m.huella_idempotencia IS NULL AND m.tipo='recarga';
COMMIT TRANSACTION;
