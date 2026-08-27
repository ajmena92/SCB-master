import { Input } from "@/components/ui/input";
import { Search } from "lucide-react";
import { ControlesReportePines } from "@/funcionalidades/estudiantes/componentes/ControlesReportePines";
import { DialogoPerfilEstudiante } from "@/funcionalidades/estudiantes/componentes/DialogoPerfilEstudiante";
import { DialogoPinTemporal } from "@/funcionalidades/estudiantes/componentes/DialogoPinTemporal";
import { PaginacionEstudiantes } from "@/funcionalidades/estudiantes/componentes/PaginacionEstudiantes";
import { ReportePines } from "@/funcionalidades/estudiantes/componentes/ReportePines";
import { TablaEstudiantes } from "@/funcionalidades/estudiantes/componentes/TablaEstudiantes";
import { useGestionEstudiantes } from "@/funcionalidades/estudiantes/estado/useGestionEstudiantes";

export default function EstudiantesTab() {
  const {
    textoBusqueda,
    setTextoBusqueda,
    pagina,
    setPagina,
    pinTemporal,
    setPinTemporal,
    turno,
    setTurno,
    seccion,
    setSeccion,
    reporte,
    setReporte,
    cargandoReporte,
    estudianteSeleccionado,
    perfil,
    beneficios,
    rutas,
    archivoFoto,
    setArchivoFoto,
    versionFoto,
    cargandoPerfil,
    guardandoPerfil,
    setEstudianteSeleccionado,
    listaEstudiantes,
    total,
    cargandoEstudiantes,
    secciones,
    cargandoSecciones,
    totalPaginas,
    seccionSeleccionada,
    reiniciarPin,
    abrirPerfil,
    guardarFoto,
    eliminarFoto,
    guardarBeneficio,
    guardarRuta,
    generarReporte,
  } = useGestionEstudiantes();
  if (reporte) return <ReportePines reporte={reporte} alCerrar={() => setReporte(null)} />;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="font-display text-2xl font-bold tracking-tight">
          Estudiantes y gestión de PIN
        </h2>
        <p className="text-sm text-muted-foreground">
          Reiniciá el PIN; el estudiante deberá cambiarlo en su próximo ingreso.
        </p>
      </div>

      <ControlesReportePines
        turno={turno}
        seccion={seccion}
        secciones={secciones}
        cargandoSecciones={cargandoSecciones}
        cargandoReporte={cargandoReporte}
        seccionSeleccionada={seccionSeleccionada}
        alCambiarTurno={(valor) => {
          setTurno(valor);
          setSeccion("");
        }}
        alCambiarSeccion={setSeccion}
        alGenerar={generarReporte}
      />

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          data-testid="estudiante-search"
          placeholder="Buscar por nombre o cédula"
          value={textoBusqueda}
          onChange={(evento) => setTextoBusqueda(evento.target.value)}
          className="pl-9"
        />
      </div>

      <TablaEstudiantes
        estudiantes={listaEstudiantes}
        cargando={cargandoEstudiantes}
        alAbrirPerfil={abrirPerfil}
        alReiniciar={reiniciarPin}
      />
      {!cargandoEstudiantes && (
        <PaginacionEstudiantes
          total={total}
          pagina={pagina}
          totalPaginas={totalPaginas}
          alCambiarPagina={setPagina}
        />
      )}

      <DialogoPinTemporal pin={pinTemporal?.pin ?? null} alCerrar={() => setPinTemporal(null)} />

      <DialogoPerfilEstudiante
        estudiante={estudianteSeleccionado}
        perfil={perfil}
        beneficios={beneficios}
        rutas={rutas}
        cargando={cargandoPerfil}
        guardando={guardandoPerfil}
        archivo={archivoFoto}
        versionFoto={versionFoto}
        alCerrar={() => setEstudianteSeleccionado(null)}
        alCambiarBeneficio={guardarBeneficio}
        alCambiarRuta={guardarRuta}
        alSeleccionarArchivo={setArchivoFoto}
        alGuardarFoto={guardarFoto}
        alEliminarFoto={eliminarFoto}
      />
    </div>
  );
}
