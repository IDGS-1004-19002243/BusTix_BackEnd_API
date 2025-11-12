# 📋 DOCUMENTACIÓN - SEMANA 1 COMPLETADA

## ✅ Endpoints Implementados (Críticos - Semana 1)

### 1️⃣ **Validación de Boletos (Staff)**

#### **POST** `/api/boletos/validar`
**Descripción**: Validar un boleto escaneando su código QR (online)  
**Autorización**: Roles: `Admin`, `Staff`, `Manager`  
**Request Body**:
```json
{
  "viajeID": 1,
  "codigoQR": "base64string...",
  "tipoValidacion": "EscaneoQR",
  "estacionLat": 19.432608,
  "estacionLong": -99.133209,
  "observaciones": "Validación normal",
  "deviceValidationId": "unique-device-id-123"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Boleto validado correctamente",
  "validacionID": 1,
  "resultado": "Aprobado",
  "fechaHoraValidacion": "2025-01-12T10:30:00",
  "boletoID": 5,
  "clienteNombre": "Juan Pérez",
  "asientoAsignado": "A12",
  "estadoBoleto": "Usado"
}
```

**Response Error (200 con success: false)**:
```json
{
  "success": false,
  "message": "Este boleto ya fue validado el 12/01/2025 10:30",
  "resultado": "Rechazado",
  "boletoID": 5
}
```

**Características**:
- ✅ Validación online en tiempo real
- ✅ Idempotencia con `deviceValidationId`
- ✅ Geolocalización de la validación
- ✅ Actualiza estado del boleto y manifiesto
- ✅ Registra en histórico de validaciones

---

### 2️⃣ **Sincronización de Validaciones Offline**

#### **POST** `/api/sincronizacion/validaciones`
**Descripción**: Sincronizar batch de validaciones realizadas offline  
**Autorización**: Roles: `Admin`, `Staff`, `Manager`  
**Request Body**:
```json
[
  {
    "boletoID": 5,
    "viajeID": 1,
    "fechaHoraValidacion": "2025-01-12T10:30:00",
    "resultado": "Aprobado",
    "tipoValidacion": "EscaneoQR",
    "estacionLat": 19.432608,
    "estacionLong": -99.133209,
    "observaciones": "Validación offline",
    "deviceValidationId": "unique-device-id-123",
    "deviceId": "android-device-456"
  },
  {
    "boletoID": 6,
    "viajeID": 1,
    "fechaHoraValidacion": "2025-01-12T10:31:00",
    "resultado": "Aprobado",
    "tipoValidacion": "EscaneoQR",
    "deviceValidationId": "unique-device-id-124"
  }
]
```

**Response (200)**:
```json
{
  "success": true,
  "message": "Sincronización completada exitosamente",
  "totalRecibidas": 2,
  "procesadas": 2,
  "fallidas": 0,
  "duplicadas": 0,
  "errores": []
}
```

**Response con errores**:
```json
{
  "success": false,
  "message": "Sincronización completada con 1 errores",
  "totalRecibidas": 3,
  "procesadas": 2,
  "fallidas": 1,
  "duplicadas": 0,
  "errores": [
    {
      "deviceValidationId": "unique-device-id-125",
      "boletoID": 7,
      "error": "Boleto no encontrado"
    }
  ]
}
```

**Características**:
- ✅ Procesa múltiples validaciones en batch
- ✅ Idempotencia total (evita duplicados)
- ✅ Manejo de errores individuales
- ✅ Actualiza boletos y manifiestos
- ✅ Marca validaciones como "offline"

---

### 3️⃣ **Webhook de Confirmación de Pagos**

#### **POST** `/api/pagos/confirmacion`
**Descripción**: Endpoint para recibir confirmación de pago desde pasarela  
**Autorización**: `AllowAnonymous` (para webhooks externos)  
**Request Body**:
```json
{
  "codigoPago": "PAG-20250112-ABC123",
  "transaccionID": "txn_1234567890",
  "estado": "approved",
  "proveedor": "Stripe",
  "montoConfirmado": 350.00
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Pago confirmado exitosamente",
  "codigoPago": "PAG-20250112-ABC123",
  "transaccionId": "txn_1234567890",
  "boletos": [
    {
      "boletoId": 5,
      "codigoBoleto": "BOL-20250112-XYZ789",
      "estatus": "Pagado"
    }
  ]
}
```

**Características**:
- ✅ Confirma pago y actualiza estado
- ✅ Genera código QR automáticamente
- ✅ Crea entrada en `ManifiestoPasajeros`
- ✅ Actualiza inventario de asientos
- ✅ Envía notificación de confirmación al cliente
- ✅ Libera asientos si el pago es rechazado

---

### 4️⃣ **Manifiesto de Pasajeros**

#### **GET** `/api/viajes/{id}/manifiesto`
**Descripción**: Obtener el manifiesto completo de pasajeros de un viaje  
**Autorización**: Roles: `Admin`, `Staff`, `Manager`, `Chofer`  
**Parámetros URL**: `id` (ViajeID)

**Response (200)**:
```json
{
  "viajeID": 1,
  "codigoViaje": "VIA-20250112-001",
  "fechaSalida": "2025-01-15T08:00:00",
  "totalPasajeros": 45,
  "pasajerosAbordados": 30,
  "pasajerosPendientes": 13,
  "pasajerosNoAsistieron": 2,
  "pasajeros": [
    {
      "boletoID": 5,
      "codigoQR": "base64string...",
      "clienteID": "user-123",
      "clienteNombre": "Juan Pérez",
      "clienteEmail": "juan@example.com",
      "clienteTelefono": "+525512345678",
      "asientoAsignado": "A12",
      "estadoBoleto": "Pagado",
      "estadoAbordaje": "Abordado",
      "fechaValidacion": "2025-01-15T07:45:00",
      "validadoPor": "staff-456"
    },
    {
      "boletoID": 6,
      "codigoQR": "base64string2...",
      "clienteID": "user-124",
      "clienteNombre": "María González",
      "clienteEmail": "maria@example.com",
      "asientoAsignado": "A13",
      "estadoBoleto": "Pagado",
      "estadoAbordaje": "Pendiente",
      "fechaValidacion": null,
      "validadoPor": null
    }
  ]
}
```

**Características**:
- ✅ Lista completa de pasajeros con boletos pagados
- ✅ Información de contacto de cada pasajero
- ✅ Estado de abordaje actualizado
- ✅ Estadísticas de ocupación
- ✅ Ordenado por número de asiento
- ✅ Ideal para descarga offline en dispositivos

---

## 🗄️ **Cambios en Base de Datos**

### Migración: `AgregarCamposValidacion`

**Campos agregados a `RegistroValidacion`**:
- `TipoValidacion` (string, 50) - Ej: "EscaneoQR", "Manual"
- `EstacionLat` (decimal nullable) - Latitud donde se validó
- `EstacionLong` (decimal nullable) - Longitud donde se validó
- `Observaciones` (string, 1000 nullable) - Notas adicionales
- `DeviceValidationId` (string, 50 nullable) - ID único para idempotencia

**Para aplicar**:
```bash
dotnet ef database update
```

---

## 📦 **DTOs Creados**

### Nuevos archivos en `/Dto/Boletos/`:
1. `ValidacionDto.cs` - Request para validación online
2. `ValidacionSyncDto.cs` - Request para sincronización batch
3. `ValidacionResponseDto.cs` - Response de validación
4. `SincronizacionResponseDto.cs` - Response de sincronización

### Nuevos archivos en `/Dto/Viajes/`:
1. `ManifiestoResponseDto.cs` - Response de manifiesto con lista de pasajeros
2. `PasajeroManifiestoDto.cs` - Información de cada pasajero

---

## 🎯 **Flujo de Trabajo Completo**

### **Escenario 1: Validación Online** (con conexión)
1. Staff escanea QR del boleto
2. App envía `POST /api/boletos/validar`
3. Backend valida, actualiza estado y manifiesto
4. Response inmediato al staff
5. Boleto marcado como "Usado"

### **Escenario 2: Validación Offline** (sin conexión)
1. Staff descarga manifiesto: `GET /api/viajes/{id}/manifiesto`
2. App guarda manifiesto en SQLite local
3. Staff escanea QR (validación contra BD local)
4. App guarda validación en cola local con `deviceValidationId` único
5. Cuando hay conexión: `POST /api/sincronizacion/validaciones`
6. Backend procesa batch y actualiza todo

### **Escenario 3: Flujo de Compra Completo**
1. Cliente inicia compra: `POST /api/boletos`
2. Backend crea boleto "Pendiente" y pago "Pendiente"
3. Cliente paga en pasarela externa
4. Pasarela llama: `POST /api/pagos/confirmacion`
5. Backend confirma pago, genera QR, crea manifiesto
6. Cliente recibe notificación con su boleto QR

---

## 🧪 **Testing**

### Test 1: Validar boleto online
```bash
curl -X POST http://localhost:5289/api/boletos/validar \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "viajeID": 1,
    "codigoQR": "ABC123...",
    "deviceValidationId": "test-001"
  }'
```

### Test 2: Sincronizar validaciones
```bash
curl -X POST http://localhost:5289/api/sincronizacion/validaciones \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '[
    {
      "boletoID": 5,
      "viajeID": 1,
      "fechaHoraValidacion": "2025-01-12T10:30:00",
      "resultado": "Aprobado",
      "deviceValidationId": "test-002"
    }
  ]'
```

### Test 3: Obtener manifiesto
```bash
curl -X GET http://localhost:5289/api/viajes/1/manifiesto \
  -H "Authorization: Bearer {token}"
```

### Test 4: Webhook de pago
```bash
curl -X POST http://localhost:5289/api/pagos/confirmacion \
  -H "Content-Type: application/json" \
  -d '{
    "codigoPago": "PAG-20250112-ABC123",
    "transaccionID": "txn_123",
    "estado": "approved",
    "proveedor": "Stripe"
  }'
```

---

## ✅ **Checklist Semana 1 - COMPLETADO**

- [x] Endpoint de validación de boletos (`POST /api/boletos/validar`)
- [x] Endpoint de sincronización batch (`POST /api/sincronizacion/validaciones`)
- [x] Webhook de confirmación de pago (`POST /api/pagos/confirmacion`)
- [x] Endpoint de manifiesto (`GET /api/viajes/{id}/manifiesto`)
- [x] Modelo `RegistroValidacion` actualizado
- [x] DTOs creados y validados
- [x] Migración de base de datos generada
- [x] Idempotencia implementada
- [x] Geolocalización de validaciones
- [x] Actualización automática de manifiestos
- [x] Notificaciones automáticas de confirmación

---

## 🚀 **Próximos Pasos (Semana 2)**

1. Asignación de staff a viajes
2. Validación de disponibilidad de choferes/unidades
3. Endpoints de reportes básicos
4. Cambio de asiento y check-in

---

## 📞 **Soporte**

Para probar estos endpoints:
1. Asegúrate de tener un usuario con rol `Staff` o `Admin`
2. Obtén un token JWT válido de `/api/account/login`
3. Usa el token en el header `Authorization: Bearer {token}`
4. Ejecuta las migraciones: `dotnet ef database update`
5. La aplicación debe estar corriendo: `dotnet run`

**¡SEMANA 1 COMPLETADA EXITOSAMENTE! 🎉**

