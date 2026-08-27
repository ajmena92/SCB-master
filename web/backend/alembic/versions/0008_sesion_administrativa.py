"""Crea la persistencia de sesiones administrativas canónicas."""

from typing import Sequence, Union

from alembic import op

revision: str = "0008_sesion_administrativa"
down_revision: Union[str, None] = "0007_permisos_identidad"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute("""
    IF OBJECT_ID(N'identidad.sesion', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.sesion (
            id_sesion varchar(64) NOT NULL CONSTRAINT PK_identidad_sesion PRIMARY KEY,
            id_usuario int NOT NULL,
            secreto_hash char(64) NOT NULL,
            expira_en datetime2(3) NOT NULL,
            csrf_hash char(64) NULL,
            revocada bit NOT NULL CONSTRAINT DF_identidad_sesion_revocada DEFAULT 0,
            fecha_creacion datetime2(3) NOT NULL CONSTRAINT DF_identidad_sesion_creacion DEFAULT SYSUTCDATETIME(),
            fecha_revocacion datetime2(3) NULL,
            CONSTRAINT FK_identidad_sesion_usuario FOREIGN KEY (id_usuario)
                REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE
        );
        CREATE INDEX IX_identidad_sesion_vigente
            ON identidad.sesion (id_usuario, revocada, expira_en);
    END
    """)


def downgrade() -> None:
    op.execute("IF OBJECT_ID(N'identidad.sesion', N'U') IS NOT NULL DROP TABLE identidad.sesion")
