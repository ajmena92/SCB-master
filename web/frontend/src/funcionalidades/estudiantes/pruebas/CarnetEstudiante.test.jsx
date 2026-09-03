import { act } from "react";
import { createRoot } from "react-dom/client";
import { describe, expect, it } from "vitest";
import { CardThumbnail } from "../componentes/CardThumbnail";
import { VistaCarnetEstudiante } from "../componentes/VistaCarnetEstudiante";

describe("carnet del estudiante", () => {
  it("muestra carnet provisional en línea sin descargas", async () => {
    const container = document.createElement("div");
    const root = createRoot(container);
    await act(async () =>
      root.render(<VistaCarnetEstudiante idEstudiante={8} tieneFoto={false} />),
    );
    expect(container.textContent).toContain("Carnet provisional");
    expect(container.querySelector("a[download]")).toBeNull();
    await act(async () => root.unmount());
  });
  it("muestra miniatura cuando hay foto", async () => {
    const container = document.createElement("div");
    const root = createRoot(container);
    await act(async () => root.render(<CardThumbnail idEstudiante={8} tieneFoto />));
    expect(container.querySelector('img[src="/api/v1/estudiantes/8/foto"]')).not.toBeNull();
    await act(async () => root.unmount());
  });
  it("renderiza el contrato canónico plano del carnet", async () => {
    const container = document.createElement("div");
    const root = createRoot(container);
    await act(async () =>
      root.render(
        <VistaCarnetEstudiante
          datosCarnet={{
            idEstudiante: 8,
            nombre: "Ana",
            primerApellido: "Solís",
            segundoApellido: "Rojas",
            cedula: "1-1111-1111",
            seccion: "10-1",
            rutaDescripcion: "Ruta Central",
            idEstadoComedor: 1,
            beneficioComedor: "Beneficiario",
            codigoQr: "SCBQR1.gAAAAABpQRCZxN32A47WJznSEQs30Y0qFcAh0igG73znwW1a2B8gB6Xt20RLwG25yY7zeBB-LmSXOT8TOHzPrjN7xxu5KfdGQ==",
            tieneFoto: false,
            anio: 2026,
          }}
          tieneFoto={false}
        />,
      ),
    );
    expect(container.textContent).toContain("Ana Solís Rojas");
    expect(container.textContent).toContain("Colegio Técnico Profesional de Platanares");
    expect(container.textContent).toContain("2026");
    expect(container.textContent).toContain("10-1");
    expect(container.textContent).toContain("Ruta Central");
    expect(container.querySelector('img[alt="Escudo del CTP Platanares"]')).not.toBeNull();
    expect(container.querySelector("a[download]")).toBeNull();
    expect(container.querySelector('svg[aria-label="Código QR del carnet"]')).not.toBeNull();
    expect(container.querySelector('button[aria-label="Ampliar QR del carnet"]')).not.toBeNull();
    expect(container.textContent).not.toContain("121560069");
    await act(async () => root.unmount());
  });

  it("no muestra ruta ni beca en el carnet de profesor", async () => {
    const container = document.createElement("div");
    const root = createRoot(container);
    await act(async () =>
      root.render(
        <VistaCarnetEstudiante
          tipoPersona="profesor"
          datosCarnet={{
            tipoPersona: "profesor",
            nombre: "Carlos",
            primerApellido: "Pérez",
            rutaDescripcion: "Ruta que no corresponde",
            idEstadoComedor: 2,
            beneficioComedor: "No beneficiario",
            codigoQr: "SCBQR1.gAAAAABpQRCZxN32A47WJznSEQs30Y0qFcAh0igG73znwW1a2B8gB6Xt20RLwG25yY7zeBB-LmSXOT8TOHzPrjN7xxu5KfdGQ==",
          }}
        />,
      ),
    );
    expect(container.textContent).toContain("Profesor");
    expect(container.textContent).not.toContain("Ruta asignada");
    expect(container.textContent).not.toContain("Ruta que no corresponde");
    expect(container.textContent).not.toContain("Beca que no corresponde");
    expect(container.querySelector('svg[aria-label="Código QR del carnet"]')).not.toBeNull();
    await act(async () => root.unmount());
  });
});
