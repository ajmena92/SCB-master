export function emitirTonoEstacionComedor(tipo: "aceptado" | "rechazado") {
  const AudioContexto =
    window.AudioContext ??
    (window as Window & { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
  if (!AudioContexto) return;
  const contexto = new AudioContexto();
  const oscilador = contexto.createOscillator();
  const ganancia = contexto.createGain();
  const duracion = tipo === "aceptado" ? 0.12 : 0.22;
  oscilador.type = "sine";
  oscilador.frequency.value = tipo === "aceptado" ? 880 : 220;
  ganancia.gain.setValueAtTime(0.07, contexto.currentTime);
  ganancia.gain.exponentialRampToValueAtTime(0.001, contexto.currentTime + duracion);
  oscilador.connect(ganancia).connect(contexto.destination);
  oscilador.start();
  oscilador.stop(contexto.currentTime + duracion);
  window.setTimeout(() => void contexto.close(), 300);
}
