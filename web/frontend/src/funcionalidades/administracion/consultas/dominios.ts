import { api } from "@/lib/api";

export type Registro = Record<string, unknown>;

export interface DefinicionDominio {
  clave: string;
  titulo: string;
  descripcion: string;
  ruta: string;
  permiso: string;
  columnas: string[];
  cargar: () => Promise<Registro[]>;
}

const comoRegistros = (valor: unknown): Registro[] => {
  if (Array.isArray(valor)) return valor as Registro[];
  if (valor && typeof valor === "object") {
    const objeto = valor as Record<string, unknown>;
    for (const clave of ["items", "datos", "eventos", "movimientos", "resultados"]) {
      if (Array.isArray(objeto[clave])) return objeto[clave] as Registro[];
    }
    return [objeto];
  }
  return [];
};

const consultar = async (ruta: string): Promise<Registro[]> => {
  const { data } = await api.get<unknown>(ruta);
  return comoRegistros(data);
};

export const DOMINIOS: Record<string, DefinicionDominio> = {
  estudiantes: {
    clave: "estudiantes", titulo: "Estudiantes", descripcion: "Consulta y gestión de estudiantes.",
    ruta: "/api/v1/estudiantes", permiso: "estudiantes.leer", columnas: ["idEstudiante", "nombreCompleto", "cedula", "seccion"],
    cargar: () => consultar("/v1/estudiantes"),
  },
  asistencia: {
    clave: "asistencia", titulo: "Asistencia", descripcion: "Marcas y correcciones de asistencia.",
    ruta: "/api/v1/asistencia", permiso: "asistencia.leer", columnas: ["idMarca", "fecha", "idEstudiante", "tipo"],
    cargar: () => consultar(`/v1/asistencia/marcas?fecha=${new Date().toISOString().slice(0, 10)}`),
  },
  beneficios: {
    clave: "beneficios", titulo: "Beneficios", descripcion: "Catálogo y asignaciones de beneficios.",
    ruta: "/api/v1/beneficios", permiso: "beneficios.leer", columnas: ["idBeneficio", "nombre", "activo"],
    cargar: () => consultar("/v1/beneficios"),
  },
  cuentas: {
    clave: "cuentas", titulo: "Cuentas y saldos", descripcion: "Saldos y movimientos de estudiantes.",
    ruta: "/api/v1/cuentas", permiso: "cuentas.leer", columnas: ["idEstudiante", "saldo", "actualizadoEn"],
    cargar: async () => [],
  },
  reportes: {
    clave: "reportes", titulo: "Reportes", descripcion: "Reportes operativos disponibles para la administración.",
    ruta: "/api/v1/reportes", permiso: "reportes.leer", columnas: ["tipo", "total", "generadoEn"],
    cargar: () => consultar("/v1/reportes/estudiantes"),
  },
  importaciones: {
    clave: "importaciones", titulo: "Importaciones", descripcion: "Previsualiza y ejecuta importaciones de datos.",
    ruta: "/api/v1/importaciones", permiso: "importaciones.leer", columnas: ["idLote", "nombreArchivo", "estado", "creadoEn"],
    cargar: async () => [],
  },
  auditoria: {
    clave: "auditoria", titulo: "Auditoría", descripcion: "Registro trazable de acciones del sistema.",
    ruta: "/api/v1/auditoria", permiso: "auditoria.leer", columnas: ["idEvento", "evento", "usuario", "fecha"],
    cargar: () => consultar("/v1/auditoria/eventos"),
  },
};
