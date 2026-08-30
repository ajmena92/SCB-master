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
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";

export default function RutasTransporte() {
  const cliente = useQueryClient();
  const [resultado, setResultado] = useState<ResultadoOperacion>();
  const rutas = useQuery({ queryKey: ["rutas"], queryFn: plataformaApi.rutas.listar });
  const crear = useMutation({
    mutationFn: plataformaApi.rutas.crear,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["rutas"] }),
  });
  const marcar = useMutation({
    mutationFn: plataformaApi.transporte.marcar,
    onSuccess: setResultado,
  });
  function nuevaRuta(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crear.mutate({
      nombre: String(datos.get("nombre")),
      descripcion: String(datos.get("descripcion")),
      activa: true,
    });
    evento.currentTarget.reset();
  }
  function registrar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    marcar.mutate(String(datos.get("codigo")));
    evento.currentTarget.reset();
  }
  const error = rutas.error || crear.error || marcar.error;
  return (
    <section>
      <EncabezadoPagina
        titulo="Rutas y transporte"
        descripcion="Administre rutas y registre una única marca diaria con la asignación vigente del estudiante."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      <div className="split-layout">
        <div>
          <h2>Catálogo de rutas</h2>
          <form className="action-panel form-grid" onSubmit={nuevaRuta}>
            <Campo etiqueta="Nombre">
              <input name="nombre" required />
            </Campo>
            <Campo etiqueta="Descripción">
              <input name="descripcion" required />
            </Campo>
            <button className="button primary">Crear ruta</button>
          </form>
          {rutas.isLoading ? (
            <EstadoCarga />
          ) : (
            <Tabla
              columnas={["Ruta", "Descripción", "Estado"]}
              filas={(rutas.data?.elementos ?? []).map((r) => [
                r.nombre,
                r.descripcion ?? "—",
                r.activa ? "Activa" : "Inactiva",
              ])}
            />
          )}
        </div>
        <div>
          <h2>Marca de transporte</h2>
          <form className="scanner-panel" onSubmit={registrar}>
            <Campo etiqueta="Código del estudiante">
              <input name="codigo" autoComplete="off" required placeholder="E-00000000" />
            </Campo>
            <button className="button primary" disabled={marcar.isPending}>
              Registrar marca
            </button>
          </form>
          {resultado && (
            <Aviso tipo={resultado.estado === "aceptada" ? "exito" : "error"}>
              <b>{resultado.estado.toUpperCase()}</b>
              <br />
              {resultado.mensaje}
            </Aviso>
          )}
        </div>
      </div>
    </section>
  );
}
