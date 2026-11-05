using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Provincia
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80)]
    public string Nombre { get; set; } = "";
}