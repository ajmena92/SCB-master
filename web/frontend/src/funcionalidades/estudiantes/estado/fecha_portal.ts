export function fechaLocalActual(): string {
  const ahora = new Date();
  const completar = (valor: number) => String(valor).padStart(2, "0");
  return `${ahora.getFullYear()}-${completar(ahora.getMonth() + 1)}-${completar(ahora.getDate())}`;
}
