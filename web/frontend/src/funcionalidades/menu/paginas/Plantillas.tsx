import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api, errMsg } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
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
type DatoMenu = Record<string, unknown>;
const campo = (dato: DatoMenu, canonico: string, legado: string): unknown =>
  dato[canonico] ?? dato[legado];

export default function Plantillas() {
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState<FormularioPlantilla | null>(null);
  const {
    data: plantillas = [],
    error,
    isPending: loading,
    refetch,
  } = useQuery({
    queryKey: ["admin", "menu", "plantillas"],
    queryFn: async () => {
      const { data } = await api.get<PlantillaMenu[]>("/v1/menu/plantillas");
      return data
        .map((p) => {
          const dato = p as unknown as DatoMenu;
          return {
            ...p,
            IdMenuPlantilla: campo(dato, "idPlantilla", "IdMenuPlantilla"),
            SemanaMes: campo(dato, "semana", "SemanaMes"),
            DiaSemana: campo(dato, "dia", "DiaSemana"),
            Titulo: campo(dato, "titulo", "Titulo"),
            Observaciones: campo(dato, "observaciones", "Observaciones"),
            Activo: campo(dato, "activo", "Activo"),
            Componentes: ((campo(dato, "componentes", "Componentes") as DatoMenu[]) ?? []).map(
              (c) => ({
                Nombre: campo(c, "nombre", "Nombre"),
                TipoComponente: campo(c, "tipo", "TipoComponente"),
                Orden: campo(c, "orden", "Orden"),
              }),
            ),
          } as PlantillaMenu;
        })
        .sort((a, b) => a.SemanaMes - b.SemanaMes || a.DiaSemana - b.DiaSemana);
    },
  });

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
      await api.post("/v1/menu/plantillas", {
        semana: Number(form.SemanaMes),
        dia: Number(form.DiaSemana),
        titulo: form.Titulo,
        observaciones: form.Observaciones || "",
        activo: !!form.Activo,
        componentes: form.Componentes.filter((c) => c.Nombre.trim()).map((c) => ({
          nombre: c.Nombre,
          tipo: c.TipoComponente,
          orden: c.Orden,
        })),
      });
      toast.success("Menú publicado");
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
        <div className="space-y-10">
          {[1, 2, 3, 4, 5].map((sem) => {
            const items = plantillas.filter((p) => p.SemanaMes === sem);
            return (
              <section key={sem} aria-labelledby={`semana-${sem}`}>
                <div className="mb-4 flex items-center gap-3">
                  <h3
                    id={`semana-${sem}`}
                    className="shrink-0 font-display text-sm font-bold text-foreground"
                  >
                    Semana {sem}
                  </h3>
                  <span className="h-px flex-1 bg-border" aria-hidden="true" />
                  <span className="shrink-0 text-xs font-medium tabular-nums text-muted-foreground">
                    {items.length} de 5 días
                  </span>
                </div>
                <div className="grid min-w-0 grid-cols-1 gap-4 min-[1100px]:grid-cols-2 min-[1680px]:grid-cols-3">
                  {items.length === 0 && (
                    <div className="rounded-2xl border border-dashed border-border bg-card/60 p-6 text-center min-[1100px]:col-span-2 min-[1680px]:col-span-3">
                      <Layers3
                        className="mx-auto h-6 w-6 text-muted-foreground"
                        aria-hidden="true"
                      />
                      <p className="mt-3 text-sm font-semibold text-foreground">
                        Todavía no hay menús para esta semana
                      </p>
                      <p className="mt-1 text-sm text-muted-foreground">
                        Creá una plantilla para comenzar.
                      </p>
                    </div>
                  )}
                  {items.map((p) => (
                    <article
                      key={p.IdMenuPlantilla}
                      data-testid={`plantilla-${p.SemanaMes}-${p.DiaSemana}`}
                      className={`group flex min-h-48 min-w-0 flex-col rounded-2xl border p-5 transition-[border-color,box-shadow,transform] duration-200 hover:-translate-y-0.5 hover:border-primary/40 hover:shadow-[0_14px_32px_rgb(45_54_150_/_0.08)] ${p.Activo ? "border-border/90 bg-card" : "border-dashed border-border bg-muted/40"}`}
                    >
                      <div className="flex min-w-0 items-center justify-between gap-3">
                        <span className="text-sm font-semibold text-muted-foreground">
                          {DIAS_MENU[p.DiaSemana]}
                        </span>
                        {!p.Activo && <Badge variant="secondary">Inactivo</Badge>}
                      </div>
                      <p
                        className="mt-3 min-w-0 break-words text-pretty font-display text-lg font-bold leading-snug text-foreground"
                        title={p.Titulo}
                      >
                        {p.Titulo}
                      </p>
                      <div className="mt-auto flex min-w-0 items-end justify-between gap-3 pt-5">
                        <span className="flex min-w-0 items-center gap-2 text-sm text-muted-foreground">
                          <Layers3 className="h-4 w-4 shrink-0" aria-hidden="true" />
                          <span className="truncate tabular-nums">
                            {p.Componentes.length} componentes
                          </span>
                        </span>
                        <Button
                          variant="ghost"
                          className="h-11 shrink-0 rounded-xl px-3 text-primary hover:bg-primary/10 hover:text-primary"
                          data-testid={`edit-plantilla-${p.SemanaMes}-${p.DiaSemana}`}
                          onClick={() => abrir(p)}
                        >
                          <Pencil className="h-4 w-4" /> Editar
                        </Button>
                      </div>
                    </article>
                  ))}
                </div>
              </section>
            );
          })}
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
