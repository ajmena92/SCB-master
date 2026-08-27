import { act } from "react";
import { createRoot } from "react-dom/client";
import { MemoryRouter, NavLink, Routes, Route, useLocation, useNavigate } from "react-router-dom";

function NavigationProbe() {
  const location = useLocation();
  const navigate = useNavigate();
  return (
    <>
      <output data-testid="location">{location.pathname}</output>
      <NavLink to="/admin/panel/inicio">Inicio</NavLink>
      <NavLink to="/admin/panel/operacion/rutas">Rutas</NavLink>
      <button type="button" onClick={() => navigate(-1)}>
        Atrás
      </button>
    </>
  );
}

function renderRouter(initialEntries) {
  const container = document.createElement("div");
  document.body.appendChild(container);
  const root = createRoot(container);
  act(() => {
    root.render(
      <MemoryRouter initialEntries={initialEntries}>
        <Routes>
          <Route path="*" element={<NavigationProbe />} />
        </Routes>
      </MemoryRouter>,
    );
  });
  return { container, root };
}

describe("administrative URL navigation", () => {
  it("loads a deep route and exposes the active location", () => {
    const { container, root } = renderRouter(["/admin/panel/operacion/rutas"]);

    expect(container.querySelector('[data-testid="location"]').textContent).toBe(
      "/admin/panel/operacion/rutas",
    );
    expect(
      container
        .querySelector('a[href="/admin/panel/operacion/rutas"]')
        .getAttribute("aria-current"),
    ).toBe("page");

    act(() => root.unmount());
    container.remove();
  });

  it("navigates between modules and supports browser back navigation", () => {
    const { container, root } = renderRouter(["/admin/panel/inicio"]);
    const rutas = container.querySelector('a[href="/admin/panel/operacion/rutas"]');

    act(() => rutas.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true })));
    expect(container.querySelector('[data-testid="location"]').textContent).toBe(
      "/admin/panel/operacion/rutas",
    );

    act(() => container.querySelector("button").click());
    expect(container.querySelector('[data-testid="location"]').textContent).toBe(
      "/admin/panel/inicio",
    );

    act(() => root.unmount());
    container.remove();
  });
});
