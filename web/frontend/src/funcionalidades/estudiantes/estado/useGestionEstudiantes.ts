import { useEffect, useState, type ChangeEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { api } from "@/compartido/consultas/cliente_http";
import { errMsg } from "@/compartido/consultas/errores_api";
import { toast } from "sonner";
import type { BeneficioSalida } from "@/compartido/contratos/beneficios";
import type {
  PaginaEstudiantes,
  PinGenerado,
  EstudianteSalida,
} from "@/compartido/contratos/estudiantes";
import type { RutaSalida } from "@/compartido/contratos/transporte";
import type {
  PerfilEstudiante,
  ReportePinesEstudiantes,
  SeccionEstudiante,
} from "@/funcionalidades/estudiantes/modelo/contratos";

const SIN_SECCION = "__SIN_SECCION__";

export function useGestionEstudiantes() {
  const [textoBusqueda, setTextoBusqueda] = useState("");
  const [busqueda, setBusqueda] = useState("");
  const [pagina, setPagina] = useState(1);
  const [pinTemporal, setPinTemporal] = useState<{ pin: string } | null>(null);
  const [turno, setTurno] = useState("diurno");
  const [seccion, setSeccion] = useState("");
  const [reporte, setReporte] = useState<ReportePinesEstudiantes | null>(null);
  const [cargandoReporte, setCargandoReporte] = useState(false);
  const [estudianteSeleccionado, setEstudianteSeleccionado] = useState<EstudianteSalida | null>(
    null,
  );
  const [perfil, setPerfil] = useState<PerfilEstudiante | null>(null);
  const [beneficios, setBeneficios] = useState<BeneficioSalida[]>([]);
  const [rutas, setRutas] = useState<RutaSalida[]>([]);
  const [archivoFoto, setArchivoFoto] = useState<File | null>(null);
  const [versionFoto, setVersionFoto] = useState(() => Date.now());
  const [cargandoPerfil, setCargandoPerfil] = useState(false);
  const [guardandoPerfil, setGuardandoPerfil] = useState(false);

  const estudiantes = useQuery<PaginaEstudiantes>({
    queryKey: ["admin", "estudiantes", pagina, busqueda],
    queryFn: async () =>
      (
        await api.get(
          `/v1/estudiantes?pagina=${pagina}&tamano=50&buscar=${encodeURIComponent(busqueda)}`,
        )
      ).data,
  });
  const secciones = useQuery<SeccionEstudiante[]>({
    queryKey: ["admin", "estudiantes", "secciones", turno],
    queryFn: async () => (await api.get(`/v1/estudiantes/secciones?turno=${turno}`)).data,
    enabled: Boolean(turno),
  });

  useEffect(() => {
    const temporizador = setTimeout(() => {
      setPagina(1);
      setBusqueda(textoBusqueda);
    }, 250);
    return () => clearTimeout(temporizador);
  }, [textoBusqueda]);
  useEffect(() => {
    if (estudiantes.error) toast.error(errMsg(estudiantes.error));
    if (secciones.error) toast.error(errMsg(secciones.error));
  }, [estudiantes.error, secciones.error]);

  const recargar = estudiantes.refetch;
  const reiniciarPin = async (idEstudiante: number) => {
    try {
      const { data } = await api.post<PinGenerado>(`/v1/estudiantes/${idEstudiante}/reset-pin`);
      setPinTemporal({ pin: data.pin });
      toast.success("PIN reiniciado");
      await recargar();
    } catch (error) {
      toast.error(errMsg(error));
    }
  };
  const abrirPerfil = async (estudiante: EstudianteSalida) => {
    setEstudianteSeleccionado(estudiante);
    setArchivoFoto(null);
    setVersionFoto((actual) => actual + 1);
    setCargandoPerfil(true);
    try {
      const [{ data: detalle }, { data: catalogoBeneficios }, { data: catalogoRutas }] =
        await Promise.all([
          api.get<PerfilEstudiante>(`/v1/estudiantes/${estudiante.idEstudiante}/perfil`),
          api.get<BeneficioSalida[]>("/v1/beneficios"),
          api.get<RutaSalida[]>("/v1/transporte/rutas"),
        ]);
      setPerfil(detalle);
      setBeneficios(catalogoBeneficios);
      setRutas(catalogoRutas);
    } catch (error) {
      toast.error(errMsg(error));
      setEstudianteSeleccionado(null);
    } finally {
      setCargandoPerfil(false);
    }
  };
  const guardarFoto = async () => {
    if (!archivoFoto || !estudianteSeleccionado) return;
    setGuardandoPerfil(true);
    try {
      const formulario = new FormData();
      formulario.append("archivo", archivoFoto);
      await api.post(`/v1/estudiantes/${estudianteSeleccionado.idEstudiante}/foto`, formulario, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setPerfil((actual) => (actual ? { ...actual, tieneFoto: true } : actual));
      setVersionFoto((actual) => actual + 1);
      setArchivoFoto(null);
      toast.success("Fotografía actualizada");
      await recargar();
    } catch (error) {
      toast.error(errMsg(error));
    } finally {
      setGuardandoPerfil(false);
    }
  };
  const eliminarFoto = async () => {
    if (!estudianteSeleccionado) return;
    setGuardandoPerfil(true);
    try {
      await api.delete(`/v1/estudiantes/${estudianteSeleccionado.idEstudiante}/foto`);
      setPerfil((actual) => (actual ? { ...actual, tieneFoto: false } : actual));
      setVersionFoto((actual) => actual + 1);
      toast.success("Fotografía marcada como pendiente");
      await recargar();
    } catch (error) {
      toast.error(errMsg(error));
    } finally {
      setGuardandoPerfil(false);
    }
  };
  const guardarBeneficio = async (evento: ChangeEvent<HTMLSelectElement>) => {
    if (!estudianteSeleccionado) return;
    const idBeca = evento.target.value === "" ? null : Number(evento.target.value);
    setGuardandoPerfil(true);
    try {
      await api.put(`/v1/estudiantes/${estudianteSeleccionado.idEstudiante}/beneficio`, {
        idBeneficio: idBeca,
      });
      setPerfil((actual) =>
        actual ? { ...actual, estudiante: { ...actual.estudiante, tipoBeca: idBeca } } : actual,
      );
      toast.success("Beneficio actualizado");
      await recargar();
    } catch (error) {
      toast.error(errMsg(error));
    } finally {
      setGuardandoPerfil(false);
    }
  };
  const guardarRuta = async (evento: ChangeEvent<HTMLSelectElement>) => {
    if (!estudianteSeleccionado) return;
    const idRuta = evento.target.value === "" ? null : Number(evento.target.value);
    const ruta = rutas.find((actual) => actual.idRuta === idRuta);
    setGuardandoPerfil(true);
    try {
      await api.put(`/v1/estudiantes/${estudianteSeleccionado.idEstudiante}/ruta`, { idRuta });
      setPerfil((actual) =>
        actual
          ? {
              ...actual,
              estudiante: {
                ...actual.estudiante,
                idRuta,
                rutaCodigo: ruta?.codigo || null,
                rutaDescripcion: ruta?.descripcion || null,
                rutaColor: ruta?.colorCarnetHex || "#CBD5E1",
              },
            }
          : actual,
      );
      toast.success("Ruta actualizada");
      await recargar();
    } catch (error) {
      toast.error(errMsg(error));
    } finally {
      setGuardandoPerfil(false);
    }
  };
  const generarReporte = async () => {
    if (!seccion) return;
    setCargandoReporte(true);
    try {
      const { data } = await api.post<ReportePinesEstudiantes>("/v1/estudiantes/pines/seccion", {
        seccion: seccion === SIN_SECCION ? "" : seccion,
        turno: turno || null,
      });
      setReporte({ ...data, generadoEn: new Date().toLocaleString("es-CR") });
      toast.success(`PIN regenerado para ${data.total} estudiante(s)`);
    } catch (error) {
      toast.error(errMsg(error));
    } finally {
      setCargandoReporte(false);
    }
  };

  const paginaEstudiantes = estudiantes.data ?? { elementos: [], pagina: 1, tamano: 50, total: 0 };
  const listaEstudiantes = paginaEstudiantes.elementos;
  const total = paginaEstudiantes.total;
  return {
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
    setEstudianteSeleccionado,
    perfil,
    beneficios,
    rutas,
    archivoFoto,
    setArchivoFoto,
    versionFoto,
    cargandoPerfil,
    guardandoPerfil,
    listaEstudiantes,
    total,
    cargandoEstudiantes: estudiantes.isPending,
    secciones: secciones.data ?? [],
    cargandoSecciones: secciones.isPending,
    totalPaginas: Math.max(1, Math.ceil(total / 50)),
    seccionSeleccionada: (secciones.data ?? []).find(
      (item) => (item.seccion ?? SIN_SECCION) === seccion,
    ),
    reiniciarPin,
    abrirPerfil,
    guardarFoto,
    eliminarFoto,
    guardarBeneficio,
    guardarRuta,
    generarReporte,
  };
}
