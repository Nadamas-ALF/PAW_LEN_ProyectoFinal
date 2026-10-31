using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace LEN_API.Datos;

// Solo lo usa el scaffold. Apaga el singularizador de EF porque convierte
// TB_CATEGORIA en TB_CATEGORIum y los nombres deben quedar como en la base.
public class DisenoServicios : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddSingleton<IPluralizer, SinPluralizador>();
    }
}

public class SinPluralizador : IPluralizer
{
    public string Pluralize(string name) => name;
    public string Singularize(string name) => name;
}
