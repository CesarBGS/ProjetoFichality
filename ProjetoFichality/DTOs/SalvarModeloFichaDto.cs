using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class SalvarModeloFichaDto
    {
        [Required, StringLength(120, MinimumLength = 1)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;
        public string EstruturaJson { get; set; } = "[]";
        public bool EhPadrao { get; set; }
    }
}
