**Incidencias**

- **Resumen:** El módulo de Incidencias ya existe en backend. Provee endpoints para crear, listar, actualizar y obtener estadísticas.
- **Endpoints principales:**
  - `POST /api/incidencias` : crear incidencia (Staff)
  - `GET /api/incidencias/viaje/{viajeId}` : incidencias de un viaje
  - `GET /api/incidencias/mis-reportes` : incidencias reportadas por el usuario actual
  - `GET /api/incidencias/tipos` : catálogo de tipos de incidencia
  - `PUT /api/incidencias/{id}` : actualizar (administrador/staff con permisos)

- **Recomendaciones para la app móvil (Staff):**
  - Usar `POST /api/incidencias` con body `{ TipoIncidenciaID, ViajeID?, UnidadID?, Titulo, Descripcion, Prioridad }`.
  - Adjuntar localización y fotos puede añadirse en futuras iteraciones.
