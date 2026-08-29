"""Traslada los datos productivos legados al modelo canónico web."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0034_migracion_datos_legados"
down_revision: Union[str, None] = "0033_horarios_origen_comedor"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def _ejecutar(sql: str) -> None:
    op.get_bind().execute(sa.text(sql))


def upgrade() -> None:
    _ejecutar(
        """
        IF OBJECT_ID(N'estudiantes.estudiante',N'U') IS NOT NULL
           AND COL_LENGTH(N'estudiantes.estudiante',N'turno') IS NULL
            ALTER TABLE estudiantes.estudiante ADD turno nvarchar(30) NULL;
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'dbo.Usuario',N'U') IS NULL RETURN;
        IF OBJECT_ID(N'comedor.migracion_ingreso_0034',N'U') IS NULL
        CREATE TABLE comedor.migracion_ingreso_0034(
            id_transaccion INT NOT NULL CONSTRAINT PK_migracion_ingreso_0034 PRIMARY KEY,
            id_usuario INT NOT NULL, fecha DATETIME NOT NULL, tipo_pago INT NOT NULL,
            cantidad INT NOT NULL, precio FLOAT NULL, tipo_usuario INT NULL,
            beca INT NOT NULL, modalidad VARCHAR(12) NOT NULL,
            es_duplicado BIT NOT NULL CONSTRAINT DF_migracion_ingreso_dup DEFAULT 0,
            migrado_en DATETIME2(3) NOT NULL CONSTRAINT DF_migracion_ingreso_fecha DEFAULT SYSUTCDATETIME()
        );
        IF OBJECT_ID(N'comedor.reconciliacion_migracion',N'U') IS NOT NULL
        BEGIN
            INSERT INTO comedor.reconciliacion_migracion(tipo,clave,detalle,origen,resuelto,creado_en)
            SELECT 'cedula_ausente',CONCAT('usuario:',u.IdUsuario),N'Usuario estudiante sin cédula; se generó carnet LEGACY.',N'dbo.Usuario',0,SYSUTCDATETIME()
            FROM dbo.Usuario u WHERE u.CodTipo=1 AND NULLIF(LTRIM(RTRIM(u.Cedula)),'') IS NULL
              AND NOT EXISTS(SELECT 1 FROM comedor.reconciliacion_migracion r WHERE r.clave=CONCAT('usuario:',u.IdUsuario));
            INSERT INTO comedor.reconciliacion_migracion(tipo,clave,detalle,origen,resuelto,creado_en)
            SELECT 'ingreso_duplicado',CONCAT('ingreso:',r.IdUsuario,':',CONVERT(char(10),CONVERT(date,r.Fecha),23)),N'Múltiples registros legados para la misma persona y fecha; se conserva uno en ingreso y todos en el archivo de migración.',N'dbo.RegistroComedor',0,SYSUTCDATETIME()
            FROM dbo.RegistroComedor r GROUP BY r.IdUsuario,CONVERT(date,r.Fecha)
            HAVING COUNT(*)>1
              AND NOT EXISTS(SELECT 1 FROM comedor.reconciliacion_migracion x WHERE x.clave=CONCAT('ingreso:',r.IdUsuario,':',CONVERT(char(10),CONVERT(date,r.Fecha),23)));
        END;
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'dbo.Ruta',N'U') IS NOT NULL
        BEGIN
            SET IDENTITY_INSERT transporte.ruta ON;
            INSERT INTO transporte.ruta(id_ruta,codigo,descripcion,color_hex,activo,creado_por,direccion_ip,fecha_creacion,fecha_actualizacion)
            SELECT r.IdRuta,LEFT(r.Codigo,50),LEFT(r.Descripcion,500),'#CBD5E1',r.Activo,1,'MIGRACION',SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.Ruta r WHERE NOT EXISTS(SELECT 1 FROM transporte.ruta x WHERE x.id_ruta=r.IdRuta);
            SET IDENTITY_INSERT transporte.ruta OFF;
        END;
        IF OBJECT_ID(N'estudiantes.estudiante',N'U') IS NOT NULL
        BEGIN
            SET IDENTITY_INSERT estudiantes.estudiante ON;
            INSERT INTO estudiantes.estudiante(id_estudiante,carne,nombre,primer_apellido,segundo_apellido,cedula,seccion,turno,hash_contrasena,debe_cambiar_pin,fecha_expiracion_pin,fecha_creacion,fecha_actualizacion)
            SELECT u.IdUsuario,
                   LEFT(CASE WHEN NULLIF(LTRIM(RTRIM(u.Cedula)),'') IS NULL THEN CONCAT('LEGACY-',u.IdUsuario) ELSE u.Cedula END,30),
                   LEFT(u.Nombre,100),LEFT(u.PrimerApellido,100),NULLIF(LEFT(u.SegundoApellido,100),''),NULLIF(LEFT(u.Cedula,30),''),
                   NULLIF(LEFT(u.Seccion,30),''),LEFT(COALESCE(ho.codigo,h.Descripcion),30),
                   NULL,COALESCE(c.DebeCambiarPin,1),NULL,SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.Usuario u LEFT JOIN dbo.Horario h ON h.IdHorario=u.IdHorario
            LEFT JOIN comedor.horario_operacion ho ON ho.id_horario_origen=u.IdHorario
            LEFT JOIN ComedorPortal.CredencialEstudiante c ON c.IdUsuario=u.IdUsuario
            WHERE u.CodTipo=1 AND NOT EXISTS(SELECT 1 FROM estudiantes.estudiante e WHERE e.id_estudiante=u.IdUsuario);
            SET IDENTITY_INSERT estudiantes.estudiante OFF;
        END;
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'dbo.Usuario',N'U') IS NOT NULL
        BEGIN
            IF OBJECT_ID(N'transporte.asignacion_ruta',N'U') IS NOT NULL
                INSERT INTO transporte.asignacion_ruta(id_ruta,id_estudiante,activa)
                SELECT u.IdRuta,u.IdUsuario,CONVERT(bit,u.Activo) FROM dbo.Usuario u
                WHERE u.CodTipo=1 AND EXISTS(SELECT 1 FROM transporte.ruta r WHERE r.id_ruta=u.IdRuta)
                  AND NOT EXISTS(SELECT 1 FROM transporte.asignacion_ruta a WHERE a.id_estudiante=u.IdUsuario);
            IF OBJECT_ID(N'transporte.uso_diario',N'U') IS NOT NULL AND OBJECT_ID(N'dbo.RegistroTransporte',N'U') IS NOT NULL
                INSERT INTO transporte.uso_diario(id_estudiante,fecha,marcado_en)
                SELECT r.IdUsuario,CONVERT(date,r.Fecha),MIN(r.Fecha) FROM dbo.RegistroTransporte r
                INNER JOIN estudiantes.estudiante e ON e.id_estudiante=r.IdUsuario
                WHERE NOT EXISTS(SELECT 1 FROM transporte.uso_diario x WHERE x.id_estudiante=r.IdUsuario AND x.fecha=CONVERT(date,r.Fecha))
                GROUP BY r.IdUsuario,CONVERT(date,r.Fecha);
        END;
        IF OBJECT_ID(N'identidad.usuario',N'U') IS NOT NULL
        BEGIN
            SET IDENTITY_INSERT identidad.usuario ON;
            INSERT INTO identidad.usuario(id_usuario,nombre_usuario,hash_contrasena,activo,fecha_creacion,fecha_actualizacion)
            SELECT u.IdUsuario,LEFT(CONCAT('profesor-',u.Cedula),100),
                   '$2b$12$J7mY8r4D6vM3KxQ2pL9nUeJq4sG6tP8wR1zC5bN7fH0aE3iO6uY5S',u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.Usuario u WHERE u.CodTipo=2 AND NOT EXISTS(SELECT 1 FROM identidad.usuario x WHERE x.id_usuario=u.IdUsuario);
            SET IDENTITY_INSERT identidad.usuario OFF;
        END;
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'comedor.persona',N'U') IS NOT NULL
        BEGIN
            SET IDENTITY_INSERT comedor.persona ON;
            INSERT INTO comedor.persona(id_persona,tipo_persona,id_estudiante,id_usuario,codigo_barras,nombre_completo,colegio,estado_comedor,activo,creado_en,actualizado_en)
            SELECT u.IdUsuario,'estudiante',u.IdUsuario,NULL,CONCAT('E-',e.carne),LEFT(CONCAT(e.nombre,N' ',e.primer_apellido,N' ',COALESCE(e.segundo_apellido,N'')),220),NULL,
                   CASE WHEN u.TipoBeca=2 THEN 'becado_comedor' ELSE 'no_becado_comedor' END,u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.Usuario u INNER JOIN estudiantes.estudiante e ON e.id_estudiante=u.IdUsuario
            WHERE u.CodTipo=1 AND NOT EXISTS(SELECT 1 FROM comedor.persona p WHERE p.id_persona=u.IdUsuario);
            INSERT INTO comedor.persona(id_persona,tipo_persona,id_estudiante,id_usuario,codigo_barras,nombre_completo,colegio,estado_comedor,activo,creado_en,actualizado_en)
            SELECT u.IdUsuario,'profesor',NULL,u.IdUsuario,CONCAT('P-',u.Cedula),LEFT(CONCAT(u.Nombre,N' ',u.PrimerApellido,N' ',COALESCE(u.SegundoApellido,N'')),220),NULL,'no_becado_comedor',u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
            FROM dbo.Usuario u WHERE u.CodTipo=2 AND NOT EXISTS(SELECT 1 FROM comedor.persona p WHERE p.id_persona=u.IdUsuario);
            SET IDENTITY_INSERT comedor.persona OFF;
        END;
        IF OBJECT_ID(N'comedor.cuenta_tiquetes',N'U') IS NOT NULL
            INSERT INTO comedor.cuenta_tiquetes(id_persona,saldo,reservados,actualizado_en)
            SELECT p.id_persona,CASE WHEN u.CantidadTiquetes<0 THEN 0 ELSE u.CantidadTiquetes END,0,SYSUTCDATETIME()
            FROM comedor.persona p INNER JOIN dbo.Usuario u ON u.IdUsuario=p.id_persona
            WHERE (p.tipo_persona='profesor' OR p.estado_comedor='no_becado_comedor')
              AND NOT EXISTS(SELECT 1 FROM comedor.cuenta_tiquetes c WHERE c.id_persona=p.id_persona);
        IF OBJECT_ID(N'comedor.movimiento_tiquetes',N'U') IS NOT NULL
            INSERT INTO comedor.movimiento_tiquetes(id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,reservados_nuevo,clave_idempotencia,concepto,creado_en)
            SELECT c.id_cuenta,'ajuste',c.saldo,0,c.saldo,0,0,CONCAT('MIGRACION-0034-',c.id_cuenta),N'Saldo inicial trasladado desde dbo.Usuario',SYSUTCDATETIME()
            FROM comedor.cuenta_tiquetes c WHERE c.saldo>0
              AND NOT EXISTS(SELECT 1 FROM comedor.movimiento_tiquetes m WHERE m.clave_idempotencia=CONCAT('MIGRACION-0034-',c.id_cuenta));
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'dbo.RegistroComedor',N'U') IS NOT NULL
        BEGIN
            INSERT INTO comedor.migracion_ingreso_0034(id_transaccion,id_usuario,fecha,tipo_pago,cantidad,precio,tipo_usuario,beca,modalidad,es_duplicado)
            SELECT r.IdTransaccion,r.IdUsuario,r.Fecha,r.TipoPago,r.Cantidad,r.Precio,r.TipoUsuario,r.Beca,
                   CASE WHEN p.estado_comedor='becado_comedor' THEN 'beca' ELSE 'tiquete' END,
                   CASE WHEN ROW_NUMBER() OVER(PARTITION BY r.IdUsuario,CONVERT(date,r.Fecha) ORDER BY r.Fecha,r.IdTransaccion)>1 THEN 1 ELSE 0 END
            FROM dbo.RegistroComedor r INNER JOIN comedor.persona p ON p.id_persona=r.IdUsuario
            WHERE NOT EXISTS(SELECT 1 FROM comedor.migracion_ingreso_0034 m WHERE m.id_transaccion=r.IdTransaccion);
            IF OBJECT_ID(N'comedor.ingreso',N'U') IS NOT NULL
                INSERT INTO comedor.ingreso(id_persona,fecha,modalidad,codigo_horario,hora_marca,marca_transporte_existente,registrado_por,creado_en,hora_limite_aplicada,resultado,permitir_marca_tardia,permitir_sin_marca_transporte)
                SELECT p.id_persona,CONVERT(date,r.Fecha),CASE WHEN p.estado_comedor='becado_comedor' THEN 'beca' ELSE 'tiquete' END,
                       ho.codigo,r.Fecha,
                       CASE WHEN EXISTS(SELECT 1 FROM transporte.uso_diario t WHERE t.id_estudiante=r.IdUsuario AND t.fecha=CONVERT(date,r.Fecha)) THEN 1 ELSE 0 END,NULL,r.Fecha,h.HoraLimite,'registrado',0,1
                FROM dbo.RegistroComedor r INNER JOIN comedor.persona p ON p.id_persona=r.IdUsuario
                INNER JOIN dbo.Usuario u ON u.IdUsuario=r.IdUsuario LEFT JOIN dbo.Horario h ON h.IdHorario=u.IdHorario
                LEFT JOIN comedor.horario_operacion ho ON ho.id_horario_origen=u.IdHorario
                WHERE NOT EXISTS(SELECT 1 FROM comedor.ingreso i WHERE i.id_persona=r.IdUsuario AND i.fecha=CONVERT(date,r.Fecha))
                  AND NOT EXISTS(SELECT 1 FROM comedor.migracion_ingreso_0034 m WHERE m.id_transaccion=r.IdTransaccion AND m.es_duplicado=1);
        END;
        """
    )
    _ejecutar(
        """
        IF OBJECT_ID(N'ComedorPortal.ConfirmacionAsistencia',N'U') IS NOT NULL AND OBJECT_ID(N'asistencia.marca',N'U') IS NOT NULL
            INSERT INTO asistencia.marca(id_estudiante,fecha,estado,observacion,corregida,creado_por,actualizado_por,direccion_ip,fecha_creacion,fecha_actualizacion)
            SELECT c.IdUsuario,c.FechaServicio,CASE WHEN LOWER(c.Estado)='confirmada' THEN 'presente' ELSE 'ausente' END,c.MotivoCorreccion,0,COALESCE(c.IdUsuarioAdmin,1),c.IdUsuarioAdmin,'MIGRACION',COALESCE(c.FechaConfirmacion,SYSUTCDATETIME()),COALESCE(c.FechaConfirmacion,SYSUTCDATETIME())
            FROM ComedorPortal.ConfirmacionAsistencia c INNER JOIN estudiantes.estudiante e ON e.id_estudiante=c.IdUsuario
            WHERE NOT EXISTS(SELECT 1 FROM asistencia.marca m WHERE m.id_estudiante=c.IdUsuario AND m.fecha=c.FechaServicio);
        IF OBJECT_ID(N'ComedorPortal.FotoEstudiante',N'U') IS NOT NULL AND OBJECT_ID(N'estudiantes.fotografia',N'U') IS NOT NULL
            INSERT INTO estudiantes.fotografia(id_estudiante,contenido,tipo_contenido)
            SELECT f.IdUsuario,f.Contenido,LEFT(COALESCE(f.TipoMime,'image/jpeg'),80)
            FROM ComedorPortal.FotoEstudiante f INNER JOIN estudiantes.estudiante e ON e.id_estudiante=f.IdUsuario
            WHERE f.Activa=1 AND NOT EXISTS(SELECT 1 FROM estudiantes.fotografia x WHERE x.id_estudiante=f.IdUsuario);
        """
    )


def downgrade() -> None:
    raise RuntimeError("La migración de datos legados no admite reversión destructiva")
