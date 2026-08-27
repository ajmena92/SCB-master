import { useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import type { LoteSalida, Previsualizacion } from "@/compartido/contratos/importaciones";
import {
  ejecutarImportacion,
  previsualizarImportacion,
} from "@/funcionalidades/administracion/consultas/importaciones";

export function useImportaciones() {
  const [archivo, setArchivo] = useState<File>();
  const [vista, setVista] = useState<Previsualizacion>();
  const [lote, setLote] = useState<LoteSalida>();
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  async function ejecutar(accion: () => Promise<void>) {
    if (!archivo) return;
    setCargando(true);
    setError("");
    try {
      await accion();
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setCargando(false);
    }
  }

  return {
    archivo,
    setArchivo,
    vista,
    lote,
    error,
    cargando,
    previsualizar: () => ejecutar(async () => setVista(await previsualizarImportacion(archivo!))),
    importar: () => ejecutar(async () => setLote(await ejecutarImportacion(archivo!))),
  };
}
