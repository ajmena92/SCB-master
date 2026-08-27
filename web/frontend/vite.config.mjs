import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "node:path";
import { fileURLToPath } from "node:url";

const rootDir = path.dirname(fileURLToPath(import.meta.url));

export default defineConfig(({ mode }) => ({
  plugins: [react()],
  resolve: { alias: { "@": path.resolve(rootDir, "src") } },
  server: {
    host: "127.0.0.1",
    port: 3000,
    strictPort: true,
    proxy: process.env.API_PROXY_TARGET
      ? { "/api": { target: process.env.API_PROXY_TARGET, changeOrigin: false, secure: false } }
      : undefined,
  },
  preview: { host: "127.0.0.1", port: 4173, strictPort: true },
  test: {
    globals: true,
    environment: "jsdom",
    setupFiles: [path.resolve(rootDir, "src/testSetup.js")],
    alias: { "@": path.resolve(rootDir, "src") },
    css: true,
    pool: "forks",
    singleThread: true,
    coverage: {
      provider: "v8",
      reporter: ["text", "html", "json-summary"],
      reportsDirectory: "./coverage",
      exclude: ["src/testSetup.js", "src/**/*.test.*", "src/compartido/contratos/api.ts"],
      thresholds: { lines: 80, functions: 80, statements: 80, branches: 75 },
    },
  },
  define: { "process.env.NODE_ENV": JSON.stringify(mode) },
}));
