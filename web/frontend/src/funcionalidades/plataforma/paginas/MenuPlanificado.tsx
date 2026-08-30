import { useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import {
  Aviso,
  Campo,
  EncabezadoPagina,
  EstadoCarga,
  Tabla,
} from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";

export default function MenuPlanificado() {
  const cliente = useQueryClient();
  const [componentes, setComponentes] = useState("");
  const plantillas = useQuery({
    queryKey: ["plantillas-menu"],
    queryFn: plataformaApi.menu.plantillas,
  });
  const publicaciones = useQuery({
    queryKey: ["publicaciones-menu"],
    queryFn: plataformaApi.menu.publicaciones,
  });
  const crear = useMutation({
    mutationFn: plataformaApi.menu.crearPlantilla,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["plantillas-menu"] }),
  });
  const publicar = useMutation({
    mutationFn: plataformaApi.menu.publicar,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["publicaciones-menu"] }),
  });
  function nuevaPlantilla(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crear.mutate({
      nombre: String(datos.get("nombre")),
      componentes: componentes
        .split("\n")
        .map((c) => c.trim())
        .filter(Boolean),
      activa: true,
    });
    evento.currentTarget.reset();
    setComponentes("");
  }
  function nuevaPublicacion(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    publicar.mutate({
      fecha: String(datos.get("fecha")),
      plantillaId: Number(datos.get("plantillaId")),
    });
  }
  const error = plantillas.error || publicaciones.error || crear.error || publicar.error;
  return (
    <section>
      <EncabezadoPagina
        titulo="Menú"
        descripcion="Las plantillas son reutilizables; cada publicación guarda una copia que no cambia con ediciones futuras."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      <div className="split-layout">
        <div>
          <h2>Nueva plantilla</h2>
          <form className="action-panel stack" onSubmit={nuevaPlantilla}>
            <Campo etiqueta="Nombre">
              <input name="nombre" required />
            </Campo>
            <Campo etiqueta="Componentes, uno por línea">
              <textarea
                rows={6}
                value={componentes}
                onChange={(e) => setComponentes(e.target.value)}
                required
              />
            </Campo>
            <button className="button primary">Guardar plantilla</button>
          </form>
        </div>
        <div>
          <h2>Publicar para una fecha</h2>
          <form className="action-panel stack" onSubmit={nuevaPublicacion}>
            <Campo etiqueta="Fecha">
              <input name="fecha" type="date" required />
            </Campo>
            <Campo etiqueta="Plantilla">
              <select name="plantillaId" required>
                {plantillas.data?.elementos.map((p) => (
                  <option value={p.id} key={p.id}>
                    {p.nombre}
                  </option>
                ))}
              </select>
            </Campo>
            <button className="button primary">Publicar menú</button>
          </form>
        </div>
      </div>
      <h2>Publicaciones</h2>
      {publicaciones.isLoading ? (
        <EstadoCarga />
      ) : (
        <Tabla
          columnas={["Fecha", "Menú", "Componentes"]}
          filas={(publicaciones.data?.elementos ?? []).map((p) => [
            p.fecha,
            p.nombre,
            p.componentes.join(", "),
          ])}
        />
      )}
    </section>
  );
}
