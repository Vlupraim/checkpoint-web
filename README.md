# Checkpoint - Sistema de Gestión de Inventario

Sistema completo de gestión de inventario con control de calidad para empresas productoras.

## ?? Características

- ? Gestión completa de inventario
- ? Control de calidad con workflow de aprobación
- ? Movimientos internos (traslados, salidas, devoluciones)
- ? Ajustes de inventario con aprobación
- ? Reportes en tiempo real
- ? Sistema de notificaciones
- ? Auditoría completa
- ? Multi-roles (Admin, Bodega, Calidad)

## ??? Stack Tecnológico

- **Backend:** ASP.NET Core 8.0 (Razor Pages)
- **Base de Datos:** SQL Server
- **ORM:** Entity Framework Core 8.0
- **Autenticación:** ASP.NET Core Identity
- **UI:** Bootstrap 5

## ?? Requisitos

- .NET 8.0 SDK
- SQL Server 2019+ (o SQL Server Express)
- Visual Studio 2022 o VS Code

## ?? Configuración Local

### 1. Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/checkpoint-web.git
cd checkpoint-web
```

### 2. Configurar la Base de Datos

Edita `appsettings.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CheckpointDev;Trusted_Connection=True;"
  }
}
```

### 3. Aplicar Migraciones

```bash
cd checkpoint-web
dotnet ef database update
```

### 4. Ejecutar la Aplicación

```bash
dotnet run
```

La aplicación estará disponible en: `https://localhost:7088`

## ?? Usuarios por Defecto

| Email | Contraseña | Rol |
|-------|-----------|-----|
| admin@example.com | Admin123! | Administrador |
| bodega@example.com | Bodega123! | Personal Bodega |
| calidad@example.com | Calidad123! | Control Calidad |

## ?? Deployment a Railway

### 1. Push a GitHub
```bash
git add .
git commit -m "Initial commit"
git push origin main
```

### 2. Conectar con Railway
1. Ve a [Railway.app](https://railway.app)
2. "New Project" ? "Deploy from GitHub repo"
3. Selecciona tu repositorio
4. Railway detectará automáticamente .NET

### 3. Agregar Base de Datos
1. En tu proyecto Railway: "New" ? "Database" ? "PostgreSQL"
2. Railway proveerá automáticamente la variable `DATABASE_URL`

### 4. Variables de Entorno
Agrega en Railway:
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=...;Database=...
```

## ?? Estructura del Proyecto

```
checkpoint-web/
??? Data/                  # DbContext y migraciones
??? Models/                # Modelos de dominio
??? Services/  # Lógica de negocio
??? Pages/        # Razor Pages
?   ??? Admin/            # Módulos administrativos
?   ??? Bodega/# Operaciones de bodega
?   ??? Calidad/          # Control de calidad
?   ??? Reportes/     # Reportes
??? Middleware/            # Middleware personalizado
??? wwwroot/      # Archivos estáticos
```

## ?? Documentación Adicional

Ver `IMPLEMENTACION_COMPLETADA.md` para detalles completos de implementación.

## ?? Contribuir

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## ?? Licencia

Proyecto propietario - Todos los derechos reservados

## ????? Autor

Tu Nombre - [@tu_usuario](https://github.com/tu_usuario)

---

**¿Necesitas ayuda?** Abre un issue en el repositorio.
