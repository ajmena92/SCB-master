"""Incorpora al comedor los profesores ya existentes en identidad.

La identidad web no tiene un campo de tipo de persona. La única derivación
segura disponible es una asignación explícita a un rol llamado exactamente
``Profesor`` o ``Docente``. Esta migración nunca crea usuarios ni consulta el
modelos fuera del esquema web.
"""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op


revision: str = "0027_catalogo_profesores"
down_revision: Union[str, None] = "0026_idempotencia_corte_comedor"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'identidad.usuario', N'U') IS NULL
                THROW 50040, 'Catalogo profesores abortado: no existe identidad.usuario', 1;
            IF OBJECT_ID(N'identidad.rol', N'U') IS NULL
                THROW 50041, 'Catalogo profesores abortado: no existe identidad.rol', 1;
            IF OBJECT_ID(N'identidad.usuario_rol', N'U') IS NULL
                THROW 50042, 'Catalogo profesores abortado: no existe identidad.usuario_rol', 1;
            IF OBJECT_ID(N'comedor.persona', N'U') IS NULL
                THROW 50043, 'Catalogo profesores abortado: no existe comedor.persona', 1;
            IF OBJECT_ID(N'comedor.cuenta_tiquetes', N'U') IS NULL
                THROW 50044, 'Catalogo profesores abortado: no existe comedor.cuenta_tiquetes', 1;

            IF EXISTS (
                SELECT 1
                FROM identidad.usuario AS u
                INNER JOIN comedor.persona AS p
                    ON p.id_usuario = u.id_usuario
                WHERE EXISTS (
                    SELECT 1
                    FROM identidad.usuario_rol AS ur
                    INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol
                    WHERE ur.id_usuario = u.id_usuario
                      AND r.activo = 1
                      AND LOWER(LTRIM(RTRIM(r.nombre))) IN (N'profesor', N'docente')
                )
                  AND p.tipo_persona <> 'profesor'
            )
                THROW 50045, 'Catalogo profesores abortado: usuario ya vinculado a otra persona de comedor', 1;

            INSERT INTO comedor.persona
                (tipo_persona, id_usuario, codigo_barras, nombre_completo,
                 colegio, estado_comedor, activo, creado_en, actualizado_en)
            SELECT
                'profesor', u.id_usuario,
                CONCAT('P-', CONVERT(varchar(20), u.id_usuario)),
                LEFT(u.nombre_usuario, 220),
                NULL, 'no_becado_comedor', u.activo, SYSUTCDATETIME(), SYSUTCDATETIME()
            FROM identidad.usuario AS u
            WHERE EXISTS (
                SELECT 1
                FROM identidad.usuario_rol AS ur
                INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol
                WHERE ur.id_usuario = u.id_usuario
                  AND r.activo = 1
                  AND LOWER(LTRIM(RTRIM(r.nombre))) IN (N'profesor', N'docente')
            )
              AND NOT EXISTS (
                  SELECT 1
                  FROM comedor.persona AS p
                  WHERE p.id_usuario = u.id_usuario
              );

            INSERT INTO comedor.cuenta_tiquetes
                (id_persona, saldo, reservados, actualizado_en)
            SELECT p.id_persona, 0, 0, SYSUTCDATETIME()
            FROM comedor.persona AS p
            INNER JOIN identidad.usuario AS u
                ON u.id_usuario = p.id_usuario
            WHERE p.tipo_persona = 'profesor'
              AND EXISTS (
                  SELECT 1
                  FROM identidad.usuario_rol AS ur
                  INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol
                  WHERE ur.id_usuario = u.id_usuario
                    AND r.activo = 1
                    AND LOWER(LTRIM(RTRIM(r.nombre))) IN (N'profesor', N'docente')
              )
              AND NOT EXISTS (
                  SELECT 1
                  FROM comedor.cuenta_tiquetes AS c
                  WHERE c.id_persona = p.id_persona
              );
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("La incorporación de profesores no admite reversión destructiva")
