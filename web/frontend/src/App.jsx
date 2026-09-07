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
import { destinoSesion } from "@/aplicacion/estado/destinoSesion";

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
const ParametrosOperativos = lazy(
  () => import("@/funcionalidades/plataforma/paginas/ParametrosOperativos"),
);
const OperacionComedor = lazy(
  () => import("@/funcionalidades/plataforma/paginas/OperacionComedor"),
);
const ReportesOperativos = lazy(
  () => import("@/funcionalidades/plataforma/paginas/ReportesOperativos"),
);
const PaginaPortalEstudiante = lazy(
  () => import("@/funcionalidades/estudiantes/paginas/PaginaPortalEstudiante"),
);

function CargadorAplicacion({ children = "Cargando módulo…", pantallaCompleta = false }) {
  return (
    <div
      className={`flex ${pantallaCompleta ? "min-h-[100dvh]" : "min-h-[50vh]"} flex-col items-center justify-center gap-3 bg-background px-4 text-center text-sm text-muted-foreground`}
      role="status"
      aria-live="polite"
      aria-busy="true"
    >
      <span
        className="h-5 w-5 animate-spin rounded-full border-2 border-primary/25 border-t-primary"
        aria-hidden="true"
      />
      <span>{children}</span>
    </div>
  );
}

function Inicio() {
  const { session, debeCambiarPin } = useAutenticacion();
  if (session === null)
    return <CargadorAplicacion pantallaCompleta> Cargando plataforma…</CargadorAplicacion>;
  if (!session) return <StudentLogin />;
  return <Navigate to={destinoSesion(session, debeCambiarPin)} replace />;
}

function AccesoAdministrativo() {
  const { session, debeCambiarPin } = useAutenticacion();
  if (session === null)
    return <CargadorAplicacion pantallaCompleta> Cargando plataforma…</CargadorAplicacion>;
  const destino = destinoSesion(session, debeCambiarPin);
  if (destino) return <Navigate to={destino} replace />;
  return <AdminLogin />;
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
        <Suspense fallback={<CargadorAplicacion />}>
          <Routes>
            <Route path="/" element={<Inicio />} />
            <Route path="/admin" element={<AccesoAdministrativo />} />
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
            <Route path="/portal/menu" element={<PortalProtegido />} />
            <Route path="/portal/carnet" element={<PortalProtegido />} />
            <Route path="/comedor" element={<PortalProtegido />} />
            <Route path="/comedor/menu" element={<PortalProtegido />} />
            <Route path="/comedor/carnet" element={<PortalProtegido />} />
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
                path="estudiantes/expediente/:referencia"
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
                path="parametros"
                element={
                  <RutaRol permisos={["tarifas.administrar"]}>
                    <ParametrosOperativos />
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
      <Toaster position="top-right" richColors={false} visibleToasts={4} />
    </ProveedorAutenticacion>
  );
}
