using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class ProductoActualizarStockDto
{
    [Range(0, int.MaxValue,
        ErrorMessage = "El stock debe ser mayor o igual a cero.")]
    public int Stock { get; set; }
}