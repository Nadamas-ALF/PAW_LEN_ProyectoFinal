using LEN_API.Datos;
using LEN_API.Datos.Entidades;
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ContextoLen _contexto;

    public UsuarioRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<int> Registrar(RegistroDto datos, string contrasenaHash)
    {
        // la interpolacion de SqlQuery se convierte en parametros, no en texto concatenado
        var filas = await _contexto.Database.SqlQuery<RegistroRespuestaDto>($@"
            EXEC dbo.SP_Usuario_Registrar
                @Nombre = {datos.Nombre},
                @Apellido = {datos.Apellido},
                @Cedula = {datos.Cedula},
                @Telefono = {datos.Telefono},
                @Correo = {datos.Correo},
                @ContrasenaHash = {contrasenaHash}").ToListAsync();

        return filas.Single().IdUsuario;
    }

    public async Task<TB_USUARIO?> ObtenerPorCorreo(string correo)
    {
        return await _contexto.TB_USUARIO
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.Correo == correo);
    }
}
