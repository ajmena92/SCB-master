import { lazy } from "react";
import {
  CalendarDays,
  CalendarRange,
  FileSpreadsheet,
  LayoutDashboard,
  Replace,
  Route,
  ScrollText,
  Settings2,
  Users,
  Wrench,
} from "lucide-react";

// The catalog is the single source of truth for the administrative navigation.
// The current tab components are intentionally kept lazy while the application
// is incrementally moved from tabs to real routes in a later phase.
const DashboardTab = lazy(() => import("@/funcionalidades/administracion/paginas/Dashboard"));
const Plantillas = lazy(() => import("@/funcionalidades/menu/paginas/Plantillas"));
const CalendarioTab = lazy(() => import("@/funcionalidades/administracion/paginas/Calendario"));
const SustitucionesTab = lazy(() => import("@/funcionalidades/administracion/paginas/Sustituciones"));
const RutasTab = lazy(() => import("@/funcionalidades/rutas/paginas/Rutas"));
const CorreccionesTab = lazy(() => import("@/funcionalidades/administracion/paginas/Correcciones"));
const ParametrosTab = lazy(() => import("@/funcionalidades/administracion/paginas/Parametros"));
const Estudiantes = lazy(() => import("@/funcionalidades/administracion/paginas/Estudiantes"));
const Asistencia = lazy(() => import("@/funcionalidades/administracion/paginas/Asistencia"));
const Beneficios = lazy(() => import("@/funcionalidades/administracion/paginas/Beneficios"));
const Cuentas = lazy(() => import("@/funcionalidades/administracion/paginas/Cuentas"));
const Reportes = lazy(() => import("@/funcionalidades/administracion/paginas/Reportes"));
const Importaciones = lazy(() => import("@/funcionalidades/administracion/paginas/Importaciones"));
const Auditoria = lazy(() => import("@/funcionalidades/administracion/paginas/AuditoriaEventos"));

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
    shortLabel: "Inicio",
    group: "inicio",
    path: "/admin/panel/inicio",
    // Debe coincidir con el permiso que exige GET /v1/dashboard.
    requiredPermissions: ["reportes.dashboard.leer"],
    icon: LayoutDashboard,
    C: DashboardTab,
  },
  {
    v: "menu",
    id: "menu",
    label: "Menú",
    shortLabel: "Menú",
    group: "operacion",
    path: "/admin/panel/operacion/menu",
    requiredPermissions: ["admin.menu.read"],
    icon: CalendarDays,
    C: Plantillas,
  },
  {
    v: "calendario",
    id: "calendario",
    label: "Calendario",
    shortLabel: "Calendario",
    group: "operacion",
    path: "/admin/panel/operacion/calendario",
    requiredPermissions: ["admin.calendario.read"],
    icon: CalendarRange,
    C: CalendarioTab,
  },
  {
    v: "sustituciones",
    id: "sustituciones",
    label: "Sustituciones",
    shortLabel: "Cambios",
    group: "operacion",
    path: "/admin/panel/operacion/sustituciones",
    requiredPermissions: ["admin.sustituciones.read"],
    icon: Replace,
    C: SustitucionesTab,
  },
  {
    v: "rutas",
    id: "rutas",
    label: "Rutas",
    shortLabel: "Rutas",
    group: "operacion",
    path: "/admin/panel/operacion/rutas",
    requiredPermissions: ["rutas.administrar"],
    icon: Route,
    C: RutasTab,
  },
  {
    v: "correcciones",
    id: "correcciones",
    label: "Correcciones",
    shortLabel: "Correcciones",
    group: "operacion",
    path: "/admin/panel/operacion/correcciones",
      requiredPermissions: ["asistencia.correcciones.editar"],
    icon: Wrench,
    C: CorreccionesTab,
    adminOnly: true,
  },
  {
    v: "estudiantes",
    id: "estudiantes",
    label: "Estudiantes / PIN",
    shortLabel: "Personas",
    group: "personas",
    path: "/admin/panel/personas/estudiantes",
    requiredPermissions: ["estudiantes.leer"],
    icon: Users,
    C: Estudiantes,
  },
  {
    v: "asistencia",
    id: "asistencia",
    label: "Asistencia",
    shortLabel: "Asistencia",
    group: "operacion",
    path: "/admin/panel/operacion/asistencia",
    requiredPermissions: ["asistencia.leer"],
    icon: CalendarRange,
    C: Asistencia,
  },
  {
    v: "beneficios",
    id: "beneficios",
    label: "Beneficios",
    shortLabel: "Beneficios",
    group: "personas",
    path: "/admin/panel/personas/beneficios",
    requiredPermissions: ["beneficios.leer"],
    icon: Users,
    C: Beneficios,
  },
  {
    v: "cuentas",
    id: "cuentas",
    label: "Cuentas y saldos",
    shortLabel: "Cuentas",
    group: "personas",
    path: "/admin/panel/personas/cuentas",
    requiredPermissions: ["cuentas.leer"],
    icon: FileSpreadsheet,
    C: Cuentas,
  },
  {
    v: "reporte",
    id: "reporte",
    label: "Reportes",
    shortLabel: "Reportes",
    group: "reportes",
    path: "/admin/panel/reportes/transporte",
    requiredPermissions: ["admin.reportes.transporte.read"],
    icon: FileSpreadsheet,
    C: Reportes,
  },
  {
    v: "importaciones",
    id: "importaciones",
    label: "Importaciones",
    shortLabel: "Importar",
    group: "mas",
    path: "/admin/panel/mas/importaciones",
    requiredPermissions: ["importaciones.leer"],
    icon: FileSpreadsheet,
    C: Importaciones,
  },
  {
    v: "parametros",
    id: "parametros",
    label: "Parámetros",
    shortLabel: "Más",
    group: "mas",
    path: "/admin/panel/mas/parametros",
    requiredPermissions: ["admin.parametros.read"],
    icon: Settings2,
    C: ParametrosTab,
  },
  {
    v: "auditoria",
    id: "auditoria",
    label: "Auditoría",
    shortLabel: "Auditoría",
    group: "mas",
    path: "/admin/panel/mas/auditoria",
      requiredPermissions: ["auditoria.leer"],
    icon: ScrollText,
    C: Auditoria,
  },
];

function roleFromSession(session) {
  if (typeof session === "string") return session;
  if (session === true) return "Administrador";
  return session?.usuario?.Rol || session?.Rol || "";
}

export function isAdministratorSession(session) {
  // Toda sesión emitida por el módulo de identidad administrativa ya fue
  // autenticada por la API. El rol se conserva para restricciones específicas,
  // pero su ausencia no debe convertir a un administrador válido en visitante.
  if (session?.tipo === "admin") return true;
  // Compatibilidad de forma, no de ruta: algunas restauraciones antiguas
  // conservaron únicamente el identificador administrativo.
  if (Number.isInteger(session?.usuario?.idUsuario)) return true;
  const role = roleFromSession(session).toLocaleLowerCase();
  return role === "administrador" || role === "admin";
}

export function getVisibleAdminModules(session) {
  const isAdmin = isAdministratorSession(session);
  const permissions = Array.isArray(session?.permisos) ? session.permisos : [];
  return ADMIN_NAVIGATION.filter((module) => {
    if (module.adminOnly && !isAdmin) return false;
    if (isAdmin) return true;
    return module.requiredPermissions.some((permission) => permissions.includes(permission));
  });
}

// Short alias for consumers that do not need the administrative prefix.
export const getVisibleModules = getVisibleAdminModules;

export function getActiveAdminGroup(pathname = "") {
  const module = ADMIN_NAVIGATION.find(
    (item) => pathname === item.path || pathname.startsWith(`${item.path}/`),
  );
  return module?.group || null;
}

export const getActiveGroup = getActiveAdminGroup;

export function getDefaultAdminRoute(session) {
  return getVisibleAdminModules(session)[0]?.path || "/admin/panel/inicio";
}

export const getDefaultRoute = getDefaultAdminRoute;
