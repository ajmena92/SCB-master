import { QRCodeSVG } from "qrcode.react";

export function CodigoQrCarnet({ valor, tamano = 184 }: { valor?: string; tamano?: number }) {
  if (!valor?.startsWith("SCBQR1.")) {
    return <p className="py-5 text-center text-sm font-semibold text-muted-foreground">QR no disponible</p>;
  }
  return (
    <QRCodeSVG
      value={valor}
      size={tamano}
      level="M"
      marginSize={3}
      role="img"
      aria-label="Código QR del carnet"
      className="mx-auto h-auto max-w-full"
    />
  );
}
