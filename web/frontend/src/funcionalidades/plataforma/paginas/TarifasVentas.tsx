import { useRef, useState, type FormEvent } from "react";
import { CashRegister, Minus, Plus, Scan, Ticket } from "@phosphor-icons/react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, Campo, EncabezadoPagina, EstadoCarga, Tabla } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";

export default function TarifasVentas() {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const administrador = esAdministrador(session);
  const cliente = useQueryClient();
  const codigoRef = useRef<HTMLInputElement>(null);
  const [codigo, setCodigo] = useState("");
  const [cantidad, setCantidad] = useState(1);
  const [medioPago, setMedioPago] = useState("efectivo");
  const [mensaje, setMensaje] = useState("");
  const tarifas = useQuery({ queryKey: ["tarifas"], queryFn: plataformaApi.tiquetes.tarifas });
  const crear = useMutation({ mutationFn: plataformaApi.tiquetes.crearTarifa, onSuccess: () => cliente.invalidateQueries({ queryKey: ["tarifas"] }) });
  const vender = useMutation({
    mutationFn: plataformaApi.tiquetes.vender,
    onSuccess: () => {
      setMensaje(`Venta registrada: ${cantidad} tiquete${cantidad === 1 ? "" : "s"} acreditado${cantidad === 1 ? "" : "s"}.`);
      setCodigo("");
      setCantidad(1);
      codigoRef.current?.focus();
    },
  });

  function nuevaTarifa(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const datos = new FormData(evento.currentTarget);
    crear.mutate({ tipoPersona: datos.get("tipoPersona") === "profesor" ? "profesor" : "estudiante", montoColones: Number(datos.get("montoColones")), vigenteDesde: String(datos.get("vigenteDesde")), vigenteHasta: null });
  }

  function venta(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    setMensaje("");
    vender.mutate({ codigo: codigo.trim(), cantidad, medioPago });
  }

  const error = tarifas.error || crear.error || vender.error;
  return <section className="pos-page">
    <EncabezadoPagina titulo="Punto de venta de tiquetes" descripcion="Escanee la cédula o código, confirme la cantidad y cobre en un solo paso." />
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
    {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
    <form className="pos-terminal" onSubmit={venta}>
      <div className="pos-terminal__header">
        <div className="pos-terminal__icon"><CashRegister size={28} weight="duotone" /></div>
        <div><p>Venta actual</p><h2>Recarga de tiquetes</h2></div>
        <span className="pos-terminal__status">Terminal lista</span>
      </div>
      <div className="pos-terminal__body">
        <label className="pos-scan-field">
          <span><Scan size={21} aria-hidden="true" /> Cédula o código del carnet</span>
          <input ref={codigoRef} value={codigo} onChange={(evento) => setCodigo(evento.target.value)} placeholder="Escanee o digite aquí" autoFocus required />
        </label>
        <div className="pos-controls">
          <div>
            <span className="pos-label">Cantidad de tiquetes</span>
            <div className="pos-stepper" aria-label="Cantidad de tiquetes">
              <button type="button" onClick={() => setCantidad((actual) => Math.max(1, actual - 1))} aria-label="Restar un tiquete"><Minus size={18} /></button>
              <output>{cantidad}</output>
              <button type="button" onClick={() => setCantidad((actual) => Math.min(100, actual + 1))} aria-label="Sumar un tiquete"><Plus size={18} /></button>
            </div>
          </div>
          <Campo etiqueta="Medio de pago"><select value={medioPago} onChange={(evento) => setMedioPago(evento.target.value)}><option value="efectivo">Efectivo</option><option value="sinpe">SINPE</option><option value="otro">Otro</option></select></Campo>
        </div>
      </div>
      <div className="pos-terminal__footer">
        <span><Ticket size={20} aria-hidden="true" /> {cantidad} tiquete{cantidad === 1 ? "" : "s"}</span>
        <button className="button primary" disabled={vender.isPending}>{vender.isPending ? "Procesando…" : "Confirmar venta"}</button>
      </div>
    </form>
    <section className="pos-tarifas" aria-labelledby="tarifas-title">
      <div className="pos-tarifas__heading"><div><h2 id="tarifas-title">Tarifas vigentes</h2><p>El sistema aplica automáticamente la tarifa que corresponda a la persona.</p></div>{administrador && <span className="person-tag person-tag--info">Administración de tarifas</span>}</div>
      {administrador && <form className="action-panel form-grid" onSubmit={nuevaTarifa}>
        <Campo etiqueta="Persona"><select name="tipoPersona"><option value="estudiante">Estudiante</option><option value="profesor">Profesor</option></select></Campo>
        <Campo etiqueta="Monto (₡)"><input name="montoColones" type="number" min="0" required /></Campo>
        <Campo etiqueta="Vigente desde"><input name="vigenteDesde" type="date" required /></Campo>
        <button className="button secondary">Programar tarifa</button>
      </form>}
      {tarifas.isLoading ? <EstadoCarga /> : <Tabla columnas={["Tipo", "Monto", "Desde", "Hasta"]} filas={(tarifas.data?.elementos ?? []).map((tarifa) => [tarifa.tipoPersona, `₡${tarifa.montoColones.toLocaleString("es-CR")}`, tarifa.vigenteDesde, tarifa.vigenteHasta ?? "Vigente"])} />}
    </section>
  </section>;
}
