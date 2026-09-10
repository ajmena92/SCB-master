# Auditoría de Hyper-V — 10 de septiembre de 2026

## Alcance
Inspección de solo lectura realizada por SSH en `SRV-VIRTUALES`
(`172.30.210.50`). No se modificaron máquinas virtuales, discos, memoria,
red, checkpoints, servicios ni archivos.

## Hallazgo principal
El host tiene capacidad disponible; el límite estaba dentro de la máquina
virtual `portal-comedor`.

| Recurso | Estado |
| --- | --- |
| Host | Windows Server 2019 Standard, compilación 17763 |
| Memoria física del host | 31,5 GiB |
| Volumen `C:` | 991 GiB; 914 GiB libres |
| Volumen `D:` (`Datos`) | 2,67 TiB; 2,61 TiB libres |
| Controlador físico | DELL PERC H330; saludable y operativo |
| VM `portal-comedor` | En ejecución; generación 2; 4 vCPU |
| Memoria de `portal-comedor` | Dinámica: mínimo 4 GiB, inicio 8 GiB, máximo 12 GiB; 4,63 GiB asignados durante la revisión, demanda 3,70 GiB, estado correcto |
| Disco virtual de `portal-comedor` | `D:\Hyper-V\portal-comedor\Virtual Hard Disks\portal-comedor.vhdx` |
| Tipo de VHDX | Dinámico; capacidad virtual 64 GiB; archivo físico 27,35 GiB; fragmentación 9 % |
| Checkpoints de `portal-comedor` | Ninguno |

El VHDX dinámico crecerá físicamente sobre `D:` hasta su límite virtual de
64 GiB. Por tanto, los aproximadamente 2,61 TiB disponibles en el host no
llegan automáticamente a Ubuntu: primero hay que ampliar ese VHDX y después
ampliar la partición, LVM y el sistema de archivos dentro de la VM.

## Ejecución del 10 de septiembre
Se verificó nuevamente que `portal-comedor.vhdx` era el VHDX dinámico auditado,
conectado al controlador SCSI 0, ubicación 0, y que no existían checkpoints.
Se amplió en línea de 64 GiB a **256 GiB** mediante Hyper-V, sin apagar la VM.
El archivo físico aumentó de forma gradual a aproximadamente 29,36 GiB: al ser
dinámico, ocupará espacio en `D:` solo conforme Ubuntu escriba datos.

Desde Ubuntu se completó la ampliación en cadena: partición `sda3`, PV LVM,
volumen raíz a **64 GiB** y volumen LUKS de PostgreSQL a **128 GiB**, seguido
de `cryptsetup resize` y `resize2fs`. El sistema raíz quedó con 44 GiB libres
y PostgreSQL con 121 GiB libres. PostgreSQL, API y web continuaron saludables
durante la operación. Se tomó y verificó un respaldo antes de modificar capas
de almacenamiento.

## Salud observada
- Las dos VMs (`portal-comedor` y `MV-VoIP`) estaban en ejecución.
- No se encontraron eventos de error de los últimos siete días de los
  proveedores Hyper-V, NTFS, disco, almacenamiento o volmgr en el registro
  System.
- No se detectaron checkpoints que consuman espacio oculto o compliquen una
  ampliación.
- La memoria dinámica de `portal-comedor` tenía margen bajo su máximo de
  12 GiB. La memoria no fue la causa del incidente de almacenamiento.

## Riesgos y acciones recomendadas
1. Ampliar el VHDX de `portal-comedor` antes de que el disco virtual alcance
   64 GiB. Un objetivo inicial de 128 o 256 GiB deja margen para Docker,
   actualizaciones, registros y respaldos. La ampliación del VHDX no amplía
   por sí sola Ubuntu; requiere completar la ampliación de sus capas internas.
2. Alertar cuando el volumen `D:` del host quede con menos de 15 % libre y
   cuando el disco raíz y el volumen PostgreSQL dentro de la VM superen 75 %.
3. Mantener checkpoints solo durante intervenciones breves y eliminarlos tras
   verificar el resultado. No usar un checkpoint como respaldo de PostgreSQL.
4. Revisar el estado de actualizaciones de Windows Server 2019 y definir una
   ventana de mantenimiento; la auditoría no instaló actualizaciones ni
   comprobó su política.
5. Mantener copias de respaldo fuera del host Hyper-V. Un respaldo guardado
   únicamente en este host no protege ante una falla de hardware, ransomware
   o error administrativo del host.

## Plan de mejoras recomendado

### Prioridad 1: capacidad y recuperación
1. La ampliación inicial ya se completó: VHDX 256 GiB, raíz 64 GiB y volumen
   PostgreSQL 128 GiB. El grupo LVM conserva cerca de 61 GiB libres para una
   futura ampliación controlada.
2. Antes de volver a ampliar, conservar el procedimiento usado: verificar
   respaldo, ampliar VHDX, crecer la partición, `pvresize`, el LV, LUKS y ext4,
   y verificar la aplicación antes de cerrar el cambio.
3. Implementar la regla 3-2-1: tres copias, en dos medios distintos y una fuera
   de `SRV-VIRTUALES`. Mantener el respaldo propio de PostgreSQL con WAL y
   añadir un respaldo del host compatible con el escritor VSS de Hyper-V. La
   prueba de restauración debe ser periódica, no solo la creación del archivo.
4. Respaldar los volúmenes que contienen tanto VHDX como configuración de las
   VMs. En este host no basta con respaldar solo `D:` porque la configuración
   predeterminada de Hyper-V está en `C:\ProgramData\Microsoft\Windows\Hyper-V`.

### Prioridad 2: observabilidad y prevención
1. Alertar por capacidad en tres niveles: `D:` del host, VHDX de
   `portal-comedor` y sistemas de archivos dentro de Ubuntu. Umbrales
   recomendados: advertencia al 75 %, crítica al 85 % y acción inmediata al
   90 %. Vigilar por separado WAL, respaldos y registros Docker.
2. Ejecutar cada día el respaldo PostgreSQL y comprobar su checksum; ejecutar
   una restauración aislada al menos mensualmente. Alertar si el último
   respaldo verificable tiene más de 24 horas.
3. Rotar registros Docker y establecer límites de memoria y CPU por servicio.
   PostgreSQL no tenía límite de memoria Docker en la revisión anterior; medir
   carga antes de fijarlo para no limitar su caché de forma inadecuada.
4. Medir CPU, memoria y latencia de disco durante horas pico antes de aumentar
   vCPU o memoria. `portal-comedor` tiene memoria dinámica de 4 a 12 GiB y no
   mostraba presión; aumentar recursos sin medición puede perjudicar al host.

### Prioridad 3: continuidad, mantenimiento y seguridad
1. Configurar UPS con apagado ordenado: primero las VMs invitadas y luego el
   host. Confirmar que `portal-comedor` inicia automáticamente tras un corte y
   que PostgreSQL monta su volumen cifrado antes de Docker.
2. Mantener checkpoints solo durante cambios breves, de tipo producción, y
   eliminarlos después de validar. No sustituir respaldos con checkpoints ni
   conservar archivos AVHDX a largo plazo.
3. Actualizar y reiniciar bajo una ventana de mantenimiento el host y Ubuntu.
   Windows Server 2019 permanece con soporte extendido hasta enero de 2029,
   pero el host debe recibir sus actualizaciones de seguridad. La revisión
   encontró que el servicio VSS de la VM está operativo, aunque informa una
   diferencia de versión con el componente esperado por el host; actualizar el
   kernel y herramientas de integración de Ubuntu durante esa misma ventana y
   comprobar de nuevo VSS, Heartbeat, hora y apagado ordenado. La ventana de
   mantenimiento del 10 de septiembre instaló 59 actualizaciones de Ubuntu y
   reinició el portal. Tras arrancar con el kernel `6.8.0-139-generic`, LUKS,
   Docker, PostgreSQL, la API y los temporizadores quedaron activos y sanos.
4. Restringir la administración del host a la VLAN de gestión, usar cuentas
   nominales con mínimo privilegio y exigir autenticación SSH por clave/firma.
   No almacenar claves de recuperación LUKS ni
   respaldos sin cifrar dentro del repositorio.

No se recomienda optimizar o compactar el VHDX ahora: su fragmentación de 9 %
no explica el incidente y una compactación no aumenta sus 64 GiB virtuales.

## Fuentes de referencia
- Microsoft confirma que el redimensionamiento en línea está disponible para
  VHDX conectados por SCSI: [Resize-VHD](https://learn.microsoft.com/en-us/powershell/module/hyper-v/resize-vhd?view=windowsserver2025-ps).
- VHDX admite hasta 64 TiB y aporta tolerancia ante fallos de energía y mejor
  alineación: [Rendimiento de E/S de Hyper-V](https://learn.microsoft.com/en-us/windows-server/administration/performance-tuning/role/hyper-v-server/storage-io-performance).
- Los checkpoints de producción usan VSS o congelamiento de sistema de archivos
  en Linux; los estándar no son un respaldo: [Checkpoints de Hyper-V](https://learn.microsoft.com/en-us/windows-server/virtualization/hyper-v/checkpoints).
- El respaldo del host debe incluir los volúmenes con VHDX y configuración de
  Hyper-V: [Respaldo de VMs desde la partición primaria](https://learn.microsoft.com/en-us/troubleshoot/windows-server/virtualization/back-up-hyper-v-vm-from-parent-partition).
- La memoria dinámica debe ajustarse según carga habitual y picos:
  [Rendimiento de memoria Hyper-V](https://learn.microsoft.com/en-us/windows-server/administration/performance-tuning/role/hyper-v-server/memory-performance).
- Windows Server 2019 tiene soporte extendido hasta el 10 de enero de 2029:
  [Ciclo de vida de Windows Server 2019](https://learn.microsoft.com/en-us/lifecycle/products/windows-server-2019).

## Formato SSH por clave
Para los Windows Server internos, usar una clave privada cargada en `ssh-agent`
o indicada mediante `-i`; OpenSSH resuelve la cuenta de dominio con el usuario
simple:

    ssh -o BatchMode=yes amenaa@172.30.210.50

No incluir el dominio en el nombre de usuario SSH: el servicio lo registró como
usuario inválido. No se documentan ni se usan contraseñas en este procedimiento.
