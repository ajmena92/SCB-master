import { useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import type { CuentaTiquetesSalida } from "@/compartido/contratos/comedor";
import {
  comprarTiquetes,
  consultarSaldoTiquetes,
} from "@/funcionalidades/comedor/consultas/tiquetes";

export function useTiquetes() {
  const [idPersona, setIdPersona] = useState("");
  const [cantidad, setCantidad] = useState("");
  const [saldo, setSaldo] = useState<CuentaTiquetesSalida>();
  const [movimiento, setMovimiento] = useState<Record<string, unknown>>();
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  async function consultar() {
    if (!idPersona) return;
    setCargando(true);
    setError("");
    try {
      setSaldo(await consultarSaldoTiquetes(Number(idPersona)));
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setCargando(false);
    }
  }

  async function comprar() {
    if (!idPersona || !cantidad) return;
    const cantidadNumerica = Number(cantidad);
    if (!Number.isInteger(cantidadNumerica) || cantidadNumerica < 1) {
      setError("La cantidad debe ser un número entero mayor que cero");
      return;
    }
    setCargando(true);
    setError("");
    try {
      setMovimiento(await comprarTiquetes(Number(idPersona), cantidadNumerica));
      setCantidad("");
      await consultar();
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setCargando(false);
    }
  }

  return {
    idPersona,
    cantidad,
    saldo,
    movimiento,
    error,
    cargando,
    setIdPersona,
    setCantidad,
    consultar,
    comprar,
  };
}
