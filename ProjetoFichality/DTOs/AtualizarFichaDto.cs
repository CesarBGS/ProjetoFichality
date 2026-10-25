using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class AtualizarFichaDto
    {
        [Required, StringLength(100, MinimumLength = 1)]
        public string NomePersonagem { get; set; } = string.Empty;
        [StringLength(100)]
        public string SistemaRpg { get; set; } = string.Empty;
    }
}
