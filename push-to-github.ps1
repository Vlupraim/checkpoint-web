# Script para push confiable a GitHub
Write-Host "=== PUSH TO GITHUB ===" -ForegroundColor Green

# Verificar estado
Write-Host "`nEstado actual:" -ForegroundColor Yellow
git status

# Agregar cambios
Write-Host "`nAgregando cambios..." -ForegroundColor Yellow
git add .

# Commit (si hay cambios)
Write-Host "`nCreando commit..." -ForegroundColor Yellow
git commit -m "Corregir NotificationsViewComponent - usar UserId en lugar de email"

# Push
Write-Host "`nHaciendo push..." -ForegroundColor Yellow
git push origin main

Write-Host "`n=== PUSH COMPLETADO ===" -ForegroundColor Green
