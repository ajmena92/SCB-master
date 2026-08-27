"""Asigna administración global al usuario web ``profe``."""

from typing import Sequence, Union

from alembic import op


revision: str = "0012_administrador_profe"
down_revision: Union[str, None] = "0011_revocacion_sesion_est"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

_PERMISOS = (
    "administracion.permisos.editar",
    "administracion.usuarios.editar",
    "administracion.usuarios.leer",
    "asistencia.editar",
    "asistencia.leer",
    "auditoria.leer",
    "beneficios.editar",
    "beneficios.leer",
    "calendario.leer",
    "comedor.registrar",
    "cuentas.editar",
    "cuentas.leer",
    "estudiantes.editar",
    "estudiantes.leer",
    "importaciones.ejecutar",
    "importaciones.leer",
    "menu.editar",
    "menu.leer",
    "parametros.editar",
    "parametros.leer",
    "reportes.dashboard.leer",
    "reportes.exportar",
    "reportes.leer",
    "rutas.administrar",
    "soporte.crear",
)


def upgrade() -> None:
    for permiso in _PERMISOS:
        op.execute(
            "IF NOT EXISTS (SELECT 1 FROM identidad.permiso WHERE clave = N'{}') "
            "INSERT INTO identidad.permiso(clave, descripcion) "
            "VALUES (N'{}', N'Permiso canónico de la plataforma web')".format(permiso, permiso)
        )
    op.execute(
        """
        IF NOT EXISTS (SELECT 1 FROM identidad.rol WHERE nombre = N'Administrador')
            INSERT INTO identidad.rol(nombre, descripcion)
            VALUES (N'Administrador', N'Acceso total a la plataforma web')
        """
    )
    op.execute(
        """
        INSERT INTO identidad.rol_permiso(id_rol, id_permiso)
        SELECT r.id_rol, p.id_permiso
        FROM identidad.rol AS r
        CROSS JOIN identidad.permiso AS p
        WHERE r.nombre = N'Administrador' AND p.activo = 1
          AND NOT EXISTS (
              SELECT 1 FROM identidad.rol_permiso AS rp
              WHERE rp.id_rol = r.id_rol AND rp.id_permiso = p.id_permiso
          )
        """
    )
    op.execute(
        """
        INSERT INTO identidad.usuario_rol(id_usuario, id_rol)
        SELECT u.id_usuario, r.id_rol
        FROM identidad.usuario AS u
        CROSS JOIN identidad.rol AS r
        WHERE LOWER(u.nombre_usuario) = N'profe' AND r.nombre = N'Administrador'
          AND NOT EXISTS (
              SELECT 1 FROM identidad.usuario_rol AS ur
              WHERE ur.id_usuario = u.id_usuario AND ur.id_rol = r.id_rol
          )
        """
    )


def downgrade() -> None:
    op.execute(
        """
        DELETE ur
        FROM identidad.usuario_rol AS ur
        INNER JOIN identidad.usuario AS u ON u.id_usuario = ur.id_usuario
        INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol
        WHERE LOWER(u.nombre_usuario) = N'profe' AND r.nombre = N'Administrador'
        """
    )
