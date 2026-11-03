
using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class CrearUsuarioAdminDto
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

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(255, MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un rol válido.")]
    public int IdRol { get; set; }

    public bool Activo { get; set; } = true;
}
