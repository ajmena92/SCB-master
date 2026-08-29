/* Corte canónico del comedor: personas, estado, tiquetes, reservas e ingresos.
   Ejecutar con respaldo y escrituras congeladas. Repetible. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'comedor') IS NULL EXEC(N'CREATE SCHEMA comedor');

IF OBJECT_ID(N'comedor.persona', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.persona (
        id_persona INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_persona PRIMARY KEY,
        tipo_persona VARCHAR(20) NOT NULL,
        id_estudiante INT NULL,
        id_usuario INT NULL,
        codigo_barras VARCHAR(80) NOT NULL,
        nombre_completo NVARCHAR(220) NOT NULL,
        colegio NVARCHAR(200) NULL,
        estado_comedor VARCHAR(20) NOT NULL,
        activo BIT NOT NULL CONSTRAINT DF_comedor_persona_activo DEFAULT 1,
        creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_persona_creado DEFAULT SYSUTCDATETIME(),
        actualizado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_persona_actualizado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_comedor_persona_tipo CHECK (tipo_persona IN ('estudiante','profesor')),
        CONSTRAINT CK_comedor_persona_estado CHECK (estado_comedor IN ('becado_comedor','no_becado_comedor')),
        CONSTRAINT CK_comedor_persona_vinculo CHECK (
            (tipo_persona='estudiante' AND id_estudiante IS NOT NULL AND id_usuario IS NULL)
            OR (tipo_persona='profesor' AND id_estudiante IS NULL AND id_usuario IS NOT NULL)
        ),
        CONSTRAINT FK_comedor_persona_estudiante FOREIGN KEY(id_estudiante)
            REFERENCES estudiantes.estudiante(id_estudiante),
        CONSTRAINT FK_comedor_persona_usuario FOREIGN KEY(id_usuario)
            REFERENCES identidad.usuario(id_usuario)
    );
END;
IF EXISTS (SELECT codigo_barras FROM comedor.persona GROUP BY codigo_barras HAVING COUNT(*) > 1)
    THROW 50034, 'Corte comedor abortado antes de restricciones: existen carnets duplicados', 1;
IF OBJECT_ID(N'estudiantes.estudiante', N'U') IS NOT NULL
   AND EXISTS (
        SELECT e.carne FROM estudiantes.estudiante e
        WHERE e.carne IS NOT NULL AND NULLIF(LTRIM(RTRIM(e.carne)), N'') IS NOT NULL
        GROUP BY e.carne HAVING COUNT(*) > 1
   )
    THROW 50034, 'Corte comedor abortado antes de restricciones: estudiantes con carnet duplicado', 1;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'comedor.persona') AND name=N'UQ_comedor_persona_barcode')
    CREATE UNIQUE INDEX UQ_comedor_persona_barcode ON comedor.persona(codigo_barras);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'comedor.persona') AND name=N'UQ_comedor_persona_estudiante')
    CREATE UNIQUE INDEX UQ_comedor_persona_estudiante ON comedor.persona(id_estudiante) WHERE id_estudiante IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'comedor.persona') AND name=N'UQ_comedor_persona_usuario')
    CREATE UNIQUE INDEX UQ_comedor_persona_usuario ON comedor.persona(id_usuario) WHERE id_usuario IS NOT NULL;

IF EXISTS (
    SELECT e.carne FROM estudiantes.estudiante e
    WHERE e.carne IS NOT NULL AND NULLIF(LTRIM(RTRIM(e.carne)), N'') IS NOT NULL
    GROUP BY e.carne HAVING COUNT(*) > 1
)
    THROW 50034, 'Corte comedor abortado antes de restricciones: estudiantes con carnet duplicado', 1;
INSERT INTO comedor.persona(tipo_persona,id_estudiante,codigo_barras,nombre_completo,estado_comedor,activo)
SELECT 'estudiante',e.id_estudiante,CONCAT('E-',e.carne),
       CONCAT(e.nombre,N' ',e.primer_apellido,N' ',ISNULL(e.segundo_apellido,N'')),
       CASE WHEN EXISTS (
           SELECT 1 FROM beneficios.asignacion ba
                   WHERE ba.id_estudiante=e.id_estudiante AND ba.id_beneficio IS NOT NULL
       ) THEN 'becado_comedor' ELSE 'no_becado_comedor' END,
       e.activo
FROM estudiantes.estudiante e
WHERE NOT EXISTS (SELECT 1 FROM comedor.persona p WHERE p.id_estudiante=e.id_estudiante);

IF OBJECT_ID(N'comedor.cuenta_tiquetes', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.cuenta_tiquetes (
        id_cuenta INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_cuenta_tiquetes PRIMARY KEY,
        id_persona INT NOT NULL CONSTRAINT UQ_comedor_cuenta_persona UNIQUE,
        saldo INT NOT NULL CONSTRAINT DF_comedor_cuenta_saldo DEFAULT 0,
        reservados INT NOT NULL CONSTRAINT DF_comedor_cuenta_reservados DEFAULT 0,
        actualizado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_cuenta_actualizado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_comedor_cuenta_saldo CHECK (saldo >= 0),
        CONSTRAINT CK_comedor_cuenta_reservados CHECK (reservados >= 0 AND reservados <= saldo),
        CONSTRAINT FK_comedor_cuenta_persona FOREIGN KEY(id_persona) REFERENCES comedor.persona(id_persona)
    );
END;
INSERT INTO comedor.cuenta_tiquetes(id_persona)
SELECT p.id_persona FROM comedor.persona p
WHERE NOT EXISTS (SELECT 1 FROM comedor.cuenta_tiquetes c WHERE c.id_persona=p.id_persona);

IF OBJECT_ID(N'comedor.movimiento_tiquetes', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.movimiento_tiquetes (
        id_movimiento BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_movimiento_tiquetes PRIMARY KEY,
        id_cuenta INT NOT NULL,
        tipo VARCHAR(12) NOT NULL,
        cantidad INT NOT NULL,
        saldo_anterior INT NOT NULL,
        saldo_nuevo INT NOT NULL,
        reservados_anterior INT NOT NULL,
        reservados_nuevo INT NOT NULL,
        clave_idempotencia VARCHAR(100) NOT NULL,
        huella_idempotencia VARBINARY(32) NULL,
        concepto NVARCHAR(250) NULL,
        creado_por INT NULL,
        creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_movimiento_creado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_comedor_movimiento_tipo CHECK (tipo IN ('recarga','consumo','reserva','liberacion','ajuste')),
        CONSTRAINT CK_comedor_movimiento_cantidad CHECK (cantidad > 0),
        CONSTRAINT UQ_comedor_movimiento_clave UNIQUE(clave_idempotencia),
        CONSTRAINT FK_comedor_movimiento_cuenta FOREIGN KEY(id_cuenta) REFERENCES comedor.cuenta_tiquetes(id_cuenta)
    );
END;
IF COL_LENGTH(N'comedor.movimiento_tiquetes', N'huella_idempotencia') IS NULL
    ALTER TABLE comedor.movimiento_tiquetes ADD huella_idempotencia VARBINARY(32) NULL;

IF OBJECT_ID(N'comedor.reserva', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.reserva (
        id_reserva BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_reserva PRIMARY KEY,
        id_persona INT NOT NULL,
        fecha DATE NOT NULL,
        estado VARCHAR(12) NOT NULL,
        requiere_tiquete BIT NOT NULL,
        modalidad VARCHAR(12) NOT NULL,
        registrada_por INT NULL,
        creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_reserva_creado DEFAULT SYSUTCDATETIME(),
        actualizada_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_reserva_actualizada DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_comedor_reserva_persona_fecha UNIQUE(id_persona,fecha),
        CONSTRAINT CK_comedor_reserva_estado CHECK (estado IN ('reservada','cancelada','consumida')),
        CONSTRAINT CK_comedor_reserva_modalidad CHECK (modalidad IN ('beca','tiquete')),
        CONSTRAINT FK_comedor_reserva_persona FOREIGN KEY(id_persona) REFERENCES comedor.persona(id_persona)
    );
END;
IF OBJECT_ID(N'comedor.ingreso', N'U') IS NULL
BEGIN
    CREATE TABLE comedor.ingreso (
        id_ingreso BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_ingreso PRIMARY KEY,
        id_persona INT NOT NULL,
        fecha DATE NOT NULL,
        modalidad VARCHAR(12) NOT NULL,
        registrado_por INT NULL,
        creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_ingreso_creado DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_comedor_ingreso_persona_fecha UNIQUE(id_persona,fecha),
        CONSTRAINT CK_comedor_ingreso_modalidad CHECK (modalidad IN ('beca','tiquete')),
        CONSTRAINT FK_comedor_ingreso_persona FOREIGN KEY(id_persona) REFERENCES comedor.persona(id_persona)
    );
END;

IF OBJECT_ID(N'comedor.registro', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'comedor.registro_migracion_0024', N'U') IS NULL
    BEGIN
        CREATE TABLE comedor.registro_migracion_0024 (
            id_registro BIGINT NOT NULL CONSTRAINT PK_comedor_registro_migracion_0024 PRIMARY KEY,
            id_estudiante INT NOT NULL,
            fecha DATE NOT NULL,
            registrado_por INT NOT NULL,
            creado_en DATETIME2 NOT NULL,
            modalidad VARCHAR(20) NOT NULL
        );
    END;
    INSERT INTO comedor.registro_migracion_0024
        (id_registro,id_estudiante,fecha,registrado_por,creado_en,modalidad)
    SELECT r.id_registro,r.id_estudiante,r.fecha,r.registrado_por,r.creado_en,r.modalidad
    FROM comedor.registro r
    WHERE NOT EXISTS (
        SELECT 1 FROM comedor.registro_migracion_0024 a WHERE a.id_registro=r.id_registro
    );
    IF EXISTS (
        SELECT 1 FROM comedor.registro r
        LEFT JOIN estudiantes.estudiante e ON e.id_estudiante=r.id_estudiante
        WHERE e.id_estudiante IS NULL
    ) THROW 50024, 'Corte comedor abortado: existen registros sin estudiante', 1;
    IF EXISTS (
        SELECT 1 FROM comedor.registro r
        LEFT JOIN comedor.persona p ON p.id_estudiante=r.id_estudiante
        WHERE p.id_persona IS NULL
    ) THROW 50031, 'Corte comedor abortado: existen registros sin persona de comedor', 1;
    IF EXISTS (SELECT 1 FROM comedor.registro WHERE modalidad NOT IN ('beca','tiquete'))
        THROW 50025, 'Corte comedor abortado: modalidad histórica no soportada', 1;
    IF EXISTS (
        SELECT 1 FROM comedor.registro r
        INNER JOIN comedor.persona p ON p.id_estudiante=r.id_estudiante
        INNER JOIN comedor.ingreso i ON i.id_persona=p.id_persona AND i.fecha=r.fecha
        WHERE i.modalidad <> r.modalidad
    ) THROW 50026, 'Corte comedor abortado: conflicto con ingresos existentes', 1;
    INSERT INTO comedor.ingreso(id_persona,fecha,modalidad,registrado_por,creado_en)
    SELECT p.id_persona,r.fecha,
           r.modalidad,
           r.registrado_por,r.creado_en
    FROM comedor.registro r
    INNER JOIN comedor.persona p ON p.id_estudiante=r.id_estudiante
    WHERE NOT EXISTS (
        SELECT 1 FROM comedor.ingreso i
        WHERE i.id_persona=p.id_persona AND i.fecha=r.fecha
    );
    IF EXISTS (
        SELECT 1 FROM comedor.registro r
        INNER JOIN comedor.persona p ON p.id_estudiante=r.id_estudiante
        LEFT JOIN comedor.ingreso i ON i.id_persona=p.id_persona AND i.fecha=r.fecha
        WHERE i.id_ingreso IS NULL
    ) THROW 50027, 'Corte comedor abortado: quedaron registros sin reconciliar', 1;
END;
COMMIT TRANSACTION;
