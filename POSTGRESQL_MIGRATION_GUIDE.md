# Guía: Regenerar Migraciones para PostgreSQL

## El Problema
Las migraciones actuales fueron generadas para SQL Server, no PostgreSQL. Esto causa incompatibilidades de tipos de datos y el esquema no se aplica correctamente.

## Solución: Regenerar Migraciones

### Paso 1: Eliminar Migraciones Antiguas
```bash
cd checkpoint-web

# Eliminar TODOS los archivos de migración
Remove-Item -Path .\Migrations\*.cs -Force

# O manualmente borrar la carpeta Migrations completa
```

### Paso 2: Asegurar que estás usando PostgreSQL
Verifica que tu `appsettings.Development.json` tenga una connection string de PostgreSQL local:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=checkpoint_dev;Username=postgres;Password=tu_password"
  }
}
```

O simplemente configura la variable de entorno:
```powershell
$env:DATABASE_URL="postgresql://usuario:password@host:puerto/database"
```

### Paso 3: Regenerar Migración Inicial
```bash
# Desde el directorio checkpoint-web
dotnet ef migrations add InitialPostgreSQL --context CheckpointDbContext

# Revisar la migración generada
code .\Migrations\*InitialPostgreSQL.cs
```

### Paso 4: Aplicar a Base de Datos Local (Opcional - Para probar)
```bash
dotnet ef database update --context CheckpointDbContext
```

### Paso 5: Aplicar a Railway (PRODUCCIÓN)

#### Opción A: Desde Railway CLI
```bash
# Instalar Railway CLI si no lo tienes
npm install -g @railway/cli

# Login
railway login

# Conectar al proyecto
railway link

# Aplicar migraciones
railway run dotnet ef database update --project checkpoint-web --context CheckpointDbContext
```

#### Opción B: Script SQL Directo (MÁS RÁPIDO)
Si el script `database-schema.sql` ya fue ejecutado exitosamente:

1. Conéctate a Railway PostgreSQL con DBeaver o pgAdmin
2. Verifica que las tablas existen:
   ```sql
   SELECT table_name 
   FROM information_schema.tables 
   WHERE table_schema = 'public' 
   ORDER BY table_name;
   ```

3. Si las tablas YA existen, solo necesitas crear la tabla de migraciones:
   ```sql
   CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
       "MigrationId" VARCHAR(150) NOT NULL PRIMARY KEY,
       "ProductVersion" VARCHAR(32) NOT NULL
   );
   
-- Registrar que la migración ya se aplicó
   INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
   VALUES ('20251110_InitialPostgreSQL', '8.0.0')
   ON CONFLICT DO NOTHING;
   ```

#### Opción C: Deploy Automático
Descomenta el código de migraciones automáticas en `Program.cs`:

```csharp
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
{
        try
        {
 var db = scope.ServiceProvider.GetRequiredService<CheckpointDbContext>();
  Console.WriteLine("[STARTUP] Applying migrations...");
            await db.Database.MigrateAsync();
          Console.WriteLine("[STARTUP] Migrations applied successfully");
        }
        catch (Exception ex)
        {
  Console.WriteLine($"[STARTUP ERROR] Migration failed: {ex.Message}");
        }
    }
}
```

Luego haz push a GitHub y Railway aplicará las migraciones automáticamente.

## Verificación Final

Después de aplicar las migraciones, verifica en Railway:

```sql
-- Ver todas las tablas
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public';

-- Verificar algunos registros
SELECT * FROM "AspNetRoles" LIMIT 5;
SELECT * FROM "Productos" LIMIT 5;
```

## Notas Importantes

1. **NO mezcles SQL Server y PostgreSQL** - Usa SOLO PostgreSQL en producción
2. **Los tipos de datos cambian**:
   - SQL Server: `uniqueidentifier` ? PostgreSQL: `uuid`
   - SQL Server: `nvarchar(max)` ? PostgreSQL: `text`
   - SQL Server: `datetime2` ? PostgreSQL: `timestamp`
3. **Las migraciones de EF Core manejan esto automáticamente** cuando usas el provider correcto

## Troubleshooting

### Error: "relation does not exist"
Las tablas no se crearon. Verifica:
```bash
railway logs --follow
```

### Error: "column does not exist"
La estructura de las tablas no coincide con los modelos. Regenera las migraciones.

### Las tablas aparecen vacías en Railway UI
Railway UI a veces no refresca. Usa DBeaver o ejecuta:
```sql
\dt public.*
```
