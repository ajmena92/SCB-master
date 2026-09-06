export function destinoSesion(session, debeCambiarPin = false) {
  if (!session) return null;
  if (session.tipo === "administracion") {
    if (session.vinculacionPendiente) return "/admin/vinculacion-inicial";
    if (session.cambioContrasenaObligatorio) return "/admin/cambiar-contrasena";
    return "/admin/panel";
  }
  if (session.tipo === "estudiante" || session.tipo === "profesor")
    return debeCambiarPin ? "/cambiar-pin" : "/portal";
  return null;
}
