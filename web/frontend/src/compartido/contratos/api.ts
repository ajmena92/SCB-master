/* eslint-disable */
/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export type MetodoHttp = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

export interface AccesoEstudiante {
  carne: string;
  pin: string;
}

export interface AsignacionEntrada {
  idBeneficio?: number | null;
}

export interface AsignacionSalida {
  idBeneficio: number | null;
  idEstudiante: number;
}

export interface AutenticacionSalida {
  csrfToken: string;
  expiraEn: string;
  idUsuario: number;
  nombreUsuario: string;
  permisos: Array<string>;
}

export interface BeneficioEntrada {
  activo?: boolean;
  descripcion?: string | null;
  diasPermitidos?: number;
  nombre: string;
}

export interface BeneficioSalida {
  activo: boolean;
  descripcion: string | null;
  diasPermitidos: number;
  idBeneficio: number;
  nombre: string;
}

export interface BodyCargarApiV1EstudiantesIdEstudianteFotoPost {
  archivo: string;
}

export interface BodyEjecutarApiV1ImportacionesLotesPost {
  archivo: string;
}

export interface BodyPrevisualizarApiV1ImportacionesPrevisualizacionesPost {
  archivo: string;
}

export interface CambioAsignacion {
  idBeneficio?: number | null;
  idRuta?: number | null;
}

export interface CambioPinEstudiante {
  pinActual: string;
  pinNuevo: string;
}

export interface ComponenteMenu {
  nombre: string;
  orden?: number;
  tipo?: string;
}

export interface CorreccionEntrada {
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  motivo: string;
}

export interface CredencialesEntrada {
  contrasena: string;
  nombreUsuario: string;
}

export interface DiaCalendario {
  fecha: string;
  habilitado: boolean;
}

export interface ErrorFila {
  fila: number;
  mensaje: string;
}

export interface EstadoSalud {
  estado?: string;
  fechaHoraUtc: string;
}

export interface EstudianteEntrada {
  activo?: boolean;
  carne: string;
  cedula?: string | null;
  nombre: string;
  primerApellido: string;
  seccion?: string | null;
  segundoApellido?: string | null;
}

export interface EstudianteSalida {
  activo: boolean;
  bloqueado?: boolean;
  carne: string;
  cedula: string | null;
  debeCambiarPin?: boolean;
  idBeneficio?: number | null;
  idEstudiante: number;
  idRuta?: number | null;
  nombre: string;
  primerApellido: string;
  rutaCodigo?: string | null;
  rutaDescripcion?: string | null;
  seccion: string | null;
  segundoApellido: string | null;
  tieneFoto?: boolean;
  tipoBeca?: string | null;
  turno?: string | null;
}

export interface EventoSalida {
  accion: string;
  creadoEn: string;
  detalle?: Record<string, unknown>;
  direccionIp?: string | null;
  entidad: string;
  idEntidad?: string | null;
  idEvento: number;
  idUsuario: number | null;
  modulo: string;
}

export interface GeneracionPinesSeccion {
  seccion?: string | null;
  turno?: string | null;
}

export interface HTTPValidationError {
  detail?: Array<ValidationError>;
}

export interface LoteSalida {
  creadoEn: string;
  errores: Array<ErrorFila>;
  estado: string;
  idLote: number;
  nombreArchivo: string;
  revertidoEn?: string | null;
  totalFilas: number;
}

export interface MarcaEntrada {
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  fecha: string;
  idEstudiante: number;
  observacion?: string | null;
}

export interface MarcaSalida {
  corregida?: boolean;
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  fecha: string;
  idEstudiante: number;
  idMarca: number;
  observacion?: string | null;
}

export interface MovimientoEntrada {
  claveIdempotencia: string;
  concepto?: string | null;
  monto: number | string;
  tipo: "recarga" | "consumo" | "ajuste";
}

export interface MovimientoSalida {
  claveIdempotencia: string;
  concepto: string | null;
  creadoEn: string;
  idCuenta: number;
  idMovimiento: number;
  monto: string;
  saldoAnterior: string;
  saldoNuevo: string;
  tipo: "recarga" | "consumo" | "ajuste";
}

export interface PaginaEstudiantes {
  elementos: Array<EstudianteSalida>;
  pagina: number;
  tamano: number;
  total: number;
}

export interface ParametrosEntrada {
  minutosAvisoPrevio: number;
}

export interface ParametrosSalida {
  minutosAvisoPrevio: number;
}

export interface PermisoSalida {
  activo?: boolean;
  clave: string;
  descripcion?: string | null;
}

export interface PinGenerado {
  idEstudiante: number;
  pin: string;
}

export interface PlantillaMenuEntrada {
  activo?: boolean;
  componentes?: Array<ComponenteMenu>;
  dia: number;
  observaciones?: string | null;
  semana: number;
  titulo: string;
}

export interface PlantillaMenuSalida {
  activo?: boolean;
  componentes?: Array<ComponenteMenu>;
  dia: number;
  idPlantilla: number;
  observaciones?: string | null;
  semana: number;
  titulo: string;
}

export interface Previsualizacion {
  cabeceras: Array<string>;
  errores: Array<ErrorFila>;
  filas: Array<Record<string, unknown>>;
  totalFilas: number;
  valida: boolean;
}

export interface RegistroComedorEntrada {
  fecha: string;
  idEstudiante: number;
}

export interface RegistroComedorSalida {
  fecha: string;
  idEstudiante: number;
  idRegistro: number;
  registradoPor: number;
}

export interface ReporteEstudiante {
  activo: boolean;
  carne: string;
  idEstudiante: number;
  nombreCompleto: string;
  seccion: string | null;
}

export interface ReporteEstudiantes {
  elementos: Array<ReporteEstudiante>;
  total: number;
}

export interface ReporteRuta {
  activo: boolean;
  codigo: string;
  descripcion: string;
  estudiantesAsignados: number;
  idRuta: number;
}

export interface ReporteTransporte {
  elementos: Array<ReporteRuta>;
  total: number;
}

export interface RolEntrada {
  descripcion?: string | null;
  nombre: string;
  permisos?: Array<string>;
}

export interface RolSalida {
  descripcion?: string | null;
  idRol: number;
  nombre: string;
  permisos?: Array<string>;
}

export interface RutaEntrada {
  activo?: boolean;
  codigo: string;
  colorHex: string;
  descripcion: string;
}

export interface RutaSalida {
  activo: boolean;
  codigo: string;
  colorCarnetHex: string;
  descripcion: string;
  estudiantesAsignados?: number;
  idRuta: number;
}

export interface SaldoSalida {
  actualizadoEn: string;
  idCuenta: number;
  idEstudiante: number;
  saldo: string;
}

export interface SesionActualSalida {
  expiraEn: string;
  idUsuario: number;
  tipo?: string;
  usuario?: Record<string, unknown>;
}

export interface SolicitudEntrada {
  asunto: string;
  detalle: string;
}

export interface SolicitudSalida {
  asunto: string;
  creadoPor: number;
  detalle: string;
  estado: string;
  idSolicitud: number;
}

export interface UsuarioEntrada {
  activo?: boolean;
  contrasena?: string | null;
  nombreUsuario: string;
  permisos?: Array<string>;
  roles?: Array<string>;
}

export interface UsuarioSalida {
  activo: boolean;
  idUsuario: number;
  nombreUsuario: string;
  permisos?: Array<string>;
  roles?: Array<string>;
}

export interface ValidationError {
  ctx?: Record<string, unknown>;
  input?: unknown;
  loc: Array<string | number>;
  msg: string;
  type: string;
}

export interface OperacionApi {
  metodo: MetodoHttp;
  ruta: string;
  operacionId: string;
}

export const OPERACIONES_API: readonly OperacionApi[] = [
  { metodo: 'GET', ruta: '/api/health', operacionId: 'consultar_salud_api_health_get' },
  { metodo: 'GET', ruta: '/api/ready', operacionId: 'consultar_disponibilidad_api_ready_get' },
  { metodo: 'GET', ruta: '/api/v1/administracion/permisos', operacionId: 'permisos_api_v1_administracion_permisos_get' },
  { metodo: 'GET', ruta: '/api/v1/administracion/roles', operacionId: 'roles_api_v1_administracion_roles_get' },
  { metodo: 'POST', ruta: '/api/v1/administracion/roles', operacionId: 'crear_rol_api_v1_administracion_roles_post' },
  { metodo: 'GET', ruta: '/api/v1/administracion/usuarios', operacionId: 'usuarios_api_v1_administracion_usuarios_get' },
  { metodo: 'POST', ruta: '/api/v1/administracion/usuarios', operacionId: 'crear_usuario_api_v1_administracion_usuarios_post' },
  { metodo: 'PUT', ruta: '/api/v1/administracion/usuarios/{id_usuario}', operacionId: 'editar_usuario_api_v1_administracion_usuarios__id_usuario__put' },
  { metodo: 'GET', ruta: '/api/v1/asistencia/marcas', operacionId: 'listar_api_v1_asistencia_marcas_get' },
  { metodo: 'POST', ruta: '/api/v1/asistencia/marcas', operacionId: 'registrar_api_v1_asistencia_marcas_post' },
  { metodo: 'PUT', ruta: '/api/v1/asistencia/marcas/{id_marca}/correccion', operacionId: 'corregir_api_v1_asistencia_marcas__id_marca__correccion_put' },
  { metodo: 'GET', ruta: '/api/v1/auditoria/eventos', operacionId: 'eventos_api_v1_auditoria_eventos_get' },
  { metodo: 'POST', ruta: '/api/v1/autenticacion', operacionId: 'autenticar_api_v1_autenticacion_post' },
  { metodo: 'GET', ruta: '/api/v1/beneficios', operacionId: 'listar_api_v1_beneficios_get' },
  { metodo: 'POST', ruta: '/api/v1/beneficios', operacionId: 'crear_api_v1_beneficios_post' },
  { metodo: 'GET', ruta: '/api/v1/beneficios/estudiantes/{id_estudiante}', operacionId: 'obtener_asignacion_api_v1_beneficios_estudiantes__id_estudiante__get' },
  { metodo: 'PUT', ruta: '/api/v1/beneficios/estudiantes/{id_estudiante}', operacionId: 'asignar_api_v1_beneficios_estudiantes__id_estudiante__put' },
  { metodo: 'PUT', ruta: '/api/v1/beneficios/{id_beneficio}', operacionId: 'editar_api_v1_beneficios__id_beneficio__put' },
  { metodo: 'GET', ruta: '/api/v1/calendario', operacionId: 'calendario_api_v1_calendario_get' },
  { metodo: 'POST', ruta: '/api/v1/comedor/registros', operacionId: 'registrar_api_v1_comedor_registros_post' },
  { metodo: 'POST', ruta: '/api/v1/cuentas/{id_estudiante}/movimientos', operacionId: 'movimiento_api_v1_cuentas__id_estudiante__movimientos_post' },
  { metodo: 'GET', ruta: '/api/v1/cuentas/{id_estudiante}/saldo', operacionId: 'saldo_api_v1_cuentas__id_estudiante__saldo_get' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes', operacionId: 'listar_api_v1_estudiantes_get' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes', operacionId: 'crear_api_v1_estudiantes_post' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/asistencia/hoy', operacionId: 'asistencia_hoy_api_v1_estudiantes_asistencia_hoy_get' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/asistencia/{accion}', operacionId: 'registrar_asistencia_api_v1_estudiantes_asistencia__accion__post' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/autenticacion', operacionId: 'autenticar_api_v1_estudiantes_autenticacion_post' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/carnet', operacionId: 'carnet_api_v1_estudiantes_carnet_get' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/menu', operacionId: 'menu_api_v1_estudiantes_menu_get' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/pin', operacionId: 'cambiar_pin_api_v1_estudiantes_pin_post' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/pines/seccion', operacionId: 'generar_pines_seccion_api_v1_estudiantes_pines_seccion_post' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/secciones', operacionId: 'secciones_api_v1_estudiantes_secciones_get' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/{id_estudiante}', operacionId: 'obtener_api_v1_estudiantes__id_estudiante__get' },
  { metodo: 'PUT', ruta: '/api/v1/estudiantes/{id_estudiante}', operacionId: 'editar_api_v1_estudiantes__id_estudiante__put' },
  { metodo: 'PUT', ruta: '/api/v1/estudiantes/{id_estudiante}/beneficio', operacionId: 'beneficio_api_v1_estudiantes__id_estudiante__beneficio_put' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/{id_estudiante}/carnet.pdf', operacionId: 'carnet_api_v1_estudiantes__id_estudiante__carnet_pdf_get' },
  { metodo: 'DELETE', ruta: '/api/v1/estudiantes/{id_estudiante}/foto', operacionId: 'eliminar_api_v1_estudiantes__id_estudiante__foto_delete' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/{id_estudiante}/foto', operacionId: 'consultar_api_v1_estudiantes__id_estudiante__foto_get' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/{id_estudiante}/foto', operacionId: 'cargar_api_v1_estudiantes__id_estudiante__foto_post' },
  { metodo: 'GET', ruta: '/api/v1/estudiantes/{id_estudiante}/perfil', operacionId: 'perfil_api_v1_estudiantes__id_estudiante__perfil_get' },
  { metodo: 'POST', ruta: '/api/v1/estudiantes/{id_estudiante}/reset-pin', operacionId: 'reset_pin_api_v1_estudiantes__id_estudiante__reset_pin_post' },
  { metodo: 'PUT', ruta: '/api/v1/estudiantes/{id_estudiante}/ruta', operacionId: 'ruta_api_v1_estudiantes__id_estudiante__ruta_put' },
  { metodo: 'POST', ruta: '/api/v1/importaciones/lotes', operacionId: 'ejecutar_api_v1_importaciones_lotes_post' },
  { metodo: 'GET', ruta: '/api/v1/importaciones/lotes/{id_lote}', operacionId: 'lote_api_v1_importaciones_lotes__id_lote__get' },
  { metodo: 'POST', ruta: '/api/v1/importaciones/lotes/{id_lote}/reversion', operacionId: 'revertir_api_v1_importaciones_lotes__id_lote__reversion_post' },
  { metodo: 'POST', ruta: '/api/v1/importaciones/previsualizaciones', operacionId: 'previsualizar_api_v1_importaciones_previsualizaciones_post' },
  { metodo: 'GET', ruta: '/api/v1/menu/plantillas', operacionId: 'listar_api_v1_menu_plantillas_get' },
  { metodo: 'POST', ruta: '/api/v1/menu/plantillas', operacionId: 'guardar_api_v1_menu_plantillas_post' },
  { metodo: 'GET', ruta: '/api/v1/parametros', operacionId: 'obtener_api_v1_parametros_get' },
  { metodo: 'PUT', ruta: '/api/v1/parametros', operacionId: 'guardar_api_v1_parametros_put' },
  { metodo: 'GET', ruta: '/api/v1/reportes/estudiantes', operacionId: 'estudiantes_api_v1_reportes_estudiantes_get' },
  { metodo: 'GET', ruta: '/api/v1/reportes/estudiantes.csv', operacionId: 'estudiantes_csv_api_v1_reportes_estudiantes_csv_get' },
  { metodo: 'GET', ruta: '/api/v1/reportes/transporte', operacionId: 'transporte_api_v1_reportes_transporte_get' },
  { metodo: 'GET', ruta: '/api/v1/reportes/transporte.csv', operacionId: 'transporte_csv_api_v1_reportes_transporte_csv_get' },
  { metodo: 'GET', ruta: '/api/v1/salud', operacionId: 'consultar_salud_api_v1_salud_get' },
  { metodo: 'GET', ruta: '/api/v1/sesion', operacionId: 'consultar_sesion_api_v1_sesion_get' },
  { metodo: 'POST', ruta: '/api/v1/sesion/cerrar', operacionId: 'cerrar_sesion_api_v1_sesion_cerrar_post' },
  { metodo: 'POST', ruta: '/api/v1/soporte/solicitudes', operacionId: 'crear_api_v1_soporte_solicitudes_post' },
  { metodo: 'GET', ruta: '/api/v1/transporte/rutas', operacionId: 'listar_rutas_api_v1_transporte_rutas_get' },
  { metodo: 'POST', ruta: '/api/v1/transporte/rutas', operacionId: 'crear_ruta_api_v1_transporte_rutas_post' },
  { metodo: 'GET', ruta: '/api/v1/transporte/rutas/paleta', operacionId: 'listar_paleta_api_v1_transporte_rutas_paleta_get' },
  { metodo: 'PUT', ruta: '/api/v1/transporte/rutas/{id_ruta}', operacionId: 'editar_ruta_api_v1_transporte_rutas__id_ruta__put' },
] as const;
