
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Data;

namespace ProjetoFichality.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/home
        [HttpGet]
        public async Task<IActionResult> ObterResumo()
        {
            var idUsuarioTexto =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(idUsuarioTexto, out var usuarioId))
                return Unauthorized();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Id == usuarioId && u.Ativo)
                .Select(u => new
                {
                    u.Id,
                    u.Nome
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return Unauthorized();

            var fichasRecentes = await _context.Fichas
                .AsNoTracking()
                .Where(f => f.UsuarioId == usuarioId)
                .OrderByDescending(f => f.DataAtualizacao)
                .Take(5)
                .Select(f => new
                {
                    f.Id,
                    f.NomePersonagem,
                    Sistema = f.Sistema == null
                        ? f.SistemaRpg
                        : f.Sistema.Nome,
                    f.DataAtualizacao
                })
                .ToListAsync();

            var totalFichas = await _context.Fichas
                .CountAsync(f => f.UsuarioId == usuarioId);

            var totalSistemasCriados = await _context.SistemasRpg
                .CountAsync(s => s.CriadorUsuarioId == usuarioId);

            return Ok(new
            {
                usuario = new
                {
                    usuario.Id,
                    usuario.Nome
                },
                totalFichas,
                totalSistemasCriados,
                fichasRecentes
            });
        }
    }
}
