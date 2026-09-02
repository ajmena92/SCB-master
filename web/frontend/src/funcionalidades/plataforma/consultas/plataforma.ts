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
  ResumenPersonas,
  ResumenImportacion,
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
    listar: (parametros: {
      buscar?: string;
      estado?: "activos" | "inactivos" | "todos";
      tipo?: "estudiante" | "profesor";
      pagina?: number;
      tamano?: number;
      ordenar_por?: "nombres" | "cedula" | "tipo" | "estado";
      direccion?: "asc" | "desc";
    } = {}) => listar<Persona>("/v1/personas", parametros),
    obtener: async (id: number) =>
      normalizarObjeto<Persona>((await api.get(`/v1/personas/${id}`)).data),
    resumen: async () => normalizarObjeto<ResumenPersonas>((await api.get("/v1/personas/resumen")).data),
    crear: async (datos: Omit<Persona, "id" | "codigo">) =>
      (
        await api.post<PersonaCreada>("/v1/personas", {
          cedula: datos.cedula,
          nombres: [datos.nombres, datos.apellidos].filter(Boolean).join(" "),
          tipo: datos.tipo,
          activo: datos.activo,
        })
      ).data,
    actualizar: async (id: number, datos: Pick<Persona, "cedula" | "nombres">) =>
      normalizarObjeto<Persona>((await api.put(`/v1/personas/${id}`, datos)).data),
    desactivar: (id: number) => api.post(`/v1/personas/${id}/desactivar`),
    reiniciarPin: async (id: number) =>
      normalizarObjeto<CredencialTemporal>((await api.post(`/v1/personas/${id}/reiniciar-pin`)).data),
    reiniciarPinesSeccion: async (datos: { anioLectivoId: number; seccion: string }) =>
      normalizarObjeto<CredencialTemporal[]>((await api.post("/v1/personas/pines/seccion", datos)).data),
    foto: {
      obtener: async (id: number) =>
        (await api.get(`/v1/personas/${id}/foto`, {
          responseType: "blob",
          omitirManejoFalloAutenticacion: true,
        })).data as Blob,
      cargar: (id: number, archivo: File) => {
        const datos = new FormData();
        datos.append("archivo", archivo);
        return api.post(`/v1/personas/${id}/foto`, datos);
      },
      eliminar: (id: number) => api.delete(`/v1/personas/${id}/foto`),
    },
  },
  anios: {
    listar: () => listar<AnioLectivo>("/v1/anios-lectivos"),
    crear: (datos: Pick<AnioLectivo, "anio" | "vigente">) =>
      api.post<AnioLectivo>("/v1/anios-lectivos", datos),
    activar: (id: number) => api.post<AnioLectivo>(`/v1/anios-lectivos/${id}/activar`),
    secciones: (id: number) => listar<string>(`/v1/anios-lectivos/${id}/secciones`),
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
        becado: datos.becaComedor,
        estado: datos.estado,
      }),
    actualizarBeneficios: async (id: number, datos: { becado: boolean; rutaId: number | null }) =>
      normalizarObjeto<{ matriculaId: number; becado: boolean; rutaId: number | null }>(
        (await api.put(`/v1/matriculas/${id}/beneficios`, datos)).data,
      ),
  },
  rutas: {
    listar: () => listar<{
      idRuta: number;
      codigo: string;
      descripcion: string;
      activo: boolean;
    }>("/v1/rutas"),
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
        desactivaciones: data.desactivaciones,
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
      return normalizarObjeto<ResultadoOperacion>(data);
    },
    estadoOperacion: async (fecha: string) =>
      normalizarObjeto<{
        fecha: string;
        ingresos: number;
        meta: number;
        porcentaje: number;
        duplicados: number;
        errores: number;
        recientes: Array<{
          id: number;
          hora: string;
          codigo: string;
          nombre: string;
          resultado: string;
          motivo?: string;
        }>;
      }>((await api.get("/v1/comedor/operacion/estado", { params: { fecha } })).data),
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
  reportes: {
    obtener: async (tipo: "comedor" | "transporte" | "ventas", desde: string, hasta: string) =>
      (await api.get<ReporteFila[]>(`/v1/reportes/${tipo}`, { params: { desde, hasta } })).data,
  },
};
