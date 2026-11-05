using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArgentinaAccesible.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Provincia> Provincias { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<CaracteristicaAccesibilidad> Caracteristicas { get; set; }
    public DbSet<Destino> Destinos { get; set; }
    public DbSet<DestinoCaracteristica> DestinoCaracteristicas { get; set; }
    public DbSet<Resena> Resenas { get; set; }
    public DbSet<Favorito> Favoritos { get; set; }
    public DbSet<Guia> Guias { get; set; }
    public DbSet<Testimonio> Testimonios { get; set; }

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
}