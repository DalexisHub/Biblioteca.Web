using System.ComponentModel.DataAnnotations;
namespace Biblioteca.Web.Models;

public class Libro
{
    public int LibroId { get; set; }
    [Required(ErrorMessage = "Escribe el título."), StringLength(200), DataType(DataType.Text)]
    [Display(Name = "Título")] public string Titulo { get; set; } = "";
    [Required(ErrorMessage = "Escribe el ISBN."), StringLength(20), DataType(DataType.Text)]
    public string ISBN { get; set; } = "";
    [Required, Range(1, int.MaxValue, ErrorMessage = "Selecciona un autor activo.")]
    [Display(Name = "Autor")] public int AutorId { get; set; }
    [Required, Range(0, int.MaxValue, ErrorMessage = "Los ejemplares deben ser cero o más.")]
    public int Ejemplares { get; set; }
    public bool Activo { get; set; }
    public string? AutorNombre { get; set; }
}
