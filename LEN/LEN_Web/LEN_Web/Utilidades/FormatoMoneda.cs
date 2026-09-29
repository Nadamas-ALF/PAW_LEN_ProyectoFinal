using System.Globalization;
using System.Text;

namespace LEN_Web.Utilidades
{
    // Formato usado en todo el diseno LEN: simbolo de colones, espacio como
    // separador de miles y coma como separador decimal. Ej: 8500 -> 8 500,00
    public static class FormatoMoneda
    {
        public static string Colones(decimal monto)
        {
            var partes = monto.ToString("N2", CultureInfo.InvariantCulture).Split('.');
            var digitos = partes[0].Replace(",", string.Empty);

            var entero = new StringBuilder();
            for (var i = 0; i < digitos.Length; i++)
            {
                if (i > 0 && (digitos.Length - i) % 3 == 0)
                {
                    entero.Append(' ');
                }
                entero.Append(digitos[i]);
            }

            return $"₡{entero},{partes[1]}";
        }
    }
}
