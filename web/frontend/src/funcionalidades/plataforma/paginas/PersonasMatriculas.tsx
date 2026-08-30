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
import type { CredencialTemporal } from "@/compartido/contratos/plataforma";
import DialogoCredencialTemporal from "../componentes/DialogoCredencialTemporal";

export default function PersonasMatriculas() {
  const cliente = useQueryClient();
  const [buscar, setBuscar] = useState("");
  const [vista, setVista] = useState<"personas" | "matriculas">("personas");
  const [credencial, setCredencial] = useState<CredencialTemporal>();
  const personas = useQuery({
    queryKey: ["personas", buscar],
    queryFn: () => plataformaApi.personas.listar(buscar),
  });
  const anios = useQuery({ queryKey: ["anios"], queryFn: plataformaApi.anios.listar });
  const matriculas = useQuery({
    queryKey: ["matriculas"],
    queryFn: () => plataformaApi.matriculas.listar(),
  });
  const crearPersona = useMutation({
    mutationFn: plataformaApi.personas.crear,
    onSuccess: (persona) => {
      setCredencial({
        codigo: persona.codigo,
        nombre: persona.nombres,
        pinTemporal: persona.pinTemporal,
      });
      cliente.invalidateQueries({ queryKey: ["personas"] });
    },
  });
  const crearMatricula = useMutation({
    mutationFn: plataformaApi.matriculas.crear,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["matriculas"] }),
  });

  function enviarPersona(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crearPersona.mutate({
      cedula: String(datos.get("cedula")),
      nombres: String(datos.get("nombres")),
      apellidos: String(datos.get("apellidos")),
      tipo: datos.get("tipo") === "profesor" ? "profesor" : "estudiante",
      activo: true,
    });
    evento.currentTarget.reset();
  }

  function enviarMatricula(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crearMatricula.mutate({
      personaId: Number(datos.get("personaId")),
      anioLectivoId: Number(datos.get("anioLectivoId")),
      seccion: String(datos.get("seccion")),
      turno: String(datos.get("turno")),
      becaComedor: datos.get("becaComedor") === "on",
      estado: "activo",
    });
  }

  const error = crearPersona.error || crearMatricula.error || personas.error || matriculas.error;
  return (
    <section>
      <DialogoCredencialTemporal
        credencial={credencial}
        alCerrar={() => setCredencial(undefined)}
      />
      <EncabezadoPagina
        titulo="Personas y matrículas"
        descripcion="La identidad permanece; sección, turno y beca se registran por año lectivo."
      />
      <div className="tabs" role="tablist">
        <button
          className={vista === "personas" ? "active" : ""}
          onClick={() => setVista("personas")}
        >
          Personas
        </button>
        <button
          className={vista === "matriculas" ? "active" : ""}
          onClick={() => setVista("matriculas")}
        >
          Matrículas anuales
        </button>
      </div>
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {vista === "personas" ? (
        <>
          <form className="form-grid action-panel" onSubmit={enviarPersona}>
            <Campo etiqueta="Cédula">
              <input name="cedula" required />
            </Campo>
            <Campo etiqueta="Nombres">
              <input name="nombres" required />
            </Campo>
            <Campo etiqueta="Apellidos">
              <input name="apellidos" required />
            </Campo>
            <Campo etiqueta="Tipo">
              <select name="tipo">
                <option value="estudiante">Estudiante</option>
                <option value="profesor">Profesor</option>
              </select>
            </Campo>
            <button className="button primary" disabled={crearPersona.isPending}>
              Crear persona
            </button>
          </form>
          <Campo etiqueta="Buscar por código, cédula o nombre">
            <input
              value={buscar}
              onChange={(e) => setBuscar(e.target.value)}
              placeholder="Escriba para filtrar"
            />
          </Campo>
          {personas.isLoading ? (
            <EstadoCarga />
          ) : (
            <Tabla
              columnas={["Código", "Cédula", "Nombre", "Tipo", "Estado"]}
              filas={(personas.data?.elementos ?? []).map((p) => [
                p.codigo,
                p.cedula,
                [p.nombres, p.apellidos].filter(Boolean).join(" "),
                p.tipo,
                p.activo ? "Activa" : "Inactiva",
              ])}
            />
          )}
        </>
      ) : (
        <>
          <form className="form-grid action-panel" onSubmit={enviarMatricula}>
            <Campo etiqueta="Persona">
              <select name="personaId" required>
                {personas.data?.elementos
                  .filter((p) => p.tipo === "estudiante")
                  .map((p) => (
                    <option value={p.id} key={p.id}>
                      {p.codigo} — {p.nombres} {p.apellidos}
                    </option>
                  ))}
              </select>
            </Campo>
            <Campo etiqueta="Año lectivo">
              <select name="anioLectivoId" required>
                {anios.data?.elementos.map((a) => (
                  <option value={a.id} key={a.id}>
                    {a.anio}
                    {a.vigente ? " (vigente)" : ""}
                  </option>
                ))}
              </select>
            </Campo>
            <Campo etiqueta="Sección">
              <input name="seccion" required placeholder="Ej. 10-2" />
            </Campo>
            <Campo etiqueta="Turno">
              <select name="turno">
                <option>Mañana</option>
                <option>Tarde</option>
              </select>
            </Campo>
            <label className="check">
              <input name="becaComedor" type="checkbox" /> Beca de comedor
            </label>
            <button className="button primary" disabled={crearMatricula.isPending}>
              Registrar matrícula
            </button>
          </form>
          {matriculas.isLoading ? (
            <EstadoCarga />
          ) : (
            <Tabla
              columnas={["Persona", "Año", "Sección", "Turno", "Beca", "Estado"]}
              filas={(matriculas.data?.elementos ?? []).map((m) => [
                personas.data?.elementos.find((p) => p.id === m.personaId)?.codigo ?? m.personaId,
                anios.data?.elementos.find((a) => a.id === m.anioLectivoId)?.anio ??
                  m.anioLectivoId,
                m.seccion,
                m.turno,
                m.becaComedor ? "Sí" : "No",
                m.estado,
              ])}
            />
          )}
        </>
      )}
    </section>
  );
}
