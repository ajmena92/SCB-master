import { useEffect, useMemo, useRef, useState, type FormEvent } from "react";
import { CashRegister, CheckCircle, CircleNotch, Minus, Plus, Printer, Scan, Ticket, UserCircle, WarningCircle } from "@phosphor-icons/react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import type { Persona } from "@/compartido/contratos/plataforma";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, Campo } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";
import { AlertDialog, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from "@/components/ui/alert-dialog";

type PersonaVenta = Persona & { becado: boolean; saldoTiquetes: number };
const moneda = new Intl.NumberFormat("es-CR", { style: "currency", currency: "CRC", maximumFractionDigits: 0 });
const escaparHtml = (valor: string) => valor.replace(/[&<>"']/g, (caracter) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" })[caracter] ?? caracter);

function imprimirComprobante(datos: { persona: PersonaVenta; cantidad: number; total: number; medioPago: string; saldoFinal: number }) {
  const ventana = window.open("", "_blank");
  if (!ventana) return;
  const fecha = new Intl.DateTimeFormat("es-CR", { dateStyle: "medium", timeStyle: "short" }).format(new Date());
  ventana.document.write(`<!doctype html><html lang="es"><head><title>Comprobante de venta</title><style>body{font-family:"Segoe UI",sans-serif;color:#172033;margin:28px;max-width:680px}header{padding-bottom:14px;border-bottom:2px solid #0a84ff}h1,p{margin:0}h1{font-size:22px;font-weight:600}p{margin-top:4px;color:#5d6b7e}dl{display:grid;grid-template-columns:1fr auto;gap:10px;margin:24px 0}dt{color:#5d6b7e}dd{margin:0;font-weight:600;text-align:right}.total{padding:16px;background:#f0f7ff;border:1px solid #bfdbfe;border-radius:8px;font-size:20px}.total dd{color:#0071e3;font-size:24px}footer{margin-top:24px;padding-top:12px;border-top:1px solid #dbe2ea;font-size:12px}@media print{body{margin:14mm}}</style></head><body><header><p>CTP Platanares · Comedor</p><h1>Comprobante de venta de tiquetes</h1><p>${escaparHtml(fecha)}</p></header><dl><dt>Persona</dt><dd>${escaparHtml(datos.persona.nombres)}</dd><dt>Cédula</dt><dd>${escaparHtml(datos.persona.cedula ?? "")}</dd><dt>Tiquetes vendidos</dt><dd>${datos.cantidad}</dd><dt>Total de tiquetes</dt><dd>${datos.saldoFinal}</dd><dt>Medio de pago</dt><dd>${escaparHtml(datos.medioPago)}</dd><div class="total"><dt>Total cobrado</dt><dd>${moneda.format(datos.total)}</dd></div></dl><footer>Venta registrada correctamente. Conserve este comprobante para control administrativo.</footer></body></html>`);
  ventana.document.close();
  window.setTimeout(() => ventana.print(), 100);
}

export default function TarifasVentas() {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const administrador = esAdministrador(session);
  const cliente = useQueryClient();
  const cedulaRef = useRef<HTMLInputElement>(null);
  const [buscar, setBuscar] = useState("");
  const [buscarAplicado, setBuscarAplicado] = useState("");
  const [persona, setPersona] = useState<PersonaVenta>();
  const [cantidad, setCantidad] = useState(1);
  const [medioPago, setMedioPago] = useState("efectivo");
  const [mensaje, setMensaje] = useState("");
  const [fotoUrl, setFotoUrl] = useState<string>();
  const [comprobante, setComprobante] = useState<{ persona: PersonaVenta; cantidad: number; total: number; medioPago: string; saldoFinal: number }>();
  const tarifas = useQuery({ queryKey: ["tarifas"], queryFn: plataformaApi.tiquetes.tarifas });
  const resultados = useQuery({ queryKey: ["tiquetes", "personas", buscarAplicado], queryFn: () => plataformaApi.tiquetes.buscarPersonas(buscarAplicado), enabled: buscarAplicado.length >= 3 });
  const vender = useMutation({ mutationFn: plataformaApi.tiquetes.vender, onSuccess: (_, datos) => { setMensaje(`Venta exitosa: ${datos.cantidad} tiquete${datos.cantidad === 1 ? "" : "s"} vendido${datos.cantidad === 1 ? "" : "s"} por ${moneda.format(total)}.`); if (persona) setComprobante({ persona, cantidad: datos.cantidad, total, medioPago: datos.medioPago, saldoFinal: persona.saldoTiquetes + datos.cantidad }); setBuscar(""); setBuscarAplicado(""); setPersona(undefined); setCantidad(1); cliente.invalidateQueries({ queryKey: ["tiquetes", "personas"] }); cedulaRef.current?.focus(); } });

  useEffect(() => { const espera = window.setTimeout(() => setBuscarAplicado(buscar.trim()), 250); return () => window.clearTimeout(espera); }, [buscar]);
  useEffect(() => { const exacta = resultados.data?.find((item) => item.cedula === buscarAplicado); if (exacta) setPersona(exacta); }, [buscarAplicado, resultados.data]);
  useEffect(() => { let url: string | undefined; setFotoUrl(undefined); if (!persona) return; plataformaApi.tiquetes.fotoPersona(persona.id).then((foto) => { url = URL.createObjectURL(foto); setFotoUrl(url); }).catch(() => undefined); return () => { if (url) URL.revokeObjectURL(url); }; }, [persona]);

  const hoy = new Date().toISOString().slice(0, 10);
  const tarifa = useMemo(() => tarifas.data?.elementos.find((item) => item.tipoPersona === persona?.tipo && item.vigenteDesde <= hoy && (!item.vigenteHasta || item.vigenteHasta >= hoy)), [persona?.tipo, tarifas.data, hoy]);
  const sinTarifaParaPersona = Boolean(persona && !tarifa);
  const total = (tarifa?.montoColones ?? 0) * cantidad;
  const error = tarifas.error || resultados.error || vender.error;
  function venta(evento: FormEvent<HTMLFormElement>) { evento.preventDefault(); if (!persona?.cedula) return; setMensaje(""); vender.mutate({ cedula: persona.cedula, cantidad, medioPago }); }
  return <section className="pos-page">
    <AlertDialog open={Boolean(comprobante)} onOpenChange={(abierto) => !abierto && setComprobante(undefined)}><AlertDialogContent className="pos-receipt-dialog"><AlertDialogHeader><AlertDialogTitle>Venta registrada correctamente</AlertDialogTitle><AlertDialogDescription>El saldo de tiquetes se actualizó. Entregue o guarde el comprobante antes de la siguiente venta.</AlertDialogDescription></AlertDialogHeader>{comprobante && <article className="pos-receipt" aria-label="Detalle de la venta"><header><Ticket aria-hidden="true" size={24} /><div><span>COMPROBANTE DE VENTA</span><strong>CTP Platanares · Comedor</strong></div></header><div className="pos-receipt__person"><span>Persona</span><strong>{comprobante.persona.nombres}</strong><small>{comprobante.persona.cedula}</small></div><dl><div><dt>Tiquetes vendidos</dt><dd>{comprobante.cantidad}</dd></div><div><dt>Saldo final</dt><dd>{comprobante.saldoFinal} tiquetes</dd></div><div><dt>Medio de pago</dt><dd>{comprobante.medioPago}</dd></div></dl><footer><span>Total cobrado</span><strong>{moneda.format(comprobante.total)}</strong></footer></article>}<AlertDialogFooter><button className="button secondary" type="button" onClick={() => setComprobante(undefined)}>Nueva venta</button><button className="button primary" type="button" onClick={() => comprobante && imprimirComprobante(comprobante)}><Printer aria-hidden="true" size={18} /> Imprimir / guardar PDF</button></AlertDialogFooter></AlertDialogContent></AlertDialog>
    {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}{mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
    {!tarifas.isLoading && sinTarifaParaPersona && <aside className="pos-prerequisite" role="status"><WarningCircle aria-hidden="true" size={28} weight="fill" /><div><p>Antes de cobrar</p><h2>No hay una tarifa vigente para esta persona</h2><span>Configure la tarifa correspondiente para habilitar la venta.</span></div>{administrador && <Link className="button secondary" to="/admin/panel/parametros">Configurar tarifa</Link>}</aside>}
    <form className="pos-terminal" onSubmit={venta}>
      <div className="pos-terminal__header"><div className="pos-terminal__identity"><div className="pos-terminal__icon"><CashRegister size={25} weight="duotone" /></div><div><p>Punto de venta</p><h2>Venta de tiquetes</h2></div></div><span className="pos-terminal__status">{resultados.isFetching ? "Buscando…" : persona?.becado ? "Compra no disponible" : persona ? "Listo para cobrar" : "Paso 1 de 3"}</span></div>
      <ol className="pos-flow" aria-label="Pasos de la venta"><li className="is-active"><span>1</span>Buscar persona</li><li className={persona ? "is-active" : ""}><span>2</span>Cantidad y pago</li><li className={persona && tarifa && !persona.becado ? "is-active" : ""}><span>3</span>Confirmar</li></ol>
      <div className="pos-terminal__body">
        <div className="pos-client"><label className="pos-scan-field"><span><Scan size={20} aria-hidden="true" /> Cédula o nombre</span><input ref={cedulaRef} value={buscar} onChange={(evento) => { setBuscar(evento.target.value); setPersona(undefined); }} placeholder="Digite cédula o nombre" autoFocus /></label>{resultados.isFetching && <span className="section-help"><CircleNotch className="pos-spin" size={16} aria-hidden="true" /> Buscando personas…</span>}{!persona && buscarAplicado.length >= 3 && !resultados.isFetching && resultados.data?.length === 0 && <p className="pos-empty-result">No se encontró una persona con esos datos.</p>}{!persona && resultados.data?.length ? <div className="pos-search-results" aria-label="Resultados de búsqueda">{resultados.data.map((item) => <button type="button" key={item.id} onClick={() => { setPersona(item); setBuscar(item.cedula ?? item.nombres); }}><span>{item.nombres}</span><small>{item.cedula} · {item.tipo}</small></button>)}</div> : null}{persona && <div className="pos-person-card"><div className="pos-person-photo">{fotoUrl ? <img src={fotoUrl} alt={`Fotografía de ${persona.nombres}`} /> : <UserCircle aria-hidden="true" size={54} />}</div><div><span className="pos-person-card__type">{persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}</span><strong>{persona.nombres}</strong><span>{persona.cedula}</span><span className="pos-person-card__balance">Saldo actual <b>{persona.saldoTiquetes} tiquetes</b></span>{persona.becado && <em><WarningCircle size={17} weight="fill" aria-hidden="true" /> Beneficiario de comedor: no puede comprar tiquetes.</em>}</div><button type="button" className="pos-change-person" onClick={() => { setPersona(undefined); setBuscar(""); cedulaRef.current?.focus(); }}>Cambiar</button></div>}</div>
        <div className="pos-controls"><div><span className="pos-label">¿Cuántos tiquetes compra?</span><div className="pos-stepper" aria-label="Cantidad de tiquetes"><button type="button" onClick={() => setCantidad((actual) => Math.max(1, actual - 1))} aria-label="Restar un tiquete"><Minus size={18} /></button><input aria-label="Cantidad de tiquetes" type="number" min="1" max="100" value={cantidad} onChange={(evento) => setCantidad(Math.min(100, Math.max(1, Number(evento.target.value) || 1)))} /><button type="button" onClick={() => setCantidad((actual) => Math.min(100, actual + 1))} aria-label="Sumar un tiquete"><Plus size={18} /></button></div><small className="pos-limit">Máximo 100 tiquetes por venta.</small></div><div className="pos-price"><span>Precio por tiquete</span><strong>{!persona ? "Seleccione una persona" : tarifa ? moneda.format(tarifa.montoColones) : "Sin tarifa vigente"}</strong><span>Total a cobrar</span><b>{tarifa ? moneda.format(total) : "—"}</b></div><Campo etiqueta="Medio de pago"><select value={medioPago} onChange={(evento) => setMedioPago(evento.target.value)} disabled={!persona}><option value="efectivo">Efectivo</option><option value="sinpe">SINPE</option><option value="otro">Otro</option></select></Campo></div>
      </div>
      <div className="pos-terminal__footer"><span><Ticket size={20} aria-hidden="true" /> {persona ? `${cantidad} tiquete${cantidad === 1 ? "" : "s"} · ${tarifa ? moneda.format(total) : "Sin tarifa"}` : "Seleccione una persona para continuar"}</span><div>{persona?.becado && <small className="pos-footer-warning">La compra no está disponible para beneficiarios.</small>}<button className="button primary" disabled={!persona || persona.becado || !tarifa || vender.isPending}>{vender.isPending ? <><CircleNotch className="pos-spin" size={18} /> Registrando venta…</> : <><CheckCircle size={18} aria-hidden="true" /> Confirmar venta</>}</button></div></div>
    </form>
  </section>;
}
