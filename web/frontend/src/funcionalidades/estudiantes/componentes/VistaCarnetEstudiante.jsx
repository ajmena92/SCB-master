import { VistaCarnetEstudiante as TarjetaCarnetEstudiante } from "@/funcionalidades/estudiantes/componentes/CarnetEstudiante";

export function VistaCarnetEstudiante({ sesion, carnet }) {
  return (
    <div className="animate-fade-up">
      <TarjetaCarnetEstudiante
        tieneFoto={sesion?.usuario?.TieneFoto ?? sesion?.usuario?.tieneFoto}
        datosCarnet={carnet.datos}
        cargando={carnet.cargando}
        error={carnet.error}
        alReintentar={carnet.recargar}
      />
    </div>
  );
}
