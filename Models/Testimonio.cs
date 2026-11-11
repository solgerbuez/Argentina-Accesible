using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Testimonio
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres")]
    public string Nombre { get; set; } = "";

    [StringLength(80, ErrorMessage = "La localidad no puede superar los {1} caracteres")]
    public string? Localidad { get; set; }

    [Required(ErrorMessage = "El testimonio es obligatorio")]
    [StringLength(600, ErrorMessage = "El testimonio no puede superar los {1} caracteres")]
    public string Texto { get; set; } = "";

    // true = lo envió un usuario y todavía no lo aprobó el administrador (no se muestra al público)
    public bool Pendiente { get; set; }

    // Auditoría
    public string? CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? ModificadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
}