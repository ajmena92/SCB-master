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
  await expect(page.getByRole("heading", { name: "Personas y matrículas" }).last()).toBeVisible();
  await expect(page.getByText("E-00000018")).toBeVisible();
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

test("muestra la credencial creada una sola vez y permite copiarla y descargarla", async ({
  context,
  page,
}) => {
  await context.grantPermissions(["clipboard-read", "clipboard-write"]);
  await page.route("**/api/v1/sesion", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(sesionAdministrador),
    }),
  );
  await page.route("**/api/v1/personas**", async (route) => {
    if (route.request().method() === "POST") {
      await route.fulfill({
        status: 201,
        contentType: "application/json",
        body: JSON.stringify({
          id: 8,
          codigo: "E-00000018",
          cedula: "1-1111-1111",
          nombres: "Ana Mora",
          tipo: "estudiante",
          activo: true,
          pinTemporal: "654321",
        }),
      });
      return;
    }
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ elementos: [], total: 0 }),
    });
  });
  await page.route("**/api/v1/anios-lectivos**", (route) =>
    route.fulfill({ status: 200, contentType: "application/json", body: "[]" }),
  );
  await page.route("**/api/v1/matriculas**", (route) =>
    route.fulfill({ status: 200, contentType: "application/json", body: "[]" }),
  );

  await page.goto("/admin/panel/personas");
  await page.getByLabel("Cédula", { exact: true }).fill("1-1111-1111");
  await page.getByLabel("Nombres").fill("Ana");
  await page.getByLabel("Apellidos").fill("Mora");
  await page.getByRole("button", { name: "Crear persona" }).click();

  const dialogo = page.getByRole("alertdialog");
  await expect(dialogo).toContainText("E-00000018");
  await expect(dialogo).toContainText("654321");
  await dialogo.getByRole("button", { name: "Copiar" }).click();
  await expect
    .poll(() => page.evaluate(() => navigator.clipboard.readText()))
    .toContain("PIN temporal: 654321");

  const descarga = page.waitForEvent("download");
  await dialogo.getByRole("button", { name: "Descargar CSV" }).click();
  expect((await descarga).suggestedFilename()).toBe("credencial-E-00000018.csv");
  await dialogo.getByRole("button", { name: "Ya la guardé" }).click();
  await expect(dialogo).toHaveCount(0);
  await expect(page.getByText("654321")).toHaveCount(0);
  expect(await page.evaluate(() => localStorage.length)).toBe(0);
  expect(await page.evaluate(() => sessionStorage.length)).toBe(0);
});

test("consume las credenciales de la confirmación y ofrece el CSV sin persistir PIN", async ({
  page,
}) => {
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
  await page.route("**/api/v1/importaciones/previsualizar", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        huella: "abc",
        total: 1,
        altas: 1,
        cambios: 0,
        errores: [],
        datos: { anio: 2026, filas: [{ cedula: "1", nombres: "Ana" }] },
      }),
    }),
  );
  await page.route("**/api/v1/importaciones/confirmar", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        credenciales: [{ codigo: "E-00000018", nombre: "Ana", pinTemporal: "654321" }],
      }),
    }),
  );

  await page.goto("/admin/panel/anios");
  await page.getByLabel("Año de destino").selectOption("2026");
  await page.getByLabel("Archivo .xlsx").setInputFiles({
    name: "padron.xlsx",
    mimeType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    buffer: Buffer.from("archivo-prueba"),
  });
  await page.getByRole("button", { name: "Previsualizar sin guardar" }).click();
  await page.getByRole("button", { name: "Confirmar importación" }).click();
  await expect(page.getByText(/Descargue ahora las 1 credenciales temporales/)).toBeVisible();

  const descarga = page.waitForEvent("download");
  await page.getByRole("button", { name: "Descargar credenciales CSV" }).click();
  expect((await descarga).suggestedFilename()).toBe("credenciales-importacion.csv");
  expect(await page.evaluate(() => localStorage.length)).toBe(0);
  expect(await page.evaluate(() => sessionStorage.length)).toBe(0);
});
