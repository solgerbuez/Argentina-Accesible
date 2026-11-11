using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los {1} caracteres")]
    public string Nombre { get; set; } = "";
}