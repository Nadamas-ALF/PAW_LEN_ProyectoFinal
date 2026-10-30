using LEN_Web.Models;

namespace LEN_Web.Utilidades
{
    // ====================================================================
    // DATOS DE EJEMPLO. Este archivo se borra cuando el API responda.
    //
    // Todo lo que las pantallas muestran mientras no hay endpoints vive aqui
    // y solo aqui. La idea es que al conectar el API se cambien las llamadas
    // y se borre este archivo, sin andar cazando datos sueltos en cada
    // controlador.
    //
    // Los datos tienen que cuadrar entre pantallas: el pedido FAC-000012 de
    // Ana Mora Jimenez vale 25 000 en el historial, en el detalle, en la
    // factura, en los pedidos del panel y en los pagos. Si se cambia un monto
    // hay que cambiarlo en todas.
    // ====================================================================
    public static class DatosEjemplo
    {
        public static PortadaViewModel Portada(IWebHostEnvironment entorno) => new()
        {
            Destacados = new List<ProductoDestacado>
            {
                new() { IdProducto = 1, NombreCategoria = "Collares", NombreProducto = "Collar Bello", Precio = 8500m, RutaImagen = Imagen(entorno, "destacado-collar-bello.jpeg") },
                new() { IdProducto = 3, NombreCategoria = "Pulseras", NombreProducto = "Pulsera de Perlas", Precio = 5500m, RutaImagen = Imagen(entorno, "destacado-pulsera-perlas.jpeg") },
                new() { IdProducto = 5, NombreCategoria = "Aretes", NombreProducto = "Aretes Luna", Precio = 3800m, RutaImagen = Imagen(entorno, "destacado-aretes-luna.jpeg") },
            },
            Productos = new List<ProductoResumen>
            {
                new() { IdProducto = 1, NombreCategoria = "Collares", NombreProducto = "Collar Bello", Precio = 8500m, Disponible = true, RutaImagen = Imagen(entorno, "producto-collar-bello.jpeg") },
                new() { IdProducto = 2, NombreCategoria = "Collares", NombreProducto = "Collar Pato", Precio = 7000m, Disponible = true, RutaImagen = Imagen(entorno, "producto-collar-pato.jpeg") },
                new() { IdProducto = 3, NombreCategoria = "Pulseras", NombreProducto = "Pulsera de Perlas", Precio = 5500m, Disponible = true, RutaImagen = Imagen(entorno, "producto-pulsera-perlas.jpeg") },
                new() { IdProducto = 4, NombreCategoria = "Llaveros", NombreProducto = "Llavero Flor", Precio = 2500m, Disponible = false, RutaImagen = Imagen(entorno, "producto-llavero-flor.jpeg") },
            },
        };

        // Las categorias del menu de la cabecera, mientras no haya catalogo
        public static (int IdCategoria, string Nombre)[] CategoriasMenu =>
        [
            (1, "Collares"),
            (2, "Pulseras"),
            (3, "Llaveros"),
            (4, "Aretes"),
        ];

        // Devuelve el nombre del archivo solo si de verdad esta en wwwroot/imagenes.
        // Si falta, la vista cae al LenMarcador en vez de dejar una imagen rota
        public static string? Imagen(IWebHostEnvironment entorno, string archivo)
        {
            var ruta = Path.Combine(entorno.WebRootPath, "imagenes", archivo);
            return File.Exists(ruta) ? archivo : null;
        }
    }
}
