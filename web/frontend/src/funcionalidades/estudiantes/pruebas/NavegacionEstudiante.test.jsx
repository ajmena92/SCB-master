import { act } from "react";
import { createRoot } from "react-dom/client";
import { vi } from "vitest";

import { NavegacionEstudiante } from "../componentes/NavegacionEstudiante";

describe("NavegacionEstudiante", () => {
  beforeEach(() => {
    global.IS_REACT_ACT_ENVIRONMENT = true;
  });

  it("expone las vistas del estudiante y conserva la vista activa", async () => {
    const container = document.createElement("div");
    document.body.appendChild(container);
    const root = createRoot(container);
    const onChange = vi.fn();

    await act(async () => {
      root.render(<NavegacionEstudiante vistaActiva="carnet" alCambiar={onChange} />);
    });

    const buttons = [...container.querySelectorAll("button")];
    expect(buttons.map((button) => button.textContent)).toEqual(["Menú", "Carnet"]);
    expect(buttons[1].getAttribute("aria-current")).toBe("page");
    expect(buttons[0].getAttribute("aria-current")).toBeNull();

    await act(async () => {
      buttons[0].dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    expect(onChange).toHaveBeenCalledWith("menu");

    await act(async () => {
      root.unmount();
    });
    container.remove();
  });
});
