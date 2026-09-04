import { Printer, Ticket } from "@phosphor-icons/react";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { monedaColones, type PersonaVenta } from "./venta_tiquetes";

export type ComprobanteVenta = {
  persona: PersonaVenta;
  cantidad: number;
  total: number;
  medioPago: string;
  saldoFinal: number;
};

const escaparHtml = (valor: string) =>
  valor.replace(
    /[&<>"']/g,
    (caracter) =>
      ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" })[caracter] ??
      caracter,
  );

function imprimirComprobante(datos: ComprobanteVenta) {
  const ventana = window.open("", "_blank");
  if (!ventana) return;
  const fecha = new Intl.DateTimeFormat("es-CR", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date());
  ventana.document.write(
    `<!doctype html><html lang="es"><head><title>Comprobante de venta</title><style>body{font-family:Karla,system-ui,sans-serif;color:rgb(24 32 82);margin:28px;max-width:680px}header{padding-bottom:14px;border-bottom:2px solid rgb(116 123 255)}h1,p{margin:0}h1{font-family:Chivo,system-ui,sans-serif;font-size:22px;font-weight:700}p{margin-top:4px;color:rgb(107 114 168)}dl{display:grid;grid-template-columns:1fr auto;gap:10px;margin:24px 0}dt{color:rgb(107 114 168)}dd{margin:0;font-weight:600;text-align:right}.total{padding:16px;background:rgb(238 240 255);border:1px solid rgb(221 225 255);border-radius:8px;font-size:20px}.total dd{color:rgb(116 123 255);font-size:24px}footer{margin-top:24px;padding-top:12px;border-top:1px solid rgb(221 225 255);font-size:12px}@media print{body{margin:14mm}}</style></head><body><header><p>CTP Platanares · Comedor</p><h1>Comprobante de venta de tiquetes</h1><p>${escaparHtml(fecha)}</p></header><dl><dt>Persona</dt><dd>${escaparHtml(datos.persona.nombres)}</dd><dt>Cédula</dt><dd>${escaparHtml(datos.persona.cedula ?? "")}</dd><dt>Tiquetes vendidos</dt><dd>${datos.cantidad}</dd><dt>Total de tiquetes</dt><dd>${datos.saldoFinal}</dd><dt>Medio de pago</dt><dd>${escaparHtml(datos.medioPago)}</dd><div class="total"><dt>Total cobrado</dt><dd>${monedaColones.format(datos.total)}</dd></div></dl><footer>Venta registrada correctamente. Conserve este comprobante para control administrativo.</footer></body></html>`,
  );
  ventana.document.close();
  window.setTimeout(() => ventana.print(), 100);
}

export function ComprobanteVentaTiquetes({
  comprobante,
  alCerrar,
}: {
  comprobante?: ComprobanteVenta;
  alCerrar: () => void;
}) {
  return (
    <AlertDialog open={Boolean(comprobante)} onOpenChange={(abierto) => !abierto && alCerrar()}>
      <AlertDialogContent className="max-h-[calc(100dvh-1rem)] max-w-md overflow-y-auto">
        <AlertDialogHeader>
          <AlertDialogTitle>Venta registrada correctamente</AlertDialogTitle>
          <AlertDialogDescription>
            El saldo de tiquetes se actualizó. Entregue o guarde el comprobante antes de la
            siguiente venta.
          </AlertDialogDescription>
        </AlertDialogHeader>
        {comprobante && (
          <article
            className="overflow-hidden rounded-xl border border-dashed border-primary/40 bg-background"
            aria-label="Detalle de la venta"
          >
            <header className="flex items-center gap-3 bg-primary px-4 py-3 text-primary-foreground">
              <Ticket aria-hidden="true" size={24} />
              <div>
                <span className="block text-xs tracking-wider">COMPROBANTE DE VENTA</span>
                <strong className="font-heading text-sm">CTP Platanares · Comedor</strong>
              </div>
            </header>
            <div className="grid gap-1 border-b border-dashed border-border p-4">
              <span className="text-xs text-muted-foreground">Persona</span>
              <strong>{comprobante.persona.nombres}</strong>
              <small className="text-muted-foreground">{comprobante.persona.cedula}</small>
            </div>
            <dl className="grid gap-2 p-4">
              {[
                ["Tiquetes vendidos", comprobante.cantidad],
                ["Saldo final", `${comprobante.saldoFinal} tiquetes`],
                ["Medio de pago", comprobante.medioPago],
              ].map(([etiqueta, valor]) => (
                <div className="flex justify-between gap-4" key={String(etiqueta)}>
                  <dt className="text-muted-foreground">{etiqueta}</dt>
                  <dd className="m-0 font-semibold capitalize">{valor}</dd>
                </div>
              ))}
            </dl>
            <footer className="flex items-baseline justify-between gap-4 border-t border-dashed border-border bg-muted p-4 text-primary">
              <span>Total cobrado</span>
              <strong className="text-xl">{monedaColones.format(comprobante.total)}</strong>
            </footer>
          </article>
        )}
        <AlertDialogFooter className="flex-col gap-2 sm:flex-row sm:justify-end">
          <button className="button secondary w-full sm:w-auto" type="button" onClick={alCerrar}>
            Nueva venta
          </button>
          <button
            className="button primary w-full sm:w-auto"
            type="button"
            onClick={() => comprobante && imprimirComprobante(comprobante)}
          >
            <Printer aria-hidden="true" size={18} /> Imprimir / guardar PDF
          </button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
