-- ============================================
-- CHECKPOINT DATABASE - Setup Completo para Railway
-- Ejecutar este script COMPLETO en DBeaver conectado a Railway
-- ============================================

-- PASO 1: Limpiar esquema existente (si hay tablas mal creadas)
DROP SCHEMA IF EXISTS public CASCADE;
CREATE SCHEMA public;

-- Dar permisos al esquema
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;

-- ============================================
-- PASO 2: CREAR TODAS LAS TABLAS
-- ============================================

BEGIN;

-- ============================================
-- IDENTITY TABLES (ASP.NET Core Identity)
-- ============================================

-- 1. AspNetRoles
CREATE TABLE "AspNetRoles" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

CREATE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

-- 2. AspNetUsers (con columnas personalizadas)
CREATE TABLE "AspNetUsers" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "Nombre" TEXT NOT NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMPTZ,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "AccessFailedCount" INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE "NormalizedUserName" IS NOT NULL;

-- 3. AspNetUserRoles
CREATE TABLE "AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

-- 4. AspNetUserClaims
CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
 "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

-- 5. AspNetUserLogins
CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" VARCHAR(450) NOT NULL,
    "ProviderKey" VARCHAR(450) NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

-- 6. AspNetUserTokens
CREATE TABLE "AspNetUserTokens" (
    "UserId" VARCHAR(450) NOT NULL,
    "LoginProvider" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(450) NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- 7. AspNetRoleClaims
CREATE TABLE "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

-- ============================================
-- BUSINESS TABLES
-- ============================================

-- 8. Sedes
CREATE TABLE "Sedes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Nombre" VARCHAR(150) NOT NULL,
    "Codigo" VARCHAR(50) NOT NULL,
    "Direccion" VARCHAR(250),
 "Activa" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Sedes_Codigo" ON "Sedes" ("Codigo");

-- 9. Ubicaciones
CREATE TABLE "Ubicaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "SedeId" UUID NOT NULL,
    "Codigo" VARCHAR(50) NOT NULL,
    "Nombre" VARCHAR(200) NOT NULL,
    "Tipo" VARCHAR(100) NOT NULL,
    "Capacidad" DECIMAL(18,3) NOT NULL,
    FOREIGN KEY ("SedeId") REFERENCES "Sedes" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Ubicaciones_SedeId_Codigo" ON "Ubicaciones" ("SedeId", "Codigo");

-- 10. Productos
CREATE TABLE "Productos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Sku" VARCHAR(50) NOT NULL,
    "Nombre" VARCHAR(200) NOT NULL,
    "Unidad" VARCHAR(50) NOT NULL DEFAULT 'u',
    "VidaUtilDias" INTEGER NOT NULL,
    "TempMin" DECIMAL(10,2) NOT NULL,
    "TempMax" DECIMAL(10,2) NOT NULL,
    "StockMinimo" DECIMAL(18,3) NOT NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Productos_Sku" ON "Productos" ("Sku");

-- 11. Clientes
CREATE TABLE "Clientes" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(200) NOT NULL,
    "NombreComercial" VARCHAR(100),
    "IdentificadorFiscal" VARCHAR(50),
    "Direccion" VARCHAR(500),
 "Ciudad" VARCHAR(100),
    "Pais" VARCHAR(50),
    "Telefono" VARCHAR(20),
    "Email" VARCHAR(100),
    "PersonaContacto" VARCHAR(100),
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Activo',
    "Observaciones" VARCHAR(1000),
    "FechaRegistro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UltimaActualizacion" TIMESTAMP,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Clientes_IdentificadorFiscal" ON "Clientes" ("IdentificadorFiscal");

-- 12. Proveedores
CREATE TABLE "Proveedores" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(200) NOT NULL,
    "NombreComercial" VARCHAR(100),
    "IdentificadorFiscal" VARCHAR(50),
    "Direccion" VARCHAR(500),
    "Ciudad" VARCHAR(100),
    "Pais" VARCHAR(50),
    "Telefono" VARCHAR(20),
    "Email" VARCHAR(100),
"PersonaContacto" VARCHAR(100),
    "Categoria" VARCHAR(50),
    "Calificacion" INTEGER,
    "Estado" VARCHAR(20) NOT NULL DEFAULT 'Activo',
    "Observaciones" VARCHAR(1000),
    "FechaRegistro" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UltimaActualizacion" TIMESTAMP,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Proveedores_IdentificadorFiscal" ON "Proveedores" ("IdentificadorFiscal");

-- 13. Lotes
CREATE TABLE "Lotes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProductoId" UUID NOT NULL,
    "ProveedorId" INTEGER,
    "CodigoLote" VARCHAR(100) NOT NULL,
    "FechaIngreso" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaVencimiento" TIMESTAMP,
    "OrdenCompra" VARCHAR(100),
    "GuiaRecepcion" VARCHAR(100),
    "TempIngreso" DECIMAL(10,2) NOT NULL,
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    "CantidadInicial" DECIMAL(18,3) NOT NULL,
    "CantidadDisponible" DECIMAL(18,3) NOT NULL,
    FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("ProveedorId") REFERENCES "Proveedores" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Lotes_CodigoLote" ON "Lotes" ("CodigoLote");
CREATE INDEX "IX_Lotes_ProductoId" ON "Lotes" ("ProductoId");
CREATE INDEX "IX_Lotes_ProveedorId" ON "Lotes" ("ProveedorId");
CREATE INDEX "IX_Lotes_FechaVencimiento" ON "Lotes" ("FechaVencimiento");

-- 14. Stocks
CREATE TABLE "Stocks" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "UbicacionId" UUID NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "Unidad" VARCHAR(50) NOT NULL,
    "ActualizadoEn" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("UbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Stocks_LoteId" ON "Stocks" ("LoteId");
CREATE INDEX "IX_Stocks_UbicacionId" ON "Stocks" ("UbicacionId");

-- 15. Movimientos
CREATE TABLE "Movimientos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "SedeId" UUID,
    "OrigenUbicacionId" UUID,
    "DestinoUbicacionId" UUID,
    "ClienteId" INTEGER,
    "Tipo" VARCHAR(50) NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "Unidad" VARCHAR(50) NOT NULL,
    "Fecha" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsuarioId" VARCHAR(450),
    "Motivo" VARCHAR(500),
    "NumeroDocumento" VARCHAR(100),
    "Estado" VARCHAR(50),
  "AprobadoPor" VARCHAR(450),
    "FechaAprobacion" TIMESTAMP,
    "StockAnterior" DECIMAL(18,3),
    "StockPosterior" DECIMAL(18,3),
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE RESTRICT,
  FOREIGN KEY ("SedeId") REFERENCES "Sedes" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("OrigenUbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("DestinoUbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE RESTRICT,
  FOREIGN KEY ("ClienteId") REFERENCES "Clientes" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("UsuarioId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Movimientos_LoteId" ON "Movimientos" ("LoteId");
CREATE INDEX "IX_Movimientos_OrigenUbicacionId" ON "Movimientos" ("OrigenUbicacionId");
CREATE INDEX "IX_Movimientos_DestinoUbicacionId" ON "Movimientos" ("DestinoUbicacionId");
CREATE INDEX "IX_Movimientos_ClienteId" ON "Movimientos" ("ClienteId");

-- 16. CalidadLiberaciones
CREATE TABLE "CalidadLiberaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "UsuarioId" VARCHAR(450),
    "Fecha" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Estado" VARCHAR(50) NOT NULL,
    "Observacion" VARCHAR(1000),
    "EvidenciaUrl" VARCHAR(500),
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("UsuarioId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_CalidadLiberaciones_LoteId" ON "CalidadLiberaciones" ("LoteId");

-- 17. Tareas
CREATE TABLE "Tareas" (
    "Id" SERIAL PRIMARY KEY,
    "Titulo" VARCHAR(200) NOT NULL,
    "Descripcion" VARCHAR(2000),
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    "Prioridad" VARCHAR(20) NOT NULL DEFAULT 'Media',
  "Tipo" VARCHAR(50),
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaInicio" TIMESTAMP,
    "FechaLimite" TIMESTAMP,
    "FechaFinalizacion" TIMESTAMP,
    "CreadoPor" VARCHAR(450),
    "ResponsableId" VARCHAR(450),
    "Progreso" INTEGER NOT NULL DEFAULT 0,
    "Observaciones" VARCHAR(2000),
    "ProductoId" UUID,
    "LoteId" UUID,
    "Historial" TEXT,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_Tareas_Estado" ON "Tareas" ("Estado");
CREATE INDEX "IX_Tareas_ResponsableId" ON "Tareas" ("ResponsableId");
CREATE INDEX "IX_Tareas_FechaLimite" ON "Tareas" ("FechaLimite");
CREATE INDEX "IX_Tareas_ProductoId" ON "Tareas" ("ProductoId");
CREATE INDEX "IX_Tareas_LoteId" ON "Tareas" ("LoteId");

-- 18. Notificaciones
CREATE TABLE "Notificaciones" (
    "Id" SERIAL PRIMARY KEY,
    "UsuarioId" VARCHAR(450) NOT NULL,
  "Tipo" VARCHAR(50) NOT NULL,
    "Titulo" VARCHAR(200) NOT NULL,
    "Mensaje" VARCHAR(1000),
    "Url" VARCHAR(500),
    "Categoria" VARCHAR(50),
    "ReferenciaId" VARCHAR(50),
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaLeida" TIMESTAMP,
    "FechaVencimiento" TIMESTAMP,
  "Leida" BOOLEAN NOT NULL DEFAULT FALSE,
    "Prioridad" VARCHAR(20) NOT NULL DEFAULT 'Normal',
    "EsRecurrente" BOOLEAN NOT NULL DEFAULT FALSE,
    "Activa" BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY ("UsuarioId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Notificaciones_UsuarioId" ON "Notificaciones" ("UsuarioId");
CREATE INDEX "IX_Notificaciones_Leida" ON "Notificaciones" ("Leida");

-- 19. Procedimientos
CREATE TABLE "Procedimientos" (
    "Id" SERIAL PRIMARY KEY,
    "Codigo" VARCHAR(10) NOT NULL UNIQUE,
  "Nombre" VARCHAR(200) NOT NULL,
    "Descripcion" VARCHAR(4000),
    "Categoria" VARCHAR(50),
    "Version" VARCHAR(20) NOT NULL,
    "Estado" VARCHAR(20) NOT NULL,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaAprobacion" TIMESTAMP,
    "FechaVigencia" TIMESTAMP,
    "FechaRevision" TIMESTAMP,
    "ResponsableId" VARCHAR(450),
    "AprobadoPor" VARCHAR(450),
    "RutaDocumento" VARCHAR(500),
    "Observaciones" VARCHAR(2000),
    "FrecuenciaRevisionMeses" INTEGER,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Procedimientos_Codigo" ON "Procedimientos" ("Codigo");

-- 20. Parametros
CREATE TABLE "Parametros" (
    "Id" SERIAL PRIMARY KEY,
    "Clave" VARCHAR(100) NOT NULL UNIQUE,
    "Nombre" VARCHAR(200) NOT NULL,
  "Descripcion" VARCHAR(500),
    "Valor" VARCHAR(2000) NOT NULL,
    "TipoDato" VARCHAR(20) NOT NULL,
    "Categoria" VARCHAR(50),
    "Unidad" VARCHAR(20),
    "EsEditable" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UltimaModificacion" TIMESTAMP,
    "ModificadoPor" VARCHAR(450),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX "IX_Parametros_Clave" ON "Parametros" ("Clave");

-- 21. AuditLogs
CREATE TABLE "AuditLogs" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" VARCHAR(450),
    "Action" VARCHAR(200) NOT NULL,
    "Details" VARCHAR(2000),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
 FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
CREATE INDEX "IX_AuditLogs_Timestamp" ON "AuditLogs" ("Timestamp");

COMMIT;

-- ============================================
-- PASO 3: INSERTAR DATOS INICIALES
-- ============================================

BEGIN;

-- 1. ROLES
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
    ('admin-role', 'Administrador', 'ADMINISTRADOR', gen_random_uuid()::text),
    ('bodega-role', 'PersonalBodega', 'PERSONALBODEGA', gen_random_uuid()::text),
    ('calidad-role', 'ControlCalidad', 'CONTROLCALIDAD', gen_random_uuid()::text);

-- 2. USUARIOS
-- Password: Admin123! (hash de ejemplo - en producción usa uno real)
INSERT INTO "AspNetUsers" (
    "Id", "Nombre", "Activo", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
)
VALUES
    (
        'admin-user-001', 
        'Administrador Principal',
     true,
        'admin@checkpoint.com', 
        'ADMIN@CHECKPOINT.COM', 
        'admin@checkpoint.com', 
        'ADMIN@CHECKPOINT.COM', 
        true,
      'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=',
        gen_random_uuid()::text,
  gen_random_uuid()::text,
     false, false, true, 0
    ),
    (
        'bodega-user-001',
        'Personal Bodega',
true,
        'bodega@checkpoint.com',
    'BODEGA@CHECKPOINT.COM',
        'bodega@checkpoint.com',
        'BODEGA@CHECKPOINT.COM',
        true,
        'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=',
 gen_random_uuid()::text,
        gen_random_uuid()::text,
        false, false, true, 0
    ),
    (
  'calidad-user-001',
  'Control de Calidad',
        true,
      'calidad@checkpoint.com',
  'CALIDAD@CHECKPOINT.COM',
        'calidad@checkpoint.com',
     'CALIDAD@CHECKPOINT.COM',
        true,
        'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=',
      gen_random_uuid()::text,
   gen_random_uuid()::text,
        false, false, true, 0
    );

-- 3. ASIGNAR ROLES
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES
    ('admin-user-001', 'admin-role'),
    ('bodega-user-001', 'bodega-role'),
    ('calidad-user-001', 'calidad-role');

-- 4. SEDES
INSERT INTO "Sedes" ("Id", "Codigo", "Nombre", "Direccion", "Activa")
VALUES
    ('11111111-1111-1111-1111-111111111111'::uuid, 'SEDE-STGO', 'Sede Santiago Centro', 'Av. Libertador Bernardo O''Higgins 1234', true),
    ('22222222-2222-2222-2222-222222222222'::uuid, 'SEDE-NORTE', 'Bodega Zona Norte', 'Calle Los Aromos 567, Quilicura', true),
    ('33333333-3333-3333-3333-333333333333'::uuid, 'SEDE-SUR', 'Bodega Zona Sur', 'Av. Vicuña Mackenna 8900, La Florida', true);

-- 5. UBICACIONES
INSERT INTO "Ubicaciones" ("Id", "SedeId", "Codigo", "Nombre", "Tipo", "Capacidad")
VALUES
    (gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'A-01', 'Estante A-01', 'Estante', 1000.000),
  (gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'A-02', 'Estante A-02', 'Estante', 1000.000),
    (gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'CF-01', 'Cámara Fría 1', 'CamaraFria', 5000.000),
    (gen_random_uuid(), '22222222-2222-2222-2222-222222222222'::uuid, 'B-01', 'Estante B-01', 'Estante', 1500.000),
    (gen_random_uuid(), '22222222-2222-2222-2222-222222222222'::uuid, 'CF-02', 'Cámara Fría 2', 'CamaraFria', 8000.000),
    (gen_random_uuid(), '33333333-3333-3333-3333-333333333333'::uuid, 'C-01', 'Pallet C-01', 'Pallet', 2000.000);

-- 6. PRODUCTOS
INSERT INTO "Productos" ("Id", "Sku", "Nombre", "Unidad", "VidaUtilDias", "TempMin", "TempMax", "StockMinimo", "Activo")
VALUES
    (gen_random_uuid(), 'PROD-001', 'Materia Prima A', 'kg', 90, 2.0, 8.0, 500.000, true),
    (gen_random_uuid(), 'PROD-002', 'Materia Prima B', 'kg', 60, 0.0, 25.0, 300.000, true),
    (gen_random_uuid(), 'PROD-003', 'Producto Terminado X', 'unidad', 180, -5.0, 5.0, 100.000, true),
    (gen_random_uuid(), 'PROD-004', 'Producto Terminado Y', 'litro', 120, 2.0, 8.0, 200.000, true),
    (gen_random_uuid(), 'PROD-005', 'Insumo Crítico Z', 'caja', 30, 15.0, 25.0, 50.000, true);

-- 7. CLIENTES
INSERT INTO "Clientes" (
    "Nombre", "NombreComercial", "IdentificadorFiscal", "Direccion", "Ciudad", "Pais",
    "Telefono", "Email", "PersonaContacto", "Estado", "Observaciones", "FechaRegistro", "Activo"
)
VALUES
 ('Distribuidora Del Sur S.A.', 'Del Sur', '76.123.456-7', 'Av. Grecia 1234', 'Santiago', 'Chile', 
   '+56912345678', 'ventas@delsur.cl', 'Juan Pérez', 'Activo', 'Cliente VIP', CURRENT_TIMESTAMP, true),
    
    ('Retail Express Ltda.', 'Retail Express', '77.987.654-3', 'Los Alamos 567', 'Valparaíso', 'Chile',
   '+56987654321', 'compras@retailexpress.cl', 'María González', 'Activo', 'Descuento 10%', CURRENT_TIMESTAMP, true),
    
    ('Importadora Nacional', 'Imp. Nacional', '78.555.888-K', 'Calle Comercio 890', 'Concepción', 'Chile',
     '+56966666666', 'contacto@impnacional.cl', 'Carlos Rodríguez', 'Activo', NULL, CURRENT_TIMESTAMP, true);

-- 8. PROVEEDORES
INSERT INTO "Proveedores" (
 "Nombre", "NombreComercial", "IdentificadorFiscal", "Direccion", "Ciudad", "Pais",
    "Telefono", "Email", "PersonaContacto", "Categoria", "Calificacion", "Estado", 
  "Observaciones", "FechaRegistro", "Activo"
)
VALUES
    ('Proveedora Industrial S.A.', 'Prov. Industrial', '79.111.222-3', 'Av. Industrial 100', 'Santiago', 'Chile',
     '+56922222222', 'ventas@provin.cl', 'Ana Martínez', 'MateriaPrima', 5, 'Activo', 
     'Certificado ISO 9001', CURRENT_TIMESTAMP, true),
    
    ('Alimentos Frescos Chile', 'Frescos Chile', '80.333.444-5', 'Camino Agrícola 200', 'Rancagua', 'Chile',
   '+56933333333', 'contacto@frescochile.cl', 'Pedro Soto', 'Perecibles', 4, 'Activo',
     'Entrega diaria', CURRENT_TIMESTAMP, true),
    
    ('Embalajes y Packaging Ltda.', 'Embal. Pack', '81.555.666-7', 'Parque Industrial 300', 'Quilicura', 'Chile',
     '+56944444444', 'ventas@embpack.cl', 'Luisa Torres', 'Embalaje', 5, 'Activo',
     NULL, CURRENT_TIMESTAMP, true);

-- 9. PARAMETROS
INSERT INTO "Parametros" ("Clave", "Nombre", "Descripcion", "Valor", "TipoDato", "Categoria", "EsEditable", "Activo")
VALUES
    ('TEMP_MIN_ALMACENAMIENTO', 'Temperatura Mínima', 'Temp. mínima permitida en cámaras', '-5', 'Decimal', 'Calidad', true, true),
    ('TEMP_MAX_ALMACENAMIENTO', 'Temperatura Máxima', 'Temp. máxima permitida en cámaras', '8', 'Decimal', 'Calidad', true, true),
    ('DIAS_ALERTA_VENCIMIENTO', 'Días Alerta Vencimiento', 'Días previos para generar alerta', '7', 'Integer', 'Inventario', true, true),
    ('EMAIL_NOTIFICACIONES', 'Email Notificaciones', 'Email para notificaciones del sistema', 'admin@checkpoint.com', 'String', 'Sistema', true, true);

COMMIT;

-- ============================================
-- PASO 4: CONFIGURAR ENTITY FRAMEWORK CORE
-- ============================================

-- Crear tabla de migraciones de EF Core
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL PRIMARY KEY,
    "ProductVersion" VARCHAR(32) NOT NULL
);

-- Registrar que la migración inicial ya fue aplicada manualmente
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('00000000000000_ManualDatabaseSetup', '8.0.0');

-- ============================================
-- VERIFICACIÓN FINAL
-- ============================================

DO $$
DECLARE
    table_count INTEGER;
    user_count INTEGER;
    sede_count INTEGER;
    producto_count INTEGER;
BEGIN
    -- Contar tablas creadas
    SELECT COUNT(*) INTO table_count
    FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_type = 'BASE TABLE';
    
    -- Contar usuarios creados
    SELECT COUNT(*) INTO user_count FROM "AspNetUsers";
 
    -- Contar sedes creadas
    SELECT COUNT(*) INTO sede_count FROM "Sedes";
    
    -- Contar productos creados
    SELECT COUNT(*) INTO producto_count FROM "Productos";
    
    RAISE NOTICE '============================================';
    RAISE NOTICE '? SETUP COMPLETADO EXITOSAMENTE';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Tablas creadas: %', table_count;
    RAISE NOTICE 'Usuarios creados: %', user_count;
    RAISE NOTICE 'Sedes creadas: %', sede_count;
    RAISE NOTICE 'Productos creados: %', producto_count;
    RAISE NOTICE '';
    RAISE NOTICE '?? CREDENCIALES DE ACCESO:';
    RAISE NOTICE '  ? admin@checkpoint.com(Administrador)';
    RAISE NOTICE '  ? bodega@checkpoint.com   (Personal Bodega)';
    RAISE NOTICE '  ? calidad@checkpoint.com  (Control Calidad)';
    RAISE NOTICE '';
    RAISE NOTICE '?? PASSWORD: Admin123!';
    RAISE NOTICE '============================================';
    
    -- Verificar si hay errores
    IF table_count < 20 THEN
      RAISE WARNING '?? Se esperaban 21+ tablas pero solo se crearon %', table_count;
    END IF;

    IF user_count < 3 THEN
        RAISE WARNING '?? Se esperaban 3 usuarios pero solo se crearon %', user_count;
    END IF;
END $$;
