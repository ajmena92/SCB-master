export interface ComponenteMenu {
  Orden: number;
  Nombre: string;
  TipoComponente: string;
}

export interface ComponenteMenuEditable extends ComponenteMenu {
  claveEdicion: string;
}

export function prepararComponente(componente: ComponenteMenu): ComponenteMenuEditable {
  return { ...componente, claveEdicion: crypto.randomUUID() };
}

export function prepararComponentes(componentes: ComponenteMenu[]): ComponenteMenuEditable[] {
  return componentes.map(prepararComponente);
}

export function componenteParaGuardar(componente: ComponenteMenuEditable): ComponenteMenu {
  return {
    Orden: componente.Orden,
    Nombre: componente.Nombre,
    TipoComponente: componente.TipoComponente,
  };
}
