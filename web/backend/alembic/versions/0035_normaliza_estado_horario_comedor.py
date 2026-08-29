"""Normaliza estados canónicos y vincula el horario de cada estudiante."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0035_normaliza_estado_horario_comedor"
down_revision: Union[str, None] = "0034_migracion_datos_legados"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'comedor.persona',N'U') IS NOT NULL
            BEGIN
                IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name=N'CK_comedor_persona_estado')
                    ALTER TABLE comedor.persona DROP CONSTRAINT CK_comedor_persona_estado;
                UPDATE comedor.persona SET estado_comedor=CASE estado_comedor
                    WHEN 'becado' THEN 'becado_comedor'
                    WHEN 'no_becado' THEN 'no_becado_comedor'
                    ELSE estado_comedor END;
                ALTER TABLE comedor.persona ADD CONSTRAINT CK_comedor_persona_estado
                    CHECK (estado_comedor IN ('becado_comedor','no_becado_comedor'));
            END;
            IF OBJECT_ID(N'dbo.Usuario',N'U') IS NOT NULL
               AND OBJECT_ID(N'comedor.horario_operacion',N'U') IS NOT NULL
               AND OBJECT_ID(N'estudiantes.estudiante',N'U') IS NOT NULL
            BEGIN
                UPDATE e SET turno=o.codigo
                FROM estudiantes.estudiante e
                INNER JOIN dbo.Usuario u ON u.IdUsuario=e.id_estudiante
                INNER JOIN comedor.horario_operacion o ON o.id_horario_origen=u.IdHorario;
                IF EXISTS (
                    SELECT 1 FROM dbo.Usuario u
                    INNER JOIN estudiantes.estudiante e ON e.id_estudiante=u.IdUsuario
                    WHERE u.CodTipo=1 AND NOT EXISTS (
                        SELECT 1 FROM comedor.horario_operacion o WHERE o.id_horario_origen=u.IdHorario
                    )
                )
                    THROW 50066, 'Existen estudiantes sin horario de comedor de origen', 1;
            END;
            IF EXISTS (
                SELECT 1 FROM estudiantes.estudiante
                WHERE turno IS NOT NULL AND LOWER(LTRIM(RTRIM(turno))) NOT IN ('diurno','nocturno')
            )
                THROW 50067, 'Existen estudiantes con turno de comedor no canónico', 1;
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("La normalización de estados no admite reversión destructiva")
