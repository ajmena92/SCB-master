import { useEffect, useRef, useState } from "react";
import {
  formatearCuentaRegresiva,
  formatearHoraServidor,
  estaProximoElCierre,
  horaServidorEn,
  segundosRestantesEn,
} from "@/funcionalidades/estudiantes/modelo/asistencia";
import type { EstadoPortalApi, SincronizacionPortal } from "./tipos_portal";

type OpcionesRelojPortal = {
  estado: EstadoPortalApi | null;
  sincronizacion: SincronizacionPortal | null;
  ahoraMs: number;
  setAhoraMs: (ahora: number) => void;
  cargar: () => Promise<unknown>;
};

export function useRelojPortal({
  estado,
  sincronizacion,
  ahoraMs,
  setAhoraMs,
  cargar,
}: OpcionesRelojPortal) {
  const [paginaVisible, setPaginaVisible] = useState(
    () => typeof document === "undefined" || document.visibilityState !== "hidden",
  );
  const actualizacionAperturaRef = useRef(false);
  const actualizacionCierreRef = useRef(false);
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
  const debeActualizarReloj =
    Boolean(estado) &&
    !cerrado &&
    Number.isFinite(sincronizacion?.horaServidorSegundos) &&
    paginaVisible;

  useEffect(() => {
    if (!debeActualizarReloj) return undefined;
    const temporizador = setInterval(() => setAhoraMs(Date.now()), 1_000);
    return () => clearInterval(temporizador);
  }, [debeActualizarReloj, setAhoraMs]);

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
  }, [cargar, setAhoraMs]);

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

  return {
    segundosParaCierre,
    cerrado,
    abierto,
    minutosAviso,
    cuentaRegresiva: formatearCuentaRegresiva(segundosParaCierre),
    horaServidor: formatearHoraServidor(segundosHoraServidor) || estado?.horaServidor,
    cierreProximo: estaProximoElCierre(segundosParaCierre, minutosAviso),
  };
}
