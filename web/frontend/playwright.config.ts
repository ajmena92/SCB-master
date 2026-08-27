import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  use: {
    baseURL: process.env.PLAYWRIGHT_BASE_URL ?? "http://127.0.0.1:4173",
    ...devices["Desktop Chrome"],
    ...(process.env.PLAYWRIGHT_EXECUTABLE_PATH
      ? { launchOptions: { executablePath: process.env.PLAYWRIGHT_EXECUTABLE_PATH, args: process.env.PLAYWRIGHT_BASE_URL ? ["--disable-crash-reporter", "--disable-breakpad"] : [] } }
      : {}),
  },
  ...(process.env.PLAYWRIGHT_BASE_URL ? {} : { webServer: { command: "npm run preview -- --host 127.0.0.1", url: "http://127.0.0.1:4173", reuseExistingServer: true } }),
  reporter: [["list"], ["html", { outputFolder: "playwright-report", open: "never" }]],
});
