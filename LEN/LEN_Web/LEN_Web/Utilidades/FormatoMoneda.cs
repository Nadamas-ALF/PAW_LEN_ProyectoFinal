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

            return $"₡{Agrupado(partes[0])},{partes[1]}";
        }

        // Los indicadores del tablero y los titulos del grafico van sin decimales
        public static string ColonesSinDecimales(decimal monto)
        {
            var redondeado = Math.Round(monto, MidpointRounding.AwayFromZero)
                .ToString("N0", CultureInfo.InvariantCulture);

            return $"₡{Agrupado(redondeado)}";
        }

        private static string Agrupado(string parteEntera)
        {
            var digitos = parteEntera.Replace(",", string.Empty);

            var entero = new StringBuilder();
            for (var i = 0; i < digitos.Length; i++)
            {
                if (i > 0 && (digitos.Length - i) % 3 == 0)
                {
                    entero.Append(' ');
                }
                entero.Append(digitos[i]);
            }

            return entero.ToString();
        }
    }
}
