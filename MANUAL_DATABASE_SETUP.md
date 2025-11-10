# ?? EJECUTAR SCRIPTS SQL EN RAILWAY CON DBEAVER

## ? **MÉTODO RECOMENDADO: DBeaver (Visual y Fácil)**

DBeaver es perfecto para conectarte a Railway PostgreSQL y ejecutar los scripts visualmente.

---

## ?? **PASO A PASO CON DBEAVER:**

### **Paso 1: Obtener credenciales de Railway**

1. Ve a **Railway** ? Tu proyecto
2. Click en **"Postgres"** (servicio de base de datos)
3. Click en pestaña **"Connect"** o **"Variables"**
4. Copia las siguientes credenciales:

```
Host: postgres.railway.internal
Port: 5432
Database: railway
Username: postgres
Password: [tu password]
```

O copia la **URL completa** que se ve así:
```
postgresql://postgres:XXXPASSWORDXXX@postgres.railway.internal:5432/railway
```

### **Paso 2: Instalar DBeaver (si no lo tienes)**

1. Descarga desde: https://dbeaver.io/download/
2. Instala la versión Community (gratis)
3. Abre DBeaver

### **Paso 3: Crear conexión en DBeaver**

1. En DBeaver, click en **"Database"** ? **"New Database Connection"**
2. Selecciona **"PostgreSQL"**
3. Click **"Next"**

#### **Configuración de la conexión:**

**Pestaña "Main":**
```
Host: postgres.railway.internal
Port: 5432
Database: railway
Username: postgres
Password: [tu password de Railway]
```

**Checkbox importante:**
- ? Marca: **"Show all databases"**

Click **"Test Connection"**
- ? Debería decir: **"Connected"**

Click **"Finish"**

### **Paso 4: Ejecutar script de Schema**

1. En DBeaver, expande la conexión **"PostgreSQL - railway"**
2. Click derecho en **"railway"** ? **"SQL Editor"** ? **"New SQL Script"**
3. **Abre el archivo** `database-schema.sql` en tu editor (VS Code)
4. **Copia TODO el contenido** (Ctrl+A, Ctrl+C)
5. **Pega** en el SQL Editor de DBeaver
6. Click en el botón **"Execute SQL Statement"** (? naranja) o presiona **Ctrl+Enter**
7. ? Deberías ver en la consola abajo:
```
Schema creado exitosamente!
Tablas principales creadas:
  - AspNetUsers (Usuario)
- Sede
  - Ubicacion
  - Producto
  - Lote
  - Stock
  - Movimiento
  ...
```

### **Paso 5: Verificar tablas creadas**

1. En DBeaver, click derecho en **"railway"** ? **"Refresh"**
2. Expande **"Schemas"** ? **"public"** ? **"Tables"**
3. ? Deberías ver ~20 tablas:
   - AspNetUsers
   - AspNetRoles
   - Sedes
   - Ubicaciones
   - Productos
   - Lotes
   - Stocks
 - Movimientos
   - CalidadLiberaciones
   - UserLocationAssignments
   - etc.

### **Paso 6: Ejecutar script de Seed Data**

1. **Abre** el archivo `database-seed.sql`
2. **Copia TODO el contenido**
3. **Pega** en una nueva ventana de SQL Editor de DBeaver
4. Click **"Execute SQL Statement"** (?)
5. ? Deberías ver:
```
Datos iniciales cargados exitosamente!
Usuarios creados:
  - admin@example.com / Admin123!
  - bodega@example.com / Bodega123!
  - calidad@example.com / Calidad123!
```

### **Paso 7: Verificar datos insertados**

En DBeaver, ejecuta estas queries para verificar:

```sql
-- Ver usuarios creados
SELECT "Email", "UserName" 
FROM "AspNetUsers";

-- Ver roles
SELECT "Name" 
FROM "AspNetRoles";

-- Ver productos de ejemplo
SELECT "Sku", "Nombre" 
FROM "Productos";

-- Ver sedes
SELECT "Codigo", "Nombre" 
FROM "Sedes";
```

---

## ?? **DESPUÉS DE EJECUTAR LOS SCRIPTS:**

### **1. Redeploy en Railway (opcional)**

Si Railway estaba en estado "Failed", haz redeploy:
```
Railway ? checkpoint-web ? Deployments ? Latest ? ... ? Redeploy
```

### **2. Verificar que la app inicia**

1. Espera 2-3 minutos
2. Abre: `https://checkpoint-web-production.up.railway.app/health`
3. ? Deberías ver: `{"status":"healthy","timestamp":"..."}`

### **3. Probar Login**

1. Abre: `https://checkpoint-web-production.up.railway.app`
2. Login con:
   - **Email:** `admin@example.com`
   - **Password:** `Admin123!`
3. ? Deberías entrar al Dashboard de Admin

---

## ?? **TIPS DE DBEAVER:**

### **Ver estructura de una tabla:**
- Click derecho en la tabla ? **"View Table"** ? Pestaña **"Columns"**

### **Ver datos de una tabla:**
- Doble click en la tabla ? Pestaña **"Data"**

### **Ejecutar queries rápidas:**
- Click derecho en tabla ? **"Generate SQL"** ? **"SELECT"**

### **Exportar datos:**
- Click derecho en tabla ? **"Export Data"**

### **Ver relaciones (Foreign Keys):**
- Click derecho en tabla ? **"View Table"** ? Pestaña **"Foreign Keys"**

---

## ?? **MÉTODOS ALTERNATIVOS:**

### **Opción 2: Railway Web UI**

1. Railway ? Postgres ? **"Query"** o **"Data"** tab
2. Copiar y pegar cada script
3. Ejecutar

### **Opción 3: Railway CLI**

```sh
railway login
railway link
railway run psql < database-schema.sql
railway run psql < database-seed.sql
```

### **Opción 4: psql directamente**

```sh
psql "postgresql://postgres:XXXXX@postgres.railway.internal:5432/railway" -f database-schema.sql
psql "postgresql://postgres:XXXXX@postgres.railway.internal:5432/railway" -f database-seed.sql
```

---

## ? **VENTAJAS DE USAR DBEAVER:**

? **Visual y fácil de usar**
? **Ver tablas y datos en tiempo real**
? **Autocompletado SQL**
? **Ver relaciones entre tablas gráficamente**
? **Exportar/importar datos fácilmente**
? **No requiere CLI o comandos complicados**
? **Puedes explorar la base de datos después**

---

## ?? **ESQUEMA BASADO EN TUS DIAGRAMAS:**

Los scripts ahora incluyen:

? **Lote** con:
   - CodigoLote, FechaIngreso, FechaVencimiento
   - GuiaDespacho, TempIngreso, Estado

? **Movimiento** con:
   - OrigenUbicacionId, DestinoUbicacionId
   - SedeId, GuiaDespacho, Motivo

? **Stock** con:
 - Unidad específica por stock

? **UserLocationAssignments**:
   - Asignación de usuarios a ubicaciones específicas

? **CalidadLiberacion** con:
   - Estado, Observacion, EvidenciaUrl

---

## ?? **RESULTADO ESPERADO:**

Después de ejecutar ambos scripts en DBeaver:

```
? 20+ tablas creadas según tus diagramas
? 3 usuarios funcionales
? Datos de ejemplo (sedes, productos, ubicaciones)
? Relaciones correctas (Foreign Keys)
? Índices optimizados
? App funciona inmediatamente
```

---

**¡Usa DBeaver, es mucho más visual y fácil!** ??
