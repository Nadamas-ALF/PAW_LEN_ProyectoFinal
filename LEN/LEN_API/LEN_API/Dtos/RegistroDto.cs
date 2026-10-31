using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

// Los mismos limites que RegistroViewModel del Web y que el CHECK de la base.
// Terminos no viaja: lo valida el Web (ver DEC-005)
public class RegistroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El nombre no puede pasar de 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(100, ErrorMessage = "El apellido no puede pasar de 100 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "La cédula no puede pasar de 20 caracteres.")]
    public string? Cedula { get; set; }

    [MaxLength(20, ErrorMessage = "El teléfono no puede pasar de 20 caracteres.")]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [MaxLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las dos contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
