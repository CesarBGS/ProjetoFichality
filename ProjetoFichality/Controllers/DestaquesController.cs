
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Data;

namespace ProjetoFichality.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DestaquesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DestaquesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/destaques
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var agora = DateTime.UtcNow;

            var destaques = await _context.Destaques
                .AsNoTracking()
                .Where(d =>
                    d.Ativo &&
                    (!d.DataInicio.HasValue ||
                     d.DataInicio.Value <= agora) &&
                    (!d.DataFim.HasValue ||
                     d.DataFim.Value >= agora))
                .OrderBy(d => d.Ordem)
                .ThenByDescending(d => d.DataCriacao)
                .Take(10)
                .Select(d => new
                {
                    d.Id,
                    d.Tipo,
                    d.Titulo,
                    d.Resumo,
                    d.ImagemUrl,
                    d.LinkUrl,
                    d.SistemaRpgId,
                    d.ModeloFichaId
                })
                .ToListAsync();

            return Ok(destaques);
        }
    }
}
