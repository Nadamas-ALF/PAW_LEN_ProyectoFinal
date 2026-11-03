
using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class CrearDireccionDto
{
    [Required]
    public int IdUsuario { get; set; }

    [Required]
    public int IdDistrito { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreDestinatario { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string DireccionExacta { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Referencia { get; set; }

    [Required]
    [StringLength(20)]
    public string TelefonoContacto { get; set; } = string.Empty;

    public bool EsPrincipal { get; set; }
}
