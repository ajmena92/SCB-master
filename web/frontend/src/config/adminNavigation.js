import {
  CalendarDays,
  FileSpreadsheet,
  KeyRound,
  LayoutDashboard,
  Route,
  Ticket,
  UtensilsCrossed,
  Users,
} from "lucide-react";

export const ADMIN_NAVIGATION_GROUPS = [
  { id: "principal", label: "Principal" },
  { id: "operacion", label: "Operación diaria" },
  { id: "administracion", label: "Administración" },
];

export const ADMIN_NAVIGATION = [
  {
    id: "dashboard",
    label: "Dashboard",
    shortLabel: "Inicio",
    group: "principal",
    path: "/admin/panel/inicio",
    requiredPermissions: ["dashboard.leer"],
    icon: LayoutDashboard,
  },
  {
    id: "comedor",
    label: "Ingreso al comedor",
    shortLabel: "Comedor",
    group: "operacion",
    path: "/admin/panel/comedor",
    requiredPermissions: ["comedor.operar"],
    icon: UtensilsCrossed,
  },
  {
    id: "rutas",
    label: "Rutas y transporte",
    shortLabel: "Rutas",
    group: "operacion",
    path: "/admin/panel/rutas",
    requiredPermissions: ["transporte.operar", "rutas.administrar"],
    icon: Route,
  },
  {
    id: "menu",
    label: "Menú del comedor",
    shortLabel: "Menú",
    group: "operacion",
    path: "/admin/panel/menu",
    requiredPermissions: ["menu.administrar"],
    icon: CalendarDays,
  },
  {
    id: "tiquetes",
    label: "Tiquetes y tarifas",
    shortLabel: "Tiquetes",
    group: "operacion",
    path: "/admin/panel/tiquetes",
    requiredPermissions: ["tiquetes.operar", "tarifas.administrar"],
    icon: Ticket,
  },
  {
    id: "personas",
    label: "Personas y matrículas",
    shortLabel: "Personas",
    group: "administracion",
    path: "/admin/panel/personas",
    requiredPermissions: ["personas.administrar"],
    icon: Users,
  },
  {
    id: "reportes",
    label: "Reportes",
    shortLabel: "Reportes",
    group: "administracion",
    path: "/admin/panel/reportes",
    requiredPermissions: ["reportes.leer"],
    icon: FileSpreadsheet,
  },
  {
    id: "anios",
    label: "Años e importación",
    shortLabel: "Años",
    group: "administracion",
    path: "/admin/panel/anios",
    requiredPermissions: ["importaciones.administrar"],
    icon: FileSpreadsheet,
  },
  {
    id: "usuarios",
    label: "Usuarios y permisos",
    shortLabel: "Usuarios",
    group: "administracion",
    path: "/admin/panel/usuarios",
    requiredPermissions: [],
    icon: KeyRound,
    adminOnly: true,
  },
];

export function isAdministratorSession(session) {
  return session?.tipo === "administracion" && session?.rol === "administrador";
}

export function obtenerModulosVisibles(session) {
  if (session?.tipo !== "administracion") return [];
  if (isAdministratorSession(session)) return ADMIN_NAVIGATION;
  const permissions = Array.isArray(session.permisos) ? session.permisos : [];
  return ADMIN_NAVIGATION.filter(
    (module) =>
      !module.adminOnly &&
      module.requiredPermissions.some((permission) => permissions.includes(permission)),
  );
}

export function obtenerGrupoAdministrativoActivo(pathname = "") {
  const module = ADMIN_NAVIGATION.find(
    (item) => pathname === item.path || pathname.startsWith(`${item.path}/`),
  );
  return module?.group || null;
}

export function obtenerRutaAdministrativaPredeterminada(session) {
  return obtenerModulosVisibles(session)[0]?.path || null;
}
