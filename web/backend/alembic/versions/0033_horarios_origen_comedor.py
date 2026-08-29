"""Completa el mapeo determinista de horarios de origen del comedor."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0033_horarios_origen_comedor"
down_revision: Union[str, None] = "0032_trazabilidad_horarios"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(sa.text("""
        IF COL_LENGTH(N'comedor.horario_operacion', N'id_horario_origen') IS NULL
            ALTER TABLE comedor.horario_operacion ADD id_horario_origen INT NULL;
    """))
    conexion.execute(sa.text("""
        IF OBJECT_ID(N'dbo.Horario', N'U') IS NOT NULL
           AND COL_LENGTH(N'dbo.Horario', N'HoraLimite') IS NOT NULL
           AND COL_LENGTH(N'dbo.Horario', N'IdHorario') IS NOT NULL
        BEGIN
            IF EXISTS (SELECT 1 FROM dbo.Horario WHERE HoraLimite IS NULL)
                THROW 50061, 'Horario contiene registros sin HoraLimite; trazabilidad abortada', 1;
            IF (SELECT COUNT(*) FROM dbo.Horario) > 2
                THROW 50064, 'dbo.Horario contiene más de dos horarios; mapeo explícito requerido', 1;
            ;WITH origen AS (
                SELECT IdHorario, HoraLimite,
                       ROW_NUMBER() OVER (ORDER BY IdHorario) AS numero
                FROM dbo.Horario
            )
            UPDATE destino
               SET destino.id_horario_origen=origen.IdHorario,
                   destino.hora_limite_origen=origen.HoraLimite,
                   destino.origen='dbo.Horario'
            FROM comedor.horario_operacion destino
            INNER JOIN origen
              ON origen.numero = CASE destino.codigo
                    WHEN 'diurno' THEN 1
                    WHEN 'nocturno' THEN 2
                 END;
        END;
        IF EXISTS (
            SELECT 1 FROM comedor.horario_operacion
            WHERE origen='dbo.Horario' AND id_horario_origen IS NULL
        )
            THROW 50065, 'Horario operativo sin IdHorario de origen; corte abortado', 1;
    """))


def downgrade() -> None:
    raise RuntimeError("El mapeo de horarios no admite reversión destructiva")
