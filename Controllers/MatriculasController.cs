using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;

namespace PortalAcademico.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        // GET: Mostrar formulario de inscripción
        [HttpGet]
        public async Task<IActionResult> Inscribir(int cursoId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == cursoId);
            
            if (curso == null)
            {
                return NotFound();
            }
            
            if (!curso.Activo)
            {
                TempData["Error"] = "Este curso no está activo para inscripciones.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            
            return View(curso);
        }
        
        // POST: Procesar inscripción
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribir(int cursoId, string confirmacion)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas.Where(m => m.Estado != EstadoMatricula.Cancelada))
                .FirstOrDefaultAsync(c => c.Id == cursoId);
            
            if (curso == null)
            {
                return NotFound();
            }
            
            // Validación 1: Usuario autenticado
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Debe estar autenticado para inscribirse.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            
            // Validación 2: Curso activo
            if (!curso.Activo)
            {
                TempData["Error"] = "Este curso no está disponible para inscripciones.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            
            // Validación 3: Ya está matriculado
            var yaMatriculado = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && 
                              m.UsuarioId == userId && 
                              m.Estado != EstadoMatricula.Cancelada);
            
            if (yaMatriculado)
            {
                TempData["Error"] = "Ya estás inscrito en este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            
            // Validación 4: Cupo máximo
            var matriculasActivas = curso.Matriculas.Count(m => m.Estado != EstadoMatricula.Cancelada);
            if (matriculasActivas >= curso.CupoMaximo)
            {
                TempData["Error"] = $"El curso ha alcanzado su cupo máximo ({curso.CupoMaximo} estudiantes).";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
            
            // Validación 5: Solapamiento de horarios
            var cursosMatriculados = await _context.Matriculas
                .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                .Include(m => m.Curso)
                .Select(m => m.Curso)
                .ToListAsync();
            
            foreach (var cursoExistente in cursosMatriculados)
            {
                // Verificar si hay solapamiento de horarios
                bool seSolapan = !(curso.HorarioFin <= cursoExistente.HorarioInicio || 
                                   curso.HorarioInicio >= cursoExistente.HorarioFin);
                
                if (seSolapan)
                {
                    TempData["Error"] = $"El horario de este curso se solapa con '{cursoExistente.Nombre}' " +
                                       $"({cursoExistente.HorarioInicio:hh\\:mm} - {cursoExistente.HorarioFin:hh\\:mm}).";
                    return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
                }
            }
            
            // Crear matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };
            
            try
            {
                _context.Matriculas.Add(matricula);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"¡Te has inscrito exitosamente en '{curso.Nombre}'! Tu matrícula está pendiente de confirmación.";
                return RedirectToAction("MisMatriculas");
            }
            catch (DbUpdateException ex)
            {
                // Manejar errores de BD (ej: violación de restricción única)
                TempData["Error"] = "Ocurrió un error al procesar tu inscripción. Por favor, intenta nuevamente.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }
        }
        
        // GET: Ver mis matrículas
        public async Task<IActionResult> MisMatriculas()
        {
            var userId = _userManager.GetUserId(User);
            
            var matriculas = await _context.Matriculas
                .Where(m => m.UsuarioId == userId)
                .Include(m => m.Curso)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();
            
            return View(matriculas);
        }
        
        // POST: Cancelar matrícula
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = _userManager.GetUserId(User);
            
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);
            
            if (matricula == null)
            {
                return NotFound();
            }
            
            if (matricula.Estado == EstadoMatricula.Cancelada)
            {
                TempData["Error"] = "Esta matrícula ya está cancelada.";
                return RedirectToAction("MisMatriculas");
            }
            
            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Has cancelado tu matrícula en '{matricula.Curso.Nombre}'.";
            return RedirectToAction("MisMatriculas");
        }
    }
}