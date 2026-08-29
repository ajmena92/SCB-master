import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import {
  consultarSustituciones,
  guardarSustitucion,
} from "@/funcionalidades/administracion/consultas/menu";
import {
  prepararComponente,
  prepararComponentes,
} from "@/funcionalidades/menu/componentesMenu";
import { toast } from "sonner";

export function useSustituciones() {
  const [open, setOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [form, setForm] = useState(null);
  const consulta = useQuery({
    queryKey: ["admin", "menu", "sustituciones"],
    queryFn: async () => {
      const sustituciones = await consultarSustituciones();
      return sustituciones.map((sustitucion) => ({
        IdMenuSustitucion: sustitucion.idSustitucion,
        Fecha: sustitucion.fecha,
        Titulo: sustitucion.titulo,
        Observaciones: sustitucion.observaciones || "",
        Componentes: prepararComponentes(
          (sustitucion.componentes || []).map((componente) => ({
            Orden: componente.orden || 1,
            Nombre: componente.nombre,
            TipoComponente: componente.tipo || "Principal",
          })),
        ),
      }));
    },
  });
  useEffect(() => {
    if (consulta.error) toast.error(mensajeError(consulta.error));
  }, [consulta.error]);
  function abrir(sustitucion) {
    setForm(
      sustitucion
        ? { ...sustitucion, Componentes: prepararComponentes(sustitucion.Componentes) }
        : {
            Fecha: new Date().toISOString().slice(0, 10),
            Titulo: "",
            Observaciones: "",
            Componentes: [
              prepararComponente({ Orden: 1, Nombre: "", TipoComponente: "Principal" }),
            ],
          },
    );
    setOpen(true);
  }
  function setComp(indice, campo, valor) {
    setForm((actual) => ({
      ...actual,
      Componentes: actual.Componentes.map((componente, i) =>
        i === indice ? { ...componente, [campo]: valor } : componente,
      ),
    }));
  }
  function addComp() {
    setForm((actual) => ({
      ...actual,
      Componentes: [
        ...actual.Componentes,
        prepararComponente({
          Orden: actual.Componentes.length + 1,
          Nombre: "",
          TipoComponente: "Acompañamiento",
        }),
      ],
    }));
  }
  function delComp(indice) {
    setForm((actual) => ({
      ...actual,
      Componentes: actual.Componentes.filter((_, i) => i !== indice).map((componente, i) => ({
        ...componente,
        Orden: i + 1,
      })),
    }));
  }
  async function guardar() {
    if (!form.Titulo.trim()) return toast.error("El título es obligatorio");
    setSaving(true);
    try {
      await guardarSustitucion({
        fecha: form.Fecha,
        titulo: form.Titulo,
        observaciones: form.Observaciones || "",
        componentes: form.Componentes.filter((componente) => componente.Nombre.trim()).map(
          (componente) => ({
            nombre: componente.Nombre,
            tipo: componente.TipoComponente,
            orden: componente.Orden,
          }),
        ),
      });
      toast.success("Sustitución guardada (prevalece sobre la plantilla)");
      setOpen(false);
      await consulta.refetch();
    } catch (error) {
      toast.error(mensajeError(error));
    } finally {
      setSaving(false);
    }
  }
  return {
    open,
    setOpen,
    saving,
    form,
    subs: consulta.data ?? [],
    loading: consulta.isPending,
    abrir,
    setForm,
    setComp,
    addComp,
    delComp,
    guardar,
  };
}
