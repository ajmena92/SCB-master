/*
  Dashboard operativo SCB (FrmPrincipal)
  Fecha: 2026-03-04
  Objetivo: consultas base para KPIs, comparativos, tendencia semanal y top rutas.

  Tablas usadas:
    - RegistroComedor
    - RegistroTransporte
    - Usuario
    - Ruta
*/

SET NOCOUNT ON;

DECLARE @FechaRef DATE = CAST(GETDATE() AS DATE);
DECLARE @FechaAyer DATE = DATEADD(DAY, -1, @FechaRef);
DECLARE @FechaIni7 DATE = DATEADD(DAY, -6, @FechaRef);

/* 1) Becados Comedor (hoy)
   Criterio funcional actual: marca en RegistroComedor con Beca = 1 */
SELECT COUNT(1) AS BecadosComedorHoy
FROM RegistroComedor
WHERE Beca = 1
  AND Fecha >= @FechaRef
  AND Fecha < DATEADD(DAY, 1, @FechaRef);

/* 2) Becados/beneficiarios Transporte (hoy)
   Criterio funcional vigente en reportes legacy: Usuario.IdRuta <> 1 */
SELECT COUNT(DISTINCT RT.IdUsuario) AS BecadosTransporteHoy
FROM RegistroTransporte RT
INNER JOIN Usuario U ON U.IdUsuario = RT.IdUsuario
WHERE ISNULL(U.IdRuta, 1) <> 1
  AND RT.Fecha >= @FechaRef
  AND RT.Fecha < DATEADD(DAY, 1, @FechaRef);

/* 3) Marcas Comedor (hoy / ayer) */
SELECT
    SUM(CASE WHEN Fecha >= @FechaRef  AND Fecha < DATEADD(DAY, 1, @FechaRef)  THEN 1 ELSE 0 END) AS MarcasComedorHoy,
    SUM(CASE WHEN Fecha >= @FechaAyer AND Fecha < DATEADD(DAY, 1, @FechaAyer) THEN 1 ELSE 0 END) AS MarcasComedorAyer
FROM RegistroComedor;

/* 4) Marcas Transporte (hoy / ayer) */
SELECT
    SUM(CASE WHEN Fecha >= @FechaRef  AND Fecha < DATEADD(DAY, 1, @FechaRef)  THEN 1 ELSE 0 END) AS MarcasTransporteHoy,
    SUM(CASE WHEN Fecha >= @FechaAyer AND Fecha < DATEADD(DAY, 1, @FechaAyer) THEN 1 ELSE 0 END) AS MarcasTransporteAyer
FROM RegistroTransporte;

/* 5) Tendencia 7 dias (Comedor vs Transporte) */
SELECT
    CAST(F.Fecha AS DATE) AS Dia,
    SUM(F.Comedor) AS Comedor,
    SUM(F.Transporte) AS Transporte
FROM (
    SELECT CAST(Fecha AS DATE) AS Fecha, COUNT(1) AS Comedor, 0 AS Transporte
    FROM RegistroComedor
    WHERE Fecha >= @FechaIni7
      AND Fecha < DATEADD(DAY, 1, @FechaRef)
    GROUP BY CAST(Fecha AS DATE)

    UNION ALL

    SELECT CAST(Fecha AS DATE) AS Fecha, 0 AS Comedor, COUNT(1) AS Transporte
    FROM RegistroTransporte
    WHERE Fecha >= @FechaIni7
      AND Fecha < DATEADD(DAY, 1, @FechaRef)
    GROUP BY CAST(Fecha AS DATE)
) F
GROUP BY CAST(F.Fecha AS DATE)
ORDER BY Dia;

/* 6) Top 5 rutas por marcas de transporte (hoy) */
SELECT TOP 5
    R.Descripcion,
    COUNT(1) AS Total
FROM RegistroTransporte RT
INNER JOIN Ruta R ON R.IdRuta = RT.IdRuta
WHERE RT.Fecha >= @FechaRef
  AND RT.Fecha < DATEADD(DAY, 1, @FechaRef)
GROUP BY R.Descripcion
ORDER BY Total DESC;

/* Notas:
   - Si deseas "becados transporte" por TipoBeca, cambiar criterio de consulta #2 a:
       ISNULL(U.TipoBeca, 1) <> 1
   - Para mejor rendimiento en dashboard:
       INDEX sugerido en RegistroComedor(Fecha), RegistroTransporte(Fecha, IdRuta, IdUsuario)
*/
