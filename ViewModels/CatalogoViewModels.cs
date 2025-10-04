using PortalAcademico.Models;

namespace PortalAcademico.ViewModels
{
    public class CatalogoViewModel
    {
        public List<Curso> Cursos { get; set; } = new List<Curso>();
        public string? BusquedaNombre { get; set; }
        public int? CreditosMin { get; set; }
        public int? CreditosMax { get; set; }
        public TimeSpan? HorarioInicio { get; set; }
        public TimeSpan? HorarioFin { get; set; }
    }
}