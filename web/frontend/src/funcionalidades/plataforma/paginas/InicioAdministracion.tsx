import { Link } from "react-router-dom";
import { EncabezadoPagina } from "../componentes/ElementosComunes";

const tareas = [
  ["Operar comedor", "/admin/panel/comedor", "Registrar ingresos y decidir excepciones."],
  ["Vender tiquetes", "/admin/panel/tiquetes", "Cargar saldo con la tarifa vigente."],
  ["Registrar transporte", "/admin/panel/transporte", "Guardar la marca diaria del estudiante."],
  ["Publicar menú", "/admin/panel/menu", "Preparar el menú visible por fecha."],
];

export default function InicioAdministracion() {
  return (
    <section>
      <EncabezadoPagina
        titulo="Inicio"
        descripcion="Accesos directos a las tareas operativas de hoy."
      />
      <div className="task-grid">
        {tareas.map(([titulo, ruta, detalle]) => (
          <Link className="task-link" to={ruta} key={ruta}>
            <strong>{titulo}</strong>
            <span>{detalle}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}
