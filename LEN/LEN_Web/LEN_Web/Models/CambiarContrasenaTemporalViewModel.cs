using System.ComponentModel.DataAnnotations;

namespace LEN_Web.Models
{
    public class CambiarContrasenaTemporalViewModel
    {
        [Required(ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare(nameof(Contrasena), ErrorMessage = "Las dos contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
