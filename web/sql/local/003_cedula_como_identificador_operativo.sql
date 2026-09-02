-- Sustituye el código E-/P- por la cédula como identificador operativo.
-- Se detiene antes de modificar datos si el padrón no permite una migración segura.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM persona WHERE cedula IS NULL OR btrim(cedula) = '') THEN
        RAISE EXCEPTION 'No se puede migrar: existen personas sin cédula';
    END IF;
    IF EXISTS (
        SELECT cedula FROM persona GROUP BY cedula HAVING count(*) > 1
    ) THEN
        RAISE EXCEPTION 'No se puede migrar: existen cédulas repetidas';
    END IF;
END $$;

ALTER TABLE persona ALTER COLUMN codigo TYPE varchar(32);
UPDATE persona SET codigo = cedula WHERE codigo IS DISTINCT FROM cedula;
