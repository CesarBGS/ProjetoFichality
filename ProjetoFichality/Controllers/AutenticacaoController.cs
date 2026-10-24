using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Data;
using ProjetoFichality.DTOs;
using ProjetoFichality.Models;
using System.Security.Claims;

namespace ProjetoFichality.Controllers;

[ApiController]
[Route("api/autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AutenticacaoController(
        ApplicationDbContext context,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [AllowAnonymous]
    [HttpPost("registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _context.Usuarios.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { mensagem = "Este e-mail já está cadastrado." });
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Perfil = "Jogador",
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);
        _context.Usuarios.Add(usuario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Também trata duas tentativas simultâneas com o mesmo e-mail.
            return Conflict(new { mensagem = "Não foi possível cadastrar. Verifique se o e-mail já está em uso." });
        }

        await CriarSessaoAsync(usuario);
        return Ok(UsuarioPublico(usuario));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var usuario = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == email);

        if (usuario is null || !usuario.Ativo)
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario, usuario.SenhaHash, dto.Senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });
        }

        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = _passwordHasher.HashPassword(usuario, dto.Senha);
            await _context.SaveChangesAsync();
        }

        await CriarSessaoAsync(usuario);
        return Ok(UsuarioPublico(usuario));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> MeuUsuario()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idClaim, out var usuarioId))
        {
            return Unauthorized(new { mensagem = "Sessão inválida." });
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

        if (usuario is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Unauthorized(new { mensagem = "Usuário não encontrado ou inativo." });
        }

        return Ok(UsuarioPublico(usuario));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    private async Task CriarSessaoAsync(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Perfil)
        };

        var identity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);
    }

    private static object UsuarioPublico(Usuario usuario) => new
    {
        id = usuario.Id,
        nome = usuario.Nome,
        email = usuario.Email,
        perfil = usuario.Perfil
    };
}
