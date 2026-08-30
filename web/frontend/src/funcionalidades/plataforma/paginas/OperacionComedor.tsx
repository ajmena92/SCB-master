import { useState, type FormEvent } from "react";
import { useMutation } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, Campo, EncabezadoPagina } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";

export default function OperacionComedor() {
  const [resultado, setResultado] = useState<ResultadoOperacion>();
  const ingreso = useMutation({
    mutationFn: plataformaApi.comedor.registrarIngreso,
    onSuccess: setResultado,
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
  function registrar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    ingreso.mutate(String(datos.get("codigo")));
    evento.currentTarget.reset();
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
  return (
    <section>
      <EncabezadoPagina
        titulo="Operación del comedor"
        descripcion="Registre el ingreso. El sistema decide reserva, beca y consumo sin permitir dobles movimientos."
      />
      {(ingreso.error || decision.error) && (
        <Aviso tipo="error">{errMsg(ingreso.error || decision.error)}</Aviso>
      )}
      <div className="split-layout operation-grid">
        <div>
          <h2>Ingreso</h2>
          <form className="scanner-panel" onSubmit={registrar}>
            <Campo etiqueta="Código de estudiante o profesor">
              <input
                name="codigo"
                autoComplete="off"
                required
                placeholder="Escanee o escriba el código"
              />
            </Campo>
            <button className="button primary jumbo" disabled={ingreso.isPending}>
              Registrar ingreso
            </button>
          </form>
          {resultado && (
            <Aviso
              tipo={
                resultado.estado === "aceptada"
                  ? "exito"
                  : resultado.estado === "pendiente"
                    ? "info"
                    : "error"
              }
            >
              <strong>
                {resultado.estado === "aceptada"
                  ? "Ingreso aceptado"
                  : resultado.estado === "pendiente"
                    ? "Requiere autorización"
                    : "Ingreso rechazado"}
              </strong>
              <br />
              {resultado.mensaje}
              {resultado.saldo !== undefined && <> Saldo: {resultado.saldo}.</>}
            </Aviso>
          )}
        </div>
        <div>
          <h2>Decidir excepción sin reserva</h2>
          <form className="action-panel stack" onSubmit={decidir}>
            <Campo etiqueta="Código del estudiante">
              <input name="codigo" placeholder="E-00000000" required />
            </Campo>
            <Campo etiqueta="Decisión">
              <select name="decision">
                <option value="aprobada">Aprobar</option>
                <option value="rechazada">Rechazar</option>
              </select>
            </Campo>
            <Campo etiqueta="Observación">
              <textarea name="observacion" rows={3} required />
            </Campo>
            <button className="button secondary" disabled={decision.isPending}>
              Guardar decisión
            </button>
          </form>
        </div>
      </div>
    </section>
  );
}
