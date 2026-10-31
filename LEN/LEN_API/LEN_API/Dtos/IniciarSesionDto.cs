using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class IniciarSesionDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MaxLength(255, ErrorMessage = "La contraseña no puede pasar de 255 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;
}
