using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.Models
{
    public class ItemSistemaRpg
    {
        public int Id { get; set; }
        public int SistemaRpgId { get; set; }
        public SistemaRpg? SistemaRpg { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(60)]
        public string Categoria { get; set; } = "Geral";

        public string Descricao { get; set; } = string.Empty;

        // Campos personalizados: peso, custo, raridade, requisitos etc.
        public string PropriedadesJson { get; set; } = "{}";

        // Efeitos controlados pelo Fichality; não executar código arbitrário.
        public string EfeitosJson { get; set; } = "[]";

        // Valor em texto porque a unidade/moeda depende do sistema.
        [StringLength(80)]
        public string? Custo { get; set; }

        public decimal? Peso { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }
}
