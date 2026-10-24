namespace ProjetoFichality.Models;

public class Ficha
{
    public int Id { get; set; }
    public string NomePersonagem { get; set; } = string.Empty;
    public string SistemaRpg { get; set; } = string.Empty;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // JSON armazenado como texto em SQL Server (nvarchar(max)).
    public string EstruturaJson { get; set; } = "[]";

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
}
