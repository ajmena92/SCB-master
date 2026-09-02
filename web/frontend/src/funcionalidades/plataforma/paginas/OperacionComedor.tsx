import { useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AlertTriangle, CheckCircle2, Clock3, ScanBarcode, Users, XCircle } from "lucide-react";
import { plataformaApi } from "../consultas/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";

function claseResultado(resultado?: ResultadoOperacion) {
  if (!resultado) return "border-border bg-card text-foreground";
  if (resultado.estado === "aceptada" && resultado.advertencia)
    return "border-amber-300 bg-amber-50 text-amber-950";
  if (resultado.estado === "aceptada") return "border-success/40 bg-success/10 text-foreground";
  return "border-destructive/40 bg-destructive/10 text-destructive";
}

function IconoResultado({ resultado }: { resultado?: ResultadoOperacion }) {
  if (!resultado) return <ScanBarcode className="h-12 w-12 text-primary" />;
  if (resultado.estado === "aceptada" && resultado.advertencia)
    return <AlertTriangle className="h-12 w-12 text-amber-600" />;
  if (resultado.estado === "aceptada") return <CheckCircle2 className="h-12 w-12 text-success" />;
  return <XCircle className="h-12 w-12 text-destructive" />;
}

export default function OperacionComedor() {
  const fecha = fechaLocalActual();
  const clienteConsultas = useQueryClient();
  const entradaRef = useRef<HTMLInputElement>(null);
  const [resultado, setResultado] = useState<ResultadoOperacion>();
  const estado = useQuery({
    queryKey: ["comedor", "operacion", fecha],
    queryFn: () => plataformaApi.comedor.estadoOperacion(fecha),
    refetchInterval: 15_000,
  });
  const ingreso = useMutation({
    mutationFn: plataformaApi.comedor.registrarIngreso,
    onSuccess: setResultado,
    onError: (error: { response?: { data?: ResultadoOperacion } }) =>
      setResultado(error.response?.data ?? { estado: "rechazada", mensaje: errMsg(error) }),
    onSettled: async () => {
      await clienteConsultas.invalidateQueries({ queryKey: ["comedor", "operacion", fecha] });
      window.setTimeout(() => entradaRef.current?.focus(), 0);
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
  });

  useEffect(() => {
    entradaRef.current?.focus();
  }, []);

  function registrar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const formulario = evento.currentTarget;
    const datos = new FormData(formulario);
    const codigo = String(datos.get("codigo") ?? "").trim();
    if (!codigo) return;
    ingreso.mutate(codigo);
    formulario.reset();
  }

  function decidir(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    decision.mutate({
      codigo: String(datos.get("codigo")),
      valor: datos.get("decision") === "rechazada" ? "rechazada" : "aprobada",
      observacion: String(datos.get("observacion")),
    });
  }

  const resumen = estado.data;
  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-black tracking-tight">Control de comedor</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Escaneá el carnet. La API valida reserva, beneficio, tiquete y duplicados.
          </p>
        </div>
        <Badge variant="secondary" className="gap-2 px-3 py-1.5">
          <Clock3 className="h-4 w-4" /> {fecha}
        </Badge>
      </div>

      <section className="grid gap-4 sm:grid-cols-4" aria-label="Resumen de la operación">
        <div className="rounded-xl border bg-card p-4 sm:col-span-2">
          <div className="flex items-center justify-between text-xs font-bold uppercase tracking-wide text-muted-foreground">
            <span>Meta diaria</span>
            <Users className="h-4 w-4 text-primary" />
          </div>
          <p className="mt-2 font-display text-3xl font-black">
            {resumen?.ingresos ?? 0} / {resumen?.meta ?? 0}
          </p>
          <Progress
            ref={undefined}
            className="mt-3"
            value={Math.min(100, resumen?.porcentaje ?? 0)}
          />
          <p className="mt-2 text-xs text-muted-foreground">{resumen?.porcentaje ?? 0}% atendido</p>
        </div>
        <div className="rounded-xl border bg-card p-4">
          <p className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
            Duplicados
          </p>
          <p className="mt-3 font-display text-3xl font-black">{resumen?.duplicados ?? 0}</p>
        </div>
        <div className="rounded-xl border bg-card p-4">
          <p className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
            Rechazos
          </p>
          <p className="mt-3 font-display text-3xl font-black">{resumen?.errores ?? 0}</p>
        </div>
      </section>

      <div className="grid gap-5 xl:grid-cols-[minmax(0,1.5fr)_minmax(20rem,0.8fr)]">
        <div className="space-y-4">
          <form className="rounded-2xl border bg-card p-5 shadow-sm" onSubmit={registrar}>
            <label htmlFor="captura-comedor" className="text-sm font-bold">
              Lector de carnet o código institucional
            </label>
            <div className="mt-3 flex flex-col gap-3 sm:flex-row">
              <div className="relative min-w-0 flex-1">
                <ScanBarcode className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-primary" />
                <Input
                  ref={entradaRef}
                  id="captura-comedor"
                  name="codigo"
                  data-testid="captura-comedor"
                  autoComplete="off"
                  required
                  placeholder="Escaneá y presioná Enter"
                  className="h-14 pl-12 font-mono text-lg"
                />
              </div>
              <Button className="h-14 px-6 text-base font-bold" disabled={ingreso.isPending}>
                {ingreso.isPending ? "Validando…" : "Registrar ingreso"}
              </Button>
            </div>
            <p className="mt-3 text-xs text-muted-foreground">
              El campo recupera el foco después de cada lectura para operar con el escáner USB.
            </p>
          </form>

          <section
            aria-live="polite"
            className={`min-h-52 rounded-2xl border p-6 transition-colors ${claseResultado(resultado)}`}
          >
            <div className="flex flex-col items-center text-center sm:flex-row sm:text-left">
              <IconoResultado resultado={resultado} />
              <div className="mt-4 min-w-0 sm:ml-5 sm:mt-0">
                <p className="text-xs font-black uppercase tracking-[0.18em]">
                  {resultado?.estado === "aceptada"
                    ? "Ingreso procesado"
                    : resultado
                      ? "Acceso no registrado"
                      : "Lector listo"}
                </p>
                <h3 className="mt-1 font-display text-2xl font-black">
                  {resultado?.persona?.nombres ?? "Esperando una lectura"}
                </h3>
                <p className="mt-2 font-semibold">
                  {resultado?.mensaje ?? "Escaneá el código de barras del carnet digital."}
                </p>
                {resultado?.persona && (
                  <p className="mt-2 text-sm opacity-80">
                    {resultado.persona.codigo} · {resultado.persona.tipo}
                    {resultado.saldo !== undefined && ` · Saldo: ${resultado.saldo}`}
                  </p>
                )}
              </div>
            </div>
          </section>

          <section className="overflow-hidden rounded-2xl border bg-card">
            <div className="border-b px-5 py-4">
              <h3 className="font-display font-bold">Lecturas recientes</h3>
            </div>
            <div className="divide-y">
              {(resumen?.recientes ?? []).length === 0 ? (
                <p className="p-6 text-center text-sm text-muted-foreground">Sin lecturas hoy.</p>
              ) : (
                resumen?.recientes.map((evento) => (
                  <div
                    key={evento.id}
                    className="grid grid-cols-[1fr_auto] gap-3 px-5 py-3 text-sm"
                  >
                    <div className="min-w-0">
                      <p className="truncate font-semibold">{evento.nombre}</p>
                      <p className="truncate text-xs text-muted-foreground">
                        {evento.codigo} · {evento.motivo}
                      </p>
                    </div>
                    <Badge variant={evento.resultado === "aceptado" ? "default" : "secondary"}>
                      {evento.resultado.replaceAll("_", " ")}
                    </Badge>
                  </div>
                ))
              )}
            </div>
          </section>
        </div>

        <aside className="rounded-2xl border bg-card p-5">
          <h2 className="font-display text-lg font-bold">Excepción sin reserva</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            La decisión queda asociada al operador y requiere un motivo.
          </p>
          <form className="mt-5 space-y-4" onSubmit={decidir}>
            <label htmlFor="excepcion-codigo" className="block text-sm font-bold">
              Cédula del estudiante
              <Input
                id="excepcion-codigo"
                name="codigo"
                className="mt-2"
                placeholder="Ej. 1-2091-0218"
                required
              />
            </label>
            <label className="block text-sm font-bold">
              Decisión
              <select
                name="decision"
                className="mt-2 h-10 w-full rounded-md border bg-background px-3"
              >
                <option value="aprobada">Aprobar</option>
                <option value="rechazada">Rechazar</option>
              </select>
            </label>
            <label className="block text-sm font-bold">
              Motivo
              <textarea
                name="observacion"
                className="mt-2 min-h-24 w-full rounded-md border bg-background p-3"
                required
              />
            </label>
            {decision.error && <p className="text-sm text-destructive">{errMsg(decision.error)}</p>}
            {decision.isSuccess && <p className="text-sm text-success">Decisión guardada.</p>}
            <Button variant="outline" className="w-full" disabled={decision.isPending}>
              Guardar decisión
            </Button>
          </form>
        </aside>
      </div>
    </div>
  );
}
