import { useEffect, useState } from "react";
import { AlertCircle, Expand, IdCard, Printer, ScanLine, Utensils } from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { api } from "@/compartido/consultas/cliente_http";
import { ImagenConFallback } from "@/compartido/componentes/ImagenConFallback";
import { CodigoQrCarnet, esCodigoQrCarnetValido } from "./CodigoQrCarnet";
import {
  LOGO_COLEGIO,
  NOMBRE_COLEGIO,
  obtenerAnioCarnet,
  obtenerColorRutaSeguro,
  obtenerColorTextoRuta,
  obtenerNombreCompleto,
} from "./accionesCarnet";
import type { DatosCarnet } from "./accionesCarnet";

export function TarjetaCarnet({
  datosCarnet = {},
  tipoPersona = datosCarnet.tipoPersona ?? "estudiante",
  tieneFoto,
}: {
  datosCarnet?: DatosCarnet;
  tieneFoto?: boolean;
  versionFoto?: string | number;
  tipoPersona?: "estudiante" | "profesor";
}) {
  const [fotoUrl, setFotoUrl] = useState<string>();
  const [qrAbierto, setQrAbierto] = useState(false);
  const colorRuta = obtenerColorRutaSeguro(datosCarnet.rutaColor);
  const nombre = obtenerNombreCompleto(datosCarnet);
  const fotoDisponible = tieneFoto ?? Boolean(datosCarnet.tieneFoto);
  const qrDisponible = esCodigoQrCarnetValido(datosCarnet.codigoQr);
  const anioLectivo = obtenerAnioCarnet(datosCarnet);
  const fechaImpresion = new Intl.DateTimeFormat("es-CR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date());

  function imprimirCarnet() {
    // Abrimos la ventana como parte directa del clic. Usar `noopener` en
    // `window.open` puede devolver `null` en algunos navegadores y dejaba el
    // botón sin acción visible.
    const ventana = window.open("", "_blank", "width=480,height=760");
    if (!ventana) return;
    const contenido = document.querySelector('[data-testid="html-student-card"]')?.outerHTML ?? "";
    const estilosAplicacion = Array.from(document.querySelectorAll('style, link[rel="stylesheet"]'))
      .map((estilo) => estilo.outerHTML)
      .join("");
    ventana.document.title = `Carné ${nombre || "estudiante"}`;
    ventana.document.head.innerHTML = `${estilosAplicacion}<style>
      *{box-sizing:border-box}body{margin:0;padding:24px;background:#fff;font-family:Arial,sans-serif;color:#0f172a}
      @page{size:letter portrait;margin:0}
      body{width:8.5in;height:11in;display:flex;align-items:center;justify-content:center;-webkit-print-color-adjust:exact;print-color-adjust:exact}
      [data-testid="html-student-card"]{display:grid!important;grid-template-columns:minmax(0,.9fr) minmax(0,1.1fr)!important;width:768px!important;max-width:none!important;margin:0 auto;overflow:hidden!important;border:1px solid #d9ddff!important;border-radius:28px!important;background:#fff!important;box-shadow:none!important}
      [data-testid="html-student-card"]>div:last-of-type{display:flex!important;flex-direction:column!important;gap:12px!important;padding:24px!important}
      [data-testid="html-student-card"] [data-testid="student-card-qr"] svg{width:220px!important;height:220px!important}
      button{display:none!important}[data-testid="student-card-qr"]{display:block!important}[data-print-hide]{display:none!important}[data-print-only]{display:block!important}@media print{body{padding:0;width:8.5in;height:11in}body,body *{visibility:visible!important}}
    </style>`;
    ventana.document.body.innerHTML = contenido;

    const esperarRecursos = async () => {
      const imagenes = Array.from(ventana.document.images);
      await Promise.all(
        imagenes.map(
          (imagen) =>
            imagen.complete
              ? Promise.resolve()
              : new Promise<void>((resolver) => {
                  imagen.addEventListener("load", () => resolver(), { once: true });
                  imagen.addEventListener("error", () => resolver(), { once: true });
                }),
        ),
      );
      if (ventana.document.fonts?.ready) await ventana.document.fonts.ready;
      await new Promise<void>((resolver) => ventana.setTimeout(resolver, 250));
      ventana.focus();
      ventana.print();
    };
    void esperarRecursos();
  }

  useEffect(() => {
    let url: string | undefined;
    if (!fotoDisponible) {
      setFotoUrl(undefined);
      return undefined;
    }
    void api
      .get("/v1/portal/carnet/foto", {
        responseType: "blob",
        omitirManejoFalloAutenticacion: true,
      })
      .then(({ data }) => {
        url = URL.createObjectURL(data as Blob);
        setFotoUrl(url);
      })
      .catch(() => setFotoUrl(undefined));
    return () => {
      if (url) URL.revokeObjectURL(url);
    };
  }, [fotoDisponible]);

  return (
    <>
    <div
      className="mx-auto grid w-full max-w-[24rem] overflow-hidden rounded-[1.75rem] border border-border bg-white text-carnet-foreground shadow-[0_20px_55px_rgb(64_68_170_/_0.2)] lg:max-w-3xl lg:grid-cols-[minmax(0,0.9fr)_minmax(0,1.1fr)]"
      data-testid="html-student-card"
    >
      <div
        className="relative flex flex-col overflow-hidden px-6 pb-7 pt-7 lg:justify-start lg:px-7 lg:py-8"
        style={{ backgroundColor: colorRuta, color: obtenerColorTextoRuta(colorRuta) }}
      >
        <div className="absolute -right-16 -top-20 h-48 w-48 rounded-full border-[22px] border-current opacity-15" />
        <div className="relative flex items-center justify-between gap-3">
          <div className="flex min-w-0 items-center gap-3 lg:gap-4">
            <img
              src={LOGO_COLEGIO}
              alt="Escudo del CTP Platanares"
              className="h-12 w-12 rounded-full bg-white/90 object-contain p-1 lg:h-16 lg:w-16 lg:p-1.5"
            />
            <div className="min-w-0">
              <p className="break-words text-xs font-bold uppercase leading-snug tracking-[0.14em] lg:text-sm lg:tracking-[0.16em]">
                {NOMBRE_COLEGIO}
              </p>
              <h3 className="mt-1 break-words font-display text-xl font-bold tracking-tight">Credencial digital</h3>
            </div>
          </div>
          <IdCard className="h-8 w-8 shrink-0" aria-hidden="true" />
        </div>
        <div className="relative mt-6 flex items-end gap-4 lg:mt-8 lg:flex-col lg:items-center lg:gap-4">
          <div className="flex shrink-0 flex-col items-center gap-1">
            <div className="h-28 w-24 overflow-hidden rounded-2xl border-4 border-white/70 bg-white/25 shadow-lg lg:h-52 lg:w-44 lg:rounded-3xl lg:shadow-xl">
              <ImagenConFallback
                src={fotoDisponible ? fotoUrl : undefined}
                alt={`Fotografía de ${nombre}`}
                className="h-full w-full object-cover object-top"
                fallback={
                  <div className="flex h-full items-center justify-center px-2 text-center text-xs font-bold uppercase leading-tight">
                    Sin fotografía
                  </div>
                }
              />
            </div>
            <p
              className="hidden text-center text-[0.58rem] font-semibold leading-tight opacity-80"
              data-print-only
            >
              Impreso: {fechaImpresion} · Año lectivo: {anioLectivo}
            </p>
          </div>
          <div className="min-w-0 pb-1 lg:text-center">
            <p className="text-xs font-bold uppercase tracking-[0.18em]">
              {tipoPersona === "profesor" ? "Profesor" : "Estudiante"}
            </p>
            <p className="mt-1 line-clamp-3 break-words font-display text-base font-bold leading-tight sm:text-lg lg:line-clamp-2 lg:text-xl">
              {nombre || "Sin nombre"}
            </p>
          </div>
        </div>
      </div>
      <div className="space-y-5 border-t border-border bg-card p-6 lg:flex lg:flex-col lg:gap-3 lg:space-y-0 lg:border-l lg:border-t-0 lg:p-6">
        <div className="order-1 space-y-4 text-sm lg:order-1">
          <div className="grid grid-cols-2 gap-x-4 gap-y-5">
          {tipoPersona === "profesor" ? (
            <div className="col-span-2">
              <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Colegio
              </p>
              <p className="mt-1 font-bold">{datosCarnet.colegio || NOMBRE_COLEGIO}</p>
            </div>
          ) : (
            <div>
              <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Año
              </p>
              <p className="mt-1 font-bold">{obtenerAnioCarnet(datosCarnet)}</p>
            </div>
          )}
          {tipoPersona === "estudiante" && (
            <div className="order-3 col-span-1 lg:col-span-1">
              <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Sección
              </p>
              <p className="mt-1 font-bold">{datosCarnet.seccion || "Sin sección"}</p>
            </div>
          )}
          {tipoPersona === "estudiante" && (
            <div className="order-2 col-span-1">
              <p className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
                Ruta asignada
              </p>
              <p className="mt-1 flex items-center gap-2 font-semibold">
                <span
                  className="h-2.5 w-2.5 shrink-0 rounded-full border border-foreground/20"
                  style={{ backgroundColor: colorRuta }}
                  aria-hidden="true"
                />
                <span className="min-w-0 break-words">{datosCarnet.rutaDescripcion || "Sin ruta"}</span>
              </p>
            </div>
          )}
          {tipoPersona === "estudiante" && datosCarnet.beneficioComedor && (
            <div className="order-4 col-span-1 lg:col-span-1">
              <p className="text-[0.58rem] font-bold uppercase tracking-wider text-muted-foreground sm:text-[0.6rem]">
                Comedor
              </p>
              <p className="mt-1 flex items-center gap-2 font-semibold">
                <Utensils className="h-4 w-4 shrink-0 text-primary" aria-hidden="true" />
                <span className="min-w-0 break-words">{datosCarnet.beneficioComedor}</span>
              </p>
            </div>
          )}
          </div>
        </div>
        <div className="order-2 space-y-3 lg:order-2">
          {qrDisponible ? (
            <button
              type="button"
              onClick={() => setQrAbierto(true)}
              className="group relative block w-full overflow-hidden rounded-[1.5rem] border border-primary/15 bg-primary/5 p-3 text-carnet-foreground transition-[transform,background-color,border-color] duration-200 hover:-translate-y-0.5 hover:border-primary/35 hover:bg-primary/10 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
              aria-label="Ampliar QR del carnet"
              aria-haspopup="dialog"
              data-testid="student-card-qr"
            >
              <span
                className="absolute right-3 top-3 grid h-10 w-10 place-items-center rounded-full bg-background/90 text-primary shadow-sm transition-transform duration-200 group-hover:scale-105"
                aria-hidden="true"
                data-print-hide
              >
                <Expand className="h-4 w-4" />
              </span>
              <span className="mb-3 flex items-center gap-2 text-left text-sm font-bold uppercase tracking-[0.16em] text-primary">
                <ScanLine className="h-4 w-4" aria-hidden="true" data-print-hide /> Listo para escanear
              </span>
              <span className="block rounded-xl bg-background p-3 shadow-sm lg:[&_svg]:h-[240px] lg:[&_svg]:w-[240px]">
                <CodigoQrCarnet valor={datosCarnet.codigoQr} />
              </span>
              <span className="mt-3 block text-center text-sm font-semibold text-muted-foreground">
                Tocá para ampliar · Presentalo ante el lector
              </span>
            </button>
          ) : (
            <div
              className="flex items-center gap-3 rounded-[1.5rem] border border-warning/35 bg-warning/10 p-4 text-sm text-foreground"
              role="status"
              data-testid="student-card-qr-unavailable"
            >
              <AlertCircle className="h-5 w-5 shrink-0 text-warning" aria-hidden="true" />
              <div>
                <p className="font-bold">QR no disponible</p>
                <p className="mt-1 text-sm text-muted-foreground">El carné podrá ampliarse cuando se genere el código.</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
    <div className="mt-3 flex justify-center">
      <button type="button" className="button secondary w-full max-w-xs" onClick={imprimirCarnet}>
        <Printer className="mr-2 h-4 w-4" aria-hidden="true" /> Imprimir carné
      </button>
    </div>
      <Dialog open={qrAbierto && qrDisponible} onOpenChange={setQrAbierto}>
        <DialogContent className="max-w-md p-5 sm:p-7">
          <DialogHeader>
            <DialogTitle className="font-display text-xl font-bold">QR del carnet</DialogTitle>
            <DialogDescription>
              Presentalo completo y con buen brillo ante el lector del comedor.
            </DialogDescription>
          </DialogHeader>
          <div className="rounded-[1.75rem] border border-primary/15 bg-primary/5 p-4 text-foreground sm:p-6">
            <div className="rounded-2xl bg-background p-3 shadow-sm sm:p-5">
              <CodigoQrCarnet valor={datosCarnet.codigoQr} tamano={320} />
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
