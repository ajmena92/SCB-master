import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  ArrowLeft,
  Bus,
  CaretRight,
  FloppyDisk,
  IdentificationCard,
  Key,
} from "@phosphor-icons/react";
import { useLocation, useNavigate, useParams } from "react-router-dom";
import type { CredencialTemporal, Persona } from "@/compartido/contratos/plataforma";
import { errMsg } from "@/compartido/consultas/errores_api";
import DialogoCredencialTemporal from "../componentes/DialogoCredencialTemporal";
import { ConfirmacionExpediente } from "../componentes/ConfirmacionExpediente";
import FotoEstudiante from "../componentes/FotoEstudiante";
import { Aviso, Campo, EstadoCarga } from "../componentes/ElementosComunes";
import {
  CargandoExpedienteEstudiante,
  ExpedienteEstudianteNoEncontrado,
} from "../componentes/EstadoExpedienteEstudiante";
import { ResumenMatriculaEstudiante } from "../componentes/ResumenMatriculaEstudiante";
import { plataformaApi } from "../consultas/plataforma";

type Confirmacion = "pin" | "desactivar" | undefined;

export default function EditarEstudiante() {
  const { referencia } = useParams();
  const personaNavegacion = (useLocation().state as { persona?: Persona } | null)?.persona;
  const personaInicial =
    personaNavegacion?.referenciaPublica === referencia ? personaNavegacion : undefined;
  const consultaPersona = useQuery({
    queryKey: ["personas", "referencia", referencia],
    queryFn: () => plataformaApi.personas.obtenerPorReferencia(referencia!),
    enabled: Boolean(referencia && !personaInicial),
  });
  const persona = personaInicial ?? consultaPersona.data;
  const esEstudiante = persona?.tipo === "estudiante";
  const navegar = useNavigate();
  const cliente = useQueryClient();
  const [confirmacion, setConfirmacion] = useState<Confirmacion>();
  const [credenciales, setCredenciales] = useState<CredencialTemporal[]>();
  const [cambiosPendientes, setCambiosPendientes] = useState(false);
  const rutas = useQuery({
    queryKey: ["rutas"],
    queryFn: plataformaApi.rutas.listar,
    enabled: esEstudiante,
  });
  const guardar = useMutation({
    mutationFn: async (formulario: FormData) => {
      if (!esEstudiante || !persona?.matriculaId) return;
      const ruta = String(formulario.get("rutaId") || "");
      await plataformaApi.matriculas.actualizarBeneficios(persona.matriculaId, {
        becado: formulario.get("becado") === "on",
        rutaId: ruta ? Number(ruta) : null,
      });
    },
    onSuccess: () => {
      setCambiosPendientes(false);
      cliente.invalidateQueries({ queryKey: ["personas"] });
    },
  });
  const reiniciarPin = useMutation({
    mutationFn: () => plataformaApi.personas.reiniciarPin(persona!.id),
    onSuccess: (credencial) => {
      setConfirmacion(undefined);
      setCredenciales([credencial]);
    },
  });
  const desactivar = useMutation({
    mutationFn: () => plataformaApi.personas.desactivar(persona!.id),
    onSuccess: () => {
      cliente.invalidateQueries({ queryKey: ["personas"] });
      navegar("/admin/panel/personas", { replace: true });
    },
  });

  if (consultaPersona.isLoading) return <CargandoExpedienteEstudiante />;
  if (!referencia || !persona)
    return <ExpedienteEstudianteNoEncontrado alVolver={() => navegar("/admin/panel/personas")} />;

  const error =
    consultaPersona.error ||
    guardar.error ||
    reiniciarPin.error ||
    desactivar.error ||
    (esEstudiante ? rutas.error : undefined);
  const ocupada = reiniciarPin.isPending || desactivar.isPending;
  return (
    <section className="mx-auto grid max-w-6xl gap-4 pb-8">
      <DialogoCredencialTemporal
        credenciales={credenciales}
        alCerrar={() => setCredenciales(undefined)}
      />
      <nav
        className="flex items-center gap-2 text-sm text-muted-foreground"
        aria-label="Ubicación actual"
      >
        <button
          className="button link"
          type="button"
          onClick={() => navegar("/admin/panel/personas")}
        >
          <ArrowLeft aria-hidden="true" size={17} /> Personas / PIN
        </button>
        <CaretRight aria-hidden="true" size={14} />
        <span>Expediente</span>
      </nav>
      <header className="grid gap-2 border-b border-border pb-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            {esEstudiante ? "Expediente del estudiante" : "Expediente del profesor"}
          </p>
          <h1 className="font-heading text-2xl font-semibold text-foreground sm:text-3xl">
            {persona.nombres}
          </h1>
          <div className="mt-2 flex flex-wrap items-center gap-3 text-sm text-muted-foreground">
            <span className="inline-flex items-center gap-1">
              <IdentificationCard aria-hidden="true" size={17} />{" "}
              {persona.cedula ?? "Cédula no registrada"}
            </span>
            {esEstudiante ? (
              <span className="inline-flex items-center gap-1">
                <Bus aria-hidden="true" size={17} /> {persona.seccion ?? "Sin sección"}
              </span>
            ) : (
              <span className="rounded-full bg-primary/10 px-2.5 py-1 text-xs font-semibold text-primary">
                Profesor
              </span>
            )}
            <span
              className={
                persona.activo
                  ? "rounded-full bg-success/15 px-2.5 py-1 text-xs font-semibold text-success"
                  : "rounded-full bg-destructive/15 px-2.5 py-1 text-xs font-semibold text-destructive"
              }
            >
              {persona.activo ? "Activo" : "Inactivo"}
            </span>
          </div>
        </div>
      </header>
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {esEstudiante && guardar.isSuccess && (
        <Aviso tipo="exito">Los beneficios se actualizaron.</Aviso>
      )}
      <form
        id="formulario-expediente"
        className="grid gap-5"
        onInput={() => setCambiosPendientes(true)}
        onSubmit={(evento: FormEvent<HTMLFormElement>) => {
          evento.preventDefault();
          guardar.mutate(new FormData(evento.currentTarget));
        }}
      >
        <div className="grid gap-5 lg:grid-cols-[minmax(17rem,22rem)_minmax(0,1fr)]">
          <aside className="grid content-start gap-4">
            <FotoEstudiante personaId={persona.id} nombre={persona.nombres} />
            {esEstudiante && <ResumenMatriculaEstudiante persona={persona} />}
          </aside>
          <div className="grid content-start gap-5">
            <section className="grid gap-4 rounded-xl border border-border bg-card p-4 shadow-sm">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Importado del padrón
                  </p>
                  <h2 className="font-heading text-xl font-semibold">
                    {esEstudiante ? "Datos del estudiante" : "Datos del profesor"}
                  </h2>
                </div>
                <span
                  className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground"
                  aria-label="Datos bloqueados"
                >
                  🔒 Solo lectura
                </span>
              </div>
              <p className="text-sm text-muted-foreground">
                Estos datos los administra el padrón anual. Para corregirlos, actualice el padrón y
                vuelva a importarlo.
              </p>
              <div className="grid gap-4 sm:grid-cols-2">
                <Campo etiqueta="Cédula">
                  <input value={persona.cedula ?? ""} disabled />
                </Campo>
                <Campo etiqueta="Nombre completo">
                  <input value={persona.nombres} disabled />
                </Campo>
                {esEstudiante && (
                  <Campo etiqueta="Sección">
                    <input value={persona.seccion ?? "Sin sección"} disabled />
                  </Campo>
                )}
              </div>
            </section>
            {esEstudiante && (
              <section
                id="beneficios"
                className="grid gap-4 rounded-xl border border-border bg-card p-4 shadow-sm"
              >
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                      Administrable en SCB
                    </p>
                    <h2 className="font-heading text-xl font-semibold">
                      Beneficios de la matrícula
                    </h2>
                  </div>
                  <span className="rounded-full bg-primary/10 px-2 py-1 text-xs font-semibold text-primary">
                    Curso vigente
                  </span>
                </div>
                <p className="text-sm text-muted-foreground">
                  Los cambios se aplican únicamente a la matrícula anual activa.
                </p>
                {rutas.isLoading ? (
                  <EstadoCarga />
                ) : (
                  <div className="grid gap-4">
                    <Campo etiqueta="Ruta de transporte">
                      <select
                        id="ruta-transporte"
                        name="rutaId"
                        defaultValue={persona.rutaId ?? ""}
                        disabled={!persona.activo}
                      >
                        <option value="">No utiliza transporte</option>
                        {rutas.data?.elementos
                          .filter((ruta) => ruta.activo && ruta.codigo !== "0000")
                          .map((ruta) => (
                            <option key={ruta.idRuta} value={ruta.idRuta}>
                              {ruta.codigo} — {ruta.descripcion}
                            </option>
                          ))}
                      </select>
                    </Campo>
                    <label
                      id="beca-comedor"
                      className="flex cursor-pointer items-center justify-between gap-4 rounded-lg border border-border bg-muted/40 p-3"
                    >
                      <span>
                        <b className="block text-sm font-semibold">Beca de comedor</b>
                        <small className="text-sm text-muted-foreground">
                          Aplica a la beca completa de cinco días.
                        </small>
                      </span>
                      <span className="relative inline-flex shrink-0">
                        <input
                          className="peer sr-only"
                          name="becado"
                          type="checkbox"
                          role="switch"
                          defaultChecked={persona.becado}
                          disabled={!persona.activo}
                          aria-label="Asignar beca de comedor"
                        />
                        <span
                          aria-hidden="true"
                          className="h-6 w-11 rounded-full bg-border transition peer-checked:bg-primary peer-focus-visible:ring-2 peer-focus-visible:ring-primary/30 after:absolute after:left-1 after:top-1 after:size-4 after:rounded-full after:bg-white after:transition peer-checked:after:translate-x-5"
                        />
                      </span>
                    </label>
                  </div>
                )}
              </section>
            )}
          </div>
        </div>
      </form>
      <footer
        className="sticky bottom-3 z-10 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-border bg-card/95 p-3 shadow-lg backdrop-blur"
        aria-label="Acciones del expediente"
      >
        <p className="text-sm text-muted-foreground" aria-live="polite">
          {esEstudiante && guardar.isPending
            ? "Guardando cambios…"
            : esEstudiante && cambiosPendientes
              ? "Cambios sin guardar"
              : esEstudiante
                ? "Sin cambios pendientes"
                : "La fotografía se guarda al cargarla."}
        </p>
        <div className="flex flex-wrap gap-2">
          <button
            className="button warning"
            type="button"
            onClick={() => setConfirmacion("pin")}
            disabled={!persona.activo || ocupada}
          >
            <Key aria-hidden="true" size={18} /> Reiniciar PIN
          </button>
          {persona.activo && (
            <button
              className="button danger"
              type="button"
              onClick={() => setConfirmacion("desactivar")}
              disabled={ocupada}
            >
              Desactivar
            </button>
          )}
          {esEstudiante && (
            <button
              form="formulario-expediente"
              className="button primary"
              disabled={
                !persona.activo || guardar.isPending || rutas.isLoading || !cambiosPendientes
              }
            >
              <FloppyDisk aria-hidden="true" size={18} /> Guardar cambios
            </button>
          )}
        </div>
      </footer>
      <ConfirmacionExpediente
        confirmacion={confirmacion}
        persona={persona}
        ocupada={ocupada}
        alCerrar={() => setConfirmacion(undefined)}
        alConfirmar={(tipo) => (tipo === "pin" ? reiniciarPin.mutate() : desactivar.mutate())}
      />
    </section>
  );
}
