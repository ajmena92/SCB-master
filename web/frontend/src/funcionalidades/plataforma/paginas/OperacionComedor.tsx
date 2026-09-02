import { useEffect, useRef, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Clock3, ScanBarcode, Users } from "lucide-react";
import { plataformaApi } from "../consultas/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import { fechaLocalActual } from "@/compartido/utilidades/fecha";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Progress } from "@/components/ui/progress";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";
import { ExcepcionSinReserva } from "../componentes/ExcepcionSinReserva";
import { ResultadoLecturaComedor } from "../componentes/ResultadoLecturaComedor";

export default function OperacionComedor() {
  const fecha = fechaLocalActual();
  const clienteConsultas = useQueryClient();
  const entradaRef = useRef<HTMLInputElement>(null);
  const [resultado, setResultado] = useState<ResultadoOperacion>();
  const [codigoExcepcion, setCodigoExcepcion] = useState("");
  const estado = useQuery({
    queryKey: ["comedor", "operacion", fecha],
    queryFn: () => plataformaApi.comedor.estadoOperacion(fecha),
    refetchInterval: 15_000,
  });
  const ingreso = useMutation({
    mutationFn: plataformaApi.comedor.registrarIngreso,
    onSuccess: (respuesta) => {
      setResultado(respuesta);
      setCodigoExcepcion("");
    },
    onError: (error: { response?: { data?: ResultadoOperacion } }) => {
      const respuesta = error.response?.data ?? { estado: "rechazada" as const, mensaje: errMsg(error) };
      setResultado(respuesta);
      if (respuesta.resultado === "sin_reserva" && respuesta.persona?.cedula) {
        setCodigoExcepcion(respuesta.persona.cedula);
      }
    },
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

  function decidir(valor: "aprobada" | "rechazada", observacion: string) {
    decision.mutate({
      codigo: codigoExcepcion,
      valor,
      observacion,
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

      <section aria-label="Resumen de la operación">
        <div className="flex items-center justify-between gap-3 rounded-xl border bg-card px-4 py-3 sm:hidden">
          <div className="min-w-0">
            <p className="text-xs font-bold uppercase tracking-wide text-muted-foreground">Reservas de hoy</p>
            <p className="font-display text-2xl font-black tabular-nums">
              {resumen?.ingresos ?? 0} / {resumen?.meta ?? 0}
            </p>
          </div>
          <div className="flex shrink-0 items-center gap-3 text-right text-xs font-bold uppercase tracking-wide text-muted-foreground">
            <span>
              Dup. <strong className="ml-1 text-base text-foreground">{resumen?.duplicados ?? 0}</strong>
            </span>
            <span>
              Rech. <strong className="ml-1 text-base text-foreground">{resumen?.errores ?? 0}</strong>
            </span>
          </div>
        </div>
        <div className="hidden grid-cols-3 gap-3 sm:grid lg:grid-cols-[1.4fr_0.8fr_0.8fr]">
        <div className="col-span-3 rounded-xl border bg-card p-4 lg:col-span-1">
          <div className="flex items-center justify-between text-xs font-bold uppercase tracking-wide text-muted-foreground">
            <span>Reservas confirmadas</span>
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

          <ResultadoLecturaComedor resultado={resultado} />

          {resultado?.resultado === "sin_reserva" && (
            <ExcepcionSinReserva
              codigo={codigoExcepcion}
              alCambiarCodigo={setCodigoExcepcion}
              alDecidir={decidir}
              pendiente={decision.isPending}
              error={decision.error}
              exito={decision.isSuccess}
            />
          )}

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

        <aside className="hidden xl:block">
          <div className="rounded-2xl border bg-card p-6 text-sm text-muted-foreground">
            <p className="font-display text-lg font-bold text-foreground">Operación ágil</p>
            <p className="mt-2 leading-6">
              La fotografía y la decisión aparecen al escanear. El lector conserva el foco para la siguiente persona.
            </p>
          </div>
        </aside>
      </div>
    </div>
  );
}
