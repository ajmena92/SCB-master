import { api } from "@/lib/api";

export interface Usuario { idUsuario: number; nombreUsuario: string; activo: boolean; permisos: string[]; roles: string[] }
export interface Rol { idRol: number; nombre: string; descripcion?: string; permisos: string[] }
export interface Permiso { clave: string; descripcion?: string; activo: boolean }
export const consultarUsuarios = async () => (await api.get<Usuario[]>("/v1/administracion/usuarios")).data;
export const consultarRoles = async () => (await api.get<Rol[]>("/v1/administracion/roles")).data;
export const consultarPermisos = async () => (await api.get<Permiso[]>("/v1/administracion/permisos")).data;
export const crearUsuario = async (datos: { nombreUsuario: string; contrasena: string; activo: boolean }) => (await api.post<Usuario>("/v1/administracion/usuarios", datos)).data;
export const crearRol = async (datos: { nombre: string; descripcion?: string }) => (await api.post<Rol>("/v1/administracion/roles", datos)).data;
export const editarUsuario = async (idUsuario: number, datos: Partial<{ nombreUsuario: string; contrasena: string; activo: boolean; roles: string[]; permisos: string[] }>) => (await api.put<Usuario>(`/v1/administracion/usuarios/${idUsuario}`, datos)).data;
