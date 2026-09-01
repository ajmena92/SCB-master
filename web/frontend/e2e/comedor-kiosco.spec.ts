import { expect, test } from "@playwright/test";

const sesionAdministrativa = {
  tipo: "administracion",
  cuentaId: 977,
  personaId: 10,
  usuario: "operador.prueba",
  nombres: "Operador de prueba",
  rol: "operador",
  permisos: ["comedor.operar"],
  cambioContrasenaObligatorio: false,
  vinculacionPendiente: false,
};

test.beforeEach(async ({ page }) => {
  await page.route("**/api/v1/sesion", (ruta) =>
    ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrativa),
    }),
  );
  await page.route("**/api/v1/comedor/operacion/estado**", (ruta) =>
    ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        fecha: "2026-08-31",
        ingresos: 0,
        meta: 10,
        porcentaje: 0,
        duplicados: 0,
        errores: 0,
        recientes: [],
      }),
    }),
  );
});

test("el kiosco independiente enfoca el lector y bloquea una lectura doble", async ({ page }) => {
  let solicitudes = 0;
  await page.route("**/api/v1/comedor/operacion", async (ruta) => {
    solicitudes += 1;
    await new Promise((resolver) => setTimeout(resolver, 150));
    await ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        estado: "aceptada",
        mensaje: "Ingreso registrado.",
        persona: { codigo: "E-10", nombres: "Estudiante de prueba", tipo: "estudiante" },
      }),
    });
  });

  await page.setViewportSize({ width: 1366, height: 768 });
  await page.goto("/admin/panel/comedor");
  const lector = page.getByLabel("Lector de carnet o código institucional");
  await expect(page.getByRole("heading", { name: "Control de comedor" })).toBeVisible();
  await expect(lector).toBeFocused();
  await lector.fill("E-10");
  await lector.press("Enter");
  await lector.press("Enter");
  await expect(page.getByText("Ingreso registrado.")).toBeVisible();
  expect(solicitudes).toBe(1);

  await page.keyboard.press("F3");
  await expect(lector).toBeFocused();
});

test("una resolución pequeña conserva la captura sin desborde", async ({ page }) => {
  let solicitudes = 0;
  await page.route("**/api/v1/comedor/operacion", (ruta) => {
    solicitudes += 1;
    return ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ estado: "aceptada", mensaje: "Ingreso registrado." }),
    });
  });
  await page.setViewportSize({ width: 320, height: 720 });
  await page.goto("/admin/panel/comedor");
  const lector = page.getByLabel("Lector de carnet o código institucional");
  await lector.fill("E-10");
  await lector.press("Enter");
  await expect(page.getByText("Ingreso registrado.")).toBeVisible();
  expect(await page.evaluate(() => document.documentElement.scrollWidth)).toBeLessThanOrEqual(320);
  expect(solicitudes).toBe(1);
});
