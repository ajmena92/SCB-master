import { toast } from "sonner";

type Opciones = {
  descripcion?: string;
  id?: string;
};

const opciones = (tipo: string, mensaje: string, duracion: number, extra?: Opciones) => ({
  duration: duracion,
  // Sonner actualiza el aviso con el mismo id en vez de apilar duplicados
  // cuando una acción se dispara varias veces (doble clic o reintento).
  id: extra?.id ?? `notificacion:${tipo}:${mensaje}`,
  ...extra,
});

/** Punto único para avisos breves de operación. Los errores de formularios
 * que necesitan corrección permanecen en la vista mediante EstadoPanel/Aviso. */
export const notificar = {
  exito: (mensaje: string, extra?: Opciones) =>
    toast.success(mensaje, opciones("exito", mensaje, 3500, extra)),
  error: (mensaje: string, extra?: Opciones) =>
    toast.error(mensaje, opciones("error", mensaje, 5000, extra)),
  advertencia: (mensaje: string, extra?: Opciones) =>
    toast.warning(mensaje, opciones("advertencia", mensaje, 5000, extra)),
  informacion: (mensaje: string, extra?: Opciones) =>
    toast.info(mensaje, opciones("informacion", mensaje, 4000, extra)),
};
