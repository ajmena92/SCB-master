"""Crea las tablas canónicas de plantillas y componentes del menú."""

from typing import Sequence, Union

from alembic import op

revision: str = "0014_plantillas_menu"
down_revision: Union[str, None] = "0013_intentos_autenticacion"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute(
        """
        IF OBJECT_ID(N'menu.plantilla', N'U') IS NULL
        BEGIN
            CREATE TABLE menu.plantilla (
                id_plantilla int IDENTITY(1,1) NOT NULL
                    CONSTRAINT PK_menu_plantilla PRIMARY KEY,
                semana tinyint NOT NULL,
                dia tinyint NOT NULL,
                titulo nvarchar(160) NOT NULL,
                observaciones nvarchar(500) NULL,
                activo bit NOT NULL CONSTRAINT DF_menu_plantilla_activo DEFAULT 1,
                creado_por int NOT NULL,
                actualizado_por int NULL,
                CONSTRAINT UQ_menu_plantilla_semana_dia UNIQUE (semana, dia),
                CONSTRAINT CK_menu_plantilla_semana CHECK (semana BETWEEN 1 AND 5),
                CONSTRAINT CK_menu_plantilla_dia CHECK (dia BETWEEN 1 AND 5)
            );
        END
        IF OBJECT_ID(N'menu.componente', N'U') IS NULL
        BEGIN
            CREATE TABLE menu.componente (
                id_componente int IDENTITY(1,1) NOT NULL
                    CONSTRAINT PK_menu_componente PRIMARY KEY,
                id_plantilla int NOT NULL,
                nombre nvarchar(120) NOT NULL,
                tipo nvarchar(40) NOT NULL,
                orden tinyint NOT NULL,
                CONSTRAINT FK_menu_componente_plantilla FOREIGN KEY (id_plantilla)
                    REFERENCES menu.plantilla(id_plantilla) ON DELETE CASCADE,
                CONSTRAINT CK_menu_componente_orden CHECK (orden BETWEEN 1 AND 20),
                CONSTRAINT UQ_menu_componente_orden UNIQUE (id_plantilla, orden)
            );
        END
        """
    )


def downgrade() -> None:
    op.execute("IF OBJECT_ID(N'menu.componente', N'U') IS NOT NULL DROP TABLE menu.componente")
    op.execute("IF OBJECT_ID(N'menu.plantilla', N'U') IS NOT NULL DROP TABLE menu.plantilla")
