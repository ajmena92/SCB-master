"""Completa el traslado de componentes históricos del menú."""

from typing import Sequence, Union

from alembic import op


revision: str = "0016_completa_componentes_menu"
down_revision: Union[str, None] = "0015_migra_menu_historico"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute(
        """
        ALTER TABLE menu.componente ALTER COLUMN nombre nvarchar(500) NOT NULL;
        IF OBJECT_ID(N'ComedorPortal.MenuComponente', N'U') IS NOT NULL
        BEGIN
            INSERT INTO menu.componente (id_plantilla, nombre, tipo, orden)
            SELECT
                cp.id_plantilla,
                hc.Nombre,
                hc.TipoComponente,
                hc.Orden
            FROM ComedorPortal.MenuComponente AS hc
            INNER JOIN ComedorPortal.MenuPlantilla AS hp
                ON hp.IdMenuPlantilla = hc.IdMenuPlantilla
            INNER JOIN menu.plantilla AS cp
                ON cp.semana = hp.SemanaMes AND cp.dia = hp.DiaSemana
            WHERE NOT EXISTS (
                SELECT 1
                FROM menu.componente AS cc
                WHERE cc.id_plantilla = cp.id_plantilla AND cc.orden = hc.Orden
            );
        END
        """
    )


def downgrade() -> None:
    # No se eliminan componentes canónicos durante un downgrade.
    pass
