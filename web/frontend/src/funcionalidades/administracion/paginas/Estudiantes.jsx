import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { api, errMsg } from "@/lib/api";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { toast } from "sonner";
import {
  Download,
  FileText,
  IdCard,
  ImagePlus,
  KeyRound,
  Printer,
  Search,
  Upload,
  Trash2,
} from "lucide-react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { CardThumbnail } from "@/components/StudentCard";

const NO_SECTION = "__SIN_SECCION__";

function PinReport({ report, onClose }) {
  return (
    <div className="pin-report min-h-screen bg-background p-6 sm:p-10">
      <div className="pin-report-actions flex flex-wrap justify-between gap-3 mb-8">
        <Button variant="outline" onClick={onClose}>
          Volver a estudiantes
        </Button>
        <Button onClick={() => window.print()} data-testid="print-pin-report">
          <Printer className="h-4 w-4 mr-2" /> Imprimir / Guardar como PDF
        </Button>
      </div>
      <header className="border-b pb-5 mb-6">
        <p className="text-xs uppercase tracking-[0.2em] font-bold text-primary">Comedor SCSC</p>
        <h1 className="font-display text-3xl font-bold tracking-tight mt-2">
          Reporte de PIN por sección
        </h1>
        <div className="mt-3 flex flex-wrap gap-x-8 gap-y-1 text-sm text-muted-foreground">
          <span>
            <strong>Turno:</strong> {report.turno}
          </span>
          <span>
            <strong>Sección:</strong> {report.seccion}
          </span>
          <span>
            <strong>Estudiantes:</strong> {report.total}
          </span>
          <span>
            <strong>Generado:</strong> {report.generadoEn}
          </span>
        </div>
      </header>
      <div className="overflow-x-auto">
        <Table data-testid="pin-report-table">
          <TableHeader>
            <TableRow>
              <TableHead>#</TableHead>
              <TableHead>Estudiante</TableHead>
              <TableHead>Cédula</TableHead>
              <TableHead>Horario</TableHead>
              <TableHead className="text-right">PIN temporal</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {report.estudiantes.map((student, index) => (
              <TableRow key={student.idEstudiante}>
                <TableCell>{index + 1}</TableCell>
                <TableCell className="font-medium">{student.nombreCompleto}</TableCell>
                <TableCell>{student.cedula}</TableCell>
                <TableCell>{student.horario}</TableCell>
                <TableCell className="text-right font-display text-lg font-black tracking-[0.2em]">
                  {student.pin}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
      <p className="mt-8 text-xs text-muted-foreground">
        Documento confidencial. Entregue cada PIN al estudiante de forma segura. Todos los
        estudiantes deben cambiar este PIN al ingresar.
      </p>
    </div>
  );
}

export default function EstudiantesTab() {
  const [q, setQ] = useState("");
  const [buscar, setBuscar] = useState("");
  const [pagina, setPagina] = useState(1);
  const [nuevoPin, setNuevoPin] = useState(null);
  const [selectedTurno, setSelectedTurno] = useState("diurno");
  const [selectedSection, setSelectedSection] = useState("");
  const [report, setReport] = useState(null);
  const [reportLoading, setReportLoading] = useState(false);
  const [selectedStudent, setSelectedStudent] = useState(null);
  const [profile, setProfile] = useState(null);
  const [benefits, setBenefits] = useState([]);
  const [routes, setRoutes] = useState([]);
  const [photoFile, setPhotoFile] = useState(null);
  const [photoVersion, setPhotoVersion] = useState(() => Date.now());
  const [profileLoading, setProfileLoading] = useState(false);
  const [savingProfile, setSavingProfile] = useState(false);

  const {
    data: studentPage = { elementos: [], total: 0 },
    error: studentsError,
    isPending: loading,
    refetch,
  } = useQuery({
    queryKey: ["admin", "estudiantes", pagina, buscar],
    queryFn: async () =>
      (
        await api.get(
          `/v1/estudiantes?pagina=${pagina}&tamano=50&buscar=${encodeURIComponent(buscar)}`,
        )
      ).data,
  });
  const {
    data: sections = [],
    error: sectionsError,
    isPending: sectionsLoading,
  } = useQuery({
    queryKey: ["admin", "estudiantes", "secciones", selectedTurno],
    queryFn: async () =>
      (await api.get(`/v1/estudiantes/secciones?turno=${selectedTurno}`)).data,
    enabled: Boolean(selectedTurno),
  });
  const rows = studentPage.elementos ?? studentPage.items ?? [];
  const total = studentPage.total;
  useEffect(() => {
    const timer = setTimeout(() => {
      setPagina(1);
      setBuscar(q);
    }, 250);
    return () => clearTimeout(timer);
  }, [q]);
  useEffect(() => {
    if (studentsError) toast.error(errMsg(studentsError));
    if (sectionsError) toast.error(errMsg(sectionsError));
  }, [sectionsError, studentsError]);

  const reset = async (id) => {
    try {
      const { data } = await api.post(`/v1/estudiantes/${id}/reset-pin`);
      setNuevoPin({ id, pin: data.pin });
      toast.success("PIN reiniciado");
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    }
  };

  const openProfile = async (student) => {
    setSelectedStudent(student);
    setPhotoFile(null);
    setPhotoVersion((version) => version + 1);
    setProfileLoading(true);
    try {
      const [{ data: detail }, { data: catalog }, { data: routeCatalog }] = await Promise.all([
        api.get(`/v1/estudiantes/${student.idEstudiante}/perfil`),
        api.get("/v1/beneficios"),
        api.get("/v1/transporte/rutas"),
      ]);
      setProfile(detail);
      setBenefits(catalog);
      setRoutes(routeCatalog);
    } catch (e) {
      toast.error(errMsg(e));
      setSelectedStudent(null);
    } finally {
      setProfileLoading(false);
    }
  };

  const savePhoto = async () => {
    if (!photoFile || !selectedStudent) return;
    setSavingProfile(true);
    try {
      const form = new FormData();
      form.append("archivo", photoFile);
      await api.post(`/v1/estudiantes/${selectedStudent.idEstudiante ?? selectedStudent.IdUsuario}/foto`, form, {
        headers: { "Content-Type": "multipart/form-data" },
      });
      setProfile((current) => ({ ...current, tieneFoto: true }));
      setPhotoVersion((version) => version + 1);
      setPhotoFile(null);
      toast.success("Fotografía actualizada");
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSavingProfile(false);
    }
  };

  const removePhoto = async () => {
    if (!selectedStudent) return;
    setSavingProfile(true);
    try {
      await api.delete(`/v1/estudiantes/${selectedStudent.idEstudiante ?? selectedStudent.IdUsuario}/foto`);
      setProfile((current) => ({ ...current, tieneFoto: false }));
      setPhotoVersion((version) => version + 1);
      toast.success("Fotografía marcada como pendiente");
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSavingProfile(false);
    }
  };

  const saveBenefit = async (event) => {
    const idBeca = event.target.value === "" ? null : Number(event.target.value);
    if (!selectedStudent) return;
    setSavingProfile(true);
    try {
      await api.put(`/v1/estudiantes/${selectedStudent.idEstudiante}/beneficio`, { idBeca });
      toast.success("Beneficio actualizado");
      setProfile((current) => ({
        ...current,
        estudiante: { ...current.estudiante, TipoBeca: idBeca },
      }));
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSavingProfile(false);
    }
  };

  const saveRoute = async (event) => {
    const idRuta = event.target.value === "" ? null : Number(event.target.value);
    if (!selectedStudent) return;
    setSavingProfile(true);
    try {
      await api.put(`/v1/estudiantes/${selectedStudent.idEstudiante}/ruta`, { idRuta });
      const selectedRoute = routes.find((route) => route.idRuta === idRuta);
      setProfile((current) => ({
        ...current,
        estudiante: {
          ...current.estudiante,
          IdRuta: idRuta,
          RutaCodigo: selectedRoute?.codigo || null,
          RutaDescripcion: selectedRoute?.descripcion || null,
          RutaColor: selectedRoute?.colorCarnetHex || "#CBD5E1",
        },
      }));
      toast.success("Ruta actualizada");
      await refetch();
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setSavingProfile(false);
    }
  };

  const totalPaginas = Math.max(1, Math.ceil(total / 50));
  const selectedSectionData = sections.find(
    (section) => (section.seccion ?? NO_SECTION) === selectedSection,
  );

  const generarReporte = async () => {
    if (!selectedSection) return;
    setReportLoading(true);
    try {
      const { data } = await api.post("/v1/estudiantes/pines/seccion", {
        seccion: selectedSection === NO_SECTION ? "" : selectedSection,
        turno: selectedTurno || null,
      });
      setReport({ ...data, generadoEn: new Date().toLocaleString("es-CR") });
      toast.success(`PIN regenerado para ${data.total} estudiante(s)`);
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setReportLoading(false);
    }
  };

  if (report) return <PinReport report={report} onClose={() => setReport(null)} />;

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

      <div className="bg-card border rounded-lg p-5 space-y-4">
        <div className="flex items-start gap-3">
          <FileText className="h-5 w-5 text-primary mt-0.5" />
          <div>
            <h3 className="font-display font-bold">Reporte de PIN por sección</h3>
            <p className="text-sm text-muted-foreground mt-1">
              Seleccioná una sección para generar nuevos PIN temporales e imprimirlos o guardarlos
              como PDF.
            </p>
          </div>
        </div>
        <div className="flex flex-col sm:flex-row sm:items-end gap-3">
          <div className="w-full sm:max-w-xs space-y-2">
            <label htmlFor="pin-report-shift" className="text-sm font-medium">
              Turno
            </label>
            <select
              id="pin-report-shift"
              data-testid="pin-report-shift"
              value={selectedTurno}
              onChange={(event) => {
                setSelectedTurno(event.target.value);
                setSelectedSection("");
              }}
              disabled={reportLoading}
              className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="diurno">Diurno</option>
              <option value="nocturno">Nocturno</option>
            </select>
          </div>
          <div className="w-full sm:max-w-sm space-y-2">
            <label htmlFor="pin-report-section" className="text-sm font-medium">
              Sección
            </label>
            <select
              id="pin-report-section"
              data-testid="pin-report-section"
              value={selectedSection}
              onChange={(event) => setSelectedSection(event.target.value)}
              disabled={sectionsLoading || reportLoading}
              className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="">Seleccioná una sección</option>
              {sections.map((section) => (
                <option key={section.seccion ?? NO_SECTION} value={section.seccion ?? NO_SECTION}>
                  {section.etiqueta} ({section.total})
                </option>
              ))}
            </select>
          </div>
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button
                type="button"
                data-testid="generate-pin-report"
                disabled={!selectedSection || sectionsLoading || reportLoading}
              >
                <FileText className="h-4 w-4 mr-2" /> Generar reporte
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>¿Resetear todos los PIN?</AlertDialogTitle>
                <AlertDialogDescription>
                  Se resetearán todos los PIN de los {selectedSectionData?.total || "estudiantes"}{" "}
                  estudiante(s) de la sección <strong>{selectedSectionData?.etiqueta}</strong> del
                  turno <strong>{selectedTurno}</strong>. Los PIN anteriores dejarán de funcionar y
                  cada estudiante deberá cambiar su nuevo PIN al ingresar.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancelar</AlertDialogCancel>
                <AlertDialogAction
                  data-testid="confirm-generate-pin-report"
                  onClick={generarReporte}
                >
                  Resetear y generar reporte
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input
          data-testid="estudiante-search"
          placeholder="Buscar por nombre o cédula"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          className="pl-9"
        />
      </div>

      {loading ? (
        <Skeleton className="h-64 w-full rounded-lg" />
      ) : (
        <div className="bg-card border rounded-lg overflow-x-auto">
          <Table data-testid="estudiantes-table">
            <TableHeader>
              <TableRow>
                <TableHead>Estudiante</TableHead>
                <TableHead>Carnet</TableHead>
                <TableHead>Cédula</TableHead>
                <TableHead>Horario</TableHead>
                <TableHead>Sección</TableHead>
                <TableHead>Ruta</TableHead>
                <TableHead>Beca</TableHead>
                <TableHead>Estado PIN</TableHead>
                <TableHead className="text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {rows.map((r) => (
                <TableRow key={r.idEstudiante} className="hover:bg-muted/40">
                  <TableCell className="font-medium">{[r.nombre, r.primerApellido, r.segundoApellido].filter(Boolean).join(" ")}</TableCell>
                  <TableCell>
                    <CardThumbnail studentId={r.idEstudiante} hasPhoto={r.tieneFoto} />
                  </TableCell>
                  <TableCell>{r.cedula || "Pendiente"}</TableCell>
                  <TableCell>{r.turno || "—"}</TableCell>
                  <TableCell>{r.seccion || "Sin sección"}</TableCell>
                  <TableCell>{r.rutaCodigo || r.rutaDescripcion || "Sin ruta"}</TableCell>
                  <TableCell>{r.tipoBeca || "Sin beca"}</TableCell>
                  <TableCell>
                    {r.bloqueado ? (
                      <Badge variant="destructive">Bloqueado</Badge>
                    ) : r.debeCambiarPin ? (
                      <Badge variant="secondary">Debe cambiar</Badge>
                    ) : (
                      <Badge className="bg-success text-white">Activo</Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button
                      variant="outline"
                      size="sm"
                      className="mr-2"
                      onClick={() => openProfile(r)}
                      data-testid={`manage-card-${r.idEstudiante}`}
                    >
                      <IdCard className="h-3 w-3 mr-1" /> Carnet
                    </Button>
                    <AlertDialog>
                      <AlertDialogTrigger asChild>
                        <Button
                          variant="outline"
                          size="sm"
                          data-testid={`reset-pin-${r.idEstudiante}`}
                        >
                          <KeyRound className="h-3 w-3 mr-1" /> Reiniciar PIN
                        </Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>Reiniciar PIN de {r.nombre}</AlertDialogTitle>
                          <AlertDialogDescription>
                            Se generará un PIN temporal de 6 dígitos y el estudiante deberá
                            cambiarlo al ingresar.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>Cancelar</AlertDialogCancel>
                          <AlertDialogAction
                            data-testid={`confirm-reset-${r.idEstudiante}`}
                            onClick={() => reset(r.idEstudiante)}
                          >
                            Reiniciar
                          </AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      {!loading && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            {total} estudiante(s) · Página {pagina} de {totalPaginas}
          </span>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={pagina === 1}
              onClick={() => setPagina((value) => value - 1)}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={pagina >= totalPaginas}
              onClick={() => setPagina((value) => value + 1)}
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}

      <AlertDialog open={!!nuevoPin} onOpenChange={(o) => !o && setNuevoPin(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>PIN temporal generado</AlertDialogTitle>
            <AlertDialogDescription>
              Comuníquelo al estudiante de forma segura. No se volverá a mostrar.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <p
            data-testid="nuevo-pin-value"
            className="text-center font-display text-4xl font-black tracking-[0.3em] py-4 text-primary"
          >
            {nuevoPin?.pin}
          </p>
          <AlertDialogFooter>
            <AlertDialogAction onClick={() => setNuevoPin(null)}>Entendido</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog open={!!selectedStudent} onOpenChange={(open) => !open && setSelectedStudent(null)}>
        <DialogContent className="max-w-xl">
          <DialogHeader>
            <DialogTitle className="font-display">Editar estudiante</DialogTitle>
            <DialogDescription>{selectedStudent?.NombreCompleto}</DialogDescription>
          </DialogHeader>
          {profileLoading || !profile ? (
            <div className="h-32 animate-pulse rounded-lg bg-muted" />
          ) : (
            <div className="space-y-5">
              <div className="grid gap-4 sm:grid-cols-[1fr_180px]">
                <div className="space-y-3">
                  <label htmlFor="beneficio-comedor" className="text-sm font-medium">
                    Beneficio de comedor
                  </label>
                  <select
                    id="beneficio-comedor"
                    value={profile.estudiante.TipoBeca ?? ""}
                    onChange={saveBenefit}
                    disabled={savingProfile}
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  >
                    <option value="">Sin beca</option>
                    {benefits.map((benefit) => (
                      <option key={benefit.IdBeca} value={benefit.IdBeca}>
                        {benefit.Descripcion} · {benefit.DiasBeca || "todos los días"}
                      </option>
                    ))}
                  </select>
                  <p className="text-xs text-muted-foreground">
                    Se conserva la regla local de días definidos en TipoBeca.
                  </p>
                  <label htmlFor="student-route" className="text-sm font-medium block pt-2">
                    Ruta de transporte
                  </label>
                  <select
                    id="student-route"
                    value={profile.estudiante.IdRuta ?? ""}
                    onChange={saveRoute}
                    disabled={savingProfile}
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  >
                    <option value="">Sin ruta</option>
                    {routes
                      .filter(
                        (route) =>
                          (route.activo !== false && route.activo !== 0) ||
                          route.idRuta === profile.estudiante.IdRuta,
                      )
                      .map((route) => (
                        <option key={route.idRuta} value={route.idRuta}>
                          {route.codigo} · {route.descripcion}
                          {!route.activo ? " · Inactiva" : ""}
                        </option>
                      ))}
                  </select>
                  <p className="flex items-center gap-2 text-xs text-muted-foreground">
                    <span
                      className="h-3 w-3 rounded-full border"
                      style={{ backgroundColor: profile.estudiante.RutaColor || "#CBD5E1" }}
                    />{" "}
                    Este color identifica visualmente el carnet.
                  </p>
                </div>
                <div
                  className="flex min-h-[220px] items-center justify-center overflow-hidden rounded-lg border bg-muted/30"
                  data-testid="student-photo-preview"
                >
                  {profile.tieneFoto ? (
                    <img
                      src={`/api/v1/estudiantes/${selectedStudent.idEstudiante ?? selectedStudent.IdUsuario}/foto?v=${photoVersion}`}
                      alt={`Fotografía de ${selectedStudent.NombreCompleto}`}
                      className="h-full max-h-[280px] w-full object-cover object-top"
                    />
                  ) : (
                    <div className="px-5 text-center text-sm text-muted-foreground">
                      <ImagePlus className="mx-auto mb-2 h-8 w-8" />
                      <p>Fotografía pendiente</p>
                    </div>
                  )}
                </div>
              </div>
              <div className="rounded-lg border p-4 space-y-3">
                <div className="flex items-center gap-2 font-semibold">
                  <ImagePlus className="h-4 w-4 text-primary" /> Fotografía
                </div>
                <input
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  onChange={(event) => setPhotoFile(event.target.files?.[0] || null)}
                  disabled={savingProfile}
                  className="block w-full text-sm"
                />
                <div className="flex flex-wrap gap-2">
                  <Button onClick={savePhoto} disabled={!photoFile || savingProfile}>
                    <Upload className="mr-2 h-4 w-4" /> Cargar fotografía
                  </Button>
                  <Button
                    variant="outline"
                    onClick={removePhoto}
                    disabled={!profile.tieneFoto || savingProfile}
                  >
                    <Trash2 className="mr-2 h-4 w-4" /> Marcar pendiente
                  </Button>
                </div>
                <p className="text-xs text-muted-foreground">
                  JPG, PNG o WEBP. Máximo 5 MB. Si falta, se genera un carnet provisional.
                </p>
              </div>
            </div>
          )}
          <DialogFooter>
            {selectedStudent && (
              <Button variant="outline" asChild>
                <a href={`/api/v1/estudiantes/${selectedStudent.idEstudiante ?? selectedStudent.IdUsuario}/carnet.pdf`} download>
                  <Download className="mr-2 h-4 w-4" /> Descargar PDF
                </a>
              </Button>
            )}
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
