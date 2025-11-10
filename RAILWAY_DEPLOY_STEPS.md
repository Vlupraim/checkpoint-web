# PASOS PARA HACER DEPLOY EN RAILWAY

## ? TODO LISTO - AHORA SIGUE ESTOS PASOS:

### 1?? Commit y Push a GitHub (hazlo desde tu terminal)

```bash
# En la raíz del proyecto (donde está checkpoint-web.sln)
git add .
git commit -m "Add Railway support with PostgreSQL and nixpacks configuration"
git push
```

### 2?? Railway Re-deploy

Railway detectará el push automáticamente y volverá a intentar el deploy.

Si no empieza automáticamente:
1. Ve a tu proyecto en Railway
2. Click en "checkpoint-web" (el servicio que falló)
3. Click en los 3 puntos (...) ? "Redeploy"

### 3?? Verificar Variables de Entorno en Railway

Ve a tu proyecto ? checkpoint-web ? Variables

**Deben existir estas variables:**

```env
DATABASE_URL=postgresql://...  (Esta la pone Railway automáticamente desde Postgres)
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

Si NO existe `DATABASE_URL`:
1. Verifica que tienes PostgreSQL agregado
2. En Railway: "New" ? "Database" ? "Add PostgreSQL"
3. Railway conectará automáticamente `DATABASE_URL`

### 4?? Ver Logs en Tiempo Real

En Railway ? Deployments ? Latest deployment ? "View logs"

Deberías ver:
```
? Building .NET 8 application...
? Restoring packages...
? Publishing application...
? Starting application...
```

### 5?? Aplicar Migraciones (DESPUÉS del primer deploy exitoso)

**Opción A: Railway CLI** (Recomendado)
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

**Opción B: Desde el código** (Ya está configurado)
El `Program.cs` ahora aplica migraciones automáticamente en producción al iniciar.

### 6?? Obtener URL de tu App

1. En Railway ? checkpoint-web ? Settings ? Networking
2. Click "Generate Domain"
3. Tu app estará en: `https://checkpoint-web-production-XXXX.up.railway.app`

---

## ?? SI FALLA EL BUILD:

### Error: "Failed to build image"
**Causa:** Railway no encuentra el archivo `.csproj`

**Solución:** Verifica que `nixpacks.toml` tiene el path correcto:
```toml
"dotnet restore checkpoint-web/checkpoint-web.csproj"
```

### Error: "No DATABASE_URL found"
**Solución:**
1. Ve a Railway ? New ? Database ? Add PostgreSQL
2. Railway lo conectará automáticamente

### Error: "Port binding error"
**Solución:** Verifica en Variables:
```env
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

### Ver logs detallados:
```bash
railway logs
```

---

## ? CHECKLIST FINAL:

- [ ] `nixpacks.toml` creado
- [ ] `railway.json` actualizado
- [ ] `Program.cs` con soporte PostgreSQL
- [ ] Paquete `Npgsql.EntityFrameworkCore.PostgreSQL` instalado
- [ ] Git commit y push realizados
- [ ] PostgreSQL agregado en Railway
- [ ] Variables de entorno configuradas
- [ ] Deploy en progreso
- [ ] Logs sin errores
- [ ] URL generada y funcionando

---

## ?? USUARIOS DE PRUEBA:

Después del primer deploy exitoso, estos usuarios estarán disponibles:

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

**¡Haz el push ahora y Railway debería funcionar! ??**
