using ArgentinaAccesible.Data;
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

            ViewData["Entidad"] = entidad;

            return View(await query
                .OrderByDescending(r => r.Fecha)
                .ThenByDescending(r => r.Id)
                .Take(200)
                .ToListAsync());
        }
    }
}