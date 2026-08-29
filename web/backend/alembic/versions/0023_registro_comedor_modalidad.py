"""Crea el registro canónico de consumos del comedor."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op


revision: str = "0023_registro_comedor_modalidad"
down_revision: Union[str, None] = "0022_activo_ruta"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(sa.text("IF SCHEMA_ID(N'comedor') IS NULL EXEC(N'CREATE SCHEMA comedor')"))
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'comedor.registro', N'U') IS NULL
            BEGIN
                CREATE TABLE comedor.registro (
                    id_registro BIGINT IDENTITY(1,1) NOT NULL
                        CONSTRAINT PK_comedor_registro PRIMARY KEY,
                    id_estudiante INT NOT NULL,
                    fecha DATE NOT NULL,
                    registrado_por INT NOT NULL,
                    creado_en DATETIME2 NOT NULL CONSTRAINT DF_comedor_registro_creado_en
                        DEFAULT SYSUTCDATETIME(),
                    modalidad VARCHAR(20) NOT NULL CONSTRAINT DF_comedor_registro_modalidad
                        DEFAULT 'beca',
                    CONSTRAINT UQ_comedor_registro_estudiante_fecha
                        UNIQUE (id_estudiante, fecha),
                    CONSTRAINT CK_comedor_registro_modalidad
                        CHECK (modalidad IN ('beca', 'tiquete', 'otro'))
                );
            END
            ELSE IF COL_LENGTH(N'comedor.registro', N'modalidad') IS NULL
            BEGIN
                ALTER TABLE comedor.registro ADD modalidad VARCHAR(20) NOT NULL
                    CONSTRAINT DF_comedor_registro_modalidad DEFAULT 'beca';
                ALTER TABLE comedor.registro ADD CONSTRAINT CK_comedor_registro_modalidad
                    CHECK (modalidad IN ('beca', 'tiquete', 'otro'));
            END
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("La migración de consumos no admite reversión destructiva")
