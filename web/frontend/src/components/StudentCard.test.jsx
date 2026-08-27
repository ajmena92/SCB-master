import { act } from "react";
import { createRoot } from "react-dom/client";
import { describe, expect, it } from "vitest";
import { CardThumbnail, StudentCardPreview } from "./StudentCard";

describe("carnet del estudiante", () => {
  it("muestra carnet provisional y enlaces canónicos", async () => {
    const container = document.createElement("div"); const root = createRoot(container);
    await act(async () => root.render(<StudentCardPreview studentId={8} hasPhoto={false} />));
    expect(container.textContent).toContain("Carnet provisional"); expect(container.querySelector('a[href="/api/v1/estudiantes/8/carnet.pdf"]')).not.toBeNull();
    await act(async () => root.unmount());
  });
  it("muestra miniatura cuando hay foto", async () => {
    const container = document.createElement("div"); const root = createRoot(container);
    await act(async () => root.render(<CardThumbnail studentId={8} hasPhoto />));
    expect(container.querySelector('img[src="/api/v1/estudiantes/8/foto"]')).not.toBeNull(); await act(async () => root.unmount());
  });
  it("renderiza el contrato canónico plano del carnet", async () => {
    const container = document.createElement("div"); const root = createRoot(container);
    await act(async () => root.render(<StudentCardPreview cardData={{
      idEstudiante: 8, carne: "2026-008", nombre: "Ana", primerApellido: "Solís",
      segundoApellido: "Rojas", cedula: "1-1111-1111", seccion: "10-1",
      rutaCodigo: "R-01", rutaDescripcion: "Ruta Central", tipoBeca: "Comedor",
      barcode: "2026-008", tieneFoto: false,
    }} hasPhoto={false} />));
    expect(container.textContent).toContain("Ana Solís Rojas");
    expect(container.textContent).toContain("Ruta Central");
    expect(container.querySelector('a[href="/api/v1/estudiantes/carnet.pdf"]')).not.toBeNull();
    expect(container.querySelector('a[href^="/api/student/"]')).toBeNull();
    await act(async () => root.unmount());
  });
});
