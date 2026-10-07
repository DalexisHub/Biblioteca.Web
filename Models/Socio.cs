using System.ComponentModel.DataAnnotations;
namespace Biblioteca.Web.Models;

public class Socio
{
    public int SocioId { get; set; }
    [Required(ErrorMessage = "Escribe el DNI."), StringLength(15)] public string DNI { get; set; } = "";
    [Required(ErrorMessage = "Escribe el nombre."), StringLength(150)] public string Nombre { get; set; } = "";
    [Required(ErrorMessage = "Escribe el correo."), StringLength(150), EmailAddress(ErrorMessage = "Introduce un correo válido."), DataType(DataType.EmailAddress)]
    [Display(Name = "Correo electrónico")] public string Email { get; set; } = "";
    public bool Activo { get; set; }
}
