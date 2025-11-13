-- ============================================
-- SCRIPT COMPLETO: Crear tabla Tareas y TareaComentarios
-- Fecha: 2025-01-13
-- Descripción: Sistema completo de gestión de tareas
-- ============================================

-- Crear tabla Tareas si no existe
CREATE TABLE IF NOT EXISTS "Tareas" (
    "Id" SERIAL PRIMARY KEY,
    "Titulo" VARCHAR(200) NOT NULL,
    "Descripcion" VARCHAR(2000),
    "Estado" VARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    "Prioridad" VARCHAR(20) NOT NULL DEFAULT 'Media',
    "Tipo" VARCHAR(50),
 "FechaCreacion" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "FechaInicio" TIMESTAMP WITH TIME ZONE,
    "FechaLimite" TIMESTAMP WITH TIME ZONE,
    "FechaFinalizacion" TIMESTAMP WITH TIME ZONE,
    "CreadoPor" VARCHAR(450),
    "ResponsableId" VARCHAR(450),
    "Progreso" INTEGER NOT NULL DEFAULT 0 CHECK ("Progreso" >= 0 AND "Progreso" <= 100),
    "Observaciones" VARCHAR(2000),
    "ProductoId" UUID,
    "LoteId" UUID,
    "Historial" TEXT,
    "Activo" BOOLEAN NOT NULL DEFAULT true,
    
    -- Foreign Keys
    CONSTRAINT "FK_Tareas_Productos_ProductoId" 
        FOREIGN KEY ("ProductoId") 
        REFERENCES "Productos"("Id") 
        ON DELETE SET NULL,
    
    CONSTRAINT "FK_Tareas_Lotes_LoteId" 
    FOREIGN KEY ("LoteId") 
      REFERENCES "Lotes"("Id") 
        ON DELETE SET NULL
);

-- Crear tabla TareaComentarios si no existe
CREATE TABLE IF NOT EXISTS "TareaComentarios" (
    "Id" SERIAL PRIMARY KEY,
    "TareaId" INTEGER NOT NULL,
    "UsuarioId" VARCHAR(450) NOT NULL,
    "Contenido" VARCHAR(2000) NOT NULL,
    "FechaCreacion" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "FechaEdicion" TIMESTAMP WITH TIME ZONE,
    "Activo" BOOLEAN NOT NULL DEFAULT true,
    
    -- Foreign Keys
    CONSTRAINT "FK_TareaComentarios_Tareas_TareaId" 
        FOREIGN KEY ("TareaId") 
        REFERENCES "Tareas"("Id") 
        ON DELETE CASCADE,
    
    CONSTRAINT "FK_TareaComentarios_AspNetUsers_UsuarioId" 
        FOREIGN KEY ("UsuarioId") 
        REFERENCES "AspNetUsers"("Id") 
   ON DELETE RESTRICT
);

-- Crear índices para Tareas
CREATE INDEX IF NOT EXISTS "IX_Tareas_Estado" ON "Tareas"("Estado");
CREATE INDEX IF NOT EXISTS "IX_Tareas_ResponsableId" ON "Tareas"("ResponsableId");
CREATE INDEX IF NOT EXISTS "IX_Tareas_FechaLimite" ON "Tareas"("FechaLimite");
CREATE INDEX IF NOT EXISTS "IX_Tareas_ProductoId" ON "Tareas"("ProductoId");
CREATE INDEX IF NOT EXISTS "IX_Tareas_LoteId" ON "Tareas"("LoteId");
CREATE INDEX IF NOT EXISTS "IX_Tareas_Activo" ON "Tareas"("Activo");

-- Crear índices para TareaComentarios
CREATE INDEX IF NOT EXISTS "IX_TareaComentarios_TareaId" ON "TareaComentarios"("TareaId");
CREATE INDEX IF NOT EXISTS "IX_TareaComentarios_UsuarioId" ON "TareaComentarios"("UsuarioId");
CREATE INDEX IF NOT EXISTS "IX_TareaComentarios_FechaCreacion" ON "TareaComentarios"("FechaCreacion" DESC);

-- Comentarios de documentación
COMMENT ON TABLE "Tareas" IS 'Sistema de gestión de tareas y procesos asignados a usuarios';
COMMENT ON TABLE "TareaComentarios" IS 'Comentarios y notas en tareas para colaboración';

COMMENT ON COLUMN "Tareas"."Estado" IS 'Estados: Pendiente, EnProgreso, Finalizada, Cancelada, Bloqueada';
COMMENT ON COLUMN "Tareas"."Prioridad" IS 'Prioridad: Baja, Media, Alta, Urgente';
COMMENT ON COLUMN "Tareas"."Tipo" IS 'Tipo: Operativa, Administrativa, Calidad, Mantenimiento';
COMMENT ON COLUMN "Tareas"."Progreso" IS 'Progreso de 0 a 100';
COMMENT ON COLUMN "Tareas"."Historial" IS 'Historial de cambios en formato JSON';

-- Verificar las tablas creadas
SELECT 
    'Tareas' as tabla,
    COUNT(*) as registros
FROM "Tareas"
UNION ALL
SELECT 
    'TareaComentarios' as tabla,
    COUNT(*) as registros
FROM "TareaComentarios";

-- Script completado exitosamente
SELECT 'Tablas Tareas y TareaComentarios creadas exitosamente' as resultado;
