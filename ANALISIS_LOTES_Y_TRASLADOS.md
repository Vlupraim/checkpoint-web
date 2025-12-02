# ?? Análisis Completo: Lógica de Lotes, Traslados y Stock

## ? **PROBLEMAS ENCONTRADOS**

### 1. **Error Crítico en Stock.cs** ??
```csharp
if (Cantidad <0) Cantidad =0;  // ? MAL: Espacio faltante y lógica incorrecta
```
**Problema:** 
- Sintaxis incorrecta (`<0` en lugar de `< 0`)
- **Lógica peligrosa**: Si el stock queda negativo, lo pone en 0 (oculta errores)

**Impacto:** Permite crear traslados que dejan stock negativo sin fallar

---

### 2. **Unidad Hardcodeada en MovimientoService**
```csharp
Unidad = "u"  // ? Siempre "u" aunque el producto use "kg", "litros", etc.
```
**Impacto:** La unidad en Stock no refleja la unidad real del producto

---

### 3. **Missing Unit en Movimiento.cshtml.cs**
El modelo no incluye la `Unidad` del producto al crear el `SelectList`

---

### 4. **Validación de FK UsuarioId**
El sistema usa `User.Identity.Name` (email) en lugar de `ClaimTypes.NameIdentifier` (GUID)

---

## ? **CORRECCIONES A APLICAR**

### 1. Corregir Stock.cs
```csharp
public void Decrementar(decimal cantidad)
{
    if (cantidad > Cantidad)
        throw new InvalidOperationException($"Stock insuficiente. Disponible: {Cantidad}, Solicitado: {cantidad}");
    
    Cantidad -= cantidad;
    ActualizadoEn = DateTime.UtcNow;
}
```

### 2. Usar Unidad del Producto
```csharp
var producto = await _context.Productos.FindAsync(lote.ProductoId);
stock.Unidad = producto?.Unidad ?? "u";
```

### 3. Validar Stock ANTES de Crear Movimiento
Agregar validación explícita en `CrearTrasladoAsync`

---

## ?? **ESTRUCTURA CORRECTA EN POSTGRESQL**

### Tabla `Lotes`
```sql
"Estado" INTEGER NOT NULL  -- 0=Cuarentena, 2=Liberado, etc.
```

### Tabla `Stocks`
```sql
"LoteId" UUID NOT NULL
"UbicacionId" UUID NOT NULL
"Cantidad" DECIMAL(18,3) NOT NULL
```

### Tabla `Movimientos`
```sql
"UsuarioId" VARCHAR(450)  -- Debe ser el GUID del usuario, no el email
```

---

## ?? **FLUJO CORRECTO DE UN TRASLADO**

1. **Validar Usuario** ? Obtener UserId (GUID) con `ClaimTypes.NameIdentifier`
2. **Validar Lote** ? Estado debe ser `Liberado` (2)
3. **Validar Stock Origen** ? Verificar que existe y tiene cantidad suficiente
4. **Crear Movimiento** ? Registrar en tabla `Movimientos`
5. **Actualizar Stock Origen** ? Decrementar cantidad (con validación)
6. **Actualizar/Crear Stock Destino** ? Incrementar cantidad
7. **Registrar Auditoría** ? Log del movimiento

---

## ?? **CAMBIOS IMPLEMENTADOS**

- ? Corregida sintaxis en `Stock.Decrementar()`
- ? Agregada validación de stock insuficiente
- ? Uso correcto de `ClaimTypes.NameIdentifier`
- ? Unidad del producto en Stock
- ? Eliminados mensajes de diagnóstico
- ? ValidationSummary condicional

---

Generado: 2025-12-02
