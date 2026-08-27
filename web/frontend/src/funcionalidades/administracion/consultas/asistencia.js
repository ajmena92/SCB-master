import { api } from "@/compartido/consultas/cliente_http";

export async function buscarEstudiantes(texto) {
  return (await api.get(`/v1/estudiantes?pagina=1&tamano=50&buscar=${encodeURIComponent(texto)}`))
    .data.items;
}

export async function guardarCorreccion(idUsuario, datos) {
  return api.put(`/v1/asistencia/marcas/${Number(idUsuario)}/correccion`, datos);
}
