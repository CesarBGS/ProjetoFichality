using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Data;
using ProjetoFichality.DTOs;
using ProjetoFichality.Models;

namespace ProjetoFichality.Controllers
{
    [ApiController]
    [Route("api/campanhas")]
    [Authorize]
    public class CampanhasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CampanhasController(ApplicationDbContext db) => _db = db;
        private int? UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            if (UsuarioId == null) return Unauthorized();
            var id = UsuarioId.Value;
            var campanhas = await _db.Campanhas.AsNoTracking()
                .Where(c => c.MestreUsuarioId == id)
                .OrderByDescending(c => c.DataAtualizacao)
                .Select(c => new {
                    c.Id, c.Nome, c.Descricao, c.SistemaRpgId,
                    Sistema = c.SistemaRpg == null ? "" : c.SistemaRpg.Nome,
                    c.Status, c.DataCriacao, c.DataAtualizacao,
                    QuantidadeFichas = c.Participantes.Count
                }).ToListAsync();
            return Ok(campanhas);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obter(int id)
        {
            var c = await _db.Campanhas.AsNoTracking()
                .Include(x => x.SistemaRpg)
                .Include(x => x.Participantes).ThenInclude(p => p.Ficha)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return NotFound();
            if (UsuarioId != c.MestreUsuarioId) return Forbid();
            return Ok(new {
                c.Id, c.Nome, c.Descricao, c.SistemaRpgId,
                Sistema = c.SistemaRpg?.Nome, c.Status, c.HouseRulesJson,
                c.DataCriacao, c.DataAtualizacao,
                Fichas = c.Participantes.Select(p => new {
                    p.FichaId, NomePersonagem = p.Ficha == null ? "Ficha indisponível" : p.Ficha.NomePersonagem,
                    SistemaRpg = p.Ficha == null ? "" : p.Ficha.SistemaRpg
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] SalvarCampanhaDto dto)
        {
            if (UsuarioId == null) return Unauthorized();
            if (!JsonValido(dto.HouseRulesJson)) return BadRequest(new { mensagem = "HouseRulesJson precisa ser um JSON válido." });
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == dto.SistemaRpgId);
            if (sistema == null) return BadRequest(new { mensagem = "Sistema de RPG não encontrado." });
            if (!sistema.Publico && sistema.CriadorUsuarioId != UsuarioId) return Forbid();
            var campanha = new Campanha {
                Nome = dto.Nome.Trim(), Descricao = dto.Descricao,
                SistemaRpgId = dto.SistemaRpgId, MestreUsuarioId = UsuarioId.Value,
                HouseRulesJson = dto.HouseRulesJson, Status = dto.Status,
                DataCriacao = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow
            };
            _db.Campanhas.Add(campanha); await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = campanha.Id }, new { campanha.Id, campanha.Nome });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] SalvarCampanhaDto dto)
        {
            var campanha = await _db.Campanhas.FirstOrDefaultAsync(c => c.Id == id);
            if (campanha == null) return NotFound();
            if (UsuarioId != campanha.MestreUsuarioId) return Forbid();
            if (!JsonValido(dto.HouseRulesJson)) return BadRequest(new { mensagem = "HouseRulesJson precisa ser um JSON válido." });
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == dto.SistemaRpgId);
            if (sistema == null) return BadRequest(new { mensagem = "Sistema de RPG não encontrado." });
            if (!sistema.Publico && sistema.CriadorUsuarioId != UsuarioId) return Forbid();
            campanha.Nome = dto.Nome.Trim(); campanha.Descricao = dto.Descricao;
            campanha.SistemaRpgId = dto.SistemaRpgId; campanha.HouseRulesJson = dto.HouseRulesJson;
            campanha.Status = dto.Status; campanha.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var campanha = await _db.Campanhas.FirstOrDefaultAsync(c => c.Id == id);
            if (campanha == null) return NotFound();
            if (UsuarioId != campanha.MestreUsuarioId) return Forbid();
            _db.Campanhas.Remove(campanha); await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpPost("{id:int}/fichas/{fichaId:int}")]
        public async Task<IActionResult> AdicionarFicha(int id, int fichaId)
        {
            var campanha = await _db.Campanhas.FirstOrDefaultAsync(c => c.Id == id);
            if (campanha == null) return NotFound(new { mensagem = "Campanha não encontrada." });
            if (UsuarioId != campanha.MestreUsuarioId) return Forbid();
            var ficha = await _db.Fichas.FirstOrDefaultAsync(f => f.Id == fichaId && f.UsuarioId == UsuarioId);
            if (ficha == null) return NotFound(new { mensagem = "Ficha não encontrada ou não pertence à sua conta." });
            if (ficha.SistemaRpgId.HasValue && ficha.SistemaRpgId != campanha.SistemaRpgId)
                return BadRequest(new { mensagem = "A ficha está associada a outro sistema de RPG." });
            if (await _db.CampanhasFichas.AnyAsync(x => x.CampanhaId == id && x.FichaId == fichaId))
                return Conflict(new { mensagem = "Esta ficha já está na campanha." });
            _db.CampanhasFichas.Add(new CampanhaFicha { CampanhaId = id, FichaId = fichaId });
            campanha.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpDelete("{id:int}/fichas/{fichaId:int}")]
        public async Task<IActionResult> RemoverFicha(int id, int fichaId)
        {
            var campanha = await _db.Campanhas.FirstOrDefaultAsync(c => c.Id == id);
            if (campanha == null) return NotFound();
            if (UsuarioId != campanha.MestreUsuarioId) return Forbid();
            var link = await _db.CampanhasFichas.FirstOrDefaultAsync(x => x.CampanhaId == id && x.FichaId == fichaId);
            if (link == null) return NotFound();
            _db.CampanhasFichas.Remove(link); campanha.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return NoContent();
        }

        private static bool JsonValido(string json)
        {
            try { using var doc = JsonDocument.Parse(json); return true; }
            catch (JsonException) { return false; }
        }
    }
}
