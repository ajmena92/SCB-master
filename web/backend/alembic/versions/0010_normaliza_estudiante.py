"""Normaliza la tabla de estudiantes al contrato canónico en español."""

from typing import Sequence, Union

from alembic import op

revision: str = "0010_normaliza_estudiante"
down_revision: Union[str, None] = "0009_expiracion_pin"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute("""
    IF COL_LENGTH(N'estudiantes.estudiante', N'carne') IS NULL ALTER TABLE estudiantes.estudiante ADD carne nvarchar(30) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'nombre') IS NULL ALTER TABLE estudiantes.estudiante ADD nombre nvarchar(100) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'primer_apellido') IS NULL ALTER TABLE estudiantes.estudiante ADD primer_apellido nvarchar(100) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'segundo_apellido') IS NULL ALTER TABLE estudiantes.estudiante ADD segundo_apellido nvarchar(100) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'cedula') IS NULL ALTER TABLE estudiantes.estudiante ADD cedula nvarchar(30) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'activo') IS NULL ALTER TABLE estudiantes.estudiante ADD activo bit NOT NULL CONSTRAINT DF_estudiante_activo DEFAULT 1;
    IF COL_LENGTH(N'estudiantes.estudiante', N'identificacion') IS NOT NULL
       AND COL_LENGTH(N'estudiantes.estudiante', N'nombre_completo') IS NOT NULL
        EXEC sp_executesql N'UPDATE e SET carne=COALESCE(carne, identificacion), nombre=COALESCE(nombre, nombre_completo) FROM estudiantes.estudiante e WHERE (carne IS NULL AND identificacion IS NOT NULL) OR (nombre IS NULL AND nombre_completo IS NOT NULL)';
    """)


def downgrade() -> None:
    op.execute("""
    IF COL_LENGTH(N'estudiantes.estudiante', N'activo') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN activo;
    IF COL_LENGTH(N'estudiantes.estudiante', N'cedula') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN cedula;
    IF COL_LENGTH(N'estudiantes.estudiante', N'segundo_apellido') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN segundo_apellido;
    IF COL_LENGTH(N'estudiantes.estudiante', N'primer_apellido') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN primer_apellido;
    IF COL_LENGTH(N'estudiantes.estudiante', N'nombre') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN nombre;
    IF COL_LENGTH(N'estudiantes.estudiante', N'carne') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN carne;
    """)
