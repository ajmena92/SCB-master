/*
    Archivo: 20260304_Parametro_Lists_Y_Mantenimiento.sql
    Objetivo:
      1) Mover configuración de Microsoft Lists a dbo.Parametro (fila Id=1).
      2) Crear columnas necesarias para importación directa desde Microsoft Lists.
*/

SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    IF COL_LENGTH('dbo.Parametro', 'MicrosoftListsEnabled') IS NULL
        ALTER TABLE dbo.Parametro ADD MicrosoftListsEnabled BIT NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphTenantId') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphTenantId NVARCHAR(200) NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphClientId') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphClientId NVARCHAR(200) NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphClientSecretProtegido') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphClientSecretProtegido NVARCHAR(2000) NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphSiteId') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphSiteId NVARCHAR(400) NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphListId') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphListId NVARCHAR(400) NULL;
    IF COL_LENGTH('dbo.Parametro', 'GraphPageSize') IS NULL
        ALTER TABLE dbo.Parametro ADD GraphPageSize INT NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnCedula') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnCedula NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnPrimerApellido') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnPrimerApellido NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnSegundoApellido') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnSegundoApellido NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnNombre') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnNombre NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnSeccion') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnSeccion NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnEspecialidad') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnEspecialidad NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnFechaNac') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnFechaNac NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsColumnTelefono') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsColumnTelefono NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsFiltroActivo') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsFiltroActivo NVARCHAR(100) NULL;
    IF COL_LENGTH('dbo.Parametro', 'ListsValorActivo') IS NULL
        ALTER TABLE dbo.Parametro ADD ListsValorActivo NVARCHAR(100) NULL;

    IF NOT EXISTS (SELECT 1 FROM dbo.Parametro WHERE Id = 1)
    BEGIN
        INSERT INTO dbo.Parametro
            (Id, Institucion, CodPresupuestario, Ubicacion, Leyenda, ControlCarnet, PrecioDocente, PrecioEstudiante,
             PermitirSinMarcaTransporte, MicrosoftListsEnabled, GraphPageSize,
             ListsColumnCedula, ListsColumnPrimerApellido, ListsColumnSegundoApellido, ListsColumnNombre,
             ListsColumnSeccion, ListsColumnEspecialidad, ListsColumnFechaNac, ListsColumnTelefono, ListsValorActivo)
        VALUES
            (1, N'', N'', N'', N'', N'', 0, 0, 0, 0, 200,
             N'Cedula', N'PrimerApellido', N'SegundoApellido', N'Nombre',
             N'Seccion', N'Especialidad', N'FechaNac', N'Telefono', N'true');
    END

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    DECLARE @ErrNum INT = ERROR_NUMBER(),
            @ErrLine INT = ERROR_LINE(),
            @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Error en migración Parametro/Lists. Numero: %d, Línea: %d, Mensaje: %s',
              16, 1, @ErrNum, @ErrLine, @ErrMsg);
END CATCH;

