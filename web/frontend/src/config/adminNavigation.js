import {
  CalendarDays,
  FileSpreadsheet,
  LayoutDashboard,
  Route,
  Ticket,
  UtensilsCrossed,
  Users,
} from "lucide-react";

export const ADMIN_NAVIGATION_GROUPS = [
  { id: "inicio", label: "Inicio" },
  { id: "operacion", label: "Operación" },
  { id: "personas", label: "Personas" },
  { id: "reportes", label: "Reportes" },
  { id: "mas", label: "Más" },
];

export const ADMIN_NAVIGATION = [
  {
    v: "dashboard",
    id: "dashboard",
    label: "Dashboard",
    group: "inicio",
    path: "/admin/panel/inicio",
    requiredPermissions: ["reportes.dashboard.leer"],
    icon: LayoutDashboard,
  },
  {
    v: "menu",
    id: "menu",
    label: "Menú",
    group: "operacion",
    path: "/admin/panel/menu",
    requiredPermissions: ["menu.leer"],
    icon: CalendarDays,
    adminOnly: true,
  },
  {
    v: "comedor",
    id: "comedor",
    label: "Ingreso al comedor",
    group: "operacion",
    path: "/admin/panel/comedor",
    requiredPermissions: ["comedor.registrar"],
    icon: UtensilsCrossed,
  },
  {
    v: "rutas",
    id: "rutas",
    label: "Rutas y transporte",
    group: "operacion",
    path: "/admin/panel/rutas",
    requiredPermissions: ["rutas.administrar"],
    icon: Route,
    adminOnly: true,
  },
  {
    v: "estudiantes",
    id: "estudiantes",
    label: "Estudiantes y profesores",
    group: "personas",
    path: "/admin/panel/personas",
    requiredPermissions: ["estudiantes.leer"],
    icon: Users,
    adminOnly: true,
  },
  {
    v: "tiquetes",
    id: "tiquetes",
    label: "Tiquetes y saldos",
    group: "personas",
    path: "/admin/panel/tiquetes",
    requiredPermissions: ["cuentas.leer"],
    icon: Ticket,
  },
  {
    v: "reporte",
    id: "reporte",
    label: "Reportes",
    group: "reportes",
    path: "/admin/panel/reportes",
    requiredPermissions: ["reportes.leer"],
    icon: FileSpreadsheet,
  },
  {
    v: "anios",
    id: "anios",
    label: "Años e importación",
    group: "mas",
    path: "/admin/panel/anios",
    requiredPermissions: ["importaciones.leer"],
    icon: FileSpreadsheet,
    adminOnly: true,
  },
];

function roleFromSession(session) {
  return session?.usuario?.Rol || session?.Rol || "";
}

export function isAdministratorSession(session) {
  const role = roleFromSession(session).toLocaleLowerCase();
  return role === "administrador" || (session?.tipo === "admin" && !role);
}

export function obtenerModulosVisibles(session) {
  const isAdmin = isAdministratorSession(session);
  const esCuentaAdministrativa = session?.tipo === "admin";
  const permissions = Array.isArray(session?.permisos) ? session.permisos : [];
  return ADMIN_NAVIGATION.filter((module) => {
    if (module.adminOnly && !isAdmin) return false;
    if (isAdmin) return true;
    if (esCuentaAdministrativa) return true;
    return module.requiredPermissions.some((permission) => permissions.includes(permission));
  });
}

export function obtenerGrupoAdministrativoActivo(pathname = "") {
  const module = ADMIN_NAVIGATION.find(
    (item) => pathname === item.path || pathname.startsWith(`${item.path}/`),
  );
  return module?.group || null;
}

export function getDefaultAdminRoute(session) {
  return obtenerModulosVisibles(session)[0]?.path || "/admin/panel/inicio";
}
