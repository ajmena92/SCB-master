import { useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import type {
  MovimientoEntrada,
  MovimientoSalida,
  SaldoSalida,
} from "@/compartido/contratos/cuentas";
import {
  consultarSaldo,
  registrarMovimiento,
} from "@/funcionalidades/administracion/consultas/cuentas";

export function useCuentas() {
  const [id, setId] = useState("");
  const [saldo, setSaldo] = useState<SaldoSalida>();
  const [movimiento, setMovimiento] = useState<MovimientoSalida>();
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);

  async function consultar() {
    setCargando(true);
    setError("");
    try {
      setSaldo(await consultarSaldo(Number(id)));
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setCargando(false);
    }
  }

  async function registrar(datos: Omit<MovimientoEntrada, "claveIdempotencia">) {
    setCargando(true);
    setError("");
    try {
      setMovimiento(
        await registrarMovimiento(Number(id), { ...datos, claveIdempotencia: crypto.randomUUID() }),
      );
      await consultar();
    } catch (exception) {
      setError(mensajeError(exception));
    } finally {
      setCargando(false);
    }
  }

  return { id, setId, saldo, movimiento, error, cargando, consultar, registrar };
}
