using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

// Sirve a HU-CLI-003 y a HU-ACC-006. ContrasenaActual se exige solo si la cuenta no tiene
// una temporal pendiente: en el cambio obligatorio la actual es la temporal con la que ya entro
public class CambiarContrasenaDto
{
    [MaxLength(255, ErrorMessage = "La contraseña no puede pasar de 255 caracteres.")]
    public string? ContrasenaActual { get; set; }

    [Required(ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    [StringLength(255, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [Compare(nameof(Contrasena), ErrorMessage = "Las dos contraseñas no coinciden.")]
    public string ConfirmarContrasena { get; set; } = string.Empty;
}
