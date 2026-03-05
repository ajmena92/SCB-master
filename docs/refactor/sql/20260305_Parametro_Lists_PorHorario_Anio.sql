/*
    Archivo: 20260305_Parametro_Lists_PorHorario_Anio.sql
    Objetivo:
      1) Agregar configuración de Microsoft Lists por horario y año lectivo en dbo.Parametro.
      2) Mantener compatibilidad con GraphListId global.

    Nuevas columnas:
      - GraphListMap NVARCHAR(MAX):
            Formato recomendado (texto plano):
                Anio|IdHorario|GraphListId
                2026|1|list-id-manana
                2026|2|list-id-tarde
                2027|1|list-id-manana-2027

            También acepta ';' como separador de filas:
                2026|1|listA;2026|2|listB

      - ListsAnioLectivo INT:
            Año lectivo activo usado para resolver GraphListMap.
*/

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    IF COL_LENGTH('dbo.Parametro', 'GraphListMap') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphListMap NVARCHAR(MAX) NULL;

    IF COL_LENGTH('dbo.Parametro', 'ListsAnioLectivo') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsAnioLectivo INT NULL;

    UPDATE dbo.Parametro
       SET ListsAnioLectivo = ISNULL(NULLIF(ListsAnioLectivo, 0), YEAR(GETDATE())),
           GraphListMap = ISNULL(GraphListMap, N'')
     WHERE Id IN (0, 1);

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    DECLARE @ErrNum INT = ERROR_NUMBER(),
            @ErrLine INT = ERROR_LINE(),
            @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Error en migración Parametro/Lists por horario-año. Numero: %d, Línea: %d, Mensaje: %s',
              16, 1, @ErrNum, @ErrLine, @ErrMsg);
END CATCH;
