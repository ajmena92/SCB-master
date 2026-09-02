import "@/App.css";
import "@/plataforma.css";
import { lazy, Suspense } from "react";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import {
  ProveedorAutenticacion,
  useAutenticacion,
} from "@/aplicacion/estado/ContextoAutenticacion";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { Toaster } from "@/components/ui/sonner";
import AdminLogin from "@/pages/AdminLogin";
import StudentLogin from "@/pages/StudentLogin";
import ChangePin from "@/pages/ChangePin";
import AdminPanel from "@/pages/AdminPanel";
import RutaRol from "@/funcionalidades/plataforma/componentes/RutaRol";

const Dashboard = lazy(() => import("@/funcionalidades/administracion/paginas/Dashboard"));
const UsuariosAdministrativos = lazy(
  () => import("@/funcionalidades/administracion/paginas/UsuariosAdministrativos"),
);
const VinculacionInicial = lazy(
  () => import("@/funcionalidades/administracion/paginas/VinculacionInicial"),
);
const CambioContrasenaAdministrativa = lazy(
  () => import("@/funcionalidades/administracion/paginas/CambioContrasenaAdministrativa"),
);
const PersonasMatriculas = lazy(
  () => import("@/funcionalidades/plataforma/paginas/PersonasMatriculas"),
);
const EditarEstudiante = lazy(
  () => import("@/funcionalidades/plataforma/paginas/EditarEstudiante"),
);
const AniosImportacion = lazy(
  () => import("@/funcionalidades/plataforma/paginas/AniosImportacion"),
);
const Rutas = lazy(() => import("@/funcionalidades/rutas/paginas/Rutas"));
const PlantillasMenu = lazy(() => import("@/funcionalidades/menu/paginas/Plantillas"));
const CalendarioMenu = lazy(() => import("@/funcionalidades/menu/paginas/CalendarioMenu"));
const TarifasVentas = lazy(() => import("@/funcionalidades/plataforma/paginas/TarifasVentas"));
const OperacionComedor = lazy(
  () => import("@/funcionalidades/plataforma/paginas/OperacionComedor"),
);
const ReportesOperativos = lazy(
  () => import("@/funcionalidades/plataforma/paginas/ReportesOperativos"),
);
const PaginaPortalEstudiante = lazy(
  () => import("@/funcionalidades/estudiantes/paginas/PaginaPortalEstudiante"),
);

function Inicio() {
  const { session, debeCambiarPin } = useAutenticacion();
  if (session === null) return <div className="splash">Cargando plataforma…</div>;
  if (!session) return <StudentLogin />;
  if (session.tipo === "administracion") {
    if (session.vinculacionPendiente) return <Navigate to="/admin/vinculacion-inicial" replace />;
    if (session.cambioContrasenaObligatorio)
      return <Navigate to="/admin/cambiar-contrasena" replace />;
    return <Navigate to="/admin/panel" replace />;
  }
  return <Navigate to={debeCambiarPin ? "/cambiar-pin" : "/portal"} replace />;
}

function PanelAdministrativoProtegido() {
  const { session } = useAutenticacion();
  if (session?.vinculacionPendiente) return <Navigate to="/admin/vinculacion-inicial" replace />;
  if (session?.cambioContrasenaObligatorio)
    return <Navigate to="/admin/cambiar-contrasena" replace />;
  return <AdminPanel />;
}

function PreparacionAdministrativa({ paso, children }) {
  const { session } = useAutenticacion();
  if (session?.tipo !== "administracion") return <Navigate to="/admin" replace />;
  if (paso === "vinculacion" && !session.vinculacionPendiente) return <Navigate to="/" replace />;
  if (paso === "contrasena" && session.vinculacionPendiente)
    return <Navigate to="/admin/vinculacion-inicial" replace />;
  if (paso === "contrasena" && !session.cambioContrasenaObligatorio)
    return <Navigate to="/" replace />;
  return children;
}

function PortalProtegido() {
  const { debeCambiarPin } = useAutenticacion();
  if (debeCambiarPin) return <Navigate to="/cambiar-pin" replace />;
  return (
    <ProtectedRoute tipo={["estudiante", "profesor"]}>
      <PaginaPortalEstudiante />
    </ProtectedRoute>
  );
}

export default function App() {
  return (
    <ProveedorAutenticacion>
      <BrowserRouter>
        <Suspense fallback={<div className="splash">Cargando módulo…</div>}>
          <Routes>
            <Route path="/" element={<Inicio />} />
            <Route path="/admin" element={<AdminLogin />} />
            <Route
              path="/admin/vinculacion-inicial"
              element={
                <ProtectedRoute tipo="administracion">
                  <PreparacionAdministrativa paso="vinculacion">
                    <VinculacionInicial />
                  </PreparacionAdministrativa>
                </ProtectedRoute>
              }
            />
            <Route
              path="/admin/cambiar-contrasena"
              element={
                <ProtectedRoute tipo="administracion">
                  <PreparacionAdministrativa paso="contrasena">
                    <CambioContrasenaAdministrativa />
                  </PreparacionAdministrativa>
                </ProtectedRoute>
              }
            />
            <Route
              path="/cambiar-pin"
              element={
                <ProtectedRoute tipo={["estudiante", "profesor"]}>
                  <ChangePin />
                </ProtectedRoute>
              }
            />
            <Route path="/portal" element={<PortalProtegido />} />
            <Route path="/comedor" element={<PortalProtegido />} />
            <Route
              path="/admin/panel"
              element={
                <ProtectedRoute tipo="administracion">
                  <PanelAdministrativoProtegido />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="inicio" replace />} />
              <Route
                path="inicio"
                element={
                  <RutaRol permisos={["dashboard.leer"]}>
                    <Dashboard />
                  </RutaRol>
                }
              />
              <Route
                path="personas"
                element={
                  <RutaRol permisos={["personas.administrar"]}>
                    <PersonasMatriculas />
                  </RutaRol>
                }
              />
              <Route
                path="estudiantes/:id"
                element={
                  <RutaRol permisos={["personas.administrar"]}>
                    <EditarEstudiante />
                  </RutaRol>
                }
              />
              <Route
                path="anios"
                element={
                  <RutaRol permisos={["importaciones.administrar"]}>
                    <AniosImportacion />
                  </RutaRol>
                }
              />
              <Route
              path="rutas"
              element={
                  <RutaRol permisos={["rutas.administrar"]}>
                    <Rutas />
                  </RutaRol>
                }
              />
              <Route
              path="menu"
                element={
                  <RutaRol permisos={["menu.administrar"]}>
                    <PlantillasMenu />
                  </RutaRol>
              }
            />
              <Route
                path="calendario-menu"
                element={
                  <RutaRol permisos={["menu.administrar"]}>
                    <CalendarioMenu />
                  </RutaRol>
                }
              />
              <Route
                path="tiquetes"
                element={
                  <RutaRol permisos={["tiquetes.operar", "tarifas.administrar"]}>
                    <TarifasVentas />
                  </RutaRol>
                }
              />
              <Route
                path="comedor"
                element={
                  <RutaRol permisos={["comedor.operar"]}>
                    <OperacionComedor />
                  </RutaRol>
                }
              />
              <Route
                path="reportes"
                element={
                  <RutaRol permisos={["reportes.leer"]}>
                    <ReportesOperativos />
                  </RutaRol>
                }
              />
              <Route
                path="usuarios"
                element={
                  <RutaRol soloAdministrador>
                    <UsuariosAdministrativos />
                  </RutaRol>
                }
              />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
      <Toaster position="top-center" richColors />
    </ProveedorAutenticacion>
  );
}
