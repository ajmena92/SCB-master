import { QRCodeSVG } from "qrcode.react";

export function esCodigoQrCarnetValido(valor?: string): boolean {
  return Boolean(valor?.startsWith("SCBQR1."));
}

export function CodigoQrCarnet({ valor, tamano = 184 }: { valor?: string; tamano?: number }) {
  if (!valor || !esCodigoQrCarnetValido(valor)) {
    return (
      <p className="py-5 text-center text-sm font-semibold text-muted-foreground" role="status">
        QR no disponible
      </p>
    );
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
