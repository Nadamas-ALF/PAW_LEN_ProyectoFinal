using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

public class RecuperacionDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El correo no puede pasar de 150 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}
