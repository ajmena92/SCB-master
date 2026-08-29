"""Migra asistencia con inspección real de columnas, una operación por DDL."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0020_asistencia_canonica"
down_revision: Union[str, None] = "0019_finaliza_asistencia_datos"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    columnas = {
        fila[0]
        for fila in conexion.execute(
            sa.text(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS "
                "WHERE TABLE_SCHEMA='asistencia' AND TABLE_NAME='marca'"
            )
        )
    }
    nuevas = {
        "fecha": "date NULL",
        "observacion": "nvarchar(500) NULL",
        "corregida": "bit NOT NULL CONSTRAINT DF_asistencia_marca_corregida_0020 DEFAULT 0",
        "creado_por": "int NULL",
        "actualizado_por": "int NULL",
        "direccion_ip": "varchar(64) NULL",
        "fecha_creacion": "datetime2(3) NULL",
        "fecha_actualizacion": "datetime2(3) NULL",
    }
    for nombre, definicion in nuevas.items():
        if nombre not in columnas:
            conexion.execute(sa.text(f"ALTER TABLE asistencia.marca ADD {nombre} {definicion}"))

    if "fecha_hora" in columnas:
        conexion.execute(
            sa.text(
                "UPDATE asistencia.marca SET fecha=CONVERT(date,fecha_hora), "
                "estado=CASE estado WHEN 'confirmada' THEN 'presente' "
                "WHEN 'cancelada' THEN 'ausente' ELSE estado END, "
                "creado_por=COALESCE(creado_por,1), direccion_ip=COALESCE(direccion_ip,'MIGRACION'), "
                "fecha_creacion=COALESCE(fecha_creacion,fecha_hora), "
                "fecha_actualizacion=COALESCE(fecha_actualizacion,fecha_hora)"
            )
        )
        conexion.execute(
            sa.text(
                "DELETE m FROM asistencia.marca m INNER JOIN "
                "(SELECT id_marca, ROW_NUMBER() OVER(PARTITION BY id_estudiante,fecha "
                "ORDER BY id_marca DESC) AS numero FROM asistencia.marca) d "
                "ON d.id_marca=m.id_marca WHERE d.numero>1"
            )
        )
        conexion.execute(
            sa.text(
                "UPDATE asistencia.marca SET estado='ausente' "
                "WHERE estado NOT IN('presente','ausente','tardanza','justificada')"
            )
        )
        for sentencia in (
            "ALTER TABLE asistencia.marca ALTER COLUMN fecha date NOT NULL",
            "ALTER TABLE asistencia.marca ALTER COLUMN creado_por int NOT NULL",
            "ALTER TABLE asistencia.marca ALTER COLUMN direccion_ip varchar(64) NOT NULL",
            "ALTER TABLE asistencia.marca ALTER COLUMN fecha_creacion datetime2(3) NOT NULL",
            "ALTER TABLE asistencia.marca ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL",
        ):
            conexion.execute(sa.text(sentencia))
        conexion.execute(sa.text("ALTER TABLE asistencia.marca DROP COLUMN fecha_hora"))

    existe_indice = conexion.execute(
        sa.text(
            "SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'asistencia.marca') "
            "AND name=N'UQ_asistencia_marca_estudiante_fecha'"
        )
    ).scalar()
    if not existe_indice:
        conexion.execute(
            sa.text(
                "ALTER TABLE asistencia.marca ADD CONSTRAINT "
                "UQ_asistencia_marca_estudiante_fecha UNIQUE(id_estudiante,fecha)"
            )
        )
    if not conexion.execute(
        sa.text("SELECT OBJECT_ID(N'asistencia.correccion',N'U')")
    ).scalar():
        conexion.execute(
            sa.text(
                "CREATE TABLE asistencia.correccion(id_correccion bigint IDENTITY PRIMARY KEY, "
                "id_marca int NOT NULL,motivo nvarchar(500) NOT NULL,id_usuario int NOT NULL, "
                "direccion_ip varchar(64) NOT NULL,fecha_correccion datetime2(3) NOT NULL "
                "DEFAULT SYSUTCDATETIME(),CONSTRAINT FK_asistencia_correccion_marca "
                "FOREIGN KEY(id_marca) REFERENCES asistencia.marca(id_marca))"
            )
        )


def downgrade() -> None:
    raise RuntimeError("La migración de asistencia no admite reversión destructiva")
