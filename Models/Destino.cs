using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Destino
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(120)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "La descripción es obligatoria")]
    public string Descripcion { get; set; } = "";

    public string? Direccion { get; set; }
    public double Latitud { get; set; }
    public double Longitud { get; set; }
    public string? ImagenUrl { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int ProvinciaId { get; set; }
    public Provincia? Provincia { get; set; }

    // Auditoría
    public string? CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public ICollection<DestinoCaracteristica> Caracteristicas { get; set; } = new List<DestinoCaracteristica>();
    public ICollection<Resena> Resenas { get; set; } = new List<Resena>();
}