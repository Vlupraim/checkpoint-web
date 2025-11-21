-- ============================================
-- CHECKPOINT DATABASE - PostgreSQL Schema
-- Compatible con Entity Framework Core Models
-- ============================================

BEGIN;

-- ============================================
-- IDENTITY TABLES (ASP.NET Core Identity)
-- ============================================

-- 1. AspNetRoles
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

CREATE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

-- 2. AspNetUsers (con columnas personalizadas)
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "Nombre" TEXT NOT NULL,         -- ? Agregada
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,    -- ? Agregada
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

CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE "NormalizedUserName" IS NOT NULL;

-- 3. AspNetUserRoles
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

-- 4. AspNetUserClaims
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

-- 5. AspNetUserLogins
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" VARCHAR(450) NOT NULL,
    "ProviderKey" VARCHAR(450) NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

-- 6. AspNetUserTokens
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" VARCHAR(450) NOT NULL,
    "LoginProvider" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(450) NOT NULL,
    "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- 7. AspNetRoleClaims
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
"ClaimValue" TEXT,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

-- ============================================
-- BUSINESS TABLES
-- ============================================

-- 8. Sedes
CREATE TABLE IF NOT EXISTS "Sedes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Nombre" VARCHAR(150) NOT NULL,
    "Codigo" VARCHAR(50) NOT NULL,
    "Direccion" VARCHAR(250),
    "Activa" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS "IX_Sedes_Codigo" ON "Sedes" ("Codigo");

-- 9. Ubicaciones (? CORREGIDA)
CREATE TABLE IF NOT EXISTS "Ubicaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "SedeId" UUID NOT NULL,
    "Codigo" VARCHAR(50) NOT NULL,
    "Nombre" VARCHAR(200) NOT NULL,  -- ? Agregada
    "Tipo" VARCHAR(100) NOT NULL,      -- ? Renombrada desde "TipoUbicacion"
    "Capacidad" DECIMAL(18,3) NOT NULL,
    FOREIGN KEY ("SedeId") REFERENCES "Sedes" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Ubicaciones_SedeId_Codigo" ON "Ubicaciones" ("SedeId", "Codigo");

-- 10. Productos
CREATE TABLE IF NOT EXISTS "Productos" (
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

CREATE INDEX IF NOT EXISTS "IX_Productos_Sku" ON "Productos" ("Sku");

-- 11. Clientes (? ID corregido)
CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" SERIAL PRIMARY KEY,  -- ? Cambiado de UUID a SERIAL
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

CREATE INDEX IF NOT EXISTS "IX_Clientes_IdentificadorFiscal" ON "Clientes" ("IdentificadorFiscal");

-- 12. Proveedores (? ID corregido)
CREATE TABLE IF NOT EXISTS "Proveedores" (
    "Id" SERIAL PRIMARY KEY,        -- ? Cambiado de UUID a SERIAL
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

CREATE INDEX IF NOT EXISTS "IX_Proveedores_IdentificadorFiscal" ON "Proveedores" ("IdentificadorFiscal");

-- 13. Lotes (? CORREGIDA con todas las columnas)
CREATE TABLE IF NOT EXISTS "Lotes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "ProductoId" UUID NOT NULL,
    "ProveedorId" INTEGER,             -- ? Agregada
    "CodigoLote" VARCHAR(100) NOT NULL,
    "FechaIngreso" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaVencimiento" TIMESTAMP,
    "OrdenCompra" VARCHAR(100),       -- ? Agregada
    "GuiaRecepcion" VARCHAR(100),             -- ? Agregada
    "TempIngreso" DECIMAL(10,2) NOT NULL,
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    "CantidadInicial" DECIMAL(18,3) NOT NULL, -- ? Agregada
    "CantidadDisponible" DECIMAL(18,3) NOT NULL, -- ? Agregada
    FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("ProveedorId") REFERENCES "Proveedores" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Lotes_CodigoLote" ON "Lotes" ("CodigoLote");
CREATE INDEX IF NOT EXISTS "IX_Lotes_ProductoId" ON "Lotes" ("ProductoId");
CREATE INDEX IF NOT EXISTS "IX_Lotes_ProveedorId" ON "Lotes" ("ProveedorId");
CREATE INDEX IF NOT EXISTS "IX_Lotes_FechaVencimiento" ON "Lotes" ("FechaVencimiento");

-- 14. Stocks (? CORREGIDA)
CREATE TABLE IF NOT EXISTS "Stocks" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
  "UbicacionId" UUID NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "Unidad" VARCHAR(50) NOT NULL,
    "ActualizadoEn" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, -- ? Renombrada
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("UbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Stocks_LoteId" ON "Stocks" ("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Stocks_UbicacionId" ON "Stocks" ("UbicacionId");

-- 15. Movimientos (? CORREGIDA con TODAS las columnas)
CREATE TABLE IF NOT EXISTS "Movimientos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "SedeId" UUID,
    "OrigenUbicacionId" UUID,
    "DestinoUbicacionId" UUID,
    "ClienteId" INTEGER,               -- ? Agregada
    "Tipo" VARCHAR(50) NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "Unidad" VARCHAR(50) NOT NULL,            -- ? Agregada
    "Fecha" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsuarioId" VARCHAR(450),
    "Motivo" VARCHAR(500),
    "NumeroDocumento" VARCHAR(100), -- ? Agregada
    "Estado" VARCHAR(50),          -- ? Agregada
    "AprobadoPor" VARCHAR(450),   -- ? Agregada
    "FechaAprobacion" TIMESTAMP, -- ? Agregada
    "StockAnterior" DECIMAL(18,3),            -- ? Agregada
    "StockPosterior" DECIMAL(18,3),      -- ? Agregada
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE RESTRICT,
FOREIGN KEY ("SedeId") REFERENCES "Sedes" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("OrigenUbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("DestinoUbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE RESTRICT,
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("UsuarioId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Movimientos_LoteId" ON "Movimientos" ("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Movimientos_OrigenUbicacionId" ON "Movimientos" ("OrigenUbicacionId");
CREATE INDEX IF NOT EXISTS "IX_Movimientos_DestinoUbicacionId" ON "Movimientos" ("DestinoUbicacionId");
CREATE INDEX IF NOT EXISTS "IX_Movimientos_ClienteId" ON "Movimientos" ("ClienteId");

-- 16. CalidadLiberaciones
CREATE TABLE IF NOT EXISTS "CalidadLiberaciones" (
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

CREATE INDEX IF NOT EXISTS "IX_CalidadLiberaciones_LoteId" ON "CalidadLiberaciones" ("LoteId");

-- 17. Tareas (? ID corregido)
CREATE TABLE IF NOT EXISTS "Tareas" (
    "Id" SERIAL PRIMARY KEY,       -- ? Cambiado de UUID a SERIAL
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

CREATE INDEX IF NOT EXISTS "IX_Tareas_Estado" ON "Tareas" ("Estado");
CREATE INDEX IF NOT EXISTS "IX_Tareas_ResponsableId" ON "Tareas" ("ResponsableId");
CREATE INDEX IF NOT EXISTS "IX_Tareas_FechaLimite" ON "Tareas" ("FechaLimite");
CREATE INDEX IF NOT EXISTS "IX_Tareas_ProductoId" ON "Tareas" ("ProductoId");
CREATE INDEX IF NOT EXISTS "IX_Tareas_LoteId" ON "Tareas" ("LoteId");

-- 18. Notificaciones (? ID corregido y columnas agregadas)
CREATE TABLE IF NOT EXISTS "Notificaciones" (
    "Id" SERIAL PRIMARY KEY,       -- ? Cambiado de UUID a SERIAL
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

CREATE INDEX IF NOT EXISTS "IX_Notificaciones_UsuarioId" ON "Notificaciones" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Notificaciones_Leida" ON "Notificaciones" ("Leida");

-- 19. Procedimientos
CREATE TABLE IF NOT EXISTS "Procedimientos" (
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

CREATE INDEX IF NOT EXISTS "IX_Procedimientos_Codigo" ON "Procedimientos" ("Codigo");

-- 20. Parametros
CREATE TABLE IF NOT EXISTS "Parametros" (
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

CREATE INDEX IF NOT EXISTS "IX_Parametros_Clave" ON "Parametros" ("Clave");

-- 21. AuditLogs
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" VARCHAR(450),
    "Action" VARCHAR(200) NOT NULL,
    "Details" VARCHAR(2000),
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp" ON "AuditLogs" ("Timestamp");

COMMIT;

-- Mensaje de éxito
DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Schema PostgreSQL creado exitosamente!';
    RAISE NOTICE 'Compatible con Entity Framework Core 8.0';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Tablas creadas: 21';
    RAISE NOTICE '============================================';
END $$;
