# Privacidad, retención y trazabilidad de fotografías personales

## Alcance

Las fotografías, identificaciones y documentos asociados a estudiantes o
familias son datos personales. No se almacenan en Git, artefactos de CI,
capturas de prueba, directorios temporales compartidos ni registros de la API.

## Almacenamiento y acceso

- La plataforma conserva la fotografía operativa en `fotografia_persona`, bajo
  la autoridad de PostgreSQL.
- La API entrega y modifica fotografías únicamente mediante endpoints protegidos
  por permisos; el portal solo accede a la fotografía de su propia sesión.
- Las fuentes de importación y reportes se conservan fuera del checkout, en
  almacenamiento privado cifrado y con permisos mínimos.
- Las copias de respaldo se cifran, tienen responsable asignado y no se usan
  como fuente de desarrollo ni de pruebas.

## Retención y eliminación

El responsable institucional de datos debe aprobar y registrar: plazo de
retención de fuentes de importación, plazo de respaldo, criterio de eliminación
de fotografías de personas inactivas y el tratamiento de respaldos al vencer
ese plazo. La eliminación operativa incluye la fuente privada, el almacenamiento
activo cuando proceda, respaldos conforme al ciclo aprobado y referencias Git.

## Trazabilidad

Las operaciones de cargar, reemplazar, consultar en contexto administrativo y
eliminar fotografías deben producir una traza con actor, fecha, operación,
persona afectada y resultado. La traza no contiene el binario, la fotografía ni
identificadores innecesarios. La revisión periódica valida permisos, retención,
respaldos y ausencia de datos personales en Git y CI.

## Respuesta ante exposición

1. Restringir distribución y conservar una copia cifrada bajo custodia.
2. Retirar la fuente del árbol actual y reescribir todas las referencias Git.
3. Revisar forks, clones, cachés, artefactos de CI, despliegues y respaldos.
4. Documentar elementos no eliminables, responsable, control de acceso y fecha
   de expiración.
