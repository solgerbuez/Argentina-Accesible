using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class TestimoniosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimoniosController(ApplicationDbContext context) => _context = context;

        // Los pendientes de aprobación se muestran primero
        public async Task<IActionResult> Index()
            => View(await _context.Testimonios
                .OrderByDescending(t => t.Pendiente)
                .ThenByDescending(t => t.FechaCreacion)
                .ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Localidad,Texto")] Testimonio testimonio)
        {
            if (!ModelState.IsValid) return View(testimonio);

            // La auditoría la completa el servidor, nunca el formulario
            testimonio.CreadoPor = User.Identity?.Name ?? "desconocido";
            testimonio.FechaCreacion = DateTime.Now;

            _context.Add(testimonio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "El testimonio se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var testimonio = await _context.Testimonios.FindAsync(id);
            if (testimonio == null) return NotFound();
            return View(testimonio);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Localidad,Texto")] Testimonio datos)
        {
            if (id != datos.Id) return NotFound();
            if (!ModelState.IsValid) return View(datos);

            // Se carga el registro original para no perder CreadoPor ni FechaCreacion
            var testimonio = await _context.Testimonios.FindAsync(id);
            if (testimonio == null) return NotFound();

            testimonio.Nombre = datos.Nombre;
            testimonio.Localidad = datos.Localidad;
            testimonio.Texto = datos.Texto;
            testimonio.ModificadoPor = User.Identity?.Name ?? "desconocido";
            testimonio.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Exito"] = "El testimonio se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // El administrador aprueba un testimonio enviado por un usuario: desde ahí se muestra al público
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var testimonio = await _context.Testimonios.FindAsync(id);
            if (testimonio != null && testimonio.Pendiente)
            {
                testimonio.Pendiente = false;
                testimonio.ModificadoPor = User.Identity?.Name ?? "desconocido";
                testimonio.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Exito"] = "El testimonio se aprobó y ya se muestra en el sitio.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var testimonio = await _context.Testimonios.FindAsync(id);
            if (testimonio != null)
            {
                _context.Testimonios.Remove(testimonio);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "El testimonio se eliminó correctamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}