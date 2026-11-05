using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.ViewModels;

public class ChatRequest
{
    [Required, MinLength(1), MaxLength(10)]
    public List<ChatMensaje> Mensajes { get; set; } = new();
}

public class ChatMensaje
{
    [Required]
    public string Rol { get; set; } = "";

    [Required, StringLength(4000)]
    public string Texto { get; set; } = "";
}