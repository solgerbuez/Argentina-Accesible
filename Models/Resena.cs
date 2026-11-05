using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ArgentinaAccesible.Models;

public class Resena
{
    public int Id { get; set; }

    public int DestinoId { get; set; }
    public Destino? Destino { get; set; }

    public string UsuarioId { get; set; } = "";
    public IdentityUser? Usuario { get; set; }

    [Range(1, 5, ErrorMessage = "El puntaje debe estar entre 1 y 5")]
    public int Puntaje { get; set; }

    [Required(ErrorMessage = "El comentario es obligatorio")]
    [StringLength(500)]
    public string Comentario { get; set; } = "";

    public DateTime Fecha { get; set; }
}