using System.Security.Cryptography;

namespace LEN_API.Infraestructura;

public static class GeneradorContrasena
{
    // Sin 0, O, 1, l ni I: la temporal se lee de un correo o se dicta por telefono
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    public static string Temporal(int largo = 12)
    {
        var caracteres = new char[largo];

        for (var i = 0; i < largo; i++)
            caracteres[i] = Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)];

        return new string(caracteres);
    }
}
