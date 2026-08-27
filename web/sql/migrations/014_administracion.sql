/* Catálogo canónico de administración web. Repetible e independiente de WinForms. */
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF SCHEMA_ID(N'identidad') IS NULL EXEC(N'CREATE SCHEMA identidad');
IF OBJECT_ID(N'identidad.rol', N'U') IS NULL
CREATE TABLE identidad.rol (
 id_rol int IDENTITY(1,1) NOT NULL CONSTRAINT PK_identidad_rol PRIMARY KEY,
 nombre nvarchar(100) NOT NULL CONSTRAINT UQ_identidad_rol_nombre UNIQUE,
 descripcion nvarchar(300) NULL, activo bit NOT NULL CONSTRAINT DF_identidad_rol_activo DEFAULT 1
);
IF OBJECT_ID(N'identidad.rol_permiso', N'U') IS NULL
CREATE TABLE identidad.rol_permiso (
 id_rol int NOT NULL, id_permiso int NOT NULL,
 CONSTRAINT PK_identidad_rol_permiso PRIMARY KEY(id_rol,id_permiso),
 CONSTRAINT FK_identidad_rol_permiso_rol FOREIGN KEY(id_rol) REFERENCES identidad.rol(id_rol) ON DELETE CASCADE,
 CONSTRAINT FK_identidad_rol_permiso_permiso FOREIGN KEY(id_permiso) REFERENCES identidad.permiso(id_permiso) ON DELETE CASCADE
);
IF OBJECT_ID(N'identidad.usuario_rol', N'U') IS NULL
CREATE TABLE identidad.usuario_rol (
 id_usuario int NOT NULL, id_rol int NOT NULL,
 CONSTRAINT PK_identidad_usuario_rol PRIMARY KEY(id_usuario,id_rol),
 CONSTRAINT FK_identidad_usuario_rol_usuario FOREIGN KEY(id_usuario) REFERENCES identidad.usuario(id_usuario) ON DELETE CASCADE,
 CONSTRAINT FK_identidad_usuario_rol_rol FOREIGN KEY(id_rol) REFERENCES identidad.rol(id_rol) ON DELETE CASCADE
);
IF NOT EXISTS (SELECT 1 FROM identidad.permiso WHERE clave=N'administracion.usuarios.leer') INSERT identidad.permiso(clave,descripcion) VALUES(N'administracion.usuarios.leer',N'Consultar usuarios y roles');
IF NOT EXISTS (SELECT 1 FROM identidad.permiso WHERE clave=N'administracion.usuarios.editar') INSERT identidad.permiso(clave,descripcion) VALUES(N'administracion.usuarios.editar',N'Crear y editar usuarios');
IF NOT EXISTS (SELECT 1 FROM identidad.permiso WHERE clave=N'administracion.permisos.editar') INSERT identidad.permiso(clave,descripcion) VALUES(N'administracion.permisos.editar',N'Administrar roles y permisos');
COMMIT TRANSACTION;
