import PanelDominio from "../componentes/PanelDominio";
import { DOMINIOS } from "../consultas/dominios";

export default function DominioPage({ dominio }: { dominio: keyof typeof DOMINIOS }) {
  return <PanelDominio definicion={DOMINIOS[dominio]} />;
}
