-- Script para agregar mejoras a las tablas de ASP.NET Identity
-- BusTixDB_IDGS1004
-- Este script es IDEMPOTENTE (se puede ejecutar múltiples veces sin romper nada)

USE [BusTixDB_IDGS1004];
GO

-- =============================================
-- 1. CREAR TABLA ESTATUS_GENERAL (Catálogo)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Estatus_General]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Estatus_General] (
        [Id_Estatus] INT PRIMARY KEY IDENTITY(1,1),
        [Nombre_Estatus] NVARCHAR(100) NOT NULL
    );
    
    PRINT 'Tabla Estatus_General creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Tabla Estatus_General ya existe.';
END
GO

-- =============================================
-- 2. INSERTAR DATOS SEMILLA EN ESTATUS_GENERAL
-- =============================================
IF NOT EXISTS (SELECT 1 FROM [dbo].[Estatus_General] WHERE [Id_Estatus] = 1)
BEGIN
    SET IDENTITY_INSERT [dbo].[Estatus_General] ON;
    
    INSERT INTO [dbo].[Estatus_General] ([Id_Estatus], [Nombre_Estatus])
    VALUES 
        (1, 'Activo'),
        (2, 'Inactivo'),
        (3, 'Validado'),
        (4, 'Pendiente'),
        (5, 'Cancelado'),
        (6, 'Suspendido'),
        (7, 'Bloqueado');
    
    SET IDENTITY_INSERT [dbo].[Estatus_General] OFF;
    
    PRINT 'Datos semilla insertados en Estatus_General.';
END
ELSE
BEGIN
    PRINT 'Datos semilla ya existen en Estatus_General.';
END
GO

-- =============================================
-- 3. AGREGAR COLUMNAS A AspNetUsers (si no existen)
-- =============================================

-- Columna: Estatus (FK a Estatus_General)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'Estatus')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [Estatus] INT NOT NULL DEFAULT 1;
    
    PRINT 'Columna Estatus agregada a AspNetUsers.';
END
ELSE
BEGIN
    PRINT 'Columna Estatus ya existe en AspNetUsers.';
END
GO

-- Columna: FechaRegistro
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'FechaRegistro')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [FechaRegistro] DATETIME NOT NULL DEFAULT GETDATE();
    
    PRINT 'Columna FechaRegistro agregada a AspNetUsers.';
END
ELSE
BEGIN
    PRINT 'Columna FechaRegistro ya existe en AspNetUsers.';
END
GO

-- Columna: TipoDocumento
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'TipoDocumento')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [TipoDocumento] NVARCHAR(50) NULL;
    
    PRINT 'Columna TipoDocumento agregada a AspNetUsers.';
END
ELSE
BEGIN
    PRINT 'Columna TipoDocumento ya existe en AspNetUsers.';
END
GO

-- Columna: NumeroDocumento
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'NumeroDocumento')
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD [NumeroDocumento] NVARCHAR(50) NULL;
    
    PRINT 'Columna NumeroDocumento agregada a AspNetUsers.';
END
ELSE
BEGIN
    PRINT 'Columna NumeroDocumento ya existe en AspNetUsers.';
END
GO

-- =============================================
-- 4. CREAR FOREIGN KEY (solo si no existe)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AspNetUsers_Estatus_General]'))
BEGIN
    ALTER TABLE [dbo].[AspNetUsers]
    ADD CONSTRAINT [FK_AspNetUsers_Estatus_General] 
    FOREIGN KEY ([Estatus]) 
    REFERENCES [dbo].[Estatus_General] ([Id_Estatus])
    ON DELETE NO ACTION;
    
    PRINT 'Foreign Key FK_AspNetUsers_Estatus_General creada correctamente.';
END
ELSE
BEGIN
    PRINT 'Foreign Key FK_AspNetUsers_Estatus_General ya existe.';
END
GO

-- =============================================
-- 5. CREAR ÍNDICES PARA MEJORAR RENDIMIENTO
-- =============================================

-- Índice en Estatus para consultas rápidas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IX_AspNetUsers_Estatus')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Estatus]
    ON [dbo].[AspNetUsers] ([Estatus])
    INCLUDE ([Email], [FullName]);
    
    PRINT 'Índice IX_AspNetUsers_Estatus creado correctamente.';
END
ELSE
BEGIN
    PRINT 'Índice IX_AspNetUsers_Estatus ya existe.';
END
GO

-- Índice en FechaRegistro para reportes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IX_AspNetUsers_FechaRegistro')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_FechaRegistro]
    ON [dbo].[AspNetUsers] ([FechaRegistro] DESC);
    
    PRINT 'Índice IX_AspNetUsers_FechaRegistro creado correctamente.';
END
ELSE
BEGIN
    PRINT 'Índice IX_AspNetUsers_FechaRegistro ya existe.';
END
GO

-- Índice en NumeroDocumento para búsquedas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'IX_AspNetUsers_NumeroDocumento')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_NumeroDocumento]
    ON [dbo].[AspNetUsers] ([NumeroDocumento])
    WHERE [NumeroDocumento] IS NOT NULL;
    
    PRINT 'Índice IX_AspNetUsers_NumeroDocumento creado correctamente.';
END
ELSE
BEGIN
    PRINT 'Índice IX_AspNetUsers_NumeroDocumento ya existe.';
END
GO

-- =============================================
-- 6. VERIFICACIÓN FINAL
-- =============================================
PRINT '==========================================';
PRINT 'VERIFICACIÓN DE CAMBIOS APLICADOS:';
PRINT '==========================================';

-- Verificar tabla Estatus_General
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Estatus_General]'))
    PRINT '✓ Tabla Estatus_General: OK';
ELSE
    PRINT '✗ Tabla Estatus_General: FALTA';

-- Verificar columnas en AspNetUsers
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'Estatus')
    PRINT '✓ Columna Estatus: OK';
ELSE
    PRINT '✗ Columna Estatus: FALTA';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'FechaRegistro')
    PRINT '✓ Columna FechaRegistro: OK';
ELSE
    PRINT '✗ Columna FechaRegistro: FALTA';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'TipoDocumento')
    PRINT '✓ Columna TipoDocumento: OK';
ELSE
    PRINT '✗ Columna TipoDocumento: FALTA';

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND name = 'NumeroDocumento')
    PRINT '✓ Columna NumeroDocumento: OK';
ELSE
    PRINT '✗ Columna NumeroDocumento: FALTA';

-- Verificar FK
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AspNetUsers_Estatus_General]'))
    PRINT '✓ Foreign Key FK_AspNetUsers_Estatus_General: OK';
ELSE
    PRINT '✗ Foreign Key FK_AspNetUsers_Estatus_General: FALTA';

-- Contar usuarios existentes
DECLARE @UserCount INT;
SELECT @UserCount = COUNT(*) FROM [dbo].[AspNetUsers];
PRINT '==========================================';
PRINT 'Total de usuarios en la base de datos: ' + CAST(@UserCount AS NVARCHAR(10));
PRINT '==========================================';

-- Mostrar datos de estatus
SELECT * FROM [dbo].[Estatus_General];

PRINT '';
PRINT 'Script completado exitosamente.';
PRINT 'Todos los usuarios existentes conservan sus datos.';
GO

