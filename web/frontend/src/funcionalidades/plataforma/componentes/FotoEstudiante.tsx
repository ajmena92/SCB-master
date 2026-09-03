import { useEffect, useRef, useState, type ChangeEvent } from "react";
import { Camera, Trash } from "@phosphor-icons/react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { plataformaApi } from "../consultas/plataforma";

export default function FotoEstudiante({ personaId, nombre }: { personaId: number; nombre: string }) {
  const [url, setUrl] = useState<string>();
  const [procesando, setProcesando] = useState(false);
  const [mensaje, setMensaje] = useState("Foto pendiente");
  const [camaraAbierta, setCamaraAbierta] = useState(false);
  const [errorCamara, setErrorCamara] = useState("");
  const videoRef = useRef<HTMLVideoElement>(null);
  const flujoRef = useRef<MediaStream | undefined>(undefined);

  function detenerCamara() {
    flujoRef.current?.getTracks().forEach((pista) => pista.stop());
    flujoRef.current = undefined;
  }

  const cargar = async () => {
    try {
      const archivo = await plataformaApi.personas.foto.obtener(personaId);
      const nuevaUrl = URL.createObjectURL(archivo);
      setUrl((anterior) => {
        if (anterior) URL.revokeObjectURL(anterior);
        return nuevaUrl;
      });
      setMensaje("Fotografía cargada");
    } catch {
      setUrl((anterior) => {
        if (anterior) URL.revokeObjectURL(anterior);
        return undefined;
      });
      setMensaje("Foto pendiente");
    }
  };

  useEffect(() => {
    void cargar();
    return () => setUrl((anterior) => {
      if (anterior) URL.revokeObjectURL(anterior);
      return undefined;
    });
  }, [personaId]);

  useEffect(() => {
    if (!camaraAbierta) return undefined;
    let activa = true;
    setErrorCamara("");
    void (async () => {
      try {
        if (!navigator.mediaDevices?.getUserMedia) throw new Error("Cámara no disponible");
        const flujo = await navigator.mediaDevices.getUserMedia({
          audio: false,
          video: { facingMode: { ideal: "user" }, width: { ideal: 1280 }, height: { ideal: 960 } },
        });
        if (!activa) {
          flujo.getTracks().forEach((pista) => pista.stop());
          return;
        }
        flujoRef.current = flujo;
        if (videoRef.current) {
          videoRef.current.srcObject = flujo;
          await videoRef.current.play();
        }
      } catch (error) {
        if (!activa) return;
        setErrorCamara(
          error instanceof DOMException && error.name === "NotAllowedError"
            ? "Permití el uso de la cámara para tomar la fotografía."
            : "No fue posible abrir la cámara. Podés subir un archivo en su lugar.",
        );
      }
    })();
    return () => {
      activa = false;
      detenerCamara();
    };
  }, [camaraAbierta]);

  async function subir(archivo: File) {
    setProcesando(true);
    try {
      await plataformaApi.personas.foto.cargar(personaId, archivo);
      await cargar();
      setCamaraAbierta(false);
    } catch {
      setMensaje("No se pudo cargar la fotografía");
    } finally {
      setProcesando(false);
    }
  }

  async function seleccionar(evento: ChangeEvent<HTMLInputElement>) {
    const archivo = evento.target.files?.[0];
    if (!archivo) return;
    await subir(archivo);
    evento.target.value = "";
  }

  async function capturar() {
    const video = videoRef.current;
    if (!video?.videoWidth || !video.videoHeight) {
      setErrorCamara("Esperá un momento a que la cámara esté lista.");
      return;
    }
    const lienzo = document.createElement("canvas");
    lienzo.width = video.videoWidth;
    lienzo.height = video.videoHeight;
    lienzo.getContext("2d")?.drawImage(video, 0, 0, lienzo.width, lienzo.height);
    const imagen = await new Promise<Blob | null>((resolver) =>
      lienzo.toBlob(resolver, "image/jpeg", 0.9),
    );
    if (!imagen) {
      setErrorCamara("No se pudo capturar la fotografía. Intentá nuevamente.");
      return;
    }
    await subir(new File([imagen], "foto-carnet.jpg", { type: "image/jpeg" }));
  }

  async function eliminar() {
    setProcesando(true);
    try {
      await plataformaApi.personas.foto.eliminar(personaId);
      await cargar();
    } catch {
      setMensaje("No se pudo eliminar la fotografía");
    } finally {
      setProcesando(false);
    }
  }

  return <>
    <section className="student-photo" aria-label="Fotografía del estudiante">
      <div className="student-photo-preview">
        {url ? <img src={url} alt={`Fotografía de ${nombre}`} /> : <Camera aria-hidden="true" size={26} />}
      </div>
      <div className="student-photo-copy">
        <p>Fotografía</p>
        <span>{mensaje}. JPEG o PNG, máximo 5 MB.</span>
        <div className="student-photo-actions">
          <button className="button secondary" type="button" onClick={() => setCamaraAbierta(true)} disabled={procesando}>
            <Camera aria-hidden="true" size={17} /> {url ? "Cambiar foto" : "Tomar foto"}
          </button>
          <label className="button secondary" aria-disabled={procesando}>
            Subir archivo
            <input type="file" accept="image/jpeg,image/png" onChange={seleccionar} disabled={procesando} />
          </label>
          {url && <button className="button link" type="button" onClick={eliminar} disabled={procesando}><Trash aria-hidden="true" size={17} /> Eliminar</button>}
        </div>
      </div>
    </section>
    <Dialog open={camaraAbierta} onOpenChange={setCamaraAbierta}>
      <DialogContent className="max-w-lg overflow-hidden p-0">
        <DialogHeader className="px-5 pt-5 text-left">
          <DialogTitle>Tomar fotografía</DialogTitle>
          <DialogDescription>Centrá el rostro y tomá la foto para el carné.</DialogDescription>
        </DialogHeader>
        <div className="px-5 pb-5">
          {errorCamara ? <p className="rounded-xl bg-destructive/10 p-4 text-sm text-destructive">{errorCamara}</p> : <video ref={videoRef} className="aspect-[4/3] w-full rounded-xl bg-muted object-cover" autoPlay muted playsInline />}
          <div className="mt-4 flex justify-end gap-2">
            <button className="button secondary" type="button" onClick={() => setCamaraAbierta(false)} disabled={procesando}>Cancelar</button>
            <button className="button primary" type="button" onClick={() => void capturar()} disabled={procesando || Boolean(errorCamara)}><Camera aria-hidden="true" size={17} /> {procesando ? "Guardando…" : "Capturar y guardar"}</button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  </>;
}
