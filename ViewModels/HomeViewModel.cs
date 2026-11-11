using ArgentinaAccesible.Models;

namespace ArgentinaAccesible.ViewModels;

public class HomeViewModel
{
    public List<Destino> Destacados { get; set; } = new();
    public List<Testimonio> Testimonios { get; set; } = new();
}