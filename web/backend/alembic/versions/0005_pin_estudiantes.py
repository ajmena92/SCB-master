"""Añade credenciales PIN al dominio canónico de estudiantes."""
from typing import Sequence, Union

from alembic import op

revision: str = "0005_pin_estudiantes"
down_revision: Union[str, None] = "0004_fotos_dashboard"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    # 0002 ya incluía id_ruta; cada columna se agrega solo si falta para que
    # la migración funcione tanto en una base nueva como en una existente.
    op.execute("""
    IF COL_LENGTH(N'estudiantes.estudiante', N'hash_contrasena') IS NULL
        ALTER TABLE estudiantes.estudiante ADD hash_contrasena nvarchar(255) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'debe_cambiar_pin') IS NULL
        ALTER TABLE estudiantes.estudiante ADD debe_cambiar_pin bit NOT NULL CONSTRAINT DF_estudiante_debe_cambiar_pin DEFAULT 0;
    IF COL_LENGTH(N'estudiantes.estudiante', N'seccion') IS NULL
        ALTER TABLE estudiantes.estudiante ADD seccion nvarchar(30) NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'id_beneficio') IS NULL
        ALTER TABLE estudiantes.estudiante ADD id_beneficio int NULL;
    IF COL_LENGTH(N'estudiantes.estudiante', N'id_ruta') IS NULL
    BEGIN
        ALTER TABLE estudiantes.estudiante ADD id_ruta int NULL;
        ALTER TABLE estudiantes.estudiante ADD CONSTRAINT FK_estudiante_ruta
            FOREIGN KEY (id_ruta) REFERENCES transporte.ruta(id_ruta);
    END;
    IF COL_LENGTH(N'estudiantes.estudiante', N'turno') IS NULL
        ALTER TABLE estudiantes.estudiante ADD turno nvarchar(30) NULL;
    """)


def downgrade() -> None:
    op.drop_column("estudiante", "seccion", schema="estudiantes")
    op.execute("""DECLARE @sql nvarchar(max); SELECT @sql = N'ALTER TABLE estudiantes.estudiante DROP CONSTRAINT [' + dc.name + N']' FROM sys.default_constraints dc JOIN sys.columns c ON c.default_object_id=dc.object_id WHERE dc.parent_object_id=OBJECT_ID(N'estudiantes.estudiante') AND c.name=N'debe_cambiar_pin'; IF @sql IS NOT NULL EXEC sp_executesql @sql;""")
    op.drop_column("estudiante", "debe_cambiar_pin", schema="estudiantes")
    op.drop_column("estudiante", "hash_contrasena", schema="estudiantes")
    op.drop_column("estudiante", "id_beneficio", schema="estudiantes")
    # id_ruta pertenece a 0002 y se conserva al revertir esta revisión.
    op.drop_column("estudiante", "turno", schema="estudiantes")
