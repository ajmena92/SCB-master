import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronDown, ScanBarcode } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { plataformaApi } from "../consultas/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import { Badge } from "@/components/ui/badge";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";
import { ExcepcionSinReserva } from "../componentes/ExcepcionSinReserva";
import { ControlesEstacionComedor } from "../componentes/ControlesEstacionComedor";
import { LectorQrCamara } from "../componentes/LectorQrCamara";
import { ResultadoLecturaComedor } from "../componentes/ResultadoLecturaComedor";
import { emitirTonoEstacionComedor } from "../componentes/sonido_estacion_comedor";

import { CapturaManualComedor } from "../componentes/captura_manual_comedor";

type EstadoCamara = "iniciando" | "activo" | "error";

export default function OperacionComedor() {
  const fecha = fechaLocalActual();
  const navegar = useNavigate();
  const clienteConsultas = useQueryClient();
  const entradaRef = useRef<HTMLInputElement>(null);
  const lecturaPendienteRef = useRef(false);
  const ultimaLecturaRef = useRef("");
  const [resultado, setResultado] = useState<ResultadoOperacion>();
  const [codigoExcepcion, setCodigoExcepcion] = useState("");
  const [mostrarHistorial, setMostrarHistorial] = useState(false);
  const [mostrarRespaldo, setMostrarRespaldo] = useState(false);
  const [silenciado, setSilenciado] = useState(
    () => localStorage.getItem("comedor-sonido") === "silenciado",
  );
  const [estadoCamara, setEstadoCamara] = useState<EstadoCamara>("iniciando");
  const [pantallaCompleta, setPantallaCompleta] = useState(false);
  const [ultimaLectura, setUltimaLectura] = useState("");
  const institucion = useQuery({
    queryKey: ["institucion"],
    queryFn: plataformaApi.tiquetes.institucion,
    staleTime: 5 * 60 * 1000,
  });
  const estado = useQuery({
    queryKey: ["comedor", "operacion", fecha],
    queryFn: () => plataformaApi.comedor.estadoOperacion(fecha),
    refetchInterval: 15_000,
  });
  const ingreso = useMutation({
    mutationFn: plataformaApi.comedor.registrarIngreso,
    onSuccess: (respuesta) => {
      setResultado(respuesta);
      navigator.vibrate?.(80);
      setUltimaLectura(new Intl.DateTimeFormat("es-CR", { timeStyle: "short" }).format(new Date()));
      setCodigoExcepcion("");
      if (!silenciado) emitirTonoEstacionComedor("aceptado");
    },
    onError: (error: { response?: { data?: ResultadoOperacion } }) => {
      const respuesta = error.response?.data ?? {
        estado: "rechazada" as const,
        mensaje: errMsg(error),
      };
      setResultado(respuesta);
      navigator.vibrate?.([120, 60, 120]);
      setUltimaLectura(new Intl.DateTimeFormat("es-CR", { timeStyle: "short" }).format(new Date()));
      if (!silenciado) emitirTonoEstacionComedor("rechazado");
      if (respuesta.resultado === "sin_reserva" && respuesta.persona?.cedula)
        setCodigoExcepcion(respuesta.persona.cedula);
    },
    onSettled: async () => {
      lecturaPendienteRef.current = false;
      await clienteConsultas.invalidateQueries({ queryKey: ["comedor", "operacion", fecha] });
    },
  });
  const decision = useMutation({
    mutationFn: ({
      codigo,
      valor,
      observacion,
    }: {
      codigo: string;
      valor: "aprobada" | "rechazada";
      observacion: string;
    }) => plataformaApi.comedor.decidirAutorizacion(codigo, valor, observacion),
    onSuccess: () => {
      setCodigoExcepcion("");
      setResultado(undefined);
      ultimaLecturaRef.current = "";
    },
  });
  const registrarCodigo = useCallback(
    (lectura: string) => {
      const codigo = lectura.trim();
      if (!codigo || lecturaPendienteRef.current || codigo === ultimaLecturaRef.current) return;
      lecturaPendienteRef.current = true;
      ultimaLecturaRef.current = codigo;
      ingreso.mutate(codigo);
    },
    [ingreso.mutate],
  );
  useEffect(() => {
    const actualizarPantalla = () => setPantallaCompleta(Boolean(document.fullscreenElement));
    document.addEventListener("fullscreenchange", actualizarPantalla);
    return () => document.removeEventListener("fullscreenchange", actualizarPantalla);
  }, []);
  useEffect(() => {
    if (!resultado || resultado.resultado === "sin_reserva") return undefined;
    const temporizador = window.setTimeout(() => {
      setResultado(undefined);
      ultimaLecturaRef.current = "";
    }, 2400);
    return () => window.clearTimeout(temporizador);
  }, [resultado]);
  useEffect(() => {
    function teclas(evento: KeyboardEvent) {
      if (evento.key === "F3") {
        evento.preventDefault();
        setMostrarRespaldo(true);
        window.setTimeout(() => entradaRef.current?.focus(), 0);
      }
      if (evento.key === "F4") {
        evento.preventDefault();
        setMostrarHistorial((actual) => !actual);
      }
      if (
        evento.key === "Escape" &&
        !document.fullscreenElement &&
        window.confirm("¿Salir de la estación de comedor?")
      )
        navegar("/admin/panel/inicio");
    }
    window.addEventListener("keydown", teclas);
    return () => window.removeEventListener("keydown", teclas);
  }, [navegar]);
  useEffect(() => {
    const soporte = navigator as Navigator & {
      wakeLock?: { request: (tipo: "screen") => Promise<{ release: () => Promise<void> }> };
    };
    let bloqueo: { release: () => Promise<void> } | undefined;
    void soporte.wakeLock?.request("screen").then((resultado) => {
      bloqueo = resultado;
    });
    return () => {
      void bloqueo?.release();
    };
  }, []);
  function registrar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const formulario = evento.currentTarget;
    const codigo = String(new FormData(formulario).get("codigo") ?? "").trim();
    if (codigo) registrarCodigo(codigo);
    formulario.reset();
  }
  function decidir(valor: "aprobada" | "rechazada") {
    decision.mutate({
      codigo: codigoExcepcion,
      valor,
      observacion: "Decisión registrada desde estación de comedor",
    });
  }
  async function alternarPantallaCompleta() {
    if (document.fullscreenElement) await document.exitFullscreen?.();
    else {
      localStorage.setItem("comedor-pantalla-completa", "preferida");
      await document.documentElement.requestFullscreen?.();
    }
  }
  function alternarSonido() {
    setSilenciado((actual) => {
      localStorage.setItem("comedor-sonido", actual ? "activo" : "silenciado");
      return !actual;
    });
  }
  const resumen = estado.data;
  const enExcepcion = resultado?.resultado === "sin_reserva";
  return (
    <section
      className="flex min-h-[100dvh] flex-col bg-slate-950 text-slate-50"
      data-testid="estacion-comedor"
    >
      <ControlesEstacionComedor
        nombreColegio={institucion.data?.nombreColegio ?? "CTP Platanares"}
        fecha={fecha}
        estadoCamara={estadoCamara}
        conectado={!estado.isError}
        ultimaLectura={ultimaLectura}
        compacto={pantallaCompleta}
        silenciado={silenciado}
        mostrarHistorial={mostrarHistorial}
        alAlternarSonido={alternarSonido}
        alAlternarHistorial={() => setMostrarHistorial((actual) => !actual)}
        alAlternarPantallaCompleta={() => void alternarPantallaCompleta()}
        alSalir={() => {
          if (window.confirm("¿Salir de la estación de comedor?")) navegar("/admin/panel/inicio");
        }}
      />
      <div className="relative mx-auto flex w-full max-w-[110rem] flex-1 flex-col justify-center gap-4 px-3 py-4 sm:px-6 sm:py-6 landscape:justify-start">
        <div className="relative mx-auto w-full max-w-6xl overflow-hidden rounded-[2rem] border border-white/10 bg-slate-900 shadow-[0_26px_80px_rgb(0_0_0_/_0.35)]">
          <LectorQrCamara
            alDetectar={registrarCodigo}
            pausado={ingreso.isPending || enExcepcion}
            alCambiarEstado={setEstadoCamara}
          />
          {resultado && !enExcepcion && (
            <div className="absolute inset-x-3 bottom-3 sm:inset-x-6 sm:bottom-6">
              <ResultadoLecturaComedor resultado={resultado} modoEstacion />
            </div>
          )}
        </div>
        {enExcepcion && (
          <ExcepcionSinReserva
            codigo={codigoExcepcion}
            alDecidir={decidir}
            pendiente={decision.isPending}
            error={decision.error}
          />
        )}
        {mostrarRespaldo && (
          <CapturaManualComedor
            referenciaEntrada={entradaRef}
            pendiente={ingreso.isPending}
            alRegistrar={registrar}
            alOcultar={() => setMostrarRespaldo(false)}
          />
        )}
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-3 text-xs font-semibold text-slate-400">
          <button
            type="button"
            onClick={() => {
              setMostrarRespaldo((actual) => !actual);
              window.setTimeout(() => entradaRef.current?.focus(), 0);
            }}
            className="inline-flex min-h-11 items-center gap-2 rounded-xl px-3 hover:bg-white/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-300"
          >
            <ScanBarcode className="h-4 w-4" /> Respaldo <kbd className="text-slate-300">F3</kbd>
          </button>
          <p className="tabular-nums">
            {resumen?.ingresos ?? 0} / {resumen?.meta ?? 0} atendidos · {resumen?.duplicados ?? 0}{" "}
            duplicados · {resumen?.errores ?? 0} rechazos
          </p>
          <span className={estado.isError ? "text-rose-300" : "text-emerald-300"}>
            {estado.isError ? "Sin conexión" : "Conectado"}
          </span>
        </div>
        {mostrarHistorial && (
          <section
            className="mx-auto w-full max-w-4xl overflow-hidden rounded-2xl border border-white/10 bg-slate-900"
            aria-label="Lecturas recientes"
          >
            <div className="flex items-center justify-between border-b border-white/10 px-5 py-3">
              <h3 className="font-bold">Lecturas recientes</h3>
              <button
                type="button"
                onClick={() => setMostrarHistorial(false)}
                className="text-slate-400 hover:text-white"
                aria-label="Ocultar historial"
              >
                <ChevronDown className="h-5 w-5" />
              </button>
            </div>
            <div className="divide-y divide-white/10">
              {(resumen?.recientes ?? []).length === 0 ? (
                <p className="p-5 text-center text-sm text-slate-400">Sin lecturas hoy.</p>
              ) : (
                resumen?.recientes.map((evento) => (
                  <div
                    key={evento.id}
                    className="flex items-center justify-between gap-4 px-5 py-3 text-sm"
                  >
                    <span className="truncate font-semibold">{evento.nombre}</span>
                    <Badge variant={evento.resultado === "aceptado" ? "default" : "secondary"}>
                      {evento.resultado.replaceAll("_", " ")}
                    </Badge>
                  </div>
                ))
              )}
            </div>
          </section>
        )}
      </div>
    </section>
  );
}
