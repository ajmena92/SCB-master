import { useEffect, useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";
import {
  BookOpen,
  Bus,
  CalendarRange,
  CreditCard,
  FileBarChart,
  LogOut,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  ShieldCheck,
  Settings,
  Soup,
  Users,
} from "lucide-react";

const navegacion = [
  { ruta: "/admin/panel/inicio", titulo: "Inicio", icono: Menu },
  { ruta: "/admin/panel/personas", titulo: "Personas", icono: Users, admin: true },
  { ruta: "/admin/panel/anios", titulo: "Años e importación", icono: CalendarRange, admin: true },
  { ruta: "/admin/panel/rutas", titulo: "Rutas", icono: Bus, admin: true },
  { ruta: "/admin/panel/menu", titulo: "Menú", icono: BookOpen, admin: true },
  { ruta: "/admin/panel/tiquetes", titulo: "Tiquetes y ventas", icono: CreditCard },
  { ruta: "/admin/panel/parametros", titulo: "Parámetros", icono: Settings, admin: true },
  { ruta: "/admin/panel/comedor", titulo: "Comedor", icono: Soup },
  { ruta: "/admin/panel/reportes", titulo: "Reportes", icono: FileBarChart },
];

export default function AdminShell() {
  const { session, logout } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const location = useLocation();
  const administrador = esAdministrador(session);
  const usuario = session && typeof session.usuario === "object" ? session.usuario : undefined;
  const visibles = navegacion.filter((item) => !item.admin || administrador);
  const titulo =
    navegacion.find((item) => item.ruta === location.pathname)?.titulo ?? "Administración";
  const esEstacionComedor = location.pathname === "/admin/panel/comedor";
  const [navegacionColapsada, setNavegacionColapsada] = useState(esEstacionComedor);

  useEffect(() => {
    setNavegacionColapsada(esEstacionComedor);
  }, [esEstacionComedor]);

  return (
    <div className={`admin-app${esEstacionComedor ? " admin-app--comedor" : ""}${navegacionColapsada ? " admin-app--nav-colapsada" : ""}`}>
      <header className="topbar">
        <div className="brand">
          <ShieldCheck aria-hidden="true" />
          <div>
            <strong>CTP Platanares</strong>
            <span>Gestión de comedor</span>
          </div>
        </div>
        <div className="user-box">
          {esEstacionComedor && (
            <button
              type="button"
              onClick={() => setNavegacionColapsada((actual) => !actual)}
              aria-label={navegacionColapsada ? "Expandir menú" : "Colapsar menú"}
              title={navegacionColapsada ? "Expandir menú" : "Colapsar menú"}
            >
              {navegacionColapsada ? <PanelLeftOpen /> : <PanelLeftClose />}
            </button>
          )}
          <span>
            {String(usuario?.Nombre || usuario?.nombres || "Usuario")}
            <small>{administrador ? "Administrador" : "Operador"}</small>
          </span>
          <button onClick={logout} aria-label="Cerrar sesión">
            <LogOut />
          </button>
        </div>
      </header>
      <aside className={`admin-nav${navegacionColapsada ? " admin-nav--colapsada" : ""}`} aria-label="Navegación administrativa">
        {visibles.map(({ ruta, titulo: texto, icono: Icono }) => (
          <NavLink key={ruta} to={ruta} className={({ isActive }) => (isActive ? "active" : "")}>
            <Icono aria-hidden="true" />
            <span>{texto}</span>
          </NavLink>
        ))}
      </aside>
      <main className={`admin-main${esEstacionComedor ? " admin-main--comedor" : ""}`} id="contenido">
        {!esEstacionComedor && <span className="mobile-title">{titulo}</span>}
        <Outlet />
      </main>
    </div>
  );
}
