# Estructura de Barra Lateral - BusTix

## Roles del Sistema

El sistema BusTix maneja 5 roles principales con diferentes niveles de acceso:

| Rol | Descripción | Nivel de Acceso |
|-----|-------------|-----------------|
| **Admin** | Administrador del sistema | Acceso completo a todos los módulos |
| **Manager** | Gerente/Supervisor | Acceso a gestión operativa y financiera |
| **Staff** | Personal de apoyo | Acceso a operaciones básicas y asignadas |
| **Operator** | Operador de unidades | Acceso a viajes asignados y validaciones |
| **User** | Cliente/Usuario final | Acceso a perfil personal y boletos |

---

## Estructura de Navegación

### 📊 Dashboard y Notificaciones
**Acceso:** Todos los roles autenticados

```
├─ Dashboard
│  └─ Endpoint: N/A (Vista general del sistema)
│  └─ Roles: Admin, Manager, Staff, Operator, User
│
└─ Notificaciones
   └─ Endpoints:
      • GET /api/Notificaciones/me
      • GET /api/Notificaciones/no-leidas/count
      • PUT /api/Notificaciones/{id}/leer
      • PUT /api/Notificaciones/marcar-todas-leidas
   └─ Roles: Admin, Manager, Staff, Operator, User
```

---

### 🎉 Eventos Masivos
**Acceso:** Admin, Manager, Operator (limitado)

```
├─ Gestión de Eventos
│  └─ Endpoints:
│     • GET /api/Eventos
│     • POST /api/Eventos
│     • GET /api/Eventos/{id}
│     • PUT /api/Eventos/{id}
│     • DELETE /api/Eventos/{id}
│     • GET /api/Eventos/{id}/viajes
│  └─ Roles: Admin, Manager
│
├─ Rutas Públicas
│  └─ Endpoints:
│     • GET /api/Rutas
│     • POST /api/Rutas
│     • GET /api/Rutas/{id}
│     • PUT /api/Rutas/{id}/toggle
│     • GET /api/Rutas/{id}/paradas
│     • DELETE /api/Rutas/{id}
│  └─ Roles: Admin, Manager, Operator (solo lectura)
│
├─ Boletos Vendidos
│  └─ Endpoints:
│     • GET /api/Boletos/{id}
│     • PUT /api/Boletos/{id}/cancelar
│     • PUT /api/Boletos/{id}/cambiar-asiento
│     • GET /api/Boletos/verificar/{codigoBoleto}
│  └─ Roles: Admin, Manager
│
└─ Estadísticas de Eventos
   └─ Endpoints:
      • GET /api/Reportes/ventas
      • GET /api/Reportes/ocupacion
      • GET /api/Reportes/dashboard
   └─ Roles: Admin, Manager
```

---

### 🚐 Viajes y Operaciones
**Acceso:** Admin, Manager, Operator, Staff (según asignación)

```
├─ Gestión de Viajes
│  └─ Endpoints:
│     • GET /api/Viajes
│     • POST /api/Viajes
│     • GET /api/Viajes/{id}
│     • PUT /api/Viajes/{id}
│     • DELETE /api/Viajes/{id}
│     • GET /api/Viajes/{id}/detalle-cliente
│     • GET /api/Viajes/{id}/paradas
│     • GET /api/Viajes/{id}/manifiesto
│     • GET /api/Viajes/verificar-disponibilidad
│  └─ Roles: Admin, Manager, Operator (viajes asignados)
│
├─ Mis Viajes Asignados
│  └─ Endpoints:
│     • GET /api/Viajes/mis-viajes
│     • GET /api/Viajes/{id}/staff
│  └─ Roles: Operator, Staff
│
├─ Asignación de Staff
│  └─ Endpoints:
│     • POST /api/viajes/{viajeId}/staff
│     • GET /api/viajes/{viajeId}/staff
│     • PUT /api/viajes/{viajeId}/staff/{asignacionId}
│     • DELETE /api/viajes/{viajeId}/staff/{asignacionId}
│  └─ Roles: Admin, Manager
│
├─ Check-In Progresivo
│  └─ Endpoints:
│     • GET /api/viajes/{viajeId}/checkin/progreso
│     • POST /api/viajes/{viajeId}/checkin/confirmar-llegada
│     • POST /api/viajes/{viajeId}/checkin/iniciar-validacion
│     • POST /api/viajes/{viajeId}/checkin/finalizar-validacion
│  └─ Roles: Operator, Staff
│
└─ Validación de Boletos
   └─ Endpoints:
      • POST /api/Boletos/{id}/validar
      • POST /api/Boletos/validar
      • POST /api/Boletos/{id}/checkin
      • POST /api/Validacion
      • POST /api/Sincronizacion/validaciones
   └─ Roles: Operator, Staff
```

---

### 🚌 Recursos Operativos
**Acceso:** Admin, Manager, Operator (limitado)

```
├─ Flota de Vehículos
│  └─ Endpoints:
│     • GET /api/Unidades
│     • POST /api/Unidades
│     • GET /api/Unidades/{id}
│     • PUT /api/Unidades/{id}
│     • DELETE /api/Unidades/{id}
│  └─ Roles: Admin, Manager, Operator (solo lectura)
│
├─ Staff y Operadores
│  ├─ Lista de Operadores
│  │  └─ Endpoints:
│  │     • GET /api/Account
│  │     • GET /api/Account/{userId}
│  │     • PUT /api/Account/{userId}/status
│  │     • GET /api/Account/users/by-status/{estatusId}
│  │  └─ Roles: Admin, Manager
│  │
│  └─ Gestión de Accesos
│     └─ Endpoints:
│        • POST /api/Account/{userId}/lock
│        • POST /api/Account/{userId}/unlock
│        • POST /api/Account/{userId}/reset-failed-attempts
│        • GET /api/Account/locked-users
│        • GET /api/Account/{userId}/lockout-info
│        • GET /api/Account/users-at-risk
│     └─ Roles: Admin
│
├─ Incidencias
│  └─ Endpoints:
│     • GET /api/Incidencias
│     • POST /api/Incidencias
│     • GET /api/Incidencias/{id}
│     • PUT /api/Incidencias/{id}
│     • GET /api/Incidencias/estadisticas
│     • GET /api/Incidencias/viaje/{viajeId}
│     • GET /api/Incidencias/mis-reportes
│     • GET /api/Incidencias/tipos
│  └─ Roles: 
│     • Crear: Operator, Staff
│     • Ver todas: Admin, Manager
│     • Ver propias: Operator, Staff
│
└─ Calendario General
   └─ Endpoints: N/A (Vista de calendario integrado)
   └─ Roles: Admin, Manager, Operator
```

---

### 💰 Finanzas
**Acceso:** Admin, Manager

```
├─ Ingresos y Ventas
│  └─ Endpoints:
│     • GET /api/Reportes/ventas
│     • GET /api/Reportes/dashboard
│     • GET /api/Account/stats
│  └─ Roles: Admin, Manager
│
├─ Pagos
│  └─ Endpoints:
│     • POST /api/Pagos/confirmacion
│     • GET /api/Pagos/{codigoPago}
│     • POST /api/Pagos/simular-pago
│     • GET /api/Pagos/me/historial (User también)
│  └─ Roles: Admin, Manager, User (solo historial propio)
│
├─ Cupones de Descuento
│  └─ Endpoints:
│     • GET /api/Cupones
│     • POST /api/Cupones
│     • GET /api/Cupones/{id}
│     • PUT /api/Cupones/{id}
│     • DELETE /api/Cupones/{id}
│     • GET /api/Cupones/validar
│  └─ Roles: Admin, Manager
│
└─ Reportes Financieros
   └─ Endpoints:
      • GET /api/Reportes/ventas
      • GET /api/Reportes/ocupacion
      • GET /api/Reportes/dashboard
   └─ Roles: Admin, Manager
```

---

### 🎟️ Precios y Tarifas
**Acceso:** Admin, Manager

```
└─ Configuración de Precios por Parada
   └─ Endpoints:
      • GET /api/viajes/{viajeId}/precios
      • GET /api/viajes/{viajeId}/precios/parada/{paradaId}
      • POST /api/viajes/{viajeId}/precios/configurar
      • PUT /api/viajes/{viajeId}/precios/{precioId}
      • DELETE /api/viajes/{viajeId}/precios/{precioId}
      • POST /api/viajes/{viajeId}/precios/copiar-base
      • GET /api/Boletos/calcular-precio
   └─ Roles: Admin, Manager
```

---

### ⚙️ Configuración y Administración
**Acceso:** Admin (principalmente)

```
├─ Gestión de Usuarios
│  ├─ Lista de Usuarios
│  │  └─ Endpoints:
│  │     • GET /api/Account
│  │     • GET /api/Account/{userId}
│  │     • PUT /api/Account/update-profile
│  │     • PUT /api/Account/{userId}/status
│  │     • GET /api/Account/statuses
│  │     • GET /api/Account/stats
│  │  └─ Roles: Admin
│  │
│  └─ Roles y Permisos
│     └─ Endpoints:
│        • GET /api/Roles
│        • POST /api/Roles
│        • DELETE /api/Roles/{id}
│        • POST /api/Roles/assign
│        • GET /api/Account/permissions
│     └─ Roles: Admin
│
├─ Confirmación de Email
│  └─ Endpoints:
│     • GET /api/Account/confirm-email
│     • POST /api/Account/resend-confirmation-email
│     • POST /api/Account/confirm-email-direct
│     • POST /api/Account/admin/confirm-email
│     • POST /api/Account/admin/resend-confirmation
│  └─ Roles: Admin (admin endpoints), User (propios)
│
├─ Auditoría
│  └─ Endpoints:
│     • GET /api/Auditoria
│     • GET /api/Auditoria/{tabla}/{registroId}
│     • GET /api/Auditoria/estadisticas
│  └─ Roles: Admin
│
└─ Configuración General
   └─ Endpoints: N/A (Configuraciones del sistema)
   └─ Roles: Admin
```

---

### 👤 Mi Perfil (User/Me)
**Acceso:** Todos los usuarios autenticados

```
├─ Mi Perfil
│  └─ Endpoints:
│     • GET /api/me/perfil
│     • PUT /api/me/perfil
│     • GET /api/me/estadisticas
│  └─ Roles: Todos
│
├─ Mis Boletos
│  └─ Endpoints:
│     • GET /api/me/boletos
│     • GET /api/Boletos/me/boletos
│  └─ Roles: Todos (principalmente User)
│
├─ Mis Notificaciones
│  └─ Endpoints:
│     • GET /api/me/notificaciones
│     • GET /api/Notificaciones/me
│  └─ Roles: Todos
│
├─ Cambiar Contraseña
│  └─ Endpoints:
│     • POST /api/me/cambiar-password
│     • POST /api/Account/change-password
│  └─ Roles: Todos
│
└─ Eliminar Cuenta
   └─ Endpoints:
      • DELETE /api/me
   └─ Roles: Todos
```

---

### 🔐 Autenticación (No en Sidebar)
**Acceso:** Público / No autenticado

```
└─ Endpoints de Autenticación:
   • POST /api/Account/register
   • POST /api/Account/login
   • POST /api/Account/logout
   • POST /api/Account/forgot-password
   • POST /api/Account/reset-password
   • POST /api/Account/refresh-token
   • POST /api/Account/revoke-token/{userId}
```

---

## Matriz de Permisos por Rol

| Módulo | Admin | Manager | Staff | Operator | User |
|--------|-------|---------|-------|----------|------|
| Dashboard | ✅ | ✅ | ✅ | ✅ | ✅ |
| Notificaciones | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Eventos Masivos** |
| Gestión de Eventos | ✅ | ✅ | ❌ | ❌ | ❌ |
| Rutas Públicas | ✅ | ✅ | ❌ | 👁️ | ❌ |
| Boletos Vendidos | ✅ | ✅ | ❌ | ❌ | ❌ |
| Estadísticas Eventos | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Viajes y Operaciones** |
| Gestión de Viajes | ✅ | ✅ | ❌ | 👁️ | ❌ |
| Mis Viajes Asignados | ❌ | ❌ | ✅ | ✅ | ❌ |
| Asignación de Staff | ✅ | ✅ | ❌ | ❌ | ❌ |
| Check-In Progresivo | ❌ | ❌ | ✅ | ✅ | ❌ |
| Validación de Boletos | ❌ | ❌ | ✅ | ✅ | ❌ |
| **Recursos Operativos** |
| Flota de Vehículos | ✅ | ✅ | ❌ | 👁️ | ❌ |
| Staff y Operadores | ✅ | ✅ | ❌ | ❌ | ❌ |
| Incidencias | ✅ | ✅ | ✅ | ✅ | ❌ |
| Calendario General | ✅ | ✅ | ❌ | ✅ | ❌ |
| **Finanzas** |
| Ingresos y Ventas | ✅ | ✅ | ❌ | ❌ | ❌ |
| Pagos | ✅ | ✅ | ❌ | ❌ | 👁️ |
| Cupones | ✅ | ✅ | ❌ | ❌ | ❌ |
| Reportes Financieros | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Precios y Tarifas** |
| Configuración Precios | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Configuración** |
| Gestión de Usuarios | ✅ | ❌ | ❌ | ❌ | ❌ |
| Roles y Permisos | ✅ | ❌ | ❌ | ❌ | ❌ |
| Auditoría | ✅ | ❌ | ❌ | ❌ | ❌ |
| Configuración General | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Mi Perfil** |
| Mi Perfil | ✅ | ✅ | ✅ | ✅ | ✅ |
| Mis Boletos | ✅ | ✅ | ✅ | ✅ | ✅ |
| Cambiar Contraseña | ✅ | ✅ | ✅ | ✅ | ✅ |

**Leyenda:**
- ✅ = Acceso completo (lectura y escritura)
- 👁️ = Solo lectura
- ❌ = Sin acceso

---

## Notas de Implementación

### Jerarquía de Roles
```
Admin > Manager > Staff / Operator > User
```

### Consideraciones Especiales

1. **Operator y Staff:**
   - Solo pueden ver viajes asignados a ellos
   - Pueden reportar incidencias en sus viajes
   - Pueden realizar validaciones de boletos
   - Tienen acceso al check-in progresivo

2. **Manager:**
   - Puede gestionar eventos, viajes y recursos
   - Tiene acceso a reportes financieros
   - No puede modificar roles ni permisos
   - No tiene acceso a auditoría del sistema

3. **User:**
   - Solo acceso a su perfil y boletos
   - Puede ver historial de pagos propios
   - Recibe notificaciones de sus viajes

4. **Admin:**
   - Acceso total al sistema
   - Único rol que puede gestionar usuarios y roles
   - Acceso a auditoría completa
   - Puede desbloquear usuarios y gestionar seguridad

### Endpoints Compartidos

Algunos endpoints tienen diferentes niveles de acceso según el rol:
- `/api/Incidencias`: Admin/Manager ven todas, Operator/Staff solo las propias
- `/api/Viajes`: Admin/Manager ven todos, Operator solo asignados
- `/api/Pagos/me/historial`: Todos pueden ver su propio historial

---

## Estructura Visual Propuesta para Sidebar

```
📊 Dashboard
🔔 Notificaciones (badge con contador)

─────────────────────────────
🎉 EVENTOS MASIVOS (Admin, Manager)
├─ 📅 Gestión de Eventos
├─ 🗺️ Rutas Públicas
├─ 🎫 Boletos Vendidos
└─ 📈 Estadísticas

─────────────────────────────
🚐 VIAJES Y OPERACIONES
├─ 🚌 Gestión de Viajes (Admin, Manager)
├─ 📋 Mis Viajes (Operator, Staff)
├─ 👥 Asignación de Staff (Admin, Manager)
├─ ✅ Check-In Progresivo (Operator, Staff)
└─ 🎟️ Validación de Boletos (Operator, Staff)

─────────────────────────────
🚌 RECURSOS OPERATIVOS
├─ 🚐 Flota de Vehículos
├─ 👨‍✈️ Staff y Operadores (Admin, Manager)
├─ ⚠️ Incidencias
└─ 📅 Calendario General

─────────────────────────────
💰 FINANZAS (Admin, Manager)
├─ 💵 Ingresos y Ventas
├─ 💳 Pagos
├─ 🎟️ Cupones
└─ 📊 Reportes Financieros

─────────────────────────────
🎯 PRECIOS Y TARIFAS (Admin, Manager)
└─ 💲 Configuración de Precios

─────────────────────────────
⚙️ CONFIGURACIÓN (Admin)
├─ 👥 Gestión de Usuarios
│  ├─ Lista de Usuarios
│  └─ Roles y Permisos
├─ 📝 Auditoría
└─ ⚙️ Configuración General

─────────────────────────────
👤 MI PERFIL (Todos)
├─ 👤 Mi Perfil
├─ 🎫 Mis Boletos
├─ 🔔 Mis Notificaciones
└─ 🔐 Cambiar Contraseña
```

---

## Recomendaciones

1. **Implementar middleware de autorización** que valide el rol antes de permitir acceso a cada endpoint
2. **Usar políticas de autorización** en ASP.NET Core para cada módulo
3. **Implementar filtros dinámicos** para que Operator/Staff solo vean sus datos asignados
4. **Cachear permisos** del usuario en el token JWT para evitar consultas constantes
5. **Logging de accesos** especialmente para módulos sensibles (Finanzas, Auditoría, Gestión de Usuarios)
6. **UI dinámica** que oculte/muestre módulos según el rol del usuario autenticado
