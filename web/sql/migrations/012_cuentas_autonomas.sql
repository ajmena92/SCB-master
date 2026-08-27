/* Esquema canónico independiente para cuentas. No importa datos locales. */
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'cuentas') IS NULL EXEC(N'CREATE SCHEMA cuentas');
IF OBJECT_ID(N'cuentas.cuenta_saldo', N'U') IS NULL
BEGIN
    CREATE TABLE cuentas.cuenta_saldo (
        id_cuenta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_cuentas_cuenta PRIMARY KEY,
        id_estudiante int NOT NULL CONSTRAINT UQ_cuentas_estudiante UNIQUE,
        saldo decimal(12,2) NOT NULL CONSTRAINT DF_cuentas_saldo DEFAULT 0,
        actualizado_en datetime2(3) NOT NULL CONSTRAINT DF_cuentas_actualizado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_cuentas_saldo_no_negativo CHECK (saldo >= 0)
    );
END;
IF OBJECT_ID(N'cuentas.movimiento', N'U') IS NULL
BEGIN
    CREATE TABLE cuentas.movimiento (
        id_movimiento bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_cuentas_movimiento PRIMARY KEY,
        id_cuenta int NOT NULL,
        id_estudiante int NOT NULL,
        tipo varchar(12) NOT NULL,
        monto decimal(12,2) NOT NULL,
        saldo_anterior decimal(12,2) NOT NULL,
        saldo_nuevo decimal(12,2) NOT NULL,
        clave_idempotencia varchar(100) NOT NULL,
        concepto nvarchar(250) NULL,
        creado_por int NULL,
        direccion_ip varchar(64) NULL,
        creado_en datetime2(3) NOT NULL CONSTRAINT DF_cuentas_mov_creado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_cuentas_mov_cuenta FOREIGN KEY (id_cuenta) REFERENCES cuentas.cuenta_saldo(id_cuenta),
        CONSTRAINT CK_cuentas_mov_tipo CHECK (tipo IN ('recarga','consumo','ajuste')),
        CONSTRAINT CK_cuentas_mov_monto CHECK (monto > 0),
        CONSTRAINT CK_cuentas_mov_saldo CHECK (saldo_anterior >= 0 AND saldo_nuevo >= 0),
        CONSTRAINT UQ_cuentas_mov_idempotencia UNIQUE (id_estudiante, clave_idempotencia)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'cuentas.movimiento') AND name=N'IX_cuentas_mov_cuenta_fecha')
    CREATE INDEX IX_cuentas_mov_cuenta_fecha ON cuentas.movimiento(id_cuenta, creado_en DESC);
COMMIT TRANSACTION;
