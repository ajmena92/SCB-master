import "@/App.css";
import "@/plataforma.css";
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
import AdminShell from "@/funcionalidades/plataforma/componentes/AdminShell";
import RutaRol from "@/funcionalidades/plataforma/componentes/RutaRol";
import InicioAdministracion from "@/funcionalidades/plataforma/paginas/InicioAdministracion";
import PersonasMatriculas from "@/funcionalidades/plataforma/paginas/PersonasMatriculas";
import AniosImportacion from "@/funcionalidades/plataforma/paginas/AniosImportacion";
import RutasTransporte from "@/funcionalidades/plataforma/paginas/RutasTransporte";
import MenuPlanificado from "@/funcionalidades/plataforma/paginas/MenuPlanificado";
import TarifasVentas from "@/funcionalidades/plataforma/paginas/TarifasVentas";
import OperacionComedor from "@/funcionalidades/plataforma/paginas/OperacionComedor";
import ReportesOperativos from "@/funcionalidades/plataforma/paginas/ReportesOperativos";
import PortalPersona from "@/funcionalidades/plataforma/paginas/PortalPersona";

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
      <PortalPersona />
    </ProtectedRoute>
  );
}

export default function App() {
  return (
    <ProveedorAutenticacion>
      <BrowserRouter>
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
          <Route path="/comedor" element={<Navigate to="/portal" replace />} />
          <Route
            path="/admin/panel"
            element={
              <ProtectedRoute tipo="admin">
                <AdminShell />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="inicio" replace />} />
            <Route path="inicio" element={<InicioAdministracion />} />
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
                  <RutasTransporte />
                </RutaRol>
              }
            />
            <Route path="transporte" element={<RutasTransporte />} />
            <Route
              path="menu"
              element={
                <RutaRol soloAdministrador>
                  <MenuPlanificado />
                </RutaRol>
              }
            />
            <Route path="tiquetes" element={<TarifasVentas />} />
            <Route path="comedor" element={<OperacionComedor />} />
            <Route path="reportes" element={<ReportesOperativos />} />
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
      <Toaster position="top-center" richColors />
    </ProveedorAutenticacion>
  );
}
