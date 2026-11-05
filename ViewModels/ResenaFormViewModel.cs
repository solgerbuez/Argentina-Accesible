using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.ViewModels
{
    public class ResenaFormViewModel
    {
        public int DestinoId { get; set; }

        [Range(1, 5, ErrorMessage = "El puntaje debe estar entre 1 y 5.")]
        public int Puntaje { get; set; }

        [Required(ErrorMessage = "El comentario es obligatorio.")]
        [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres.")]
        public string Comentario { get; set; } = "";
    }
}