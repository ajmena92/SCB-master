import { useEffect, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import type { ConfiguracionOperacionSalida, IngresoSalida } from "@/compartido/contratos/comedor";
import {
  consultarConfiguracionComedor,
  registrarMarcacionComedor,
  consultarHistorialComedor,
} from "@/funcionalidades/comedor/consultas/marcacion";
import { clasificarErrorOperacion } from "@/funcionalidades/comedor/consultas/erroresOperacion";

export function useMarcacionComedor() {
  const [codigoBarras, setCodigoBarras] = useState("");
  const [fecha, setFecha] = useState(fechaLocalActual);
  const [guardando, setGuardando] = useState(false);
  const [modoManual, setModoManual] = useState(false);
  const [altoContraste, setAltoContraste] = useState(false);
  const [historial, setHistorial] = useState<IngresoSalida[]>([]);
  const [pequeno, setPequeno] = useState(
    () => window.innerWidth < 1280 || window.innerHeight < 720,
  );
  const [ultimoIngreso, setUltimoIngreso] = useState<IngresoSalida | null>(null);
  const [errorOperacion, setErrorOperacion] = useState<{ codigo: string; mensaje: string } | null>(
    null,
  );
  const [totalIngresos, setTotalIngresos] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);
  const registroEnCursoRef = useRef(false);
  const limpiezaResultadoRef = useRef<number | null>(null);
  const configuracion = useQuery<ConfiguracionOperacionSalida>({
    queryKey: ["comedor", "configuracion-operacion"],
    queryFn: consultarConfiguracionComedor,
    staleTime: 60_000,
    refetchInterval: 30_000,
  });
  const horarios = (configuracion.data?.horarios ?? []).filter(
    (horario) => horario.activo !== false,
  );
  useEffect(() => {
    const actualizarTamano = () => setPequeno(window.innerWidth < 1280 || window.innerHeight < 720);
    window.addEventListener("resize", actualizarTamano);
    return () => window.removeEventListener("resize", actualizarTamano);
  }, []);

  useEffect(() => {
    const intervalo = window.setInterval(() => setFecha(fechaLocalActual()), 60_000);
    return () => {
      window.clearInterval(intervalo);
      if (limpiezaResultadoRef.current !== null) {
        window.clearTimeout(limpiezaResultadoRef.current);
      }
    };
  }, []);

  async function registrar() {
    if (registroEnCursoRef.current) return;
    if (!codigoBarras.trim()) {
      toast.error("Lea o indique el código de barras");
      return;
    }
    registroEnCursoRef.current = true;
    setGuardando(true);
    setErrorOperacion(null);
    try {
      const ingreso = await registrarMarcacionComedor({
        codigoBarras,
        fecha,
      });
      toast.success(
        ingreso.modalidad === "tiquete" ? "Tiquete consumido" : "Ingreso becado registrado",
      );
      setCodigoBarras("");
      setErrorOperacion(null);
      setUltimoIngreso(ingreso);
      setHistorial((actual) => [ingreso, ...actual].slice(0, 10));
      setTotalIngresos((actual) => actual + 1);
      if (limpiezaResultadoRef.current !== null) {
        window.clearTimeout(limpiezaResultadoRef.current);
      }
      limpiezaResultadoRef.current = window.setTimeout(() => {
        setUltimoIngreso(null);
        limpiezaResultadoRef.current = null;
      }, 60_000);
      window.setTimeout(() => inputRef.current?.focus(), 0);
    } catch (error) {
      const estado = clasificarErrorOperacion(error);
      setErrorOperacion(estado);
      toast.error(estado.mensaje);
    } finally {
      registroEnCursoRef.current = false;
      setGuardando(false);
    }
  }

  return {
    codigoBarras,
    fecha,
    horarios,
    configuracion,
    guardando,
    ultimoIngreso,
    errorOperacion,
    totalIngresos,
    inputRef,
    setCodigoBarras,
    setFecha,
    registrar,
    modoManual,
    setModoManual,
    altoContraste,
    setAltoContraste,
    historial,
    pequeno,
    horaServidor: configuracion.data?.horaServidor ?? "--:--:--",
    conexionDisponible: !configuracion.isError,
    recargarHistorial: async () => setHistorial(await consultarHistorialComedor(fecha)),
  };
}
