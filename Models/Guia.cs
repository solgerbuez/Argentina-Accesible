using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Guia
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(120)]
    public string Titulo { get; set; } = "";

    public string? ImagenUrl { get; set; }
    public string? ArchivoPdf { get; set; }

    // Auditoría
    public string? CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}