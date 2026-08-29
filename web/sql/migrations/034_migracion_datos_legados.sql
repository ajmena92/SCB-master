/* Migración de datos legados. La ruta oficial es Alembic 0034. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF COL_LENGTH(N'estudiantes.estudiante',N'turno') IS NULL
    ALTER TABLE estudiantes.estudiante ADD turno nvarchar(30) NULL;

IF OBJECT_ID(N'comedor.migracion_ingreso_0034',N'U') IS NULL
    CREATE TABLE comedor.migracion_ingreso_0034(
        id_transaccion int NOT NULL CONSTRAINT PK_migracion_ingreso_0034 PRIMARY KEY,
        id_usuario int NOT NULL, fecha datetime NOT NULL, tipo_pago int NOT NULL,
        cantidad int NOT NULL, precio float NULL, tipo_usuario int NULL,
        beca int NOT NULL, modalidad varchar(12) NOT NULL,
        es_duplicado bit NOT NULL CONSTRAINT DF_migracion_ingreso_dup DEFAULT 0,
        migrado_en datetime2(3) NOT NULL CONSTRAINT DF_migracion_ingreso_fecha DEFAULT SYSUTCDATETIME());

SET IDENTITY_INSERT transporte.ruta ON;
INSERT transporte.ruta(id_ruta,codigo,descripcion,color_hex,activo,creado_por,direccion_ip,fecha_creacion,fecha_actualizacion)
SELECT IdRuta,LEFT(Codigo,50),LEFT(Descripcion,500),'#CBD5E1',Activo,1,'MIGRACION',SYSUTCDATETIME(),SYSUTCDATETIME()
FROM dbo.Ruta r WHERE NOT EXISTS(SELECT 1 FROM transporte.ruta x WHERE x.id_ruta=r.IdRuta);
SET IDENTITY_INSERT transporte.ruta OFF;

SET IDENTITY_INSERT estudiantes.estudiante ON;
INSERT estudiantes.estudiante(id_estudiante,carne,nombre,primer_apellido,segundo_apellido,cedula,seccion,turno,hash_contrasena,debe_cambiar_pin,fecha_expiracion_pin,fecha_creacion,fecha_actualizacion)
SELECT u.IdUsuario,LEFT(CASE WHEN NULLIF(LTRIM(RTRIM(u.Cedula)),'') IS NULL THEN CONCAT('LEGACY-',u.IdUsuario) ELSE u.Cedula END,30),LEFT(u.Nombre,100),LEFT(u.PrimerApellido,100),NULLIF(LEFT(u.SegundoApellido,100),''),NULLIF(LEFT(u.Cedula,30),''),NULLIF(LEFT(u.Seccion,30),''),LEFT(COALESCE(ho.codigo,h.Descripcion),30),NULL,COALESCE(c.DebeCambiarPin,1),NULL,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM dbo.Usuario u LEFT JOIN dbo.Horario h ON h.IdHorario=u.IdHorario LEFT JOIN comedor.horario_operacion ho ON ho.id_horario_origen=u.IdHorario LEFT JOIN ComedorPortal.CredencialEstudiante c ON c.IdUsuario=u.IdUsuario
WHERE u.CodTipo=1 AND NOT EXISTS(SELECT 1 FROM estudiantes.estudiante e WHERE e.id_estudiante=u.IdUsuario);
SET IDENTITY_INSERT estudiantes.estudiante OFF;

INSERT transporte.asignacion_ruta(id_ruta,id_estudiante,activa)
SELECT u.IdRuta,u.IdUsuario,CONVERT(bit,u.Activo) FROM dbo.Usuario u
WHERE u.CodTipo=1 AND NOT EXISTS(SELECT 1 FROM transporte.asignacion_ruta a WHERE a.id_estudiante=u.IdUsuario);

INSERT transporte.uso_diario(id_estudiante,fecha,marcado_en)
SELECT r.IdUsuario,CONVERT(date,r.Fecha),MIN(r.Fecha) FROM dbo.RegistroTransporte r
WHERE NOT EXISTS(SELECT 1 FROM transporte.uso_diario x WHERE x.id_estudiante=r.IdUsuario AND x.fecha=CONVERT(date,r.Fecha))
GROUP BY r.IdUsuario,CONVERT(date,r.Fecha);

SET IDENTITY_INSERT identidad.usuario ON;
INSERT identidad.usuario(id_usuario,nombre_usuario,hash_contrasena,activo,fecha_creacion,fecha_actualizacion)
SELECT u.IdUsuario,LEFT(CONCAT('profesor-',u.Cedula),100),'$2b$12$J7mY8r4D6vM3KxQ2pL9nUeJq4sG6tP8wR1zC5bN7fH0aE3iO6uY5S',u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM dbo.Usuario u WHERE u.CodTipo=2 AND NOT EXISTS(SELECT 1 FROM identidad.usuario x WHERE x.id_usuario=u.IdUsuario);
SET IDENTITY_INSERT identidad.usuario OFF;

SET IDENTITY_INSERT comedor.persona ON;
INSERT comedor.persona(id_persona,tipo_persona,id_estudiante,id_usuario,codigo_barras,nombre_completo,estado_comedor,activo,creado_en,actualizado_en)
SELECT u.IdUsuario,'estudiante',u.IdUsuario,NULL,CONCAT('E-',e.carne),LEFT(CONCAT(e.nombre,N' ',e.primer_apellido,N' ',COALESCE(e.segundo_apellido,N'')),220),CASE WHEN u.TipoBeca=2 THEN 'becado_comedor' ELSE 'no_becado_comedor' END,u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM dbo.Usuario u JOIN estudiantes.estudiante e ON e.id_estudiante=u.IdUsuario
WHERE u.CodTipo=1 AND NOT EXISTS(SELECT 1 FROM comedor.persona p WHERE p.id_persona=u.IdUsuario);
INSERT comedor.persona(id_persona,tipo_persona,id_usuario,codigo_barras,nombre_completo,estado_comedor,activo,creado_en,actualizado_en)
SELECT u.IdUsuario,'profesor',u.IdUsuario,CONCAT('P-',u.Cedula),LEFT(CONCAT(u.Nombre,N' ',u.PrimerApellido,N' ',COALESCE(u.SegundoApellido,N'')),220),'no_becado_comedor',u.Activo,SYSUTCDATETIME(),SYSUTCDATETIME()
FROM dbo.Usuario u WHERE u.CodTipo=2 AND NOT EXISTS(SELECT 1 FROM comedor.persona p WHERE p.id_persona=u.IdUsuario);
SET IDENTITY_INSERT comedor.persona OFF;

INSERT comedor.cuenta_tiquetes(id_persona,saldo,reservados,actualizado_en)
SELECT p.id_persona,CASE WHEN u.CantidadTiquetes<0 THEN 0 ELSE u.CantidadTiquetes END,0,SYSUTCDATETIME()
FROM comedor.persona p JOIN dbo.Usuario u ON u.IdUsuario=p.id_persona
WHERE (p.tipo_persona='profesor' OR p.estado_comedor='no_becado_comedor')
  AND NOT EXISTS(SELECT 1 FROM comedor.cuenta_tiquetes c WHERE c.id_persona=p.id_persona);

INSERT comedor.movimiento_tiquetes(id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,reservados_nuevo,clave_idempotencia,concepto,creado_en)
SELECT c.id_cuenta,'ajuste',c.saldo,0,c.saldo,0,0,CONCAT('MIGRACION-0034-',c.id_cuenta),N'Saldo inicial trasladado desde dbo.Usuario',SYSUTCDATETIME()
FROM comedor.cuenta_tiquetes c WHERE c.saldo>0
  AND NOT EXISTS(SELECT 1 FROM comedor.movimiento_tiquetes m WHERE m.clave_idempotencia=CONCAT('MIGRACION-0034-',c.id_cuenta));

INSERT comedor.migracion_ingreso_0034(id_transaccion,id_usuario,fecha,tipo_pago,cantidad,precio,tipo_usuario,beca,modalidad,es_duplicado)
SELECT r.IdTransaccion,r.IdUsuario,r.Fecha,r.TipoPago,r.Cantidad,r.Precio,r.TipoUsuario,r.Beca,CASE WHEN p.estado_comedor='becado_comedor' THEN 'beca' ELSE 'tiquete' END,CASE WHEN ROW_NUMBER() OVER(PARTITION BY r.IdUsuario,CONVERT(date,r.Fecha) ORDER BY r.Fecha,r.IdTransaccion)>1 THEN 1 ELSE 0 END
FROM dbo.RegistroComedor r JOIN comedor.persona p ON p.id_persona=r.IdUsuario
WHERE NOT EXISTS(SELECT 1 FROM comedor.migracion_ingreso_0034 m WHERE m.id_transaccion=r.IdTransaccion);

INSERT comedor.ingreso(id_persona,fecha,modalidad,codigo_horario,hora_marca,marca_transporte_existente,creado_en,hora_limite_aplicada,resultado,permitir_marca_tardia,permitir_sin_marca_transporte)
SELECT p.id_persona,CONVERT(date,r.Fecha),CASE WHEN p.estado_comedor='becado_comedor' THEN 'beca' ELSE 'tiquete' END,ho.codigo,r.Fecha,CASE WHEN EXISTS(SELECT 1 FROM transporte.uso_diario t WHERE t.id_estudiante=r.IdUsuario AND t.fecha=CONVERT(date,r.Fecha)) THEN 1 ELSE 0 END,r.Fecha,h.HoraLimite,'registrado',0,1
FROM dbo.RegistroComedor r JOIN comedor.persona p ON p.id_persona=r.IdUsuario JOIN dbo.Usuario u ON u.IdUsuario=r.IdUsuario LEFT JOIN dbo.Horario h ON h.IdHorario=u.IdHorario LEFT JOIN comedor.horario_operacion ho ON ho.id_horario_origen=u.IdHorario
WHERE NOT EXISTS(SELECT 1 FROM comedor.ingreso i WHERE i.id_persona=r.IdUsuario AND i.fecha=CONVERT(date,r.Fecha))
  AND NOT EXISTS(SELECT 1 FROM comedor.migracion_ingreso_0034 m WHERE m.id_transaccion=r.IdTransaccion AND m.es_duplicado=1);

INSERT asistencia.marca(id_estudiante,fecha,estado,observacion,corregida,creado_por,actualizado_por,direccion_ip,fecha_creacion,fecha_actualizacion)
SELECT c.IdUsuario,c.FechaServicio,CASE WHEN LOWER(c.Estado)='confirmada' THEN 'presente' ELSE 'ausente' END,c.MotivoCorreccion,0,COALESCE(c.IdUsuarioAdmin,1),c.IdUsuarioAdmin,'MIGRACION',COALESCE(c.FechaConfirmacion,SYSUTCDATETIME()),COALESCE(c.FechaConfirmacion,SYSUTCDATETIME())
FROM ComedorPortal.ConfirmacionAsistencia c JOIN estudiantes.estudiante e ON e.id_estudiante=c.IdUsuario
WHERE NOT EXISTS(SELECT 1 FROM asistencia.marca m WHERE m.id_estudiante=c.IdUsuario AND m.fecha=c.FechaServicio);

INSERT estudiantes.fotografia(id_estudiante,contenido,tipo_contenido)
SELECT f.IdUsuario,f.Contenido,LEFT(COALESCE(f.TipoMime,'image/jpeg'),80)
FROM ComedorPortal.FotoEstudiante f JOIN estudiantes.estudiante e ON e.id_estudiante=f.IdUsuario
WHERE f.Activa=1 AND NOT EXISTS(SELECT 1 FROM estudiantes.fotografia x WHERE x.id_estudiante=f.IdUsuario);

COMMIT TRANSACTION;
