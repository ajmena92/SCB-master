"""Reconcilia físicamente las tablas web según el catálogo real de SQL Server."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0021_reconciliacion_fisica"
down_revision: Union[str, None] = "0020_asistencia_canonica"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def _columnas(conexion, esquema: str, tabla: str) -> set[str]:
    return {
        fila[0]
        for fila in conexion.execute(
            sa.text(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS "
                "WHERE TABLE_SCHEMA=:esquema AND TABLE_NAME=:tabla"
            ),
            {"esquema": esquema, "tabla": tabla},
        )
    }


def _existe(conexion, esquema: str, tabla: str) -> bool:
    return bool(
        conexion.execute(
            sa.text(
                "SELECT 1 FROM INFORMATION_SCHEMA.TABLES "
                "WHERE TABLE_SCHEMA=:esquema AND TABLE_NAME=:tabla"
            ),
            {"esquema": esquema, "tabla": tabla},
        ).scalar()
    )


def _eliminar_restricciones_columna(conexion, esquema: str, tabla: str, columna: str) -> None:
    objeto = f"{esquema}.{tabla}"
    nombres = conexion.execute(
        sa.text(
            "SELECT dc.name FROM sys.default_constraints dc "
            "JOIN sys.columns c ON c.default_object_id=dc.object_id "
            "WHERE dc.parent_object_id=OBJECT_ID(:objeto) AND c.name=:columna"
        ),
        {"objeto": objeto, "columna": columna},
    ).fetchall()
    for (nombre,) in nombres:
        conexion.execute(sa.text(f"ALTER TABLE {objeto} DROP CONSTRAINT [{nombre}]"))
    claves = conexion.execute(
        sa.text(
            "SELECT kc.name FROM sys.key_constraints kc "
            "JOIN sys.index_columns ic ON ic.object_id=kc.parent_object_id "
            "AND ic.index_id=kc.unique_index_id JOIN sys.columns c "
            "ON c.object_id=ic.object_id AND c.column_id=ic.column_id "
            "WHERE kc.parent_object_id=OBJECT_ID(:objeto) AND c.name=:columna"
        ),
        {"objeto": objeto, "columna": columna},
    ).fetchall()
    for (nombre,) in claves:
        conexion.execute(sa.text(f"ALTER TABLE {objeto} DROP CONSTRAINT [{nombre}]"))
    claves_fk = conexion.execute(
        sa.text(
            "SELECT DISTINCT fk.name FROM sys.foreign_keys fk "
            "JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id=fk.object_id "
            "JOIN sys.columns c ON c.object_id=fkc.parent_object_id "
            "AND c.column_id=fkc.parent_column_id "
            "WHERE fk.parent_object_id=OBJECT_ID(:objeto) AND c.name=:columna"
        ),
        {"objeto": objeto, "columna": columna},
    ).fetchall()
    for (nombre,) in claves_fk:
        conexion.execute(sa.text(f"ALTER TABLE {objeto} DROP CONSTRAINT [{nombre}]"))


def upgrade() -> None:
    conexion = op.get_bind()

    rutas = _columnas(conexion, "transporte", "ruta")
    if "nombre" in rutas:
        for sentencia in (
            "ALTER TABLE transporte.ruta ADD codigo nvarchar(50) NULL",
            "ALTER TABLE transporte.ruta ADD descripcion nvarchar(500) NULL",
            "ALTER TABLE transporte.ruta ADD color_hex char(7) NULL",
            "ALTER TABLE transporte.ruta ADD creado_por int NULL",
            "ALTER TABLE transporte.ruta ADD direccion_ip varchar(64) NULL",
            "ALTER TABLE transporte.ruta ADD fecha_creacion datetime2(3) NULL",
            "ALTER TABLE transporte.ruta ADD fecha_actualizacion datetime2(3) NULL",
        ):
            conexion.execute(sa.text(sentencia))
        conexion.execute(
            sa.text(
                "UPDATE transporte.ruta SET codigo=CONCAT(N'RUTA-',id_ruta),"
                "descripcion=LEFT(nombre,500),color_hex='#CBD5E1',creado_por=1,"
                "direccion_ip='MIGRACION',fecha_creacion=SYSUTCDATETIME(),"
                "fecha_actualizacion=SYSUTCDATETIME()"
            )
        )
        for sentencia in (
            "ALTER TABLE transporte.ruta ALTER COLUMN codigo nvarchar(50) NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN descripcion nvarchar(500) NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN color_hex char(7) NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN creado_por int NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN direccion_ip varchar(64) NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN fecha_creacion datetime2(3) NOT NULL",
            "ALTER TABLE transporte.ruta ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL",
        ):
            conexion.execute(sa.text(sentencia))
        conexion.execute(
            sa.text(
                "ALTER TABLE transporte.ruta ADD CONSTRAINT "
                "UQ_transporte_ruta_codigo_0021 UNIQUE(codigo)"
            )
        )
        _eliminar_restricciones_columna(conexion, "transporte", "ruta", "activa")
        conexion.execute(sa.text("ALTER TABLE transporte.ruta DROP COLUMN nombre"))
        conexion.execute(sa.text("ALTER TABLE transporte.ruta DROP COLUMN activa"))

    if not _existe(conexion, "transporte", "asignacion_ruta"):
        conexion.execute(
            sa.text(
                "CREATE TABLE transporte.asignacion_ruta("
                "id_asignacion bigint IDENTITY PRIMARY KEY,id_ruta int NOT NULL,"
                "id_estudiante int NOT NULL,activa bit NOT NULL DEFAULT 1,"
                "fecha_creacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),"
                "CONSTRAINT FK_transporte_asignacion_ruta_0021 FOREIGN KEY(id_ruta) "
                "REFERENCES transporte.ruta(id_ruta),CONSTRAINT "
                "UQ_transporte_asignacion_estudiante_0021 UNIQUE(id_estudiante))"
            )
        )

    estudiantes = _columnas(conexion, "estudiantes", "estudiante")
    if "identificacion" in estudiantes:
        for sentencia in (
            "ALTER TABLE estudiantes.estudiante ADD creado_por int NULL",
            "ALTER TABLE estudiantes.estudiante ADD actualizado_por int NULL",
            "ALTER TABLE estudiantes.estudiante ADD direccion_ip varchar(64) NULL",
            "ALTER TABLE estudiantes.estudiante ADD fecha_creacion datetime2(3) NULL",
            "ALTER TABLE estudiantes.estudiante ADD fecha_actualizacion datetime2(3) NULL",
        ):
            conexion.execute(sa.text(sentencia))
        conexion.execute(
            sa.text(
                "UPDATE estudiantes.estudiante SET carne=COALESCE(carne,identificacion),"
                "nombre=COALESCE(nombre,LEFT(nombre_completo,100)),"
                "primer_apellido=COALESCE(primer_apellido,N'Sin apellido'),"
                "creado_por=1,direccion_ip='MIGRACION',fecha_creacion=SYSUTCDATETIME(),"
                "fecha_actualizacion=SYSUTCDATETIME()"
            )
        )
        conexion.execute(
            sa.text(
                "UPDATE estudiantes.estudiante SET nombre=N'Sin nombre' "
                "WHERE nombre IS NULL OR nombre=N''"
            )
        )
        conexion.execute(
            sa.text(
                "UPDATE estudiantes.estudiante SET carne=CONCAT(N'LEGACY-',id_estudiante) "
                "WHERE carne IS NULL OR carne=N''"
            )
        )
        for sentencia in (
            "ALTER TABLE estudiantes.estudiante ALTER COLUMN carne nvarchar(30) NOT NULL",
            "ALTER TABLE estudiantes.estudiante ALTER COLUMN nombre nvarchar(100) NOT NULL",
            "ALTER TABLE estudiantes.estudiante ALTER COLUMN primer_apellido nvarchar(100) NOT NULL",
            "ALTER TABLE estudiantes.estudiante ALTER COLUMN fecha_creacion datetime2(3) NOT NULL",
            "ALTER TABLE estudiantes.estudiante ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL",
        ):
            conexion.execute(sa.text(sentencia))
        conexion.execute(
            sa.text(
                "ALTER TABLE estudiantes.estudiante ADD CONSTRAINT "
                "UQ_estudiantes_estudiante_carne_0021 UNIQUE(carne)"
            )
        )

    conexion.execute(
        sa.text(
            "INSERT INTO transporte.asignacion_ruta(id_ruta,id_estudiante) "
            "SELECT id_ruta,id_estudiante FROM estudiantes.estudiante e "
            "WHERE id_ruta IS NOT NULL AND NOT EXISTS(SELECT 1 FROM "
            "transporte.asignacion_ruta a WHERE a.id_estudiante=e.id_estudiante)"
        )
    )
    for nombre in ("identificacion", "nombre_completo", "id_ruta", "id_beneficio"):
        if nombre in _columnas(conexion, "estudiantes", "estudiante"):
            _eliminar_restricciones_columna(conexion, "estudiantes", "estudiante", nombre)
            conexion.execute(sa.text(f"ALTER TABLE estudiantes.estudiante DROP COLUMN {nombre}"))


def downgrade() -> None:
    raise RuntimeError("La reconciliación física no admite reversión destructiva")
