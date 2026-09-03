import { BrowserQRCodeReader } from "@zxing/browser";
import { Camera, LoaderCircle, RefreshCw, ScanLine, SwitchCamera, TriangleAlert } from "lucide-react";
import { useEffect, useRef, useState } from "react";

type DetectorQr = { detect: (fuente: ImageBitmapSource) => Promise<Array<{ rawValue?: string }>> };
type ConstructorDetectorQr = new (opciones: { formats: string[] }) => DetectorQr;

function detectorNativo(): ConstructorDetectorQr | undefined {
  return (globalThis as typeof globalThis & { BarcodeDetector?: ConstructorDetectorQr }).BarcodeDetector;
}

const restriccionesVideo: MediaTrackConstraints = {
  facingMode: { ideal: "environment" },
  width: { ideal: 1280 },
  height: { ideal: 720 },
  frameRate: { ideal: 30, max: 30 },
};

function restriccionesParaCamara(camaraId?: string): MediaStreamConstraints {
  return {
    audio: false,
    video: { ...restriccionesVideo, ...(camaraId ? { deviceId: { exact: camaraId } } : {}) },
  };
}

export function LectorQrCamara({
  alDetectar,
  pausado,
  alCambiarEstado,
}: {
  alDetectar: (codigo: string) => void;
  pausado?: boolean;
  alCambiarEstado?: (estado: "iniciando" | "activo" | "error") => void;
}) {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [estado, setEstado] = useState<"iniciando" | "activo" | "error">("iniciando");
  const [mensajeError, setMensajeError] = useState("");
  const [intento, setIntento] = useState(0);
  const [camaras, setCamaras] = useState<MediaDeviceInfo[]>([]);
  const [camaraId, setCamaraId] = useState<string>();

  function cambiarEstado(siguiente: "iniciando" | "activo" | "error") {
    setEstado(siguiente);
    alCambiarEstado?.(siguiente);
  }

  useEffect(() => {
    let activo = true;
    let detener: (() => void) | undefined;
    const video = videoRef.current;
    if (!video) return undefined;

    async function iniciarConDetectorNativo(Detector: ConstructorDetectorQr) {
      const flujo = await navigator.mediaDevices.getUserMedia(restriccionesParaCamara(camaraId));
      video.srcObject = flujo;
      await video.play();
      const detector = new Detector({ formats: ["qr_code"] });
      let temporizador: number | undefined;
      const leer = async () => {
        if (!activo) return;
        try {
          const [resultado] = await detector.detect(video);
          if (resultado?.rawValue) alDetectar(resultado.rawValue);
        } catch {
          // Un cuadro borroso no es una falla de la cámara; se intenta en el siguiente ciclo.
        }
        temporizador = window.setTimeout(leer, 80);
      };
      detener = () => {
        if (temporizador !== undefined) window.clearTimeout(temporizador);
        flujo.getTracks().forEach((pista) => pista.stop());
      };
      void leer();
    }

    async function iniciarConZxing() {
      const lector = new BrowserQRCodeReader(undefined, {
        delayBetweenScanAttempts: 80,
        delayBetweenScanSuccess: 250,
      });
      const controles = await lector.decodeFromConstraints(restriccionesParaCamara(camaraId), video, (resultado) => {
        if (resultado) alDetectar(resultado.getText());
      });
      detener = () => controles.stop();
    }

    void (async () => {
      try {
        cambiarEstado("iniciando");
        if (!navigator.mediaDevices?.getUserMedia) {
          throw new Error("Este navegador no permite usar la cámara.");
        }
        const Detector = detectorNativo();
        if (Detector) await iniciarConDetectorNativo(Detector);
        else await iniciarConZxing();
        const dispositivos = await navigator.mediaDevices.enumerateDevices();
        if (activo) setCamaras(dispositivos.filter((dispositivo) => dispositivo.kind === "videoinput"));
        if (activo) cambiarEstado("activo");
      } catch (error) {
        if (!activo) return;
        cambiarEstado("error");
        const nombre = error instanceof DOMException ? error.name : "";
        setMensajeError(
          nombre === "NotAllowedError"
            ? "Permití el uso de la cámara para leer los carnets."
            : "No fue posible iniciar la cámara. Podés usar el lector USB como respaldo.",
        );
      }
    })();

    return () => {
      activo = false;
      detener?.();
    };
  }, [alDetectar, camaraId, intento]);

  function usarOtraCamara() {
    const indiceActual = camaras.findIndex((camara) => camara.deviceId === camaraId);
    const siguiente = camaras[(indiceActual + 1) % camaras.length];
    if (siguiente) setCamaraId(siguiente.deviceId);
  }

  return (
    <section className="relative isolate overflow-hidden rounded-[2rem] border bg-slate-950 shadow-[0_24px_70px_rgb(15_23_42_/_0.28)]" aria-label="Lector QR por cámara">
      <video ref={videoRef} className="aspect-[4/3] w-full object-cover" muted playsInline />
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_center,transparent_43%,rgb(2_6_23_/_0.68)_100%)]" />
      <div className="pointer-events-none absolute inset-y-[16%] left-[22%] right-[22%] rounded-[1.5rem] border-2 border-emerald-300/90 shadow-[0_0_0_999px_rgb(2_6_23_/_0.18)]" />
      <div className="absolute inset-x-0 bottom-0 flex items-center justify-between gap-3 bg-slate-950/80 px-5 py-4 text-sm text-white backdrop-blur-sm">
        <span className="flex items-center gap-2 font-semibold">
          {estado === "iniciando" ? <LoaderCircle className="h-4 w-4 animate-spin text-emerald-300" /> : estado === "error" ? <TriangleAlert className="h-4 w-4 text-amber-300" /> : <ScanLine className="h-4 w-4 text-emerald-300" />}
          {estado === "iniciando" ? "Activando cámara…" : estado === "error" ? mensajeError : pausado ? "Validando lectura…" : "Apuntá el QR dentro del recuadro"}
        </span>
        <Camera className="h-5 w-5 shrink-0 text-emerald-300" aria-hidden="true" />
      </div>
      {estado === "error" && (
        <div className="absolute inset-x-4 top-4 flex flex-wrap justify-end gap-2">
          <button
            type="button"
            onClick={() => setIntento((valor) => valor + 1)}
            className="inline-flex min-h-11 items-center gap-2 rounded-xl bg-white px-4 text-sm font-bold text-slate-950 shadow-lg"
          >
            <RefreshCw className="h-4 w-4" /> Reintentar
          </button>
          {camaras.length > 1 && (
            <button
              type="button"
              onClick={usarOtraCamara}
              className="inline-flex min-h-11 items-center gap-2 rounded-xl border border-white/30 bg-slate-950/90 px-4 text-sm font-bold text-white"
            >
              <SwitchCamera className="h-4 w-4" /> Otra cámara
            </button>
          )}
        </div>
      )}
    </section>
  );
}
