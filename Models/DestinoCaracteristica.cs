namespace ArgentinaAccesible.Models;

public class DestinoCaracteristica
{
    public int DestinoId { get; set; }
    public Destino? Destino { get; set; }

    public int CaracteristicaAccesibilidadId { get; set; }
    public CaracteristicaAccesibilidad? Caracteristica { get; set; }
}