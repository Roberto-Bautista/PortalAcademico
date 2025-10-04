using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalAcademico.Data;
using PortalAcademico.Helpers;
using PortalAcademico.Models;
using PortalAcademico.ViewModels;
using System.Text.Json;

namespace PortalAcademico.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;
        private const string CACHE_KEY_CURSOS = "cursos_activos";
        
        public CursosController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
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
            
            List<Curso> cursos;
            bool fromCache = false;
            
            // Si no hay filtros, intentar obtener del cache
            if (string.IsNullOrWhiteSpace(busquedaNombre) && !creditosMin.HasValue && 
                !creditosMax.HasValue && !horarioInicio.HasValue && !horarioFin.HasValue)
            {
                var cachedData = await _cache.GetStringAsync(CACHE_KEY_CURSOS);
                
                if (cachedData != null)
                {
                    cursos = JsonSerializer.Deserialize<List<Curso>>(cachedData) ?? new List<Curso>();
                    fromCache = true;
                    Console.WriteLine("📦 Cursos obtenidos del CACHE");
                }
                else
                {
                    cursos = await _context.Cursos
                        .Where(c => c.Activo)
                        .Include(c => c.Matriculas)
                        .ToListAsync();
                    
                    // Guardar en cache por 60 segundos
                    var cacheOptions = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                    };
                    
                    await _cache.SetStringAsync(
                        CACHE_KEY_CURSOS,
                    JsonSerializer.Serialize(cursos, new JsonSerializerOptions
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                        WriteIndented = false
                    }),
                    cacheOptions
                    );
                    
                    Console.WriteLine("💾 Cursos guardados en CACHE (60s)");
                }
            }
            else
            {
                // Con filtros, consultar directamente a la BD
                IQueryable<Curso> query = _context.Cursos
                        .Where(c => c.Activo)
                        .Include(c => c.Matriculas);                
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
                
                cursos = await query.ToListAsync();
                Console.WriteLine("🔍 Cursos filtrados desde BD");
            }
            
            ViewBag.FromCache = fromCache;
            
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
            
            // ============== GUARDAR EN SESIÓN ==============
            HttpContext.Session.SetObject("UltimoCurso", new UltimoCursoSession
            { 
                Id = curso.Id, 
                Nombre = curso.Nombre,
                Codigo = curso.Codigo
            });
            
            Console.WriteLine($"💾 Último curso guardado en sesión: {curso.Nombre}");
            
            return View(curso);
        }
        
        // Método para invalidar cache (se llamará desde el coordinador)
        public async Task InvalidarCacheCursos()
        {
            await _cache.RemoveAsync(CACHE_KEY_CURSOS);
            Console.WriteLine("🗑️ Cache de cursos INVALIDADO");
        }
    }
}