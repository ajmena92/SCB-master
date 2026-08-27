import path from "node:path";
import { fileURLToPath } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vitest/config";

const rootDir = path.dirname(fileURLToPath(import.meta.url));

/** Configuración explícita de las puertas de cobertura del frontend. */
export default defineConfig({
  plugins: [react()],
  resolve: { alias: { "@": path.resolve(rootDir, "src") } },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: [path.resolve(rootDir, "src/testSetup.js")],
    css: true,
    pool: "forks",
    singleThread: true,
    exclude: ["**/node_modules/**", "**/dist/**", "**/e2e/**", "**/playwright-report/**", "**/.{git,cache,output,temp}/**"],
    coverage: {
      provider: "v8",
      reporter: ["text", "json-summary", "html"],
      reportsDirectory: "./coverage",
      exclude: ["src/testSetup.js", "src/**/*.test.*", "src/compartido/contratos/api.ts"],
      thresholds: { lines: 80, functions: 80, statements: 80, branches: 75 },
    },
  },
});
