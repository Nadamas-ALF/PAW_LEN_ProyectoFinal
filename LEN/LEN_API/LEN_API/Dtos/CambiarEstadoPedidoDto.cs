using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

// BT-11. HU-ADM-PED-003 y 004. Cuerpo de PATCH api/pedidos/{id}/estado
public class CambiarEstadoPedidoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione el nuevo estado.")]
    public int IdEstadoNuevo { get; set; }

    // Obligatorio solo al cancelar: lo revisan el servicio y el procedimiento
    [MaxLength(255, ErrorMessage = "El motivo admite hasta 255 caracteres.")]
    public string? MotivoCancelacion { get; set; }
}
