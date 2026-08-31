import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { toast } from "sonner";
import { AlertCircle, Layers3, Plus, Pencil, RefreshCw } from "lucide-react";
import { prepararComponente, prepararComponentes } from "@/funcionalidades/menu/componentesMenu";
import type { ComponenteMenu } from "@/funcionalidades/menu/componentesMenu";
import { DIAS_MENU, EditorPlantilla } from "@/funcionalidades/menu/EditorPlantilla";
import type { FormularioPlantilla } from "@/funcionalidades/menu/EditorPlantilla";

interface PlantillaMenu extends Omit<FormularioPlantilla, "Componentes"> {
  IdMenuPlantilla: number;
  Componentes: ComponenteMenu[];
}
const SEMANAS_MENU = [1, 2, 3, 4, 5] as const;

function fechaLocalActual(): string {
  const ahora = new Date();
  const completar = (valor: number) => String(valor).padStart(2, "0");
  return `${ahora.getFullYear()}-${completar(ahora.getMonth() + 1)}-${completar(ahora.getDate())}`;
}

function obtenerSemanaActual(): number {
  return Math.min(5, Math.ceil(new Date().getDate() / 7));
}

function obtenerDiaActual(): number | null {
  const dia = new Date().getDay();
  return dia >= 1 && dia <= 5 ? dia : null;
}

type DatoMenu = Record<string, unknown>;
const campo = (dato: DatoMenu, canonico: string, legado: string): unknown =>
  dato[canonico] ?? dato[legado];

export default function Plantillas() {
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<FormularioPlantilla | null>(null);
  const semanaActual = obtenerSemanaActual();
  const [semanaActiva, setSemanaActiva] = useState<number>(semanaActual);
  const diaActual = obtenerDiaActual();
  const {
    data: plantillas = [],
    error,
    isPending: loading,
    refetch,
  } = useQuery({
    queryKey: ["admin", "menu", "plantillas"],
    staleTime: 60_000,
    queryFn: async () => {
      const { data } = await api.get<PlantillaMenu[]>("/v1/menu/plantillas");
      return data
        .map((p, indice) => {
          const dato = p as unknown as DatoMenu;
          const componentes = (campo(dato, "componentes", "Componentes") as unknown[]) ?? [];
          return {
            ...p,
            IdMenuPlantilla: Number(campo(dato, "id", "IdMenuPlantilla")),
            SemanaMes: Number(campo(dato, "semana", "SemanaMes") ?? Math.floor(indice / 5) + 1),
            DiaSemana: Number(campo(dato, "dia", "DiaSemana") ?? (indice % 5) + 1),
            Titulo: String(campo(dato, "nombre", "Titulo") ?? "Menú"),
            Observaciones: String(campo(dato, "observaciones", "Observaciones") ?? ""),
            Activo: Boolean(campo(dato, "activa", "Activo") ?? true),
            Componentes: componentes.map((componente, orden) => {
              const c =
                typeof componente === "string"
                  ? ({ nombre: componente } as DatoMenu)
                  : (componente as DatoMenu);
              return {
                Nombre: String(campo(c, "nombre", "Nombre") ?? ""),
                TipoComponente: String(campo(c, "tipo", "TipoComponente") ?? "Principal"),
                Orden: Number(campo(c, "orden", "Orden") ?? orden + 1),
              };
            }),
          } as PlantillaMenu;
        })
        .sort((a, b) => a.SemanaMes - b.SemanaMes || a.DiaSemana - b.DiaSemana);
    },
  });

  const plantillasPorSemana = useMemo(() => {
    const grupos = new Map<number, PlantillaMenu[]>();
    for (const semana of SEMANAS_MENU) {
      grupos.set(semana, []);
    }
    for (const plantilla of plantillas) {
      grupos.get(plantilla.SemanaMes)?.push(plantilla);
    }
    for (const items of grupos.values()) {
      items.sort((a, b) => a.DiaSemana - b.DiaSemana);
    }
    return grupos;
  }, [plantillas]);

  const plantillasSemanaActiva = plantillasPorSemana.get(semanaActiva) ?? [];

  useEffect(() => {
    if (error) toast.error(errMsg(error));
  }, [error]);

  const abrir = (plantilla: PlantillaMenu | null) => {
    setForm(
      plantilla
        ? { ...plantilla, Componentes: prepararComponentes(plantilla.Componentes) }
        : {
            SemanaMes: 1,
            DiaSemana: 1,
            Titulo: "",
            Observaciones: "",
            Activo: true,
            Componentes: [
              prepararComponente({ Orden: 1, Nombre: "", TipoComponente: "Principal" }),
            ],
          },
    );
    setOpen(true);
  };

  const guardar = async () => {
    if (!form) return;
    if (!form.Titulo.trim()) {
      toast.error("El título es obligatorio");
      return;
    }
    setSaving(true);
    try {
      const payload = {
        nombre: form.Titulo,
        componentes: form.Componentes.filter((c) => c.Nombre.trim()).map((c) => c.Nombre),
      };
      const { data: plantilla } = form.IdMenuPlantilla
        ? await api.put(`/v1/menu/plantillas/${form.IdMenuPlantilla}`, payload)
        : await api.post("/v1/menu/plantillas", payload);
      await api.post("/v1/menu/publicaciones", {
        plantillaId: plantilla.id,
        fecha: fechaLocalActual(),
      });
      toast.success("Menú guardado y publicado para hoy");
      setOpen(false);
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-w-0 space-y-8">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="min-w-0">
          <h2 className="text-pretty font-display text-2xl font-[900] tracking-tight">
            Plantillas de menú
          </h2>
          <p className="mt-1 max-w-[65ch] text-sm leading-relaxed text-muted-foreground">
            Organizá las cinco semanas de lunes a viernes. Los cambios se publican cuando guardás.
          </p>
        </div>
        <Button
          className="w-full shrink-0 sm:w-auto"
          data-testid="new-plantilla-btn"
          onClick={() => abrir(null)}
        >
          <Plus className="h-4 w-4" /> Nueva plantilla
        </Button>
      </div>

      {loading ? (
        <div
          className="grid grid-cols-1 gap-4 min-[1100px]:grid-cols-2 min-[1680px]:grid-cols-3"
          aria-label="Cargando plantillas"
        >
          {[0, 1, 2, 3].map((item) => (
            <Skeleton key={item} className="h-52 w-full rounded-2xl" />
          ))}
        </div>
      ) : error ? (
        <div
          className="rounded-2xl border border-destructive/30 bg-card p-6"
          role="alert"
          data-testid="plantillas-error"
        >
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex min-w-0 items-start gap-3">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-destructive/10 text-destructive">
                <AlertCircle className="h-5 w-5" />
              </span>
              <div className="min-w-0">
                <h3 className="font-display font-bold">No pudimos cargar las plantillas</h3>
                <p className="mt-1 break-words text-sm text-muted-foreground">{errMsg(error)}</p>
              </div>
            </div>
            <Button
              variant="outline"
              className="w-full shrink-0 sm:w-auto"
              data-testid="plantillas-retry"
              onClick={() => refetch()}
            >
              <RefreshCw className="h-4 w-4" /> Reintentar
            </Button>
          </div>
        </div>
      ) : (
        <div className="space-y-5">
          <nav
            aria-label="Semanas del menú"
            className="hidden gap-1 overflow-x-auto rounded-xl border border-border bg-muted/40 p-1 md:flex"
          >
            {SEMANAS_MENU.map((semana) => {
              const cantidad = plantillasPorSemana.get(semana)?.length ?? 0;
              const activa = semana === semanaActiva;
              return (
                <button
                  key={semana}
                  type="button"
                  role="tab"
                  aria-selected={activa}
                  data-testid={`semana-tab-${semana}`}
                  onClick={() => setSemanaActiva(semana)}
                  className={`min-h-11 min-w-28 shrink-0 rounded-lg px-4 py-2 text-left transition-colors ${activa ? "bg-background text-foreground shadow-sm" : "text-muted-foreground hover:bg-background/70 hover:text-foreground"}`}
                >
                  <span className="block text-sm font-bold">Semana {semana}</span>
                  <span className="block text-xs tabular-nums">{cantidad} de 5 días</span>
                </button>
              );
            })}
          </nav>

          <div className="rounded-xl border border-border bg-muted/40 p-3 md:hidden">
            <label
              htmlFor="selector-semana-movil"
              className="mb-2 block text-xs font-semibold uppercase tracking-wide text-muted-foreground"
            >
              Semana del menú
            </label>
            <Select
              value={String(semanaActiva)}
              onValueChange={(valor) => setSemanaActiva(Number(valor))}
            >
              <SelectTrigger id="selector-semana-movil" className="min-h-12 bg-background">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {SEMANAS_MENU.map((semana) => (
                  <SelectItem key={semana} value={String(semana)}>
                    Semana {semana} · {plantillasPorSemana.get(semana)?.length ?? 0} de 5 días
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <section aria-labelledby={`semana-${semanaActiva}`}>
            <div className="flex items-end justify-between gap-4">
              <div>
                <h3 id={`semana-${semanaActiva}`} className="font-display text-lg font-bold">
                  Semana {semanaActiva}
                </h3>
                <p className="mt-1 text-sm text-muted-foreground">
                  Revisá y publicá los menús de lunes a viernes.
                </p>
              </div>
              <span className="shrink-0 text-sm font-medium tabular-nums text-muted-foreground">
                {plantillasSemanaActiva.length} de 5 días
              </span>
            </div>

            {plantillasSemanaActiva.length === 0 ? (
              <div className="mt-4 rounded-xl border border-dashed border-border bg-card/60 p-8 text-center">
                <Layers3 className="mx-auto h-6 w-6 text-muted-foreground" aria-hidden="true" />
                <p className="mt-3 text-sm font-semibold text-foreground">
                  Todavía no hay menús para esta semana
                </p>
                <p className="mt-1 text-sm text-muted-foreground">
                  Creá una plantilla para comenzar.
                </p>
              </div>
            ) : (
              <div className="mt-4 hidden overflow-x-auto rounded-xl border border-border bg-card md:block">
                <table className="w-full min-w-[40rem] text-left text-sm">
                  <caption className="sr-only">Menús de la semana {semanaActiva}</caption>
                  <thead className="border-b border-border bg-muted/40 text-xs uppercase tracking-wide text-muted-foreground">
                    <tr>
                      <th className="w-32 px-4 py-3 font-semibold">Día</th>
                      <th className="px-4 py-3 font-semibold">Menú publicado</th>
                      <th className="w-40 px-4 py-3 font-semibold">Componentes</th>
                      <th className="w-28 px-4 py-3 text-right font-semibold">Acción</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-border">
                    {plantillasSemanaActiva.map((p) => (
                      <tr
                        key={p.IdMenuPlantilla}
                        data-testid={`plantilla-${p.SemanaMes}-${p.DiaSemana}`}
                        className={`group min-w-0 align-middle transition-colors hover:bg-muted/30 ${semanaActiva === semanaActual && p.DiaSemana === diaActual ? "bg-primary/[0.06]" : ""}`}
                      >
                        <td className="whitespace-nowrap px-4 py-4 font-semibold text-foreground">
                          <span className="inline-flex items-center gap-2">
                            {DIAS_MENU[p.DiaSemana]}
                            {semanaActiva === semanaActual && p.DiaSemana === diaActual && (
                              <Badge variant="default" className="px-2 py-0.5 text-[10px]">
                                Hoy
                              </Badge>
                            )}
                          </span>
                        </td>
                        <td className="min-w-0 px-4 py-4">
                          <p
                            className="min-w-0 break-words text-pretty font-display font-bold leading-snug text-foreground"
                            title={p.Titulo}
                          >
                            {p.Titulo}
                          </p>
                          {p.Componentes.length > 0 && (
                            <p className="mt-1 min-w-0 truncate text-xs text-muted-foreground">
                              {p.Componentes.slice(0, 2)
                                .map((componente) => componente.Nombre)
                                .join(" · ")}
                              {p.Componentes.length > 2 && ` · +${p.Componentes.length - 2}`}
                            </p>
                          )}
                          {!p.Activo && (
                            <Badge variant="secondary" className="mt-2">
                              Inactivo
                            </Badge>
                          )}
                        </td>
                        <td className="whitespace-nowrap px-4 py-4 text-muted-foreground">
                          <span className="inline-flex items-center gap-2">
                            <Layers3 className="h-4 w-4" aria-hidden="true" />
                            <span className="tabular-nums">{p.Componentes.length}</span>
                          </span>
                        </td>
                        <td className="px-4 py-4 text-right">
                          <Button
                            variant="ghost"
                            className="h-10 rounded-lg px-3 text-primary hover:bg-primary/10 hover:text-primary"
                            data-testid={`edit-plantilla-${p.SemanaMes}-${p.DiaSemana}`}
                            onClick={() => abrir(p)}
                          >
                            <Pencil className="h-4 w-4" /> Editar
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            {plantillasSemanaActiva.length > 0 && (
              <div className="mt-4 space-y-2 md:hidden">
                {plantillasSemanaActiva.map((p) => {
                  const esHoy = semanaActiva === semanaActual && p.DiaSemana === diaActual;
                  return (
                    <article
                      key={`movil-${p.IdMenuPlantilla}`}
                      data-testid={`plantilla-movil-${p.SemanaMes}-${p.DiaSemana}`}
                      className={`rounded-xl border p-4 ${esHoy ? "border-primary/40 bg-primary/[0.06]" : "border-border bg-card"}`}
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div className="min-w-0">
                          <div className="flex flex-wrap items-center gap-2">
                            <span className="text-sm font-bold text-foreground">
                              {DIAS_MENU[p.DiaSemana]}
                            </span>
                            {esHoy && (
                              <Badge variant="default" className="px-2 py-0.5 text-[10px]">
                                Hoy
                              </Badge>
                            )}
                            {!p.Activo && <Badge variant="secondary">Inactivo</Badge>}
                          </div>
                          <p
                            className="mt-2 break-words text-pretty font-display font-bold leading-snug text-foreground"
                            title={p.Titulo}
                          >
                            {p.Titulo}
                          </p>
                          {p.Componentes.length > 0 && (
                            <p className="mt-1 truncate text-xs text-muted-foreground">
                              {p.Componentes.slice(0, 2)
                                .map((componente) => componente.Nombre)
                                .join(" · ")}
                              {p.Componentes.length > 2 && ` · +${p.Componentes.length - 2}`}
                            </p>
                          )}
                        </div>
                        <Button
                          variant="ghost"
                          className="h-10 shrink-0 rounded-lg px-3 text-primary hover:bg-primary/10 hover:text-primary"
                          data-testid={`edit-plantilla-movil-${p.SemanaMes}-${p.DiaSemana}`}
                          onClick={() => abrir(p)}
                        >
                          <Pencil className="h-4 w-4" />
                          <span className="sr-only sm:not-sr-only">Editar</span>
                        </Button>
                      </div>
                      <div className="mt-3 flex items-center gap-2 text-xs text-muted-foreground">
                        <Layers3 className="h-4 w-4" aria-hidden="true" />
                        <span className="tabular-nums">{p.Componentes.length} componentes</span>
                      </div>
                    </article>
                  );
                })}
              </div>
            )}
          </section>
        </div>
      )}

      <EditorPlantilla
        open={open}
        onOpenChange={setOpen}
        form={form}
        setForm={setForm}
        saving={saving}
        onGuardar={guardar}
      />
    </div>
  );
}
