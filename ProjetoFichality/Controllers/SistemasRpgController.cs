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
    [Route("api/sistemas")]
    [Authorize]
    public class SistemasRpgController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SistemasRpgController(ApplicationDbContext db) => _db = db;

        private int? UsuarioId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            if (UsuarioId == null) return Unauthorized();
            var id = UsuarioId.Value;
            var lista = await _db.SistemasRpg.AsNoTracking()
                .Where(s => s.Publico || s.CriadorUsuarioId == id)
                .OrderBy(s => s.Nome)
                .Select(s => new {
                    s.Id, s.Nome, s.Descricao, s.Versao, s.Publico, s.PermiteCopia,
                    s.CriadorUsuarioId, s.SistemaOrigemId,
                    QuantidadeModelos = s.Modelos.Count,
                    QuantidadeItens = s.Itens.Count,
                    Meu = s.CriadorUsuarioId == id
                }).ToListAsync();
            return Ok(lista);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Obter(int id)
        {
            var usuarioId = UsuarioId;
            if (usuarioId == null) return Unauthorized();
            var sistema = await _db.SistemasRpg.AsNoTracking()
                .Include(s => s.Modelos).Include(s => s.Itens)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (sistema == null) return NotFound(new { mensagem = "Sistema não encontrado." });
            if (!sistema.Publico && sistema.CriadorUsuarioId != usuarioId)
                return Forbid();
            return Ok(new {
                sistema.Id, sistema.Nome, sistema.Descricao, sistema.Lore,
                sistema.RegrasBasicas, sistema.Versao, sistema.Publico,
                sistema.PermiteCopia, sistema.CriadorUsuarioId, sistema.SistemaOrigemId,
                modelos = sistema.Modelos.Select(m => new { m.Id, m.Nome, m.Descricao, m.EstruturaJson, m.SistemaRpgId }),
                itens = sistema.Itens.Select(ItemResumo)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] SalvarSistemaRpgDto dto)
        {
            if (UsuarioId == null) return Unauthorized();
            var sistema = new SistemaRpg {
                Nome = dto.Nome.Trim(), Descricao = dto.Descricao,
                Lore = dto.Lore, RegrasBasicas = dto.RegrasBasicas,
                Versao = dto.Versao, CriadorUsuarioId = UsuarioId.Value,
                Publico = dto.Publico, PermiteCopia = dto.Publico && dto.PermiteCopia,
                DataCriacao = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow
            };
            _db.SistemasRpg.Add(sistema);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Obter), new { id = sistema.Id }, new { sistema.Id, sistema.Nome });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] SalvarSistemaRpgDto dto)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == id);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            sistema.Nome = dto.Nome.Trim(); sistema.Descricao = dto.Descricao;
            sistema.Lore = dto.Lore; sistema.RegrasBasicas = dto.RegrasBasicas;
            sistema.Versao = dto.Versao; sistema.Publico = dto.Publico;
            sistema.PermiteCopia = dto.Publico && dto.PermiteCopia;
            sistema.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == id);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            if (await _db.Fichas.AnyAsync(f => f.SistemaRpgId == id) ||
                await _db.Campanhas.AnyAsync(c => c.SistemaRpgId == id) ||
                await _db.SistemasRpg.AnyAsync(s => s.SistemaOrigemId == id))
                return Conflict(new { mensagem = "O sistema está sendo usado por fichas, campanhas ou cópias e não pode ser excluído." });
            _db.SistemasRpg.Remove(sistema);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id:int}/copiar")]
        public async Task<IActionResult> Copiar(int id)
        {
            if (UsuarioId == null) return Unauthorized();
            var original = await _db.SistemasRpg.Include(s => s.Modelos).Include(s => s.Itens)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (original == null) return NotFound();
            if (!original.Publico || !original.PermiteCopia) return Forbid();

            var copia = new SistemaRpg {
                Nome = original.Nome + " (cópia)", Descricao = original.Descricao,
                Lore = original.Lore, RegrasBasicas = original.RegrasBasicas,
                Versao = original.Versao, CriadorUsuarioId = UsuarioId.Value,
                Publico = false, PermiteCopia = false, SistemaOrigemId = original.Id,
                DataCriacao = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow
            };
            _db.SistemasRpg.Add(copia);
            await _db.SaveChangesAsync();

            foreach (var m in original.Modelos)
                _db.ModelosFicha.Add(new ModeloFicha { Nome = m.Nome, Descricao = m.Descricao, EstruturaJson = m.EstruturaJson, SistemaRpgId = copia.Id, EhPadrao = m.EhPadrao, DataCriacao = DateTime.UtcNow });
            foreach (var i in original.Itens)
                _db.ItensSistemaRpg.Add(new ItemSistemaRpg { SistemaRpgId = copia.Id, Nome = i.Nome, Categoria = i.Categoria, Descricao = i.Descricao, PropriedadesJson = i.PropriedadesJson, EfeitosJson = i.EfeitosJson, Custo = i.Custo, Peso = i.Peso, Ativo = i.Ativo, DataCriacao = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow });
            await _db.SaveChangesAsync();
            return Ok(new { copia.Id, copia.Nome, copia.SistemaOrigemId });
        }

        [HttpGet("{sistemaId:int}/itens")]
        public async Task<IActionResult> ListarItens(int sistemaId)
        {
            var sistema = await _db.SistemasRpg.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (!sistema.Publico && UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var itens = await _db.ItensSistemaRpg.AsNoTracking().Where(i => i.SistemaRpgId == sistemaId)
                .OrderBy(i => i.Categoria).ThenBy(i => i.Nome).Select(i => new {
                    i.Id, i.SistemaRpgId, i.Nome, i.Categoria, i.Descricao,
                    i.PropriedadesJson, i.EfeitosJson, i.Custo, i.Peso, i.Ativo,
                    i.DataCriacao, i.DataAtualizacao
                }).ToListAsync();
            return Ok(itens);
        }

        [HttpPost("{sistemaId:int}/itens")]
        public async Task<IActionResult> CriarItem(int sistemaId, [FromBody] SalvarItemSistemaRpgDto dto)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            if (!JsonValido(dto.PropriedadesJson) || !JsonValido(dto.EfeitosJson))
                return BadRequest(new { mensagem = "PropriedadesJson e EfeitosJson precisam conter JSON válido." });
            var item = new ItemSistemaRpg {
                SistemaRpgId = sistemaId, Nome = dto.Nome.Trim(), Categoria = dto.Categoria,
                Descricao = dto.Descricao, PropriedadesJson = dto.PropriedadesJson,
                EfeitosJson = dto.EfeitosJson, Custo = dto.Custo, Peso = dto.Peso,
                Ativo = dto.Ativo, DataCriacao = DateTime.UtcNow, DataAtualizacao = DateTime.UtcNow
            };
            _db.ItensSistemaRpg.Add(item); await _db.SaveChangesAsync();
            return Created($"/api/sistemas/{sistemaId}/itens/{item.Id}", new { item.Id, item.Nome });
        }

        [HttpPut("{sistemaId:int}/itens/{itemId:int}")]
        public async Task<IActionResult> AtualizarItem(int sistemaId, int itemId, [FromBody] SalvarItemSistemaRpgDto dto)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var item = await _db.ItensSistemaRpg.FirstOrDefaultAsync(i => i.Id == itemId && i.SistemaRpgId == sistemaId);
            if (item == null) return NotFound();
            if (!JsonValido(dto.PropriedadesJson) || !JsonValido(dto.EfeitosJson))
                return BadRequest(new { mensagem = "PropriedadesJson e EfeitosJson precisam conter JSON válido." });
            item.Nome = dto.Nome.Trim(); item.Categoria = dto.Categoria; item.Descricao = dto.Descricao;
            item.PropriedadesJson = dto.PropriedadesJson; item.EfeitosJson = dto.EfeitosJson;
            item.Custo = dto.Custo; item.Peso = dto.Peso; item.Ativo = dto.Ativo;
            item.DataAtualizacao = DateTime.UtcNow;
            await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpDelete("{sistemaId:int}/itens/{itemId:int}")]
        public async Task<IActionResult> ExcluirItem(int sistemaId, int itemId)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var item = await _db.ItensSistemaRpg.FirstOrDefaultAsync(i => i.Id == itemId && i.SistemaRpgId == sistemaId);
            if (item == null) return NotFound();
            _db.ItensSistemaRpg.Remove(item); await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpGet("{sistemaId:int}/modelos")]
        public async Task<IActionResult> ListarModelos(int sistemaId)
        {
            var sistema = await _db.SistemasRpg.AsNoTracking().FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (!sistema.Publico && UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var modelos = await _db.ModelosFicha.AsNoTracking()
                .Where(m => m.SistemaRpgId == sistemaId)
                .OrderByDescending(m => m.EhPadrao).ThenBy(m => m.Nome)
                .Select(m => new { m.Id, m.Nome, m.Descricao, m.EstruturaJson, m.SistemaRpgId, m.EhPadrao, m.DataCriacao })
                .ToListAsync();
            return Ok(modelos);
        }

        [HttpPost("{sistemaId:int}/modelos")]
        public async Task<IActionResult> CriarModelo(int sistemaId, [FromBody] SalvarModeloFichaDto dto)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            if (!JsonValido(dto.EstruturaJson)) return BadRequest(new { mensagem = "EstruturaJson precisa conter JSON válido." });
            if (dto.EhPadrao)
            {
                var padroes = await _db.ModelosFicha.Where(m => m.SistemaRpgId == sistemaId && m.EhPadrao).ToListAsync();
                foreach (var modeloAntigo in padroes) modeloAntigo.EhPadrao = false;
            }
            var modelo = new ModeloFicha { Nome = dto.Nome.Trim(), Descricao = dto.Descricao,
                EstruturaJson = dto.EstruturaJson, SistemaRpgId = sistemaId,
                EhPadrao = dto.EhPadrao, DataCriacao = DateTime.UtcNow };
            _db.ModelosFicha.Add(modelo); await _db.SaveChangesAsync();
            return Ok(new { modelo.Id, modelo.Nome, modelo.EhPadrao });
        }

        [HttpPut("{sistemaId:int}/modelos/{modeloId:int}")]
        public async Task<IActionResult> AtualizarModelo(int sistemaId, int modeloId, [FromBody] SalvarModeloFichaDto dto)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var modelo = await _db.ModelosFicha.FirstOrDefaultAsync(m => m.Id == modeloId && m.SistemaRpgId == sistemaId);
            if (modelo == null) return NotFound();
            if (!JsonValido(dto.EstruturaJson)) return BadRequest(new { mensagem = "EstruturaJson precisa conter JSON válido." });
            if (dto.EhPadrao)
            {
                var padroes = await _db.ModelosFicha.Where(m => m.SistemaRpgId == sistemaId && m.EhPadrao && m.Id != modeloId).ToListAsync();
                foreach (var modeloAntigo in padroes) modeloAntigo.EhPadrao = false;
            }
            modelo.Nome = dto.Nome.Trim(); modelo.Descricao = dto.Descricao;
            modelo.EstruturaJson = dto.EstruturaJson; modelo.EhPadrao = dto.EhPadrao;
            await _db.SaveChangesAsync(); return NoContent();
        }

        [HttpDelete("{sistemaId:int}/modelos/{modeloId:int}")]
        public async Task<IActionResult> ExcluirModelo(int sistemaId, int modeloId)
        {
            var sistema = await _db.SistemasRpg.FirstOrDefaultAsync(s => s.Id == sistemaId);
            if (sistema == null) return NotFound();
            if (UsuarioId != sistema.CriadorUsuarioId) return Forbid();
            var modelo = await _db.ModelosFicha.FirstOrDefaultAsync(m => m.Id == modeloId && m.SistemaRpgId == sistemaId);
            if (modelo == null) return NotFound();
            if (await _db.Fichas.AnyAsync(f => f.ModeloFichaId == modeloId))
                return Conflict(new { mensagem = "Este modelo já está associado a fichas e não pode ser excluído." });
            _db.ModelosFicha.Remove(modelo); await _db.SaveChangesAsync(); return NoContent();
        }

        private static bool JsonValido(string json)
        {
            try { using var doc = JsonDocument.Parse(json); return true; }
            catch (JsonException) { return false; }
        }

        private static object ItemResumo(ItemSistemaRpg i) => new {
            i.Id, i.Nome, i.Categoria, i.Descricao, i.PropriedadesJson,
            i.EfeitosJson, i.Custo, i.Peso, i.Ativo
        };
    }
}
