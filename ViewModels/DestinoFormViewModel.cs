using System.ComponentModel.DataAnnotations;
using ArgentinaAccesible.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArgentinaAccesible.ViewModels;

public class DestinoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(120, ErrorMessage = "Máximo 120 caracteres")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = "";

    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

   

    [Display(Name = "Categoría")]
    [Range(1, int.MaxValue, ErrorMessage = "Elegí una categoría")]
    public int CategoriaId { get; set; }

    [Display(Name = "Provincia")]
    [Range(1, int.MaxValue, ErrorMessage = "Elegí una provincia")]
    public int ProvinciaId { get; set; }

    
    public string? ImagenUrl { get; set; }

    [Display(Name = "Imagen")]
    public IFormFile? Imagen { get; set; }

  
    public List<int> CaracteristicasSeleccionadas { get; set; } = new();

    // Datos para los combos y checkboxes (no vienen del formulario)
    [ValidateNever] public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
    [ValidateNever] public IEnumerable<SelectListItem> Provincias { get; set; } = new List<SelectListItem>();
    [ValidateNever] public List<CaracteristicaAccesibilidad> TodasCaracteristicas { get; set; } = new();
}