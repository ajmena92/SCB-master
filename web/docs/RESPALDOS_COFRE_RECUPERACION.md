# Respaldos y cofre de recuperación

La única raíz visible de Drive para material técnico de SCB es
`Mi unidad/SCB-RESPALDOS`. No contiene valores legibles de secretos: PostgreSQL
usa el remoto cifrado de rclone y el cofre de recuperación usa GPG.

## Estructura

- `postgresql/`: copias diarias y mensuales cifradas de PostgreSQL.
- `recuperacion/legado/`: paquete previo, conservado hasta completar la prueba
  de recuperación.
- `recuperacion/cofre/`: cofre GPG y su SHA-256.

Las carpetas previas no se eliminan durante la migración. Solo se retiran tras
verificar `rclone check`, restaurar PostgreSQL en un entorno aislado y registrar
la aprobación de la persona responsable.

## Cofre GPG

Se requiere una o dos claves públicas GPG de custodios autorizados. Cada
custodio conserva su clave privada fuera del servidor, de Drive, del correo y
del repositorio. El cofre se cifra para todas las claves indicadas; cualquiera
puede abrirlo con su propia clave privada. Con un solo custodio no existe
redundancia: deberá respaldar su clave privada y su frase de paso fuera de
Drive y en una ubicación independiente.

El comando operativo recibe únicamente rutas a las claves públicas:

```bash
sudo /usr/local/sbin/scb-crear-cofre-recuperacion \
  --clave-publica /ruta/protegida/custodio.asc
```

Incluye secretos técnicos de la aplicación, ambas claves LUKS, cabecera LUKS y
configuración de rclone. Las rutas protegidas de la clave de recuperación y de
la cabecera se suministran mediante `SCB_CLAVE_LUKS_RECUPERACION` y
`SCB_CABECERA_LUKS`; si falta cualquiera, el comando aborta sin publicar nada.

El correo institucional notifica únicamente resultado, ruta y checksum. Nunca
incluye contraseñas, claves, adjuntos de cofres ni material de recuperación.
