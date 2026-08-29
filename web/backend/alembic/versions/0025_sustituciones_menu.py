"""Crea sustituciones de menú dentro del esquema web."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op


revision: str = "0025_sustituciones_menu"
down_revision: Union[str, None] = "0024_corte_comedor_tiquetes"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'menu.sustitucion', N'U') IS NULL
            BEGIN
                CREATE TABLE menu.sustitucion (
                    id_sustitucion INT IDENTITY(1,1) NOT NULL
                        CONSTRAINT PK_menu_sustitucion PRIMARY KEY,
                    fecha DATE NOT NULL
                        CONSTRAINT UQ_menu_sustitucion_fecha UNIQUE,
                    titulo NVARCHAR(160) NOT NULL,
                    observaciones NVARCHAR(500) NULL,
                    creado_por INT NOT NULL,
                    actualizado_por INT NULL
                );
            END;
            IF OBJECT_ID(N'menu.componente_sustitucion', N'U') IS NULL
            BEGIN
                CREATE TABLE menu.componente_sustitucion (
                    id_componente INT IDENTITY(1,1) NOT NULL
                        CONSTRAINT PK_menu_componente_sustitucion PRIMARY KEY,
                    id_sustitucion INT NOT NULL,
                    nombre NVARCHAR(500) NOT NULL,
                    tipo NVARCHAR(40) NOT NULL,
                    orden TINYINT NOT NULL,
                    CONSTRAINT FK_menu_componente_sustitucion
                        FOREIGN KEY (id_sustitucion)
                        REFERENCES menu.sustitucion(id_sustitucion),
                    CONSTRAINT UQ_menu_componente_sustitucion_orden
                        UNIQUE (id_sustitucion, orden),
                    CONSTRAINT CK_menu_componente_sustitucion_orden
                        CHECK (orden BETWEEN 1 AND 20)
                );
            END;
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("Las sustituciones web no admiten reversión destructiva")
