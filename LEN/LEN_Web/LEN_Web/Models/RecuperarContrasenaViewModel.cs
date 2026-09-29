using System.ComponentModel.DataAnnotations;

namespace LEN_Web.Models
{
    public class RecuperarContrasenaViewModel
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        public string Correo { get; set; } = string.Empty;
    }
}
