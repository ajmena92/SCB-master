"""Completa las políticas y la trazabilidad del ingreso de comedor."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0030_politicas_y_auditoria_operacion"
down_revision: Union[str, None] = "0029_uso_transporte_y_auditoria_comedor"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.get_bind().execute(sa.text("""
        IF COL_LENGTH(N'comedor.parametro', N'permitir_marca_tardia') IS NULL
            ALTER TABLE comedor.parametro ADD permitir_marca_tardia BIT NOT NULL
                CONSTRAINT DF_comedor_parametro_tardia DEFAULT 0;
        IF COL_LENGTH(N'comedor.parametro', N'permitir_sin_marca_transporte') IS NULL
            ALTER TABLE comedor.parametro ADD permitir_sin_marca_transporte BIT NOT NULL
                CONSTRAINT DF_comedor_parametro_transporte DEFAULT 1;
        IF COL_LENGTH(N'comedor.ingreso', N'hora_limite_aplicada') IS NULL
            ALTER TABLE comedor.ingreso ADD hora_limite_aplicada TIME NULL;
        IF COL_LENGTH(N'comedor.ingreso', N'resultado') IS NULL
            ALTER TABLE comedor.ingreso ADD resultado VARCHAR(20) NOT NULL
                CONSTRAINT DF_comedor_ingreso_resultado DEFAULT 'registrado';
        IF COL_LENGTH(N'comedor.ingreso', N'advertencias') IS NULL
            ALTER TABLE comedor.ingreso ADD advertencias NVARCHAR(500) NULL;
        IF COL_LENGTH(N'comedor.ingreso', N'permitir_marca_tardia') IS NULL
            ALTER TABLE comedor.ingreso ADD permitir_marca_tardia BIT NOT NULL
                CONSTRAINT DF_comedor_ingreso_politica_tardia DEFAULT 0;
        IF COL_LENGTH(N'comedor.ingreso', N'permitir_sin_marca_transporte') IS NULL
            ALTER TABLE comedor.ingreso ADD permitir_sin_marca_transporte BIT NOT NULL
                CONSTRAINT DF_comedor_ingreso_politica_transporte DEFAULT 1;
        IF OBJECT_ID(N'comedor.auditoria_ingreso', N'U') IS NULL
        BEGIN
            CREATE TABLE comedor.auditoria_ingreso(
                id_auditoria BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_auditoria_ingreso PRIMARY KEY,
                id_ingreso BIGINT NULL,
                id_persona INT NULL,
                fecha DATE NOT NULL,
                codigo_resultado VARCHAR(40) NOT NULL,
                detalle NVARCHAR(500) NULL,
                advertencias NVARCHAR(500) NULL,
                hora_servidor DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_auditoria_hora DEFAULT SYSUTCDATETIME(),
                registrado_por INT NULL,
                terminal_id VARCHAR(100) NULL,
                CONSTRAINT FK_comedor_auditoria_ingreso FOREIGN KEY(id_ingreso) REFERENCES comedor.ingreso(id_ingreso)
            );
            CREATE INDEX IX_comedor_auditoria_fecha ON comedor.auditoria_ingreso(fecha, id_persona);
        END;
    """))


def downgrade() -> None:
    raise RuntimeError("Las políticas y auditoría operativa no admiten reversión destructiva")
