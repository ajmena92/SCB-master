import { useCallback, useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  reservarComedorEstudiante,
  cancelarComedorEstudiante,
} from "@/funcionalidades/comedor/consultas/reservas";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import {
  obtenerVistaAsistencia,
  formatearCuentaRegresiva,
  formatearHoraServidor,
  estaProximoElCierre,
  analizarHoraServidor,
  segundosRestantesEn,
  horaServidorEn,
} from "@/funcionalidades/estudiantes/modelo/asistencia";

export type EstadoPortalApi = {
  segundosParaApertura?: number;
  segundosParaCierre?: number;
  horaServidor?: string;
  periodoAbierto?: boolean;
  periodoCerrado?: boolean;
  estado?: string;
  minutosAvisoPrevio?: number;
  descripcionHorario?: string;
  horaLimite?: string;
  horaInicio?: string;
  fechaHoraConfirmacionServidor?: string;
};
export type TipoPersonaComedor = "estudiante" | "profesor";

function fechaLocalActual(): string {
  const ahora = new Date();
  const completar = (valor: number) => String(valor).padStart(2, "0");
  return `${ahora.getFullYear()}-${completar(ahora.getMonth() + 1)}-${completar(ahora.getDate())}`;
}
export type MenuEstudiante = {
  Titulo: string;
  Componentes: Array<{ Orden: number; Nombre: string; TipoComponente: string }>;
  Observaciones?: string;
  origen?: string;
};
export type EstadoPortal = {
  menu: MenuEstudiante | null;
  vistaActiva: "menu" | "carnet";
  setVistaActiva: (vista: "menu" | "carnet") => void;
  estado: EstadoPortalApi | null;
  cargando: boolean;
  error: string;
  ejecutando: boolean;
  cerrado: boolean;
  abierto: boolean;
  asistenciaConfirmada: boolean;
  rechazada: boolean;
  servicioDisponible: boolean;
  cuentaRegresiva: string | null;
  horaServidor: string | null;
  cierreProximo: boolean;
  vistaAsistencia: string;
  tarjetaConfirmacion: React.RefObject<HTMLElement | null>;
  registrarAsistencia: (tipo: "confirm" | "decline") => Promise<void>;
  cargar: () => Promise<unknown>;
  carnet: {
    datos: Record<string, unknown> | null;
    error: string;
    cargando: boolean;
    recargar: () => unknown;
  };
};

export function usePortalEstudiante(tipoPersona: TipoPersonaComedor = "estudiante"): EstadoPortal {
  const [menu, setMenu] = useState<MenuEstudiante | null>(null);
  const [vistaActiva, setVistaActiva] = useState<"menu" | "carnet">("menu");
  const [estado, setEstado] = useState<EstadoPortalApi | null>(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");
  const [ejecutando, setEjecutando] = useState(false);
  const [sincronizacion, setSincronizacion] = useState<{
    segundosParaApertura: number | null;
    segundosParaCierre: number | null;
    horaServidorSegundos: number | null;
    sincronizadoEn: number;
  } | null>(null);
  const [ahoraMs, setAhoraMs] = useState(() => Date.now());
  const [paginaVisible, setPaginaVisible] = useState(
    () => typeof document === "undefined" || document.visibilityState !== "hidden",
  );
  const avisoMostradoRef = useRef(false);
  const colaActualizacionRef = useRef(Promise.resolve());
  const actualizacionAperturaRef = useRef(false);
  const actualizacionCierreRef = useRef(false);
  const tarjetaConfirmacionRef = useRef<HTMLElement | null>(null);
  const enfocarConfirmacionRef = useRef(false);

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

  const carnet = useQuery({
    queryKey: [tipoPersona, "carnet"],
    queryFn: async () =>
      (await api.get("/v1/portal/carnet", { params: { fecha: fechaLocalActual() } })).data,
    enabled: vistaActiva === "carnet",
    retry: false,
  });

  useEffect(() => {
    void cargar();
  }, [cargar]);

  const segundosParaCierre = segundosRestantesEn(
    sincronizacion?.segundosParaCierre,
    sincronizacion?.sincronizadoEn,
    ahoraMs,
  );
  const segundosParaApertura = segundosRestantesEn(
    sincronizacion?.segundosParaApertura,
    sincronizacion?.sincronizadoEn,
    ahoraMs,
  );
  const segundosHoraServidor = horaServidorEn(
    sincronizacion?.horaServidorSegundos,
    sincronizacion?.sincronizadoEn,
    ahoraMs,
  );
  const cierreLocal = Boolean(estado?.periodoAbierto) && segundosParaCierre === 0;
  const cerrado = Boolean(estado?.periodoCerrado) || cierreLocal;
  const abierto = Boolean(estado?.periodoAbierto) && !cerrado;
  const asistenciaConfirmada = estado?.estado === "Confirmada" || estado?.estado === "Corregida";
  const debeActualizarReloj =
    Boolean(estado) &&
    !cerrado &&
    Number.isFinite(sincronizacion?.horaServidorSegundos) &&
    paginaVisible;

  useEffect(() => {
    if (!debeActualizarReloj) return undefined;
    const temporizador = setInterval(() => setAhoraMs(Date.now()), 1_000);
    return () => clearInterval(temporizador);
  }, [debeActualizarReloj]);

  useEffect(() => {
    const temporizador = setInterval(() => void cargar(), 60_000);
    return () => clearInterval(temporizador);
  }, [cargar]);

  useEffect(() => {
    const sincronizarVisibilidad = () => {
      const visible = document.visibilityState !== "hidden";
      setPaginaVisible(visible);
      if (visible) {
        setAhoraMs(Date.now());
        void cargar();
      }
    };
    document.addEventListener("visibilitychange", sincronizarVisibilidad);
    return () => document.removeEventListener("visibilitychange", sincronizarVisibilidad);
  }, [cargar]);

  useEffect(() => {
    if (segundosParaApertura !== null && segundosParaApertura > 0)
      actualizacionAperturaRef.current = false;
    if (
      !estado?.periodoAbierto &&
      !estado?.periodoCerrado &&
      segundosParaApertura === 0 &&
      !actualizacionAperturaRef.current
    ) {
      actualizacionAperturaRef.current = true;
      void cargar();
    }
  }, [segundosParaApertura, estado, cargar]);

  useEffect(() => {
    if (segundosParaCierre !== null && segundosParaCierre > 0)
      actualizacionCierreRef.current = false;
    if (
      estado?.periodoAbierto &&
      !estado?.periodoCerrado &&
      segundosParaCierre === 0 &&
      !actualizacionCierreRef.current
    ) {
      actualizacionCierreRef.current = true;
      void cargar();
    }
  }, [segundosParaCierre, estado, cargar]);

  const minutosAvisoConfigurados = Number(estado?.minutosAvisoPrevio);
  const minutosAviso =
    Number.isFinite(minutosAvisoConfigurados) && minutosAvisoConfigurados > 0
      ? minutosAvisoConfigurados
      : 15;
  const estadoParaVista =
    cerrado === Boolean(estado?.periodoCerrado) ? estado : { ...estado, periodoCerrado: cerrado };
  const vistaAsistencia = obtenerVistaAsistencia(estadoParaVista);
  const rechazada = estado?.estado === "Cancelada";
  const servicioDisponible = Boolean(menu);
  const cuentaRegresiva = formatearCuentaRegresiva(segundosParaCierre);
  const horaServidor = formatearHoraServidor(segundosHoraServidor) || estado?.horaServidor;
  const cierreProximo = estaProximoElCierre(segundosParaCierre, minutosAviso);

  useEffect(() => {
    if (cierreProximo && !asistenciaConfirmada && !cerrado && !avisoMostradoRef.current) {
      avisoMostradoRef.current = true;
      toast.warning(
        `Faltan menos de ${minutosAviso} minutos para el cierre. ¡No olvidés confirmar tu asistencia!`,
      );
    }
  }, [cerrado, cierreProximo, asistenciaConfirmada, minutosAviso]);

  useEffect(() => {
    if (asistenciaConfirmada && enfocarConfirmacionRef.current) {
      tarjetaConfirmacionRef.current?.focus();
      enfocarConfirmacionRef.current = false;
    }
  }, [asistenciaConfirmada]);

  const registrarAsistencia = async (tipo: "confirm" | "decline") => {
    setEjecutando(true);
    const fecha = fechaLocalActual();
    try {
      if (tipo === "confirm") {
        await reservarComedorEstudiante(fecha);
      } else {
        await cancelarComedorEstudiante(fecha);
      }
      if (tipo === "confirm") enfocarConfirmacionRef.current = true;
      toast.success(
        tipo === "confirm" ? "¡Asistencia confirmada!" : "Registrado: no asistirás hoy",
      );
      await cargar();
    } catch (errorAsistencia) {
      toast.error(errMsg(errorAsistencia));
    } finally {
      setEjecutando(false);
    }
  };

  return {
    menu,
    vistaActiva,
    setVistaActiva,
    estado,
    cargando,
    error,
    ejecutando,
    cerrado,
    abierto,
    asistenciaConfirmada,
    rechazada,
    servicioDisponible,
    cuentaRegresiva,
    horaServidor: horaServidor ?? null,
    cierreProximo,
    vistaAsistencia,
    tarjetaConfirmacion: tarjetaConfirmacionRef,
    registrarAsistencia,
    cargar,
    carnet: {
      datos: carnet.data ?? null,
      error: carnet.error ? errMsg(carnet.error) : "",
      cargando: carnet.isPending,
      recargar: carnet.refetch,
    },
  };
}
