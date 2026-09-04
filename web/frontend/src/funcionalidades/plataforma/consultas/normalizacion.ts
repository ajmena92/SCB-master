import type { Pagina } from "@/compartido/contratos/plataforma";

export function normalizarPagina<T>(datos: Pagina<T> | T[]): Pagina<T> {
  return Array.isArray(datos) ? { elementos: datos, total: datos.length } : datos;
}

function camel(nombre: string): string {
  return nombre.replace(/_([a-z])/g, (_, letra: string) => letra.toUpperCase());
}

export function normalizarObjeto<T>(valor: unknown): T {
  if (Array.isArray(valor)) return valor.map((elemento) => normalizarObjeto(elemento)) as T;
  if (valor && typeof valor === "object") {
    return Object.fromEntries(
      Object.entries(valor).map(([clave, contenido]) => [
        camel(clave),
        normalizarObjeto(contenido),
      ]),
    ) as T;
  }
  return valor as T;
}
