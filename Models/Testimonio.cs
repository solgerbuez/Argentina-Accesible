using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Testimonio
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80)]
    public string Nombre { get; set; } = "";

    [StringLength(80)]
    public string? Localidad { get; set; }

    [Required(ErrorMessage = "El testimonio es obligatorio")]
    [StringLength(600)]
    public string Texto { get; set; } = "";

    // Auditoría
    public string? CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}