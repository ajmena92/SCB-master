import { expect, test } from "@playwright/test";

const sesionAdministrativa = {
  tipo: "admin",
  usuario: {
    idUsuario: 977,
    NombreCompleto: "Operador de prueba",
    roles: ["Administrador"],
    permisos: ["comedor.registrar"],
  },
};

test.beforeEach(async ({ page }) => {
  await page.route("**/api/v1/sesion", (ruta) =>
    ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrativa),
    }),
  );
  await page.route("**/api/v1/comedor/operacion/configuracion", (ruta) =>
    ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        fechaServidor: "2026-08-29",
        horaServidor: "11:30:00",
        horarios: [
          { codigo: "diurno", descripcion: "Diurno", horaLimite: "12:00:00", activo: true },
        ],
        minutosAvisoPrevio: 15,
        permitirMarcaTardia: false,
        permitirSinMarcaTransporte: true,
      }),
    }),
  );
  await page.route("**/api/v1/comedor/operacion/historial**", (ruta) =>
    ruta.fulfill({ status: 200, contentType: "application/json", body: "[]" }),
  );
});

test("el kiosco independiente enfoca el lector y bloquea una lectura doble", async ({ page }) => {
  let solicitudes = 0;
  await page.route("**/api/v1/comedor/operacion/ingresos", async (ruta) => {
    solicitudes += 1;
    await new Promise((resolver) => setTimeout(resolver, 150));
    await ruta.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        idIngreso: 1,
        idPersona: 10,
        fecha: "2026-08-29",
        modalidad: "beca",
        nombreCompleto: "Estudiante de prueba",
        resultado: "registrado",
      }),
    });
  });

  await page.setViewportSize({ width: 1366, height: 768 });
  await page.goto("/admin/comedor/operacion");
  const lector = page.getByLabel("Código de barras");
  await expect(page.getByTestId("operacion-comedor")).toBeVisible();
  await expect(lector).toBeFocused();
  await lector.fill("E-10");
  await lector.press("Enter");
  await lector.press("Enter");
  await expect(page.getByRole("status")).toContainText("Ingreso registrado");
  expect(solicitudes).toBe(1);

  await page.keyboard.press("F3");
  await expect(lector).toBeFocused();
});

test("una resolución pequeña bloquea realmente el registro", async ({ page }) => {
  let solicitudes = 0;
  await page.route("**/api/v1/comedor/operacion/ingresos", (ruta) => {
    solicitudes += 1;
    return ruta.fulfill({ status: 500 });
  });
  await page.setViewportSize({ width: 1024, height: 700 });
  await page.goto("/admin/comedor/operacion");
  await page.getByLabel("Código de barras").fill("E-10");
  await expect(page.getByRole("alert")).toContainText("1280×720");
  await expect(page.getByRole("button", { name: "Registrar ingreso" })).toBeDisabled();
  expect(solicitudes).toBe(0);
});
