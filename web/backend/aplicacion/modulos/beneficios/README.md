# Beneficios históricos

Este paquete conserva únicamente los modelos SQLAlchemy de las tablas históricas
`beneficios.tipo_beneficio` y `beneficios.asignacion`. Alembic los carga para poder
reconocer y reconciliar el esquema existente durante el corte de datos.

No existe API, servicio ni repositorio operativo de beneficios. La beca de comedor
se determina exclusivamente mediante `comedor.persona.id_estado_comedor`, cuyos
valores canónicos representan beneficiario completo y no beneficiario. Los días
permitidos, `TipoBeca` y las asignaciones históricas no participan en el acceso, el
dashboard ni las estadísticas del comedor.

Estas tablas solo podrán consultarse desde migraciones y procesos explícitos de
reconciliación hasta que se apruebe su eliminación física posterior al corte.
