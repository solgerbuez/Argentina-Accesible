using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using ArgentinaAccesible.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Controllers
{
    public class GuiasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ExtImagen = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxImagen = 2 * 1024 * 1024;   // 2 MB
        private const long MaxPdf = 10 * 1024 * 1024;     // 10 MB

        public GuiasController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // Público: listado para todos
        public async Task<IActionResult> Index()
        {
            var guias = await _context.Guias.OrderBy(g => g.Titulo).ToListAsync();
            return View(guias);
        }

        // Panel de administración
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Admin()
        {
            var guias = await _context.Guias.OrderBy(g => g.Titulo).ToListAsync();
            return View(guias);
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Create() => View(new GuiaFormViewModel());

        [Authorize(Roles = "Administrador")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GuiaFormViewModel vm)
        {
            if (vm.ArchivoPdf == null)
                ModelState.AddModelError(nameof(vm.ArchivoPdf), "Tenés que subir el PDF de la guía.");

            await ValidarArchivosAsync(vm);
            if (!ModelState.IsValid) return View(vm);

            var guia = new Guia
            {
                Titulo = vm.Titulo,
                ImagenUrl = vm.Imagen != null ? await GuardarAsync(vm.Imagen, "img") : string.Empty,
                ArchivoPdf = await GuardarAsync(vm.ArchivoPdf!, "pdf"),
                CreadoPor = User.Identity?.Name ?? "desconocido",
                FechaCreacion = DateTime.Now
            };

            _context.Guias.Add(guia);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La guía se creó correctamente.";
            return RedirectToAction(nameof(Admin));
        }

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var guia = await _context.Guias.FindAsync(id);
            if (guia == null) return NotFound();

            return View(new GuiaFormViewModel
            {
                Id = guia.Id,
                Titulo = guia.Titulo,
                ImagenActual = guia.ImagenUrl,
                ArchivoPdfActual = guia.ArchivoPdf
            });
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GuiaFormViewModel vm)
        {
            if (id != vm.Id) return NotFound();
            var guia = await _context.Guias.FindAsync(id);
            if (guia == null) return NotFound();

            await ValidarArchivosAsync(vm);
            if (!ModelState.IsValid)
            {
                vm.ImagenActual = guia.ImagenUrl;
                vm.ArchivoPdfActual = guia.ArchivoPdf;
                return View(vm);
            }

            guia.Titulo = vm.Titulo;

            if (vm.Imagen != null)
            {
                BorrarSiEsSubido(guia.ImagenUrl);
                guia.ImagenUrl = await GuardarAsync(vm.Imagen, "img");
            }
            if (vm.ArchivoPdf != null)
            {
                BorrarSiEsSubido(guia.ArchivoPdf);
                guia.ArchivoPdf = await GuardarAsync(vm.ArchivoPdf, "pdf");
            }

            guia.ModificadoPor = User.Identity?.Name ?? "desconocido";
            guia.FechaModificacion = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Exito"] = "La guía se actualizó correctamente.";
            return RedirectToAction(nameof(Admin));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var guia = await _context.Guias.FindAsync(id);
            if (guia != null)
            {
                BorrarSiEsSubido(guia.ImagenUrl);
                BorrarSiEsSubido(guia.ArchivoPdf);
                _context.Guias.Remove(guia);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "La guía se eliminó correctamente.";
            }
            return RedirectToAction(nameof(Admin));
        }

        // ---------- Helpers ----------

        private async Task ValidarArchivosAsync(GuiaFormViewModel vm)
        {
            if (vm.Imagen != null)
            {
                var ext = Path.GetExtension(vm.Imagen.FileName).ToLowerInvariant();
                if (!ExtImagen.Contains(ext))
                    ModelState.AddModelError(nameof(vm.Imagen), "La imagen debe ser JPG, PNG o WEBP.");
                else if (vm.Imagen.Length > MaxImagen)
                    ModelState.AddModelError(nameof(vm.Imagen), "La imagen no puede superar los 2 MB.");
            }

            if (vm.ArchivoPdf != null)
            {
                var ext = Path.GetExtension(vm.ArchivoPdf.FileName).ToLowerInvariant();
                if (ext != ".pdf")
                    ModelState.AddModelError(nameof(vm.ArchivoPdf), "El archivo debe ser un PDF.");
                else if (vm.ArchivoPdf.Length > MaxPdf)
                    ModelState.AddModelError(nameof(vm.ArchivoPdf), "El PDF no puede superar los 10 MB.");
                else
                {
                    // Un PDF real empieza con los bytes "%PDF"
                    var cabecera = new byte[4];
                    using var s = vm.ArchivoPdf.OpenReadStream();
                    await s.ReadAsync(cabecera, 0, 4);
                    if (System.Text.Encoding.ASCII.GetString(cabecera) != "%PDF")
                        ModelState.AddModelError(nameof(vm.ArchivoPdf), "El archivo no es un PDF válido.");
                }
            }
        }

        private async Task<string> GuardarAsync(IFormFile archivo, string tipo)
        {
            var carpeta = Path.Combine(_env.WebRootPath, "uploads", "guias", tipo);
            Directory.CreateDirectory(carpeta);

            // Nombre único: evita pisar archivos y no usa el nombre que manda el usuario
            var nombre = Guid.NewGuid() + Path.GetExtension(archivo.FileName).ToLowerInvariant();
            using var fs = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
            await archivo.CopyToAsync(fs);

            return $"/uploads/guias/{tipo}/{nombre}";
        }

        // Solo borra lo que subimos nosotros; los archivos del seed no se tocan
        private void BorrarSiEsSubido(string? url)
        {
            if (string.IsNullOrEmpty(url) || !url.StartsWith("/uploads/")) return;
            var ruta = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }
}