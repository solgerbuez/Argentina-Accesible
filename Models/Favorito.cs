using Microsoft.AspNetCore.Identity;

namespace ArgentinaAccesible.Models;

public class Favorito
{
    public int Id { get; set; }

    public int DestinoId { get; set; }
    public Destino? Destino { get; set; }

    public string UsuarioId { get; set; } = "";
    public IdentityUser? Usuario { get; set; }

    public DateTime Fecha { get; set; }
}