namespace LEN_Web.Models
{
    // El catalogo visto desde el panel. A diferencia del publico, aca si se ven
    // los productos dados de baja: por eso el estado tiene tres valores y no dos

    public class CategoriaOpcion
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }

    public class ProductoAdmin
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public bool EsPiezaUnica { get; set; }

        public bool Destacado { get; set; }

        public bool Activo { get; set; }

        public string? RutaImagen { get; set; }

        // Inactivo sale de Activo en 0. Entre los activos, Disponible o Agotado
        // se calcula del stock: no es una columna de la base
        public string EstadoVisible => !Activo ? "Inactivo" : Stock > 0 ? "Disponible" : "Agotado";

        public string ClaseCssEstado => !Activo
            ? "LenBadge--Neutro"
            : Stock > 0 ? "LenBadge--Exito" : "LenBadge--Negativo";

        // Una pieza unica no admite mas de uno por CK_Producto_PiezaUnicaStock
        public int? MaximoStock => EsPiezaUnica ? 1 : null;
    }

    public class AlertaStock
    {
        public string NombreProducto { get; set; } = string.Empty;

        public int Stock { get; set; }
    }

    public class ListadoProductosViewModel
    {
        public List<ProductoAdmin> Productos { get; set; } = new();

        public List<CategoriaOpcion> Categorias { get; set; } = new();

        public string? Busqueda { get; set; }

        public int? IdCategoria { get; set; }

        public int PaginaActual { get; set; } = 1;

        public int TotalPaginas { get; set; } = 1;

        // SP_Producto_ListarBajoStock con umbral 3 por defecto
        public List<AlertaStock> AlertasBajoStock { get; set; } = new();
    }

    // Lo comparten ProductoCrear y ProductoEditar por la parcial
    // _ProductoFormulario. Lo que cambia entre las dos lo decide EsEdicion
    public class ProductoFormViewModel
    {
        // En cero al crear. Al editar va en un oculto
        public int IdProducto { get; set; }

        public string? NombreProducto { get; set; }

        public int? IdCategoria { get; set; }

        public string? Descripcion { get; set; }

        public decimal? Precio { get; set; }

        public int Stock { get; set; }

        public bool EsPiezaUnica { get; set; }

        public bool Destacado { get; set; }

        // Solo las activas: el procedimiento rechaza una categoria inactiva
        public List<CategoriaOpcion> Categorias { get; set; } = new();

        public bool EsEdicion { get; set; }

        // Al editar se muestra el estado pero no se cambia: eso es de la tabla de
        // productos, con SP_Producto_CambiarActivo
        public string EstadoVisible { get; set; } = string.Empty;

        public string ClaseCssEstado { get; set; } = string.Empty;

        public string Accion => EsEdicion ? "/Producto/Editar" : "/Producto/Crear";

        public string TextoBoton => EsEdicion ? "Guardar cambios" : "Guardar producto";
    }

    public class ImagenProducto
    {
        public int IdImagen { get; set; }

        public string? RutaImagen { get; set; }

        public string? TextoAlternativo { get; set; }

        // Solo una por producto, por el indice UX_ImagenProducto_PrincipalProducto
        public bool EsPrincipal { get; set; }
    }

    public class ImagenesProductoViewModel
    {
        public int IdProducto { get; set; }

        public string NombreProducto { get; set; } = string.Empty;

        // La principal viene primera
        public List<ImagenProducto> Imagenes { get; set; } = new();
    }

    public class CategoriaAdmin
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int CantidadProductos { get; set; }

        // Con productos activos no se puede desactivar. La columna lo avisa de
        // antemano para que el THROW 50000 no sea una sorpresa
        public int CantidadProductosActivos { get; set; }
    }
}
