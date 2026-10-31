using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

// HU-USR-005. Sin Contrasena, el API genera una al azar. Con ella, el administrador la escribio
// y se guarda igual como temporal: nunca queda como definitiva (DEC-004)
public class ContrasenaTemporalDto
{
    [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string? Contrasena { get; set; }

    [Compare(nameof(Contrasena), ErrorMessage = "Las dos contraseñas no coinciden.")]
    public string? ConfirmarContrasena { get; set; }
}
