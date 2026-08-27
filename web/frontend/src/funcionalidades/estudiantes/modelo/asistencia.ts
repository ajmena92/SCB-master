export type EstadoAsistencia = {
  estado?: string;
  periodoCerrado?: boolean;
  periodoAbierto?: boolean;
  [clave: string]: unknown;
};
export function obtenerVistaAsistencia(estado?: EstadoAsistencia | null): string {
  const confirmada = estado?.estado === "Confirmada" || estado?.estado === "Corregida";
  if (estado?.periodoCerrado) return confirmada ? "expired-confirmed" : "expired-unconfirmed";
  if (confirmada) return "confirmed";
  if (!estado?.periodoAbierto) return "pending";
  return "open";
}

export function formatearCuentaRegresiva(segundos?: number | null): string | null {
  if (typeof segundos !== "number" || !Number.isFinite(segundos)) return null;
  const segundosSeguros = Math.max(0, Math.floor(segundos));
  const horas = Math.floor(segundosSeguros / 3_600);
  const minutos = Math.floor((segundosSeguros % 3_600) / 60);
  const segundosRestantes = segundosSeguros % 60;
  return `${String(horas).padStart(2, "0")} h ${String(minutos).padStart(2, "0")} min ${String(segundosRestantes).padStart(2, "0")} s`;
}

export function analizarHoraServidor(valor?: unknown): number | null {
  if (typeof valor !== "string") return null;
  const coincidencia = /^(\d{1,2}):([0-5]\d)(?::([0-5]\d)(?:\.\d+)?)?$/.exec(valor.trim());
  if (!coincidencia) return null;

  const horas = Number(coincidencia[1]);
  if (horas > 23) return null;
  return horas * 3_600 + Number(coincidencia[2]) * 60 + Number(coincidencia[3] || 0);
}

export function formatearHoraServidor(segundos?: number | null): string | null {
  if (typeof segundos !== "number" || !Number.isFinite(segundos)) return null;
  const segundosSeguros = ((Math.floor(segundos) % 86_400) + 86_400) % 86_400;
  const horas = Math.floor(segundosSeguros / 3_600);
  const minutos = Math.floor((segundosSeguros % 3_600) / 60);
  const segundosRestantes = segundosSeguros % 60;
  return `${String(horas).padStart(2, "0")}:${String(minutos).padStart(2, "0")}:${String(segundosRestantes).padStart(2, "0")}`;
}

export function segundosTranscurridos(
  sincronizadoEnMs?: number | null,
  ahoraMs = Date.now(),
): number {
  if (
    typeof sincronizadoEnMs !== "number" ||
    !Number.isFinite(sincronizadoEnMs) ||
    !Number.isFinite(ahoraMs)
  )
    return 0;
  return Math.max(0, Math.floor((ahoraMs - sincronizadoEnMs) / 1_000));
}

export function segundosRestantesEn(
  segundos?: number | null,
  sincronizadoEnMs?: number | null,
  ahoraMs = Date.now(),
): number | null {
  if (typeof segundos !== "number" || !Number.isFinite(segundos)) return null;
  return Math.max(0, Math.floor(segundos) - segundosTranscurridos(sincronizadoEnMs, ahoraMs));
}

export function horaServidorEn(
  segundos?: number | null,
  sincronizadoEnMs?: number | null,
  ahoraMs = Date.now(),
): number | null {
  if (typeof segundos !== "number" || !Number.isFinite(segundos)) return null;
  return (segundos + segundosTranscurridos(sincronizadoEnMs, ahoraMs)) % 86_400;
}

export function estaProximoElCierre(
  segundos?: number | null,
  minutosAviso?: number | null,
): boolean {
  if (typeof segundos !== "number" || !Number.isFinite(segundos) || segundos <= 0) return false;
  const minutosAvisoSeguro =
    typeof minutosAviso === "number" && Number.isFinite(minutosAviso) ? minutosAviso : 15;
  return segundos <= minutosAvisoSeguro * 60;
}
