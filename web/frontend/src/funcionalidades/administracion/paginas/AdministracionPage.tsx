import { FormEvent, useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { errMsg } from "@/lib/api";
import { crearRol, crearUsuario, editarUsuario, consultarPermisos, consultarRoles, consultarUsuarios, type Permiso, type Rol, type Usuario } from "../consultas/administracion";

export function AdministracionPage() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [roles, setRoles] = useState<Rol[]>([]);
  const [permisos, setPermisos] = useState<Permiso[]>([]);
  const [error, setError] = useState(""); const [mensaje, setMensaje] = useState(""); const [cargando, setCargando] = useState(true);
  const cargar = async () => { setCargando(true); try { const [u, r, p] = await Promise.all([consultarUsuarios(), consultarRoles(), consultarPermisos()]); setUsuarios(u); setRoles(r); setPermisos(p); } catch (e) { setError(errMsg(e)); } finally { setCargando(false); } };
  useEffect(() => { const id = window.setTimeout(() => { void cargar(); }, 0); return () => window.clearTimeout(id); }, []);
  const ejecutar = async (evento: FormEvent<HTMLFormElement>, accion: (datos: Record<string, unknown>) => Promise<unknown>) => { evento.preventDefault(); setError(""); const datos = Object.fromEntries(new FormData(evento.currentTarget)); try { await accion(datos); evento.currentTarget.reset(); setMensaje("Cambios guardados correctamente."); await cargar(); } catch (e) { setError(errMsg(e)); } };
  return <main aria-labelledby="titulo-administracion" className="space-y-6 p-6">
    <h1 id="titulo-administracion" className="text-2xl font-semibold">Administración</h1>
    {error && <p role="alert">{error}</p>}{mensaje && <p role="status">{mensaje}</p>}
    <section className="grid gap-4 md:grid-cols-2" aria-label="Crear registros"><form className="space-y-3 rounded-xl border p-4" onSubmit={(e) => ejecutar(e, (d) => crearUsuario({ nombreUsuario: String(d.nombreUsuario), contrasena: String(d.contrasena), activo: true }))}><h2 className="font-semibold">Crear usuario</h2><Input name="nombreUsuario" required placeholder="Nombre de usuario" aria-label="Nombre de usuario" /><Input name="contrasena" required type="password" minLength={8} placeholder="Contraseña temporal" aria-label="Contraseña temporal" /><Button type="submit">Crear usuario</Button></form><form className="space-y-3 rounded-xl border p-4" onSubmit={(e) => ejecutar(e, (d) => crearRol({ nombre: String(d.nombre), descripcion: String(d.descripcion || "") }))}><h2 className="font-semibold">Crear rol</h2><Input name="nombre" required placeholder="Nombre del rol" aria-label="Nombre del rol" /><Input name="descripcion" placeholder="Descripción" aria-label="Descripción" /><Button type="submit">Crear rol</Button></form></section>
    <section aria-labelledby="usuarios-titulo"><h2 id="usuarios-titulo">Usuarios</h2>{cargando ? <p>Cargando…</p> : <ul>{usuarios.map((u) => <li key={u.idUsuario} className="flex gap-3 py-2"><span>{u.nombreUsuario} — {u.activo ? "Activo" : "Inactivo"}</span><Button type="button" variant="outline" size="sm" onClick={async () => { try { await editarUsuario(u.idUsuario, { activo: !u.activo }); await cargar(); } catch (e) { setError(errMsg(e)); } }}>{u.activo ? "Desactivar" : "Activar"}</Button></li>)}</ul>}</section>
    <section aria-labelledby="roles-titulo"><h2 id="roles-titulo">Roles</h2><ul>{roles.map((r) => <li key={r.idRol}>{r.nombre}: {r.permisos.join(", ") || "sin permisos"}</li>)}</ul></section>
    <section aria-labelledby="permisos-titulo"><h2 id="permisos-titulo">Permisos disponibles</h2><ul>{permisos.map((p) => <li key={p.clave}>{p.clave}{p.descripcion ? ` — ${p.descripcion}` : ""}</li>)}</ul></section>
  </main>;
}
export default AdministracionPage;
