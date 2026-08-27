import { api } from "@/compartido/consultas/cliente_http";

const TIME_PATTERN = /^([01]\d|2[0-3]):[0-5]\d$/;

function field(item, camel, pascal) {
  return item?.[camel] ?? item?.[pascal];
}

export function normalizeParametros(data) {
  const horarios = data?.horarios ?? data?.Horarios ?? [];
  return {
    minutosAvisoPrevio: String(
      data?.minutosAvisoPrevio ??
        data?.MinutosAvisoPrevio ??
        data?.minutosAviso ??
        data?.MinutosAviso ??
        "15",
    ),
    horarios: horarios.map((horario) => ({
      idHorario: field(horario, "idHorario", "IdHorario"),
      descripcion: field(horario, "descripcion", "Descripcion") || "Horario",
      horaApertura:
        field(horario, "horaInicio", "HoraInicio") ??
        field(horario, "horaApertura", "HoraApertura"),
      horaLimite: field(horario, "horaLimite", "HoraLimite") || "",
      activo:
        field(horario, "activo", "Activo") !== false && field(horario, "activo", "Activo") !== 0,
    })),
  };
}

export function validateParametros({ minutosAvisoPrevio, horarios }) {
  const editables = horarios.filter((horario) => horario.activo !== false);
  const minutos = Number(minutosAvisoPrevio);
  if (!Number.isInteger(minutos) || minutos < 1 || minutos > 120)
    return "El aviso previo debe ser un número entre 1 y 120 minutos.";
  if (editables.some((horario) => !TIME_PATTERN.test(horario.horaLimite)))
    return "Cada hora límite debe tener el formato HH:mm.";
  if (
    editables.some((horario) => horario.horaApertura && horario.horaLimite <= horario.horaApertura)
  )
    return "La hora límite debe ser posterior a la hora de apertura de cada horario.";
  return "";
}

export async function consultarCalendario(anio, mes) {
  return (await api.get(`/v1/parametros/calendario?anio=${anio}&mes=${mes}`)).data.dias;
}

export async function consultarParametros() {
  return (await api.get("/v1/parametros")).data;
}

export async function guardarParametros(datos) {
  return api.put("/v1/parametros", datos);
}
