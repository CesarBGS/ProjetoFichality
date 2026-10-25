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
    [Route("api/ficha")]
    [Authorize]
    public class FichaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public FichaController(ApplicationDbContext db) => _db = db;
        private int? UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        [HttpGet("minhas")]
        public async Task<IActionResult> Minhas()
        {
            if (UsuarioId == null) return Unauthorized();
            var fichas = await _db.Fichas.AsNoTracking()
                .Where(f => f.UsuarioId == UsuarioId)
                .OrderByDescending(f => f.DataAtualizacao)
                .Select(f => new { f.Id, f.NomePersonagem, f.SistemaRpg, f.SistemaRpgId, f.ModeloFichaId, f.DataAtualizacao })
                .ToListAsync();
            return Ok(fichas);
        }

        // Mantido por compatibilidade; só permite consultar o próprio usuário.
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<IActionResult> ObterPorUsuario(int usuarioId)
        {
            if (UsuarioId != usuarioId) return Forbid();
            return await Minhas();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterPorId(int id)
        {
            if (UsuarioId == null) return Unauthorized();
            var ficha = await _db.Fichas.AsNoTracking()
                .Where(f => f.Id == id && f.UsuarioId == UsuarioId)
                .Select(f => new {
                    f.Id, f.NomePersonagem, f.SistemaRpg, f.SistemaRpgId,
                    f.ModeloFichaId, f.UsuarioId, f.EstruturaJson,
                    f.DataCriacao, f.DataAtualizacao
                }).FirstOrDefaultAsync();
            if (ficha == null) return NotFound(new { sucesso = false, mensagem = "Ficha não encontrada." });
            return Ok(ficha);
        }

        [HttpPost("criar")]
        public async Task<IActionResult> Criar([FromBody] CriarFichaDto dto)
        {
            if (UsuarioId == null) return Unauthorized();
            string sistemaNome = dto.SistemaRpg.Trim();
            string estruturaJson = "[]";
            int? sistemaId = dto.SistemaRpgId;
            int? modeloId = dto.ModeloFichaId;

            if (sistemaId.HasValue)
            {
                var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId.Value);
                if (sistema == null) return BadRequest(new { mensagem = "Sistema não encontrado." });
                if (!sistema.Publico && sistema.CriadorUsuarioId != UsuarioId) return Forbid();
                sistemaNome = sistema.Nome;
            }

            if (modeloId.HasValue)
            {
                var modelo = await _db.ModelosFicha.FirstOrDefaultAsync(m => m.Id == modeloId.Value);
                if (modelo == null) return BadRequest(new { mensagem = "Modelo de ficha não encontrado." });
                if (sistemaId.HasValue && modelo.SistemaRpgId != sistemaId.Value)
                    return BadRequest(new { mensagem = "O modelo não pertence ao sistema selecionado." });
                var sistemaModelo = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == modelo.SistemaRpgId);
                if (sistemaModelo == null || (!sistemaModelo.Publico && sistemaModelo.CriadorUsuarioId != UsuarioId)) return Forbid();
                sistemaId ??= modelo.SistemaRpgId;
                sistemaNome = sistemaModelo.Nome;
                estruturaJson = modelo.EstruturaJson;
            }

            var ficha = new Ficha {
                NomePersonagem = dto.NomePersonagem.Trim(),
                SistemaRpg = sistemaNome,
                UsuarioId = UsuarioId.Value,
                SistemaRpgId = sistemaId,
                ModeloFichaId = modeloId,
                EstruturaJson = estruturaJson,
                DataCriacao = DateTime.UtcNow,
                DataAtualizacao = DateTime.UtcNow
            };
            _db.Fichas.Add(ficha); await _db.SaveChangesAsync();
            return Ok(new { sucesso = true, fichaId = ficha.Id });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarFichaDto dto)
        {
            if (UsuarioId == null) return Unauthorized();
            var ficha = await _db.Fichas.FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId == UsuarioId);
            if (ficha == null) return NotFound(new { mensagem = "Ficha não encontrada." });
            ficha.NomePersonagem = dto.NomePersonagem.Trim();
            if (!string.IsNullOrWhiteSpace(dto.SistemaRpg)) ficha.SistemaRpg = dto.SistemaRpg.Trim();
            ficha.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("salvar-estrutura")]
        public async Task<IActionResult> SalvarEstrutura([FromBody] SalvarEstruturaDto dto)
        {
            if (UsuarioId == null) return Unauthorized();
            try { using var jsonDoc = JsonDocument.Parse(dto.EstruturaJson); }
            catch (JsonException) { return BadRequest(new { sucesso = false, mensagem = "JSON sintaticamente inválido." }); }

            var ficha = await _db.Fichas.FirstOrDefaultAsync(f => f.Id == dto.FichaId && f.UsuarioId == UsuarioId);
            if (ficha == null) return NotFound(new { sucesso = false, mensagem = "Ficha não encontrada." });
            ficha.EstruturaJson = dto.EstruturaJson;
            ficha.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { sucesso = true, mensagem = "Estrutura salva com sucesso." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            if (UsuarioId == null) return Unauthorized();
            var ficha = await _db.Fichas.FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId == UsuarioId);
            if (ficha == null) return NotFound(new { mensagem = "Ficha não encontrada." });
            _db.Fichas.Remove(ficha);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
