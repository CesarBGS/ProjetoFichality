
using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.Models
{
    public class ModeloFicha
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        // Estrutura dinâmica dos blocos e campos da ficha.
        public string EstruturaJson { get; set; } = "[]";

        public int SistemaRpgId { get; set; }

        public SistemaRpg? SistemaRpg { get; set; }

        public ICollection<Ficha> Fichas { get; set; }
            = new List<Ficha>();

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
