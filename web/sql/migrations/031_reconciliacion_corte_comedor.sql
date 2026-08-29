/* Tabla de diferencias. El script de corte inserta aquí ambigüedades sin convertirlas. */
IF OBJECT_ID(N'comedor.reconciliacion_migracion', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.reconciliacion_migracion(
        id_reconciliacion BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_reconciliacion PRIMARY KEY,
        tipo VARCHAR(50) NOT NULL, clave VARCHAR(200) NOT NULL,
        detalle NVARCHAR(1000) NOT NULL,
        origen VARCHAR(40) NOT NULL CONSTRAINT DF_comedor_reconciliacion_origen DEFAULT 'corte_web',
        resuelto BIT NOT NULL CONSTRAINT DF_comedor_reconciliacion_resuelto DEFAULT 0,
        creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_reconciliacion_creado DEFAULT SYSUTCDATETIME(),
        resuelto_en DATETIME2(3) NULL, resuelto_por INT NULL
    );
    CREATE UNIQUE INDEX UX_comedor_reconciliacion_clave ON comedor.reconciliacion_migracion(tipo, clave);
END;

/* Hallazgos deterministas del modelo web: no elimina ni transforma datos. */
INSERT comedor.reconciliacion_migracion(tipo, clave, detalle)
SELECT 'ruta_multiple', CONVERT(varchar(200), ar.id_estudiante), N'Estudiante con más de una asignación activa'
FROM transporte.asignacion_ruta ar
WHERE ar.activa=1
GROUP BY ar.id_estudiante HAVING COUNT(*) > 1
AND NOT EXISTS (SELECT 1 FROM comedor.reconciliacion_migracion r WHERE r.tipo='ruta_multiple' AND r.clave=CONVERT(varchar(200), ar.id_estudiante));

INSERT comedor.reconciliacion_migracion(tipo, clave, detalle)
SELECT 'saldo_negativo', CONVERT(varchar(200), id_cuenta), N'Cuenta con saldo negativo'
FROM comedor.cuenta_tiquetes WHERE saldo < 0
AND NOT EXISTS (SELECT 1 FROM comedor.reconciliacion_migracion r WHERE r.tipo='saldo_negativo' AND r.clave=CONVERT(varchar(200), id_cuenta));
