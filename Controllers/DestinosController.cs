using ArgentinaAccesible.Data;
using ArgentinaAccesible.Models;
using ArgentinaAccesible.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArgentinaAccesible.Controllers;

public class DestinosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _env;

    public DestinosController(ApplicationDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }


    public async Task<IActionResult> Index(string? busqueda)
    {
        var query = _context.Destinos
            .Include(d => d.Provincia)
            .Include(d => d.Categoria)
            .Include(d => d.Caracteristicas).ThenInclude(dc => dc.Caracteristica)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
            query = query.Where(d => d.Nombre.Contains(busqueda)
                || d.Descripcion.Contains(busqueda)
                || d.Provincia!.Nombre.Contains(busqueda)
                || d.Categoria!.Nombre.Contains(busqueda));


        var favoritos = new HashSet<int>();
        if (User.IsInRole("Usuario"))
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            favoritos = (await _context.Favoritos
                .Where(f => f.UsuarioId == uid)
                .Select(f => f.DestinoId)
                .ToListAsync()).ToHashSet();
        }
        ViewData["Favoritos"] = favoritos;

        ViewData["Busqueda"] = busqueda;
        return View(await query.OrderBy(d => d.Nombre).ToListAsync());
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var destino = await _context.Destinos
            .Include(d => d.Provincia)
            .Include(d => d.Categoria)
            .Include(d => d.Caracteristicas).ThenInclude(dc => dc.Caracteristica)
            .Include(d => d.Resenas).ThenInclude(r => r.Usuario)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id);
        if (destino == null) return NotFound();

        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var resenas = destino.Resenas.OrderByDescending(r => r.Fecha).ToList();

        var vm = new DetalleDestinoViewModel
        {
            Destino = destino,
            Resenas = resenas,
            Promedio = resenas.Count > 0 ? resenas.Average(r => r.Puntaje) : null,
            MiResena = uid == null ? null : resenas.FirstOrDefault(r => r.UsuarioId == uid)
        };
        return PartialView("_DetalleDestino", vm);
    }


    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Admin()
    {
        var destinos = await _context.Destinos
            .Include(d => d.Provincia)
            .Include(d => d.Categoria)
            .OrderBy(d => d.Nombre)
            .ToListAsync();
        return View(destinos);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create()
    {
        var vm = new DestinoFormViewModel();
        await CargarListas(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Create(DestinoFormViewModel vm)
    {
        if (vm.Imagen != null && !ImagenValida(vm.Imagen, out var error))
            ModelState.AddModelError(nameof(vm.Imagen), error);

        if (!ModelState.IsValid)
        {
            await CargarListas(vm);
            return View(vm);
        }

        var destino = new Destino
        {
            Nombre = vm.Nombre,
            Descripcion = vm.Descripcion,
            Direccion = vm.Direccion,
            CategoriaId = vm.CategoriaId,
            ProvinciaId = vm.ProvinciaId,
            CreadoPor = User.Identity?.Name,
            FechaCreacion = DateTime.Now,
            Caracteristicas = vm.CaracteristicasSeleccionadas
                .Select(id => new DestinoCaracteristica { CaracteristicaAccesibilidadId = id })
                .ToList()
        };

        if (vm.Imagen != null)
            destino.ImagenUrl = await GuardarImagen(vm.Imagen);

        _context.Destinos.Add(destino);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Destino creado correctamente.";
        return RedirectToAction(nameof(Admin));
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var d = await _context.Destinos
            .Include(x => x.Caracteristicas)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (d == null) return NotFound();

        var vm = new DestinoFormViewModel
        {
            Id = d.Id,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion,
            Direccion = d.Direccion,
            CategoriaId = d.CategoriaId,
            ProvinciaId = d.ProvinciaId,
            ImagenUrl = d.ImagenUrl,
            CaracteristicasSeleccionadas = d.Caracteristicas
                .Select(c => c.CaracteristicaAccesibilidadId).ToList()
        };
        await CargarListas(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Edit(int id, DestinoFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var destino = await _context.Destinos
            .Include(d => d.Caracteristicas)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (destino == null) return NotFound();

        if (vm.Imagen != null && !ImagenValida(vm.Imagen, out var error))
            ModelState.AddModelError(nameof(vm.Imagen), error);

        if (!ModelState.IsValid)
        {
            vm.ImagenUrl = destino.ImagenUrl;
            await CargarListas(vm);
            return View(vm);
        }

        destino.Nombre = vm.Nombre;
        destino.Descripcion = vm.Descripcion;
        destino.Direccion = vm.Direccion;
        destino.CategoriaId = vm.CategoriaId;
        destino.ProvinciaId = vm.ProvinciaId;
        destino.ModificadoPor = User.Identity?.Name;
        destino.FechaModificacion = DateTime.Now;


        var actuales = destino.Caracteristicas.ToList();
        foreach (var dc in actuales.Where(a => !vm.CaracteristicasSeleccionadas.Contains(a.CaracteristicaAccesibilidadId)))
            _context.DestinoCaracteristicas.Remove(dc);
        foreach (var cid in vm.CaracteristicasSeleccionadas.Where(i => !actuales.Any(a => a.CaracteristicaAccesibilidadId == i)))
            destino.Caracteristicas.Add(new DestinoCaracteristica { CaracteristicaAccesibilidadId = cid });

        
        if (vm.Imagen != null)
        {
            EliminarImagen(destino.ImagenUrl);
            destino.ImagenUrl = await GuardarImagen(vm.Imagen);
        }

        await _context.SaveChangesAsync();

        TempData["Exito"] = "Destino actualizado correctamente.";
        return RedirectToAction(nameof(Admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id)
    {
        var destino = await _context.Destinos.FindAsync(id);
        if (destino != null)
        {
            EliminarImagen(destino.ImagenUrl);
            _context.Destinos.Remove(destino);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Destino eliminado correctamente.";
        }
        return RedirectToAction(nameof(Admin));
    }


    private async Task CargarListas(DestinoFormViewModel vm)
    {
        vm.Categorias = await _context.Categorias.OrderBy(c => c.Nombre)
            .Select(c => new SelectListItem(c.Nombre, c.Id.ToString())).ToListAsync();
        vm.Provincias = await _context.Provincias.OrderBy(p => p.Nombre)
            .Select(p => new SelectListItem(p.Nombre, p.Id.ToString())).ToListAsync();
        vm.TodasCaracteristicas = await _context.Caracteristicas.OrderBy(c => c.Nombre).ToListAsync();
    }

    private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

    private static bool ImagenValida(IFormFile archivo, out string error)
    {
        error = "";
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(ext))
        {
            error = "Solo se permiten imágenes JPG, PNG o WEBP.";
            return false;
        }
        if (archivo.Length > 2 * 1024 * 1024)
        {
            error = "La imagen no puede superar los 2 MB.";
            return false;
        }
        return true;
    }

    private async Task<string> GuardarImagen(IFormFile archivo)
    {
        var carpeta = Path.Combine(_env.WebRootPath, "images", "destinos");
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid() + Path.GetExtension(archivo.FileName).ToLowerInvariant();
        using var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create);
        await archivo.CopyToAsync(stream);
        return "/images/destinos/" + nombre;
    }

    // Solo borra imágenes subidas desde el sistema (no las del seed)
    private void EliminarImagen(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith("/images/destinos/")) return;
        var ruta = Path.Combine(_env.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
    }
}