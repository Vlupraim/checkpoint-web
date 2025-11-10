# ?? GUÍA COMPLETA DE DEPLOYMENT

## ?? TABLA DE CONTENIDOS
1. [Preparación del Proyecto](#preparación)
2. [Git y GitHub](#git-github)
3. [Railway Deployment](#railway)
4. [Azure Deployment](#azure)
5. [Docker Deployment](#docker)
6. [Variables de Entorno](#variables)
7. [Troubleshooting](#troubleshooting)

---

## 1?? PREPARACIÓN DEL PROYECTO

### ? Archivos Creados
- ? `.gitignore` - Ignora archivos innecesarios
- ? `README.md` - Documentación del proyecto
- ? `Dockerfile` - Para deployment con Docker
- ? `railway.json` - Configuración de Railway
- ? `appsettings.Production.json` - Configuración de producción

### ? Verificar que todo compila
```bash
cd checkpoint-web
dotnet build
dotnet test  # Si tienes tests
```

---

## 2?? GIT Y GITHUB

### Paso 1: Inicializar Git (si no lo has hecho)
```bash
# En la raíz del proyecto (donde está checkpoint-web.sln)
git init
git add .
git commit -m "Initial commit - Checkpoint Sistema completo"
```

### Paso 2: Crear Repositorio en GitHub
1. Ve a [GitHub.com](https://github.com)
2. Click en "New repository"
3. Nombre: `checkpoint-web`
4. **NO** inicializar con README (ya tenemos uno)
5. Click "Create repository"

### Paso 3: Conectar y Push
```bash
# Reemplaza TU_USUARIO con tu usuario de GitHub
git remote add origin https://github.com/TU_USUARIO/checkpoint-web.git
git branch -M main
git push -u origin main
```

### Paso 4: Verificar
- Visita tu repositorio en GitHub
- Deberías ver todos tus archivos (excepto los del `.gitignore`)

---

## 3?? RAILWAY DEPLOYMENT (RECOMENDADO) ?

Railway es la opción más fácil y económica para .NET.

### Paso 1: Crear Cuenta
1. Ve a [Railway.app](https://railway.app)
2. "Login" ? "Login with GitHub"
3. Autoriza Railway

### Paso 2: Crear Proyecto
1. Dashboard ? "New Project"
2. "Deploy from GitHub repo"
3. Selecciona `checkpoint-web`
4. Railway detectará automáticamente .NET 8

### Paso 3: Agregar Base de Datos PostgreSQL
```
1. En tu proyecto ? "New" ? "Database" ? "Add PostgreSQL"
2. Railway creará automáticamente la base de datos
3. Variable DATABASE_URL se agregará automáticamente
```

**NOTA:** Railway usa PostgreSQL por defecto. SQL Server cuesta extra.

#### Opción A: Usar PostgreSQL (Recomendado)
Necesitas instalar el proveedor de PostgreSQL:

```bash
cd checkpoint-web
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.0
```

Luego modifica `Program.cs`:
```csharp
// Reemplaza la línea de UseSqlServer con:
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway PostgreSQL
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var host = databaseUri.Host;
    var database = databaseUri.AbsolutePath.Trim('/');
    var user = userInfo[0];
    var password = userInfo[1];
    var port = databaseUri.Port;
    
    var connString = $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Require;Trust Server Certificate=true";
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseNpgsql(connString));
}
else
{
    // Local SQL Server
    builder.Services.AddDbContext<CheckpointDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

#### Opción B: Usar SQL Server en Railway ($$$)
1. En Railway: "New" ? "Database" ? "Add MSSQL"
2. Cuesta ~$10/mes adicionales
3. Railway proveerá `MSSQL_URL`

### Paso 4: Configurar Variables de Entorno
En Railway ? Settings ? Variables:

```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080

# Si usas PostgreSQL (automático)
DATABASE_URL=postgresql://... (Railway lo pone automáticamente)

# Si usas SQL Server
ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True
```

### Paso 5: Deploy!
1. Railway comenzará el deployment automáticamente
2. Verás los logs en tiempo real
3. Cuando termine: "View Deployment" para ver tu app

### Paso 6: Aplicar Migraciones
```bash
# Opción A: Desde Railway CLI
railway run dotnet ef database update --project checkpoint-web

# Opción B: Agregar al Dockerfile (ya está incluido)
```

### Paso 7: Configurar Dominio
1. Railway ? Settings ? Networking
2. "Generate Domain" (gratis: `tu-app.up.railway.app`)
3. O agregar dominio personalizado

---

## 4?? AZURE APP SERVICE DEPLOYMENT

Si prefieres Azure (más profesional):

### Paso 1: Crear App Service
```bash
# Instalar Azure CLI
winget install Microsoft.AzureCLI

# Login
az login

# Crear grupo de recursos
az group create --name checkpoint-rg --location westus

# Crear App Service Plan
az appservice plan create --name checkpoint-plan --resource-group checkpoint-rg --sku B1 --is-linux

# Crear Web App
az webapp create --name checkpoint-web-app --resource-group checkpoint-rg --plan checkpoint-plan --runtime "DOTNETCORE:8.0"
```

### Paso 2: Configurar Deployment desde GitHub
```bash
az webapp deployment source config --name checkpoint-web-app --resource-group checkpoint-rg --repo-url https://github.com/TU_USUARIO/checkpoint-web --branch main --manual-integration
```

### Paso 3: Agregar SQL Database
```bash
# Crear SQL Server
az sql server create --name checkpoint-sql --resource-group checkpoint-rg --location westus --admin-user checkpointadmin --admin-password TuPassword123!

# Crear Database
az sql db create --name CheckpointDB --server checkpoint-sql --resource-group checkpoint-rg --service-objective S0

# Configurar Firewall
az sql server firewall-rule create --resource-group checkpoint-rg --server checkpoint-sql --name AllowAzure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### Paso 4: Configurar Connection String
```bash
az webapp config connection-string set --name checkpoint-web-app --resource-group checkpoint-rg --connection-string-type SQLAzure --settings DefaultConnection="Server=tcp:checkpoint-sql.database.windows.net,1433;Database=CheckpointDB;User ID=checkpointadmin;Password=TuPassword123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

## 5?? DOCKER DEPLOYMENT (Render, Fly.io, DigitalOcean)

### Paso 1: Probar Docker Localmente
```bash
# Build
docker build -t checkpoint-web .

# Run
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="Server=..." checkpoint-web
```

### Paso 2: Deploy a Render.com
1. Ve a [Render.com](https://render.com)
2. "New" ? "Web Service"
3. Conecta tu repositorio GitHub
4. Configuración:
   - **Name:** checkpoint-web
   - **Environment:** Docker
   - **Plan:** Free ($0) o Starter ($7/mes)

5. Variables de entorno (en Render):
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...
```

6. "Create Web Service"

---

## 6?? VARIABLES DE ENTORNO

### Railway
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
DATABASE_URL=postgresql://...  # Automático
```

### Azure
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=tcp:...
```

### Docker
```env
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...
ASPNETCORE_URLS=http://0.0.0.0:8080
```

---

## 7?? CI/CD AUTOMÁTICO

### Con GitHub Actions (Azure)

Crea `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
 
    steps:
    - uses: actions/checkout@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
   dotnet-version: '8.0.x'
    
  - name: Build
      run: dotnet build checkpoint-web/checkpoint-web.csproj --configuration Release
    
  - name: Publish
      run: dotnet publish checkpoint-web/checkpoint-web.csproj -c Release -o ./publish
    
  - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
   with:
        app-name: 'checkpoint-web-app'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
      package: ./publish
```

### Con Railway
- ? **Automático!** Cada push a `main` hace deploy automáticamente
- Ver logs en tiempo real en Railway dashboard

---

## 8?? WORKFLOW DE TRABAJO

### Desarrollo Local
```bash
git checkout -b feature/nueva-funcionalidad
# ... hacer cambios ...
git add .
git commit -m "feat: agregar nueva funcionalidad"
git push origin feature/nueva-funcionalidad
```

### Pull Request y Merge
1. Crear PR en GitHub
2. Revisar cambios
3. Merge a `main`
4. **Railway/Azure hace deploy automáticamente** ??

### Trabajar desde Otro PC
```bash
git clone https://github.com/TU_USUARIO/checkpoint-web.git
cd checkpoint-web/checkpoint-web
dotnet restore
dotnet ef database update
dotnet run
```

---

## ?? TROUBLESHOOTING

### Error: "The ConnectionString property has not been initialized"
**Solución:** Verifica que la variable `ConnectionStrings__DefaultConnection` está configurada en Railway/Azure.

### Error: "No database provider has been configured"
**Solución:** Verifica que `UseSqlServer()` o `UseNpgsql()` está presente en `Program.cs`.

### Error 500 en producción
**Solución:**
1. Habilita logs detallados:
   ```env
   ASPNETCORE_ENVIRONMENT=Development  # Temporalmente
   ```
2. Revisa logs en Railway ? Deployments ? View Logs

### Migraciones no se aplican
**Solución:**
```bash
# Desde Railway CLI
railway run dotnet ef database update --project checkpoint-web

# O agrega al Dockerfile:
RUN dotnet ef database update
```

---

## ?? COMPARACIÓN DE PLATAFORMAS

| Plataforma | Precio/mes | Base de Datos | CI/CD | Dificultad |
|------------|------------|---------------|-------|------------|
| **Railway** | $5-10 | PostgreSQL gratis | ? Auto | ? Fácil |
| **Azure** | $13-55 | SQL Server incluido | ? GitHub Actions | ?? Media |
| **Render** | $7-25 | PostgreSQL gratis | ? Auto | ? Fácil |
| **Fly.io** | $5-20 | PostgreSQL gratis | ? Auto | ?? Media |

---

## ? CHECKLIST FINAL

Antes de hacer deployment:

- [ ] `.gitignore` creado
- [ ] `README.md` actualizado
- [ ] Variables sensibles NO están en el código
- [ ] `appsettings.Production.json` configurado
- [ ] Migraciones probadas localmente
- [ ] Aplicación compila sin errores
- [ ] Repositorio en GitHub creado
- [ ] Plataforma de hosting elegida
- [ ] Variables de entorno configuradas
- [ ] Base de datos creada y conectada
- [ ] Primer deployment exitoso
- [ ] Usuarios de prueba creados
- [ ] SSL/HTTPS configurado

---

**¡Listo para producción! ??**

¿Necesitas ayuda? Revisa los logs o abre un issue en GitHub.
