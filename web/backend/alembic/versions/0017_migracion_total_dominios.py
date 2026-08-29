"""Transforma las tablas mínimas heredadas al contrato web canónico."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0017_migracion_total_dominios"
down_revision: Union[str, None] = "0016_completa_componentes_menu"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    ruta_heredada = conexion.execute(
        sa.text("SELECT COL_LENGTH(N'transporte.ruta', N'nombre')")
    ).scalar()
    if ruta_heredada is None:
        return

    op.execute(
        """
        IF COL_LENGTH(N'transporte.ruta', N'codigo') IS NULL ALTER TABLE transporte.ruta ADD codigo nvarchar(50) NULL;
        IF COL_LENGTH(N'transporte.ruta', N'descripcion') IS NULL ALTER TABLE transporte.ruta ADD descripcion nvarchar(500) NULL;
        IF COL_LENGTH(N'transporte.ruta', N'color_hex') IS NULL ALTER TABLE transporte.ruta ADD color_hex char(7) NULL;
        IF COL_LENGTH(N'transporte.ruta', N'creado_por') IS NULL ALTER TABLE transporte.ruta ADD creado_por int NULL;
        IF COL_LENGTH(N'transporte.ruta', N'direccion_ip') IS NULL ALTER TABLE transporte.ruta ADD direccion_ip varchar(64) NULL;
        IF COL_LENGTH(N'transporte.ruta', N'fecha_creacion') IS NULL ALTER TABLE transporte.ruta ADD fecha_creacion datetime2(3) NULL;
        IF COL_LENGTH(N'transporte.ruta', N'fecha_actualizacion') IS NULL ALTER TABLE transporte.ruta ADD fecha_actualizacion datetime2(3) NULL;
        """
    )
    op.execute(
        """
        UPDATE transporte.ruta SET codigo=COALESCE(codigo,CONCAT(N'RUTA-',id_ruta)),descripcion=COALESCE(descripcion,nombre),color_hex=COALESCE(color_hex,'#CBD5E1'),creado_por=COALESCE(creado_por,1),direccion_ip=COALESCE(direccion_ip,'MIGRACION'),fecha_creacion=COALESCE(fecha_creacion,SYSUTCDATETIME()),fecha_actualizacion=COALESCE(fecha_actualizacion,SYSUTCDATETIME());
        ALTER TABLE transporte.ruta ALTER COLUMN codigo nvarchar(50) NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN descripcion nvarchar(500) NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN color_hex char(7) NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN creado_por int NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN direccion_ip varchar(64) NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN fecha_creacion datetime2(3) NOT NULL;
        ALTER TABLE transporte.ruta ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'transporte.ruta') AND name=N'UQ_transporte_ruta_codigo') ALTER TABLE transporte.ruta ADD CONSTRAINT UQ_transporte_ruta_codigo UNIQUE(codigo);
        IF COL_LENGTH(N'transporte.ruta',N'nombre') IS NOT NULL ALTER TABLE transporte.ruta DROP COLUMN nombre;
        IF COL_LENGTH(N'transporte.ruta',N'activa') IS NOT NULL ALTER TABLE transporte.ruta DROP COLUMN activa;
        """
    )
    op.execute(
        """
        IF OBJECT_ID(N'transporte.asignacion_ruta',N'U') IS NULL CREATE TABLE transporte.asignacion_ruta(id_asignacion bigint IDENTITY PRIMARY KEY,id_ruta int NOT NULL,id_estudiante int NOT NULL,activa bit NOT NULL DEFAULT 1,fecha_creacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT FK_transporte_asignacion_ruta FOREIGN KEY(id_ruta) REFERENCES transporte.ruta(id_ruta),CONSTRAINT UQ_transporte_asignacion_estudiante UNIQUE(id_estudiante));
        INSERT INTO transporte.asignacion_ruta(id_ruta,id_estudiante) SELECT e.id_ruta,e.id_estudiante FROM estudiantes.estudiante e WHERE e.id_ruta IS NOT NULL AND NOT EXISTS(SELECT 1 FROM transporte.asignacion_ruta a WHERE a.id_estudiante=e.id_estudiante);

        """
    )
    op.execute(
        """
        IF COL_LENGTH(N'estudiantes.estudiante',N'creado_por') IS NULL ALTER TABLE estudiantes.estudiante ADD creado_por int NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'actualizado_por') IS NULL ALTER TABLE estudiantes.estudiante ADD actualizado_por int NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'direccion_ip') IS NULL ALTER TABLE estudiantes.estudiante ADD direccion_ip varchar(64) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'fecha_creacion') IS NULL ALTER TABLE estudiantes.estudiante ADD fecha_creacion datetime2(3) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'fecha_actualizacion') IS NULL ALTER TABLE estudiantes.estudiante ADD fecha_actualizacion datetime2(3) NULL;
        """
    )
    # Separar las altas de columnas del UPDATE. SQL Server compila un lote
    # completo antes de ejecutarlo y no reconoce columnas agregadas dentro del
    # mismo lote; los respaldos productivos antiguos llegan sin el contrato
    # canónico de estudiantes.
    op.execute(
        """
        IF COL_LENGTH(N'estudiantes.estudiante',N'carne') IS NULL ALTER TABLE estudiantes.estudiante ADD carne nvarchar(30) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'nombre') IS NULL ALTER TABLE estudiantes.estudiante ADD nombre nvarchar(100) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'primer_apellido') IS NULL ALTER TABLE estudiantes.estudiante ADD primer_apellido nvarchar(100) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'segundo_apellido') IS NULL ALTER TABLE estudiantes.estudiante ADD segundo_apellido nvarchar(100) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'cedula') IS NULL ALTER TABLE estudiantes.estudiante ADD cedula nvarchar(30) NULL;
        IF COL_LENGTH(N'estudiantes.estudiante',N'activo') IS NULL ALTER TABLE estudiantes.estudiante ADD activo bit NOT NULL CONSTRAINT DF_estudiante_activo_0017 DEFAULT 1;
        """
    )
    op.execute(
        """
        UPDATE e SET carne=COALESCE(NULLIF(carne,N''),identificacion),nombre=COALESCE(NULLIF(nombre,N''),LEFT(nombre_completo,100)),primer_apellido=COALESCE(NULLIF(primer_apellido,N''),N'Sin apellido'),creado_por=COALESCE(creado_por,1),direccion_ip=COALESCE(direccion_ip,'MIGRACION'),fecha_creacion=COALESCE(fecha_creacion,SYSUTCDATETIME()),fecha_actualizacion=COALESCE(fecha_actualizacion,SYSUTCDATETIME()) FROM estudiantes.estudiante e;
        UPDATE e SET nombre=N'Sin nombre' FROM estudiantes.estudiante e WHERE e.nombre IS NULL OR e.nombre=N'';
        UPDATE e SET carne=CONCAT(N'LEGACY-',e.id_estudiante) FROM estudiantes.estudiante e WHERE e.carne IS NULL OR e.carne=N'';
        ALTER TABLE estudiantes.estudiante ALTER COLUMN carne nvarchar(30) NOT NULL;
        ALTER TABLE estudiantes.estudiante ALTER COLUMN nombre nvarchar(100) NOT NULL;
        ALTER TABLE estudiantes.estudiante ALTER COLUMN primer_apellido nvarchar(100) NOT NULL;
        ALTER TABLE estudiantes.estudiante ALTER COLUMN fecha_creacion datetime2(3) NOT NULL;
        ALTER TABLE estudiantes.estudiante ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'estudiantes.estudiante') AND name=N'UQ_estudiantes_estudiante_carne') ALTER TABLE estudiantes.estudiante ADD CONSTRAINT UQ_estudiantes_estudiante_carne UNIQUE(carne);
        IF COL_LENGTH(N'estudiantes.estudiante',N'identificacion') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN identificacion;
        IF COL_LENGTH(N'estudiantes.estudiante',N'nombre_completo') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN nombre_completo;
        IF COL_LENGTH(N'estudiantes.estudiante',N'id_ruta') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN id_ruta;

        """
    )
    op.execute(
        """
        IF COL_LENGTH(N'asistencia.marca',N'fecha') IS NULL ALTER TABLE asistencia.marca ADD fecha date NULL;
        IF COL_LENGTH(N'asistencia.marca',N'observacion') IS NULL ALTER TABLE asistencia.marca ADD observacion nvarchar(500) NULL;
        IF COL_LENGTH(N'asistencia.marca',N'corregida') IS NULL ALTER TABLE asistencia.marca ADD corregida bit NOT NULL DEFAULT 0;
        IF COL_LENGTH(N'asistencia.marca',N'creado_por') IS NULL ALTER TABLE asistencia.marca ADD creado_por int NULL;
        IF COL_LENGTH(N'asistencia.marca',N'actualizado_por') IS NULL ALTER TABLE asistencia.marca ADD actualizado_por int NULL;
        IF COL_LENGTH(N'asistencia.marca',N'direccion_ip') IS NULL ALTER TABLE asistencia.marca ADD direccion_ip varchar(64) NULL;
        IF COL_LENGTH(N'asistencia.marca',N'fecha_creacion') IS NULL ALTER TABLE asistencia.marca ADD fecha_creacion datetime2(3) NULL;
        IF COL_LENGTH(N'asistencia.marca',N'fecha_actualizacion') IS NULL ALTER TABLE asistencia.marca ADD fecha_actualizacion datetime2(3) NULL;
        """
    )
    op.execute(
        """
        UPDATE m SET fecha=COALESCE(fecha,CONVERT(date,fecha_hora)),estado=CASE estado WHEN 'confirmada' THEN 'presente' WHEN 'cancelada' THEN 'ausente' ELSE estado END,creado_por=COALESCE(creado_por,1),direccion_ip=COALESCE(direccion_ip,'MIGRACION'),fecha_creacion=COALESCE(fecha_creacion,fecha_hora),fecha_actualizacion=COALESCE(fecha_actualizacion,fecha_hora) FROM asistencia.marca m;
        ;WITH d AS (SELECT id_marca,ROW_NUMBER() OVER(PARTITION BY id_estudiante,fecha ORDER BY id_marca DESC) n FROM asistencia.marca) DELETE FROM asistencia.marca WHERE id_marca IN(SELECT id_marca FROM d WHERE n>1);
        UPDATE asistencia.marca SET estado='ausente' WHERE estado NOT IN('presente','ausente','tardanza','justificada');
        ALTER TABLE asistencia.marca ALTER COLUMN fecha date NOT NULL;
        ALTER TABLE asistencia.marca ALTER COLUMN creado_por int NOT NULL;
        ALTER TABLE asistencia.marca ALTER COLUMN direccion_ip varchar(64) NOT NULL;
        ALTER TABLE asistencia.marca ALTER COLUMN fecha_creacion datetime2(3) NOT NULL;
        ALTER TABLE asistencia.marca ALTER COLUMN fecha_actualizacion datetime2(3) NOT NULL;
        IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'asistencia.marca') AND name=N'UQ_asistencia_marca_estudiante_fecha') ALTER TABLE asistencia.marca ADD CONSTRAINT UQ_asistencia_marca_estudiante_fecha UNIQUE(id_estudiante,fecha);
        IF COL_LENGTH(N'asistencia.marca',N'fecha_hora') IS NOT NULL ALTER TABLE asistencia.marca DROP COLUMN fecha_hora;
        IF OBJECT_ID(N'asistencia.correccion',N'U') IS NULL CREATE TABLE asistencia.correccion(id_correccion bigint IDENTITY PRIMARY KEY,id_marca bigint NOT NULL,motivo nvarchar(500) NOT NULL,id_usuario int NOT NULL,direccion_ip varchar(64) NOT NULL,fecha_correccion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),CONSTRAINT FK_asistencia_correccion_marca FOREIGN KEY(id_marca) REFERENCES asistencia.marca(id_marca));

        """
    )
    op.execute(
        """
        IF OBJECT_ID(N'beneficios.tipo_beneficio',N'U') IS NULL CREATE TABLE beneficios.tipo_beneficio(id_beneficio int IDENTITY PRIMARY KEY,nombre nvarchar(100) NOT NULL,descripcion nvarchar(500) NULL,dias_permitidos tinyint NOT NULL DEFAULT 5,activo bit NOT NULL DEFAULT 1,creado_por int NULL,actualizado_por int NULL,direccion_ip varchar(64) NULL,fecha_creacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),fecha_actualizacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME());
        IF OBJECT_ID(N'beneficios.beneficio',N'U') IS NOT NULL
        BEGIN
            SET IDENTITY_INSERT beneficios.tipo_beneficio ON;
            EXEC(N'INSERT INTO beneficios.tipo_beneficio(id_beneficio,nombre,activo) SELECT b.id_beneficio,b.nombre,b.activo FROM beneficios.beneficio b WHERE NOT EXISTS(SELECT 1 FROM beneficios.tipo_beneficio t WHERE t.id_beneficio=b.id_beneficio)');
            SET IDENTITY_INSERT beneficios.tipo_beneficio OFF;
        END;
        IF OBJECT_ID(N'beneficios.asignacion',N'U') IS NULL CREATE TABLE beneficios.asignacion(id_estudiante int PRIMARY KEY,id_beneficio int NULL,creado_por int NULL,actualizado_por int NULL,direccion_ip varchar(64) NULL,fecha_creacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),fecha_actualizacion datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME());
        IF OBJECT_ID(N'beneficios.beneficio_estudiante',N'U') IS NOT NULL
            EXEC(N'INSERT INTO beneficios.asignacion(id_estudiante,id_beneficio) SELECT be.id_estudiante,be.id_beneficio FROM beneficios.beneficio_estudiante be WHERE NOT EXISTS(SELECT 1 FROM beneficios.asignacion a WHERE a.id_estudiante=be.id_estudiante)');
        IF NOT EXISTS(SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_beneficios_asignacion_beneficio') ALTER TABLE beneficios.asignacion ADD CONSTRAINT FK_beneficios_asignacion_beneficio FOREIGN KEY(id_beneficio) REFERENCES beneficios.tipo_beneficio(id_beneficio);
        DECLARE @sql nvarchar(max)=N'';
        SELECT @sql=@sql+N'ALTER TABLE '+QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id))+N'.'+QUOTENAME(OBJECT_NAME(parent_object_id))+N' DROP CONSTRAINT '+QUOTENAME(name)+N';' FROM sys.foreign_keys WHERE referenced_object_id IN(OBJECT_ID(N'beneficios.beneficio'),OBJECT_ID(N'beneficios.beneficio_estudiante'));
        IF @sql<>N'' EXEC sp_executesql @sql;
        IF OBJECT_ID(N'beneficios.beneficio_estudiante',N'U') IS NOT NULL DROP TABLE beneficios.beneficio_estudiante;
        IF OBJECT_ID(N'beneficios.beneficio',N'U') IS NOT NULL DROP TABLE beneficios.beneficio;
        IF COL_LENGTH(N'estudiantes.estudiante',N'id_beneficio') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN id_beneficio;
        """
    )


def downgrade() -> None:
    raise RuntimeError("La migración total de datos no admite reversión destructiva")
