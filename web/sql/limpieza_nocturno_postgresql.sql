-- Limpieza excepcional y auditable del padrón nocturno heredado.
-- Requiere un pg_dump verificado antes de ejecutarse. Es idempotente.
BEGIN;

CREATE TEMP TABLE personas_nocturnas ON COMMIT DROP AS
SELECT DISTINCT m.persona_id
FROM matricula m
WHERE m.turno = '2'
  AND NOT EXISTS (
    SELECT 1 FROM matricula d
    WHERE d.persona_id = m.persona_id AND d.turno <> '2'
  );

CREATE TEMP TABLE matriculas_nocturnas ON COMMIT DROP AS
SELECT id FROM matricula WHERE turno = '2';

DELETE FROM marca_transporte WHERE matricula_id IN (SELECT id FROM matriculas_nocturnas);
DELETE FROM asignacion_ruta WHERE matricula_id IN (SELECT id FROM matriculas_nocturnas);
DELETE FROM ingreso_comedor WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM autorizacion_comedor WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM reserva_comedor WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM venta_tiquete WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM movimiento_tiquete WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM cuenta_tiquete WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM sesion_acceso WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM credencial_portal WHERE persona_id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM matricula WHERE id IN (SELECT id FROM matriculas_nocturnas);
DELETE FROM persona WHERE id IN (SELECT persona_id FROM personas_nocturnas);
DELETE FROM horario_reserva WHERE turno = '2';

-- Rutas que el respaldo previo demostró exclusivas del nocturno y ya no tienen uso.
DELETE FROM ruta
WHERE codigo IN ('02', '08')
  AND NOT EXISTS (SELECT 1 FROM asignacion_ruta a WHERE a.ruta_id = ruta.id);

COMMIT;
