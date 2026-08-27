export function attendanceViewState(estado) {
  const confirmed = estado?.estado === "Confirmada" || estado?.estado === "Corregida";
  if (estado?.periodoCerrado) return confirmed ? "expired-confirmed" : "expired-unconfirmed";
  if (confirmed) return "confirmed";
  if (!estado?.periodoAbierto) return "pending";
  return "open";
}

export function formatCountdown(seconds) {
  if (!Number.isFinite(seconds)) return null;
  const safeSeconds = Math.max(0, Math.floor(seconds));
  const hours = Math.floor(safeSeconds / 3_600);
  const minutes = Math.floor((safeSeconds % 3_600) / 60);
  const remainingSeconds = safeSeconds % 60;
  return `${String(hours).padStart(2, "0")} h ${String(minutes).padStart(2, "0")} min ${String(remainingSeconds).padStart(2, "0")} s`;
}

export function parseServerClock(value) {
  if (typeof value !== "string") return null;
  const match = /^(\d{1,2}):([0-5]\d)(?::([0-5]\d)(?:\.\d+)?)?$/.exec(value.trim());
  if (!match) return null;

  const hours = Number(match[1]);
  if (hours > 23) return null;
  return hours * 3_600 + Number(match[2]) * 60 + Number(match[3] || 0);
}

export function formatServerClock(seconds) {
  if (!Number.isFinite(seconds)) return null;
  const safeSeconds = ((Math.floor(seconds) % 86_400) + 86_400) % 86_400;
  const hours = Math.floor(safeSeconds / 3_600);
  const minutes = Math.floor((safeSeconds % 3_600) / 60);
  const remainingSeconds = safeSeconds % 60;
  return `${String(hours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:${String(remainingSeconds).padStart(2, "0")}`;
}

export function elapsedWholeSeconds(syncedAtMs, nowMs = Date.now()) {
  if (!Number.isFinite(syncedAtMs) || !Number.isFinite(nowMs)) return 0;
  return Math.max(0, Math.floor((nowMs - syncedAtMs) / 1_000));
}

export function secondsRemainingAt(seconds, syncedAtMs, nowMs = Date.now()) {
  if (!Number.isFinite(seconds)) return null;
  return Math.max(0, Math.floor(seconds) - elapsedWholeSeconds(syncedAtMs, nowMs));
}

export function serverClockAt(seconds, syncedAtMs, nowMs = Date.now()) {
  if (!Number.isFinite(seconds)) return null;
  return (seconds + elapsedWholeSeconds(syncedAtMs, nowMs)) % 86_400;
}

export function isClosingSoon(seconds, warningMinutes) {
  if (!Number.isFinite(seconds) || seconds <= 0) return false;
  const safeWarningMinutes = Number.isFinite(warningMinutes) ? warningMinutes : 15;
  return seconds <= safeWarningMinutes * 60;
}
