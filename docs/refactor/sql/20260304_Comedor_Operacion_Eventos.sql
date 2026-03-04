/*
    Archivo: 20260304_Comedor_Operacion_Eventos.sql
    Objetivo:
      1) Crear tabla de eventos operativos de comedor para analitica, alertas e incidencias.
      2) Crear indices para consultas por fecha y estado.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID(N'dbo.OperacionComedorEvento', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.OperacionComedorEvento
        (
            IdOperacionComedorEvento BIGINT IDENTITY(1,1) NOT NULL,
            FechaEvento DATETIME2(0) NOT NULL CONSTRAINT DF_OperacionComedorEvento_FechaEvento DEFAULT (SYSUTCDATETIME()),
            Cedula NVARCHAR(50) NULL,
            Estado NVARCHAR(40) NOT NULL,
            Motivo NVARCHAR(300) NULL,
            TiempoAtencionMs INT NULL,
            EsDuplicado BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_EsDuplicado DEFAULT (0),
            TieneAdvertencia BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_TieneAdvertencia DEFAULT (0),
            TieneError BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_TieneError DEFAULT (0),
            EsIncidenciaManual BIT NOT NULL CONSTRAINT DF_OperacionComedorEvento_EsIncidenciaManual DEFAULT (0),
            CONSTRAINT PK_OperacionComedorEvento PRIMARY KEY CLUSTERED (IdOperacionComedorEvento)
        );
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_OperacionComedorEvento_FechaEvento'
          AND object_id = OBJECT_ID(N'dbo.OperacionComedorEvento')
    )
    BEGIN
        CREATE INDEX IX_OperacionComedorEvento_FechaEvento
            ON dbo.OperacionComedorEvento (FechaEvento DESC);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'IX_OperacionComedorEvento_Estado'
          AND object_id = OBJECT_ID(N'dbo.OperacionComedorEvento')
    )
    BEGIN
        CREATE INDEX IX_OperacionComedorEvento_Estado
            ON dbo.OperacionComedorEvento (Estado, FechaEvento DESC);
    END

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;

    DECLARE @ErrorNumero INT = ERROR_NUMBER();
    DECLARE @ErrorLinea INT = ERROR_LINE();
    DECLARE @ErrorMensaje NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Error en migración OperacionComedorEvento. Numero: %d, Línea: %d, Mensaje: %s',
              16, 1, @ErrorNumero, @ErrorLinea, @ErrorMensaje);
END CATCH;

