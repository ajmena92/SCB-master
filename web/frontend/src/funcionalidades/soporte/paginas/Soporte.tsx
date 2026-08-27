import { useState } from "react";
import { api, errMsg } from "@/lib/api";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
export default function Soporte() {
  const [asunto, setAsunto] = useState("");
  const [detalle, setDetalle] = useState("");
  const [enviando, setEnviando] = useState(false);
  async function enviar() {
    setEnviando(true);
    try {
      await api.post("/v1/soporte/solicitudes", { asunto, detalle });
      toast.success("Solicitud enviada");
      setAsunto("");
      setDetalle("");
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setEnviando(false);
    }
  }
  return (
    <section className="max-w-2xl space-y-6">
      <h2 className="font-display text-2xl font-black">Ayuda y soporte</h2>
      <p className="text-muted-foreground">Envíe una solicitud al equipo de soporte.</p>
      <Input
        aria-label="Asunto"
        value={asunto}
        onChange={(e) => setAsunto(e.target.value)}
        placeholder="Asunto"
      />
      <Textarea
        aria-label="Detalle"
        value={detalle}
        onChange={(e) => setDetalle(e.target.value)}
        placeholder="Describa la situación"
        rows={6}
      />
      <Button disabled={enviando || !asunto || !detalle} onClick={enviar}>
        {enviando ? "Enviando…" : "Enviar solicitud"}
      </Button>
    </section>
  );
}
