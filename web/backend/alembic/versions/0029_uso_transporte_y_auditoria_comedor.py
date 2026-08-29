"""Agrega la lectura diaria de transporte y la trazabilidad del ingreso."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0029_uso_transporte_y_auditoria_comedor"
down_revision: Union[str, None] = "0028_horarios_operacion_comedor"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    # Los respaldos históricos pueden conservar alembic_version.version_num
    # como varchar(32); esta revisión tiene un identificador más largo.
    # Ampliarlo antes de que Alembic registre la revisión evita truncamientos
    # y no modifica ninguna tabla del dominio.
    conexion.execute(
        sa.text(
            """
            IF COL_LENGTH(N'dbo.alembic_version', N'version_num') IS NOT NULL
            BEGIN
                DECLARE @pk sysname;
                SELECT @pk = kc.name
                FROM sys.key_constraints kc
                WHERE kc.parent_object_id = OBJECT_ID(N'dbo.alembic_version')
                  AND kc.type = 'PK';
                IF @pk IS NOT NULL
                BEGIN
                    DECLARE @drop_pk nvarchar(4000) = N'ALTER TABLE dbo.alembic_version DROP CONSTRAINT ' + QUOTENAME(@pk);
                    EXEC sp_executesql @drop_pk;
                END;
                ALTER TABLE dbo.alembic_version ALTER COLUMN version_num nvarchar(128) NOT NULL;
                IF NOT EXISTS (
                    SELECT 1 FROM sys.key_constraints
                    WHERE parent_object_id = OBJECT_ID(N'dbo.alembic_version') AND type = 'PK'
                )
                    ALTER TABLE dbo.alembic_version ADD CONSTRAINT alembic_version_pkc PRIMARY KEY (version_num);
            END;
            """
        )
    )
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'transporte.uso_diario', N'U') IS NULL
            BEGIN
                CREATE TABLE transporte.uso_diario(
                    id_uso INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_transporte_uso_diario PRIMARY KEY,
                    id_estudiante INT NOT NULL,
                    fecha DATE NOT NULL,
                    marcado_en DATETIME2(3) NOT NULL CONSTRAINT DF_transporte_uso_marcado DEFAULT SYSUTCDATETIME(),
                    CONSTRAINT UQ_transporte_uso_estudiante_fecha UNIQUE(id_estudiante,fecha),
                    CONSTRAINT FK_transporte_uso_estudiante FOREIGN KEY(id_estudiante)
                        REFERENCES estudiantes.estudiante(id_estudiante)
                );
            END;
            IF COL_LENGTH(N'comedor.ingreso', N'codigo_horario') IS NULL
                ALTER TABLE comedor.ingreso ADD codigo_horario VARCHAR(20) NULL;
            IF COL_LENGTH(N'comedor.ingreso', N'hora_marca') IS NULL
                ALTER TABLE comedor.ingreso ADD hora_marca DATETIME2(3) NULL;
            IF COL_LENGTH(N'comedor.ingreso', N'marca_transporte_existente') IS NULL
                ALTER TABLE comedor.ingreso ADD marca_transporte_existente BIT NOT NULL
                    CONSTRAINT DF_comedor_ingreso_marca_transporte DEFAULT 0;
            IF OBJECT_ID(N'dbo.RegistroTransporte', N'U') IS NOT NULL
               AND COL_LENGTH(N'dbo.RegistroTransporte', N'IdUsuario') IS NOT NULL
               AND COL_LENGTH(N'dbo.RegistroTransporte', N'Fecha') IS NOT NULL
            BEGIN
                EXEC sys.sp_executesql N'
                    INSERT INTO transporte.uso_diario(id_estudiante,fecha,marcado_en)
                    SELECT rt.IdUsuario,CONVERT(date,rt.Fecha),MIN(rt.Fecha)
                    FROM dbo.RegistroTransporte rt
                    INNER JOIN estudiantes.estudiante e ON e.id_estudiante=rt.IdUsuario
                    WHERE NOT EXISTS(
                        SELECT 1 FROM transporte.uso_diario u
                        WHERE u.id_estudiante=rt.IdUsuario AND u.fecha=CONVERT(date,rt.Fecha)
                    )
                    GROUP BY rt.IdUsuario,CONVERT(date,rt.Fecha)';
            END;
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("La trazabilidad operativa no admite reversión destructiva")
