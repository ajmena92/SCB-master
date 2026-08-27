export function formatApiError(detail: unknown): string {
  if (detail == null) return "Ocurrió un error. Intente de nuevo.";
  if (typeof detail === "string") return detail;
  if (Array.isArray(detail))
    return detail
      .map((item) =>
        item && typeof item === "object" && "msg" in item && typeof item.msg === "string"
          ? item.msg
          : JSON.stringify(item),
      )
      .join(" ");
  if (
    typeof detail === "object" &&
    detail !== null &&
    "msg" in detail &&
    typeof detail.msg === "string"
  )
    return detail.msg;
  return String(detail);
}

type ErrorApi = { response?: { status?: number; data?: { detail?: unknown } } };

export function errMsg(error: unknown, options: { showUnauthorizedDetail?: boolean } = {}): string {
  const candidato = typeof error === "object" && error !== null ? (error as ErrorApi) : {};
  if (!candidato.response)
    return "No fue posible comunicarse con el servidor. Verifique su conexión e intente de nuevo.";
  const status = candidato.response.status;
  if (status === 401) {
    if (options.showUnauthorizedDetail)
      return candidato.response.data?.detail == null
        ? "Credenciales inválidas."
        : formatApiError(candidato.response.data.detail);
    return "Su sesión no es válida o ha expirado. Ingrese nuevamente.";
  }
  if (status === 403) return "No tiene permiso para realizar esta acción.";
  if (status === 429) return "Demasiados intentos. Espere un momento antes de continuar.";
  if (status !== undefined && status >= 500)
    return "El servidor no pudo completar la solicitud. Intente de nuevo más tarde.";
  return (
    formatApiError(candidato.response.data?.detail) || "No fue posible completar la solicitud."
  );
}
