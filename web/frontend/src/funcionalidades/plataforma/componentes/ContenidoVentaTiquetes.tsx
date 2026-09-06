import { CircleNotch, Minus, Plus, Scan, UserCircle, WarningCircle } from "@phosphor-icons/react";
import type { RefObject } from "react";
import type { Tarifa } from "@/compartido/contratos/plataforma";
import { ImagenConFallback } from "@/compartido/componentes/ImagenConFallback";
import { Campo } from "./ElementosComunes";
import { monedaColones, type PersonaVenta } from "./venta_tiquetes";

export function ContenidoVentaTiquetes({
  entradaRef,
  buscar,
  busquedaAplicada,
  resultados,
  buscando,
  persona,
  fotoUrl,
  tarifa,
  cantidad,
  medioPago,
  alCambiarBuscar,
  alSeleccionarPersona,
  alLimpiarPersona,
  alCambiarCantidad,
  alCambiarMedioPago,
}: {
  entradaRef: RefObject<HTMLInputElement | null>;
  buscar: string;
  busquedaAplicada: string;
  resultados?: PersonaVenta[];
  buscando: boolean;
  persona?: PersonaVenta;
  fotoUrl?: string;
  tarifa?: Tarifa;
  cantidad: number;
  medioPago: string;
  alCambiarBuscar: (valor: string) => void;
  alSeleccionarPersona: (persona: PersonaVenta) => void;
  alLimpiarPersona: () => void;
  alCambiarCantidad: (cantidad: number) => void;
  alCambiarMedioPago: (medio: string) => void;
}) {
  const total = (tarifa?.montoColones ?? 0) * cantidad;
  return (
    <div className="grid gap-5 p-4 sm:grid-cols-[minmax(0,1.35fr)_minmax(16rem,.8fr)] sm:p-6">
      <div className="grid content-start gap-4">
        <label className="grid gap-2">
          <span className="flex items-center gap-2 text-sm font-semibold text-muted-foreground">
            <Scan size={20} aria-hidden="true" /> Cédula o nombre
          </span>
          <input
            ref={entradaRef}
            value={buscar}
            onChange={(evento) => alCambiarBuscar(evento.target.value)}
            placeholder="Digite cédula o nombre"
            autoFocus
          />
        </label>
        {buscando && (
          <span className="text-sm text-muted-foreground">
            <CircleNotch className="animate-spin" size={16} aria-hidden="true" /> Buscando personas…
          </span>
        )}
        {!persona && busquedaAplicada.length >= 3 && !buscando && resultados?.length === 0 && (
          <p className="m-0 rounded-lg border border-border bg-muted p-3 text-sm text-muted-foreground">
            No se encontró una persona con esos datos.
          </p>
        )}
        {!persona && resultados?.length ? (
          <div
            className="grid max-h-52 overflow-auto rounded-lg border border-border"
            aria-label="Resultados de búsqueda"
          >
            {resultados.map((item) => (
              <button
                className="grid gap-1 border-b border-border p-3 text-left last:border-0 hover:bg-muted"
                type="button"
                key={item.id}
                onClick={() => alSeleccionarPersona(item)}
              >
                <span className="font-semibold">{item.nombres}</span>
                <small className="text-muted-foreground">
                  {item.cedula} · {item.tipo}
                </small>
              </button>
            ))}
          </div>
        ) : null}
        {persona && (
          <div className="grid grid-cols-[3.75rem_minmax(0,1fr)_auto] items-center gap-3 rounded-lg border border-primary/20 bg-primary/5 p-3">
            <div className="grid size-14 place-items-center overflow-hidden rounded-lg bg-card text-primary">
              <ImagenConFallback
                src={fotoUrl}
                alt={`Fotografía de ${persona.nombres}`}
                className="size-full object-cover object-top"
                fallback={<UserCircle aria-hidden="true" size={54} />}
              />
            </div>
            <div className="grid min-w-0 gap-1">
              <span className="text-xs font-semibold uppercase tracking-wide text-primary">
                {persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}
              </span>
              <strong className="truncate">{persona.nombres}</strong>
              <span className="text-sm text-muted-foreground">{persona.cedula}</span>
              <span className="text-sm text-muted-foreground">
                Saldo actual <b className="text-foreground">{persona.saldoTiquetes} tiquetes</b>
              </span>
              {persona.becado && (
                <em className="flex items-center gap-1 text-sm not-italic text-warning">
                  <WarningCircle size={17} weight="fill" aria-hidden="true" /> Beneficiario de
                  comedor: no puede comprar tiquetes.
                </em>
              )}
            </div>
            <button
              type="button"
              className="self-start text-sm font-semibold text-primary hover:underline"
              onClick={alLimpiarPersona}
            >
              Cambiar
            </button>
          </div>
        )}
      </div>
      <div className="grid content-start gap-4">
        <div>
          <span className="text-sm font-semibold text-muted-foreground">
            ¿Cuántos tiquetes compra?
          </span>
          <div className="mt-2 flex w-fit overflow-hidden rounded-lg border border-border">
            <button
              className="grid size-11 place-items-center bg-muted text-primary hover:bg-primary/10"
              type="button"
              onClick={() => alCambiarCantidad(Math.max(1, cantidad - 1))}
              aria-label="Restar un tiquete"
            >
              <Minus size={18} />
            </button>
            <input
              className="!h-11 !w-14 rounded-none border-0 border-x border-border p-0 text-center font-semibold shadow-none focus:ring-0"
              aria-label="Cantidad de tiquetes"
              type="number"
              min="1"
              max="100"
              value={cantidad}
              onChange={(evento) =>
                alCambiarCantidad(Math.min(100, Math.max(1, Number(evento.target.value) || 1)))
              }
            />
            <button
              className="grid size-11 place-items-center bg-muted text-primary hover:bg-primary/10"
              type="button"
              onClick={() => alCambiarCantidad(Math.min(100, cantidad + 1))}
              aria-label="Sumar un tiquete"
            >
              <Plus size={18} />
            </button>
          </div>
          <small className="mt-1 block text-xs text-muted-foreground">
            Máximo 100 tiquetes por venta.
          </small>
        </div>
        <div className="grid gap-1 rounded-lg border border-border bg-muted/40 p-4">
          <span className="text-sm text-muted-foreground">Precio por tiquete</span>
          <strong>
            {!persona
              ? "Seleccione una persona"
              : tarifa
                ? monedaColones.format(tarifa.montoColones)
                : "Sin tarifa vigente"}
          </strong>
          <span className="mt-2 text-sm text-muted-foreground">Total a cobrar</span>
          <b className="text-2xl font-semibold text-primary">
            {tarifa ? monedaColones.format(total) : "—"}
          </b>
        </div>
        <Campo etiqueta="Medio de pago">
          <select
            value={medioPago}
            onChange={(evento) => alCambiarMedioPago(evento.target.value)}
            disabled={!persona}
          >
            <option value="efectivo">Efectivo</option>
            <option value="sinpe">SINPE</option>
            <option value="otro">Otro</option>
          </select>
        </Campo>
      </div>
    </div>
  );
}
