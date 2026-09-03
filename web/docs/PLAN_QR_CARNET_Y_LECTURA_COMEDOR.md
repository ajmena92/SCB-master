# Plan de QR cifrado para carnet y comedor

**Estado:** aprobado para implementación local el 2026-09-02.

## Objetivo

Sustituir el código de barras visible del carnet digital por un QR que no exponga
la cédula y que pueda ser leído por la captura de comedor sin modificar las reglas
de reservas, tiquetes, autorizaciones o duplicidad.

## Decisiones de diseño

1. El QR contendrá un token `SCBQR1.` cifrado y autenticado; nunca una cédula,
   nombre, identificador secuencial ni PIN en texto claro.
2. El lector QR solamente entrega el texto al campo de captura como teclado USB.
   El descifrado ocurre exclusivamente en la API, antes de localizar a la persona.
3. El token contiene el identificador interno, la institución, versión, emisión y
   vencimiento anual. La API rechazará tokens alterados, vencidos o de otra institución.
4. La clave de cifrado se suministra mediante la variable de entorno
   `CARNET_QR_CLAVE`; no se registra en Git, en el QR, en el navegador ni en los
   eventos operativos.
5. Durante la transición, la API conserva temporalmente la lectura manual de
   cédula. Esto permite contingencia con carnets ya emitidos sin doble escritura
   ni cambios de datos.

## Alcance

- API: emitir y resolver el token QR. La auditoría guarda una huella no reversible
  de 39 caracteres del QR; no persiste el token ni expone la cédula.
- Portal: dibujar QR desde el token entregado por la API.
- Comedor: aceptar la lectura del QR en el mismo campo y aplicar exactamente las
  validaciones actuales después de resolver la persona.
- Pruebas: token válido, alterado, vencido y flujo de captura con QR.

No se cambia el modelo de reservas, los saldos, los ingresos ni los permisos.

## Secuencia y validación

1. Añadir el cifrado autenticado y pruebas unitarias que fallen para token válido,
   alterado y vencido.
2. Publicar el token QR desde el contrato de carnet y reemplazar el dibujo Code 128
   por QR en la interfaz. Verificar que el QR no contiene la cédula.
3. Resolver `SCBQR1.` en `POST /comedor/operacion`, conservar cédula directa como
   contingencia y comprobar que la captura QR produce el mismo resultado de negocio.
4. En entorno local, probar el lector físico primero en un campo neutral y luego
   con una reserva de prueba; no usar la operación real para validar el lector sin
  una reserva destinada a esa prueba.

Para generar la clave sin registrarla en el repositorio:

```bash
cd web/backend
python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"
```

Copiar el resultado únicamente a `CARNET_QR_CLAVE` del archivo `web/ops/.env`,
que se conserva fuera de Git.

## Riesgos y reversión

- Una fotografía del QR sigue siendo una copia del carnet. La imagen y el nombre
  mostrados al operador son el control visual; el cifrado protege la identidad,
  no la posesión del carnet.
- Rotar `CARNET_QR_CLAVE` invalida carnets existentes. Se hará al inicio del año
  lectivo o con una ventana anunciada, nunca de forma silenciosa.
- Si el lector QR falla, el operador podrá introducir la cédula durante la fase
  de transición. Retirar esa compatibilidad exige una prueba operativa aprobada.
