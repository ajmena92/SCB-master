import type {
  AnioLectivo,
  CredencialTemporal,
  Matricula,
  Pagina,
  Persona,
  PlantillaMenu,
  PersonaCreada,
  PublicacionMenu,
  ReporteFila,
  ResultadoConfirmacionImportacion,
  ResultadoOperacion,
  ResumenImportacion,
  RutaTransporte,
  Tarifa,
} from "@/compartido/contratos/plataforma";
import { api } from "@/compartido/consultas/cliente_http";

function normalizarPagina<T>(datos: Pagina<T> | T[]): Pagina<T> {
  return Array.isArray(datos) ? { elementos: datos, total: datos.length } : datos;
}

function camel(nombre: string): string {
  return nombre.replace(/_([a-z])/g, (_, letra: string) => letra.toUpperCase());
}

function normalizarObjeto<T>(valor: unknown): T {
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

async function listar<T>(ruta: string, parametros?: Record<string, unknown>): Promise<Pagina<T>> {
  const { data } = await api.get<Pagina<T> | T[]>(ruta, { params: parametros });
  return normalizarPagina(normalizarObjeto<Pagina<T> | T[]>(data));
}

export const plataformaApi = {
  personas: {
    listar: (buscar = "") => listar<Persona>("/v1/personas", { buscar }),
    crear: async (datos: Omit<Persona, "id" | "codigo">) =>
      (
        await api.post<PersonaCreada>("/v1/personas", {
          cedula: datos.cedula,
          nombres: [datos.nombres, datos.apellidos].filter(Boolean).join(" "),
          tipo: datos.tipo,
          activo: datos.activo,
        })
      ).data,
    actualizar: (id: number, datos: Partial<Persona>) =>
      api.put<Persona>(`/v1/personas/${id}`, datos),
  },
  anios: {
    listar: () => listar<AnioLectivo>("/v1/anios-lectivos"),
    crear: (datos: Pick<AnioLectivo, "anio" | "vigente">) =>
      api.post<AnioLectivo>("/v1/anios-lectivos", datos),
    activar: (id: number) => api.post<AnioLectivo>(`/v1/anios-lectivos/${id}/activar`),
  },
  matriculas: {
    listar: async (anioLectivoId?: number) => {
      const pagina = await listar<Matricula & { becado?: boolean }>("/v1/matriculas", {
        anio_id: anioLectivoId,
      });
      return {
        ...pagina,
        elementos: pagina.elementos.map((matricula) => ({
          ...matricula,
          becaComedor: matricula.becaComedor ?? matricula.becado ?? false,
        })),
      };
    },
    crear: (datos: Omit<Matricula, "id">) =>
      api.post<Matricula>("/v1/matriculas", {
        personaId: datos.personaId,
        anioLectivoId: datos.anioLectivoId,
        seccion: datos.seccion,
        turno: datos.turno,
        becado: datos.becaComedor,
        estado: datos.estado,
      }),
  },
  importaciones: {
    previsualizar: async (archivo: File, anio: number) => {
      const cuerpo = new FormData();
      cuerpo.append("archivo", archivo);
      cuerpo.append("anio", String(anio));
      const { data } = await api.post<{
        huella: string;
        total: number;
        altas: number;
        cambios: number;
        errores: Array<{ fila: number; error: string }>;
        datos: { anio: number; filas: unknown[] };
      }>("/v1/importaciones/previsualizar", cuerpo);
      return {
        token: JSON.stringify({ ...data.datos, huella: data.huella }),
        filas: data.total,
        altas: data.altas,
        cambios: data.cambios,
        errores: data.errores.length,
        detalle: data.errores.map((error) => ({
          fila: error.fila,
          estado: "error",
          mensaje: error.error,
        })),
      } satisfies ResumenImportacion;
    },
    confirmar: async (token: string): Promise<ResultadoConfirmacionImportacion> => {
      const { data } = await api.post("/v1/importaciones/confirmar", JSON.parse(token));
      const normalizados = normalizarObjeto<{
        credenciales?: CredencialTemporal[];
        [campo: string]: unknown;
      }>(data);
      return { ...normalizados, credenciales: normalizados.credenciales ?? [] };
    },
  },
  rutas: {
    listar: () => listar<RutaTransporte>("/v1/rutas"),
    crear: (datos: Omit<RutaTransporte, "id">) => api.post<RutaTransporte>("/v1/rutas", datos),
  },
  menu: {
    plantillas: () => listar<PlantillaMenu>("/v1/menu/plantillas"),
    crearPlantilla: (datos: Omit<PlantillaMenu, "id">) =>
      api.post<PlantillaMenu>("/v1/menu/plantillas", datos),
    publicaciones: () => listar<PublicacionMenu>("/v1/menu/publicaciones"),
    publicar: (datos: { fecha: string; plantillaId: number }) =>
      api.post<PublicacionMenu>("/v1/menu/publicaciones", datos),
    hoy: async () => {
      const publicaciones = await listar<PublicacionMenu>("/v1/menu/publicaciones");
      const fecha = new Date().toISOString().slice(0, 10);
      return publicaciones.elementos.find((publicacion) => publicacion.fecha === fecha) ?? null;
    },
  },
  tiquetes: {
    tarifas: async () => {
      const pagina = await listar<
        Tarifa & { monto?: number; fechaInicio?: string; fechaFin?: string | null }
      >("/v1/tiquetes/tarifas");
      return {
        ...pagina,
        elementos: pagina.elementos.map((tarifa) => ({
          ...tarifa,
          montoColones: tarifa.montoColones ?? tarifa.monto ?? 0,
          vigenteDesde: tarifa.vigenteDesde ?? tarifa.fechaInicio ?? "",
          vigenteHasta: tarifa.vigenteHasta ?? tarifa.fechaFin,
        })),
      };
    },
    crearTarifa: (datos: Omit<Tarifa, "id">) =>
      api.post<Tarifa>("/v1/tiquetes/tarifas", {
        tipoPersona: datos.tipoPersona,
        monto: datos.montoColones,
        fechaInicio: datos.vigenteDesde,
        fechaFin: datos.vigenteHasta,
      }),
    vender: (datos: { codigo: string; cantidad: number; medioPago: string }) =>
      api.post("/v1/tiquetes/ventas", datos),
  },
  comedor: {
    reservar: (fecha: string) => api.post("/v1/comedor/reservas", { fecha }),
    cancelarReserva: (id: number) => api.delete(`/v1/comedor/reservas/${id}`),
    registrarIngreso: async (codigo: string): Promise<ResultadoOperacion> => {
      const { data } = await api.post("/v1/comedor/operacion", {
        codigo,
        fecha: new Date().toISOString().slice(0, 10),
      });
      return {
        estado: "aceptada",
        mensaje: data.mensaje ?? "Ingreso registrado.",
        saldo: data.saldo,
      };
    },
    decidirAutorizacion: (
      codigo: string,
      decision: "aprobada" | "rechazada",
      observacion: string,
    ) =>
      api.post("/v1/comedor/autorizaciones", {
        codigo,
        fecha: new Date().toISOString().slice(0, 10),
        decision,
        motivo: observacion,
      }),
  },
  transporte: {
    marcar: async (codigo: string): Promise<ResultadoOperacion> => {
      const { data } = await api.post("/v1/transporte/marcas", {
        codigo,
        fecha: new Date().toISOString().slice(0, 10),
      });
      return { estado: "aceptada", mensaje: data.mensaje ?? "Marca registrada." };
    },
  },
  reportes: {
    obtener: async (tipo: "comedor" | "transporte" | "ventas", desde: string, hasta: string) =>
      (await api.get<ReporteFila[]>(`/v1/reportes/${tipo}`, { params: { desde, hasta } })).data,
  },
};
