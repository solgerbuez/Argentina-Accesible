using ArgentinaAccesible.Models;

namespace ArgentinaAccesible.ViewModels;

public class AuditoriaViewModel
{
    // Últimas operaciones (ya filtradas)
    public List<RegistroAuditoria> Registros { get; set; } = new();

    // Totales reales de la base (con el mismo filtro aplicado)
    public int Total { get; set; }
    public int Altas { get; set; }
    public int Modificaciones { get; set; }
    public int Bajas { get; set; }

    // Entidad elegida en el filtro (null = todas)
    public string? Entidad { get; set; }
}