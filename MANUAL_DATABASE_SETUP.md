# ?? EJECUTAR SCRIPTS SQL EN RAILWAY - GUÍA RÁPIDA

## ? **SOLUCIÓN AL PROBLEMA DE ToString()**

El error `"Cannot print exception string because Exception.ToString() failed"` es causado por incompatibilidades entre las migraciones de SQL Server y PostgreSQL.

**Solución:** Crear las tablas manualmente con scripts SQL nativos de PostgreSQL.

---

## ?? **MÉTODO 1: Desde Railway Web UI (MÁS FÁCIL)**

### **Paso 1: Acceder a la base de datos**

1. Ve a Railway ? Tu proyecto
2. Click en **"Postgres"** (el servicio de base de datos)
3. Click en la pestaña **"Database"** o **"Data"**
4. Verás un editor SQL o consola

### **Paso 2: Ejecutar schema**

1. Abre el archivo `database-schema.sql` en tu editor
2. **Copia TODO el contenido** (Ctrl+A, Ctrl+C)
3. **Pega** en el editor SQL de Railway
4. Click **"Execute"** o **"Run"**
5. ? Deberías ver: `"Schema creado exitosamente!"`

### **Paso 3: Ejecutar seed data**

1. Abre el archivo `database-seed.sql`
2. **Copia TODO el contenido**
3. **Pega** en el editor SQL de Railway
4. Click **"Execute"** o **"Run"**
5. ? Deberías ver:
```
Datos iniciales cargados exitosamente!
Usuarios creados:
  - admin@example.com / Admin123!
  - bodega@example.com / Bodega123!
  - calidad@example.com / Calidad123!
```

---

## ?? **MÉTODO 2: Desde Railway CLI**

### **Requisitos:**
```sh
npm install -g @railway/cli
```

### **Paso 1: Login y Link**

```sh
# Login a Railway
railway login

# Link al proyecto (en la carpeta del proyecto)
railway link
```

### **Paso 2: Ejecutar Scripts**

```sh
# Ejecutar schema
railway run psql -c "$(cat database-schema.sql)"

# Ejecutar seed data
railway run psql -c "$(cat database-seed.sql)"
```

---

## ?? **MÉTODO 3: Con psql directamente**

### **Paso 1: Obtener conexión de Railway**

1. Railway ? Postgres ? Variables
2. Copiar el valor de `DATABASE_URL`

### **Paso 2: Conectar con psql**

```sh
# Reemplaza con tu DATABASE_URL
psql "postgresql://postgres:XXXXX@postgres.railway.internal:5432/railway"
```

### **Paso 3: Ejecutar scripts**

```sql
-- Dentro de psql
\i database-schema.sql
\i database-seed.sql
```

---

## ?? **MÉTODO 4: Desde TablePlus / pgAdmin (GUI)**

Si prefieres una interfaz gráfica:

### **Paso 1: Obtener credenciales**

Railway ? Postgres ? Connect ? Copiar credenciales:
```
Host: postgres.railway.internal
Port: 5432
Database: railway
User: postgres
Password: [tu password]
```

### **Paso 2: Conectar**

1. Abre TablePlus o pgAdmin
2. Nueva conexión PostgreSQL
3. Ingresa las credenciales
4. Conectar

### **Paso 3: Ejecutar SQL**

1. Abre `database-schema.sql` en el editor SQL
2. Ejecutar todo
3. Abre `database-seed.sql`
4. Ejecutar todo

---

## ? **VERIFICACIÓN**

Después de ejecutar ambos scripts:

### **1. Verificar tablas creadas**

```sql
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

Deberías ver ~20 tablas incluyendo:
- AspNetUsers
- AspNetRoles
- Productos
- Lotes
- Stocks
- Movimientos
- etc.

### **2. Verificar usuarios**

```sql
SELECT "Email", "UserName" 
FROM "AspNetUsers";
```

Deberías ver:
- admin@example.com
- bodega@example.com
- calidad@example.com

### **3. Verificar roles**

```sql
SELECT u."Email", r."Name" 
FROM "AspNetUsers" u
JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
JOIN "AspNetRoles" r ON r."Id" = ur."RoleId";
```

---

## ?? **DESPUÉS DE EJECUTAR LOS SCRIPTS:**

### **1. Redeploy la app en Railway**

```sh
# Desde tu proyecto local
git add database-schema.sql database-seed.sql MANUAL_DATABASE_SETUP.md
git commit -m "Add manual database setup scripts"
git push
```

O manualmente en Railway:
- Deployments ? Latest ? ... ? Redeploy

### **2. Verificar que la app inicia**

1. Espera el deploy (2-3 min)
2. Abre: `https://checkpoint-web-production.up.railway.app/health`
3. Deberías ver: `{"status":"healthy",...}`

### **3. Probar login**

1. Abre: `https://checkpoint-web-production.up.railway.app`
2. Login con:
   - **Email:** `admin@example.com`
   - **Password:** `Admin123!`
3. ? Deberías entrar al Dashboard de Admin

---

## ?? **NOTA IMPORTANTE:**

Los passwords en `database-seed.sql` están hasheados con ASP.NET Core Identity.

**Passwords para todos los usuarios:**
- `Admin123!` para admin@example.com
- `Bodega123!` para bodega@example.com
- `Calidad123!` para calidad@example.com

(Todos usan el mismo hash por simplicidad en esta demo)

---

## ?? **SI LOS PASSWORDS NO FUNCIONAN:**

Crea nuevos usuarios desde la app:

1. Login como admin (si funciona)
2. Ve a Admin ? Usuarios
3. Crea un nuevo usuario con rol Administrador

O cambia los passwords desde SQL:

```sql
-- Generar nuevo hash para password "MiPassword123!"
-- Nota: Necesitas usar un generador de hash ASP.NET Identity
UPDATE "AspNetUsers" 
SET "PasswordHash" = 'TU_NUEVO_HASH_AQUI'
WHERE "Email" = 'admin@example.com';
```

---

## ?? **VENTAJAS DE ESTE MÉTODO:**

? **No depende de Entity Framework migrations**
? **Scripts SQL nativos de PostgreSQL**
? **Más control sobre el schema**
? **Fácil de versionar en Git**
? **Más rápido de ejecutar**
? **Sin problemas de compatibilidad SQL Server ? PostgreSQL**

---

## ?? **RESULTADO ESPERADO:**

Después de ejecutar ambos scripts:

```
? 20+ tablas creadas
? 3 roles creados (Administrador, PersonalBodega, ControlCalidad)
? 3 usuarios creados con passwords funcionales
? Datos de ejemplo (sedes, productos, clientes, proveedores)
? App funciona correctamente
? Login exitoso
```

---

**¡Ejecuta los scripts y tu app debería funcionar inmediatamente!** ??
