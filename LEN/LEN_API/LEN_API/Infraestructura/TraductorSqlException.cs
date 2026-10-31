using Microsoft.Data.SqlClient;

namespace LEN_API.Infraestructura;

public static class TraductorSqlException
{
    // Los procedimientos lanzan las reglas de negocio con THROW 50000 o mayor.
    // EF envuelve la SqlException segun el camino, por eso se baja por las internas.
    public static bool EsErrorDeNegocio(Exception excepcion, out string mensaje)
    {
        for (var actual = excepcion; actual != null; actual = actual.InnerException)
        {
            if (actual is SqlException sql && sql.Number >= 50000)
            {
                mensaje = sql.Message;
                return true;
            }
        }

        mensaje = string.Empty;
        return false;
    }
}
