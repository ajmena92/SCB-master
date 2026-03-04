IF OBJECT_ID(N'dbo.OperacionTransporteEvento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OperacionTransporteEvento
    (
        IdOperacionTransporteEvento BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_OperacionTransporteEvento_FechaEvento DEFAULT (SYSUTCDATETIME()),
        Cedula NVARCHAR(50) NULL,
        Estado NVARCHAR(40) NOT NULL,
        Motivo NVARCHAR(300) NULL,
        TiempoAtencionMs INT NULL,
        EsDuplicado BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_EsDuplicado DEFAULT (0),
        TieneAdvertencia BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_TieneAdvertencia DEFAULT (0),
        TieneError BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_TieneError DEFAULT (0),
        EsIncidenciaManual BIT NOT NULL CONSTRAINT DF_OperacionTransporteEvento_EsIncidenciaManual DEFAULT (0)
    );

    CREATE INDEX IX_OperacionTransporteEvento_FechaEvento
        ON dbo.OperacionTransporteEvento (FechaEvento DESC);

    CREATE INDEX IX_OperacionTransporteEvento_Estado
        ON dbo.OperacionTransporteEvento (Estado, FechaEvento DESC);
END;

