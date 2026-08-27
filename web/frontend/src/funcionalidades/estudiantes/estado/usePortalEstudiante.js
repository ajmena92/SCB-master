import { useCallback, useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { api, errMsg } from "@/lib/api";
import {
  obtenerVistaAsistencia,
  formatearCuentaRegresiva,
  formatearHoraServidor,
  estaProximoElCierre,
  analizarHoraServidor,
  segundosRestantesEn,
  horaServidorEn,
} from "@/funcionalidades/estudiantes/modelo/asistencia";

export function usePortalEstudiante() {
  const [menu, setMenu] = useState(null);
  const [vistaActiva, setVistaActiva] = useState("menu");
  const [estado, setEstado] = useState(null);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState("");
  const [ejecutando, setEjecutando] = useState(false);
  const [sincronizacion, setSincronizacion] = useState(null);
  const [ahoraMs, setAhoraMs] = useState(() => Date.now());
  const [paginaVisible, setPaginaVisible] = useState(
    () => typeof document === "undefined" || document.visibilityState !== "hidden",
  );
  const avisoMostradoRef = useRef(false);
  const colaActualizacionRef = useRef(Promise.resolve());
  const actualizacionAperturaRef = useRef(false);
  const actualizacionCierreRef = useRef(false);
  const tarjetaConfirmacionRef = useRef(null);
  const enfocarConfirmacionRef = useRef(false);

  const cargar = useCallback(async () => {
    const actualizar = async () => {
      setError("");
      const [resultadoMenu, resultadoAsistencia] = await Promise.allSettled([
        api.get("/v1/estudiantes/menu"),
        api.get("/v1/estudiantes/asistencia/hoy"),
      ]);

      if (resultadoMenu.status === "fulfilled") setMenu(resultadoMenu.value.data.menu);

      if (resultadoAsistencia.status === "fulfilled") {
        const siguienteEstado = resultadoAsistencia.value.data;
        const sincronizadoEn = Date.now();
        setEstado(siguienteEstado);
        setSincronizacion({
          segundosParaApertura: Number.isFinite(siguienteEstado.segundosParaApertura)
            ? Math.max(0, siguienteEstado.segundosParaApertura)
            : null,
          segundosParaCierre: Number.isFinite(siguienteEstado.segundosParaCierre)
            ? Math.max(0, siguienteEstado.segundosParaCierre)
            : null,
          horaServidorSegundos: analizarHoraServidor(siguienteEstado.horaServidor),
          sincronizadoEn,
        });
        setAhoraMs(sincronizadoEn);
      } else {
        setError(errMsg(resultadoAsistencia.reason));
      }
      setCargando(false);
    };

    const actualizacionProgramada = colaActualizacionRef.current.then(actualizar, actualizar);
    colaActualizacionRef.current = actualizacionProgramada.catch(() => undefined);
    return actualizacionProgramada;
  }, []);

  const carnet = useQuery({
    queryKey: ["estudiante", "carnet"],
    queryFn: async () => (await api.get("/v1/estudiantes/carnet")).data,
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
    if (segundosParaApertura > 0) actualizacionAperturaRef.current = false;
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
    if (segundosParaCierre > 0) actualizacionCierreRef.current = false;
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

  const registrarAsistencia = async (tipo) => {
    setEjecutando(true);
    try {
      await api.post(`/v1/estudiantes/asistencia/${tipo}`);
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
    horaServidor,
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
