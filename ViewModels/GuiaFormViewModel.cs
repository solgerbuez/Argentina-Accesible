using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.ViewModels
{
    public class GuiaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(150, ErrorMessage = "El título no puede superar los 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        public string? ImagenActual { get; set; }
        public string? ArchivoPdfActual { get; set; }

        [Display(Name = "Imagen de portada")]
        public IFormFile? Imagen { get; set; }

        [Display(Name = "Archivo PDF")]
        public IFormFile? ArchivoPdf { get; set; }
    }
}