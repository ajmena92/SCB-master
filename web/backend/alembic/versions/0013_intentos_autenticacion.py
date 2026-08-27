"""Persiste el bloqueo de autenticación compartido entre procesos."""

from typing import Sequence, Union

from alembic import op

revision: str = "0013_intentos_autenticacion"
down_revision: Union[str, None] = "0012_administrador_profe"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute(
        """
        IF OBJECT_ID(N'identidad.intento_autenticacion', N'U') IS NULL
        BEGIN
            CREATE TABLE identidad.intento_autenticacion (
                identificador_hash char(64) NOT NULL
                    CONSTRAINT PK_identidad_intento_autenticacion PRIMARY KEY,
                intentos_fallidos int NOT NULL
                    CONSTRAINT DF_identidad_intento_fallidos DEFAULT 0,
                bloqueado_hasta datetime2(3) NULL,
                fecha_actualizacion datetime2(3) NOT NULL
                    CONSTRAINT DF_identidad_intento_actualizacion DEFAULT SYSUTCDATETIME(),
                CONSTRAINT CK_identidad_intento_fallidos CHECK (intentos_fallidos BETWEEN 0 AND 20)
            );
        END
        """
    )


def downgrade() -> None:
    op.execute(
        "IF OBJECT_ID(N'identidad.intento_autenticacion', N'U') IS NOT NULL "
        "DROP TABLE identidad.intento_autenticacion"
    )
