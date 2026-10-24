
using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.Models
{
    public class SistemaRpg
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Lore { get; set; } = string.Empty;

        public string RegrasBasicas { get; set; } = string.Empty;

        [StringLength(50)]
        public string Versao { get; set; } = "1.0";

        public int CriadorUsuarioId { get; set; }

        public Usuario? CriadorUsuario { get; set; }

        public bool Publico { get; set; } = false;

        public bool PermiteCopia { get; set; } = false;

        // Permite registrar que este sistema veio de uma cópia.
        public int? SistemaOrigemId { get; set; }

        public SistemaRpg? SistemaOrigem { get; set; }

        public ICollection<SistemaRpg> Copias { get; set; }
            = new List<SistemaRpg>();

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public ICollection<ModeloFicha> Modelos { get; set; }
            = new List<ModeloFicha>();

        public ICollection<Ficha> Fichas { get; set; }
            = new List<Ficha>();
    }
}
