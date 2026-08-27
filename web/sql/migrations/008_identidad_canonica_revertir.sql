/*
  Rollback de 008_identidad_canonica.sql.
  Operación destructiva: requiere respaldo y autorización explícita del DBA.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'identidad.sesion', N'U') IS NOT NULL DROP TABLE identidad.sesion;
IF OBJECT_ID(N'identidad.usuario_permiso', N'U') IS NOT NULL DROP TABLE identidad.usuario_permiso;
IF OBJECT_ID(N'identidad.permiso', N'U') IS NOT NULL DROP TABLE identidad.permiso;
IF OBJECT_ID(N'identidad.usuario', N'U') IS NOT NULL DROP TABLE identidad.usuario;

IF SCHEMA_ID(N'identidad') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID(N'identidad'))
    DROP SCHEMA identidad;

COMMIT TRANSACTION;
