/*
   Corrige una transferencia previa que pudo incluir profesores en
   estudiantes.estudiante. Ejecutar después de 021 y con respaldo.
   La operación es conservadora: si los registros incorrectos tienen marcas,
   consumos, fotografías, cuentas o asignaciones, se detiene para no perder
   histórico; la reconciliación manual debe preservar esos datos.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Usuario', N'U') IS NULL
    THROW 50024, 'No existe dbo.Usuario; no se puede reconciliar el padrón.', 1;

DECLARE @incorrectos TABLE (id_estudiante int PRIMARY KEY);
INSERT INTO @incorrectos
SELECT e.id_estudiante
FROM estudiantes.estudiante e
JOIN dbo.Usuario u ON u.IdUsuario=e.id_estudiante AND u.CodTipo=2
WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuario estudiante WHERE estudiante.IdUsuario=e.id_estudiante AND estudiante.CodTipo=1);

IF EXISTS (SELECT 1 FROM asistencia.marca m JOIN @incorrectos i ON i.id_estudiante=m.id_estudiante)
    THROW 50025, 'Hay marcas históricas asociadas a registros que no son estudiantes; migración detenida.', 1;
IF EXISTS (SELECT 1 FROM comedor.registro c JOIN @incorrectos i ON i.id_estudiante=c.id_estudiante)
    THROW 50026, 'Hay consumos asociados a registros que no son estudiantes; migración detenida.', 1;
IF EXISTS (SELECT 1 FROM cuentas.cuenta_saldo c JOIN @incorrectos i ON i.id_estudiante=c.id_estudiante)
    THROW 50027, 'Hay cuentas asociadas a registros que no son estudiantes; migración detenida.', 1;
IF EXISTS (SELECT 1 FROM identidad.sesion_estudiante s JOIN @incorrectos i ON i.id_estudiante=s.id_usuario)
    THROW 50028, 'Hay sesiones asociadas a registros que no son estudiantes; migración detenida.', 1;

DELETE a FROM beneficios.asignacion a JOIN @incorrectos i ON i.id_estudiante=a.id_estudiante;
DELETE a FROM transporte.asignacion_ruta a JOIN @incorrectos i ON i.id_estudiante=a.id_estudiante;
DELETE f FROM estudiantes.fotografia f JOIN @incorrectos i ON i.id_estudiante=f.id_estudiante;
DELETE e FROM estudiantes.estudiante e JOIN @incorrectos i ON i.id_estudiante=e.id_estudiante;

COMMIT TRANSACTION;
