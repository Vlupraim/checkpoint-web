using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using checkpoint_web.Data;
using checkpoint_web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace checkpoint_web.Pages.Bodega
{
    [Authorize(Roles = "PersonalBodega,ControlCalidad")]
    public class StockModel : PageModel
    {
        private readonly CheckpointDbContext _context;

        public StockModel(CheckpointDbContext context)
        {
            _context = context;
        }

        public IList<Stock> Stocks { get; set; } = new List<Stock>();
        public SelectList Sedes { get; set; } = new SelectList(new List<Sede>(), "Id", "Nombre");
        public SelectList Ubicaciones { get; set; } = new SelectList(new List<Ubicacion>(), "Id", "Codigo");

        [BindProperty(SupportsGet = true)]
        public string? Producto { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? SedeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? UbicacionId { get; set; }

        public async Task OnGetAsync()
        {
            // Cargar Sedes y Ubicaciones para los filtros
            var sedes = await _context.Sedes
                .Where(s => s.Activa)
                .AsNoTracking()
                .ToListAsync();
            Sedes = new SelectList(sedes, "Id", "Nombre", SedeId);
            
            var ubicaciones = await _context.Ubicaciones
                .Include(u => u.Sede)
                .AsNoTracking()
                .ToListAsync();
            Ubicaciones = new SelectList(
                ubicaciones.Select(u => new { 
                    u.Id, 
                    Codigo = $"{u.Sede?.Nombre} - {u.Codigo}" 
                }), 
                "Id", 
                "Codigo",
                UbicacionId
            );

            // Consultar stocks con filtros aplicados
            var query = _context.Stocks
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Producto)
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Proveedor)
                .Include(s => s.Ubicacion)
                    .ThenInclude(u => u!.Sede)
                .Where(s => s.Cantidad > 0)
                .AsQueryable();

            // Aplicar filtro de producto (nombre o SKU) - CASE INSENSITIVE para PostgreSQL
            if (!string.IsNullOrWhiteSpace(Producto))
            {
                var productoLower = Producto.ToLower();
                query = query.Where(s => 
                    EF.Functions.ILike(s.Lote!.Producto!.Nombre, $"%{Producto}%") ||
                    EF.Functions.ILike(s.Lote!.Producto!.Sku, $"%{Producto}%")
                );
            }

            // Aplicar filtro de sede
            if (SedeId.HasValue)
            {
                query = query.Where(s => s.Ubicacion!.SedeId == SedeId.Value);
            }

            // Aplicar filtro de ubicación
            if (UbicacionId.HasValue)
            {
                query = query.Where(s => s.UbicacionId == UbicacionId.Value);
            }

            Stocks = await query
                .OrderBy(s => s.Lote!.Producto!.Nombre)
                .ThenBy(s => s.Ubicacion!.Codigo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            // Configurar licencia de EPPlus (NonCommercial para uso no comercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Consultar stocks con los mismos filtros
            var query = _context.Stocks
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Producto)
                .Include(s => s.Lote)
                    .ThenInclude(l => l!.Proveedor)
                .Include(s => s.Ubicacion)
                    .ThenInclude(u => u!.Sede)
                .Where(s => s.Cantidad > 0)
                .AsQueryable();

            // Aplicar mismos filtros que OnGetAsync
            if (!string.IsNullOrWhiteSpace(Producto))
            {
                query = query.Where(s => 
                    EF.Functions.ILike(s.Lote!.Producto!.Nombre, $"%{Producto}%") ||
                    EF.Functions.ILike(s.Lote!.Producto!.Sku, $"%{Producto}%")
                );
            }

            if (SedeId.HasValue)
            {
                query = query.Where(s => s.Ubicacion!.SedeId == SedeId.Value);
            }

            if (UbicacionId.HasValue)
            {
                query = query.Where(s => s.UbicacionId == UbicacionId.Value);
            }

            var stocks = await query
                .OrderBy(s => s.Lote!.Producto!.Nombre)
                .ThenBy(s => s.Ubicacion!.Codigo)
                .AsNoTracking()
                .ToListAsync();

            // Crear archivo Excel
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Stock Actual");

            // Encabezados
            worksheet.Cells[1, 1].Value = "Producto";
            worksheet.Cells[1, 2].Value = "SKU";
            worksheet.Cells[1, 3].Value = "Lote";
            worksheet.Cells[1, 4].Value = "Sede";
            worksheet.Cells[1, 5].Value = "Ubicación";
            worksheet.Cells[1, 6].Value = "Cantidad";
            worksheet.Cells[1, 7].Value = "Unidad";
            worksheet.Cells[1, 8].Value = "Estado Lote";
            worksheet.Cells[1, 9].Value = "Última Actualización";

            // Estilo de encabezados
            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }

            // Datos
            int row = 2;
            foreach (var stock in stocks)
            {
                worksheet.Cells[row, 1].Value = stock.Lote?.Producto?.Nombre ?? "N/A";
                worksheet.Cells[row, 2].Value = stock.Lote?.Producto?.Sku ?? "-";
                worksheet.Cells[row, 3].Value = stock.Lote?.CodigoLote ?? "-";
                worksheet.Cells[row, 4].Value = stock.Ubicacion?.Sede?.Nombre ?? "N/A";
                worksheet.Cells[row, 5].Value = stock.Ubicacion?.Codigo ?? "N/A";
                worksheet.Cells[row, 6].Value = stock.Cantidad;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.000";
                worksheet.Cells[row, 7].Value = stock.Unidad;
                worksheet.Cells[row, 8].Value = stock.Lote?.Estado.ToString() ?? "N/A";
                worksheet.Cells[row, 9].Value = stock.ActualizadoEn.ToString("dd/MM/yyyy HH:mm");

                // Color de fondo según estado del lote
                var estadoColor = stock.Lote?.Estado switch
                {
                    EstadoLote.Liberado => Color.FromArgb(198, 239, 206),
                    EstadoLote.Cuarentena => Color.FromArgb(255, 235, 156),
                    EstadoLote.Rechazado => Color.FromArgb(255, 199, 206),
                    EstadoLote.Bloqueado => Color.FromArgb(217, 217, 217),
                    _ => Color.White
                };

                worksheet.Cells[row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row, 8].Style.Fill.BackgroundColor.SetColor(estadoColor);

                row++;
            }

            // Ajustar ancho de columnas
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Congelar primera fila
            worksheet.View.FreezePanes(2, 1);

            // Agregar filtros
            worksheet.Cells[1, 1, row - 1, 9].AutoFilter = true;

            // Agregar resumen al final
            row++;
            worksheet.Cells[row, 1].Value = "RESUMEN";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 6].Value = stocks.Sum(s => s.Cantidad);
            worksheet.Cells[row, 6].Style.Font.Bold = true;
            worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.000";
            worksheet.Cells[row, 7].Value = "Total Registros";
            worksheet.Cells[row, 7].Style.Font.Bold = true;

            // Información del reporte
            row += 2;
            worksheet.Cells[row, 1].Value = $"Reporte generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            worksheet.Cells[row, 1].Style.Font.Italic = true;
            worksheet.Cells[row, 1].Style.Font.Size = 10;

            if (!string.IsNullOrEmpty(Producto) || SedeId.HasValue || UbicacionId.HasValue)
            {
                row++;
                worksheet.Cells[row, 1].Value = "Filtros aplicados: ";
                worksheet.Cells[row, 1].Style.Font.Italic = true;
                worksheet.Cells[row, 1].Style.Font.Size = 10;

                if (!string.IsNullOrEmpty(Producto))
                {
                    row++;
                    worksheet.Cells[row, 1].Value = $"  - Producto: {Producto}";
                    worksheet.Cells[row, 1].Style.Font.Italic = true;
                    worksheet.Cells[row, 1].Style.Font.Size = 9;
                }
            }

            // Generar archivo
            var stream = new MemoryStream(package.GetAsByteArray());
            var fileName = $"Stock_Checkpoint_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
