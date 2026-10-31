using LEN_API.Datos;
using LEN_API.Dtos;

namespace LEN_API.Repositorios;

public class DiagnosticoRepositorio : IDiagnosticoRepositorio
{
    private readonly ContextoLen _contexto;

    public DiagnosticoRepositorio(ContextoLen contexto)
    {
        _contexto = contexto;
    }

    public async Task<EstadoBaseDto> ObtenerEstado()
    {
        return new EstadoBaseDto
        {
            BaseDeDatos = _contexto.Database.GetDbConnection().Database,
            Roles = await _contexto.TB_ROL.CountAsync(),
            Distritos = await _contexto.TB_DISTRITO.CountAsync()
        };
    }

    // simula lo que hace un procedimiento cuando una regla no se cumple
    public async Task ProvocarErrorDeNegocio()
    {
        await _contexto.Database.ExecuteSqlRawAsync("THROW 50000, N'Error de negocio provocado para la prueba.', 1;");
    }

    // division por cero: un error de SQL Server que no es de negocio
    public async Task ProvocarErrorInterno()
    {
        await _contexto.Database.ExecuteSqlRawAsync("SELECT 1 / 0;");
    }
}
