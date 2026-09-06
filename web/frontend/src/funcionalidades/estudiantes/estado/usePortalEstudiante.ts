import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { notificar } from "@/compartido/notificaciones/notificaciones";
import {
  cancelarComedorEstudiante,
  reservarComedorEstudiante,
} from "@/funcionalidades/comedor/consultas/reservas";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { obtenerVistaAsistencia } from "@/funcionalidades/estudiantes/modelo/asistencia";
import { fechaLocalActual } from "./fecha_portal";
import { useCargaPortal } from "./useCargaPortal";
import { useRelojPortal } from "./useRelojPortal";
import type { EstadoPortal, TipoPersonaComedor } from "./tipos_portal";

export type {
  EstadoPortal,
  EstadoPortalApi,
  MenuEstudiante,
  TipoPersonaComedor,
} from "./tipos_portal";

export function usePortalEstudiante(
  tipoPersona: TipoPersonaComedor = "estudiante",
  vistaInicial: "menu" | "carnet" = "menu",
): EstadoPortal {
  const [vistaActiva, setVistaActiva] = useState<"menu" | "carnet">(vistaInicial);
  const [ejecutando, setEjecutando] = useState(false);
  const avisoMostradoRef = useRef(false);
  const tarjetaConfirmacionRef = useRef<HTMLElement | null>(null);
  const enfocarConfirmacionRef = useRef(false);
  const { menu, estado, cargando, error, sincronizacion, ahoraMs, setAhoraMs, cargar } =
    useCargaPortal();
  const { cerrado, abierto, minutosAviso, cuentaRegresiva, horaServidor, cierreProximo } =
    useRelojPortal({ estado, sincronizacion, ahoraMs, setAhoraMs, cargar });

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

  const asistenciaConfirmada = estado?.estado === "Confirmada" || estado?.estado === "Corregida";
  const estadoParaVista =
    cerrado === Boolean(estado?.periodoCerrado) ? estado : { ...estado, periodoCerrado: cerrado };
  const vistaAsistencia = obtenerVistaAsistencia(estadoParaVista);
  const rechazada = estado?.estado === "Cancelada";
  const servicioDisponible = Boolean(menu);

  useEffect(() => {
    if (cierreProximo && !asistenciaConfirmada && !cerrado && !avisoMostradoRef.current) {
      avisoMostradoRef.current = true;
      notificar.advertencia(
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
        const reserva = await reservarComedorEstudiante(fecha);
        if ((reserva as typeof reserva & { sin_tiquete?: boolean }).sin_tiquete) {
          notificar.advertencia("Asistencia confirmada, pero no tenés tiquetes disponibles.");
        } else {
          notificar.exito("¡Asistencia confirmada!");
        }
      } else {
        await cancelarComedorEstudiante(fecha);
        notificar.exito("Registrado: no asistirás hoy");
      }
      if (tipo === "confirm") enfocarConfirmacionRef.current = true;
      await cargar();
    } catch (errorAsistencia) {
      notificar.error(errMsg(errorAsistencia));
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
