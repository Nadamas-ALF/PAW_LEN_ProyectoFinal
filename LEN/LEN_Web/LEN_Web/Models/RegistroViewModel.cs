using System.ComponentModel.DataAnnotations;

namespace LEN_Web.Models
{
    public class RegistroViewModel
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
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare(nameof(Contrasena), ErrorMessage = "Las dos contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe aceptar los términos para continuar.")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos para continuar.")]
        public bool Terminos { get; set; }
    }
}
