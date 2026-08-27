"""Crea permisos y asignaciones canónicas de identidad."""

from typing import Sequence, Union

from alembic import op

revision: str = "0007_permisos_identidad"
down_revision: Union[str, None] = "0006_sesion_estudiante"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute("""
    IF OBJECT_ID(N'identidad.permiso', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.permiso (
            id_permiso int IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_identidad_permiso PRIMARY KEY,
            clave nvarchar(150) NOT NULL,
            descripcion nvarchar(300) NULL,
            activo bit NOT NULL CONSTRAINT DF_identidad_permiso_activo DEFAULT 1,
            CONSTRAINT UQ_identidad_permiso_clave UNIQUE (clave)
        );
    END
    """)
    op.execute("""
    IF OBJECT_ID(N'identidad.usuario_permiso', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.usuario_permiso (
            id_usuario int NOT NULL,
            id_permiso int NOT NULL,
            fecha_asignacion datetime2(3) NOT NULL
                CONSTRAINT DF_identidad_usuario_permiso_fecha DEFAULT SYSUTCDATETIME(),
            CONSTRAINT PK_identidad_usuario_permiso PRIMARY KEY (id_usuario, id_permiso),
            CONSTRAINT FK_identidad_usuario_permiso_usuario FOREIGN KEY (id_usuario)
                REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE,
            CONSTRAINT FK_identidad_usuario_permiso_permiso FOREIGN KEY (id_permiso)
                REFERENCES identidad.permiso(id_permiso) ON DELETE CASCADE
        );
    END
    """)
    op.execute("""
    IF OBJECT_ID(N'identidad.rol', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.rol (
            id_rol int IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_identidad_rol PRIMARY KEY,
            nombre nvarchar(120) NOT NULL,
            descripcion nvarchar(300) NULL,
            activo bit NOT NULL CONSTRAINT DF_identidad_rol_activo DEFAULT 1,
            CONSTRAINT UQ_identidad_rol_nombre UNIQUE (nombre)
        );
    END
    """)
    op.execute("""
    IF OBJECT_ID(N'identidad.rol_permiso', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.rol_permiso (
            id_rol int NOT NULL,
            id_permiso int NOT NULL,
            CONSTRAINT PK_identidad_rol_permiso PRIMARY KEY (id_rol, id_permiso),
            CONSTRAINT FK_identidad_rol_permiso_rol FOREIGN KEY (id_rol)
                REFERENCES identidad.rol(id_rol) ON DELETE CASCADE,
            CONSTRAINT FK_identidad_rol_permiso_permiso FOREIGN KEY (id_permiso)
                REFERENCES identidad.permiso(id_permiso) ON DELETE CASCADE
        );
    END
    """)
    op.execute("""
    IF OBJECT_ID(N'identidad.usuario_rol', N'U') IS NULL
    BEGIN
        CREATE TABLE identidad.usuario_rol (
            id_usuario int NOT NULL,
            id_rol int NOT NULL,
            CONSTRAINT PK_identidad_usuario_rol PRIMARY KEY (id_usuario, id_rol),
            CONSTRAINT FK_identidad_usuario_rol_usuario FOREIGN KEY (id_usuario)
                REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE,
            CONSTRAINT FK_identidad_usuario_rol_rol FOREIGN KEY (id_rol)
                REFERENCES identidad.rol(id_rol) ON DELETE CASCADE
        );
    END
    """)


def downgrade() -> None:
    op.execute(
        "IF OBJECT_ID(N'identidad.usuario_rol', N'U') IS NOT NULL DROP TABLE identidad.usuario_rol"
    )
    op.execute(
        "IF OBJECT_ID(N'identidad.rol_permiso', N'U') IS NOT NULL DROP TABLE identidad.rol_permiso"
    )
    op.execute(
        "IF OBJECT_ID(N'identidad.usuario_permiso', N'U') IS NOT NULL DROP TABLE identidad.usuario_permiso"
    )
    op.execute("IF OBJECT_ID(N'identidad.rol', N'U') IS NOT NULL DROP TABLE identidad.rol")
    op.execute("IF OBJECT_ID(N'identidad.permiso', N'U') IS NOT NULL DROP TABLE identidad.permiso")
