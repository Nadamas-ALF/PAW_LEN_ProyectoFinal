using System.Security.Claims;

namespace LEN_API.Infraestructura;

public static class UsuarioActualExtensiones
{
    // El id sale del token que el propio API firmo, nunca del cuerpo de la peticion
    public static int IdUsuario(this ClaimsPrincipal usuario)
    {
        return int.Parse(usuario.FindFirst("IdUsuario")!.Value);
    }
}
