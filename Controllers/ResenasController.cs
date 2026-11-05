using System.Security.Claims;
using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using ArgentinaAccesible.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize]
    public class ResenasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResenasController(ApplicationDbContext context) => _context = context;

        // Crea la reseña del usuario o, si ya tenía una para ese destino, la actualiza
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Guardar(ResenaFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { errores });
            }

            if (!await _context.Destinos.AnyAsync(d => d.Id == vm.DestinoId))
                return NotFound();

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var existente = await _context.Resenas
                .FirstOrDefaultAsync(r => r.DestinoId == vm.DestinoId && r.UsuarioId == uid);

            string mensaje;
            if (existente == null)
            {
                _context.Resenas.Add(new Resena
                {
                    DestinoId = vm.DestinoId,
                    UsuarioId = uid,
                    Puntaje = vm.Puntaje,
                    Comentario = vm.Comentario.Trim(),
                    Fecha = DateTime.Now
                });
                mensaje = "¡Gracias por tu reseña!";
            }
            else
            {
                existente.Puntaje = vm.Puntaje;
                existente.Comentario = vm.Comentario.Trim();
                existente.Fecha = DateTime.Now;
                mensaje = "Tu reseña se actualizó.";
            }

            await _context.SaveChangesAsync();
            return Json(new { ok = true, mensaje });
        }

        // El autor puede borrar la suya; el administrador puede borrar cualquiera
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resena = await _context.Resenas.FindAsync(id);
            if (resena == null) return NotFound();

            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (resena.UsuarioId != uid && !User.IsInRole("Administrador"))
                return Forbid();

            _context.Resenas.Remove(resena);
            await _context.SaveChangesAsync();
            return Json(new { ok = true, mensaje = "La reseña se eliminó." });
        }
    }
}