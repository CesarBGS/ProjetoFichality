namespace ProjetoFichality.Models
{
    public class Ficha
    {
        public int Id { get; set; }
        public string NomePersonagem { get; set; } = string.Empty;
        // Nome legível do sistema mantido para compatibilidade com fichas antigas.
        public string SistemaRpg { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int? SistemaRpgId { get; set; }
        public SistemaRpg? Sistema { get; set; }
        public int? ModeloFichaId { get; set; }
        public ModeloFicha? ModeloFicha { get; set; }
        public string EstruturaJson { get; set; } = "[]";
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public ICollection<CampanhaFicha> Campanhas { get; set; } = new List<CampanhaFicha>();
    }
}
