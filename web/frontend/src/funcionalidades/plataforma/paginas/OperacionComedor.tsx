import { useCallback, useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ChevronDown, Expand, History, LogOut, ScanBarcode, Volume2, VolumeX, X } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { plataformaApi } from "../consultas/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";
import { ExcepcionSinReserva } from "../componentes/ExcepcionSinReserva";
import { LectorQrCamara } from "../componentes/LectorQrCamara";
import { ResultadoLecturaComedor } from "../componentes/ResultadoLecturaComedor";

type EstadoCamara = "iniciando" | "activo" | "error";

function emitirTono(tipo: "aceptado" | "rechazado") {
  const AudioContexto =
    window.AudioContext ??
    (window as Window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
  if (!AudioContexto) return;
  const contexto = new AudioContexto();
  const oscilador = contexto.createOscillator();
  const ganancia = contexto.createGain();
  oscilador.type = "sine";
  oscilador.frequency.value = tipo === "aceptado" ? 880 : 220;
  ganancia.gain.setValueAtTime(0.07, contexto.currentTime);
  ganancia.gain.exponentialRampToValueAtTime(0.001, contexto.currentTime + (tipo === "aceptado" ? 0.12 : 0.22));
  oscilador.connect(ganancia).connect(contexto.destination);
  oscilador.start();
  oscilador.stop(contexto.currentTime + (tipo === "aceptado" ? 0.12 : 0.22));
  window.setTimeout(() => void contexto.close(), 300);
}

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
  const [silenciado, setSilenciado] = useState(() => localStorage.getItem("comedor-sonido") === "silenciado");
  const [estadoCamara, setEstadoCamara] = useState<EstadoCamara>("iniciando");
  const estado = useQuery({ queryKey: ["comedor", "operacion", fecha], queryFn: () => plataformaApi.comedor.estadoOperacion(fecha), refetchInterval: 15_000 });
  const ingreso = useMutation({
    mutationFn: plataformaApi.comedor.registrarIngreso,
    onSuccess: (respuesta) => { setResultado(respuesta); setCodigoExcepcion(""); if (!silenciado) emitirTono("aceptado"); },
    onError: (error: { response?: { data?: ResultadoOperacion } }) => {
      const respuesta = error.response?.data ?? { estado: "rechazada" as const, mensaje: errMsg(error) };
      setResultado(respuesta);
      if (!silenciado) emitirTono("rechazado");
      if (respuesta.resultado === "sin_reserva" && respuesta.persona?.cedula) setCodigoExcepcion(respuesta.persona.cedula);
    },
    onSettled: async () => { lecturaPendienteRef.current = false; await clienteConsultas.invalidateQueries({ queryKey: ["comedor", "operacion", fecha] }); },
  });
  const decision = useMutation({
    mutationFn: ({ codigo, valor, observacion }: { codigo: string; valor: "aprobada" | "rechazada"; observacion: string }) => plataformaApi.comedor.decidirAutorizacion(codigo, valor, observacion),
    onSuccess: () => { setCodigoExcepcion(""); setResultado(undefined); ultimaLecturaRef.current = ""; },
  });
  const registrarCodigo = useCallback((lectura: string) => {
    const codigo = lectura.trim();
    if (!codigo || lecturaPendienteRef.current || codigo === ultimaLecturaRef.current) return;
    lecturaPendienteRef.current = true;
    ultimaLecturaRef.current = codigo;
    ingreso.mutate(codigo);
  }, [ingreso.mutate]);
  useEffect(() => {
    if (!resultado || resultado.resultado === "sin_reserva") return undefined;
    const temporizador = window.setTimeout(() => { setResultado(undefined); ultimaLecturaRef.current = ""; }, 2400);
    return () => window.clearTimeout(temporizador);
  }, [resultado]);
  useEffect(() => {
    function teclas(evento: KeyboardEvent) {
      if (evento.key === "F3") { evento.preventDefault(); setMostrarRespaldo(true); window.setTimeout(() => entradaRef.current?.focus(), 0); }
      if (evento.key === "F4") { evento.preventDefault(); setMostrarHistorial((actual) => !actual); }
      if (evento.key === "Escape" && !document.fullscreenElement && window.confirm("¿Salir de la estación de comedor?")) navegar("/admin/panel/inicio");
    }
    window.addEventListener("keydown", teclas);
    return () => window.removeEventListener("keydown", teclas);
  }, [navegar]);
  function registrar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const formulario = evento.currentTarget;
    const codigo = String(new FormData(formulario).get("codigo") ?? "").trim();
    if (codigo) registrarCodigo(codigo);
    formulario.reset();
  }
  function decidir(valor: "aprobada" | "rechazada") {
    decision.mutate({ codigo: codigoExcepcion, valor, observacion: "Decisión registrada desde estación de comedor" });
  }
  async function alternarPantallaCompleta() {
    if (document.fullscreenElement) await document.exitFullscreen?.();
    else { localStorage.setItem("comedor-pantalla-completa", "preferida"); await document.documentElement.requestFullscreen?.(); }
  }
  function alternarSonido() { setSilenciado((actual) => { localStorage.setItem("comedor-sonido", actual ? "activo" : "silenciado"); return !actual; }); }
  const resumen = estado.data;
  const enExcepcion = resultado?.resultado === "sin_reserva";
  return (
    <section className="flex min-h-[100dvh] flex-col bg-slate-950 text-slate-50" data-testid="estacion-comedor">
      <header className="flex min-h-14 items-center justify-between gap-3 border-b border-white/10 px-3 sm:px-5">
        <div className="flex min-w-0 items-center gap-3"><span className={`h-2.5 w-2.5 rounded-full ${estadoCamara === "activo" ? "bg-emerald-400" : estadoCamara === "error" ? "bg-rose-400" : "bg-amber-300"}`} /><p className="truncate text-sm font-bold tracking-wide">Estación de comedor</p><span className="hidden text-xs text-slate-400 sm:inline">{fecha}</span></div>
        <div className="flex items-center gap-1">
          <button type="button" onClick={alternarSonido} className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10" aria-label={silenciado ? "Activar sonido" : "Silenciar sonido"}>{silenciado ? <VolumeX className="h-5 w-5" /> : <Volume2 className="h-5 w-5" />}</button>
          <button type="button" onClick={() => setMostrarHistorial((actual) => !actual)} className="hidden min-h-11 items-center gap-2 rounded-xl px-3 text-sm font-bold text-slate-300 hover:bg-white/10 sm:inline-flex" aria-expanded={mostrarHistorial}><History className="h-4 w-4" /> Historial <kbd className="text-xs text-slate-500">F4</kbd></button>
          <button type="button" onClick={() => void alternarPantallaCompleta()} className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10" aria-label="Alternar pantalla completa"><Expand className="h-5 w-5" /></button>
          <button type="button" onClick={() => { if (window.confirm("¿Salir de la estación de comedor?")) navegar("/admin/panel/inicio"); }} className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10" aria-label="Salir de estación"><LogOut className="h-5 w-5" /></button>
        </div>
      </header>
      <main className="relative mx-auto flex w-full max-w-[110rem] flex-1 flex-col justify-center gap-4 px-3 py-4 sm:px-6 sm:py-6">
        <div className="relative mx-auto w-full max-w-6xl overflow-hidden rounded-[2rem] border border-white/10 bg-slate-900 shadow-[0_26px_80px_rgb(0_0_0_/_0.35)]">
          <LectorQrCamara alDetectar={registrarCodigo} pausado={ingreso.isPending || enExcepcion} alCambiarEstado={setEstadoCamara} />
          {resultado && !enExcepcion && <div className="absolute inset-x-3 bottom-3 sm:inset-x-6 sm:bottom-6"><ResultadoLecturaComedor resultado={resultado} modoEstacion /></div>}
        </div>
        {enExcepcion && <ExcepcionSinReserva codigo={codigoExcepcion} alDecidir={decidir} pendiente={decision.isPending} error={decision.error} />}
        {mostrarRespaldo && <form className="mx-auto flex w-full max-w-4xl flex-col gap-3 rounded-2xl border border-white/15 bg-slate-900 p-4 sm:flex-row" onSubmit={registrar}><label htmlFor="captura-comedor" className="sr-only">Lector USB o ingreso manual</label><div className="relative min-w-0 flex-1"><ScanBarcode className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-emerald-300" /><Input ref={entradaRef} id="captura-comedor" name="codigo" autoComplete="off" required placeholder="Lector USB o número de identificación" className="h-12 border-white/15 bg-slate-950 pl-12 text-base text-white placeholder:text-slate-500" /></div><Button className="h-12 bg-emerald-400 px-6 font-bold text-slate-950 hover:bg-emerald-300" disabled={ingreso.isPending}>{ingreso.isPending ? "Validando…" : "Registrar"}</Button><button type="button" onClick={() => setMostrarRespaldo(false)} className="grid h-12 w-12 place-items-center rounded-xl text-slate-300 hover:bg-white/10" aria-label="Ocultar respaldo"><X className="h-5 w-5" /></button></form>}
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-3 text-xs font-semibold text-slate-400"><button type="button" onClick={() => { setMostrarRespaldo((actual) => !actual); window.setTimeout(() => entradaRef.current?.focus(), 0); }} className="inline-flex min-h-11 items-center gap-2 rounded-xl px-3 hover:bg-white/10"><ScanBarcode className="h-4 w-4" /> Respaldo <kbd className="text-slate-500">F3</kbd></button><p className="tabular-nums">{resumen?.ingresos ?? 0} / {resumen?.meta ?? 0} atendidos · {resumen?.duplicados ?? 0} duplicados · {resumen?.errores ?? 0} rechazos</p><span className={estado.isError ? "text-rose-300" : "text-emerald-300"}>{estado.isError ? "Sin conexión" : "Conectado"}</span></div>
        {mostrarHistorial && <section className="mx-auto w-full max-w-4xl overflow-hidden rounded-2xl border border-white/10 bg-slate-900" aria-label="Lecturas recientes"><div className="flex items-center justify-between border-b border-white/10 px-5 py-3"><h3 className="font-bold">Lecturas recientes</h3><button type="button" onClick={() => setMostrarHistorial(false)} className="text-slate-400 hover:text-white" aria-label="Ocultar historial"><ChevronDown className="h-5 w-5" /></button></div><div className="divide-y divide-white/10">{(resumen?.recientes ?? []).length === 0 ? <p className="p-5 text-center text-sm text-slate-400">Sin lecturas hoy.</p> : resumen?.recientes.map((evento) => <div key={evento.id} className="flex items-center justify-between gap-4 px-5 py-3 text-sm"><span className="truncate font-semibold">{evento.nombre}</span><Badge variant={evento.resultado === "aceptado" ? "default" : "secondary"}>{evento.resultado.replaceAll("_", " ")}</Badge></div>)}</div></section>}
      </main>
    </section>
  );
}
