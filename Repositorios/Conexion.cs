using Microsoft.Data.SqlClient;
namespace Biblioteca.Web.Repositorios;

public class Conexion(IConfiguration configuration)
{
    public SqlConnection Crear()
    {
        var cadena = configuration.GetConnectionString("BibliotecaDB");
        if (string.IsNullOrWhiteSpace(cadena) || cadena.Contains("COMPLETAR", StringComparison.OrdinalIgnoreCase))
            throw new DatosNoDisponiblesException();
        return new SqlConnection(cadena);
    }
}
public class DatosNoDisponiblesException : Exception { }
public class ReglaDatosException(string campo, string mensaje) : Exception(mensaje)
{
    public string Campo { get; } = campo;
}
