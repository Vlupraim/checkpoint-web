-- ============================================
-- CHECKPOINT DATABASE - Seed Data
-- Compatible con el schema PostgreSQL corregido
-- ============================================

BEGIN;

-- 1. ROLES
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
 ('admin-role', 'Administrador', 'ADMINISTRADOR', gen_random_uuid()::text),
    ('bodega-role', 'PersonalBodega', 'PERSONALBODEGA', gen_random_uuid()::text),
    ('calidad-role', 'ControlCalidad', 'CONTROLCALIDAD', gen_random_uuid()::text)
ON CONFLICT ("Id") DO NOTHING;

-- 2. USUARIOS (con Nombre y Activo)
-- Password: Admin123! (el hash es un ejemplo, debes usar el real de ASP.NET Identity)
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
    )
ON CONFLICT ("Id") DO NOTHING;

-- 3. ASIGNAR ROLES A USUARIOS
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES
    ('admin-user-001', 'admin-role'),
    ('bodega-user-001', 'bodega-role'),
    ('calidad-user-001', 'calidad-role')
ON CONFLICT DO NOTHING;

-- 4. SEDES
INSERT INTO "Sedes" ("Id", "Codigo", "Nombre", "Direccion", "Activa")
VALUES
    ('11111111-1111-1111-1111-111111111111'::uuid, 'SEDE-STGO', 'Sede Santiago Centro', 'Av. Libertador Bernardo O''Higgins 1234, Santiago', true),
    ('22222222-2222-2222-2222-222222222222'::uuid, 'SEDE-NORTE', 'Bodega Zona Norte', 'Calle Los Aromos 567, Quilicura', true),
('33333333-3333-3333-3333-333333333333'::uuid, 'SEDE-SUR', 'Bodega Zona Sur', 'Av. Vicuña Mackenna 8900, La Florida', true)
ON CONFLICT ("Id") DO NOTHING;

-- 5. UBICACIONES (con Nombre y Tipo)
INSERT INTO "Ubicaciones" ("Id", "SedeId", "Codigo", "Nombre", "Tipo", "Capacidad")
VALUES
(gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'A-01', 'Estante A-01', 'Estante', 1000.000),
    (gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'A-02', 'Estante A-02', 'Estante', 1000.000),
    (gen_random_uuid(), '11111111-1111-1111-1111-111111111111'::uuid, 'CF-01', 'Cámara Fría 1', 'CamaraFria', 5000.000),
    (gen_random_uuid(), '22222222-2222-2222-2222-222222222222'::uuid, 'B-01', 'Estante B-01', 'Estante', 1500.000),
  (gen_random_uuid(), '22222222-2222-2222-2222-222222222222'::uuid, 'CF-02', 'Cámara Fría 2', 'CamaraFria', 8000.000),
    (gen_random_uuid(), '33333333-3333-3333-3333-333333333333'::uuid, 'C-01', 'Pallet C-01', 'Pallet', 2000.000)
ON CONFLICT DO NOTHING;

-- 6. PRODUCTOS
INSERT INTO "Productos" ("Id", "Sku", "Nombre", "Unidad", "VidaUtilDias", "TempMin", "TempMax", "StockMinimo", "Activo")
VALUES
  (gen_random_uuid(), 'PROD-001', 'Materia Prima A', 'kg', 90, 2.0, 8.0, 500.000, true),
    (gen_random_uuid(), 'PROD-002', 'Materia Prima B', 'kg', 60, 0.0, 25.0, 300.000, true),
    (gen_random_uuid(), 'PROD-003', 'Producto Terminado X', 'unidad', 180, -5.0, 5.0, 100.000, true),
    (gen_random_uuid(), 'PROD-004', 'Producto Terminado Y', 'litro', 120, 2.0, 8.0, 200.000, true),
    (gen_random_uuid(), 'PROD-005', 'Insumo Crítico Z', 'caja', 30, 15.0, 25.0, 50.000, true)
ON CONFLICT DO NOTHING;

-- 7. CLIENTES (con todas las columnas)
INSERT INTO "Clientes" (
    "Nombre", "NombreComercial", "IdentificadorFiscal", "Direccion", "Ciudad", "Pais",
    "Telefono", "Email", "PersonaContacto", "Estado", "Observaciones", "FechaRegistro", "Activo"
)
VALUES
    ('Distribuidora Del Sur S.A.', 'Del Sur', '76.123.456-7', 'Av. Grecia 1234', 'Santiago', 'Chile', 
     '+56912345678', 'ventas@delsur.cl', 'Juan Pérez', 'Activo', 'Cliente VIP - Pago al contado', CURRENT_TIMESTAMP, true),
    
    ('Retail Express Ltda.', 'Retail Express', '77.987.654-3', 'Los Alamos 567', 'Valparaíso', 'Chile',
     '+56987654321', 'compras@retailexpress.cl', 'María González', 'Activo', 'Descuento 10% volumen', CURRENT_TIMESTAMP, true),
    
    ('Importadora Nacional', 'Imp. Nacional', '78.555.888-K', 'Calle Comercio 890', 'Concepción', 'Chile',
     '+56966666666', 'contacto@impnacional.cl', 'Carlos Rodríguez', 'Activo', NULL, CURRENT_TIMESTAMP, true)
ON CONFLICT DO NOTHING;

-- 8. PROVEEDORES (con todas las columnas)
INSERT INTO "Proveedores" (
    "Nombre", "NombreComercial", "IdentificadorFiscal", "Direccion", "Ciudad", "Pais",
    "Telefono", "Email", "PersonaContacto", "Categoria", "Calificacion", "Estado", 
    "Observaciones", "FechaRegistro", "Activo"
)
VALUES
    ('Proveedora Industrial S.A.', 'Prov. Industrial', '79.111.222-3', 'Av. Industrial 100', 'Santiago', 'Chile',
     '+56922222222', 'ventas@provin.cl', 'Ana Martínez', 'MateriaPrima', 5, 'Activo', 
'Proveedor certificado ISO 9001', CURRENT_TIMESTAMP, true),
    
    ('Alimentos Frescos Chile', 'Frescos Chile', '80.333.444-5', 'Camino Agrícola 200', 'Rancagua', 'Chile',
   '+56933333333', 'contacto@frescochile.cl', 'Pedro Soto', 'Perecibles', 4, 'Activo',
     'Entrega diaria', CURRENT_TIMESTAMP, true),
    
    ('Embalajes y Packaging Ltda.', 'Embal. Pack', '81.555.666-7', 'Parque Industrial 300', 'Quilicura', 'Chile',
     '+56944444444', 'ventas@embpack.cl', 'Luisa Torres', 'Embalaje', 5, 'Activo',
     NULL, CURRENT_TIMESTAMP, true)
ON CONFLICT DO NOTHING;

-- 9. PARAMETROS DEL SISTEMA
INSERT INTO "Parametros" ("Clave", "Nombre", "Descripcion", "Valor", "TipoDato", "Categoria", "EsEditable", "Activo")
VALUES
    ('TEMP_MIN_ALMACENAMIENTO', 'Temperatura Mínima de Almacenamiento', 'Temperatura mínima permitida en cámaras frías', '-5', 'Decimal', 'Calidad', true, true),
    ('TEMP_MAX_ALMACENAMIENTO', 'Temperatura Máxima de Almacenamiento', 'Temperatura máxima permitida en cámaras frías', '8', 'Decimal', 'Calidad', true, true),
    ('DIAS_ALERTA_VENCIMIENTO', 'Días de Alerta de Vencimiento', 'Días previos al vencimiento para generar alerta', '7', 'Integer', 'Inventario', true, true),
    ('EMAIL_NOTIFICACIONES', 'Email de Notificaciones', 'Email para notificaciones del sistema', 'admin@checkpoint.com', 'String', 'Sistema', true, true)
ON CONFLICT ("Clave") DO NOTHING;

COMMIT;

-- ============================================
-- MENSAJE FINAL
-- ============================================
DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Seed data cargado exitosamente!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'USUARIOS CREADOS:';
    RAISE NOTICE '  ? admin@checkpoint.com    (Administrador)';
    RAISE NOTICE '  ? bodega@checkpoint.com   (Personal Bodega)';
    RAISE NOTICE '  ? calidad@checkpoint.com  (Control Calidad)';
    RAISE NOTICE '';
    RAISE NOTICE 'PASSWORD PARA TODOS: Admin123!';
    RAISE NOTICE '';
    RAISE NOTICE 'DATOS DE PRUEBA:';
    RAISE NOTICE '  ? 3 Sedes';
    RAISE NOTICE '  ? 6 Ubicaciones';
    RAISE NOTICE '  ? 5 Productos';
    RAISE NOTICE '  ? 3 Clientes';
    RAISE NOTICE '  ? 3 Proveedores';
  RAISE NOTICE '  ? 4 Parámetros del sistema';
    RAISE NOTICE '============================================';
END $$;
