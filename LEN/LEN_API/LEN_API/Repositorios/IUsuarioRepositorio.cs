using LEN_API.Datos.Entidades;
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public interface IUsuarioRepositorio
{
    // SP_Usuario_Registrar. Devuelve el IdUsuario nuevo; una regla incumplida sale como SqlException 50000
    Task<int> Registrar(RegistroDto datos, string contrasenaHash);

    // Lectura por EF con el rol incluido, o null si el correo no existe
    Task<TB_USUARIO?> ObtenerPorCorreo(string correo);
}
