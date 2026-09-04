import { useCallback, useRef, useState } from "react";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { analizarHoraServidor } from "@/funcionalidades/estudiantes/modelo/asistencia";
import { fechaLocalActual } from "./fecha_portal";
import type { EstadoPortalApi, MenuEstudiante, SincronizacionPortal } from "./tipos_portal";

export function useCargaPortal() {
  const [menu, setMenu] = useState<MenuEstudiante | null>(null);
  const [estado, setEstado] = useState<EstadoPortalApi | null>(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");
  const [sincronizacion, setSincronizacion] = useState<SincronizacionPortal | null>(null);
  const [ahoraMs, setAhoraMs] = useState(() => Date.now());
  const colaActualizacionRef = useRef(Promise.resolve());

  const cargar = useCallback(async () => {
    const actualizar = async () => {
      setError("");
      const [resultadoMenu, resultadoEstado] = await Promise.allSettled([
        api.get("/v1/portal/estado", { params: { fecha: fechaLocalActual() } }),
        api.get("/v1/portal/estado", { params: { fecha: fechaLocalActual() } }),
      ]);
      if (resultadoMenu.status === "fulfilled")
        setMenu(resultadoMenu.value.data.menu as MenuEstudiante | null);

      if (resultadoEstado.status === "fulfilled") {
        const respuestaEstado = resultadoEstado.value.data;
        const siguienteEstado = (
          typeof respuestaEstado.estado === "object" && respuestaEstado.estado !== null
            ? respuestaEstado.estado
            : respuestaEstado
        ) as EstadoPortalApi;
        const sincronizadoEn = Date.now();
        setEstado(siguienteEstado);
        setSincronizacion({
          segundosParaApertura: Number.isFinite(siguienteEstado.segundosParaApertura)
            ? Math.max(0, Number(siguienteEstado.segundosParaApertura))
            : null,
          segundosParaCierre: Number.isFinite(siguienteEstado.segundosParaCierre)
            ? Math.max(0, Number(siguienteEstado.segundosParaCierre))
            : null,
          horaServidorSegundos: analizarHoraServidor(siguienteEstado.horaServidor),
          sincronizadoEn,
        });
        setAhoraMs(sincronizadoEn);
      } else {
        setError(errMsg(resultadoEstado.reason));
      }
      setCargando(false);
    };

    const actualizacionProgramada = colaActualizacionRef.current.then(actualizar, actualizar);
    colaActualizacionRef.current = actualizacionProgramada.catch(() => undefined);
    return actualizacionProgramada;
  }, []);

  return { menu, estado, cargando, error, sincronizacion, ahoraMs, setAhoraMs, cargar };
}
