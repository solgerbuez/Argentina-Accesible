using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly IHttpContextAccessor _http;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor http)
        : base(options)
    {
        _http = http;
    }

    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<CaracteristicaAccesibilidad> Caracteristicas { get; set; }
    public DbSet<Destino> Destinos { get; set; }
    public DbSet<DestinoCaracteristica> DestinoCaracteristicas { get; set; }
    public DbSet<Resena> Resenas { get; set; }
    public DbSet<Favorito> Favoritos { get; set; }
    public DbSet<Guia> Guias { get; set; }
    public DbSet<Testimonio> Testimonios { get; set; }
    public DbSet<RegistroAuditoria> RegistrosAuditoria { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<DestinoCaracteristica>()
            .HasKey(dc => new { dc.DestinoId, dc.CaracteristicaAccesibilidadId });

        // Un usuario no puede tener el mismo destino dos veces en favoritos
        builder.Entity<Favorito>()
            .HasIndex(f => new { f.UsuarioId, f.DestinoId }).IsUnique();

        // Borrados protegidos: no se puede eliminar una provincia, categoría
        // o característica que está en uso por algún destino
        var protegidas = new[] { typeof(Provincia), typeof(Categoria), typeof(CaracteristicaAccesibilidad) };
        foreach (var fk in builder.Model.GetEntityTypes()
                     .SelectMany(e => e.GetForeignKeys())
                     .Where(fk => protegidas.Contains(fk.PrincipalEntityType.ClrType)))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    // ---------- Auditoría automática ----------
    // Cada vez que se guardan cambios, se anota qué usuario creó, modificó o eliminó qué.

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RegistrarAuditoria();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RegistrarAuditoria();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RegistrarAuditoria()
    {
        // Sin usuario con sesión (por ejemplo, el seeder al arrancar) no se audita
        var identidad = _http.HttpContext?.User?.Identity;
        if (identidad == null || !identidad.IsAuthenticated) return;

        var usuario = identidad.Name ?? "desconocido";
        var ahora = DateTime.Now;
        var registros = new List<RegistroAuditoria>();

        foreach (var entrada in ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added
                              || e.State == EntityState.Modified
                              || e.State == EntityState.Deleted))
        {
            var descripcion = Describir(entrada.Entity);
            if (descripcion == null) continue; // solo se auditan estas entidades

            registros.Add(new RegistroAuditoria
            {
                Usuario = usuario,
                Accion = entrada.State == EntityState.Added ? "Creó"
                       : entrada.State == EntityState.Modified ? "Modificó"
                       : "Eliminó",
                Entidad = descripcion.Value.Entidad,
                Detalle = descripcion.Value.Detalle,
                Fecha = ahora
            });
        }

        if (registros.Count > 0)
            RegistrosAuditoria.AddRange(registros);
    }

    private static (string Entidad, string Detalle)? Describir(object entidad)
    {
        return entidad switch
        {
            Destino d => ("Destino", d.Nombre),
            Guia g => ("Guía", g.Titulo),
            Provincia p => ("Provincia", p.Nombre),
            Categoria cat => ("Categoría", cat.Nombre),
            CaracteristicaAccesibilidad car => ("Característica", car.Nombre),
            Testimonio t => ("Testimonio", t.Nombre),
            Resena r => ("Reseña", $"Puntaje {r.Puntaje} (destino n.º {r.DestinoId})"),
            _ => null
        };
    }
}