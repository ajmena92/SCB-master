import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? "http://127.0.0.1:3000",
    ...devices["Desktop Chrome"],
    ...(process.env.PLAYWRIGHT_EXECUTABLE_PATH
      ? { launchOptions: { executablePath: process.env.PLAYWRIGHT_EXECUTABLE_PATH, args: process.env.PLAYWRIGHT_BASE_URL ? ["--disable-crash-reporter", "--disable-breakpad"] : [] } }
      : {}),
  },
  ...(process.env.PLAYWRIGHT_BASE_URL
    ? {}
    : {
        webServer: {
          // El servidor de Vite conserva /api en el mismo origen y lo reenvía a la API E2E.
          command: "npm run start -- --host 127.0.0.1",
          env: { API_PROXY_TARGET: process.env.API_PROXY_TARGET ?? "http://127.0.0.1:8000" },
          url: "http://127.0.0.1:3000",
          reuseExistingServer: true,
        },
      }),
  reporter: [["list"], ["html", { outputFolder: "playwright-report", open: "never" }]],
});
