using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Models;

namespace PortalAcademico.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar índice único para Codigo
            modelBuilder.Entity<Curso>()
                .HasIndex(c => c.Codigo)
                .IsUnique();
            
            // Restricción: usuario no puede matricularse dos veces en el mismo curso
            modelBuilder.Entity<Matricula>()
                .HasIndex(m => new { m.UsuarioId, m.CursoId })
                .IsUnique();
            
            // Validación de créditos
            modelBuilder.Entity<Curso>()
                .ToTable(t => t.HasCheckConstraint("CK_Curso_Creditos", "[Creditos] > 0"));
            
            // Seed data - 3 cursos activos
            modelBuilder.Entity<Curso>().HasData(
                new Curso
                {
                    Id = 1,
                    Codigo = "CS101",
                    Nombre = "Programación I",
                    Creditos = 4,
                    CupoMaximo = 30,
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(10, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Id = 2,
                    Codigo = "CS102",
                    Nombre = "Estructuras de Datos",
                    Creditos = 5,
                    CupoMaximo = 25,
                    HorarioInicio = new TimeSpan(10, 0, 0),
                    HorarioFin = new TimeSpan(12, 0, 0),
                    Activo = true
                },
                new Curso
                {
                    Id = 3,
                    Codigo = "MAT101",
                    Nombre = "Cálculo I",
                    Creditos = 4,
                    CupoMaximo = 35,
                    HorarioInicio = new TimeSpan(14, 0, 0),
                    HorarioFin = new TimeSpan(16, 0, 0),
                    Activo = true
                }
            );
        }
}