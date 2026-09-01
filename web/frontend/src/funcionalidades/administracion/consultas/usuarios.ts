import { api } from "@/compartido/consultas/cliente_http";
import type {
  CuentaAdministrativa,
  CuentaCrearEntrada,
  CuentaEditarEntrada,
  PermisoAdministrativo,
  ProfesorDisponible,
  RespuestaCuentaCreada,
  RespuestaRestablecimiento,
  RespuestaVinculacionInicial,
  VinculacionInicialEntrada,
} from "@/compartido/contratos/usuarios_administrativos";

export const usuariosAdministrativosApi = {
  async listar(): Promise<CuentaAdministrativa[]> {
    return (await api.get<CuentaAdministrativa[]>("/v1/administracion/cuentas")).data;
  },
  async permisos(): Promise<PermisoAdministrativo[]> {
    return (await api.get<PermisoAdministrativo[]>("/v1/administracion/permisos")).data;
  },
  async profesores(): Promise<ProfesorDisponible[]> {
    return (await api.get<ProfesorDisponible[]>("/v1/administracion/profesores-disponibles")).data;
  },
  async crear(datos: CuentaCrearEntrada): Promise<RespuestaCuentaCreada> {
    return (await api.post<RespuestaCuentaCreada>("/v1/administracion/cuentas", datos)).data;
  },
  async actualizar(id: number, datos: CuentaEditarEntrada): Promise<CuentaAdministrativa> {
    return (await api.put<CuentaAdministrativa>(`/v1/administracion/cuentas/${id}`, datos)).data;
  },
  async restablecer(id: number): Promise<RespuestaRestablecimiento> {
    return (
      await api.post<RespuestaRestablecimiento>(
        `/v1/administracion/cuentas/${id}/restablecer-contrasena`,
      )
    ).data;
  },
  async vincular(datos: VinculacionInicialEntrada): Promise<RespuestaVinculacionInicial> {
    return (
      await api.post<RespuestaVinculacionInicial>("/v1/administracion/vinculacion-inicial", datos)
    ).data;
  },
  async cambiarContrasena(contrasenaActual: string, contrasenaNueva: string): Promise<void> {
    await api.post("/v1/autenticacion/administracion/contrasena", {
      contrasenaActual,
      contrasenaNueva,
    });
  },
};
