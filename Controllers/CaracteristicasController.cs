using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CaracteristicasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaracteristicasController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
            => View(await _context.Caracteristicas.OrderBy(c => c.Nombre).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Icono")] CaracteristicaAccesibilidad item)
        {
            if (await _context.Caracteristicas.AnyAsync(c => c.Nombre == item.Nombre))
                ModelState.AddModelError(nameof(item.Nombre), "Ya existe una característica con ese nombre.");

            if (!ModelState.IsValid) return View(item);

            _context.Add(item);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La característica se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Caracteristicas.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Icono")] CaracteristicaAccesibilidad item)
        {
            if (id != item.Id) return NotFound();

            if (await _context.Caracteristicas.AnyAsync(c => c.Nombre == item.Nombre && c.Id != item.Id))
                ModelState.AddModelError(nameof(item.Nombre), "Ya existe una característica con ese nombre.");

            if (!ModelState.IsValid) return View(item);

            _context.Update(item);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La característica se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var item = await _context.Caracteristicas.FindAsync(id);
            if (item == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Caracteristicas.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "La característica se eliminó correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "No se puede eliminar: hay destinos que usan esta característica.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}