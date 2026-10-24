using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Data;
using ProjetoFichality.DTOs;
using ProjetoFichality.Models;
using System.Security.Claims;
using System.Text.Json;

namespace ProjetoFichality.Controllers;

[ApiController]
[Authorize]
[Route("api/ficha")]
public class FichaController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FichaController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("usuario/{usuarioId:int}")]
    public async Task<IActionResult> ObterPorUsuario(int usuarioId)
    {
        if (!TentarObterUsuarioId(out var usuarioAutenticadoId))
            return Unauthorized();

        if (usuarioId != usuarioAutenticadoId)
            return Forbid();

        var fichas = await _context.Fichas
            .AsNoTracking()
            .Where(f => f.UsuarioId == usuarioAutenticadoId)
            .Select(f => new
            {
                f.Id,
                f.NomePersonagem,
                f.SistemaRpg,
                f.DataAtualizacao
            })
            .ToListAsync();

        return Ok(fichas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        var ficha = await _context.Fichas
            .AsNoTracking()
            .Where(f => f.Id == id && f.UsuarioId == usuarioId)
            .Select(f => new
            {
                f.Id,
                f.NomePersonagem,
                f.SistemaRpg,
                f.UsuarioId,
                f.EstruturaJson,
                f.DataCriacao,
                f.DataAtualizacao
            })
            .FirstOrDefaultAsync();

        if (ficha is null)
            return NotFound(new { sucesso = false, mensagem = "Ficha não encontrada." });

        return Ok(ficha);
    }

    [HttpPost("criar")]
    public async Task<IActionResult> Criar([FromBody] CriarFichaDto dto)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        var usuarioExiste = await _context.Usuarios
            .AnyAsync(u => u.Id == usuarioId && u.Ativo);

        if (!usuarioExiste)
            return Unauthorized(new { sucesso = false, mensagem = "Usuário inválido ou inativo." });

        var agora = DateTime.UtcNow;
        var novaFicha = new Ficha
        {
            NomePersonagem = dto.NomePersonagem.Trim(),
            SistemaRpg = dto.SistemaRpg.Trim(),
            UsuarioId = usuarioId,
            EstruturaJson = "[]",
            DataCriacao = agora,
            DataAtualizacao = agora
        };

        _context.Fichas.Add(novaFicha);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = novaFicha.Id },
            new { sucesso = true, fichaId = novaFicha.Id });
    }

    [HttpPut("salvar-estrutura")]
    public async Task<IActionResult> SalvarEstrutura([FromBody] SalvarEstruturaDto dto)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
            return Unauthorized();

        try
        {
            using var jsonDoc = JsonDocument.Parse(dto.EstruturaJson);
        }
        catch (JsonException)
        {
            return BadRequest(new { sucesso = false, mensagem = "JSON sintaticamente inválido." });
        }

        var ficha = await _context.Fichas
            .FirstOrDefaultAsync(f => f.Id == dto.FichaId && f.UsuarioId == usuarioId);

        if (ficha is null)
            return NotFound(new { sucesso = false, mensagem = "Ficha não encontrada." });

        ficha.EstruturaJson = dto.EstruturaJson;
        ficha.DataAtualizacao = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { sucesso = true, mensagem = "Estrutura da ficha salva com sucesso no SQL Server!" });
    }

    private bool TentarObterUsuarioId(out int usuarioId)
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out usuarioId);
    }
}
