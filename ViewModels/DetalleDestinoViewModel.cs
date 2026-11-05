using ArgentinaAccesible.Models;

namespace ArgentinaAccesible.ViewModels
{
    public class DetalleDestinoViewModel
    {
        public Destino Destino { get; set; } = null!;
        public List<Resena> Resenas { get; set; } = new();
        public double? Promedio { get; set; }
        public Resena? MiResena { get; set; }
    }
}