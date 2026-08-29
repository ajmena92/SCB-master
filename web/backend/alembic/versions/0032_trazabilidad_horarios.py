"""Completa el origen de la hora límite en instalaciones ya migradas."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0032_trazabilidad_horarios"
down_revision: Union[str, None] = "0031_reconciliacion_corte_comedor"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.get_bind().execute(sa.text("""
        IF COL_LENGTH(N'comedor.horario_operacion', N'origen') IS NULL
            ALTER TABLE comedor.horario_operacion ADD origen VARCHAR(30) NOT NULL
                CONSTRAINT DF_comedor_horario_origen DEFAULT 'migracion_0028';
        IF COL_LENGTH(N'comedor.horario_operacion', N'hora_limite_origen') IS NULL
            ALTER TABLE comedor.horario_operacion ADD hora_limite_origen TIME NULL;
        IF COL_LENGTH(N'comedor.horario_operacion', N'id_horario_origen') IS NULL
            ALTER TABLE comedor.horario_operacion ADD id_horario_origen INT NULL;
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
            UPDATE o SET hora_limite_origen=origen.HoraLimite,
                         id_horario_origen=origen.IdHorario,
                         origen='dbo.Horario'
            FROM comedor.horario_operacion o
            INNER JOIN origen ON origen.numero = CASE o.codigo WHEN 'diurno' THEN 1 WHEN 'nocturno' THEN 2 END;
        END;
    """))


def downgrade() -> None:
    raise RuntimeError("La trazabilidad de horarios no admite reversión destructiva")
