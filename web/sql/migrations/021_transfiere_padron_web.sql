/*
   Transferencia única del padrón histórico al modelo web canónico.
   Ejecutar con respaldo y escrituras congeladas. Es idempotente y no elimina
   ni modifica dbo/ComedorPortal; el retiro del origen requiere reconciliación.
   CodTipo=1 corresponde al catálogo institucional de estudiantes.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
    THROW 50021, 'No existe dbo.Usuario; no se puede transferir el padrón.', 1;
IF OBJECT_ID(N'dbo.Ruta', N'U') IS NULL
    THROW 50022, 'No existe dbo.Ruta; no se puede transferir el transporte.', 1;
IF OBJECT_ID(N'dbo.TipoBeca', N'U') IS NULL
    THROW 50023, 'No existe dbo.TipoBeca; no se puede transferir beneficios.', 1;

/* Catálogo de rutas: los IDs se conservan. */
SET IDENTITY_INSERT transporte.ruta ON;
INSERT INTO transporte.ruta
    (id_ruta, codigo, descripcion, color_hex, activo, creado_por, direccion_ip,
     fecha_creacion, fecha_actualizacion)
SELECT r.IdRuta, LEFT(COALESCE(NULLIF(r.Codigo, N''), CONCAT(N'RUTA-', r.IdRuta)), 50),
       LEFT(COALESCE(NULLIF(r.Descripcion, N''), CONCAT(N'Ruta ', r.IdRuta)), 500),
       '#CBD5E1', r.Activo, 1, 'TRANSFERENCIA', SYSUTCDATETIME(), SYSUTCDATETIME()
FROM dbo.Ruta r
WHERE NOT EXISTS (SELECT 1 FROM transporte.ruta destino WHERE destino.id_ruta=r.IdRuta);
SET IDENTITY_INSERT transporte.ruta OFF;

/* Tipos de beca: los IDs se conservan. */
SET IDENTITY_INSERT beneficios.tipo_beneficio ON;
INSERT INTO beneficios.tipo_beneficio
    (id_beneficio, nombre, descripcion, dias_permitidos, activo,
     creado_por, direccion_ip, fecha_creacion, fecha_actualizacion)
SELECT b.IdBeca, LEFT(b.Descripcion, 100), NULL,
       CONVERT(tinyint, CASE WHEN b.DiasBeca BETWEEN 0 AND 7 THEN b.DiasBeca ELSE 5 END),
       b.Activo, 1, 'TRANSFERENCIA', SYSUTCDATETIME(), SYSUTCDATETIME()
FROM dbo.TipoBeca b
WHERE NOT EXISTS (
    SELECT 1 FROM beneficios.tipo_beneficio destino
    WHERE destino.id_beneficio=b.IdBeca
);
SET IDENTITY_INSERT beneficios.tipo_beneficio OFF;

/* Estudiantes activos e inactivos del tipo institucional 1. Los profesores no
   forman parte del padrón canónico de estudiantes. */
SET IDENTITY_INSERT estudiantes.estudiante ON;
INSERT INTO estudiantes.estudiante
    (id_estudiante, carne, nombre, primer_apellido, segundo_apellido, cedula,
     seccion, turno, hash_contrasena, debe_cambiar_pin, activo, creado_por,
     direccion_ip, fecha_creacion, fecha_actualizacion)
SELECT u.IdUsuario, LEFT(CONVERT(nvarchar(30), u.Cedula), 30),
       LEFT(COALESCE(NULLIF(u.Nombre, N''), N'Sin nombre'), 100),
       LEFT(COALESCE(NULLIF(u.PrimerApellido, N''), N'Sin apellido'), 100),
       LEFT(NULLIF(u.SegundoApellido, N''), 100), LEFT(CONVERT(nvarchar(30), u.Cedula), 30),
       LEFT(NULLIF(u.Seccion, N''), 30), LEFT(CONVERT(nvarchar(30), u.IdHorario), 30),
       NULL, 1, u.Activo, 1, 'TRANSFERENCIA', SYSUTCDATETIME(), SYSUTCDATETIME()
FROM dbo.Usuario u
WHERE u.CodTipo=1
  AND NOT EXISTS (SELECT 1 FROM estudiantes.estudiante destino WHERE destino.id_estudiante=u.IdUsuario);
SET IDENTITY_INSERT estudiantes.estudiante OFF;

/* Asignaciones de transporte y beneficios, también conservando IDs. */
INSERT INTO transporte.asignacion_ruta (id_ruta, id_estudiante)
SELECT u.IdRuta, u.IdUsuario
FROM dbo.Usuario u
WHERE u.CodTipo=1 AND u.IdRuta IS NOT NULL
  AND EXISTS (SELECT 1 FROM transporte.ruta r WHERE r.id_ruta=u.IdRuta)
  AND NOT EXISTS (
      SELECT 1 FROM transporte.asignacion_ruta a WHERE a.id_estudiante=u.IdUsuario
  );

INSERT INTO beneficios.asignacion (id_estudiante, id_beneficio, creado_por, direccion_ip)
SELECT u.IdUsuario, u.TipoBeca, 1, 'TRANSFERENCIA'
FROM dbo.Usuario u
WHERE u.CodTipo=1 AND u.TipoBeca IS NOT NULL
  AND EXISTS (
      SELECT 1 FROM beneficios.tipo_beneficio b WHERE b.id_beneficio=u.TipoBeca
  )
  AND NOT EXISTS (
      SELECT 1 FROM beneficios.asignacion a WHERE a.id_estudiante=u.IdUsuario
  );

COMMIT TRANSACTION;
