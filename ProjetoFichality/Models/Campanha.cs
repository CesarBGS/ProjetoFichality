using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.Models
{
    public class Campanha
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public int SistemaRpgId { get; set; }
        public SistemaRpg? SistemaRpg { get; set; }

        public int MestreUsuarioId { get; set; }
        public Usuario? MestreUsuario { get; set; }

        // JSON de regras narrativas/mecânicas da campanha, versionável.
        public string HouseRulesJson { get; set; } = "[]";

        [StringLength(30)]
        public string Status { get; set; } = "Preparação";

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

        public ICollection<CampanhaFicha> Participantes { get; set; }
            = new List<CampanhaFicha>();
    }
}
