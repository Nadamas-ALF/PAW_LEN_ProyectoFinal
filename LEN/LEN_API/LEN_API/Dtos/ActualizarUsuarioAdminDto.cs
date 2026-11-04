using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class ActualizarUsuarioAdminDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Cedula { get; set; }

    [MaxLength(20)]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un rol válido.")]
    public int IdRol { get; set; }

    public bool Activo { get; set; }
}