import { test, expect } from "@playwright/test";

test("acceso administrativo presenta formulario accesible", async ({ page }) => {
  await page.goto("/admin");
  await expect(page.getByRole("heading", { name: "Iniciar sesión" })).toBeVisible();
  await expect(page.getByLabel("Nombre de usuario")).toBeVisible();
  await expect(page.getByLabel("Contraseña")).toBeVisible();
  await expect(page.getByRole("button", { name: "Ingresar" })).toBeEnabled();
});

test("acceso estudiantil presenta controles etiquetados", async ({ page }) => {
  await page.goto("/");
  await expect(page.getByTestId("student-carne-input")).toBeVisible();
  await expect(page.getByTestId("student-pin-input")).toBeVisible();
  await expect(page.getByTestId("student-login-submit")).toBeVisible();
});
