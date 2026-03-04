/*
    Archivo: 20260304_Parametro_PermitirSinMarcaTransporte_Y_EventosES.sql
    Objetivo:
      1) Agregar PermitirSinMarcaTransporte en tabla Parametro.
      2) Migrar Estado/Motivo de OperacionComedorEvento a codigos en español.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF COL_LENGTH('dbo.Parametro', 'PermitirSinMarcaTransporte') IS NULL
    BEGIN
        ALTER TABLE dbo.Parametro
            ADD PermitirSinMarcaTransporte BIT NULL;
    END

    EXEC(N'
        UPDATE dbo.Parametro
           SET PermitirSinMarcaTransporte = 1
         WHERE PermitirSinMarcaTransporte IS NULL;
    ');

    IF EXISTS (
        SELECT 1
        FROM sys.columns c
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        WHERE t.name = 'Parametro'
          AND c.name = 'PermitirSinMarcaTransporte'
          AND c.is_nullable = 1
    )
    BEGIN
        EXEC(N'
            ALTER TABLE dbo.Parametro
                ALTER COLUMN PermitirSinMarcaTransporte BIT NOT NULL;
        ');
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        WHERE t.name = 'Parametro' AND c.name = 'PermitirSinMarcaTransporte'
    )
    BEGIN
        ALTER TABLE dbo.Parametro
            ADD CONSTRAINT DF_Parametro_PermitirSinMarcaTransporte DEFAULT(1) FOR PermitirSinMarcaTransporte;
    END

    IF OBJECT_ID('dbo.OperacionComedorEvento', 'U') IS NOT NULL
    BEGIN
        UPDATE dbo.OperacionComedorEvento
           SET Estado = CASE UPPER(LTRIM(RTRIM(ISNULL(Estado,''))))
                WHEN 'SUCCESS' THEN 'EXITO'
                WHEN 'PROCESSING' THEN 'PROCESANDO'
                WHEN 'DUPLICATE' THEN 'DUPLICADO'
                WHEN 'NOTICKETS' THEN 'SIN_TIQUETES'
                WHEN 'NOTRANSPORTMARK' THEN 'SIN_MARCA_TRANSPORTE'
                WHEN 'LATETRANSPORTMARK' THEN 'MARCA_TARDIA_TRANSPORTE'
                WHEN 'NOTFOUND' THEN 'CARNET_NO_ENCONTRADO'
                WHEN 'DENIEDBYRULE' THEN 'DENEGADO_POR_REGLA'
                WHEN 'IDLE' THEN 'EN_ESPERA'
                WHEN 'EXITO' THEN 'EXITO'
                WHEN 'PROCESANDO' THEN 'PROCESANDO'
                WHEN 'DUPLICADO' THEN 'DUPLICADO'
                WHEN 'SIN_TIQUETES' THEN 'SIN_TIQUETES'
                WHEN 'SIN_MARCA_TRANSPORTE' THEN 'SIN_MARCA_TRANSPORTE'
                WHEN 'MARCA_TARDIA_TRANSPORTE' THEN 'MARCA_TARDIA_TRANSPORTE'
                WHEN 'CARNET_NO_ENCONTRADO' THEN 'CARNET_NO_ENCONTRADO'
                WHEN 'DENEGADO_POR_REGLA' THEN 'DENEGADO_POR_REGLA'
                WHEN 'EN_ESPERA' THEN 'EN_ESPERA'
                ELSE 'EN_ESPERA'
           END;

        UPDATE dbo.OperacionComedorEvento
           SET Motivo = CASE
                WHEN Motivo IS NULL OR LTRIM(RTRIM(Motivo)) = '' THEN NULL
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'MEAL_REGISTERED' THEN 'COMIDA_REGISTRADA'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'PROCESSING_SCAN' THEN 'PROCESANDO_LECTURA'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'DUPLICATE_SCAN' THEN 'LECTURA_DUPLICADA'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'NO_TICKETS' THEN 'SIN_TIQUETES'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'NO_TRANSPORT_MARK' THEN 'SIN_MARCA_TRANSPORTE'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) IN ('TRANSPORT_LATE_MARK','TRANSPORT_LATE') THEN 'MARCA_TARDIA_TRANSPORTE'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'CARD_NOT_FOUND' THEN 'CARNET_NO_ENCONTRADO'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'RULE_DENIED' THEN 'DENEGADO_POR_REGLA'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'MANUAL_INCIDENT' THEN 'INCIDENCIA_MANUAL'
                WHEN UPPER(LTRIM(RTRIM(Motivo))) LIKE 'INCIDENT:%' THEN REPLACE(UPPER(LTRIM(RTRIM(Motivo))), 'INCIDENT: ', 'INCIDENCIA_')
                WHEN UPPER(LTRIM(RTRIM(Motivo))) LIKE 'INCIDENCIA:%' THEN REPLACE(UPPER(LTRIM(RTRIM(Motivo))), 'INCIDENCIA: ', 'INCIDENCIA_')
                WHEN UPPER(LTRIM(RTRIM(Motivo))) = 'ACCION: VALIDE EL CARNET Y VUELVA A ESCANEAR.' THEN 'CARNET_NO_ENCONTRADO'
                ELSE UPPER(REPLACE(LTRIM(RTRIM(Motivo)), ' ', '_'))
           END;
    END

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;

    DECLARE @ErrorNumero INT = ERROR_NUMBER();
    DECLARE @ErrorLinea INT = ERROR_LINE();
    DECLARE @ErrorMensaje NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Error en migración Parametro/EventosES. Numero: %d, Línea: %d, Mensaje: %s',
              16, 1, @ErrorNumero, @ErrorLinea, @ErrorMensaje);
END CATCH;
