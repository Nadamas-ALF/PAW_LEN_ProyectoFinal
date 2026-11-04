
using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class ProductoRegistrarDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    [Range(typeof(decimal), "0", "99999999.99",
        ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal Precio { get; set; }

    [Range(0, int.MaxValue,
        ErrorMessage = "El stock debe ser mayor o igual a cero.")]
    public int Stock { get; set; }

    public bool EsPiezaUnica { get; set; }

    public bool Destacado { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Debe seleccionar una categoría válida.")]
    public int IdCategoria { get; set; }
}
