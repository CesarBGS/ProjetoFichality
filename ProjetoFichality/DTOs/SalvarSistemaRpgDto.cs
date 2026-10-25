using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.DTOs
{
    public class SalvarSistemaRpgDto
    {
        [Required, StringLength(120, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;
        public string Lore { get; set; } = string.Empty;
        public string RegrasBasicas { get; set; } = string.Empty;
        [StringLength(50)]
        public string Versao { get; set; } = "1.0";
        public bool Publico { get; set; }
        public bool PermiteCopia { get; set; }
    }
}
