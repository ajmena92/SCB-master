import { expect, test } from "@playwright/test";

const sesionAdministrador = {
  tipo: "administracion",
  cuentaId: 1,
  personaId: 10,
  usuario: "direccion",
  nombres: "Dirección",
  rol: "administrador",
  permisos: [],
  cambioContrasenaObligatorio: false,
  vinculacionPendiente: false,
};

test.beforeEach(async ({ page }) => {
  await page.route("**/api/v1/parametros-operativos/institucion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ nombreColegio: "CTP Platanares", subtituloReportes: "" }),
    }),
  );
});

test("el administrador navega al padrón anual", async ({ page }) => {
  await page.route("**/api/v1/sesion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrador),
    }),
  );
  await page.route("**/api/v1/personas**", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        elementos: [
          {
            id: 1,
            codigo: "E-00000018",
            cedula: "1-1111-1111",
            nombres: "Ana",
            apellidos: "Mora",
            tipo: "estudiante",
            activo: true,
          },
        ],
        total: 1,
      }),
    }),
  );
  await page.route("**/api/v1/anios-lectivos**", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        elementos: [{ id: 1, anio: 2026, vigente: true, cerrado: false }],
        total: 1,
      }),
    }),
  );
  await page.route("**/api/v1/matriculas**", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ elementos: [], total: 0 }),
    }),
  );

  await page.goto("/admin/panel/personas");
  await expect(page.getByRole("heading", { name: "Estudiantes / PIN" })).toBeVisible({
    timeout: 10_000,
  });
  await expect(page.getByRole("cell", { name: "1-1111-1111" })).toBeVisible();
  await expect(page.getByRole("link", { name: "Años e importación" })).toBeVisible();
});

test("el operador no recibe enlaces de configuración", async ({ page }) => {
  await page.route("**/api/v1/sesion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        ...sesionAdministrador,
        usuario: "operador",
        nombres: "Operador",
        rol: "operador",
        permisos: ["comedor.operar"],
      }),
    }),
  );
  await page.goto("/admin/panel/inicio");
  await expect(page.getByRole("link", { name: "Ingreso al comedor" })).toBeVisible();
  await expect(page.getByRole("link", { name: "Personas" })).toHaveCount(0);
  await expect(page.getByRole("link", { name: "Años e importación" })).toHaveCount(0);
});

test("expone las operaciones PIN sin persistir credenciales en el navegador", async ({ page }) => {
  await page.route("**/api/v1/sesion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrador),
    }),
  );
  await page.route("**/api/v1/personas**", (route) =>
    route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ elementos: [], total: 0 }) }),
  );
  await page.route("**/api/v1/anios-lectivos**", (route) =>
    route.fulfill({ status: 200, contentType: "application/json", body: "[]" }),
  );
  await page.route("**/api/v1/matriculas**", (route) =>
    route.fulfill({ status: 200, contentType: "application/json", body: "[]" }),
  );

  await page.goto("/admin/panel/personas");
  await expect(page.getByRole("heading", { name: "Estudiantes / PIN" })).toBeVisible();
  await expect(page.getByRole("button", { name: "Operaciones PIN" })).toBeVisible();
  expect(await page.evaluate(() => localStorage.length)).toBe(0);
  expect(await page.evaluate(() => sessionStorage.length)).toBe(0);
});

test("consume las credenciales de la confirmación y ofrece el CSV sin persistir PIN", async ({
  page,
}) => {
  await page.route("**/api/v1/autenticacion/csrf", (route) => route.fulfill({ status: 204 }));
  await page.route("**/api/v1/sesion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrador),
    }),
  );
  await page.route("**/api/v1/anios-lectivos**", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify([{ id: 1, anio: 2026, vigente: true, cerrado: false }]),
    }),
  );
  await page.route("**/api/v1/importaciones/**", (route) =>
    route.request().url().includes("previsualizar")
      ? route.fulfill({
          status: 200,
          contentType: "application/json",
          body: JSON.stringify({ huella: "abc", total: 1, altas: 1, cambios: 0, errores: [], datos: { anio: 2026, filas: [{ cedula: "1", nombres: "Ana" }] } }),
        })
      : route.request().url().includes("/credenciales")
        ? route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ credenciales: [{ codigo: "E-00000018", nombre: "Ana", pinTemporal: "654321" }] }) })
        : route.fulfill({ status: 200, contentType: "application/json", body: JSON.stringify({ trabajoId: 18, estado: "completado", altas: 1, total: 1 }) }),
  );
  await page.goto("/admin/panel/anios");
  await page.getByLabel("Año de destino").selectOption("2026");
  await page.locator('input[type="file"]').setInputFiles({
    name: "padron.xlsx",
    mimeType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    buffer: Buffer.from("archivo-prueba"),
  });
  await page.getByRole("button", { name: "Previsualizar sin guardar" }).click();
  await expect(page.getByRole("button", { name: "Confirmar importación" })).toBeEnabled({ timeout: 10000 });
  await page.getByRole("button", { name: "Confirmar importación" }).click();
  await page.getByRole("button", { name: "Descargar credenciales una vez" }).click();
  await expect(page.getByText(/Descargue ahora las 1 credenciales temporales/)).toBeVisible();

  const descarga = page.waitForEvent("download");
  await page.getByRole("button", { name: "Descargar credenciales CSV" }).click();
  expect((await descarga).suggestedFilename()).toBe("credenciales-importacion.csv");
  expect(await page.evaluate(() => localStorage.length)).toBe(0);
  expect(await page.evaluate(() => sessionStorage.length)).toBe(0);
});
