/*
  Local development seed only. Source: docs/menu_escolar_por_semanas.md.
  Safe to rerun: it upserts the 25 weekly slots and five fixed 2026 dates,
  then replaces only their component lists. It never touches attendance,
  credentials, desktop tables, or production.

  "Conclusión del año estudiantil" is deliberately not imported: the source
  does not provide a calendar date. Dates such as 2026-06-28 are retained as
  explicit substitutions even when they fall on a weekend; the schema permits
  date-specific overrides independently of the Monday-Friday templates.
*/
SET XACT_ABORT ON;
SET NOCOUNT ON;
BEGIN TRANSACTION;

DECLARE @IdUsuarioModifica int = (SELECT TOP (1) IdUsuario FROM Seguridad.Usuario WHERE NombreUsuario = N'profe');
IF @IdUsuarioModifica IS NULL SET @IdUsuarioModifica = (SELECT MIN(IdUsuario) FROM Seguridad.Usuario);
IF @IdUsuarioModifica IS NULL THROW 50001, 'No existe un usuario administrativo local para auditar la importación.', 1;

DECLARE @Plantillas TABLE (SemanaMes tinyint, DiaSemana tinyint, Titulo nvarchar(150), Acompanamientos nvarchar(500), Fruta nvarchar(150));
INSERT INTO @Plantillas VALUES
(1,1,N'Papas con pollo',N'Arroz blanco, frijoles negros y ensalada de repollo blanco, repollo morado y tomate',N'Fruta de temporada o disponible según proveedor'),
(1,2,N'Pasta corta —tornillitos, coditos o plumas— con carne molida de res en salsa de tomate',N'Ensalada de garbanzo, pepino, lechuga y chile dulce',N'Fruta de temporada o disponible según proveedor'),
(1,3,N'Sopa negra con huevo duro',N'Arroz jardinero',N'Sandía'),
(1,4,N'Muslo de pollo deshuesado en salsa caribeña',N'Arroz blanco con frijoles rojos o gallo pinto; ensalada de repollo blanco, zanahoria y culantro',N'Fruta de temporada o disponible según proveedor'),
(1,5,N'Frijoles tiernos con cerdo',N'Arroz blanco y pico de gallo',N'Fruta de temporada o disponible según proveedor'),
(2,1,N'Espagueti con atún, zanahoria rallada y zucchini o chayote en salsa de tomate',NULL,N'Fruta de temporada o disponible según proveedor'),
(2,2,N'Arroz con pollo',N'Frijoles negros molidos y ensalada de repollo blanco, tomate y culantro',N'Fruta de temporada o disponible según proveedor'),
(2,3,N'Garbanzos con res, plátano verde o papa y chayote',N'Arroz blanco; ensalada de remolacha y zanahoria cocida en cuadritos, con aderezo de yogur y mostaza miel',N'Fruta de temporada o disponible según proveedor'),
(2,4,N'Sopa de pollo con vegetales',N'Arroz blanco',N'Fruta de temporada o disponible según proveedor'),
(2,5,N'Filete de pescado empanizado',N'Arroz blanco, frijoles rojos y ensalada de lechuga y tomate',N'Fruta de temporada o disponible según proveedor'),
(3,1,N'Fajitas de cerdo',N'Arroz blanco, frijoles rojos y ensalada de repollo blanco, zanahoria y maíz dulce',N'Fruta de temporada o disponible según proveedor'),
(3,2,N'Pastel o tortas de yuca con carne molida y queso',N'Arroz blanco, frijoles negros arreglados y ensalada de lechuga, tomate y culantro',N'Fruta de temporada o disponible según proveedor'),
(3,3,N'Pasta corta —cabitos, coditos o plumas— con vegetales y pollo',N'Frijoles negros',N'Banano'),
(3,4,N'Estofado de trocitos de res con vainica y plátano o banano verde en salsa de tomate',N'Arroz blanco, frijoles negros y guarnición de zucchini, coliflor y zanahoria',N'Fruta de temporada o disponible según proveedor'),
(3,5,N'Arroz con atún y vegetales',N'Frijoles negros molidos y ensalada de repollo blanco, repollo morado y pepino',N'Fruta de temporada o disponible según proveedor'),
(4,1,N'Frijoles blancos o lentejas con pollo',N'Arroz blanco',N'Fruta de temporada o disponible según proveedor'),
(4,2,N'Posta de cerdo en salsa criolla con papas',N'Arroz blanco, frijoles negros y ensalada de repollo blanco y zanahoria',N'Fruta de temporada o disponible según proveedor'),
(4,3,N'Filete de pescado empanizado con limón',N'Arroz blanco, frijoles rojos y ensalada de lechuga y tomate',N'Fruta de temporada o disponible según proveedor'),
(4,4,N'Arroz mixto de pollo, cerdo y huevo con cebollino y zanahoria',N'Frijoles negros molidos, ensalada de pepino y guacamole',N'Fruta de temporada o disponible según proveedor'),
(4,5,N'Olla de carne',N'Arroz blanco',N'Fruta de temporada o disponible según proveedor'),
(5,1,N'Arroz con cerdo y vegetales',N'Frijoles negros y ceviche de chayote tierno',N'Fruta de temporada o disponible según proveedor'),
(5,2,N'Vegetales con pollo',N'Arroz blanco, frijoles negros y ensalada de lechuga, pepino y tomate',N'Fruta de temporada o disponible según proveedor'),
(5,3,N'Picadillo de papa o plátano verde con frijoles blancos y carne de res mechada',N'Arroz blanco, frijoles negros y guarnición de brócoli, zucchini y chile dulce',N'Fruta de temporada o disponible según proveedor'),
(5,4,N'Opción 1: Canelones al horno con queso y espinaca en salsa de tomate. Opción 2: Espagueti con queso y espinaca en salsa de tomate',N'Arroz blanco; ensalada de lechuga, repollo blanco y maíz dulce; vinagreta de mango',N'Fruta de temporada o disponible según proveedor'),
(5,5,N'Trocitos de res con zanahoria en salsa criolla',N'Arroz blanco, frijoles rojos y caracolitos con atún',N'Fruta de temporada o disponible según proveedor');

MERGE ComedorPortal.MenuPlantilla AS target
USING @Plantillas AS src ON target.SemanaMes=src.SemanaMes AND target.DiaSemana=src.DiaSemana
WHEN MATCHED THEN UPDATE SET Titulo=src.Titulo, Observaciones=N'Importado de docs/menu_escolar_por_semanas.md', Activo=1, IdUsuarioModifica=@IdUsuarioModifica, FechaModificacion=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(SemanaMes,DiaSemana,Titulo,Observaciones,Activo,IdUsuarioModifica) VALUES(src.SemanaMes,src.DiaSemana,src.Titulo,N'Importado de docs/menu_escolar_por_semanas.md',1,@IdUsuarioModifica);

DECLARE @Semana tinyint, @Dia tinyint, @Titulo nvarchar(150), @Acompanamientos nvarchar(500), @Fruta nvarchar(150), @IdPlantilla int;
DECLARE plantillas_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT SemanaMes,DiaSemana,Titulo,Acompanamientos,Fruta FROM @Plantillas;
OPEN plantillas_cursor;
FETCH NEXT FROM plantillas_cursor INTO @Semana,@Dia,@Titulo,@Acompanamientos,@Fruta;
WHILE @@FETCH_STATUS=0
BEGIN
  SELECT @IdPlantilla=IdMenuPlantilla FROM ComedorPortal.MenuPlantilla WHERE SemanaMes=@Semana AND DiaSemana=@Dia;
  DELETE FROM ComedorPortal.MenuComponente WHERE IdMenuPlantilla=@IdPlantilla;
  INSERT INTO ComedorPortal.MenuComponente(IdMenuPlantilla,Orden,Nombre,TipoComponente) VALUES(@IdPlantilla,1,@Titulo,N'Principal');
  IF @Acompanamientos IS NOT NULL INSERT INTO ComedorPortal.MenuComponente(IdMenuPlantilla,Orden,Nombre,TipoComponente) VALUES(@IdPlantilla,2,@Acompanamientos,N'Acompañamiento');
  INSERT INTO ComedorPortal.MenuComponente(IdMenuPlantilla,Orden,Nombre,TipoComponente) VALUES(@IdPlantilla,CASE WHEN @Acompanamientos IS NULL THEN 2 ELSE 3 END,@Fruta,N'Postre');
  INSERT INTO ComedorPortal.MenuComponente(IdMenuPlantilla,Orden,Nombre,TipoComponente) VALUES(@IdPlantilla,CASE WHEN @Acompanamientos IS NULL THEN 3 ELSE 4 END,N'Agua pura',N'Bebida');
  FETCH NEXT FROM plantillas_cursor INTO @Semana,@Dia,@Titulo,@Acompanamientos,@Fruta;
END
CLOSE plantillas_cursor; DEALLOCATE plantillas_cursor;

DECLARE @Sustituciones TABLE (Fecha date, Titulo nvarchar(150), Acompanamientos nvarchar(500));
INSERT INTO @Sustituciones VALUES
(CONVERT(date,'20260628',112),N'Nachos',NULL),
(CONVERT(date,'20260725',112),N'Arroz guacho con cerdo',NULL),
(CONVERT(date,'20260909',112),N'Hamburguesa con torta de carne casera',NULL),
(CONVERT(date,'20260915',112),N'Carne mechada en salsa',N'Arroz, frijoles, plátano maduro y ensalada de palmito'),
(CONVERT(date,'20261012',112),N'Vigorón especial',NULL);

MERGE ComedorPortal.MenuSustitucion AS target
USING @Sustituciones AS src ON target.Fecha=src.Fecha
WHEN MATCHED THEN UPDATE SET Titulo=src.Titulo, Observaciones=N'Importado de docs/menu_escolar_por_semanas.md', IdUsuarioModifica=@IdUsuarioModifica, FechaModificacion=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(Fecha,Titulo,Observaciones,IdUsuarioModifica) VALUES(src.Fecha,src.Titulo,N'Importado de docs/menu_escolar_por_semanas.md',@IdUsuarioModifica);

DECLARE @Fecha date, @IdSustitucion int;
DECLARE sustituciones_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT Fecha,Titulo,Acompanamientos FROM @Sustituciones;
OPEN sustituciones_cursor;
FETCH NEXT FROM sustituciones_cursor INTO @Fecha,@Titulo,@Acompanamientos;
WHILE @@FETCH_STATUS=0
BEGIN
  SELECT @IdSustitucion=IdMenuSustitucion FROM ComedorPortal.MenuSustitucion WHERE Fecha=@Fecha;
  DELETE FROM ComedorPortal.MenuSustitucionComponente WHERE IdMenuSustitucion=@IdSustitucion;
  INSERT INTO ComedorPortal.MenuSustitucionComponente(IdMenuSustitucion,Orden,Nombre,TipoComponente) VALUES(@IdSustitucion,1,@Titulo,N'Principal');
  IF @Acompanamientos IS NOT NULL INSERT INTO ComedorPortal.MenuSustitucionComponente(IdMenuSustitucion,Orden,Nombre,TipoComponente) VALUES(@IdSustitucion,2,@Acompanamientos,N'Acompañamiento');
  INSERT INTO ComedorPortal.MenuSustitucionComponente(IdMenuSustitucion,Orden,Nombre,TipoComponente) VALUES(@IdSustitucion,CASE WHEN @Acompanamientos IS NULL THEN 2 ELSE 3 END,N'Fruta de temporada o disponible según proveedor',N'Postre');
  INSERT INTO ComedorPortal.MenuSustitucionComponente(IdMenuSustitucion,Orden,Nombre,TipoComponente) VALUES(@IdSustitucion,CASE WHEN @Acompanamientos IS NULL THEN 3 ELSE 4 END,N'Agua pura',N'Bebida');
  FETCH NEXT FROM sustituciones_cursor INTO @Fecha,@Titulo,@Acompanamientos;
END
CLOSE sustituciones_cursor; DEALLOCATE sustituciones_cursor;

COMMIT TRANSACTION;
