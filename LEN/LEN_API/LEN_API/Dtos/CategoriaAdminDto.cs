
namespace LEN_API.Dtos;

public class CategoriaAdminDto
{
    public int IdCategoria { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public int CantidadProductos { get; set; }

    public int CantidadProductosActivos { get; set; }
}
