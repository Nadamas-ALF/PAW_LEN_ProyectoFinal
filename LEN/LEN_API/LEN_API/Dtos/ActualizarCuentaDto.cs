using System.ComponentModel.DataAnnotations;

namespace LEN_API.Dtos;

// BT-02. Lo que el cliente puede cambiar de si mismo. A proposito no trae
// IdRol ni Activo, por eso no se reusa ActualizarUsuarioAdminDto (que exige
// IdRol). El servicio los toma de la base, asi un cliente no se puede subir
// a Administrador mandando el campo en el cuerpo
public class ActualizarCuentaDto
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
}
