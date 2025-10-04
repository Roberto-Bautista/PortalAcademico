using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PortalAcademico.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinadorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Coordinador
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.ToListAsync();
            return View(cursos);
        }

        // GET: Coordinador/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Coordinador/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Curso curso)
        {
            if (ModelState.IsValid)
            {
                _context.Add(curso);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Curso creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Coordinador/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Curso curso)
        {
            if (id != curso.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Curso actualizado correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cursos.Any(e => e.Id == curso.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Coordinador/Matriculas
        public async Task<IActionResult> Matriculas()
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        // POST: Coordinador/ConfirmarMatricula
        [HttpPost]
        public async Task<IActionResult> ConfirmarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            matricula.Estado = EstadoMatricula.Confirmada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Matrícula confirmada correctamente.";
            return RedirectToAction(nameof(Matriculas));
        }

        // POST: Coordinador/CancelarMatricula
        [HttpPost]
        public async Task<IActionResult> CancelarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula == null)
                return NotFound();

            matricula.Estado = EstadoMatricula.Cancelada;
            _context.Update(matricula);
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "Matrícula cancelada correctamente.";
            return RedirectToAction(nameof(Matriculas));
        }
    }
}