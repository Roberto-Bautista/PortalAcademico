using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortalAcademico.Models
{
    public enum EstadoMatricula
    {
        Pendiente,
        Confirmada,
        Cancelada
    }
    
    public class Matricula
    {
        public int Id { get; set; }
        
        [Required]
        public int CursoId { get; set; }
        
        [Required]
        public string UsuarioId { get; set; } = string.Empty;
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        
        public EstadoMatricula Estado { get; set; } = EstadoMatricula.Pendiente;
        
        // Navegación
        public virtual Curso Curso { get; set; } = null!;
    }
}