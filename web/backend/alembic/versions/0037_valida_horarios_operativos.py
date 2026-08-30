"""Garantiza horarios canónicos para estudiantes habilitados en comedor."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0037_valida_horarios_operativos"
down_revision: Union[str, None] = "0036_estado_comedor_catalogo"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(
        sa.text(
            """
            IF EXISTS (
                SELECT 1
                FROM estudiantes.estudiante e
                INNER JOIN comedor.persona p
                    ON p.id_estudiante=e.id_estudiante
                   AND p.tipo_persona='estudiante'
                   AND p.activo=1
                WHERE e.activo=1 AND (
                    e.turno IS NULL OR
                    LOWER(LTRIM(RTRIM(e.turno))) NOT IN ('diurno','nocturno')
                )
            )
                THROW 50069, 'Existen estudiantes activos con horario de comedor no canónico', 1;

            IF EXISTS (
                SELECT 1
                FROM estudiantes.estudiante e
                INNER JOIN comedor.persona p
                    ON p.id_estudiante=e.id_estudiante
                   AND p.tipo_persona='estudiante'
                   AND p.activo=1
                WHERE e.activo=1 AND NOT EXISTS (
                    SELECT 1 FROM comedor.horario_operacion h
                    WHERE h.codigo=LOWER(LTRIM(RTRIM(e.turno))) AND h.activo=1
                )
            )
                THROW 50070, 'Existen estudiantes cuyo horario no está activo en comedor', 1;

            IF EXISTS (
                SELECT 1 FROM sys.check_constraints
                WHERE name=N'CK_estudiantes_turno_comedor_canonico'
            )
                ALTER TABLE estudiantes.estudiante
                    DROP CONSTRAINT CK_estudiantes_turno_comedor_canonico;

            ALTER TABLE estudiantes.estudiante
                ADD CONSTRAINT CK_estudiantes_turno_comedor_canonico
                CHECK (turno IS NULL OR LOWER(LTRIM(RTRIM(turno))) IN ('diurno','nocturno'));
            """
        )
    )


def downgrade() -> None:
    op.execute(
        """
        IF EXISTS (
            SELECT 1 FROM sys.check_constraints
            WHERE name=N'CK_estudiantes_turno_comedor_canonico'
        )
            ALTER TABLE estudiantes.estudiante
                DROP CONSTRAINT CK_estudiantes_turno_comedor_canonico;
        """
    )
