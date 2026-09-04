const urlConfigurada = import.meta.env.VITE_API_BASE_URL || "/api";

if (!urlConfigurada.startsWith("/") || urlConfigurada.startsWith("//")) {
  throw new Error("VITE_API_BASE_URL debe ser una ruta relativa, por ejemplo /api.");
}

export const API = urlConfigurada.replace(/\/$/, "");
