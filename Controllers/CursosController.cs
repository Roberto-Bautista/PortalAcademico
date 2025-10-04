using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.ViewModels;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CursosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Catalogo(string? busquedaNombre, int? creditosMin,
            int? creditosMax, TimeSpan? horarioInicio, TimeSpan? horarioFin)
        {
            // Validaciones server-side
            if (creditosMin.HasValue && creditosMin < 0)
            {
                ModelState.AddModelError("CreditosMin", "Los créditos no pueden ser negativos");
                creditosMin = null;
            }

            if (creditosMax.HasValue && creditosMax < 0)
            {
                ModelState.AddModelError("CreditosMax", "Los créditos no pueden ser negativos");
                creditosMax = null;
            }

            if (horarioInicio.HasValue && horarioFin.HasValue && horarioFin <= horarioInicio)
            {
                ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al de inicio");
                horarioFin = null;
            }

            var query = _context.Cursos.Where(c => c.Activo);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(busquedaNombre))
            {
                query = query.Where(c => c.Nombre.Contains(busquedaNombre) ||
                                        c.Codigo.Contains(busquedaNombre));
            }

            if (creditosMin.HasValue)
            {
                query = query.Where(c => c.Creditos >= creditosMin.Value);
            }

            if (creditosMax.HasValue)
            {
                query = query.Where(c => c.Creditos <= creditosMax.Value);
            }

            if (horarioInicio.HasValue)
            {
                query = query.Where(c => c.HorarioInicio >= horarioInicio.Value);
            }

            if (horarioFin.HasValue)
            {
                query = query.Where(c => c.HorarioFin <= horarioFin.Value);
            }

            var cursos = await query.ToListAsync();

            var viewModel = new CatalogoViewModel
            {
                Cursos = cursos,
                BusquedaNombre = busquedaNombre,
                CreditosMin = creditosMin,
                CreditosMax = creditosMax,
                HorarioInicio = horarioInicio,
                HorarioFin = horarioFin
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }
    }
}