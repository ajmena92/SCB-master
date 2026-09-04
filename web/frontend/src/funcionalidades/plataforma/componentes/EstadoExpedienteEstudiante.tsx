import { Aviso, EstadoCarga } from "./ElementosComunes";

export function CargandoExpedienteEstudiante() {
  return <section className="grid gap-4"><EstadoCarga /></section>;
}

export function ExpedienteEstudianteNoEncontrado({ alVolver }: { alVolver: () => void }) {
  return (
    <section className="grid gap-4">
      <Aviso tipo="error">No se encontró el estudiante solicitado. Vuelva al padrón y selecciónelo de nuevo.</Aviso>
      <button className="button secondary" type="button" onClick={alVolver}>Volver a estudiantes</button>
    </section>
  );
}
