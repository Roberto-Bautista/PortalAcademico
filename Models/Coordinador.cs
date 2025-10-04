using System.ComponentModel.DataAnnotations;

namespace Proyecto.Models
{
    public class Coordinador
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [StringLength(80)]
        public string Especialidad { get; set; }

        public bool Activo { get; set; } = true;
    }
}