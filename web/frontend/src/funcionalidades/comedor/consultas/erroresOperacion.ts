export type ErrorOperacion = { codigo: string; mensaje: string };

type ErrorHttp = {
  response?: { data?: { detail?: { codigo?: string; mensaje?: string } } };
};

export function clasificarErrorOperacion(error: unknown): ErrorOperacion {
  const respuesta = (error ?? {}) as ErrorHttp;
  const detalle = respuesta.response?.data?.detail;
  return {
    codigo: detalle?.codigo ?? (respuesta.response ? "error_operacion" : "error_conexion"),
    mensaje: detalle?.mensaje ?? "No se pudo registrar el ingreso",
  };
}
