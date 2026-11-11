using ArgentinaAccesible.Data;
using ArgentinaAccesible.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AuditoriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(string? entidad)
        {
            var query = _context.RegistrosAuditoria.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entidad))
                query = query.Where(r => r.Entidad == entidad);

            // Totales por tipo de acción, calculados en la base de datos
            var conteos = await query
                .GroupBy(r => r.Accion)
                .Select(g => new { Accion = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            int Contar(string accion) => conteos.FirstOrDefault(c => c.Accion == accion)?.Cantidad ?? 0;

            var vm = new AuditoriaViewModel
            {
                Entidad = entidad,
                Altas = Contar("Creó"),
                Modificaciones = Contar("Modificó"),
                Bajas = Contar("Eliminó"),
                Registros = await query
                    .OrderByDescending(r => r.Fecha)
                    .ThenByDescending(r => r.Id)
                    .Take(200)
                    .ToListAsync()
            };
            vm.Total = vm.Altas + vm.Modificaciones + vm.Bajas;

            return View(vm);
        }
    }
}