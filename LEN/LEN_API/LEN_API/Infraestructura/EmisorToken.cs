using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LEN_API.Datos.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LEN_API.Infraestructura;

public class EmisorToken : IEmisorToken
{
    private readonly OpcionesJwt _opciones;

    public EmisorToken(IOptions<OpcionesJwt> opciones)
    {
        _opciones = opciones.Value;
    }

    public (string Token, DateTime Expira) Emitir(TB_USUARIO usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_opciones.Minutos);

        // nombres de claim del contrato, tal cual: el JwtBearer los lee con MapInboundClaims apagado
        var claims = new[]
        {
            new Claim("IdUsuario", usuario.IdUsuario.ToString()),
            new Claim("Correo", usuario.Correo),
            new Claim("IdRol", usuario.IdRol.ToString()),
            new Claim("NombreRol", usuario.IdRolNavigation.Nombre),
            new Claim("TieneContrasenaTemporal", usuario.TieneContrasenaTemporal ? "true" : "false")
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Llave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Emisor,
            audience: _opciones.Audiencia,
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
