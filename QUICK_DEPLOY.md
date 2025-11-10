# QUICK START - DEPLOYMENT

## ?? OPCIÓN 1: RAILWAY (MÁS FÁCIL)

### 1. Preparar Git y GitHub
```bash
# Inicializar git (si no lo has hecho)
git init
git add .
git commit -m "Initial commit - Checkpoint Sistema"

# Crear repo en GitHub y conectar
git remote add origin https://github.com/TU_USUARIO/checkpoint-web.git
git branch -M main
git push -u origin main
```

### 2. Railway Setup
1. Ir a https://railway.app
2. Login con GitHub
3. "New Project" ? "Deploy from GitHub repo"
4. Seleccionar `checkpoint-web`
5. Railway detecta .NET automáticamente

### 3. Agregar PostgreSQL
```
En Railway proyecto:
? "New" ? "Database" ? "Add PostgreSQL"
? Railway crea DATABASE_URL automáticamente
```

### 4. Instalar Postgres Provider
```bash
cd checkpoint-web
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
```

### 5. Modificar Program.cs
Agregar este código ANTES de `builder.Services.AddDbContext`:

```csharp
// Detectar si estamos en Railway (PostgreSQL) o local (SQL Server)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
// Parsear Railway DATABASE_URL
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var connString = $"Host={databaseUri.Host};Port={databaseUri.Port};Database={databaseUri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
    
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    // SQL Server local
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

### 6. Variables de Entorno en Railway
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
```

### 7. Push y Deploy!
```bash
git add .
git commit -m "Add PostgreSQL support for Railway"
git push
```

Railway hace deploy automáticamente! ?

### 8. Aplicar Migraciones
```bash
# Instalar Railway CLI
npm install -g @railway/cli

# Login
railway login

# Link al proyecto
railway link

# Aplicar migraciones
railway run dotnet ef database update --project checkpoint-web
```

---

## ?? WORKFLOW DIARIO

### Hacer Cambios
```bash
# Crear rama
git checkout -b feature/mi-cambio

# Hacer cambios...
# Probar localmente

# Commit
git add .
git commit -m "feat: descripción del cambio"

# Push
git push origin feature/mi-cambio
```

### Merge y Deploy
```bash
# En GitHub: Crear Pull Request ? Merge a main

# Railway hace deploy automáticamente cuando detecta push a main
```

### Trabajar desde Otro PC
```bash
git clone https://github.com/TU_USUARIO/checkpoint-web.git
cd checkpoint-web/checkpoint-web
dotnet restore
dotnet ef database update
dotnet run
```

---

## ?? ACCEDER A TU APP

### Obtener URL en Railway
1. Ve a tu proyecto en Railway
2. Settings ? Networking ? "Generate Domain"
3. Tu app estará en: `https://checkpoint-web-production.up.railway.app`

### Usuarios de Prueba
```
Admin:
- Email: admin@example.com
- Password: Admin123!

Bodega:
- Email: bodega@example.com
- Password: Bodega123!

Calidad:
- Email: calidad@example.com
- Password: Calidad123!
```

---

## ?? SOLUCIÓN RÁPIDA DE PROBLEMAS

### Ver Logs
```bash
railway logs
```

### Error de Conexión DB
```bash
# Verificar variables
railway variables

# Debe existir DATABASE_URL
```

### Reinstalar Migraciones
```bash
railway run dotnet ef database drop --force --project checkpoint-web
railway run dotnet ef database update --project checkpoint-web
```

---

## ?? COSTOS RAILWAY

- **Hobby Plan**: $5/mes
  - 500 horas de ejecución
  - PostgreSQL incluido (5GB)
  - 100GB bandwidth

- **Pro Plan**: $20/mes
  - Unlimited execution
  - PostgreSQL incluido (más GB)
  - Priority support

**Start with Hobby, upgrade si lo necesitas.**

---

## ? VERIFICACIÓN POST-DEPLOY

- [ ] App carga correctamente
- [ ] Login funciona
- [ ] Base de datos conectada
- [ ] Usuarios seed creados
- [ ] HTTPS funcionando
- [ ] Sin errores en logs

---

**¡Listo! Tu app está en la nube ??**
