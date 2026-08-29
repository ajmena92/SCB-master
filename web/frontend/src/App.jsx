import "@/App.css";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import {
  ProveedorAutenticacion,
  useAutenticacion,
} from "@/aplicacion/estado/ContextoAutenticacion";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { Toaster } from "@/components/ui/sonner";
import StudentLogin from "@/pages/StudentLogin";
import ChangePin from "@/pages/ChangePin";
import PaginaPortalEstudiante from "@/funcionalidades/estudiantes/paginas/PaginaPortalEstudiante";
import AdminLogin from "@/pages/AdminLogin";
import AdminPanel from "@/pages/AdminPanel";
import AdminModule from "@/pages/AdminModule";
import AdminGroupHub from "@/pages/AdminGroupHub";

function Inicio() {
  const { session, debeCambiarPin } = useAutenticacion();
  if (session === null)
    return <div className="min-h-screen flex items-center justify-center">Cargando…</div>;
  if (!session) return <StudentLogin />;
  if (session.tipo === "admin") return <Navigate to="/admin/panel" replace />;
  return <Navigate to={debeCambiarPin ? "/cambiar-pin" : "/comedor"} replace />;
}

function App() {
  return (
    <div className="App">
      <ProveedorAutenticacion>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Inicio />} />
            <Route
              path="/cambiar-pin"
              element={
                <ProtectedRoute tipo="estudiante">
                  <ChangePin />
                </ProtectedRoute>
              }
            />
            <Route
              path="/comedor"
              element={
                <ProtectedRoute tipo={["estudiante", "profesor"]}>
                  <PaginaPortalEstudiante />
                </ProtectedRoute>
              }
            />
            <Route path="/admin" element={<AdminLogin />} />
            <Route
              path="/admin/panel"
              element={
                <ProtectedRoute tipo="admin">
                  <AdminPanel />
                </ProtectedRoute>
              }
            >
              <Route index element={<Navigate to="inicio" replace />} />
              <Route path="inicio" element={<AdminModule moduleId="dashboard" />} />
              <Route path="operacion" element={<AdminGroupHub groupId="operacion" />} />
              <Route path="operacion/menu" element={<AdminModule moduleId="menu" />} />
              <Route path="operacion/calendario" element={<AdminModule moduleId="calendario" />} />
              <Route
                path="operacion/sustituciones"
                element={<AdminModule moduleId="sustituciones" />}
              />
              <Route path="operacion/rutas" element={<AdminModule moduleId="rutas" />} />
              <Route path="operacion/asistencia" element={<AdminModule moduleId="asistencia" />} />
              <Route
                path="operacion/correcciones"
                element={<AdminModule moduleId="correcciones" />}
              />
              <Route path="personas/estudiantes" element={<AdminModule moduleId="estudiantes" />} />
              <Route path="personas/beneficios" element={<AdminModule moduleId="beneficios" />} />
              <Route path="personas/cuentas" element={<AdminModule moduleId="cuentas" />} />
              <Route path="personas" element={<AdminGroupHub groupId="personas" />} />
              <Route path="reportes/transporte" element={<AdminModule moduleId="reporte" />} />
              <Route path="reportes" element={<AdminGroupHub groupId="reportes" />} />
              <Route path="reportes/general" element={<AdminModule moduleId="reporte" />} />
              <Route path="mas/parametros" element={<AdminModule moduleId="parametros" />} />
              <Route path="mas/auditoria" element={<AdminModule moduleId="auditoria" />} />
              <Route path="mas/importaciones" element={<AdminModule moduleId="importaciones" />} />
              <Route path="*" element={<Navigate to="inicio" replace />} />
            </Route>
            <Route
              path="/admin/comedor/operacion"
              element={
                <ProtectedRoute tipo="admin">
                  <AdminModule moduleId="comedor" />
                </ProtectedRoute>
              }
            />
          </Routes>
        </BrowserRouter>
        <Toaster position="top-center" richColors />
      </ProveedorAutenticacion>
    </div>
  );
}

export default App;
