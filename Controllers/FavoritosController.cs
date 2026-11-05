using System.Security.Claims;
using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class FavoritosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoritosController(ApplicationDbContext context) => _context = context;

        // "Mis favoritos": solo los del usuario que inició sesión
        public async Task<IActionResult> Index()
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ids = await _context.Favoritos
                .Where(f => f.UsuarioId == uid)
                .Select(f => f.DestinoId)
                .ToListAsync();

            var destinos = await _context.Destinos
                .Include(d => d.Provincia)
                .Include(d => d.Categoria)
                .Include(d => d.Caracteristicas).ThenInclude(dc => dc.Caracteristica)
                .Where(d => ids.Contains(d.Id))
                .OrderBy(d => d.Nombre)
                .ToListAsync();

            return View(destinos);
        }

        // Llamado por AJAX: agrega el favorito si no está, o lo quita si ya estaba
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!await _context.Destinos.AnyAsync(d => d.Id == id))
                return NotFound();

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var fav = await _context.Favoritos
                .FirstOrDefaultAsync(f => f.UsuarioId == uid && f.DestinoId == id);

            bool esFavorito;
            if (fav == null)
            {
                _context.Favoritos.Add(new Favorito { UsuarioId = uid, DestinoId = id, Fecha = DateTime.Now });
                esFavorito = true;
            }
            else
            {
                _context.Favoritos.Remove(fav);
                esFavorito = false;
            }

            await _context.SaveChangesAsync();
            return Json(new
            {
                favorito = esFavorito,
                mensaje = esFavorito ? "Agregado a tus favoritos" : "Quitado de tus favoritos"
            });
        }
    }
}