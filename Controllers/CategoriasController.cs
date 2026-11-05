using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
            => View(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync());

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria categoria)
        {
            if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
                ModelState.AddModelError(nameof(categoria.Nombre), "Ya existe una categoría con ese nombre.");

            if (!ModelState.IsValid) return View(categoria);

            _context.Add(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La categoría se creó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();

            if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre && c.Id != categoria.Id))
                ModelState.AddModelError(nameof(categoria.Nombre), "Ya existe una categoría con ese nombre.");

            if (!ModelState.IsValid) return View(categoria);

            _context.Update(categoria);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La categoría se actualizó correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return RedirectToAction(nameof(Index));

            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "La categoría se eliminó correctamente.";
            }
            catch (DbUpdateException)
            {
                // Hay destinos que usan esta categoría: la base de datos no deja borrarla
                TempData["Error"] = "No se puede eliminar: hay destinos que usan esta categoría.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}