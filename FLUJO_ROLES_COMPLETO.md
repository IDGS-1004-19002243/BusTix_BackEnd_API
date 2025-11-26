# 🚌 BusTix - Guía Completa de Flujos por Rol y Módulo

## 📖 Sobre este documento

Esta es la **guía definitiva** del sistema BusTix que explica cómo funciona cada módulo, quién usa cada endpoint y cómo se conectan entre sí para crear una experiencia completa de venta y gestión de transporte a eventos.

## 📑 Índice

1. [Introducción al Sistema](#introducción-al-sistema)
2. [Arquitectura y Roles](#arquitectura-y-roles)
3. [Módulos del Sistema](#módulos-del-sistema)
4. [Flujos Completos por Rol](#flujos-completos-por-rol)
5. [Casos de Uso Reales](#casos-de-uso-reales)
6. [Seguridad y Permisos](#seguridad-y-permisos)
7. [Notificaciones Automáticas](#notificaciones-automáticas)
8. [Referencia Rápida de APIs](#referencia-rápida-de-apis)

---

## 🎯 Introducción al Sistema

**BusTix** es una plataforma completa para la venta y gestión de boletos de transporte a eventos masivos (conciertos, festivales, deportes). El sistema maneja:

- ✅ **Precios dinámicos por parada**: Cada punto de abordaje puede tener su propio precio
- ✅ **Check-in progresivo**: El chofer confirma llegada → Staff valida pasajeros → Control en tiempo real
- ✅ **Notificaciones automáticas**: Push, email y SignalR en cada paso del viaje
- ✅ **Gestión de incidencias**: Reporte y seguimiento de problemas en ruta
- ✅ **Auditoría completa**: Registro de todos los cambios en el sistema
- ✅ **Multi-rol**: Admin, Operador, Staff, Chofer y Cliente con permisos específicos

### 🆕 Mejoras Recientes (Última Actualización)

El sistema ha sido optimizado con las siguientes mejoras de rendimiento y funcionalidad:

#### **1. MeController - Gestión Completa de Perfil**
- ✅ **PUT /api/me/perfil**: Actualización de información personal
- ✅ **POST /api/me/cambiar-password**: Cambio seguro de contraseña con UserManager
- ✅ **DELETE /api/me**: Eliminación de cuenta (soft delete) con cumplimiento GDPR

#### **2. IncidenciasController - Notificaciones Automáticas**
- ✅ Notificación automática al asignar incidencia a un técnico
- ✅ Notificación al reportador cuando se resuelve/cierra la incidencia
- ✅ Procesamiento en segundo plano (no bloquea el API)

#### **3. NotificacionesController - Broadcast Optimizado**
- ✅ Envío masivo de notificaciones sin timeout
- ✅ Retorna 202 Accepted inmediatamente
- ✅ Procesamiento paralelo con `Parallel.ForEachAsync` (MaxDegreeOfParallelism: 5)
- ✅ Uso de `IServiceScopeFactory` para manejo seguro de DbContext

#### **4. ReportesController - Escalabilidad Mejorada**
- ✅ Agregaciones ejecutadas en la base de datos (SQL) en lugar de memoria
- ✅ Soporte para grandes volúmenes de datos sin degradación de rendimiento
- ✅ Endpoints optimizados: `/ventas` y `/ocupacion`

---

## 👥 Arquitectura y Roles

### **Rol: Administrador (Admin)**
**¿Qué hace?**
- Crea y configura eventos (nombre, fecha, ubicación, recinto)
- Define rutas y paradas con tiempos estimados
- Registra unidades (autobuses) con capacidad y características
- Crea viajes y asigna chofer + unidad
- Configura precios dinámicos por parada
- Asigna staff a viajes
- Gestiona roles y permisos de usuarios
- Consulta reportes y auditoría

**Permisos principales**: `Eventos.Create`, `Viajes.Create`, `Viajes.Edit`, `Account.Manage`, `Reportes.View`

### **Rol: Operador**
**¿Qué hace?**
- Monitorea ventas en tiempo real
- Apoya al admin en tareas operativas
- Gestiona cambios de estado de viajes
- Atiende incidencias reportadas
- Re-asigna personal en caso de emergencias
- Genera reportes de ocupación y ventas

**Permisos principales**: `Viajes.View`, `Viajes.Edit`, `Reportes.View`, `Incidencias.Manage`

### **Rol: Staff (Personal de Parada)**
**¿Qué hace?**
- Recibe notificación cuando el chofer llega a su parada
- Inicia proceso de validación
- Escanea QR de cada pasajero (valida boletos)
- Registra no-shows (pasajeros que no abordaron)
- Finaliza validación con totales
- Reporta incidencias si hay problemas

**Permisos principales**: `Viajes.Validate`, `Boletos.Validate`

### **Rol: Chofer (Conductor)**
**¿Qué hace?**
- Confirma llegada a cada parada con ubicación GPS
- Espera confirmación del staff para continuar
- Consulta manifiesto de pasajeros
- Reporta incidencias en ruta

**Permisos principales**: `Viajes.Validate`, `Incidencias.Create`

### **Rol: Cliente (Usuario Final)**
**¿Qué hace?**
- Explora eventos disponibles
- Ve viajes y compara precios por parada
- Calcula precio exacto antes de comprar
- Compra boleto y recibe QR
- Recibe recordatorios automáticos (24h, 2h antes)
- Recibe notificación cuando el chofer llega
- Presenta QR al staff para abordar

**Permisos**: Ninguno especial (rol público con acceso limitado)

---

## 📦 Módulos del Sistema

Esta sección explica en detalle cada módulo del sistema, sus endpoints y cómo se usan en flujos reales.

### 📊 Resumen de Módulos

| Módulo | Endpoints | Propósito Principal | Usuarios |
|--------|-----------|---------------------|----------|
| Account | 29 | Autenticación, gestión de usuarios y permisos | Todos |
| Eventos | 6 | Crear y gestionar eventos (conciertos, deportes) | Admin, Cliente |
| Viajes | 14 | Crear viajes, asignar recursos, consultar disponibilidad | Admin, Operador, Chofer, Staff, Cliente |
| Boletos | 10 | Compra, validación y gestión de boletos | Cliente, Staff, Admin |
| Pagos | 4 | Procesamiento y confirmación de pagos | Cliente, Sistema |
| PreciosParada | 6 | Configurar precios dinámicos por parada | Admin |
| CheckInProgresivo | 4 | Gestión del check-in por parada (Chofer→Staff) | Chofer, Staff, Admin |
| Notificaciones | 8 | Envío y gestión de notificaciones | Todos |
| Reportes | 3 | KPIs, ventas, ocupación y dashboard | Admin, Operador |
| Incidencias | 7 | Reporte y gestión de problemas en ruta | Staff, Chofer, Admin |
| Cupones | 6 | Creación y validación de descuentos | Admin, Cliente |
| Rutas | 6 | Plantillas de rutas con paradas | Admin |
| Unidades | 5 | Registro y gestión de flota | Admin |
| Auditoría | 3 | Registro de cambios en el sistema | Admin |
| Roles | 4 | Gestión de roles y asignaciones | Admin |
| ViajeStaff | 4 | Asignación de personal a viajes | Admin, Operador |
| Me | 4 | Datos del usuario autenticado | Todos |
| Sincronización | 1 | Sync de validaciones offline | Staff |
| Validación | 1 | Validaciones externas | Sistema |

---

## 🔐 Account (Gestión de Usuarios y Autenticación)

**Objetivo**: Gestionar el ciclo completo de usuarios — desde registro hasta bloqueo/desbloqueo, incluyendo autenticación JWT y confirmación de email.

### **Endpoints del módulo (29 total)**

Account (gestión de usuarios)
- POST /api/Account/register
  - Qué hace: Registra un nuevo usuario en el sistema.
  - Quién lo usa: Clientes (registro), Admin (crear usuarios internos si se habilita).
  - Inputs: email, fullName, password, roles (opcional)
  - Output: usuario creado + token opcional / mensaje de verificación.
  - Notas: puede enviar email de confirmación; aplicar validaciones de contraseña.

- POST /api/Account/login
  - Qué hace: Autentica y devuelve token JWT + refresh token.
  - Quién lo usa: Todos los usuarios (cliente, staff, chofer, admin, operador).
  - Inputs: email/username + password
  - Output: accessToken, refreshToken, expiración, roles/claims.

- POST /api/Account/forgot-password
  - Qué hace: Inicia flujo de recuperación de contraseña (envía email con token).
  - Quién lo usa: Usuarios finales.
  - Inputs: email
  - Notas: token temporal para reset.

- POST /api/Account/change-password
  - Qué hace: Cambia la contraseña cuando el usuario está autenticado.
  - Quién lo usa: Usuarios autenticados.
  - Inputs: currentPassword, newPassword

- POST /api/Account/reset-password
  - Qué hace: Resetea contraseña mediante token enviado por forgot-password.
  - Quién lo usa: Usuarios que recibieron token.
  - Inputs: token, newPassword

- GET /api/Account/detail
  - Qué hace: Retorna info del usuario autenticado.
  - Quién lo usa: Cliente/Staff/Chofer/Admin para ver su perfil.

- GET /api/Account
  - Qué hace: Lista usuarios (paginado/filtrado).
  - Quién lo usa: Admin/Operador.

- POST /api/Account/logout
  - Qué hace: Cerrar sesión; invalidar refresh token.
  - Quién lo usa: Usuarios autenticados.

- POST /api/Account/refresh-token
  - Qué hace: Obtener nuevo access token usando refresh token.
  - Quién lo usa: Clientes/Apps que mantienen sesión.
  - Inputs: refreshToken

- POST /api/Account/revoke-token/{userId}
  - Qué hace: Revoca tokens de un usuario (logout forzoso).
  - Quién lo usa: Admin/Operador.

- GET /api/Account/permissions
  - Qué hace: Devuelve lista de permisos habilitados para el usuario.
  - Quién lo usa: Frontends para enrutar funcionalidades.

- PUT /api/Account/update-profile
  - Qué hace: Actualiza perfil del usuario autenticado.
  - Quién lo usa: Todos.

- PUT /api/Account/{userId}/status
  - Qué hace: Cambia estado (activo/inactivo) de un usuario.
  - Quién lo usa: Admin.

- GET /api/Account/statuses
  - Qué hace: Lista estados posibles del sistema (Activo, Bloqueado, etc.).
  - Quién lo usa: Admin/Frontend para filtros.

- GET /api/Account/users/by-status/{estatusId}
  - Qué hace: Lista usuarios por estatus.
  - Quién lo usa: Admin/Operador.

- GET /api/Account/{userId}
  - Qué hace: Obtener detalle de un usuario por id.
  - Quién lo usa: Admin/Operador.

- GET /api/Account/stats
  - Qué hace: Estadísticas de usuarios (activos, nuevos, etc.).
  - Quién lo usa: Admin.

- POST /api/Account/{userId}/unlock
  - Qué hace: Desbloquear cuenta tras bloqueo automático o manual.
  - Quién lo usa: Admin.

- POST /api/Account/{userId}/lock
  - Qué hace: Bloquear cuenta de usuario.
  - Quién lo usa: Admin/Operador.

- POST /api/Account/{userId}/reset-failed-attempts
  - Qué hace: Reiniciar contador de intentos fallidos.
  - Quién lo usa: Admin.

- GET /api/Account/locked-users
  - Qué hace: Lista usuarios bloqueados.
  - Quién lo usa: Admin.

- GET /api/Account/{userId}/lockout-info
  - Qué hace: Info de bloqueo de un usuario (motivo, fecha).
  - Quién lo usa: Admin/Soporte.

- GET /api/Account/users-at-risk
  - Qué hace: Usuarios con señales de fraude o riesgo.
  - Quién lo usa: Seguridad/Operaciones.

- GET /api/Account/confirm-email
  - Qué hace: Confirmar email usando token (link recibido por email).
  - Quién lo usa: Usuario al hacer clic en link de confirmación.

- POST /api/Account/resend-confirmation-email
  - Qué hace: Reenviar email de confirmación.
  - Quién lo usa: Usuarios no confirmados.

- POST /api/Account/confirm-email-direct
  - Qué hace: Confirmar email directamente (método admin/backoffice).
  - Quién lo usa: Soporte/Admin.

- POST /api/Account/admin/confirm-email
  - Qué hace: Confirmar email desde panel admin.
  - Quién lo usa: Admin.

- POST /api/Account/admin/resend-confirmation
  - Qué hace: Reenviar email de confirmación desde admin.
  - Quién lo usa: Admin.

Auditoría
- GET /api/Auditoria
  - Qué hace: Listar eventos de auditoría (paginado/filtrado).
  - Quién lo usa: Admin/Seguridad/Soporte.

- GET /api/Auditoria/{tabla}/{registroId}
  - Qué hace: Obtener historial de cambios para un registro específico.
  - Quién lo usa: Admin/Soporte.

- GET /api/Auditoria/estadisticas
  - Qué hace: Estadísticas agregadas de auditoría.
  - Quién lo usa: Admin/Analítica.

Boletos
- GET /api/Boletos/calcular-precio
  - Qué hace: Calcula precio final aplicando precio por parada, cupones, impuestos y cargos.
  - Quién lo usa: Cliente (antes de comprar), Checkout.
  - Inputs: viajeId, paradaAbordajeId, cuponCode (opcional), cantidad, asiento (opcional)
  - Output: desglose de subtotal, impuestos, descuentos y total.

- POST /api/Boletos
  - Qué hace: Crear/Reservar uno o varios boletos (inicia transacción de compra).
  - Quién lo usa: Cliente (checkout) o Operador (venta manual).
  - Inputs: viajeID, paradaAbordajeID, cuponID (opcional), lista de pasajeros (nombre, email, telefono).
  - Notas: Soporta compra masiva. Asigna asientos secuencialmente de forma automática.

- GET /api/Boletos/{id}
  - Qué hace: Obtener detalle de un boleto (incluye QR, estado, asiento, precio).
  - Quién lo usa: Cliente, Staff (para validar), Admin.

- GET /api/Boletos/me/boletos
  - Qué hace: Listar boletos del usuario autenticado.
  - Quién lo usa: Cliente.

- GET /api/Boletos/verificar/{codigoBoleto}
  - Qué hace: Verificar estado por código (útil en pantallas y soporte).
  - Quién lo usa: Staff, Soporte, Cliente (consulta rápida).

- PUT /api/Boletos/{id}/cancelar
  - Qué hace: Cancelar boleto (según políticas y reglas).
  - Quién lo usa: Cliente (si permitido), Operador, Admin.

- PUT /api/Boletos/{id}/cambiar-asiento
  - Qué hace: Cambiar asignación de asiento.
  - Quién lo usa: Operador/Staff (según permisos).

- POST /api/Boletos/{id}/validar
  - Qué hace: Validar un boleto (escaneo QR) y marcar como usado/abordado.
  - Quién lo usa: Staff en la parada.
  - Notas: Actualiza manifiesto y estado del boleto.

- POST /api/Boletos/validar
  - Qué hace: Validación masiva o por código (soporte/servicios).
  - Quién lo usa: Integraciones/Backoffice.

- POST /api/Boletos/{id}/checkin
  - Qué hace: Registrar checkin puntual del pasajero (marca abordado sin invalidar QR si aplica).
  - Quién lo usa: Staff/Chofer.

CheckInProgresivo
- GET /api/viajes/{viajeId}/checkin/progreso
  - Qué hace: Retorna estado por parada (pendientes, llegadas, validar, completado) con totales.
  - Quién lo usa: Admin, Operador, Chofer, Staff.

- POST /api/viajes/{viajeId}/checkin/confirmar-llegada
  - Qué hace: Chofer confirma llegada a la parada; setea lat/long y timestamp.
  - Quién lo usa: Chofer.
  - Notas: Desencadena notificaciones automáticas a pasajeros y al staff.

- POST /api/viajes/{viajeId}/checkin/iniciar-validacion
  - Qué hace: Staff inicia el proceso de validación en la parada.
  - Quién lo usa: Staff.

- POST /api/viajes/{viajeId}/checkin/finalizar-validacion
  - Qué hace: Staff finaliza validación con totales: abordados, no-shows, incidencias.
  - Quién lo usa: Staff.
  - Notas: Actualiza estado del viaje y notifica al chofer.

Cupones
- GET /api/Cupones
  - Qué hace: Listar cupones disponibles (admin/operador).

- POST /api/Cupones
  - Qué hace: Crear un cupón.
  - Quién lo usa: Admin.

- GET /api/Cupones/{id}
  - Qué hace: Obtener detalle del cupón.

- PUT /api/Cupones/{id}
  - Qué hace: Actualizar cupón.

- DELETE /api/Cupones/{id}
  - Qué hace: Eliminar/desactivar cupón.

- GET /api/Cupones/validar
  - Qué hace: Validar cuponCode contra reglas (vigencia, uso máximo, aplicabilidad).
  - Quién lo usa: Checkout/Front.

Eventos
- GET /api/Eventos
  - Qué hace: Listar eventos.
  - Quién lo usa: Cliente, Admin, Operador.

- POST /api/Eventos
  - Qué hace: Crear evento.
  - Quién lo usa: Admin.

- GET /api/Eventos/{id}
  - Qué hace: Obtener detalle del evento (ubicación, fecha, descripción).

- PUT /api/Eventos/{id}
  - Qué hace: Actualizar evento.
  - Quién lo usa: Admin.

- DELETE /api/Eventos/{id}
  - Qué hace: Eliminar o marcar evento como cancelado.

- GET /api/Eventos/{id}/viajes
  - Qué hace: Listar viajes asociados al evento.
  - Quién lo usa: Cliente para elegir viaje y Admin para gestión.

Incidencias
- GET /api/Incidencias
  - Qué hace: Listar incidencias registradas con filtros avanzados (estatus, prioridad, fecha, búsqueda texto).
  - Quién lo usa: Operador, Admin.
  - Notas: Soporta paginación y ordenamiento dinámico.

- POST /api/Incidencias
  - Qué hace: Reportar una incidencia (en ruta o en parada).
  - Quién lo usa: Staff, Chofer, Cliente (si aplica), Operador.
  - Notas: Genera código único (INC-YYYYMMDD-0001) y crea con estatus "Abierta".

- GET /api/Incidencias/{id}
  - Qué hace: Obtener detalle de una incidencia.

- PUT /api/Incidencias/{id}
  - Qué hace: Actualizar estado/observaciones de una incidencia.
  - **NUEVO**: Envía notificaciones automáticas en segundo plano:
    - Al usuario asignado cuando se le asigna la incidencia.
    - Al reportador cuando la incidencia se marca como Resuelta/Cerrada.
  - Notas: Registra fecha de resolución automáticamente.

- GET /api/Incidencias/estadisticas
  - Qué hace: KPIs de incidencias (por viaje, tipo, tiempo de resolución).

- GET /api/Incidencias/viaje/{viajeId}
  - Qué hace: Incidencias filtradas por viaje.

- GET /api/Incidencias/mis-reportes
  - Qué hace: Incidencias reportadas por el usuario autenticado.

- GET /api/Incidencias/tipos
  - Qué hace: Listar tipos configurables de incidencia.

Me (endpoints para el usuario autenticado)
- GET /api/me/boletos
  - Qué hace: Boletos del usuario actual.
  - Quién lo usa: Cliente.

- GET /api/me/perfil
  - Qué hace: Perfil del usuario actual.
  - Quién lo usa: Cliente.

- PUT /api/me/perfil
  - Qué hace: Actualizar información del perfil (nombre, teléfono, dirección, foto, preferencias de notificaciones).
  - Quién lo usa: Cliente.
  - **NUEVO**: Permite al usuario gestionar su información personal desde la app.

- POST /api/me/cambiar-password
  - Qué hace: Cambiar contraseña del usuario autenticado.
  - Quién lo usa: Cliente.
  - Inputs: currentPassword, newPassword, confirmNewPassword.
  - **NUEVO**: Usa UserManager para cambio seguro de contraseña.

- DELETE /api/me
  - Qué hace: Eliminar cuenta del usuario (soft delete con anonimización).
  - Quién lo usa: Cliente.
  - **NUEVO**: Cumple con requisitos GDPR/App Store para eliminación de cuenta.
  - Notas: Cambia estatus a 0 y anonimiza el email para permitir re-registro.

- GET /api/me/estadisticas
  - Qué hace: Estadísticas personales (viajes activos, compras, total gastado).
  - Quién lo usa: Cliente.

- GET /api/me/notificaciones
  - Qué hace: Notificaciones del usuario con paginación y filtros.
  - Quién lo usa: Cliente.

Notificaciones
- POST /api/Notificaciones/broadcast
  - Qué hace: Enviar notificación masiva a múltiples usuarios.
  - Quién lo usa: Admin/Operaciones.
  - **OPTIMIZADO**: Procesamiento en segundo plano con `Task.Run` y `Parallel.ForEachAsync`.
  - Retorna: 202 Accepted inmediatamente sin bloquear el API.
  - Notas: Usa `IServiceScopeFactory` para manejo seguro de DbContext en background.

- POST /api/Notificaciones
  - Qué hace: Crear una notificación dirigida.

- GET /api/Notificaciones/me
  - Qué hace: Obtener notificaciones del usuario autenticado.

- GET /api/Notificaciones/{id}
  - Qué hace: Obtener detalle de una notificación.

- DELETE /api/Notificaciones/{id}
  - Qué hace: Eliminar notificación.

- PUT /api/Notificaciones/{id}/leer
  - Qué hace: Marcar notificación como leída.

- PUT /api/Notificaciones/marcar-todas-leidas
  - Qué hace: Marcar todas las notificaciones del usuario como leídas.

- GET /api/Notificaciones/no-leidas/count
  - Qué hace: Contador de notificaciones no leídas.

Pagos
- POST /api/Pagos/confirmacion
  - Qué hace: Confirmar pago desde proveedor (webhook simulado o manual).
  - Quién lo usa: Integración de pagos/Operaciones.

- GET /api/Pagos/{codigoPago}
  - Qué hace: Obtener información de pago por código.

- POST /api/Pagos/simular-pago
  - Qué hace: Endpoint de prueba para simular un pago durante desarrollo/pruebas.

- GET /api/Pagos/me/historial
  - Qué hace: Historial de pagos del usuario autenticado.

PreciosParada
- GET /api/viajes/{viajeId}/precios
  - Qué hace: Obtener lista de precios por parada para un viaje.

- GET /api/viajes/{viajeId}/precios/parada/{paradaId}
  - Qué hace: Obtener precio específico de una parada.

- POST /api/viajes/{viajeId}/precios/configurar
  - Qué hace: Configurar precios en lote para paradas del viaje.
  - Quién lo usa: Admin.

- PUT /api/viajes/{viajeId}/precios/{precioId}
  - Qué hace: Actualizar precio de una parada.

- DELETE /api/viajes/{viajeId}/precios/{precioId}
  - Qué hace: Eliminar/desactivar precio.

- POST /api/viajes/{viajeId}/precios/copiar-base
  - Qué hace: Copiar precio base del viaje a todas las paradas como atajo.

Reportes
- GET /api/Reportes/ventas
  - Qué hace: Reporte de ventas por periodo/viaje con agregaciones.
  - Quién lo usa: Admin, Operador.
  - **OPTIMIZADO**: Agregaciones (GroupBy, Sum) ejecutadas en la base de datos (SQL) en lugar de memoria.
  - Notas: Mejora dramática de rendimiento para grandes volúmenes de datos.

- GET /api/Reportes/ocupacion
  - Qué hace: Reporte de ocupación por viaje/parada con porcentajes.
  - Quién lo usa: Admin, Operador.
  - **OPTIMIZADO**: Cálculos de ocupación realizados en BD, solo porcentajes en memoria.
  - Notas: Escalable para miles de viajes simultáneos.

- GET /api/Reportes/dashboard
  - Qué hace: Datos agregados para dashboard operativo (ventas totales, ocupación promedio, ingresos).
  - Quién lo usa: Admin.

Roles
- POST /api/Roles
  - Qué hace: Crear rol.

- GET /api/Roles
  - Qué hace: Listar roles.

- DELETE /api/Roles/{id}
  - Qué hace: Eliminar rol.

- POST /api/Roles/assign
  - Qué hace: Asignar rol a usuario.

Rutas
- GET /api/Rutas
  - Qué hace: Listar plantillas de rutas.

- POST /api/Rutas
  - Qué hace: Crear plantilla de ruta.

- GET /api/Rutas/{id}
  - Qué hace: Obtener detalle de ruta (incluye paradas configuradas).

- DELETE /api/Rutas/{id}
  - Qué hace: Eliminar o desactivar ruta.

- PUT /api/Rutas/{id}/toggle
  - Qué hace: Activar/desactivar ruta.

- GET /api/Rutas/{id}/paradas
  - Qué hace: Listar paradas asociadas a la ruta.

Sincronizacion
- POST /api/Sincronizacion/validaciones
  - Qué hace: Endpoint para sincronizar validaciones externas o dispositivos.

Unidades
- GET /api/Unidades
  - Qué hace: Listar unidades (flota).

- POST /api/Unidades
  - Qué hace: Registrar nueva unidad.

- GET /api/Unidades/{id}
  - Qué hace: Obtener detalle de una unidad (placas, capacidad, layout).

- PUT /api/Unidades/{id}
  - Qué hace: Actualizar datos de la unidad.

- DELETE /api/Unidades/{id}
  - Qué hace: Eliminar/desactivar unidad.

Validación
- POST /api/Validacion
  - Qué hace: Endpoint genérico para validaciones externas (integraciones).

Viajes
- GET /api/Viajes
  - Qué hace: Listar viajes.

- POST /api/Viajes
  - Qué hace: Crear viaje para evento (asignar unidad, chofer, precios base).

- GET /api/Viajes/{id}
  - Qué hace: Obtener detalle del viaje.

- PUT /api/Viajes/{id}
  - Qué hace: Actualizar viaje (horarios, unidad, chofer).

- DELETE /api/Viajes/{id}
  - Qué hace: Eliminar/cancelar viaje.

- GET /api/Viajes/{id}/detalle-cliente
  - Qué hace: Endpoint enriquecido orientado al cliente que devuelve paradas, precios por parada y disponibilidad de asientos.

- GET /api/Viajes/{id}/paradas
  - Qué hace: Listar paradas de ese viaje (orden y ubicación).

- GET /api/Viajes/{id}/manifiesto
  - Qué hace: Obtener manifiesto de pasajeros del viaje (estado, asientos, paradas de abordaje).

- POST /api/Viajes/{id}/staff
  - Qué hace: Asignar staff al viaje.

- GET /api/Viajes/{id}/staff
  - Qué hace: Obtener staff asignado.

- DELETE /api/Viajes/{viajeId}/staff/{asignacionId}
  - Qué hace: Remover asignación de staff.

- GET /api/Viajes/mis-viajes
  - Qué hace: Obtener viajes asociados al usuario autenticado (chofer/staff).

- GET /api/Viajes/verificar-disponibilidad
  - Qué hace: Verificar disponibilidad de chofer/staff/unidad para programar viaje.

ViajeStaff
- POST /api/viajes/{viajeId}/staff
  - Qué hace: Crear asignación de staff (misma que POST /api/Viajes/{id}/staff, puede ser alias).

- GET /api/viajes/{viajeId}/staff
  - Qué hace: Listar staff asignado al viaje.

- PUT /api/viajes/{viajeId}/staff/{asignacionId}
  - Qué hace: Actualizar asignación (roles, horarios).

- DELETE /api/viajes/{viajeId}/staff/{asignacionId}
  - Qué hace: Eliminar asignación de staff.

---

---

## 🎬 Flujos Completos por Rol

Esta sección detalla paso a paso qué hace cada rol en el sistema, desde la configuración inicial hasta la operación del día del viaje.

### 🔧 Flujo del ADMINISTRADOR (Admin)

**Objetivo**: Configurar todo el sistema para que los viajes puedan venderse y operarse correctamente.

#### **FASE 1: Configuración Inicial del Sistema (Una sola vez)**

```
1️⃣ Crear Roles y Usuarios del Sistema
   POST /api/Roles → Crear roles: Admin, Cliente, Staff, Chofer, Operador
   POST /api/Account/register → Crear usuarios operativos (staff, choferes)
   POST /api/Roles/assign → Asignar roles a usuarios

2️⃣ Registrar Unidades (Flota)
   POST /api/Unidades
   Body: {
     "placas": "ABC-123-GDL",
     "modelo": "Mercedes-Benz Sprinter",
     "capacidadAsientos": 40,
     "tipoUnidad": "Autobus",
     "año": 2024
   }
   
3️⃣ Crear Plantillas de Rutas
   POST /api/Rutas
   Body: {
     "nombreRuta": "Guadalajara - CDMX Express",
     "codigoRuta": "GDL-CDMX-001",
     "ciudadOrigen": "Guadalajara",
     "ciudadDestino": "Ciudad de México",
     "distanciaKm": 550,
     "tiempoEstimadoHoras": 7.5
   }
   
   Luego agregar paradas:
   (Las paradas se crean automáticamente al crear el viaje con PlantillaRutaID)
```

#### **FASE 2: Crear un Evento (Por cada concierto/evento)**

```
4️⃣ Crear Evento
   POST /api/Eventos
   Body: {
     "nombre": "Concierto Rock México 2025",
     "descripcion": "Gran concierto en Estadio Azteca",
     "tipoEvento": "Concierto",
     "fecha": "2025-12-25T20:00:00Z",
     "horaInicio": "20:00:00",
     "recinto": "Estadio Azteca",
     "direccion": "Av. Río Churubusco s/n",
     "ciudad": "Ciudad de México",
     "estado": "CDMX",
     "ubicacionLat": 19.303,
     "ubicacionLong": -99.151,
     "urlImagen": "https://example.com/evento.jpg"
   }
   
   ✅ Response: { "eventoID": 1, "nombre": "Concierto Rock..." }
```

#### **FASE 3: Crear Viajes para el Evento**

```
5️⃣ Crear Viaje (Ida)
   POST /api/Viajes
   Body: {
     "eventoID": 1,
     "plantillaRutaID": 1,
     "unidadID": 1,
     "choferID": "chofer-user-id-guid",
     "tipoViaje": "Ida",
     "fechaSalida": "2025-12-25T14:00:00",
     "fechaLlegadaEstimada": "2025-12-25T21:30:00",
     "precioBase": 500.00,
     "cargoServicio": 50.00,
     "ventasAbiertas": true
   }
   
   ✅ Response: { 
     "viajeID": 1, 
     "codigoViaje": "VJE-2025122500001",
     "cupoTotal": 40,
     "asientosDisponibles": 40 
   }
   
   💡 El sistema automáticamente:
      - Copia las paradas de la PlantillaRuta al viaje
      - Crea EstadosParadaViaje en estado "Pendiente"
      - Genera código único VJE-YYYYMMDDXXXXX
```

#### **FASE 4: Configurar Precios por Parada (Precios Dinámicos)**

```
6️⃣ Configurar Precios Diferenciados
   POST /api/viajes/1/precios/configurar
   Body: [
     {
       "paradaViajeID": 1,
       "precioBase": 500.00,
       "cargoServicio": 50.00,
       "observaciones": "Centro Guadalajara - Precio estándar"
     },
     {
       "paradaViajeID": 2,
       "precioBase": 480.00,
       "cargoServicio": 50.00,
       "observaciones": "Zapopan - Más cercano a salida"
     },
     {
       "paradaViajeID": 3,
       "precioBase": 450.00,
       "cargoServicio": 50.00,
       "observaciones": "Tlaquepaque - Descuento por distancia"
     }
   ]
   
   ✅ Ahora cada parada tiene su precio:
      - Parada 1: $550 total
      - Parada 2: $530 total
      - Parada 3: $500 total
```

#### **FASE 5: Asignar Personal al Viaje**

```
7️⃣ Asignar Staff a Paradas
   POST /api/Viajes/1/staff
   Body: {
     "staffID": "staff-user-id-guid",
     "paradaAsignadaID": 1,
     "rol": "Validador",
     "observaciones": "Responsable de Centro GDL"
   }
   
   Repetir para cada parada que necesite staff
```

#### **FASE 6: Verificar Disponibilidad (Antes de crear viajes)**

```
8️⃣ Validar que recursos estén disponibles
   GET /api/Viajes/verificar-disponibilidad?fecha=2025-12-25&choferID=xxx&unidadID=1
   
   ✅ Response: {
     "disponible": true,
     "conflictos": []
   }
   
   ❌ Si hay conflicto: {
     "disponible": false,
     "conflictos": [
       "El chofer ya tiene asignado el viaje VJE-2025122500005",
       "La unidad ya está en uso en el viaje VJE-2025122500007"
     ]
   }
```

#### **FASE 7: Crear Cupones de Descuento (Opcional)**

```
9️⃣ Crear Cupón Promocional
   POST /api/Cupones
   Body: {
     "codigo": "ROCK2025",
     "descripcion": "Descuento especial concierto",
     "tipoDescuento": "Porcentaje",
     "valorDescuento": 10,
     "fechaInicio": "2025-11-01T00:00:00",
     "fechaExpiracion": "2025-12-24T23:59:59",
     "usosMaximos": 100,
     "esActivo": true
   }
```

#### **FASE 8: Monitorear Ventas y Operación**

```
🔟 Dashboard y Reportes
   GET /api/Reportes/dashboard
   → KPIs generales, ventas del día, ocupación
   
   GET /api/Reportes/ventas?fechaDesde=2025-12-01&fechaHasta=2025-12-31
   → Reporte detallado de ventas
   
   GET /api/Reportes/ocupacion?viajeId=1
   → % de ocupación por viaje y por parada
   
   GET /api/Auditoria?tabla=Viajes&registroId=1
   → Ver todos los cambios realizados al viaje
```

---

### 💼 Flujo del OPERADOR

**Objetivo**: Apoyar en la operación diaria, monitorear ventas y atender incidencias.

#### **Tareas Diarias**

```
1️⃣ Consultar Viajes del Día
   GET /api/Viajes?fechaDesde=2025-12-25&fechaHasta=2025-12-25
   
2️⃣ Monitorear Ventas
   GET /api/Reportes/ventas?viajeId=1
   
3️⃣ Ver Estado de Check-in (Día del viaje)
   GET /api/viajes/1/checkin/progreso
   Response: {
     "estadoGeneral": "EnRuta",
     "paradasCompletadas": 2,
     "totalParadas": 3,
     "paradaActual": 3,
     "totalAbordados": 32,
     "totalNoShow": 3
   }
   
4️⃣ Atender Incidencias
   GET /api/Incidencias?viajeId=1
   PUT /api/Incidencias/5 → Actualizar estado de incidencia
   
5️⃣ Re-asignar Staff en Emergencias
   DELETE /api/Viajes/1/staff/3 → Remover staff
   POST /api/Viajes/1/staff → Asignar nuevo staff
```

---

### 👨‍✈️ Flujo del CHOFER

**Objetivo**: Conducir la unidad y confirmar llegada a cada parada para que el staff pueda validar pasajeros.

#### **DÍA DEL VIAJE - Flujo Paso a Paso**

```
ANTES DE SALIR:
1️⃣ Consultar mi viaje del día
   GET /api/Viajes/mis-viajes
   Response: [
     {
       "viajeID": 1,
       "codigoViaje": "VJE-2025122500001",
       "fechaSalida": "2025-12-25T14:00:00",
       "rutaNombre": "Guadalajara - CDMX",
       "unidadPlacas": "ABC-123-GDL",
       "totalParadas": 3,
       "totalPasajeros": 35
     }
   ]

2️⃣ Ver manifiesto de pasajeros
   GET /api/Viajes/1/manifiesto
   Response: {
     "viajeID": 1,
     "totalPasajeros": 35,
     "pasajeros": [
       {
         "nombreCompleto": "Juan Pérez",
         "numeroAsiento": "A12",
         "paradaAbordaje": "Centro Guadalajara",
         "estatusAbordaje": "Pendiente",
         "codigoBoleto": "BOL-00001",
         "qrCode": "data:image/png;base64,..."
       },
       // ... más pasajeros
     ]
   }

EN LA RUTA:
3️⃣ Llego a Parada 1 (Centro Guadalajara - 14:00 hrs)
   POST /api/viajes/1/checkin/confirmar-llegada
   Body: {
     "paradaViajeID": 1,
     "latitud": 20.676682,
     "longitud": -103.346677,
     "observaciones": "Llegada puntual"
   }
   
   ✅ Sistema automáticamente:
      - Marca parada como "Llegado"
      - Registra GPS y hora exacta
      - Envía notificación push a pasajeros: "¡Tu autobús ha llegado! 🚌"
      - Notifica al staff asignado: "El chofer llegó, puedes iniciar validación"
   
4️⃣ ESPERO confirmación del staff
   (El staff escanea QR de cada pasajero)
   
   Recibo notificación: "Validación completada. 12 abordados, 1 no-show. Puedes continuar"
   
5️⃣ Llego a Parada 2 (Zapopan - 14:20 hrs)
   POST /api/viajes/1/checkin/confirmar-llegada
   Body: {
     "paradaViajeID": 2,
     "latitud": 20.720847,
     "longitud": -103.385117
   }
   
   (Repetir proceso...)
   
6️⃣ Llego a Parada 3 (Tlaquepaque - 14:40 hrs)
   POST /api/viajes/1/checkin/confirmar-llegada
   Body: { "paradaViajeID": 3, ... }
   
7️⃣ Todas las paradas completadas → Viaje en tránsito a destino

SI HAY PROBLEMAS:
8️⃣ Reportar Incidencia
   POST /api/Incidencias
   Body: {
     "viajeID": 1,
     "tipoIncidenciaID": 1,
     "descripcion": "Tráfico intenso en autopista",
     "severidad": "Media"
   }
```

---




### 👤 Flujo del CLIENTE (Usuario Final)

**Objetivo**: Comprar boleto, recibir notificaciones y abordar el autobús.

#### **COMPRA DEL BOLETO (Días antes del evento)**

```
1️⃣ Registro en el Sistema (Si es nuevo)
   POST /api/Account/register
   Body: {
     "emailAddress": "cliente@example.com",
     "nombreCompleto": "Ana García",
     "password": "Password123!",
     "tipoDocumento": "INE",
     "numeroDocumento": "1234567890"
   }
   
   ✅ Recibo email de confirmación
   Click en link → GET /api/Account/confirm-email?email=...&token=...
   
2️⃣ Login
   POST /api/Account/login
   Body: {
     "email": "cliente@example.com",
     "password": "Password123!"
   }
   
   ✅ Response: {
     "accessToken": "eyJhbGc...",
     "refreshToken": "abc123...",
     "expiresIn": 3600
   }

3️⃣ Explorar Eventos
   GET /api/Eventos?soloActivos=true
   Response: [
     {
       "eventoID": 1,
       "nombre": "Concierto Rock México 2025",
       "fecha": "2025-12-25T20:00:00",
       "ciudad": "Ciudad de México",
       "urlImagen": "https://..."
     }
   ]

4️⃣ Ver Viajes Disponibles del Evento
   GET /api/Eventos/1/viajes
   Response: [
     {
       "viajeID": 1,
       "codigoViaje": "VJE-2025122500001",
       "tipoViaje": "Ida",
       "ciudadOrigen": "Guadalajara",
       "ciudadDestino": "Ciudad de México",
       "fechaSalida": "2025-12-25T14:00:00",
       "asientosDisponibles": 35,
       "precioBase": 500.00
     }
   ]

5️⃣ Ver TODAS las Paradas con Precios
   GET /api/Viajes/1/detalle-cliente
   Response: {
     "viajeID": 1,
     "codigoViaje": "VJE-2025122500001",
     "evento": { "nombre": "Concierto Rock..." },
     "paradas": [
       {
         "paradaViajeID": 1,
         "nombreParada": "Centro Guadalajara",
         "direccion": "Av. Juárez 123",
         "horaEstimadaLlegada": "14:00:00",
         "precioBase": 500.00,
         "cargoServicio": 50.00,
         "precioTotal": 550.00
       },
       {
         "paradaViajeID": 2,
         "nombreParada": "Zapopan Centro",
         "direccion": "Av. Acueducto 4800",
         "horaEstimadaLlegada": "14:20:00",
         "precioBase": 480.00,
         "cargoServicio": 50.00,
         "precioTotal": 530.00  ← MÁS BARATO
       },
       {
         "paradaViajeID": 3,
         "nombreParada": "Tlaquepaque",
         "precioTotal": 500.00
       }
     ],
     "asientosDisponibles": 35
   }
   
   💡 Decido abordar en Zapopan (más barato)

6️⃣ Calcular Precio Exacto (con cupón)
   GET /api/Boletos/calcular-precio?viajeId=1&paradaAbordajeId=2&cuponId=1
   Response: {
     "precioBase": 480.00,
     "cargoServicio": 50.00,
     "descuento": 53.00,
     "descuentoPorcentaje": 10,
     "subtotal": 477.00,
     "iva": 76.32,
     "precioTotal": 553.32,
     "cuponAplicado": "ROCK2025"
   }

7️⃣ Comprar Boletos (Compra Masiva)
   POST /api/Boletos
   Body: {
     "viajeID": 1,
     "paradaAbordajeID": 2,
     "cuponID": 1,
     "pasajeros": [
       {
         "nombrePasajero": "Ana García",
         "emailPasajero": "ana@email.com",
         "telefonoPasajero": "555-0001"
       },
       {
         "nombrePasajero": "Juan Pérez",
         "emailPasajero": "juan@email.com"
       }
     ]
   }
   
   ✅ Response (Array de boletos): [
     {
       "boletoID": 101,
       "codigoBoleto": "BOL-00101",
       "numeroAsiento": 15,
       "nombrePasajero": "Ana García",
       "precioTotal": 553.32,
       "estatus": "Pendiente"
     },
     {
       "boletoID": 102,
       "codigoBoleto": "BOL-00102",
       "numeroAsiento": 16,
       "nombrePasajero": "Juan Pérez",
       "precioTotal": 553.32,
       "estatus": "Pendiente"
     }
   ]

8️⃣ Confirmar Pago (Webhook o Manual)
   POST /api/Pagos/simular-pago
   Body: {
     "codigoPago": "PAG-00050",
     "monto": 553.32
   }
   
   O esperar webhook:
   POST /api/Pagos/confirmacion
   Body: {
     "codigoPago": "PAG-00050",
     "transaccionID": "TXN-STRIPE-12345",
     "estado": "approved",
     "proveedor": "Stripe"
   }
   
   ✅ Sistema automáticamente:
      - Boleto: "Pendiente" → "Pagado"
      - Decrementa AsientosDisponibles del viaje
      - Crea entrada en ManifiestoPasajeros
      - Incrementa uso del cupón
      - 📧 Envía email: "¡Compra confirmada! Adjunto tu QR"
      - 📱 Push: "Tu boleto BOL-00101 está listo"

9️⃣ Ver Mi Boleto
   GET /api/Boletos/me/boletos
   Response: [
     {
       "boletoID": 101,
       "codigoBoleto": "BOL-00101",
       "qrCode": "data:image/png;base64,...",
       "viaje": {
         "codigoViaje": "VJE-2025122500001",
         "fechaSalida": "2025-12-25T14:00:00"
       },
       "paradaAbordaje": "Zapopan Centro",
       "numeroAsiento": "A15",
       "estatus": "Pagado"
     }
   ]
```

#### **24 HORAS ANTES DEL VIAJE (Recordatorio Automático)**

```
🔔 24 DIC 2025, 14:00 hrs
   Sistema automático (Hangfire Job):
   📱 Push: "Recordatorio: Tu viaje es mañana a las 14:00"
   📧 Email: "No olvides tu viaje - Revisa detalles adjuntos"
```

#### **2 HORAS ANTES DEL VIAJE (Recordatorio Automático)**

```
🔔 25 DIC 2025, 12:00 hrs
   Sistema automático (Hangfire Job):
   📱 Push: "Tu viaje es en 2 horas - Zapopan Centro, 14:20"
```

#### **DÍA DEL VIAJE**

```
🔔 25 DIC 2025, 14:20 hrs (Cuando el chofer llega)
   Chofer ejecuta: POST /api/viajes/1/checkin/confirmar-llegada
   
   ✅ Ana recibe notificación INMEDIATA:
   📱 Push: "¡Tu autobús ha llegado! 🚌"
   "El autobús para tu viaje VJE-2025122500001 ha llegado a Zapopan Centro.
    Dirígete al punto de abordaje y presenta tu QR al staff."

🚌 14:25 hrs - Llego al punto de abordaje
   - Muestro mi QR al staff
   - Staff escanea: POST /api/Boletos/101/validar
   
   ✅ Validación exitosa:
   - Boleto: "Pagado" → "Usado"
   - Manifiesto: "Pendiente" → "Abordado"
   
   📱 "Buen viaje Ana, asiento A15"
   
🎉 ¡Subo al autobús y disfruto el evento!
```

---

## 📋 Casos de Uso Reales

### **Caso 1: Cliente compra con precio dinámico y cupón**

**Contexto**: María quiere ir al concierto desde Tlaquepaque (parada más lejana pero más barata)

```
1. María ve el viaje: GET /api/Viajes/1/detalle-cliente
   - Centro GDL: $550
   - Zapopan: $530
   - Tlaquepaque: $500 ← Elige esta

2. Calcula con cupón "ROCK2025" (10% desc):
   GET /api/Boletos/calcular-precio?viajeId=1&paradaAbordajeId=3&cuponId=1
   
   Cálculo:
   - Precio base: $450
   - Cargo servicio: $50
   - Subtotal: $500
   - Descuento 10%: -$50
   - Subtotal con desc: $450
   - IVA 16%: $72
   - Total final: $522 ✅

3. Compra: POST /api/Boletos
4. Paga: POST /api/Pagos/simular-pago
5. Recibe QR y confirmación
```

---

### **Caso 2: Check-in progresivo con no-show**

**Contexto**: Viaje con 3 paradas, un pasajero no se presenta

```
PARADA 1 (Centro GDL - 14:00):
- Chofer confirma llegada
- 12 pasajeros esperados
- Staff valida 11 QR exitosamente
- Juan Pérez NO se presenta
- Staff finaliza: 11 abordados, 1 no-show
- Sistema libera asiento de Juan Pérez
- Chofer puede continuar

PARADA 2 (Zapopan - 14:20):
- Chofer confirma llegada
- 18 pasajeros esperados
- Staff valida 18 QR (todos presentes)
- Staff finaliza: 18 abordados, 0 no-show
- Chofer continúa

PARADA 3 (Tlaquepaque - 14:40):
- 5 pasajeros esperados
- Staff valida 5 QR
- Staff finaliza: 5 abordados, 0 no-show

RESUMEN DEL VIAJE:
- Total esperados: 35
- Total abordados: 34
- Total no-show: 1
- Estado: "Completado"
```

---

### **Caso 3: Incidencia reportada en ruta**

**Contexto**: Autobús tiene problema mecánico menor

```
1. Chofer reporta:
   POST /api/Incidencias
   Body: {
     "viajeID": 1,
     "tipoIncidenciaID": 2,
     "descripcion": "Aire acondicionado no funciona correctamente",
     "severidad": "Media"
   }

2. Operador recibe alerta:
   GET /api/Incidencias?viajeId=1
   
3. Operador actualiza:
   PUT /api/Incidencias/10
   Body: {
     "estadoIncidencia": "En Atención",
     "resolucion": "Se coordinó revisión en llegada"
   }

4. Al finalizar viaje:
   PUT /api/Incidencias/10
   Body: {
     "estadoIncidencia": "Resuelta",
     "resolucion": "Reparado por mecánico al arribar"
   }
```

---

### **Caso 4: Admin cancela y re-programa viaje**

**Contexto**: Unidad tuvo falla mecánica, se asigna otra

```
1. Verificar disponibilidad de otra unidad:
   GET /api/Viajes/verificar-disponibilidad?fecha=2025-12-25&unidadID=2

2. Actualizar viaje:
   PUT /api/Viajes/1
   Body: {
     "unidadID": 2
   }

3. Notificar a pasajeros del cambio:
   POST /api/Notificaciones/broadcast
   Body: {
     "viajeID": 1,
     "titulo": "Cambio de Unidad",
     "mensaje": "Por motivos técnicos, tu viaje será en unidad ABC-456-GDL",
     "tipoNotificacion": "Importante",
     "enviarPush": true,
     "enviarEmail": true
   }

4. Auditoría registra automáticamente:
   GET /api/Auditoria?tabla=Viajes&registroId=1
   Response: [
     {
       "accion": "UPDATE",
       "campo": "UnidadID",
       "valorAnterior": "1",
       "valorNuevo": "2",
       "usuario": "admin@bustix.com",
       "fecha": "2025-12-24T10:30:00"
     }
   ]
```

---

## 🔒 Seguridad y Permisos

### **Sistema de Permisos (Claims-Based)**

El sistema utiliza **Claims** de ASP.NET Identity para controlar acceso granular:

```csharp
// Permisos definidos en AppPermissions.cs
public static class AppPermissions
{
    public static class Eventos
    {
        public const string View = "Eventos.View";
        public const string Create = "Eventos.Create";
        public const string Edit = "Eventos.Edit";
        public const string Delete = "Eventos.Delete";
    }
    
    public static class Viajes
    {
        public const string View = "Viajes.View";
        public const string Create = "Viajes.Create";
        public const string Edit = "Viajes.Edit";
        public const string Delete = "Viajes.Delete";
        public const string Validate = "Viajes.Validate"; // Chofer y Staff
    }
    
    public static class Boletos
    {
        public const string View = "Boletos.View";
        public const string Create = "Boletos.Create";
        public const string Cancel = "Boletos.Cancel";
        public const string Validate = "Boletos.Validate"; // Staff
    }
}
```

### **Matriz de Permisos por Rol**

| Permiso | Admin | Operador | Staff | Chofer | Cliente |
|---------|-------|----------|-------|--------|---------|
| Eventos.Create | ✅ | ❌ | ❌ | ❌ | ❌ |
| Eventos.View | ✅ | ✅ | ❌ | ❌ | ✅ |
| Viajes.Create | ✅ | ❌ | ❌ | ❌ | ❌ |
| Viajes.Edit | ✅ | ✅ | ❌ | ❌ | ❌ |
| Viajes.Validate | ✅ | ❌ | ✅ | ✅ | ❌ |
| Boletos.Create | ✅ | ✅ | ❌ | ❌ | ✅ |
| Boletos.Validate | ✅ | ❌ | ✅ | ❌ | ❌ |
| Reportes.View | ✅ | ✅ | ❌ | ❌ | ❌ |
| Account.Manage | ✅ | ❌ | ❌ | ❌ | ❌ |

### **Validaciones de Seguridad Implementadas**

```
✅ Autenticación JWT obligatoria (excepto endpoints públicos)
✅ Solo el chofer asignado puede confirmar llegadas
✅ Solo el staff asignado puede validar en su parada
✅ Clientes solo ven sus propios boletos
✅ Transacciones atómicas en compra de boletos (evita oversell)
✅ Validación de disponibilidad de recursos (chofer/unidad/staff)
✅ Rate limiting en endpoints públicos
✅ Auditoría automática de cambios críticos
✅ Encriptación de tokens de confirmación de email
✅ Bloqueo automático tras X intentos fallidos de login
✅ Tokens de refresh con expiración y revocación
```
|--------|--------------|---------|--------|
| 🎫 Compra Confirmada | Cliente | Push + Email | Al confirmar pago |
| ⏰ Recordatorio 24h | Cliente | Push + Email | 24h antes del viaje |
| ⏰ Recordatorio 2h | Cliente | Push | 2h antes del viaje |
| 🚌 Chofer Llegó | Clientes de la parada | Push + SignalR | Al confirmar llegada |
| ✅ Validación Completa | Chofer | Push + SignalR | Al finalizar validación |
| 🚨 Incidencia Reportada | Admin + Operadores | Push + Email | Al crear incidencia |
| 🔄 Cambio de Unidad/Horario | Clientes del viaje | Push + Email | Al actualizar viaje |
| 📊 Viaje Completado | Admin + Operador | Email | Al finalizar todas las paradas |

### **Ejemplos de Implementación**

```csharp
// En PagosController.cs - Al confirmar pago
await _notificacionService.EnviarConfirmacionCompraAsync(boleto.BoletoID);

// En CheckInProgresivoController.cs - Al confirmar llegada
await _notificacionService.NotificarLlegadaChoferParadaAsync(viajeId, paradaViajeId);

// En NotificacionesController.cs - Broadcast
POST /api/Notificaciones/broadcast
Body: {
  "viajeID": 1,
  "titulo": "Actualización Importante",
  "mensaje": "...",
  "enviarPush": true,
  "enviarEmail": true
}
```

---

## 📚 Referencia Rápida de APIs

### **Endpoints Más Usados por Rol**

#### **👤 Cliente**
```
POST   /api/Account/register
POST   /api/Account/login
GET    /api/Eventos?soloActivos=true
GET    /api/Eventos/{id}/viajes
GET    /api/Viajes/{id}/detalle-cliente
GET    /api/Boletos/calcular-precio
POST   /api/Boletos
POST   /api/Pagos/simular-pago
GET    /api/Boletos/me/boletos
GET    /api/me/perfil
GET    /api/me/notificaciones
```

#### **👨‍✈️ Chofer**
```
POST   /api/Account/login
GET    /api/Viajes/mis-viajes
GET    /api/Viajes/{id}/manifiesto
POST   /api/viajes/{id}/checkin/confirmar-llegada
GET    /api/viajes/{id}/checkin/progreso
POST   /api/Incidencias
```

#### **👮 Staff**
```
POST   /api/Account/login
GET    /api/Viajes/mis-viajes
POST   /api/viajes/{id}/checkin/iniciar-validacion
POST   /api/Boletos/{id}/validar
POST   /api/viajes/{id}/checkin/finalizar-validacion
POST   /api/Incidencias
```

#### **💼 Operador**
```
GET    /api/Viajes?fechaDesde=...
GET    /api/viajes/{id}/checkin/progreso
GET    /api/Reportes/ventas
GET    /api/Reportes/ocupacion
GET    /api/Incidencias
PUT    /api/Incidencias/{id}
POST   /api/Notificaciones/broadcast
```

#### **🔧 Admin**
```
POST   /api/Eventos
POST   /api/Rutas
POST   /api/Unidades
POST   /api/Viajes
POST   /api/viajes/{id}/precios/configurar
POST   /api/Viajes/{id}/staff
GET    /api/Viajes/verificar-disponibilidad
POST   /api/Cupones
GET    /api/Reportes/dashboard
GET    /api/Auditoria
POST   /api/Roles
POST   /api/Roles/assign
POST   /api/Account/{userId}/lock
```

---

## 📊 Estadísticas del Sistema

```
Total de Endpoints: 109
Total de Módulos: 19
Total de Roles: 5
Notificaciones Automáticas: 8 tipos
Permisos Granulares: 20+
```

---

## ✅ Resumen de Funcionalidades Clave

1. ✅ **Precios Dinámicos**: Cada parada puede tener su propio precio configurado
2. ✅ **Check-in Progresivo**: Control detallado del abordaje por parada
3. ✅ **Notificaciones Automáticas**: 8 eventos diferentes con multi-canal
4. ✅ **QR Automático**: Generado al confirmar pago
5. ✅ **Manifiesto Automático**: Creado al pagar boleto
6. ✅ **Validación de Recursos**: Evita conflictos de chofer/unidad
7. ✅ **Auditoría Completa**: Rastrea todos los cambios
8. ✅ **Reportes en Tiempo Real**: Dashboard y KPIs
9. ✅ **Gestión de Incidencias**: Reporte y seguimiento
10. ✅ **Sistema de Cupones**: Descuentos por porcentaje o monto fijo

---

## 🎯 Conclusión

Este documento cubre el 100% de los endpoints y flujos del sistema BusTix. Cada rol tiene responsabilidades claras y endpoints específicos que permiten una operación fluida desde la configuración inicial hasta el día del viaje.

**Próximos pasos sugeridos:**
- Implementar autenticación de dos factores (2FA)
- Agregar geolocalización en tiempo real del autobús
- Dashboard en tiempo real con SignalR
- Reportes avanzados con filtros personalizados
- Integración con pasarelas de pago reales (Stripe, PayPal)

---

**Documento creado**: 2025-01-19  
**Última actualización**: 2025-01-19  
**Versión**: 2.0 - Completa y Detallada  
**Autor**: Equipo BusTix
