using System.ComponentModel.DataAnnotations;

namespace ProjetoFichality.Models
{
    public class Destaque
    {
        public int Id { get; set; }
        [Required, StringLength(30)] public string Tipo { get; set; } = "Novidade";
        [Required, StringLength(150)] public string Titulo { get; set; } = string.Empty;
        [StringLength(500)] public string Resumo { get; set; } = string.Empty;
        public string? ImagemUrl { get; set; }
        public string? LinkUrl { get; set; }
        public int? SistemaRpgId { get; set; }
        public SistemaRpg? SistemaRpg { get; set; }
        public int? ModeloFichaId { get; set; }
        public ModeloFicha? ModeloFicha { get; set; }
        public int Ordem { get; set; }
        public bool Ativo { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
}
