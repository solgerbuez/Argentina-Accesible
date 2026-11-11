using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using ArgentinaAccesible.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArgentinaAccesible.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                // Los 3 destinos con más favoritos reales; si hay empate, por nombre
                Destacados = await _context.Destinos
                    .Include(d => d.Provincia)
                    .OrderByDescending(d => _context.Favoritos.Count(f => f.DestinoId == d.Id))
                    .ThenBy(d => d.Nombre)
                    .Take(3)
                    .ToListAsync(),
                Testimonios = await _context.Testimonios
                    .Where(t => !t.Pendiente)
                    .OrderByDescending(t => t.FechaCreacion)
                    .Take(3)
                    .ToListAsync()
            };
            return View(vm);
        }

        public IActionResult QuienesSomos() => View();

        // Solo se muestran al público los testimonios ya aprobados
        public async Task<IActionResult> Testimonios()
        {
            var lista = await _context.Testimonios
                .Where(t => !t.Pendiente)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            // El administrador ve cuántos testimonios esperan su aprobación
            if (User.IsInRole("Administrador"))
                ViewData["Pendientes"] = await _context.Testimonios.CountAsync(t => t.Pendiente);

            return View(lista);
        }

        // Solo los usuarios (rol Usuario) pueden enviar testimonios
        [Authorize(Roles = "Usuario")]
        public IActionResult DejarTestimonio() => View(new TestimonioEnvioViewModel());

        [Authorize(Roles = "Usuario")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DejarTestimonio(TestimonioEnvioViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Queda pendiente hasta que el administrador lo apruebe.
            // Quién lo envió y cuándo lo completa el servidor, nunca el formulario.
            var testimonio = new Testimonio
            {
                Nombre = vm.Nombre,
                Localidad = vm.Localidad,
                Texto = vm.Texto,
                Pendiente = true,
                CreadoPor = User.Identity?.Name ?? "desconocido",
                FechaCreacion = DateTime.Now
            };

            _context.Testimonios.Add(testimonio);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "¡Gracias por compartir tu experiencia! Se va a publicar cuando el equipo la revise.";
            return RedirectToAction(nameof(Testimonios));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}