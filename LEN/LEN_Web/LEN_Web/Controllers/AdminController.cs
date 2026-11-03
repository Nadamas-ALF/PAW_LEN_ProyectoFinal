using Microsoft.AspNetCore.Authorization;
using LEN_Web.Utilidades;
using Microsoft.AspNetCore.Mvc;
using LEN_Web.Models;


namespace LEN_Web.Controllers
{
    // Pantallas del panel. Solo del rol Administrador
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IWebHostEnvironment _entorno;
        private readonly IClienteApi _api;

        public AdminController(IWebHostEnvironment entorno, IClienteApi api)
        {
            _entorno = entorno;
            _api = api;
        }

        public IActionResult Tablero()
        {
            return View(DatosEjemplo.Tablero());
        }

        public IActionResult Productos(string? busqueda, int? idCategoria, int pagina = 1)
        {
            return View(DatosEjemplo.ProductosAdmin(_entorno, busqueda, idCategoria, pagina));
        }

        public IActionResult ProductoCrear()
        {
            return View(DatosEjemplo.FormularioNuevo());
        }

        public IActionResult ProductoEditar(int idProducto)
        {
            var formulario = DatosEjemplo.FormularioDe(idProducto);

            if (formulario is null)
            {
                TempData["MensajeError"] = "El producto indicado no existe.";
                return RedirectToAction(nameof(Productos));
            }

            return View(formulario);
        }

        public IActionResult ProductoImagenes(int idProducto)
        {
            var imagenes = DatosEjemplo.ImagenesDe(_entorno, idProducto);

            if (imagenes is null)
            {
                TempData["MensajeError"] = "El producto indicado no existe.";
                return RedirectToAction(nameof(Productos));
            }

            return View(imagenes);
        }

        public IActionResult Categorias()
        {
            return View(DatosEjemplo.CategoriasAdmin());
        }

        public IActionResult Pedidos(
            string? busqueda,
            int? idEstado,
            string? fechaInicio,
            string? fechaFin,
            int pagina = 1)
        {
            return View(DatosEjemplo.PedidosAdmin(busqueda, idEstado, fechaInicio, fechaFin, pagina));
        }

        // Aqui no se filtra por usuario: el administrador ve cualquier pedido
        public IActionResult PedidoDetalle(int idPedido)
        {
            var pedido = DatosEjemplo.DetallePedidoAdmin(_entorno, idPedido);

            if (pedido is null)
            {
                TempData["MensajeError"] = "No fue posible abrir el pedido solicitado.";
                return RedirectToAction(nameof(Pedidos));
            }

            return View(pedido);
        }

        public IActionResult Pagos(
            string? busqueda,
            int? idEstadoPago,
            string? fechaInicio,
            string? fechaFin,
            int pagina = 1)
        {
            return View(DatosEjemplo.Pagos(busqueda, idEstadoPago, fechaInicio, fechaFin, pagina));
        }

        public IActionResult Facturas(string? busqueda, string? fechaInicio, string? fechaFin, int pagina = 1)
        {
            return View(DatosEjemplo.Facturas(busqueda, fechaInicio, fechaFin, pagina));
        }

        public async Task<IActionResult> Usuarios(
            string? busqueda,
            int? idRol,
            bool? activo,
            int pagina = 1)
        {
            // Consultar los usuarios reales del API
            var resultado = await _api.Obtener<List<UsuarioAdmin>>("usuarios");

            if (resultado.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultado);

            if (!resultado.EsExito || resultado.Datos is null)
                return this.PaginaError(resultado);

            var usuarios = resultado.Datos;

            // Aplicar el filtro de busqueda
            var encontrados = usuarios.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                encontrados = encontrados.Where(usuario =>
                    usuario.NombreCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || usuario.Correo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || (usuario.Cedula ?? string.Empty).Contains(texto, StringComparison.OrdinalIgnoreCase));
            }

            // Filtrar por rol
            if (idRol.HasValue)
            {
                encontrados = encontrados.Where(usuario =>
                    usuario.IdRol == idRol.Value);
            }

            // Filtrar por estado
            if (activo.HasValue)
            {
                encontrados = encontrados.Where(usuario =>
                    usuario.Activo == activo.Value);
            }

            var lista = encontrados.ToList();

            // Calcular la paginacion
            int totalPaginas = Math.Max(
                1,
                (int)Math.Ceiling(lista.Count / (double)DatosEjemplo.UsuariosPorPagina));

            int paginaSegura = Math.Clamp(pagina, 1, totalPaginas);

            // Preparar el modelo para la vista
            var modelo = new ListadoUsuariosViewModel
            {
                Usuarios = lista
                    .Skip((paginaSegura - 1) * DatosEjemplo.UsuariosPorPagina)
                    .Take(DatosEjemplo.UsuariosPorPagina)
                    .ToList(),

                Roles = DatosEjemplo.Roles(),

                Busqueda = busqueda,
                IdRol = idRol,
                Activo = activo,

                PaginaActual = paginaSegura,
                TotalPaginas = totalPaginas,

                // Los indicadores se calculan usando todos los usuarios
                TotalUsuarios = usuarios.Count,
                TotalActivos = usuarios.Count(u => u.Activo),
                TotalInactivos = usuarios.Count(u => !u.Activo),
                TotalAdministradores = usuarios.Count(u => u.EsAdministrador)
            };

            return View(modelo);
        }

        // Es tambien el Perfil del menu de la barra de arriba, con el Id del
        // usuario de la sesion

        public async Task<IActionResult> UsuarioDetalle(int idUsuario)
        {
            // Consultar todos los usuarios para encontrar el solicitado
            var resultadoUsuarios = await _api.Obtener<List<UsuarioAdmin>>("usuarios");

            if (resultadoUsuarios.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultadoUsuarios);

            if (!resultadoUsuarios.EsExito || resultadoUsuarios.Datos is null)
                return this.PaginaError(resultadoUsuarios);

            var usuarios = resultadoUsuarios.Datos;

            var usuario = usuarios.FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario is null)
            {
                TempData["MensajeError"] = "El usuario indicado no existe.";
                return RedirectToAction(nameof(Usuarios));
            }

            // Consultar las direcciones activas e inactivas
            var resultadoDirecciones = await _api.Obtener<List<DireccionAdminApi>>(
                $"direcciones/usuarios/{idUsuario}?incluirInactivas=true");

            if (resultadoDirecciones.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultadoDirecciones);

            if (!resultadoDirecciones.EsExito || resultadoDirecciones.Datos is null)
                return this.PaginaError(resultadoDirecciones);

            // Consultar las provincias para los selectores
            var resultadoProvincias = await _api.Obtener<List<ProvinciaAdminApi>>(
                "direcciones/provincias");

            if (resultadoProvincias.Tipo == TipoResultadoApi.SinPermiso)
                return await this.SinPermiso(resultadoProvincias);

            if (!resultadoProvincias.EsExito || resultadoProvincias.Datos is null)
                return this.PaginaError(resultadoProvincias);

            // Preparar el modelo que utiliza la vista
            var modelo = new DetalleUsuarioViewModel
            {
                Usuario = usuario,
                Roles = DatosEjemplo.Roles(),

                Direcciones = resultadoDirecciones.Datos.Select(d => new DireccionEnvio
                {
                    IdDireccion = d.IdDireccion,
                    NombreDestinatario = d.NombreDestinatario,
                    DireccionExacta = d.DireccionExacta,
                    Distrito = d.NombreDistrito,
                    Canton = d.NombreCanton,
                    Provincia = d.NombreProvincia,
                    TelefonoContacto = d.TelefonoContacto,
                    Referencia = d.Referencia,
                    EsPrincipal = d.EsPrincipal,
                    Activo = d.Activo,
                    IdProvincia = d.IdProvincia,
                    IdCanton = d.IdCanton,
                    IdDistrito = d.IdDistrito
                }).ToList(),

                Provincias = resultadoProvincias.Datos.Select(p => new OpcionUbicacion
                {
                    Id = p.IdProvincia,
                    Nombre = p.Nombre
                }).ToList(),

                EsUnicoAdministradorActivo =
                    usuario.EsAdministrador &&
                    usuario.Activo &&
                    usuarios.Count(u => u.EsAdministrador && u.Activo) == 1
            };

            return View(modelo);
        }


        // Las dos fechas son obligatorias. Sin ellas se propone un rango en
        // lugar de responder con un error: la pantalla tiene que abrir
        public IActionResult Reportes(string? fechaInicio, string? fechaFin)
        {
            return View(DatosEjemplo.Reporte(fechaInicio, fechaFin));
        }

        public IActionResult FacturaDetalle(int idFactura)
        {
            var factura = DatosEjemplo.DetalleFactura(_entorno, idFactura);

            if (factura is null)
            {
                TempData["MensajeError"] = "No fue posible abrir la factura solicitada.";
                return RedirectToAction(nameof(Facturas));
            }

            return View(factura);
        }


        private class ProvinciaAdminApi
        {
            public int IdProvincia { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        private class DireccionAdminApi
        {
            public int IdDireccion { get; set; }
            public int IdUsuario { get; set; }
            public int IdProvincia { get; set; }
            public int IdCanton { get; set; }
            public int IdDistrito { get; set; }

            public string NombreProvincia { get; set; } = string.Empty;
            public string NombreCanton { get; set; } = string.Empty;
            public string NombreDistrito { get; set; } = string.Empty;

            public string NombreDestinatario { get; set; } = string.Empty;
            public string DireccionExacta { get; set; } = string.Empty;
            public string? Referencia { get; set; }
            public string? TelefonoContacto { get; set; }

            public bool EsPrincipal { get; set; }
            public bool Activo { get; set; }
        }

    }
}
