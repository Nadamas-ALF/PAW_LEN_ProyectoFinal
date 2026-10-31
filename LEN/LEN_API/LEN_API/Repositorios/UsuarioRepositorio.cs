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

    public async Task<TB_USUARIO?> ObtenerPorId(int idUsuario)
    {
        return await _contexto.TB_USUARIO
            .AsNoTracking()
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
    }

    public async Task AsignarContrasenaTemporal(int idUsuarioAccion, int idUsuario, string contrasenaHash, DateTime vigencia)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
            EXEC dbo.SP_Usuario_AsignarContrasenaTemporal
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdUsuario = {idUsuario},
                @ContrasenaHash = {contrasenaHash},
                @Vigencia = {vigencia}");
    }

    public async Task RestablecerContrasena(int idUsuarioAccion, int idUsuario, string contrasenaHash)
    {
        await _contexto.Database.ExecuteSqlAsync($@"
            EXEC dbo.SP_Usuario_RestablecerContrasena
                @IdUsuarioAccion = {idUsuarioAccion},
                @IdUsuario = {idUsuario},
                @ContrasenaHash = {contrasenaHash}");
    }
}
