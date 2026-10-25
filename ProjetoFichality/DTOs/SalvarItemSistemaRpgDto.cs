using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class SalvarItemSistemaRpgDto
    {
        [Required, StringLength(120, MinimumLength = 1)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(60)]
        public string Categoria { get; set; } = "Geral";
        public string Descricao { get; set; } = string.Empty;
        public string PropriedadesJson { get; set; } = "{}";
        public string EfeitosJson { get; set; } = "[]";
        [StringLength(80)]
        public string? Custo { get; set; }
        [Range(0, 1000000)]
        public decimal? Peso { get; set; }
        public bool Ativo { get; set; } = true;
    }
}
