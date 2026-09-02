import { useEffect, useState, type ChangeEvent } from "react";
import { Camera, Trash } from "@phosphor-icons/react";
import { plataformaApi } from "../consultas/plataforma";

export default function FotoEstudiante({ personaId, nombre }: { personaId: number; nombre: string }) {
  const [url, setUrl] = useState<string>();
  const [procesando, setProcesando] = useState(false);
  const [mensaje, setMensaje] = useState("Foto pendiente");

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

  async function seleccionar(evento: ChangeEvent<HTMLInputElement>) {
    const archivo = evento.target.files?.[0];
    if (!archivo) return;
    setProcesando(true);
    try {
      await plataformaApi.personas.foto.cargar(personaId, archivo);
      await cargar();
    } catch {
      setMensaje("No se pudo cargar la fotografía");
    } finally {
      setProcesando(false);
      evento.target.value = "";
    }
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

  return <section className="student-photo" aria-label="Fotografía del estudiante">
    <div className="student-photo-preview">
      {url ? <img src={url} alt={`Fotografía de ${nombre}`} /> : <Camera aria-hidden="true" size={26} />}
    </div>
    <div className="student-photo-copy">
      <p>Fotografía</p>
      <span>{mensaje}. JPEG o PNG, máximo 5 MB.</span>
      <div className="student-photo-actions">
        <label className="button secondary" aria-disabled={procesando}>
          <Camera aria-hidden="true" size={17} /> {url ? "Cambiar foto" : "Cargar foto"}
          <input type="file" accept="image/jpeg,image/png" onChange={seleccionar} disabled={procesando} />
        </label>
        {url && <button className="button link" type="button" onClick={eliminar} disabled={procesando}><Trash aria-hidden="true" size={17} /> Eliminar</button>}
      </div>
    </div>
  </section>;
}
