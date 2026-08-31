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
const PersonasMatriculas = lazy(
  () => import("@/funcionalidades/plataforma/paginas/PersonasMatriculas"),
);
const AniosImportacion = lazy(
  () => import("@/funcionalidades/plataforma/paginas/AniosImportacion"),
);
const Rutas = lazy(() => import("@/funcionalidades/rutas/paginas/Rutas"));
const PlantillasMenu = lazy(() => import("@/funcionalidades/menu/paginas/Plantillas"));
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
  if (session.tipo === "admin") return <Navigate to="/admin/panel/inicio" replace />;
  return <Navigate to={debeCambiarPin ? "/cambiar-pin" : "/portal"} replace />;
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
                <ProtectedRoute tipo="admin">
                  <AdminPanel />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="inicio" replace />} />
              <Route path="inicio" element={<Dashboard />} />
              <Route
                path="personas"
                element={
                  <RutaRol soloAdministrador>
                    <PersonasMatriculas />
                  </RutaRol>
                }
              />
              <Route
                path="anios"
                element={
                  <RutaRol soloAdministrador>
                    <AniosImportacion />
                  </RutaRol>
                }
              />
              <Route
                path="rutas"
                element={
                  <RutaRol soloAdministrador>
                    <Rutas />
                  </RutaRol>
                }
              />
              <Route path="transporte" element={<Navigate to="../rutas" replace />} />
              <Route
                path="menu"
                element={
                  <RutaRol soloAdministrador>
                    <PlantillasMenu />
                  </RutaRol>
                }
              />
              <Route path="tiquetes" element={<TarifasVentas />} />
              <Route path="comedor" element={<OperacionComedor />} />
              <Route path="reportes" element={<ReportesOperativos />} />
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
      <Toaster position="top-center" richColors />
    </ProveedorAutenticacion>
  );
}
