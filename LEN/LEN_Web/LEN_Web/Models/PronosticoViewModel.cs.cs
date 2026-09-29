using Microsoft.AspNetCore.Mvc;

namespace LEN_Web.Models
{
    public class PronosticoViewModel
    {
        public DateOnly Date { get; set; }
        public int TemperatureC { get; set; }
        public string? Summary { get; set; }
    }
}
