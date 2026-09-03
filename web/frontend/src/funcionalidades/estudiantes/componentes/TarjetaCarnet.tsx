import { useEffect, useState } from "react";
import { Expand, IdCard, ScanLine } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { api } from "@/compartido/consultas/cliente_http";
import { CodigoQrCarnet } from "./CodigoQrCarnet";
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
    <div
      className="mx-auto w-full max-w-[23rem] overflow-hidden rounded-[1.75rem] border border-white/80 bg-white shadow-[0_20px_55px_rgb(64_68_170_/_0.2)]"
      data-testid="html-student-card"
    >
      <div
        className="relative overflow-hidden px-6 pb-7 pt-7"
        style={{ backgroundColor: colorRuta, color: obtenerColorTextoRuta(colorRuta) }}
      >
        <div className="absolute -right-16 -top-20 h-48 w-48 rounded-full border-[22px] border-current opacity-15" />
        <div className="relative flex items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <img
              src={LOGO_COLEGIO}
              alt="Escudo del CTP Platanares"
              className="h-12 w-12 rounded-full bg-white/90 object-contain p-1"
            />
            <div>
              <p className="text-[0.6rem] font-black uppercase tracking-[0.14em] opacity-80">
                {NOMBRE_COLEGIO}
              </p>
              <h3 className="mt-1 font-display text-xl font-black tracking-tight">Mi carnet</h3>
            </div>
          </div>
          <IdCard className="h-8 w-8 shrink-0 opacity-90" aria-hidden="true" />
        </div>
        <div className="relative mt-6 flex items-end gap-4">
          <div className="h-28 w-24 shrink-0 overflow-hidden rounded-2xl border-4 border-white/70 bg-white/25 shadow-lg">
            {fotoDisponible && fotoUrl ? (
              <img
                src={fotoUrl}
                alt={`Fotografía de ${nombre}`}
                className="h-full w-full object-cover object-top"
              />
            ) : (
              <div className="flex h-full items-center justify-center text-center text-[0.6rem] font-black uppercase leading-tight opacity-80">
                Foto
                <br />
                pendiente
              </div>
            )}
          </div>
          <div className="min-w-0 pb-1">
            <p className="text-[0.62rem] font-black uppercase tracking-[0.18em] opacity-70">
              {tipoPersona === "profesor" ? "Profesor" : "Estudiante"}
            </p>
            <p className="mt-1 line-clamp-3 font-display text-lg font-black leading-tight">
              {nombre || "Sin nombre"}
            </p>
          </div>
        </div>
      </div>
      <div className="space-y-5 p-6">
        <div className="grid grid-cols-2 gap-4 text-sm">
          {tipoPersona === "profesor" ? (
            <div className="col-span-2">
              <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
                Colegio
              </p>
              <p className="mt-1 font-bold">{datosCarnet.colegio || NOMBRE_COLEGIO}</p>
            </div>
          ) : (
            <div>
              <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
                Año
              </p>
              <p className="mt-1 font-bold">{obtenerAnioCarnet(datosCarnet)}</p>
            </div>
          )}
          {tipoPersona === "estudiante" && (
            <div>
              <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
                Sección
              </p>
              <p className="mt-1 font-bold">{datosCarnet.seccion || "Sin sección"}</p>
            </div>
          )}
          {tipoPersona === "estudiante" && (
            <div>
              <p className="text-[0.62rem] font-black uppercase tracking-wider text-muted-foreground">
                Ruta asignada
              </p>
              <p className="mt-1 font-bold">{datosCarnet.rutaDescripcion || "Sin ruta"}</p>
            </div>
          )}
        </div>
        <button
          type="button"
          onClick={() => setQrAbierto(true)}
          className="group relative block w-full overflow-hidden rounded-[1.5rem] border border-primary/15 bg-primary/5 p-4 text-secondary transition-[transform,background-color,border-color] duration-200 hover:-translate-y-0.5 hover:border-primary/35 hover:bg-primary/10 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2"
          aria-label="Ampliar QR del carnet"
          aria-haspopup="dialog"
          data-testid="student-card-qr"
        >
          <span className="absolute right-3 top-3 grid h-10 w-10 place-items-center rounded-full bg-background/90 text-primary shadow-sm transition-transform duration-200 group-hover:scale-105" aria-hidden="true">
            <Expand className="h-4 w-4" />
          </span>
          <span className="mb-3 flex items-center gap-2 text-left text-[0.65rem] font-black uppercase tracking-[0.16em] text-primary">
            <ScanLine className="h-4 w-4" aria-hidden="true" /> Listo para escanear
          </span>
          <span className="block rounded-xl bg-background p-3 shadow-sm">
            <CodigoQrCarnet valor={datosCarnet.codigoQr} />
          </span>
          <span className="mt-3 block text-center text-xs font-semibold text-muted-foreground">
            Tocá para ampliar
          </span>
        </button>
        {tipoPersona === "estudiante" && datosCarnet.beneficioComedor && (
          <Badge variant="secondary">{datosCarnet.beneficioComedor}</Badge>
        )}
        <p className="text-center text-xs font-semibold text-muted-foreground">
          Presentá este QR ante el lector del comedor.
        </p>
      </div>
      <Dialog open={qrAbierto} onOpenChange={setQrAbierto}>
        <DialogContent className="max-w-md p-5 sm:p-7">
          <DialogHeader>
            <DialogTitle className="font-display text-xl font-black">QR del carnet</DialogTitle>
            <DialogDescription>
              Presentalo completo y con buen brillo ante el lector del comedor.
            </DialogDescription>
          </DialogHeader>
          <div className="rounded-[1.75rem] border border-primary/15 bg-primary/5 p-4 text-secondary sm:p-6">
            <div className="rounded-2xl bg-background p-3 shadow-sm sm:p-5">
              <CodigoQrCarnet valor={datosCarnet.codigoQr} tamano={320} />
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
