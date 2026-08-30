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
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";

export default function TarifasVentas() {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const administrador = esAdministrador(session);
  const cliente = useQueryClient();
  const [mensaje, setMensaje] = useState("");
  const tarifas = useQuery({ queryKey: ["tarifas"], queryFn: plataformaApi.tiquetes.tarifas });
  const crear = useMutation({
    mutationFn: plataformaApi.tiquetes.crearTarifa,
    onSuccess: () => cliente.invalidateQueries({ queryKey: ["tarifas"] }),
  });
  const vender = useMutation({
    mutationFn: plataformaApi.tiquetes.vender,
    onSuccess: () => setMensaje("Venta registrada y saldo actualizado."),
  });
  function nuevaTarifa(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const d = new FormData(evento.currentTarget);
    crear.mutate({
      tipoPersona: d.get("tipoPersona") === "profesor" ? "profesor" : "estudiante",
      montoColones: Number(d.get("montoColones")),
      vigenteDesde: String(d.get("vigenteDesde")),
      vigenteHasta: null,
    });
  }
  function venta(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setMensaje("");
    const d = new FormData(evento.currentTarget);
    vender.mutate({
      codigo: String(d.get("codigo")),
      cantidad: Number(d.get("cantidad")),
      medioPago: String(d.get("medioPago")),
    });
    evento.currentTarget.reset();
  }
  const error = tarifas.error || crear.error || vender.error;
  return (
    <section>
      <EncabezadoPagina
        titulo="Tarifas y ventas"
        descripcion="Las tarifas tienen vigencia; cada venta conserva monto, medio de pago y operador."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      <div className="split-layout">
        <div>
          <h2>Registrar venta</h2>
          <form className="scanner-panel" onSubmit={venta}>
            <Campo etiqueta="Código">
              <input name="codigo" required placeholder="E-00000000 o P-00000000" />
            </Campo>
            <Campo etiqueta="Cantidad">
              <input name="cantidad" type="number" min="1" max="100" defaultValue="1" required />
            </Campo>
            <Campo etiqueta="Medio de pago">
              <select name="medioPago">
                <option value="efectivo">Efectivo</option>
                <option value="sinpe">SINPE</option>
                <option value="otro">Otro</option>
              </select>
            </Campo>
            <button className="button primary" disabled={vender.isPending}>
              Registrar venta
            </button>
          </form>
        </div>
        <div>
          <h2>{administrador ? "Nueva tarifa" : "Tarifas vigentes"}</h2>
          {administrador && (
            <form className="action-panel form-grid" onSubmit={nuevaTarifa}>
              <Campo etiqueta="Persona">
                <select name="tipoPersona">
                  <option value="estudiante">Estudiante</option>
                  <option value="profesor">Profesor</option>
                </select>
              </Campo>
              <Campo etiqueta="Monto (₡)">
                <input name="montoColones" type="number" min="0" required />
              </Campo>
              <Campo etiqueta="Vigente desde">
                <input name="vigenteDesde" type="date" required />
              </Campo>
              <button className="button secondary">Programar tarifa</button>
            </form>
          )}
          {tarifas.isLoading ? (
            <EstadoCarga />
          ) : (
            <Tabla
              columnas={["Tipo", "Monto", "Desde", "Hasta"]}
              filas={(tarifas.data?.elementos ?? []).map((t) => [
                t.tipoPersona,
                `₡${t.montoColones.toLocaleString("es-CR")}`,
                t.vigenteDesde,
                t.vigenteHasta ?? "Vigente",
              ])}
            />
          )}
        </div>
      </div>
    </section>
  );
}
