using System.ComponentModel.DataAnnotations;

namespace ArgentinaAccesible.Models;

public class RegistroAuditoria
{
    public int Id { get; set; }

    [Required, StringLength(256)]
    public string Usuario { get; set; } = "";

    // Creó, Modificó o Eliminó
    [Required, StringLength(20)]
    public string Accion { get; set; } = "";

    // Destino, Guía, Provincia, etc.
    [Required, StringLength(50)]
    public string Entidad { get; set; } = "";

    // Nombre o título del registro afectado
    [StringLength(300)]
    public string? Detalle { get; set; }

    public DateTime Fecha { get; set; }
}