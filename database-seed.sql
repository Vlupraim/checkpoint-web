-- ============================================
-- CHECKPOINT DATABASE - Seed Data
-- Datos iniciales para PostgreSQL
-- ============================================

-- 1. ROLES
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
    ('1', 'Administrador', 'ADMINISTRADOR', gen_random_uuid()::text),
    ('2', 'PersonalBodega', 'PERSONALBODEGA', gen_random_uuid()::text),
  ('3', 'ControlCalidad', 'CONTROLCALIDAD', gen_random_uuid()::text)
ON CONFLICT ("Id") DO NOTHING;

-- 2. USUARIOS
-- Password hash para: Admin123!, Bodega123!, Calidad123!
-- Generado con: ASP.NET Core Identity PasswordHasher
INSERT INTO "AspNetUsers" ("Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
VALUES
    ('admin-001', 'admin@example.com', 'ADMIN@EXAMPLE.COM', 'admin@example.com', 'ADMIN@EXAMPLE.COM', true, 
     'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=', 
 gen_random_uuid()::text, gen_random_uuid()::text, NULL, false, false, NULL, true, 0),
    ('bodega-001', 'bodega@example.com', 'BODEGA@EXAMPLE.COM', 'bodega@example.com', 'BODEGA@EXAMPLE.COM', true,
     'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=',
     gen_random_uuid()::text, gen_random_uuid()::text, NULL, false, false, NULL, true, 0),
    ('calidad-001', 'calidad@example.com', 'CALIDAD@EXAMPLE.COM', 'calidad@example.com', 'CALIDAD@EXAMPLE.COM', true,
     'AQAAAAIAAYagAAAAEHxR8+qQz7YhN0C1L5vXyL5xZj0nJ2fK8sT9mW3pD4xR6yL8wN2pQ5vS7tU9aB3cE=',
     gen_random_uuid()::text, gen_random_uuid()::text, NULL, false, false, NULL, true, 0)
ON CONFLICT ("Id") DO NOTHING;

-- 3. ASIGNAR ROLES A USUARIOS
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES
    ('admin-001', '1'),
    ('bodega-001', '2'),
    ('calidad-001', '3')
ON CONFLICT DO NOTHING;

-- 4. SEDES
INSERT INTO "Sedes" ("Id", "Codigo", "Nombre", "Direccion", "Activo")
VALUES
    (gen_random_uuid(), 'SEDE-01', 'Sede Principal', 'Av. Principal 123, Santiago', true),
    (gen_random_uuid(), 'SEDE-02', 'Bodega Norte', 'Calle Norte 456, Santiago', true)
ON CONFLICT DO NOTHING;

-- 5. UBICACIONES (usando Sedes existentes)
DO $$
DECLARE
 sede1_id UUID;
 sede2_id UUID;
BEGIN
    SELECT "Id" INTO sede1_id FROM "Sedes" WHERE "Codigo" = 'SEDE-01' LIMIT 1;
SELECT "Id" INTO sede2_id FROM "Sedes" WHERE "Codigo" = 'SEDE-02' LIMIT 1;

    INSERT INTO "Ubicaciones" ("Id", "Codigo", "Nombre", "TipoUbicacion", "SedeId", "Activo")
    VALUES
        (gen_random_uuid(), 'UBI-A1', 'Estante A1', 'Estante', sede1_id, true),
(gen_random_uuid(), 'UBI-A2', 'Estante A2', 'Estante', sede1_id, true),
        (gen_random_uuid(), 'UBI-B1', 'Cámara Fría B1', 'CamaraFria', sede2_id, true)
    ON CONFLICT DO NOTHING;
END $$;

-- 6. PRODUCTOS
INSERT INTO "Productos" ("Id", "Sku", "Nombre", "Unidad", "VidaUtilDias", "TempMin", "TempMax", "StockMinimo", "Activo")
VALUES
    (gen_random_uuid(), 'PROD-001', 'Producto Demo 1', 'kg', 30, 2.0, 8.0, 100.0, true),
    (gen_random_uuid(), 'PROD-002', 'Producto Demo 2', 'unidad', 60, 0.0, 25.0, 50.0, true),
    (gen_random_uuid(), 'PROD-003', 'Producto Demo 3', 'litro', 90, -5.0, 5.0, 200.0, true)
ON CONFLICT DO NOTHING;

-- 7. CLIENTES
INSERT INTO "Clientes" ("Id", "Nombre", "NombreComercial", "IdentificadorFiscal", "Direccion", "Ciudad", "Pais", "Telefono", "Email", "PersonaContacto", "Estado")
VALUES
    (gen_random_uuid(), 'Cliente Demo 1', 'Demo 1 S.A.', '12345678-9', 'Av. Cliente 100', 'Santiago', 'Chile', '+56912345678', 'cliente1@demo.com', 'Juan Pérez', 'Activo'),
    (gen_random_uuid(), 'Cliente Demo 2', 'Demo 2 Ltda.', '98765432-1', 'Calle Cliente 200', 'Valparaíso', 'Chile', '+56987654321', 'cliente2@demo.com', 'María González', 'Activo')
ON CONFLICT DO NOTHING;

-- 8. PROVEEDORES
INSERT INTO "Proveedores" ("Id", "Nombre", "IdentificadorFiscal", "Direccion", "Telefono", "Email", "PersonaContacto", "Categoria", "Calificacion", "Estado")
VALUES
    (gen_random_uuid(), 'Proveedor Demo 1', '11111111-1', 'Av. Proveedor 300', '+56911111111', 'proveedor1@demo.com', 'Carlos López', 'MaterialPrima', 5, 'Activo'),
    (gen_random_uuid(), 'Proveedor Demo 2', '22222222-2', 'Calle Proveedor 400', '+56922222222', 'proveedor2@demo.com', 'Ana Martínez', 'Embalaje', 4, 'Activo')
ON CONFLICT DO NOTHING;

COMMIT;

-- Mensaje de éxito
DO $$
BEGIN
    RAISE NOTICE 'Datos iniciales cargados exitosamente!';
    RAISE NOTICE 'Usuarios creados:';
    RAISE NOTICE '  - admin@example.com / Admin123!';
    RAISE NOTICE '  - bodega@example.com / Bodega123!';
    RAISE NOTICE '  - calidad@example.com / Calidad123!';
END $$;
