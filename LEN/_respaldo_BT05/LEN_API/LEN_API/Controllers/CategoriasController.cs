
using LEN_API.Dtos;
using LEN_API.Infraestructura;
using LEN_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LEN_API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaServicio _categoriaServicio;

    public CategoriasController(ICategoriaServicio categoriaServicio)
    {
        _categoriaServicio = categoriaServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _categoriaServicio.Listar();

        return Ok(categorias);
    }


    [HttpPost]
    public async Task<IActionResult> Registrar(
        [FromBody] CategoriaRegistrarDto datos)
    {
        int idAdministrador = User.IdUsuario();

        int idCategoria = await _categoriaServicio.Registrar(
            idAdministrador,
            datos);

        return StatusCode(201, new
        {
            mensaje = "Categoría registrada correctamente.",
            idCategoria
        });
    }


    [HttpPut("{idCategoria:int}")]
    public async Task<IActionResult> Actualizar(
        int idCategoria,
        [FromBody] CategoriaActualizarDto datos)
    {
        int idAdministrador = User.IdUsuario();

        await _categoriaServicio.Actualizar(
            idAdministrador,
            idCategoria,
            datos);

        return Ok(new
        {
            mensaje = "Categoría actualizada correctamente."
        });
    }



    [HttpPatch("{idCategoria:int}/estado")]
    public async Task<IActionResult> CambiarActivo(int idCategoria)
    {
        int idAdministrador = User.IdUsuario();

        await _categoriaServicio.CambiarActivo(
            idAdministrador,
            idCategoria);

        return Ok(new
        {
            mensaje = "Estado de la categoría actualizado correctamente."
        });
    }



}
