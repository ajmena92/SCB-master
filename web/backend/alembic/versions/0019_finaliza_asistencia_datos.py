"""Finaliza la migración física de asistencia en bases parcialmente actualizadas."""

from typing import Sequence, Union

from alembic import op

revision: str = "0019_finaliza_asistencia_datos"
down_revision: Union[str, None] = "0018_repara_asistencia_canonica"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute(
        """
        IF COL_LENGTH(N'asistencia.marca',N'fecha') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD fecha date NULL');
        IF COL_LENGTH(N'asistencia.marca',N'observacion') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD observacion nvarchar(500) NULL');
        IF COL_LENGTH(N'asistencia.marca',N'corregida') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD corregida bit NOT NULL CONSTRAINT DF_asistencia_marca_corregida_0019 DEFAULT 0');
        IF COL_LENGTH(N'asistencia.marca',N'creado_por') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD creado_por int NULL');
        IF COL_LENGTH(N'asistencia.marca',N'actualizado_por') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD actualizado_por int NULL');
        IF COL_LENGTH(N'asistencia.marca',N'direccion_ip') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD direccion_ip varchar(64) NULL');
        IF COL_LENGTH(N'asistencia.marca',N'fecha_creacion') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD fecha_creacion datetime2(3) NULL');
        IF COL_LENGTH(N'asistencia.marca',N'fecha_actualizacion') IS NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD fecha_actualizacion datetime2(3) NULL');
        """
    )
    op.execute(
        """
        IF COL_LENGTH(N'asistencia.marca',N'fecha_hora') IS NOT NULL
        BEGIN
            EXEC(N'UPDATE asistencia.marca SET fecha=CONVERT(date,fecha_hora),estado=CASE estado WHEN ''confirmada'' THEN ''presente'' WHEN ''cancelada'' THEN ''ausente'' ELSE estado END,creado_por=COALESCE(creado_por,1),direccion_ip=COALESCE(direccion_ip,''MIGRACION''),fecha_creacion=COALESCE(fecha_creacion,fecha_hora),fecha_actualizacion=COALESCE(fecha_actualizacion,fecha_hora)');
            EXEC(N'ALTER TABLE asistencia.marca DROP COLUMN fecha_hora');
        END
        IF COL_LENGTH(N'asistencia.marca',N'fecha') IS NOT NULL
        BEGIN
            EXEC(N'UPDATE asistencia.marca SET estado=''ausente'' WHERE estado NOT IN (''presente'',''ausente'',''tardanza'',''justificada'')');
            EXEC(N'ALTER TABLE asistencia.marca ALTER COLUMN fecha date NOT NULL');
            EXEC(N'ALTER TABLE asistencia.marca ALTER COLUMN creado_por int NOT NULL');
            EXEC(N'ALTER TABLE asistencia.marca ALTER COLUMN direccion_ip varchar(64) NOT NULL');
            EXEC(N'ALTER TABLE asistencia.marca ALTER COLUMN fecha_creacion datetime2(3) NOT NULL');
            EXEC(N'ALTER TABLE asistencia.marca ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL');
        END
        IF OBJECT_ID(N'asistencia.correccion',N'U') IS NULL
            EXEC(N'CREATE TABLE asistencia.correccion(id_correccion bigint IDENTITY PRIMARY KEY,id_marca bigint NOT NULL,motivo nvarchar(500) NOT NULL,id_usuario int NOT NULL,direccion_ip varchar(64) NOT NULL,fecha_correccion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT FK_asistencia_correccion_marca FOREIGN KEY(id_marca) REFERENCES asistencia.marca(id_marca))');
        IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'asistencia.marca') AND name=N'UQ_asistencia_marca_estudiante_fecha')
           AND COL_LENGTH(N'asistencia.marca',N'fecha') IS NOT NULL
            EXEC(N'ALTER TABLE asistencia.marca ADD CONSTRAINT UQ_asistencia_marca_estudiante_fecha UNIQUE(id_estudiante,fecha)');
        """
    )


def downgrade() -> None:
    raise RuntimeError("La migración de asistencia no admite reversión destructiva")
