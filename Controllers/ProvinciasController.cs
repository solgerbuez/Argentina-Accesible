using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProvinciasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProvinciasController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
            => View(await _context.Provincias.OrderBy(p => p.Nombre).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Provincia provincia)
        {
            if (await _context.Provincias.AnyAsync(p => p.Nombre == provincia.Nombre))
                ModelState.AddModelError(nameof(provincia.Nombre), "Ya existe una provincia con ese nombre.");

            if (!ModelState.IsValid) return View(provincia);

            _context.Add(provincia);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La provincia se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var provincia = await _context.Provincias.FindAsync(id);
            if (provincia == null) return NotFound();
            return View(provincia);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Provincia provincia)
        {
            if (id != provincia.Id) return NotFound();

            if (await _context.Provincias.AnyAsync(p => p.Nombre == provincia.Nombre && p.Id != provincia.Id))
                ModelState.AddModelError(nameof(provincia.Nombre), "Ya existe una provincia con ese nombre.");

            if (!ModelState.IsValid) return View(provincia);

            _context.Update(provincia);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La provincia se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var provincia = await _context.Provincias.FindAsync(id);
            if (provincia == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Provincias.Remove(provincia);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "La provincia se eliminó correctamente.";
            }
            catch (DbUpdateException)
            {
                // Hay destinos que usan esta provincia: la base de datos no deja borrarla
                TempData["Error"] = "No se puede eliminar: hay destinos que usan esta provincia.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}