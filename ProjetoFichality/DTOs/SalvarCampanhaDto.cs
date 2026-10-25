using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class SalvarCampanhaDto
    {
        [Required, StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(3000)]
        public string Descricao { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int SistemaRpgId { get; set; }
        public string HouseRulesJson { get; set; } = "[]";
        [RegularExpression("^(Preparação|Ativa|Pausada|Concluída)$")]
        public string Status { get; set; } = "Preparação";
    }
}
