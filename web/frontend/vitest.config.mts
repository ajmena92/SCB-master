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
    environment: "happy-dom",
    setupFiles: [path.resolve(rootDir, "src/testSetup.js")],
    css: true,
    // Un solo proceso evita bloqueos de workers en el entorno Windows/WSL y
    // mantiene aislados los módulos que mockean el cliente HTTP global.
    pool: "threads",
    maxWorkers: 1,
    fileParallelism: false,
    exclude: [
      "**/node_modules/**",
      "**/node_modules-deps/**",
      "**/dist/**",
      "**/e2e/**",
      "**/playwright-report/**",
      "**/.{git,cache,output,temp}/**",
    ],
    coverage: {
      provider: "v8",
      reporter: ["text", "json-summary", "html"],
      reportsDirectory: "./coverage",
      exclude: ["src/testSetup.js", "src/**/*.test.*"],
      thresholds: { lines: 80, functions: 80, statements: 80, branches: 75 },
    },
  },
});
