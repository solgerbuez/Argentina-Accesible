using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        await context.Database.MigrateAsync();

        // Roles
        foreach (var rol in new[] { "Administrador", "Usuario" })
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));

        // Usuarios de prueba (para la corrección)
        await CrearUsuario(userManager, "admin@argentinaaccesible.com", "Admin123!", "Administrador");
        await CrearUsuario(userManager, "usuario@argentinaaccesible.com", "Usuario123!", "Usuario");

        // Catálogos base: se agrega solo lo que falta (se compara por nombre)
        foreach (var nombre in new[] { "Buenos Aires", "Mendoza", "Río Negro", "La Pampa", "Misiones", "Córdoba" })
            if (!await context.Provincias.AnyAsync(p => p.Nombre == nombre))
                context.Provincias.Add(new Provincia { Nombre = nombre });

        foreach (var nombre in new[] { "Alojamiento", "Museo", "Restaurante", "Atractivo turístico" })
            if (!await context.Categorias.AnyAsync(c => c.Nombre == nombre))
                context.Categorias.Add(new Categoria { Nombre = nombre });

        var caracteristicas = new (string Nombre, string Icono)[]
        {
            ("Rampas de acceso", "fa-wheelchair"),
            ("Baño adaptado", "fa-restroom"),
            ("Audioguía", "fa-headphones"),
            ("Señalética braille", "fa-braille"),
            ("Intérprete de lengua de señas", "fa-sign-language")
        };
        foreach (var (nombre, icono) in caracteristicas)
            if (!await context.Caracteristicas.AnyAsync(c => c.Nombre == nombre))
                context.Caracteristicas.Add(new CaracteristicaAccesibilidad { Nombre = nombre, Icono = icono });

        await context.SaveChangesAsync();

        int Prov(string n) => context.Provincias.First(p => p.Nombre == n).Id;
        int Cat(string n) => context.Categorias.First(c => c.Nombre == n).Id;
        var ahora = DateTime.Now;

        // Destinos
        var destinos = new[]
        {
            new Destino { Nombre = "Obelisco", Descripcion = "Acceso con rampas y audioguías", Latitud = -34.6037, Longitud = -58.3816, ImagenUrl = "/images/obelisco.jpg", ProvinciaId = Prov("Buenos Aires"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora },
            new Destino { Nombre = "Cerro Campanario", Descripcion = "Teleférico accesible y senderos adaptados", Latitud = -41.1333, Longitud = -71.3, ImagenUrl = "/images/cerro-campanario.jpg", ProvinciaId = Prov("Río Negro"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora },
            new Destino { Nombre = "Bodega Catena Zapata", Descripcion = "Visitas guiadas inclusivas y accesibilidad en el recorrido", Latitud = -33.0234, Longitud = -68.8329, ImagenUrl = "/images/bodega-catena.jpg", ProvinciaId = Prov("Mendoza"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora },
            new Destino { Nombre = "Cataratas del Iguazú", Descripcion = "Pasarelas accesibles y transporte adaptado", Latitud = -25.6953, Longitud = -54.4366, ImagenUrl = "/images/guia-iguazu.jpg", ProvinciaId = Prov("Misiones"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora },
            new Destino { Nombre = "Mendoza Ciudad", Descripcion = "Centros cívicos y parques con accesibilidad certificada", Latitud = -32.8895, Longitud = -68.8458, ImagenUrl = "/images/guia-mendoza.jpg", ProvinciaId = Prov("Mendoza"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora },
            new Destino { Nombre = "La Pampa", Descripcion = "Senderos adaptados en parques y reservas naturales", Latitud = -36.6167, Longitud = -64.2833, ImagenUrl = "/images/guia-pampa.jpg", ProvinciaId = Prov("La Pampa"), CategoriaId = Cat("Atractivo turístico"), CreadoPor = "Sistema", FechaCreacion = ahora }
        };
        foreach (var d in destinos)
            if (!await context.Destinos.AnyAsync(x => x.Nombre == d.Nombre))
                context.Destinos.Add(d);

        // Guías
        var guias = new[]
        {
            new Guia { Titulo = "Buenos Aires Accesible", ImagenUrl = "/images/guia-buenos-aires.jpg", ArchivoPdf = "/guias/guia-buenos-aires.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "Bariloche Accesible", ImagenUrl = "/images/guia-bariloche.jpg", ArchivoPdf = "/guias/guia-bariloche.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "Mendoza Accesible", ImagenUrl = "/images/guia-mendoza.jpg", ArchivoPdf = "/guias/guia-mendoza.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "Iguazú Accesible", ImagenUrl = "/images/guia-iguazu.jpg", ArchivoPdf = "/guias/guia-iguazu.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "Córdoba Accesible", ImagenUrl = "/images/cordoba.jpg", ArchivoPdf = "/guias/guia-cordoba.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "La Pampa Accesible", ImagenUrl = "/images/la-pampa.jpg", ArchivoPdf = "/guias/guia-la-pampa.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "Derechos del turista", ImagenUrl = "/images/derecho-turismo.jpg", ArchivoPdf = "/guias/leyes-turismo.pdf", CreadoPor = "Sistema", FechaCreacion = ahora },
            new Guia { Titulo = "10 consejos de viaje", ImagenUrl = "/images/consejos-viajes.jpg", ArchivoPdf = "/guias/10-consejos-viaje.pdf", CreadoPor = "Sistema", FechaCreacion = ahora }
        };
        foreach (var g in guias)
            if (!await context.Guias.AnyAsync(x => x.Titulo == g.Titulo))
                context.Guias.Add(g);

        // EJEMPLOS para la demo: reemplazalos por testimonios reales (con permiso de cada persona) desde el ABM
        if (!context.Testimonios.Any())
        {
            context.Testimonios.AddRange(
                new Testimonio { Nombre = "Laura M.", Localidad = "Córdoba", Texto = "Antes de viajar pude confirmar que el hotel tenía rampa y baño adaptado. Por primera vez no tuve que llamar a cinco lugares distintos.", CreadoPor = "Sistema", FechaCreacion = ahora },
                new Testimonio { Nombre = "Martín G.", Localidad = "Rosario", Texto = "Las reseñas de otras personas con necesidades parecidas a las mías me ahorraron sorpresas desagradables en el destino.", CreadoPor = "Sistema", FechaCreacion = ahora },
                new Testimonio { Nombre = "Carla y familia", Localidad = "Mendoza", Texto = "Armamos el itinerario completo con lugares accesibles para todos. Viajamos tranquilos y disfrutamos cada parada.", CreadoPor = "Sistema", FechaCreacion = ahora }
            );
        }

        await context.SaveChangesAsync();
    }

    private static async Task CrearUsuario(UserManager<IdentityUser> um, string email, string pass, string rol)
    {
        if (await um.FindByEmailAsync(email) != null) return;
        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await um.CreateAsync(user, pass);
        if (result.Succeeded) await um.AddToRoleAsync(user, rol);
    }
}