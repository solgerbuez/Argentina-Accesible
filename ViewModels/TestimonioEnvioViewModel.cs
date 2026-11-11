using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.ViewModels;

public class TestimonioEnvioViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres")]
    [Display(Name = "Tu nombre")]
    public string Nombre { get; set; } = "";

    [StringLength(80, ErrorMessage = "La localidad no puede superar los {1} caracteres")]
    [Display(Name = "Tu localidad (opcional)")]
    public string? Localidad { get; set; }

    [Required(ErrorMessage = "Escribí tu testimonio")]
    [StringLength(600, MinimumLength = 20, ErrorMessage = "El testimonio debe tener entre {2} y {1} caracteres")]
    [Display(Name = "Tu testimonio")]
    public string Texto { get; set; } = "";
}