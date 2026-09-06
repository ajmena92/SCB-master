import { useEffect, useMemo, useRef, useState, type FormEvent } from "react";
import {
  CashRegister,
  CheckCircle,
  CircleNotch,
  Ticket,
  WarningCircle,
} from "@phosphor-icons/react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { plataformaApi } from "../consultas/plataforma";
import { Aviso, EncabezadoPagina } from "../componentes/ElementosComunes";
import { errMsg } from "@/compartido/consultas/errores_api";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";
import {
  ComprobanteVentaTiquetes,
  type ComprobanteVenta,
} from "../componentes/ComprobanteVentaTiquetes";
import { ContenidoVentaTiquetes } from "../componentes/ContenidoVentaTiquetes";
import { monedaColones, type PersonaVenta } from "../componentes/venta_tiquetes";

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
  const [comprobante, setComprobante] = useState<ComprobanteVenta>();
  const tarifas = useQuery({ queryKey: ["tarifas"], queryFn: plataformaApi.tiquetes.tarifas });
  const resultados = useQuery({
    queryKey: ["tiquetes", "personas", buscarAplicado],
    queryFn: () => plataformaApi.tiquetes.buscarPersonas(buscarAplicado),
    enabled: buscarAplicado.length >= 3,
  });
  const vender = useMutation({
    mutationFn: plataformaApi.tiquetes.vender,
    onSuccess: (_, datos) => {
      setMensaje(
        `Venta exitosa: ${datos.cantidad} tiquete${datos.cantidad === 1 ? "" : "s"} vendido${datos.cantidad === 1 ? "" : "s"} por ${monedaColones.format(total)}.`,
      );
      if (persona)
        setComprobante({
          persona,
          cantidad: datos.cantidad,
          total,
          medioPago: datos.medioPago,
          saldoFinal: persona.saldoTiquetes + datos.cantidad,
        });
      setBuscar("");
      setBuscarAplicado("");
      setPersona(undefined);
      setCantidad(1);
      cliente.invalidateQueries({ queryKey: ["tiquetes", "personas"] });
      cedulaRef.current?.focus();
    },
  });

  useEffect(() => {
    const espera = window.setTimeout(() => setBuscarAplicado(buscar.trim()), 250);
    return () => window.clearTimeout(espera);
  }, [buscar]);
  useEffect(() => {
    const exacta = resultados.data?.find((item) => item.cedula === buscarAplicado);
    if (exacta) setPersona(exacta);
  }, [buscarAplicado, resultados.data]);
  useEffect(() => {
    let url: string | undefined;
    setFotoUrl(undefined);
    if (!persona) return;
    plataformaApi.tiquetes
      .fotoPersona(persona.id)
      .then((foto) => {
        url = URL.createObjectURL(foto);
        setFotoUrl(url);
      })
      .catch(() => undefined);
    return () => {
      if (url) URL.revokeObjectURL(url);
    };
  }, [persona]);

  const hoy = new Date().toISOString().slice(0, 10);
  const tarifa = useMemo(
    () =>
      tarifas.data?.elementos.find(
        (item) =>
          item.tipoPersona === persona?.tipo &&
          item.vigenteDesde <= hoy &&
          (!item.vigenteHasta || item.vigenteHasta >= hoy),
      ),
    [persona?.tipo, tarifas.data, hoy],
  );
  const sinTarifaParaPersona = Boolean(persona && !tarifa);
  const total = (tarifa?.montoColones ?? 0) * cantidad;
  const error = tarifas.error || resultados.error || vender.error;
  function venta(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (!persona?.cedula) return;
    setMensaje("");
    vender.mutate({ cedula: persona.cedula, cantidad, medioPago });
  }
  return (
    <section className="grid max-w-6xl gap-4">
      <ComprobanteVentaTiquetes
        comprobante={comprobante}
        alCerrar={() => setComprobante(undefined)}
      />
      <EncabezadoPagina
        titulo="Tiquetes y tarifas"
        descripcion="Busque a la persona, revise su saldo y confirme la venta de tiquetes."
      />
      {error && <Aviso tipo="error">{errMsg(error)}</Aviso>}
      {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      {!tarifas.isLoading && sinTarifaParaPersona && (
        <aside
          className="flex flex-wrap items-center gap-4 rounded-xl border border-warning/30 bg-warning/10 p-4"
          role="status"
        >
          <WarningCircle aria-hidden="true" size={28} weight="fill" />
          <div className="grid gap-0.5">
            <p className="text-xs uppercase tracking-wide">Antes de cobrar</p>
            <h2 className="font-heading text-base font-semibold">
              No hay una tarifa vigente para esta persona
            </h2>
            <span className="text-sm">
              Configure la tarifa correspondiente para habilitar la venta.
            </span>
          </div>
          {administrador && (
            <Link
              className="button secondary ml-auto whitespace-nowrap max-sm:ml-0 max-sm:w-full"
              to="/admin/panel/parametros"
            >
              Configurar tarifa
            </Link>
          )}
        </aside>
      )}
      <form
        className="overflow-hidden rounded-xl border border-border bg-card shadow-sm"
        onSubmit={venta}
      >
        <div className="flex flex-wrap items-center justify-between gap-3 bg-primary px-4 py-4 text-primary-foreground sm:px-6">
          <div className="flex items-center gap-3">
            <div className="grid size-11 place-items-center rounded-lg bg-primary-foreground text-primary">
              <CashRegister size={25} weight="duotone" />
            </div>
            <div>
              <p className="text-primary-foreground/80 text-xs uppercase tracking-wide">
                Punto de venta
              </p>
              <h2 className="font-heading text-lg font-semibold">Venta de tiquetes</h2>
            </div>
          </div>
          <span className="rounded-full border border-primary-foreground/40 px-2.5 py-1 text-xs">
            {resultados.isFetching
              ? "Buscando…"
              : persona?.becado
                ? "Compra no disponible"
                : persona
                  ? "Listo para cobrar"
                  : "Paso 1 de 3"}
          </span>
        </div>
        <ol
          className="flex flex-wrap gap-3 border-b border-border bg-muted/30 px-4 py-3 text-sm"
          aria-label="Pasos de la venta"
        >
          <li className="flex items-center gap-2 font-semibold text-primary">
            <span className="grid size-5 place-items-center rounded-full bg-primary text-xs text-primary-foreground">
              1
            </span>
            Buscar persona
          </li>
          <li
            className={
              persona
                ? "flex items-center gap-2 font-semibold text-primary"
                : "flex items-center gap-2 text-muted-foreground"
            }
          >
            <span className="grid size-5 place-items-center rounded-full border border-border text-xs">
              2
            </span>
            Cantidad y pago
          </li>
          <li
            className={
              persona && tarifa && !persona.becado
                ? "flex items-center gap-2 font-semibold text-primary"
                : "flex items-center gap-2 text-muted-foreground"
            }
          >
            <span className="grid size-5 place-items-center rounded-full border border-border text-xs">
              3
            </span>
            Confirmar
          </li>
        </ol>
        <ContenidoVentaTiquetes
          entradaRef={cedulaRef}
          buscar={buscar}
          busquedaAplicada={buscarAplicado}
          resultados={resultados.data}
          buscando={resultados.isFetching}
          persona={persona}
          fotoUrl={fotoUrl}
          tarifa={tarifa}
          cantidad={cantidad}
          medioPago={medioPago}
          alCambiarBuscar={(valor) => {
            setBuscar(valor);
            setPersona(undefined);
          }}
          alSeleccionarPersona={(item: PersonaVenta) => {
            setPersona(item);
            setBuscar(item.cedula ?? item.nombres);
          }}
          alLimpiarPersona={() => {
            setPersona(undefined);
            setBuscar("");
            cedulaRef.current?.focus();
          }}
          alCambiarCantidad={setCantidad}
          alCambiarMedioPago={setMedioPago}
        />
        <div className="flex flex-wrap items-center justify-between gap-3 border-t border-border bg-muted/30 px-4 py-4 sm:px-6">
          <span className="flex items-center gap-2 text-sm text-muted-foreground">
            <Ticket size={20} aria-hidden="true" />{" "}
            {persona
              ? `${cantidad} tiquete${cantidad === 1 ? "" : "s"} · ${tarifa ? monedaColones.format(total) : "Sin tarifa"}`
              : "Seleccione una persona para continuar"}
          </span>
          <div className="flex flex-wrap items-center gap-3 max-sm:w-full">
            {persona?.becado && (
              <small className="text-warning max-sm:w-full max-sm:text-center">
                La compra no está disponible para beneficiarios.
              </small>
            )}
            <button
              className="button primary max-sm:w-full"
              disabled={!persona || persona.becado || !tarifa || vender.isPending}
            >
              {vender.isPending ? (
                <>
                  <CircleNotch className="animate-spin" size={18} /> Registrando venta…
                </>
              ) : (
                <>
                  <CheckCircle size={18} aria-hidden="true" /> Confirmar venta
                </>
              )}
            </button>
          </div>
        </div>
      </form>
    </section>
  );
}
