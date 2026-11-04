namespace LEN_API.Dtos;

public class UbicacionDto
{
    public int? IdProvincia { get; set; }
    public int? IdCanton { get; set; }
    public int? IdDistrito { get; set; }
    public string Nombre { get; set; } = string.Empty;
}