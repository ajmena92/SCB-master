/* Estado compartido de bloqueo de autenticación para todos los workers. */
IF OBJECT_ID(N'identidad.intento_autenticacion', N'U') IS NULL
BEGIN
    CREATE TABLE identidad.intento_autenticacion (
        identificador_hash char(64) NOT NULL
            CONSTRAINT PK_identidad_intento_autenticacion PRIMARY KEY,
        intentos_fallidos int NOT NULL
            CONSTRAINT DF_identidad_intento_fallidos DEFAULT 0,
        bloqueado_hasta datetime2(3) NULL,
        fecha_actualizacion datetime2(3) NOT NULL
            CONSTRAINT DF_identidad_intento_actualizacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_identidad_intento_fallidos CHECK (intentos_fallidos BETWEEN 0 AND 20)
    );
END
