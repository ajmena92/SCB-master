SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_comedor_persona_estado')
    ALTER TABLE comedor.persona DROP CONSTRAINT CK_comedor_persona_estado;
UPDATE comedor.persona SET estado_comedor=CASE estado_comedor
    WHEN 'becado' THEN 'becado_comedor' WHEN 'no_becado' THEN 'no_becado_comedor'
    ELSE estado_comedor END;
ALTER TABLE comedor.persona ADD CONSTRAINT CK_comedor_persona_estado
    CHECK (estado_comedor IN ('becado_comedor','no_becado_comedor'));

UPDATE e SET turno=o.codigo
FROM estudiantes.estudiante e
JOIN dbo.Usuario u ON u.IdUsuario=e.id_estudiante
JOIN comedor.horario_operacion o ON o.id_horario_origen=u.IdHorario;
IF EXISTS (SELECT 1 FROM dbo.Usuario u JOIN estudiantes.estudiante e ON e.id_estudiante=u.IdUsuario
          WHERE u.CodTipo=1 AND NOT EXISTS
          (SELECT 1 FROM comedor.horario_operacion o WHERE o.id_horario_origen=u.IdHorario))
    THROW 50066, 'Existen estudiantes sin horario de comedor de origen', 1;
IF EXISTS (SELECT 1 FROM estudiantes.estudiante
          WHERE turno IS NOT NULL AND LOWER(LTRIM(RTRIM(turno))) NOT IN ('diurno','nocturno'))
    THROW 50067, 'Existen estudiantes con turno de comedor no canónico', 1;

COMMIT TRANSACTION;
