"""Traslada las plantillas existentes al almacenamiento canónico del menú."""

from typing import Sequence, Union

from alembic import op

revision: str = "0015_migra_menu_historico"
down_revision: Union[str, None] = "0014_plantillas_menu"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute(
        """
        IF OBJECT_ID(N'ComedorPortal.MenuPlantilla', N'U') IS NOT NULL
        BEGIN
            INSERT INTO menu.plantilla
                (semana, dia, titulo, observaciones, activo, creado_por, actualizado_por)
            SELECT
                hp.SemanaMes,
                hp.DiaSemana,
                hp.Titulo,
                hp.Observaciones,
                hp.Activo,
                COALESCE(hp.IdUsuarioModifica, 1),
                hp.IdUsuarioModifica
            FROM ComedorPortal.MenuPlantilla AS hp
            WHERE NOT EXISTS (
                SELECT 1
                FROM menu.plantilla AS cp
                WHERE cp.semana = hp.SemanaMes AND cp.dia = hp.DiaSemana
            );

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
        END
        """
    )


def downgrade() -> None:
    # La transferencia no elimina datos históricos ni canónicos.
    pass
