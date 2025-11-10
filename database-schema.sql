-- ============================================
-- CHECKPOINT DATABASE - PostgreSQL Schema
-- Crear todas las tablas manualmente
-- ============================================

-- 1. Tabla: AspNetRoles
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "Name" VARCHAR(256),
    "NormalizedName" VARCHAR(256),
    "ConcurrencyStamp" TEXT
);

CREATE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

-- 2. Tabla: AspNetUsers
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id" VARCHAR(450) NOT NULL PRIMARY KEY,
    "UserName" VARCHAR(256),
    "NormalizedUserName" VARCHAR(256),
    "Email" VARCHAR(256),
    "NormalizedEmail" VARCHAR(256),
    "EmailConfirmed" BOOLEAN NOT NULL,
    "PasswordHash" TEXT,
    "SecurityStamp" TEXT,
    "ConcurrencyStamp" TEXT,
    "PhoneNumber" TEXT,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,
    "TwoFactorEnabled" BOOLEAN NOT NULL,
    "LockoutEnd" TIMESTAMPTZ,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);

CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

-- 3. Tabla: AspNetUserRoles
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

-- 4. Tabla: AspNetUserClaims
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

-- 5. Tabla: AspNetUserLogins
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider" VARCHAR(450) NOT NULL,
    "ProviderKey" VARCHAR(450) NOT NULL,
    "ProviderDisplayName" TEXT,
    "UserId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

-- 6. Tabla: AspNetUserTokens
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId" VARCHAR(450) NOT NULL,
    "LoginProvider" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(450) NOT NULL,
  "Value" TEXT,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- 7. Tabla: AspNetRoleClaims
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT,
    "ClaimValue" TEXT,
    FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

-- 8. Tabla: Sedes
CREATE TABLE IF NOT EXISTS "Sedes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Codigo" VARCHAR(20) NOT NULL,
    "Nombre" VARCHAR(200) NOT NULL,
    "Direccion" VARCHAR(500),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Sedes_Codigo" ON "Sedes" ("Codigo");

-- 9. Tabla: Ubicaciones
CREATE TABLE IF NOT EXISTS "Ubicaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Codigo" VARCHAR(50) NOT NULL,
    "Nombre" VARCHAR(200) NOT NULL,
    "TipoUbicacion" VARCHAR(50) NOT NULL,
    "SedeId" UUID NOT NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY ("SedeId") REFERENCES "Sedes" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Ubicaciones_Codigo" ON "Ubicaciones" ("Codigo");
CREATE INDEX IF NOT EXISTS "IX_Ubicaciones_SedeId" ON "Ubicaciones" ("SedeId");

-- 10. Tabla: Productos
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

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Productos_Sku" ON "Productos" ("Sku");

-- 11. Tabla: Lotes
CREATE TABLE IF NOT EXISTS "Lotes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
 "NumeroLote" VARCHAR(50) NOT NULL,
    "ProductoId" UUID NOT NULL,
    "FechaProduccion" TIMESTAMP NOT NULL,
    "FechaVencimiento" TIMESTAMP NOT NULL,
    "CantidadInicial" DECIMAL(18,3) NOT NULL,
    "EstadoCalidad" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
"Observaciones" TEXT,
    FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Lotes_NumeroLote" ON "Lotes" ("NumeroLote");
CREATE INDEX IF NOT EXISTS "IX_Lotes_ProductoId" ON "Lotes" ("ProductoId");

-- 12. Tabla: Stocks
CREATE TABLE IF NOT EXISTS "Stocks" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "UbicacionId" UUID NOT NULL,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "FechaUltimaActualizacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("UbicacionId") REFERENCES "Ubicaciones" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Stocks_LoteId" ON "Stocks" ("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Stocks_UbicacionId" ON "Stocks" ("UbicacionId");

-- 13. Tabla: Movimientos
CREATE TABLE IF NOT EXISTS "Movimientos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Tipo" VARCHAR(50) NOT NULL,
    "LoteId" UUID NOT NULL,
    "UbicacionOrigenId" UUID,
    "UbicacionDestinoId" UUID,
    "Cantidad" DECIMAL(18,3) NOT NULL,
    "Fecha" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
"UsuarioId" VARCHAR(450),
    "Observaciones" TEXT,
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Completado',
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE,
    FOREIGN KEY ("UbicacionOrigenId") REFERENCES "Ubicaciones" ("Id") ON DELETE SET NULL,
    FOREIGN KEY ("UbicacionDestinoId") REFERENCES "Ubicaciones" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Movimientos_LoteId" ON "Movimientos" ("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Movimientos_Fecha" ON "Movimientos" ("Fecha");

-- 14. Tabla: CalidadLiberaciones
CREATE TABLE IF NOT EXISTS "CalidadLiberaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LoteId" UUID NOT NULL,
    "UsuarioId" VARCHAR(450),
    "FechaRevision" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Resultado" VARCHAR(50) NOT NULL,
    "Observaciones" TEXT,
    FOREIGN KEY ("LoteId") REFERENCES "Lotes" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_CalidadLiberaciones_LoteId" ON "CalidadLiberaciones" ("LoteId");

-- 15. Tabla: Clientes
CREATE TABLE IF NOT EXISTS "Clientes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Nombre" VARCHAR(200) NOT NULL,
    "NombreComercial" VARCHAR(200),
    "IdentificadorFiscal" VARCHAR(50),
    "Direccion" VARCHAR(500),
  "Ciudad" VARCHAR(100),
    "Pais" VARCHAR(100) NOT NULL DEFAULT 'Chile',
    "Telefono" VARCHAR(50),
    "Email" VARCHAR(200),
    "PersonaContacto" VARCHAR(200),
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Activo'
);

-- 16. Tabla: Proveedores
CREATE TABLE IF NOT EXISTS "Proveedores" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Nombre" VARCHAR(200) NOT NULL,
    "IdentificadorFiscal" VARCHAR(50),
 "Direccion" VARCHAR(500),
    "Telefono" VARCHAR(50),
    "Email" VARCHAR(200),
    "PersonaContacto" VARCHAR(200),
    "Categoria" VARCHAR(100),
    "Calificacion" INTEGER,
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Activo'
);

-- 17. Tabla: Tareas
CREATE TABLE IF NOT EXISTS "Tareas" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Titulo" VARCHAR(200) NOT NULL,
    "Descripcion" TEXT,
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    "Prioridad" VARCHAR(50) NOT NULL DEFAULT 'Media',
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "FechaLimite" TIMESTAMP,
    "ResponsableId" VARCHAR(450),
    "ProductoId" UUID,
    "Progreso" INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY ("ProductoId") REFERENCES "Productos" ("Id") ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS "IX_Tareas_Estado" ON "Tareas" ("Estado");
CREATE INDEX IF NOT EXISTS "IX_Tareas_ResponsableId" ON "Tareas" ("ResponsableId");

-- 18. Tabla: Notificaciones
CREATE TABLE IF NOT EXISTS "Notificaciones" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UsuarioId" VARCHAR(450) NOT NULL,
    "Mensaje" TEXT NOT NULL,
    "Tipo" VARCHAR(50) NOT NULL,
    "Leida" BOOLEAN NOT NULL DEFAULT FALSE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "Url" VARCHAR(500)
);

CREATE INDEX IF NOT EXISTS "IX_Notificaciones_UsuarioId" ON "Notificaciones" ("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_Notificaciones_Leida" ON "Notificaciones" ("Leida");

-- 19. Tabla: AuditLogs
CREATE TABLE IF NOT EXISTS "AuditLogs" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" VARCHAR(450),
    "Action" VARCHAR(200) NOT NULL,
    "Entity" VARCHAR(100),
    "EntityId" VARCHAR(450),
    "Changes" TEXT,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IpAddress" VARCHAR(50)
);

CREATE INDEX IF NOT EXISTS "IX_AuditLogs_UserId" ON "AuditLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Timestamp" ON "AuditLogs" ("Timestamp");
CREATE INDEX IF NOT EXISTS "IX_AuditLogs_Action" ON "AuditLogs" ("Action");

-- 20. Tabla: Parametros (opcional)
CREATE TABLE IF NOT EXISTS "Parametros" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Clave" VARCHAR(100) NOT NULL,
    "Valor" TEXT,
    "Descripcion" VARCHAR(500)
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Parametros_Clave" ON "Parametros" ("Clave");

COMMIT;

-- Mensaje de éxito
DO $$
BEGIN
    RAISE NOTICE 'Schema creado exitosamente!';
END $$;
